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

    public sealed record NothingToUnpack : UnpackOutcome;

    /// <summary>Starting would have thrown away actions the player queued, so nothing was touched.</summary>
    public sealed record QueueBusy : UnpackOutcome;

    /// <summary>Unpacks were lined up but the game would not start them. PickedUp packages came off the floor first and are in the bag.</summary>
    public sealed record StartFailed(int Queued, int PickedUp) : UnpackOutcome;

    public sealed record Done(UnpackTally Tally) : UnpackOutcome;
}

/// <summary>
/// What a press did, counted in packages. Queued started unpacking; PickedUp came off the floor into the bag, and
/// WaitingInBag of those were not lined up and are still in it. The rest were left, each for one reason: NoSpot had no
/// free package spot at home, FloorRefused could not be carried, FloorAway lie at home while the character is
/// elsewhere, and Unusable were refused by the game's use check, the first refusal's reason kept for the message.
/// </summary>
public sealed record UnpackTally(int Queued, int PickedUp, int WaitingInBag, int NoSpot, int FloorRefused, int FloorAway, int Unusable, string? UnusableReason);
