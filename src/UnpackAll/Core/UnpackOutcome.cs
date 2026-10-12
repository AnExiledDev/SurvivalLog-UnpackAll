namespace UnpackAll.Core;

/// <summary>How one unpack press ended.</summary>
public abstract record UnpackOutcome
{
    private UnpackOutcome()
    {
    }

    public sealed record NotReady(string Reason) : UnpackOutcome;

    /// <summary>Unpacks from an earlier press (or a hand-used package) are still lined up, so nothing was touched.</summary>
    public sealed record AlreadyUnpacking(int Queued) : UnpackOutcome;

    public sealed record NothingToUnpack : UnpackOutcome;

    /// <summary>Unpacks were lined up but the game would not start them.</summary>
    public sealed record StartFailed(int Queued) : UnpackOutcome;

    public sealed record Done(UnpackTally Tally) : UnpackOutcome;
}

/// <summary>
/// What a press did, counted in packages. Queued started unpacking; PickedUp came off the floor into the bag. The rest
/// were left, each for one reason: NoSpot had no free package spot at home, FloorRefused could not be carried,
/// FloorAway lie at home while the character is elsewhere, and Unusable were refused by the game's use check, the
/// first refusal's reason kept for the message.
/// </summary>
public sealed record UnpackTally(int Queued, int PickedUp, int NoSpot, int FloorRefused, int FloorAway, int Unusable, string? UnusableReason);
