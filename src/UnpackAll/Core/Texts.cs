using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>One piece of on-screen text in the two languages the game ships.</summary>
public readonly record struct Phrase(string En, string Zh)
{
    public string Pick(bool isChinese) => isChinese ? Zh : En;

    /// <summary>A line the game wrote itself, which is already in the player's language.</summary>
    public static Phrase FromGame(string line) => new(line, line);
}

/// <summary>
/// The fixed words the mod puts on screen, including its own reasons a package can't be unpacked; the toasts after a
/// press are worded in <see cref="OutcomeMessage"/>.
/// </summary>
public static class Texts
{
    public static readonly Phrase MenuLabel = new("Unpack all", "全部拆封");

    public static readonly Phrase MenuTitle = new(
        "Unpack every package in your bag, then the ones on the floor at home, up to your free package spots",
        "拆封背包里的所有包裹，再拆封家里地上的包裹，数量以空余的包裹位为限");

    /// <summary>Who a toast is from, on its first line.</summary>
    public static readonly Phrase Source = new("Unpack All", "一键拆封");

    public static readonly Phrase Failed = new("Unpack All failed, see BepInEx/unpack-all.log.", "全部拆封失败，详见 BepInEx/unpack-all.log。");

    public static readonly Phrase NoWorld = new("the game world is not loaded", "游戏世界尚未加载");

    public static readonly Phrase NoHome = new("your home is not loaded", "你的家尚未加载");

    public static readonly Phrase ReadFailed = new("the game could not be read, see BepInEx/unpack-all.log", "无法读取游戏数据，详见 BepInEx/unpack-all.log");

    public static readonly Phrase CheckFailed = new("the game's use check failed, see BepInEx/unpack-all.log", "游戏的使用检查出错，详见 BepInEx/unpack-all.log");

    public static readonly Phrase NoReasonGiven = new("the game would not allow it", "游戏不允许这样做");

    public static readonly Phrase QueueRefused = new("the game would not line it up", "游戏无法将它加入队列");

    public static readonly Phrase SentOneToDoor = new(
        "1 crafted furniture package sent to the entryway. Remember to install it.",
        "1 件做好的家具包裹已送到玄关，记得去安装。");

    /// <summary>{0} is the number of packages sent.</summary>
    public static readonly Phrase SentToDoor = new(
        "{0} crafted furniture packages sent to the entryway. Remember to install them.",
        "{0} 件做好的家具包裹已送到玄关，记得去安装。");

    /// <summary>The entry in the game's click menu on a furniture package in the yard.</summary>
    public static readonly Phrase PickUpLabel = new("Pick up", "拿起");

    /// <summary>The entry's hover tip while it works; the tooltip does not wrap, so it stays short.</summary>
    public static readonly Phrase PickUpTip = new("Put it back in your bag. A bought package can't be returned to the shop after this.", "放回背包。买来的包裹之后无法再退货。");

    public static readonly Phrase NoBagForm = new("it can't go in a bag", "它无法放进背包");

    public static readonly Phrase BeingMoved = new("it is being moved or installed", "它正在搬动或安装");

    public static readonly Phrase PickUpGone = new("That package is no longer there.", "这个包裹已经不在了。");

    public static readonly Phrase PickUpNoRoom = new(
        "No room for the package: your bag can't carry it and the shopping bags are full. It stays in the yard.",
        "没有地方放这个包裹：背包装不下，购物袋也满了。包裹留在院子里。");

    public static readonly Phrase PickUpFailed = new(
        "The package could not be taken from the yard, so it stays there. See BepInEx/unpack-all.log.",
        "无法从院子里拿走这个包裹，它留在原处。详见 BepInEx/unpack-all.log。");

    public static readonly Phrase PickUpFailedKept = new(
        "The package could not be taken from the yard, and its copy could not be taken back out of your bag. See BepInEx/unpack-all.log.",
        "无法从院子里拿走这个包裹，放进背包的那一个也没能取回。详见 BepInEx/unpack-all.log。");

    public static readonly Phrase PickedIntoBag = new("Package put back in your bag.", "包裹已放回背包。");

    public static readonly Phrase PickedIntoShoppingBag = new(
        "Your bag is full, so the package went into a shopping bag in the living room.",
        "背包已满，包裹已放进客厅的购物袋。");

    public static readonly Phrase NoLongerRefundable = new("It can no longer be returned to the shop.", "它已无法再退货。");

    public static readonly Phrase PickUpError = new("Pick up failed, see BepInEx/unpack-all.log.", "拿起失败，详见 BepInEx/unpack-all.log。");

    public static IReadOnlyList<Phrase> All { get; } = new[]
    {
        MenuLabel, MenuTitle, Failed, NoWorld, NoHome, ReadFailed, CheckFailed, NoReasonGiven, QueueRefused, SentOneToDoor, SentToDoor,
        PickUpLabel, PickUpTip, NoBagForm, BeingMoved, PickUpGone, PickUpNoRoom, PickUpFailed, PickUpFailedKept, PickedIntoBag,
        PickedIntoShoppingBag, NoLongerRefundable, PickUpError,
    };
}
