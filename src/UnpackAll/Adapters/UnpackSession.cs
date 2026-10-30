using System;
using ExiledToolkit;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's game side, run once a frame. Button clicks arrive on the web view's thread and the hotkey arrives here;
/// both only ask for a press, and the press itself runs in this frame's update, at most once a frame.
/// </summary>
internal sealed class UnpackSession
{
    private const double LanguageLookSeconds = 1;

    private readonly GamePackages _game;
    private readonly IPickUpGame _pickUp;
    private readonly WebPage _page;
    private KeyCode _unpackKey = KeyCode.None;
    private readonly Action<string> _log;
    private readonly UnpackWatcher _watcher;
    private readonly Func<bool> _isTyping = OptionalLive.IsTyping;

    private bool? _shownChinese;
    private double _nextLanguageLook;

    /// <param name="detail">Logs a line to the mod's own file only.</param>
    public UnpackSession(GamePackages game, IPickUpGame pickUp, WebPage page, Action<string> log, Action<string> detail)
    {
        _game = game;
        _pickUp = pickUp;
        _page = page;
        _log = log;
        _watcher = new UnpackWatcher(game, page, log, detail);
    }

    /// <summary>Takes a new unpack key and names it on the menu row.</summary>
    public void UseKey(KeyCode unpackKey)
    {
        _unpackKey = unpackKey;
        _page.ShowKey(KeyName());
    }

    public void Update()
    {
        var now = Clock.Now;

        _page.KeepInstalled(now);
        _watcher.Tick(now);
        KeepLanguage(now);

        var isPressed = TakePageCommands();

        if (HotkeyGate.IsMenuOpen && Input.GetKeyDown(KeyCode.Escape)) CloseMenu();
        if (KeyGate.Fires(_unpackKey != KeyCode.None, _unpackKey != KeyCode.None && Input.GetKeyDown(_unpackKey), _isTyping)) isPressed = true;
        if (isPressed) UnpackAll();

        ShowCraftedSent();
    }

    /// <summary>
    /// Called by the game's own menu-open call, before the game sends the menu to the web view, so the page knows
    /// whether to add Pick up by the time the menu's entries are drawn.
    /// </summary>
    public void MenuOpened(long id)
    {
        var isChinese = GameLanguage.IsChinese();

        GameMenu.Opening(id);
        _page.ShowPickMenu(PickUpRun.EntryState(_pickUp, id == 0 ? null : id, isChinese), Texts.PickUpLabel.Pick(isChinese), Texts.PickUpTip.Pick(isChinese));
    }

    /// <summary>Tells the player when a craft sent its packages to the entryway; the game says nothing then.</summary>
    private void ShowCraftedSent()
    {
        var sent = CraftedDelivery.TakeSent();

        if (sent == 0) return;

        var message = CraftedToDoor.Message(sent, GameLanguage.IsChinese());

        if (message != null) _page.Status(message);
    }

    /// <summary>
    /// The player can switch the game's language at any time, so it is read once a second and the menu row follows.
    /// The first look waits half a second, so it and the page check (every two seconds from the first frame) never
    /// land on the same frame.
    /// </summary>
    private void KeepLanguage(double now)
    {
        if (_nextLanguageLook == 0) _nextLanguageLook = now + (LanguageLookSeconds / 2);
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
                case PageCommand.PickUp:
                    PickUp();
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
                    _page.ShowKey(KeyName());
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
            if (outcome is UnpackOutcome.Done done) _watcher.Begin(done.Tally.Queued, Clock.Now);
        }
        catch (Exception ex)
        {
            _log("press failed: " + ex);
            _page.Status(Texts.Failed.Pick(GameLanguage.IsChinese()), ToastKind.Error);
        }
    }

    /// <summary>Picks up whatever the click menu is open on right now; a package that left the yard has its menu shut.</summary>
    private void PickUp()
    {
        var isChinese = GameLanguage.IsChinese();

        try
        {
            var openOn = GameMenu.OpenOn();
            var outcome = PickUpRun.Run(_pickUp, openOn);
            var message = PickUpMessage.Describe(outcome, isChinese);

            _log($"pick up on {openOn?.ToString() ?? "nothing"}: {outcome} -> {message}");
            _page.Status(message, PickUpToast(outcome));

            if (outcome is not PickUpOutcome.PickedUp || openOn is not { } id) return;

            _page.ShowPickMenu(null, "", "");
            GameMenu.CloseIfOn(id);
        }
        catch (Exception ex)
        {
            _log("pick up failed: " + ex);
            _page.Status(Texts.PickUpError.Pick(isChinese), ToastKind.Error);
        }
    }

    private static ToastKind PickUpToast(PickUpOutcome outcome) => outcome switch
    {
        PickUpOutcome.PickedUp => ToastKind.Success,
        PickUpOutcome.Failed => ToastKind.Error,
        _ => ToastKind.Warning,
    };

    private string KeyName() => _unpackKey == KeyCode.None ? "" : _unpackKey.ToString();
}
