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

    /// <summary>Western and full-width (Chinese) full stop, exclamation and question marks, then whitespace.</summary>
    private static readonly char[] ClosingPunctuation = { '.', '!', '?', '\u3002', '\uFF01', '\uFF1F', ' ', '\t', '\r', '\n' };

    /// <summary>Unity rich-text tags such as &lt;color=#f55&gt;, &lt;/b&gt; or &lt;#ff0000&gt;, which the game's own UI renders but a toast would print.</summary>
    private static readonly Regex RichTextTag = new("</?[A-Za-z][^<>]*>|<#[0-9A-Fa-f]{3,8}>", RegexOptions.CultureInvariant);

    /// <summary>The toast for a press, or null when the press should pass without a word.</summary>
    public static string? Describe(UnpackOutcome outcome) => outcome switch
    {
        UnpackOutcome.NoGame => null,
        UnpackOutcome.NotReady notReady => $"Can't unpack right now: {Reason(notReady.Reason)}.",
        UnpackOutcome.AlreadyUnpacking busy => $"Already unpacking {Packages(busy.Queued)}. Press again once {ItOrThey(busy.Queued)} {IsOrAre(busy.Queued)} done.",
        UnpackOutcome.NothingToUnpack nothing => nothing.HasLookedInStorages ? "No packages in your bag, on the floor or in storages at home." : "No packages in your bag or on the floor at home.",
        UnpackOutcome.QueueBusy => "Finish or cancel your queued actions first.",
        UnpackOutcome.StartFailed failed => StartFailedMessage(failed.PickedUp, failed.FromStorage),
        UnpackOutcome.Done done => Describe(done.Tally),
        _ => throw new InvalidOperationException("unknown unpack outcome " + outcome),
    };

    private static string Describe(UnpackTally tally)
    {
        var lines = new List<string> { tally.Queued > 0 ? $"Unpacking {Packages(tally.Queued)}." : "Nothing unpacked." };

        if (tally.Dropped > 0) lines.Add($"{tally.Dropped} dropped by the game before {ItOrThey(tally.Dropped)} started, {SeeLog}");
        if (tally.PickedUp + tally.FromStorage > 0) lines.Add(BroughtLine(tally.PickedUp, tally.FromStorage, tally.WaitingInBag));
        if (tally.NoSpot > 0) lines.Add($"{tally.NoSpot} left: no free package spot at home.");
        if (tally.FloorRefused > 0) lines.Add($"{tally.FloorRefused} left on the floor: your bag could not carry {ItOrThem(tally.FloorRefused)}.");
        if (tally.StorageRefused > 0) lines.Add($"{tally.StorageRefused} left in storages: your bag could not carry {ItOrThem(tally.StorageRefused)}.");
        if (tally.FloorAway > 0) lines.Add($"{tally.FloorAway} on the floor at home: go home to pick {ItOrThem(tally.FloorAway)} up.");
        if (tally.StorageAway > 0) lines.Add($"{tally.StorageAway} in storages at home: go home to unpack {ItOrThem(tally.StorageAway)}.");
        if (tally.StorageBehindPanel > 0) lines.Add($"{tally.StorageBehindPanel} left in storages: close the open window and press again.");
        if (tally.StorageUnchecked > 0) lines.Add($"{tally.StorageUnchecked} left in storages: could not check for open windows, {SeeLog}");
        if (tally.StorageTooBig > 0) lines.Add($"{tally.StorageTooBig} left in storages: a stack is bigger than the free package spots.");
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

    /// <summary>Nothing started, so every package brought into the bag before the failure is still in it.</summary>
    private static string StartFailedMessage(int pickedUp, int fromStorage)
    {
        const string failed = "Could not start unpacking, " + SeeLog;

        return pickedUp + fromStorage > 0 ? $"{failed} {BroughtLine(pickedUp, fromStorage, waitingInBag: pickedUp + fromStorage)}" : failed;
    }

    /// <summary>Where the packages brought into the bag came from, and how many of them are still in it.</summary>
    private static string BroughtLine(int pickedUp, int fromStorage, int waitingInBag)
    {
        var brought = (pickedUp, fromStorage) switch
        {
            (> 0, > 0) => $"Picked up {pickedUp} from the floor and took {fromStorage} from storages",
            (> 0, _) => $"Picked up {pickedUp} from the floor",
            _ => $"Took {fromStorage} from storages",
        };

        return waitingInBag > 0 ? $"{brought}, {waitingInBag} waiting in your bag." : $"{brought}.";
    }

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
