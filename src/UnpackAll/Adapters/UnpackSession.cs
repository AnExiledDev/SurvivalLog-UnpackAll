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
    private const float LanguageLookSeconds = 1f;

    private readonly GamePackages _game;
    private readonly WebPage _page;
    private readonly KeyCode _unpackKey;
    private readonly Action<string> _log;
    private readonly UnpackWatcher _watcher;

    private bool? _shownChinese;
    private float _nextLanguageLook;

    public UnpackSession(GamePackages game, WebPage page, KeyCode unpackKey, Action<string> log)
    {
        _game = game;
        _page = page;
        _unpackKey = unpackKey;
        _log = log;
        _watcher = new UnpackWatcher(game, page, log);
    }

    public void Update()
    {
        _page.KeepInstalled(Time.unscaledTime);
        _watcher.Tick(Time.unscaledTime);
        KeepLanguage(Time.unscaledTime);

        var isPressed = TakePageCommands();

        if (HotkeyGate.IsMenuOpen && Input.GetKeyDown(KeyCode.Escape)) CloseMenu();
        if (_unpackKey != KeyCode.None && Input.GetKeyDown(_unpackKey)) isPressed = true;
        if (isPressed) UnpackAll();
    }

    /// <summary>The player can switch the game's language at any time, so it is read once a second and the menu row follows.</summary>
    private void KeepLanguage(float now)
    {
        if (now < _nextLanguageLook) return;

        _nextLanguageLook = now + LanguageLookSeconds;

        var isChinese = GameLanguage.IsChinese();

        if (isChinese == _shownChinese) return;

        _shownChinese = isChinese;
        _page.ShowMenuText(Texts.MenuLabel.Pick(isChinese), Texts.MenuTitle.Pick(isChinese));
    }

    private void CloseMenu()
    {
        _page.CloseMenu();
        HotkeyGate.MenuClosed();
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
                case PageCommand.Menu { IsOpen: true }:
                    HotkeyGate.MenuOpened();
                    break;
                case PageCommand.Menu:
                    HotkeyGate.MenuClosed();
                    break;
                case PageCommand.Ready:
                    _log("page script ready");
                    // A reloaded page has no menu, and one left marked open would hold the game's hotkeys for good.
                    HotkeyGate.MenuClosed();
                    _page.ShowKey(_unpackKey == KeyCode.None ? "" : _unpackKey.ToString());
                    // A reloaded page shows the English row again, so the next language look renames it.
                    _shownChinese = null;
                    break;
                case PageCommand.NoScript:
                    HotkeyGate.MenuClosed();
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
            var message = OutcomeMessage.Describe(outcome, GameLanguage.IsChinese());

            _log($"press: {outcome} -> {message ?? "(no message)"}");

            if (message != null) _page.Status(message);
            if (outcome is UnpackOutcome.Done done) _watcher.Begin(done.Tally.Queued, Time.unscaledTime);
        }
        catch (Exception ex)
        {
            _log("press failed: " + ex);
            _page.Status(Texts.Failed.Pick(GameLanguage.IsChinese()));
        }
    }
}
