using System;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's game side, run once a frame. Button clicks arrive on the web view's thread and the hotkey arrives here;
/// both only ask for a press, and the press itself runs in this frame's update, at most once a frame.
/// </summary>
internal sealed class UnpackSession
{
    private const string FailedStatus = "Unpack All failed, see BepInEx/unpack-all.log.";

    private readonly GamePackages _game;
    private readonly WebPage _page;
    private readonly KeyCode _unpackKey;
    private readonly Action<string> _log;

    public UnpackSession(GamePackages game, WebPage page, KeyCode unpackKey, Action<string> log)
    {
        _game = game;
        _page = page;
        _unpackKey = unpackKey;
        _log = log;
    }

    public void Update()
    {
        _page.KeepInstalled(Time.unscaledTime);

        var isPressed = TakePageCommands();

        if (_unpackKey != KeyCode.None && Input.GetKeyDown(_unpackKey)) isPressed = true;
        if (isPressed) UnpackAll();
    }

    /// <summary>Handles the page's messages; true when the button was clicked at least once since the last frame.</summary>
    private bool TakePageCommands()
    {
        var isPressed = false;

        while (PageMessages.TryTake(out var command))
        {
            switch (command)
            {
                case PageCommand.Unpack:
                    isPressed = true;
                    break;
                case PageCommand.Ready:
                    _log("page script ready");
                    break;
                case PageCommand.NoScript:
                    _page.Install();
                    break;
                case PageCommand.Unknown unknown:
                    _log("page said: " + unknown.Text);
                    break;
                default:
                    throw new InvalidOperationException("unhandled page command " + command);
            }
        }

        return isPressed;
    }

    private void UnpackAll()
    {
        try
        {
            var outcome = UnpackRun.Run(_game);
            var message = OutcomeMessage.Describe(outcome);

            _log($"press: {outcome} -> {message}");
            _page.Status(message);
        }
        catch (Exception ex)
        {
            _log("press failed: " + ex);
            _page.Status(FailedStatus);
        }
    }
}
