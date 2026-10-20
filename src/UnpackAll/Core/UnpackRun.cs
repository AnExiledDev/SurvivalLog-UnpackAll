using System;
using System.Collections.Generic;
using System.Linq;

namespace UnpackAll.Core;

/// <summary>
/// One press of Unpack All. Packages already in the bag go first; packages on the floor at home are picked up while
/// spots are left after that, and the stacks in the home's storages that best fill the spots still left after the
/// floor are moved into the bag. Only as many unpacks are lined up as the home has free package spots, so every one of them can run
/// and none sits in the queue bar waiting to fail. Everything else stays where it is and is counted by the reason it
/// was left.
/// </summary>
public static class UnpackRun
{

    public static UnpackOutcome Run(IPackageGame game)
    {
        var read = game.Read();

        return read switch
        {
            PackageRead.NoGame => new UnpackOutcome.NoGame(),
            PackageRead.NotReady notReady => new UnpackOutcome.NotReady(notReady.Reason),
            PackageRead.Ready ready => Unpack(game, ready.Stock),
            _ => throw new InvalidOperationException("unknown package read " + read),
        };
    }

    private static UnpackOutcome Unpack(IPackageGame game, PackageStock stock)
    {
        var hasLookedInStorages = stock.StorageAccess != StorageAccess.Off;
        // Storages the config turned off are never touched, whatever the read handed over.
        var storages = hasLookedInStorages ? stock.Storages : Array.Empty<Package>();

        // Unpacks still lined up hold spots the free count does not show until each one runs, so a second batch would
        // line up unpacks with no spot left to land in.
        if (stock.AlreadyQueued > 0) return new UnpackOutcome.AlreadyUnpacking(stock.AlreadyQueued);
        if (Units(stock.Bag) + Units(stock.Floor) + Units(storages) == 0) return new UnpackOutcome.NothingToUnpack(hasLookedInStorages);
        // Actions the player queued are never traded for unpacks, so this is checked before anything leaves the floor.
        if (stock.QueueOnStart == QueueOnStart.ReplacesPlayerActions) return new UnpackOutcome.QueueBusy();

        var press = new Press(game, stock.FreeSpots);

        press.Choose(stock.Bag, isBrought: false);
        press.TakeFromFloor(stock.Bag, stock.Floor, stock.IsHome);
        press.TakeFromStorages(storages, stock.IsHome, stock.StorageAccess);
        press.QueueChosen();

        // Starting with nothing lined up does nothing useful: the game logs an error, and when starting replaces the
        // queue it first cancels whatever the character is doing.
        if (press.Queued == 0) return new UnpackOutcome.Done(press.Tally());
        if (!game.TryStart()) return new UnpackOutcome.StartFailed(press.Queued, press.PickedUp, press.FromStorage);

        // The game can accept the start and still throw the unpacks away, so only what reached the queue counts. Before
        // this press no use of any of these kinds was pending (AlreadyQueued), so every use found is one of its own. A
        // queue that cannot be read leaves the press as lined up; the adapter logs why.
        var tally = game.UsesInQueue(press.LinedUp) is { } inQueue ? press.Tally(inQueue) : press.Tally();

        return new UnpackOutcome.Done(tally);
    }

    private static int Units(IEnumerable<Package> packages) => packages.Sum(package => Math.Max(0, package.Count));

    /// <summary>
    /// Packages that arrived in the bag since it was last read: new items, and units added to a stack the bag already
    /// had (a floor or storage package can merge into one when a stacking mod lets packages stack).
    /// </summary>
    private static IEnumerable<Package> Arrivals(IReadOnlyList<Package> before, IReadOnlyList<Package> after)
    {
        var unitsBefore = before.GroupBy(package => package.ItemId).ToDictionary(group => group.Key, group => group.Sum(package => package.Count));

        foreach (var package in after)
        {
            var added = package.Count - unitsBefore.GetValueOrDefault(package.ItemId);

            if (added > 0) yield return package with { Count = added };
        }
    }

    /// <summary>A package given spots, and whether this press brought it into the bag from the floor or a storage.</summary>
    private readonly record struct Chosen(Package Package, bool IsBrought);

