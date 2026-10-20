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

    public static readonly Phrase Failed = new("Unpack All failed, see BepInEx/unpack-all.log.", "全部拆封失败，详见 BepInEx/unpack-all.log。");

    public static readonly Phrase NoWorld = new("the game world is not loaded", "游戏世界尚未加载");

    public static readonly Phrase NoHome = new("your home is not loaded", "你的家尚未加载");

    public static readonly Phrase ReadFailed = new("the game could not be read, see BepInEx/unpack-all.log", "无法读取游戏数据，详见 BepInEx/unpack-all.log");

    public static readonly Phrase CheckFailed = new("the game's use check failed, see BepInEx/unpack-all.log", "游戏的使用检查出错，详见 BepInEx/unpack-all.log");

    public static readonly Phrase NoReasonGiven = new("the game would not allow it", "游戏不允许这样做");

    public static readonly Phrase QueueRefused = new("the game would not line it up", "游戏无法将它加入队列");

    public static IReadOnlyList<Phrase> All { get; } = new[] { MenuLabel, MenuTitle, Failed, NoWorld, NoHome, ReadFailed, CheckFailed, NoReasonGiven, QueueRefused };
}
