using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>
/// The first press compiles the whole press path (the run, its LINQ over packages, the toast wording and its regular
/// expression) on the game's frame. Run on a worker at load, on a throwaway game, this pays that cost before any
/// press. Pure .NET only: nothing here touches the game.
/// </summary>
public static class WarmUp
{
    /// <summary>One press over a bag, the floor and a storage, worded in both languages; the outcome is returned for tests.</summary>
    public static UnpackOutcome Run()
    {
        var outcome = UnpackRun.Run(new ThrowawayGame());

        OutcomeMessage.Describe(outcome, isChinese: false);
        OutcomeMessage.Describe(outcome, isChinese: true);
        OutcomeMessage.Describe(new UnpackOutcome.NotReady(Phrase.FromGame("<color=#f55>warm</color>")), isChinese: false);
        WarmPage();
        WarmWatch();

        return outcome;
    }

    private static void WarmPage()
    {
        foreach (var message in new[] { "unpack", "pickup", "menu|1", "menu|0", "ready", "noscript", "diag|warm" }) PageCommand.Parse(PageCommand.Prefix + message);
    }

    private static void WarmWatch()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 1, now: 0);
        watch.Observe(pending: 0, bagUnits: 1, now: UnpackWatch.SettleSeconds);
        OutcomeMessage.Describe(new WatchStep.Finished(Started: 1, Consumed: 0), isChinese: false);
        OutcomeMessage.Describe(new WatchStep.Finished(Started: 1, Consumed: 0), isChinese: true);
    }

    /// <summary>
    /// Two packages in the bag, one of which the use check refuses, one on the floor and one in a storage, with a spot
    /// for each usable one: the press goes through every step and lines up three unpacks.
    /// </summary>
    private sealed class ThrowawayGame : IPackageGame
    {
        private readonly List<Package> _bag = new() { new Package(1, 7, 1), new Package(2, 8, 1) };

        public PackageRead Read() => new PackageRead.Ready(new PackageStock(
            FreeSpots: 3,
            AlreadyQueued: 0,
            IsHome: true,
            Bag: _bag.ToArray(),
            Floor: new[] { new Package(3, 7, 1) },
            QueueOnStart: QueueOnStart.Appends,
            Storages: new[] { new Package(4, 7, 1) },
            StorageAccess: StorageAccess.Allowed));

        public int PickUp(Package floorPackage) => Bring(floorPackage, 100);

        public int TakeFromStorage(Package storagePackage) => Bring(storagePackage, 0);

        public IReadOnlyList<Package> BagPackages() => _bag.ToArray();

        public Phrase? WhyUnusable(Package package) => package.ConfigId == 8 ? Texts.NoReasonGiven : null;

        public bool TryQueue(Package package) => true;

        public bool TryStart() => true;

        public int? UsesInQueue(IReadOnlyList<Package> packages) => packages.Count;

        private int Bring(Package package, long idOffset)
        {
            _bag.Add(package with { ItemId = package.ItemId + idOffset });

            return package.Count;
        }
    }
}
