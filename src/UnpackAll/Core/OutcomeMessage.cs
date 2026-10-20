using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnpackAll.Core;

/// <summary>
/// The toast a press ends with: what started, then each reason something was left and how many it left, so a
/// package that did not unpack is never a mystery. Worded in English or in Chinese, the game's two languages.
/// </summary>
public static class OutcomeMessage
{
    private static readonly Phrase SeeLog = new("see BepInEx/unpack-all.log.", "详见 BepInEx/unpack-all.log。");

    /// <summary>Western and full-width (Chinese) full stop, exclamation and question marks, then whitespace.</summary>
    private static readonly char[] ClosingPunctuation = { '.', '!', '?', '。', '！', '？', ' ', '\t', '\r', '\n' };

    /// <summary>Unity rich-text tags such as &lt;color=#f55&gt;, &lt;/b&gt; or &lt;#ff0000&gt;, which the game's own UI renders but a toast would print.</summary>
    private static readonly Regex RichTextTag = new("</?[A-Za-z][^<>]*>|<#[0-9A-Fa-f]{3,8}>", RegexOptions.CultureInvariant);

    /// <summary>The toast for a press, or null when the press should pass without a word.</summary>
    public static string? Describe(UnpackOutcome outcome, bool isChinese = false) => outcome switch
    {
        UnpackOutcome.NoGame => null,
        UnpackOutcome.NotReady notReady => isChinese ? $"现在无法拆封：{Reason(notReady.Reason, isChinese)}。" : $"Can't unpack right now: {Reason(notReady.Reason, isChinese)}.",
        UnpackOutcome.AlreadyUnpacking busy => isChinese
            ? $"正在拆封 {busy.Queued} 个包裹，拆完后再按一次。"
            : $"Already unpacking {Packages(busy.Queued)}. Press again once {ItOrThey(busy.Queued)} {IsOrAre(busy.Queued)} done.",
        UnpackOutcome.NothingToUnpack nothing => NothingMessage(nothing.HasLookedInStorages, isChinese),
        UnpackOutcome.QueueBusy => isChinese ? "请先完成或取消排队中的行动。" : "Finish or cancel your queued actions first.",
        UnpackOutcome.StartFailed failed => StartFailedMessage(failed.PickedUp, failed.FromStorage, isChinese),
        UnpackOutcome.Done done => isChinese ? DescribeChinese(done.Tally) : Describe(done.Tally),
        _ => throw new InvalidOperationException("unknown unpack outcome " + outcome),
    };

    private static string Describe(UnpackTally tally)
    {
        var see = SeeLog.En;
        var lines = new List<string> { tally.Queued > 0 ? $"Unpacking {Packages(tally.Queued)}." : "Nothing unpacked." };

        if (tally.Dropped > 0) lines.Add($"{tally.Dropped} dropped by the game before {ItOrThey(tally.Dropped)} started, {see}");
        if (tally.PickedUp + tally.FromStorage > 0) lines.Add(BroughtLine(tally.PickedUp, tally.FromStorage, tally.WaitingInBag, isChinese: false));
        if (tally.NoSpot > 0) lines.Add($"{tally.NoSpot} left: no free package spot at home.");
        if (tally.FloorRefused > 0) lines.Add($"{tally.FloorRefused} left on the floor: your bag could not carry {ItOrThem(tally.FloorRefused)}.");
        if (tally.StorageRefused > 0) lines.Add($"{tally.StorageRefused} left in storages: your bag could not carry {ItOrThem(tally.StorageRefused)}.");
        if (tally.FloorAway > 0) lines.Add($"{tally.FloorAway} on the floor at home: go home to pick {ItOrThem(tally.FloorAway)} up.");
        if (tally.StorageAway > 0) lines.Add($"{tally.StorageAway} in storages at home: go home to unpack {ItOrThem(tally.StorageAway)}.");
        if (tally.StorageBehindPanel > 0) lines.Add($"{tally.StorageBehindPanel} left in storages: close the open window and press again.");
        if (tally.StorageUnchecked > 0) lines.Add($"{tally.StorageUnchecked} left in storages: could not check for open windows, {see}");
        if (tally.StorageTooBig > 0) lines.Add($"{tally.StorageTooBig} left in storages: a stack is bigger than the free package spots.");
        if (tally.Unusable > 0) lines.Add(UnusableLine(tally.Unusable, tally.UnusableReason, isChinese: false));

        return string.Join(" ", lines);
    }

