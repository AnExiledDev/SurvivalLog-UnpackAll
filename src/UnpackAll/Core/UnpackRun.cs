using System;
using System.Collections.Generic;
using System.Linq;

namespace UnpackAll.Core;

/// <summary>
/// One press of Unpack All. Packages already in the bag go first; packages on the floor at home are picked up only for
/// the spots left after that. Only as many unpacks are lined up as the home has free package spots, so every one of
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
            PackageRead.NotReady notReady => new UnpackOutcome.NotReady(notReady.Reason),
            PackageRead.Ready ready => Unpack(game, ready.Stock),
            _ => throw new InvalidOperationException("unknown package read " + read),
        };
    }

    private static UnpackOutcome Unpack(IPackageGame game, PackageStock stock)
    {
        // Starting a second batch would restart the first one: the game drops the character's current action when it
        // starts lined-up actions, so the unpack in progress would lose its time and the queue bar would refill.
        if (stock.AlreadyQueued > 0) return new UnpackOutcome.AlreadyUnpacking(stock.AlreadyQueued);
        if (Units(stock.Bag) + Units(stock.Floor) == 0) return new UnpackOutcome.NothingToUnpack();

        var press = new Press(game, stock.FreeSpots);

        press.Choose(stock.Bag);
        press.TakeFromFloor(stock.Bag, stock.Floor, stock.IsHome);
        press.QueueChosen();

        // Starting with nothing lined up would only cancel whatever the character is doing.
        if (press.Queued == 0) return new UnpackOutcome.Done(press.Tally());

        return game.TryStart() ? new UnpackOutcome.Done(press.Tally()) : new UnpackOutcome.StartFailed(press.Queued);
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

    /// <summary>The spots one press hands out, the packages chosen for them, and the count of every package left.</summary>
    private sealed class Press
    {
        private readonly IPackageGame _game;
        private readonly List<Package> _chosen = new();
        private int _spotsLeft;
        private int _pickedUp;
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

        /// <summary>
        /// Gives each package in the bag a spot until none are left. The game's use check runs only while a spot is
        /// free, so a refused package hands its spot to the next one.
        /// </summary>
        public void Choose(IEnumerable<Package> bag)
        {
            foreach (var package in bag)
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

                _chosen.Add(package with { Count = taken });
            }
        }

        /// <summary>
        /// Picks up floor packages while picked-up ones have not yet filled the spots left, then chooses among the
        /// packages that arrived in the bag. Nothing leaves the floor once the spots are spoken for, and nothing is
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

            if (_pickedUp > 0) Choose(Arrivals(bagBefore, _game.BagPackages()));
        }

        /// <summary>Lines up one unpack per chosen package; a package the game will not take gives up the rest of its stack.</summary>
        public void QueueChosen()
        {
            foreach (var package in _chosen)
            {
                for (var unit = 0; unit < package.Count; unit++)
                {
                    if (_game.TryQueue(package))
                    {
                        Queued++;
                        continue;
                    }

                    CountUnusable(package.Count - unit, QueueRefusedReason);

                    break;
                }
            }
        }

        public UnpackTally Tally() => new(Queued, _pickedUp, _noSpot, _floorRefused, _floorAway, _unusable, _unusableReason);

        private void PickUp(Package drop)
        {
            var units = Math.Max(0, drop.Count);

            if (units == 0) return;

            if (_pickedUp >= _spotsLeft)
            {
                _noSpot += units;

                return;
            }

            var picked = Math.Clamp(_game.PickUp(drop), 0, units);
            _pickedUp += picked;
            _floorRefused += units - picked;
        }

        private void CountUnusable(int units, string reason)
        {
            _unusable += units;
            _unusableReason ??= reason;
        }
    }
}
