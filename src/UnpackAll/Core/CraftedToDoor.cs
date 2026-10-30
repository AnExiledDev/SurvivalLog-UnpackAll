using System;
using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>A furniture package the workbench just made: the item and its config row.</summary>
public readonly record struct CraftedPackage(long ItemId, int ConfigId);

/// <summary>
/// Which freshly crafted furniture packages go straight to the entryway, the same as the workbench result's
/// 「拆到玄关」 button. A package an open commission still wants stays a package so it can be handed in, and no more are
/// sent than the entryway has room for: the game shows a "full" message for every one it cannot place.
/// </summary>
public static class CraftedToDoor
{
    /// <summary>Fills <paramref name="send"/> with the item ids to send, in the order they were crafted.</summary>
    /// <param name="isWanted">Whether an open commission wants packages of this config.</param>
    /// <param name="room">How many more packages the entryway takes.</param>
    public static void Pick(bool isOn, IReadOnlyList<CraftedPackage> crafted, Func<int, bool> isWanted, int room, List<long> send)
    {
        send.Clear();

        if (!isOn) return;

        foreach (var package in crafted)
        {
            if (send.Count >= room) return;
            if (isWanted(package.ConfigId)) continue;

            send.Add(package.ItemId);
        }
    }

    /// <summary>The toast after a craft sent packages to the entryway, or null when none went.</summary>
    public static string? Message(int sent, bool isChinese) => sent switch
    {
        <= 0 => null,
        1 => Texts.SentOneToDoor.Pick(isChinese),
        _ => string.Format(Texts.SentToDoor.Pick(isChinese), sent),
    };
}
