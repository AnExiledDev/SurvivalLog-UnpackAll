using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnpackAll.Core;

/// <summary>
/// The toast a press ends with: what started, then each reason something was left and how many it left, so a
/// package that did not unpack is never a mystery.
/// </summary>
public static class OutcomeMessage
{
    private const string SeeLog = "see BepInEx/unpack-all.log.";

    private static readonly char[] ClosingPunctuation = { '.', '!', '?', '。', '！', '？', ' ', '\t', '\r', '\n' };

    /// <summary>Unity rich-text tags such as &lt;color=#f55&gt;, &lt;/b&gt; or &lt;#ff0000&gt;, which the game's own UI renders but a toast would print.</summary>
    private static readonly Regex RichTextTag = new("</?[A-Za-z][^<>]*>|<#[0-9A-Fa-f]{3,8}>", RegexOptions.CultureInvariant);

    /// <summary>The toast for a press, or null when the press should pass without a word.</summary>
    public static string? Describe(UnpackOutcome outcome) => outcome switch
    {
        UnpackOutcome.NoGame => null,
        UnpackOutcome.NotReady notReady => $"Can't unpack right now: {Reason(notReady.Reason)}.",
        UnpackOutcome.AlreadyUnpacking busy => $"Already unpacking {Packages(busy.Queued)}. Press again once {ItOrThey(busy.Queued)} {IsOrAre(busy.Queued)} done.",
        UnpackOutcome.NothingToUnpack => "No packages in your bag or on the floor at home.",
        UnpackOutcome.QueueBusy => "Finish or cancel your queued actions first.",
        UnpackOutcome.StartFailed failed => StartFailedMessage(failed.PickedUp),
        UnpackOutcome.Done done => Describe(done.Tally),
        _ => throw new InvalidOperationException("unknown unpack outcome " + outcome),
    };

    private static string Describe(UnpackTally tally)
    {
        var lines = new List<string> { tally.Queued > 0 ? $"Unpacking {Packages(tally.Queued)}." : "Nothing unpacked." };

        if (tally.Dropped > 0) lines.Add($"{tally.Dropped} dropped by the game before {ItOrThey(tally.Dropped)} started, {SeeLog}");
        if (tally.PickedUp > 0) lines.Add(PickupLine(tally.PickedUp, tally.WaitingInBag));
        if (tally.NoSpot > 0) lines.Add($"{tally.NoSpot} left: no free package spot at home.");
        if (tally.FloorRefused > 0) lines.Add($"{tally.FloorRefused} left on the floor: your bag could not carry {ItOrThem(tally.FloorRefused)}.");
        if (tally.FloorAway > 0) lines.Add($"{tally.FloorAway} on the floor at home: go home to pick {ItOrThem(tally.FloorAway)} up.");
        if (tally.Unusable > 0) lines.Add(UnusableLine(tally.Unusable, tally.UnusableReason));

        return string.Join(" ", lines);
    }

    /// <summary>The toast once the unpacks a press started have all left the queue, or null when each one used its package.</summary>
    public static string? Describe(WatchStep.Finished finished)
    {
        var unused = finished.Started - finished.Consumed;

        if (unused <= 0) return null;

        var count = unused == finished.Started ? $"{unused}" : $"{unused} of {finished.Started}";
        var their = unused == 1 ? "its package" : "their packages";

        return $"{count} {(finished.Started == 1 ? "unpack" : "unpacks")} ended without opening {their}, {SeeLog}";
    }

    /// <summary>Nothing started, so every package picked up before the failure is still in the bag.</summary>
    private static string StartFailedMessage(int pickedUp)
    {
        const string failed = "Could not start unpacking, " + SeeLog;

        return pickedUp > 0 ? $"{failed} {PickupLine(pickedUp, waitingInBag: pickedUp)}" : failed;
    }

    private static string PickupLine(int pickedUp, int waitingInBag) =>
        waitingInBag > 0 ? $"Picked up {pickedUp} from the floor, {waitingInBag} waiting in your bag." : $"Picked up {pickedUp} from the floor.";

    private static string UnusableLine(int count, string? reason)
    {
        var trimmed = reason is null ? "" : Reason(reason);

        return trimmed.Length == 0 ? $"{count} can't be unpacked right now." : $"{count} can't be unpacked right now: {trimmed}.";
    }

    /// <summary>
    /// The game's reasons are whole sentences, often in Chinese and sometimes coloured with rich-text tags; the tags
    /// would print as text, and their own closing mark would double ours.
    /// </summary>
    private static string Reason(string reason) => RichTextTag.Replace(reason, "").Trim().TrimEnd(ClosingPunctuation);

    private static string Packages(int count) => count == 1 ? "1 package" : $"{count} packages";

    private static string ItOrThey(int count) => count == 1 ? "it" : "they";

    private static string ItOrThem(int count) => count == 1 ? "it" : "them";

    private static string IsOrAre(int count) => count == 1 ? "is" : "are";
}
