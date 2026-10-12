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
/// QueueOnStart says what starting the unpacks would do to the actions the character already has. Storages holds the
/// packages in the home's storages, and StorageAccess says whether they may be taken out right now.
/// </summary>
public sealed record PackageStock(
    int FreeSpots,
    int AlreadyQueued,
    bool IsHome,
    IReadOnlyList<Package> Bag,
    IReadOnlyList<Package> Floor,
    QueueOnStart QueueOnStart,
    IReadOnlyList<Package> Storages,
    StorageAccess StorageAccess);

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

/// <summary>Whether a press may take packages out of the home's storages.</summary>
public enum StorageAccess
{
    /// <summary>Turned off in the config: storages are not read and nothing in them is touched.</summary>
    Off,

    /// <summary>Packages in storages may be moved into the bag and unpacked.</summary>
    Allowed,

    /// <summary>
    /// Another window than the player's own bag is open (a storage, a workbench, a shop), so storages are read but
    /// nothing is taken out of them: moving items under an open window could leave that window showing stale stacks.
    /// </summary>
    PanelOpen,
}
