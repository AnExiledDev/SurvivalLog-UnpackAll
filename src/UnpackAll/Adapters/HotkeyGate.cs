using System;
using GameCore.HotUpdate.Battle.Show;
using HarmonyLib;
using UnityEngine;

namespace UnpackAll.Adapters;

/// <summary>
/// Stops the game's hotkeys while the inventory's Mods menu is open, and through the next frame after Esc closes it,
/// so that Esc closes only the menu and not the inventory too. The page gets no keys, so the session closes the menu
/// on Esc itself. Static because a Harmony patch has nowhere else to look.
/// </summary>
internal static class HotkeyGate
{
    private static bool _isMenuOpen;
    private static int _blockedThroughFrame = -1;

    public static bool IsMenuOpen => _isMenuOpen;

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

    public static void MenuOpened() => _isMenuOpen = true;

    /// <summary>Keeps hotkeys blocked through the next frame for the Esc that closed the menu; a menu already shut changes nothing.</summary>
    public static void MenuClosed()
    {
        if (!_isMenuOpen) return;

        _isMenuOpen = false;
        _blockedThroughFrame = Time.frameCount + 1;
    }

    private static bool SkipWhileBlocked() => !(_isMenuOpen || Time.frameCount <= _blockedThroughFrame);
}
