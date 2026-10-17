using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>One piece of on-screen text in the two languages the game ships.</summary>
public readonly record struct Phrase(string En, string Zh)
{
    public string Pick(bool isChinese) => isChinese ? Zh : En;
}

/// <summary>The fixed words the mod puts on screen; the toasts after a press are worded in <see cref="OutcomeMessage"/>.</summary>
public static class Texts
{
    public static readonly Phrase MenuLabel = new("Unpack all", "全部拆封");

    public static readonly Phrase MenuTitle = new(
        "Unpack every package in your bag, then the ones on the floor at home, up to your free package spots",
        "拆封背包里的所有包裹，再拆封家里地上的包裹，数量以空余的包裹位为限");

    public static readonly Phrase Failed = new("Unpack All failed, see BepInEx/unpack-all.log.", "全部拆封失败，详见 BepInEx/unpack-all.log。");

    public static IReadOnlyList<Phrase> All { get; } = new[] { MenuLabel, MenuTitle, Failed };
}
