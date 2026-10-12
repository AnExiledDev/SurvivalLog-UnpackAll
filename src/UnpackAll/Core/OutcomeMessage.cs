using System;
using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>
/// The toast a press ends with: what started, then each reason something was left and how many it left, so a
/// package that did not unpack is never a mystery.
/// </summary>
public static class OutcomeMessage
{
    private static readonly char[] ClosingPunctuation = { '.', '!', '?', '。', '！', '？', ' ', '\t', '\r', '\n' };

    public static string Describe(UnpackOutcome outcome) => outcome switch
    {
        UnpackOutcome.NotReady notReady => $"Can't unpack right now: {Reason(notReady.Reason)}.",
        UnpackOutcome.AlreadyUnpacking busy => $"Already unpacking {Packages(busy.Queued)}. Press again once {ItOrThey(busy.Queued)} {IsOrAre(busy.Queued)} done.",
        UnpackOutcome.NothingToUnpack => "No packages in your bag or on the floor at home.",
        UnpackOutcome.StartFailed => "Could not start unpacking, see BepInEx/unpack-all.log.",
        UnpackOutcome.Done done => Describe(done.Tally),
        _ => throw new InvalidOperationException("unknown unpack outcome " + outcome),
    };

    private static string Describe(UnpackTally tally)
    {
        var lines = new List<string> { tally.Queued > 0 ? $"Unpacking {Packages(tally.Queued)}." : "Nothing unpacked." };

        if (tally.PickedUp > 0) lines.Add($"Picked up {tally.PickedUp} from the floor.");
        if (tally.NoSpot > 0) lines.Add($"{tally.NoSpot} left: no free package spot at home.");
        if (tally.FloorRefused > 0) lines.Add($"{tally.FloorRefused} left on the floor: your bag could not carry {ItOrThem(tally.FloorRefused)}.");
        if (tally.FloorAway > 0) lines.Add($"{tally.FloorAway} on the floor at home: go home to pick {ItOrThem(tally.FloorAway)} up.");
        if (tally.Unusable > 0) lines.Add(UnusableLine(tally.Unusable, tally.UnusableReason));

        return string.Join(" ", lines);
    }

    private static string UnusableLine(int count, string? reason)
    {
        var trimmed = reason is null ? "" : Reason(reason);

        return trimmed.Length == 0 ? $"{count} can't be unpacked right now." : $"{count} can't be unpacked right now: {trimmed}.";
    }

    /// <summary>The game's reasons are whole sentences, often in Chinese; their own closing mark would double ours.</summary>
    private static string Reason(string reason) => reason.Trim().TrimEnd(ClosingPunctuation);

    private static string Packages(int count) => count == 1 ? "1 package" : $"{count} packages";

    private static string ItOrThey(int count) => count == 1 ? "it" : "they";

    private static string ItOrThem(int count) => count == 1 ? "it" : "them";

    private static string IsOrAre(int count) => count == 1 ? "is" : "are";
}
