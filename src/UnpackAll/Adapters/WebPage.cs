using System;
using ExiledToolkit;
using ExiledToolkit.Game;
using UnpackAll.Core;
using ToolkitPage = ExiledToolkit.Web.WebPage;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's pages sit in. It
/// puts Pick up in the game's click menu; Unpack all in the inventory toolbar's Mods menu comes from Exiled's Toolkit.
/// The result of a press is an Exiled's Toolkit toast. The web view can reload at any time and lose the script, so
/// Exiled's Toolkit checks for it every couple of seconds and the session puts it back when the page says it is
/// missing. The script itself goes only to a page that said so (see <see cref="PageCalls"/>).
/// </summary>
internal sealed class WebPage
{
    private const string ScriptResource = "UnpackAll.page.js";
    private const string Global = "window.__slunpackall";

    private readonly ToolkitPage _page;
    private readonly Action<string> _log;

    public WebPage(Action<string> log)
    {
        _log = log;
        _page = new ToolkitPage(typeof(WebPage).Assembly, ScriptResource, Global, PageCommand.Prefix + "noscript", log);
    }

    public void KeepInstalled(double now) => _page.KeepInstalled(now);

    /// <summary>Sends the script to a page that said it has none.</summary>
    public void Install() => _page.Install();

    /// <summary>The result of a press, as an Exiled's Toolkit toast; Pick up is then free for the next press.</summary>
    public void Status(string text, ToastKind kind = ToastKind.Info)
    {
        if (Toasts.Show(new ToastRequest(Texts.Source.Pick(GameLanguage.IsChinese()), text) { Kind = kind }).IsNone) _log("toast not shown: " + text);

        _page.Run(PageCalls.Answered);
    }

    /// <summary>The Pick up entry in the game's click menu: none when why is null, working when empty, else greyed out with the reason.</summary>
    public void ShowPickMenu(string? why, string label, string tip) => _page.Run(PageCalls.PickMenu(why, label, tip));
}
