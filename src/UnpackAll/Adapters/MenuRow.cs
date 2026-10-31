using System;
using ExiledToolkit;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>Unpack all in the Mods menu that Exiled's Toolkit draws on the inventory and the station screens.</summary>
internal static class MenuRow
{
    private const string Id = "unpackall.unpack";

    // An arrow rising out of an open box, drawn on the 24-unit grid the game's toolbar icons use.
    private const string Icon = "M4 10h4v2H6v8h12v-8h-2v-2h4v12H4zM12 2l5 5h-3v8h-4V7H7z";

    /// <param name="hotkey">The unpack key's name shown on the row; empty when the key is off.</param>
    public static ModsMenuEntry Entry(string hotkey, Action onClick) => new(Id, Texts.MenuLabel.En, onClick)
    {
        ChineseLabel = Texts.MenuLabel.Zh,
        Title = Texts.MenuTitle.En,
        ChineseTitle = Texts.MenuTitle.Zh,
        Icon = Icon,
        Order = 200,
        Stations = true,
        Hotkey = hotkey,
    };
}
