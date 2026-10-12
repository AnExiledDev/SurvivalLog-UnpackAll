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

    /// <summary>There is no game to unpack in: the title screen, a save still loading, or the prologue.</summary>
    public sealed record NoGame : PackageRead;
}

/// <summary>
/// Everything one press decides from. FreeSpots is how many more packages the home takes (see
/// <see cref="PackageRules.FreeSpots"/>). AlreadyQueued counts the unpacks the character still has lined up. Floor
/// holds the packages lying on the floor at home; IsHome says whether the character is there to pick them up.
/// QueueOnStart says what starting the unpacks would do to the actions the character already has.
/// </summary>
public sealed record PackageStock(int FreeSpots, int AlreadyQueued, bool IsHome, IReadOnlyList<Package> Bag, IReadOnlyList<Package> Floor, QueueOnStart QueueOnStart);

/// <summary>What starting the lined-up unpacks does to the character's action queue.</summary>
public enum QueueOnStart
{
    /// <summary>The unpacks join the end of the queue and everything already in it stays.</summary>
    Appends,

    /// <summary>The queue is replaced, but it holds nothing the player queued: only what the character chose to do itself.</summary>
    ReplacesOwnActions,

    /// <summary>The queue is replaced and actions the player queued would be lost.</summary>
    ReplacesPlayerActions,
}
