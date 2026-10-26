using System;
using GameCore.HotUpdate.Battle.Show;
using HarmonyLib;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// Stops the game's hotkeys while the inventory's Mods menu is open, and through the next frame after Esc closes it,
/// so that Esc closes only the menu and not the inventory too. The page gets no keys, so the session closes the menu
/// on Esc itself. Static because a Harmony patch has nowhere else to look. The hook runs every frame, and returns at
/// once without asking Unity for the frame while nothing is held.
/// </summary>
internal static class HotkeyGate
{
    private static readonly HotkeyBlock Block = new();

    public static bool IsMenuOpen => Block.IsMenuOpen;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HotKeyManager), "Update"), prefix: new HarmonyMethod(typeof(HotkeyGate), nameof(SkipWhileBlocked)));
            log("hotkeys are blocked while the Mods menu is open");
        }
        catch (Exception ex)
        {
            log("could not hook HotKeyManager.Update, Esc on the Mods menu will also reach the game: " + ex.Message);
        }
    }

    public static void MenuOpened() => Block.MenuOpened();

    /// <summary>Keeps hotkeys blocked through the next frame for the Esc that closed the menu; a menu already shut changes nothing.</summary>
    public static void MenuClosed()
    {
        if (Block.IsMenuOpen) Block.MenuClosed(Time.frameCount);
    }

    private static bool SkipWhileBlocked() => !Block.IsHeld || !Block.IsBlocked(Time.frameCount);
}
