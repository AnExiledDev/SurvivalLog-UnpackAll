using UnpackAll.Core;

namespace UnpackAll.Tests;

/// <summary>
/// The game as far as an unpack press can tell: a bag, the floor and the storages at home, the unpacks the character
/// has lined up and its action queue. Like the game, lining up an unpack is never refused for want of units in the stack (the game only
/// finds out when the unpack's turn comes), so <see cref="OverQueued"/> shows a press that lined up more than the bag
/// holds. Starting appends to the queue, as the game does after the prologue; with <see cref="StartClearsQueue"/> it
/// replaces the queue, as the game's default action mode does, so a test can see a press that would have thrown away
/// the character's actions.
/// </summary>
internal sealed class FakeGame : IPackageGame
{
    /// <summary>What the character is doing before any press, by its own choice. Not an unpack.</summary>
    public const long Walking = -1;

    /// <summary>An action the player queued (taking adrenaline). Not an unpack.</summary>
    public const long Adrenaline = -2;

    /// <summary>A package picked up off the floor gets a new id in the bag: the floor id plus this.</summary>
    public const long BagIdOffset = 1000;

    /// <summary>The title screen, a loading save or the prologue: there is no game to unpack in.</summary>
    public bool IsNoGame { get; set; }

    public string? NotReadyReason { get; set; }

    public int FreeSpots { get; set; }

    public bool IsHome { get; set; } = true;

    public bool StartClearsQueue { get; set; }

    public List<Package> Bag { get; } = new();

    public List<Package> Floor { get; } = new();

    /// <summary>The packages in the home's storages, all storages together.</summary>
    public List<Package> Storages { get; } = new();

    /// <summary>Whether a press may take from the storages; allowed by default, as the mod ships.</summary>
    public StorageAccess StorageAccess { get; set; } = StorageAccess.Allowed;

    public List<long> LinedUp { get; } = new();

    /// <summary>The character's action queue, the running action first. Negative ids are not unpacks.</summary>
    public List<long> Running { get; private set; } = new() { Walking };

    /// <summary>Bag items the game's use check refuses, with its reason.</summary>
    public Dictionary<long, string> Refusals { get; } = new();

    /// <summary>Floor packages the bag cannot carry at all.</summary>
    public HashSet<long> TooHeavy { get; } = new();

    /// <summary>Storage packages the game's move refuses outright, as for a bag too heavy to take the stack.</summary>
    public HashSet<long> StuckInStorage { get; } = new();

    /// <summary>How many more packages the bag can carry in total, from the floor and storages alike.</summary>
    public int BagRoom { get; set; } = int.MaxValue;

    /// <summary>
    /// How many more packages the bag's weight allows from storages. Like the game's quick move
    /// (ItemManager.TryQuickMoveItem), the whole stack is weighed before anything moves, so a stack heavier than this
    /// is refused whole.
    /// </summary>
    public int BagWeight { get; set; } = int.MaxValue;

    /// <summary>Floor or storage packages that land on a stack already in the bag (their id to bag id), as when packages stack.</summary>
    public Dictionary<long, long> MergesInto { get; } = new();

    /// <summary>The game's queue refuses everything, as it does for an action id it does not know.</summary>
    public bool RefusesToQueue { get; set; }

    public bool FailsToStart { get; set; }

    /// <summary>
    /// How many of the lined-up unpacks the game throws away while starting them, though the start itself reports no
    /// error: the unpack never reaches the character's queue and its package stays in the bag. This is what the game did
    /// with an unpack aimed at the world's origin rather than at a package point.
    /// </summary>
    public int DropsOnStart { get; set; }

    public List<long> PickupsTried { get; } = new();

    public List<long> StorageMovesTried { get; } = new();

    public IEnumerable<long> Unpacking => Running.Where(id => id >= 0);

    /// <summary>Bag items lined up or unpacking more times than their stack holds: unpacks that fail when their turn comes.</summary>
    public IEnumerable<long> OverQueued => LinedUp.Concat(Unpacking).GroupBy(id => id).Where(uses => uses.Count() > StackOf(uses.Key)).Select(uses => uses.Key);