    /// <summary>The spots one press hands out, the packages chosen for them, and the count of every package left.</summary>
    private sealed class Press
    {
        private readonly IPackageGame _game;
        private readonly List<Chosen> _chosen = new();
        private readonly List<Package> _linedUp = new();
        private int _spotsLeft;
        private int _queuedBrought;
        private int _storageClaimed;
        private int _noSpot;
        private int _floorRefused;
        private int _floorAway;
        private int _storageRefused;
        private int _storageAway;
        private int _storageBehindPanel;
        private int _storageUnchecked;
        private int _storageTooBig;
        private int _unusable;
        private Phrase? _unusableReason;

        public Press(IPackageGame game, int freeSpots)
        {
            _game = game;
            _spotsLeft = Math.Max(0, freeSpots);
        }

        public int Queued { get; private set; }

        public int PickedUp { get; private set; }

        public int FromStorage { get; private set; }

        /// <summary>The packages with at least one unpack lined up, each once.</summary>
        public IReadOnlyList<Package> LinedUp => _linedUp;

        /// <summary>
        /// Gives each package a spot until none are left. The game's use check runs only while a spot is free, so a
        /// refused package hands its spot to the next one. A brought package left without a spot is not counted here:
        /// the tally already says it waits in the bag.
        /// </summary>
        public void Choose(IEnumerable<Package> packages, bool isBrought)
        {
            foreach (var package in packages)
            {
                var units = Math.Max(0, package.Count);

                if (units == 0) continue;

                if (_spotsLeft == 0)
                {
                    if (!isBrought) _noSpot += units;

                    continue;
                }

                if (_game.WhyUnusable(package) is { } reason)
                {
                    CountUnusable(units, reason);
                    continue;
                }

                var taken = Math.Min(units, _spotsLeft);
                _spotsLeft -= taken;

                if (!isBrought) _noSpot += units - taken;

                _chosen.Add(new Chosen(package with { Count = taken }, isBrought));
            }
        }

        /// <summary>
        /// Picks up floor packages while the ones picked up have not yet filled the spots left, then chooses among the
        /// packages that arrived in the bag. The game's pickup takes a whole floor stack and cannot be asked for part
        /// of one, so a stack bigger than the spots left still comes up: it uses those spots, and the packages it
        /// brings beyond them wait in the bag. Nothing leaves the floor once the spots are spoken for, and nothing is
        /// picked up away from home, where the game's pickup would reach across maps.
        /// </summary>
        public void TakeFromFloor(IReadOnlyList<Package> bagBefore, IReadOnlyList<Package> floor, bool isHome)
        {
            if (!isHome)
            {
                _floorAway += Units(floor);

                return;
            }

            foreach (var drop in floor) PickUp(drop);

            if (PickedUp > 0) Choose(Arrivals(bagBefore, _game.BagPackages()), isBrought: true);
        }

        /// <summary>
        /// Moves into the bag the storage stacks that together fill the most of the spots the floor left (see
        /// <see cref="StackFit"/>), then chooses among the packages that arrived. The game's move takes a whole stack,
        /// so unlike the floor a stack is never moved past the spots left: it stays in its storage rather than spilling
        /// the extra packages into the bag. A picked stack that leaves its spots unused, because the game would not let
        /// the character use it or the bag refused it, hands them to a new pick among the stacks not yet tried. Nothing
        /// is moved away from home or while another window may be open.
        /// </summary>
        public void TakeFromStorages(IReadOnlyList<Package> storages, bool isHome, StorageAccess access)
        {
            if (Units(storages) == 0) return;

            if (!isHome)
            {
                _storageAway += Units(storages);

                return;
            }

            var bagBefore = _game.BagPackages();
            var untried = Fitting(storages);

            while (StackFit.Pick(untried.Select(package => package.Count).ToList(), _spotsLeft - _storageClaimed) is { Count: > 0 } picked)
            {
                foreach (var index in picked) TakeFromStorage(untried[index], access);

                untried = untried.Where((_, index) => !picked.Contains(index)).ToList();
            }

            _noSpot += Units(untried);

            if (FromStorage > 0) Choose(Arrivals(bagBefore, _game.BagPackages()), isBrought: true);
        }

