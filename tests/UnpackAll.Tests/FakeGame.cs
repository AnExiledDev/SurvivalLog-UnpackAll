using UnpackAll.Core;

namespace UnpackAll.Tests;

/// <summary>
/// The game as far as an unpack press can tell: a bag, the floor at home, the unpacks the character has lined up and
/// its action queue. Like the game, lining up an unpack is never refused for want of units in the stack (the game only
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

    public List<long> LinedUp { get; } = new();

    /// <summary>The character's action queue, the running action first. Negative ids are not unpacks.</summary>
    public List<long> Running { get; private set; } = new() { Walking };

    /// <summary>Bag items the game's use check refuses, with its reason.</summary>
    public Dictionary<long, string> Refusals { get; } = new();

    /// <summary>Floor packages the bag cannot carry at all.</summary>
    public HashSet<long> TooHeavy { get; } = new();

    /// <summary>How many more packages the bag can carry in total.</summary>
    public int BagRoom { get; set; } = int.MaxValue;

    /// <summary>Floor packages that land on a stack already in the bag (floor id to bag id), as when packages stack.</summary>
    public Dictionary<long, long> MergesInto { get; } = new();

    /// <summary>The game's queue refuses everything, as it does for an action id it does not know.</summary>
    public bool RefusesToQueue { get; set; }

    public bool FailsToStart { get; set; }

    public List<long> PickupsTried { get; } = new();

    public IEnumerable<long> Unpacking => Running.Where(id => id >= 0);

    /// <summary>Bag items lined up or unpacking more times than their stack holds: unpacks that fail when their turn comes.</summary>
    public IEnumerable<long> OverQueued => LinedUp.Concat(Unpacking).GroupBy(id => id).Where(uses => uses.Count() > StackOf(uses.Key)).Select(uses => uses.Key);

    public static Package Pkg(long itemId, int count = 1, int configId = 7) => new(itemId, configId, count);

    public PackageRead Read()
    {
        if (IsNoGame) return new PackageRead.NoGame();
        if (NotReadyReason is { } reason) return new PackageRead.NotReady(reason);

        return new PackageRead.Ready(new PackageStock(FreeSpots, LinedUp.Count + Unpacking.Count(), IsHome, Bag.ToList(), Floor.ToList(), WhatStartDoes()));
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

    public IReadOnlyList<Package> BagPackages() => Bag.ToList();

    public string? WhyUnusable(Package package) => Refusals.TryGetValue(package.ItemId, out var reason) ? reason : null;

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

        if (StartClearsQueue) Running = LinedUp.ToList();
        else Running.AddRange(LinedUp);

        LinedUp.Clear();

        return true;
    }

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