    public static Package Pkg(long itemId, int count = 1, int configId = 7) => new(itemId, configId, count);

    public PackageRead Read()
    {
        if (IsNoGame) return new PackageRead.NoGame();
        if (NotReadyReason is { } reason) return new PackageRead.NotReady(Phrase.FromGame(reason));

        return new PackageRead.Ready(new PackageStock(FreeSpots, LinedUp.Count + Unpacking.Count(), IsHome, Bag.ToList(), Floor.ToList(), WhatStartDoes(), Storages.ToList(), StorageAccess));
    }

    public int PickUp(Package floorPackage)
    {
        PickupsTried.Add(floorPackage.ItemId);

        var index = Floor.FindIndex(package => package.ItemId == floorPackage.ItemId);

        if (index < 0 || TooHeavy.Contains(floorPackage.ItemId)) return 0;

        var lying = Floor[index];
        var picked = Math.Min(lying.Count, BagRoom);

        if (picked == 0) return 0;

        BagRoom -= picked;

        if (picked == lying.Count) Floor.RemoveAt(index);
        else Floor[index] = lying with { Count = lying.Count - picked };

        AddToBag(MergesInto.TryGetValue(lying.ItemId, out var stackId) ? stackId : lying.ItemId + BagIdOffset, lying.ConfigId, picked);

        return picked;
    }

    /// <summary>
    /// Like the game's quick move, a stack keeps its id when it moves whole into the bag, and a stack too heavy for the
    /// bag does not move at all. When the bag runs out of room partway, the units already moved stay in the bag and the
    /// rest stay in the storage: nothing is lost or doubled.
    /// </summary>
    public int TakeFromStorage(Package storagePackage)
    {
        StorageMovesTried.Add(storagePackage.ItemId);

        var index = Storages.FindIndex(package => package.ItemId == storagePackage.ItemId);

        if (index < 0 || StuckInStorage.Contains(storagePackage.ItemId)) return 0;

        var stored = Storages[index];
        var moved = Math.Min(stored.Count, BagRoom);

        if (stored.Count > BagWeight || moved == 0) return 0;

        BagRoom -= moved;
        BagWeight -= moved;

        if (moved == stored.Count) Storages.RemoveAt(index);
        else Storages[index] = stored with { Count = stored.Count - moved };

        AddToBag(MergesInto.TryGetValue(stored.ItemId, out var stackId) ? stackId : stored.ItemId, stored.ConfigId, moved);

        return moved;
    }

    public IReadOnlyList<Package> BagPackages() => Bag.ToList();

    public Phrase? WhyUnusable(Package package) => Refusals.TryGetValue(package.ItemId, out var reason) ? Phrase.FromGame(reason) : null;

    /// <summary>Like the game's own queue, this takes any use it is handed, whatever the bag holds.</summary>
    public bool TryQueue(Package package)
    {
        if (RefusesToQueue) return false;

        LinedUp.Add(package.ItemId);

        return true;
    }

    public bool TryStart()
    {
        if (FailsToStart) return false;

        var started = LinedUp.Skip(Math.Min(DropsOnStart, LinedUp.Count)).ToList();

        if (StartClearsQueue) Running = started;
        else Running.AddRange(started);

        LinedUp.Clear();

        return true;
    }

    /// <summary>Like the game's own count, this sees only what reached the character's queue, never the unstarted line.</summary>
    public int? UsesInQueue(IReadOnlyList<Package> packages) => Unpacking.Count(id => packages.Any(package => package.ItemId == id));

    private QueueOnStart WhatStartDoes()
    {
        if (!StartClearsQueue) return QueueOnStart.Appends;

        return Running.Any(id => id != Walking) ? QueueOnStart.ReplacesPlayerActions : QueueOnStart.ReplacesOwnActions;
    }

    private int StackOf(long itemId) => Bag.Where(held => held.ItemId == itemId).Sum(held => held.Count);

    private void AddToBag(long itemId, int configId, int count)
    {
        var index = Bag.FindIndex(held => held.ItemId == itemId);

        if (index < 0) Bag.Add(new Package(itemId, configId, count));
        else Bag[index] = Bag[index] with { Count = Bag[index].Count + count };
    }
}
