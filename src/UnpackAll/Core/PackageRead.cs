using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>What the game holds for one unpack press, or why it could not be read.</summary>
public abstract record PackageRead
{
    private PackageRead()
    {
    }

    public sealed record Ready(PackageStock Stock) : PackageRead;

    public sealed record NotReady(string Reason) : PackageRead;
}

/// <summary>
/// Everything one press decides from. FreeSpots is how many more packages the home takes (see
/// <see cref="PackageRules.FreeSpots"/>). AlreadyQueued counts the unpacks the character still has lined up. Floor
/// holds the packages lying on the floor at home; IsHome says whether the character is there to pick them up.
/// </summary>
public sealed record PackageStock(int FreeSpots, int AlreadyQueued, bool IsHome, IReadOnlyList<Package> Bag, IReadOnlyList<Package> Floor);