        /// <summary>Lines up one unpack per chosen package; a package the game will not take gives up the rest of its stack.</summary>
        public void QueueChosen()
        {
            foreach (var (package, isBrought) in _chosen)
            {
                for (var unit = 0; unit < package.Count; unit++)
                {
                    if (!_game.TryQueue(package))
                    {
                        CountUnusable(package.Count - unit, Texts.QueueRefused);

                        break;
                    }

                    if (unit == 0) _linedUp.Add(package);

                    Queued++;

                    if (isBrought) _queuedBrought++;
                }
            }
        }

        /// <summary>The press as lined up, for a batch that never started or whose queue could not be read.</summary>
        public UnpackTally Tally() => TallyDropping(0);

        /// <summary>The press after its start, with the unpacks missing from the character's queue counted as dropped.</summary>
        public UnpackTally Tally(int inQueue) => TallyDropping(Math.Clamp(Queued - inQueue, 0, Queued));

        /// <summary>
        /// Which packages were dropped is not known, only how many, so a dropped unpack counts against the packages
        /// brought into the bag first: a brought package is then said to wait in the bag, where it is, rather than to
        /// unpack.
        /// </summary>
        private UnpackTally TallyDropping(int dropped)
        {
            var queuedBrought = Math.Max(0, _queuedBrought - dropped);
            var waitingInBag = Math.Max(0, PickedUp + FromStorage - queuedBrought);

            return new UnpackTally(
                Queued - dropped,
                PickedUp,
                waitingInBag,
                _noSpot,
                _floorRefused,
                _floorAway,
                _unusable,
                _unusableReason,
                dropped,
                FromStorage,
                _storageRefused,
                _storageAway,
                _storageBehindPanel,
                _storageUnchecked,
                _storageTooBig);
        }

        /// <summary>Takes a whole floor stack while any spot is still unclaimed by an earlier pickup, even when the stack overshoots it.</summary>
        private void PickUp(Package drop)
        {
            var units = Math.Max(0, drop.Count);

            if (units == 0) return;

            if (PickedUp >= _spotsLeft)
            {
                _noSpot += units;

                return;
            }

            var picked = Math.Clamp(_game.PickUp(drop), 0, units);
            PickedUp += picked;
            _floorRefused += units - picked;
        }

        /// <summary>
        /// The storage stacks a pick may choose from. With no spot left none may, and a stack bigger than all the spots
        /// the bag and the floor left could never fit this press, so it is counted as too big rather than as having no
        /// spot.
        /// </summary>
        private List<Package> Fitting(IReadOnlyList<Package> storages)
        {
            var fitting = new List<Package>();

            foreach (var stored in storages)
            {
                var units = Math.Max(0, stored.Count);

                if (units == 0) continue;

                if (_spotsLeft == 0) _noSpot += units;
                else if (units > _spotsLeft) _storageTooBig += units;
                else fitting.Add(stored);
            }

            return fitting;
        }

        /// <summary>
        /// Takes one picked storage stack, which fits the spots no earlier move has claimed. The use check runs first,
        /// so a package the character could not use never leaves its storage. When a window is open or could not be
        /// checked the stack stays put but still claims its spots, so the count said to wait is what a press with the
        /// window closed would move.
        /// </summary>
        private void TakeFromStorage(Package stored, StorageAccess access)
        {
            var units = Math.Max(0, stored.Count);

            if (_game.WhyUnusable(stored) is { } reason)
            {
                CountUnusable(units, reason);

                return;
            }

            if (access != StorageAccess.Allowed)
            {
                CountUntaken(units, access);

                return;
            }

            var moved = Math.Clamp(_game.TakeFromStorage(stored), 0, units);
            FromStorage += moved;
            _storageClaimed += moved;
            _storageRefused += units - moved;
        }

        private void CountUntaken(int units, StorageAccess access)
        {
            if (access == StorageAccess.Unknown) _storageUnchecked += units;
            else _storageBehindPanel += units;

            _storageClaimed += units;
        }

        private void CountUnusable(int units, Phrase reason)
        {
            _unusable += units;
            _unusableReason ??= reason;
        }
    }
}
