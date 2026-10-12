namespace UnpackAll.Core;

/// <summary>How one unpack press ended.</summary>
public abstract record UnpackOutcome
{
    private UnpackOutcome()
    {
    }

    /// <summary>No game to unpack in (title screen, loading, prologue); the press is ignored.</summary>
    public sealed record NoGame : UnpackOutcome;

    public sealed record NotReady(string Reason) : UnpackOutcome;

    /// <summary>Unpacks from an earlier press (or a hand-used package) are still lined up, so nothing was touched.</summary>
    public sealed record AlreadyUnpacking(int Queued) : UnpackOutcome;

    /// <summary>No package anywhere a press looks. HasLookedInStorages says whether the home's storages were among them.</summary>
    public sealed record NothingToUnpack(bool HasLookedInStorages = false) : UnpackOutcome;

    /// <summary>Starting would have thrown away actions the player queued, so nothing was touched.</summary>
    public sealed record QueueBusy : UnpackOutcome;

    /// <summary>
    /// Unpacks were lined up but the game would not start them. PickedUp packages came off the floor and FromStorage
    /// came out of storages first, and all of them are in the bag.
    /// </summary>
    public sealed record StartFailed(int Queued, int PickedUp, int FromStorage = 0) : UnpackOutcome;

    public sealed record Done(UnpackTally Tally) : UnpackOutcome;
}

/// <summary>
/// What a press did, counted in packages. Queued started unpacking and were seen in the character's queue afterwards;
/// Dropped were lined up and started, but the game threw them away and their packages are still in the bag. PickedUp
/// came off the floor into the bag and FromStorage came out of the home's storages into it, and WaitingInBag of those
/// are still in it, not lined up or dropped. The rest were left, each for one reason: NoSpot had no free package spot
/// at home, FloorRefused and StorageRefused could not be carried, FloorAway and StorageAway lie at home while the
/// character is elsewhere, StorageBehindPanel stayed in storages because another window was open, and Unusable were
/// refused by the game's use check, the first refusal's reason kept for the message.
/// </summary>
public sealed record UnpackTally(
    int Queued,
    int PickedUp,
    int WaitingInBag,
    int NoSpot,
    int FloorRefused,
    int FloorAway,
    int Unusable,
    string? UnusableReason,
    int Dropped = 0,
    int FromStorage = 0,
    int StorageRefused = 0,
    int StorageAway = 0,
    int StorageBehindPanel = 0);
