using System;
using ExiledToolkit;
using ExiledToolkit.Game;
using ExiledToolkit.Kit;
using ExiledToolkit.Web;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's game side, run once a frame. The Mods menu row and the hotkey only ask for a press, and the press itself
/// runs in this frame's update, at most once a frame.
/// </summary>
internal sealed class UnpackSession
{
    private readonly PageMailbox _messages = PageInbox.Listen(PageCommand.Prefix);
    private readonly GamePackages _game;
    private readonly IPickUpGame _pickUp;
    private readonly WebPage _page;
    private KeyCode _unpackKey = KeyCode.None;
    private readonly Action<string> _log;
    private readonly UnpackWatcher _watcher;
    private readonly Func<bool> _isTyping = () => Typing.IsActive;

    private bool _isMenuPressed;

    /// <param name="detail">Logs a line to the mod's own file only.</param>
    public UnpackSession(GamePackages game, IPickUpGame pickUp, WebPage page, Action<string> log, Action<string> detail)
    {
        _game = game;
        _pickUp = pickUp;
        _page = page;
        _log = log;
        _watcher = new UnpackWatcher(game, page, log, detail);
    }

    /// <summary>Takes a new unpack key and names it on the Mods menu row.</summary>
    public void UseKey(KeyCode unpackKey)
    {
        _unpackKey = unpackKey;
        ModsMenu.Add(MenuRow.Entry(KeyName(), MenuPressed));
    }

    /// <summary>The Mods menu row was picked; Exiled's Toolkit calls this on the main thread.</summary>
    private void MenuPressed() => _isMenuPressed = true;

    public void Update()
    {
        var now = Clock.Now;

        _page.KeepInstalled(now);
        _watcher.Tick(now);
        TakePageCommands();

        var isPressed = _isMenuPressed;

        _isMenuPressed = false;

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

    /// <summary>Handles the page's messages.</summary>
    private void TakePageCommands()
    {
        while (_messages.TryTake(out var message))
        {
            var command = PageCommand.Parse(message);

            switch (command)
            {
                case PageCommand.PickUp:
                    PickUp();
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
