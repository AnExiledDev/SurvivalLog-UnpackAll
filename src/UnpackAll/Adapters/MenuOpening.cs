using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate.ReduxUI;

namespace UnpackAll.Adapters;

/// <summary>
/// Tells the session the moment the game opens its click menu, in the same call that opens it. The game sends the
/// menu's buttons to the web view later in the frame, so the Pick up entry's state reaches the page first and the
/// entry is drawn with the game's. Static because a Harmony patch has nowhere else to look.
/// </summary>
internal static class MenuOpening
{
    private static Action<long> _onOpened = _ => { };
    private static PatchKit? _hooks;

    public static void Apply(PatchKit hooks, Action<long> onOpened)
    {
        _onOpened = onOpened;
        _hooks = hooks;

        hooks.Patch(
            typeof(MenuOpening),
            typeof(Ac_SelectMenu_Open),
            nameof(Ac_SelectMenu_Open.SendAction),
            postfix: nameof(Opened),
            done: "the Pick up entry is sent as the click menu opens",
            consequence: "so the click menu has no Pick up entry");
    }

    // Harmony matches the parameter by its name in the game's method.
    private static void Opened(long FurnitureLogicId)
    {
        try
        {
            _onOpened(FurnitureLogicId);
        }
        catch (Exception ex)
        {
            _hooks?.Fail("sending the Pick up entry as the menu opened", ex);
        }
    }
}
