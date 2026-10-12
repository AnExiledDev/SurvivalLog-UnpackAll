using System;
using System.Collections.Generic;
using System.Linq;

namespace UnpackAll.Core;

/// <summary>
/// One press of Unpack All. Packages already in the bag go first; packages on the floor at home are picked up while
/// spots are left after that. Only as many unpacks are lined up as the home has free package spots, so every one of
/// them can run and none sits in the queue bar waiting to fail. Everything else stays where it is and is counted by
/// the reason it was left.
/// </summary>
public static class UnpackRun
{
    public const string QueueRefusedReason = "the game would not line it up";

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
        // Unpacks still lined up hold spots the free count does not show until each one runs, so a second batch would
        // line up unpacks with no spot left to land in.
        if (stock.AlreadyQueued > 0) return new UnpackOutcome.AlreadyUnpacking(stock.AlreadyQueued);
        if (Units(stock.Bag) + Units(stock.Floor) == 0) return new UnpackOutcome.NothingToUnpack();
        // Actions the player queued are never traded for unpacks, so this is checked before anything leaves the floor.
        if (stock.QueueOnStart == QueueOnStart.ReplacesPlayerActions) return new UnpackOutcome.QueueBusy();

        var press = new Press(game, stock.FreeSpots);

        press.Choose(stock.Bag, isFromFloor: false);
        press.TakeFromFloor(stock.Bag, stock.Floor, stock.IsHome);
        press.QueueChosen();

        // Starting with nothing lined up does nothing useful: the game logs an error, and when starting replaces the
        // queue it first cancels whatever the character is doing.
        if (press.Queued == 0) return new UnpackOutcome.Done(press.Tally());
        if (!game.TryStart()) return new UnpackOutcome.StartFailed(press.Queued, press.PickedUp);

        // The game can accept the start and still throw the unpacks away, so only what reached the queue counts. Before
        // this press no use of any of these kinds was pending (AlreadyQueued), so every use found is one of its own. A
        // queue that cannot be read leaves the press as lined up; the adapter logs why.
        var tally = game.UsesInQueue(press.LinedUp) is { } inQueue ? press.Tally(inQueue) : press.Tally();

        return new UnpackOutcome.Done(tally);
    }

    private static int Units(IEnumerable<Package> packages) => packages.Sum(package => Math.Max(0, package.Count));

    /// <summary>
    /// Packages that arrived in the bag since it was last read: new items, and units added to a stack the bag already
    /// had (a floor package can merge into one when a stacking mod lets packages stack).
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

    /// <summary>A package given spots, and whether it came up off the floor in this press.</summary>
    private readonly record struct Chosen(Package Package, bool IsFromFloor);

    /// <summary>The spots one press hands out, the packages chosen for them, and the count of every package left.</summary>
    private sealed class Press
    {
        private readonly IPackageGame _game;
        private readonly List<Chosen> _chosen = new();
        private readonly List<Package> _linedUp = new();
        private int _spotsLeft;
        private int _queuedFromFloor;
        private int _noSpot;
        private int _floorRefused;
        private int _floorAway;
        private int _unusable;
        private string? _unusableReason;

        public Press(IPackageGame game, int freeSpots)
        {
            _game = game;
            _spotsLeft = Math.Max(0, freeSpots);
        }

        public int Queued { get; private set; }

        public int PickedUp { get; private set; }

        /// <summary>The packages with at least one unpack lined up, each once.</summary>
        public IReadOnlyList<Package> LinedUp => _linedUp;

        /// <summary>
        /// Gives each package a spot until none are left. The game's use check runs only while a spot is free, so a
        /// refused package hands its spot to the next one.
        /// </summary>
        public void Choose(IEnumerable<Package> packages, bool isFromFloor)
        {
            foreach (var package in packages)
            {
                var units = Math.Max(0, package.Count);

                if (units == 0) continue;

                if (_spotsLeft == 0)
                {
                    _noSpot += units;
                    continue;
                }

                if (_game.WhyUnusable(package) is { } reason)
                {
                    CountUnusable(units, reason);
                    continue;
                }

                var taken = Math.Min(units, _spotsLeft);
                _spotsLeft -= taken;
                _noSpot += units - taken;

                _chosen.Add(new Chosen(package with { Count = taken }, isFromFloor));
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

            if (PickedUp > 0) Choose(Arrivals(bagBefore, _game.BagPackages()), isFromFloor: true);
        }

        /// <summary>Lines up one unpack per chosen package; a package the game will not take gives up the rest of its stack.</summary>
        public void QueueChosen()
        {
            foreach (var (package, isFromFloor) in _chosen)
            {
                for (var unit = 0; unit < package.Count; unit++)
                {
                    if (!_game.TryQueue(package))
                    {
                        CountUnusable(package.Count - unit, QueueRefusedReason);

                        break;
                    }

                    if (unit == 0) _linedUp.Add(package);

                    Queued++;

                    if (isFromFloor) _queuedFromFloor++;
                }
            }
        }

        /// <summary>The press as lined up, for a batch that never started or whose queue could not be read.</summary>
        public UnpackTally Tally() => TallyDropping(0);

        /// <summary>The press after its start, with the unpacks missing from the character's queue counted as dropped.</summary>
        public UnpackTally Tally(int inQueue) => TallyDropping(Math.Clamp(Queued - inQueue, 0, Queued));

        /// <summary>
        /// Which packages were dropped is not known, only how many, so a dropped unpack counts against the floor
        /// packages first: a picked-up package is then said to wait in the bag, where it is, rather than to unpack.
        /// </summary>
        private UnpackTally TallyDropping(int dropped)
        {
            var queuedFromFloor = Math.Max(0, _queuedFromFloor - dropped);
            var waitingInBag = Math.Max(0, PickedUp - queuedFromFloor);

            return new UnpackTally(Queued - dropped, PickedUp, waitingInBag, _noSpot, _floorRefused, _floorAway, _unusable, _unusableReason, dropped);
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

        private void CountUnusable(int units, string reason)
        {
            _unusable += units;
            _unusableReason ??= reason;
        }
    }
}