    /// <summary>The same lines in Chinese, which needs no plurals and runs its sentences together without spaces.</summary>
    private static string DescribeChinese(UnpackTally tally)
    {
        var see = SeeLog.Zh;
        var lines = new List<string> { tally.Queued > 0 ? $"正在拆封 {tally.Queued} 个包裹。" : "没有拆封任何包裹。" };

        if (tally.Dropped > 0) lines.Add($"{tally.Dropped} 个在开始前被游戏移出了队列，{see}");
        if (tally.PickedUp + tally.FromStorage > 0) lines.Add(BroughtLine(tally.PickedUp, tally.FromStorage, tally.WaitingInBag, isChinese: true));
        if (tally.NoSpot > 0) lines.Add($"{tally.NoSpot} 个未拆：家里没有空余的包裹位。");
        if (tally.FloorRefused > 0) lines.Add($"{tally.FloorRefused} 个留在地上：背包装不下。");
        if (tally.StorageRefused > 0) lines.Add($"{tally.StorageRefused} 个留在储物家具中：背包装不下。");
        if (tally.FloorAway > 0) lines.Add($"{tally.FloorAway} 个在家里地上：回家后才能捡起。");
        if (tally.StorageAway > 0) lines.Add($"{tally.StorageAway} 个在家里的储物家具中：回家后才能拆封。");
        if (tally.StorageBehindPanel > 0) lines.Add($"{tally.StorageBehindPanel} 个留在储物家具中：请关闭打开的界面后再按一次。");
        if (tally.StorageUnchecked > 0) lines.Add($"{tally.StorageUnchecked} 个留在储物家具中：无法检查是否有打开的界面，{see}");
        if (tally.StorageTooBig > 0) lines.Add($"{tally.StorageTooBig} 个留在储物家具中：一组包裹超过了空余的包裹位。");
        if (tally.Unusable > 0) lines.Add(UnusableLine(tally.Unusable, tally.UnusableReason, isChinese: true));

        return string.Concat(lines);
    }

    /// <summary>The toast once the unpacks a press started have all left the queue, or null when each one used its package.</summary>
    public static string? Describe(WatchStep.Finished finished, bool isChinese = false)
    {
        var unused = finished.Started - finished.Consumed;

        if (unused <= 0) return null;

        if (isChinese)
        {
            var share = unused == finished.Started ? $"{unused} 次拆封" : $"{finished.Started} 次拆封中有 {unused} 次";

            return $"{share}结束时没有打开包裹，{SeeLog.Zh}";
        }

        var count = unused == finished.Started ? $"{unused}" : $"{unused} of {finished.Started}";
        var their = unused == 1 ? "its package" : "their packages";

        return $"{count} {(finished.Started == 1 ? "unpack" : "unpacks")} ended without opening {their}, {SeeLog.En}";
    }

    private static string NothingMessage(bool hasLookedInStorages, bool isChinese) => (hasLookedInStorages, isChinese) switch
    {
        (true, true) => "背包、家里地上和储物家具中都没有包裹。",
        (false, true) => "背包和家里地上都没有包裹。",
        (true, false) => "No packages in your bag, on the floor or in storages at home.",
        (false, false) => "No packages in your bag or on the floor at home.",
    };

    /// <summary>Nothing started, so every package brought into the bag before the failure is still in it.</summary>
    private static string StartFailedMessage(int pickedUp, int fromStorage, bool isChinese)
    {
        var failed = isChinese ? "无法开始拆封，" + SeeLog.Zh : "Could not start unpacking, " + SeeLog.En;

        if (pickedUp + fromStorage == 0) return failed;

        var brought = BroughtLine(pickedUp, fromStorage, waitingInBag: pickedUp + fromStorage, isChinese);

        return isChinese ? failed + brought : $"{failed} {brought}";
    }

    /// <summary>Where the packages brought into the bag came from, and how many of them are still in it.</summary>
    private static string BroughtLine(int pickedUp, int fromStorage, int waitingInBag, bool isChinese)
    {
        if (isChinese)
        {
            var taken = (pickedUp, fromStorage) switch
            {
                (> 0, > 0) => $"从地上捡起 {pickedUp} 个，并从储物家具取出 {fromStorage} 个",
                (> 0, _) => $"从地上捡起 {pickedUp} 个",
                _ => $"从储物家具取出 {fromStorage} 个",
            };

            return waitingInBag > 0 ? $"{taken}，{waitingInBag} 个在背包里等待拆封。" : $"{taken}。";
        }

        var brought = (pickedUp, fromStorage) switch
        {
            (> 0, > 0) => $"Picked up {pickedUp} from the floor and took {fromStorage} from storages",
            (> 0, _) => $"Picked up {pickedUp} from the floor",
            _ => $"Took {fromStorage} from storages",
        };

        return waitingInBag > 0 ? $"{brought}, {waitingInBag} waiting in your bag." : $"{brought}.";
    }

    private static string UnusableLine(int count, Phrase? reason, bool isChinese)
    {
        var trimmed = reason is { } given ? Reason(given, isChinese) : "";

        if (isChinese) return trimmed.Length == 0 ? $"{count} 个现在无法拆封。" : $"{count} 个现在无法拆封：{trimmed}。";

        return trimmed.Length == 0 ? $"{count} can't be unpacked right now." : $"{count} can't be unpacked right now: {trimmed}.";
    }

    /// <summary>
    /// The reason in the player's language. The game's reasons are whole sentences, sometimes coloured with rich-text
    /// tags; the tags would print as text, and their own closing mark would double ours.
    /// </summary>
    private static string Reason(Phrase reason, bool isChinese) => RichTextTag.Replace(reason.Pick(isChinese), "").Trim().TrimEnd(ClosingPunctuation);

    private static string Packages(int count) => count == 1 ? "1 package" : $"{count} packages";

    private static string ItOrThey(int count) => count == 1 ? "it" : "they";

    private static string ItOrThem(int count) => count == 1 ? "it" : "them";

    private static string IsOrAre(int count) => count == 1 ? "is" : "are";
}
