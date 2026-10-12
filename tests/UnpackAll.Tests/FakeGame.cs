using UnpackAll.Core;

namespace UnpackAll.Tests;

/// <summary>
/// The game as far as an unpack press can tell: a bag, the floor at home, the unpacks the character has lined up and
/// what it is doing now. Starting lined-up actions replaces what the character is doing, as the game's default action
/// mode does, so a test can see a press that would have cancelled the character's work.
/// </summary>
internal sealed class FakeGame : IPackageGame
{
    /// <summary>What the character is doing before any press. Not an unpack.</summary>
    public const long Walking = -1;

    /// <summary>A package picked up off the floor gets a new id in the bag: the floor id plus this.</summary>
    public const long BagIdOffset = 1000;

    public string? NotReadyReason { get; set; }

    public int FreeSpots { get; set; }

    public bool IsHome { get; set; } = true;

    public List<Package> Bag { get; } = new();

    public List<Package> Floor { get; } = new();

    public List<long> LinedUp { get; } = new();

    public List<long> Running { get; private set; } = new() { Walking };

    /// <summary>Bag items the game's use check refuses, with its reason.</summary>
    public Dictionary<long, string> Refusals { get; } = new();

    /// <summary>Floor packages the bag cannot carry at all.</summary>
    public HashSet<long> TooHeavy { get; } = new();

    /// <summary>How many more packages the bag can carry in total.</summary>
    public int BagRoom { get; set; } = int.MaxValue;

    /// <summary>Floor packages that land on a stack already in the bag (floor id to bag id), as when packages stack.</summary>
    public Dictionary<long, long> MergesInto { get; } = new();

    public bool RefusesToQueue { get; set; }

    public bool FailsToStart { get; set; }

    public List<long> PickupsTried { get; } = new();

    public IEnumerable<long> Unpacking => Running.Where(id => id != Walking);

    public static Package Pkg(long itemId, int count = 1, int configId = 7) => new(itemId, configId, count);

    public PackageRead Read() => NotReadyReason is { } reason
        ? new PackageRead.NotReady(reason)
        : new PackageRead.Ready(new PackageStock(FreeSpots, LinedUp.Count + Unpacking.Count(), IsHome, Bag.ToList(), Floor.ToList()));

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

    /// <summary>Like the game's own queue, this takes only items that are in the bag, and no more units than the stack holds.</summary>
    public bool TryQueue(Package package)
    {
        var index = Bag.FindIndex(held => held.ItemId == package.ItemId);

        if (RefusesToQueue || index < 0 || LinedUp.Count(id => id == package.ItemId) >= Bag[index].Count) return false;

        LinedUp.Add(package.ItemId);

        return true;
    }

    public bool TryStart()
    {
        if (FailsToStart) return false;

        Running = LinedUp.ToList();

        LinedUp.Clear();

        return true;
    }

    private void AddToBag(long itemId, int configId, int count)
    {
        var index = Bag.FindIndex(held => held.ItemId == itemId);

        if (index < 0) Bag.Add(new Package(itemId, configId, count));
        else Bag[index] = Bag[index] with { Count = Bag[index].Count + count };
    }
}
