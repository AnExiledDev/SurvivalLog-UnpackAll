using System;
using System.IO;
using ExiledToolkit;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using UnpackAll.Core;
using Vuplex.WebView;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// puts Unpack all in the inventory toolbar's Mods menu. The result of a press is an Exiled's Toolkit toast. The web view
/// can reload at any time and lose the script, so it is checked for every couple of seconds and put back when the page
/// says it is missing. The script itself goes only to a page that said so (see <see cref="PageCalls"/>).
/// </summary>
internal sealed class WebPage
{
    private const double CheckSeconds = 2;
    private const string ScriptResource = "UnpackAll.page.js";

    private readonly Action<string> _log;
    private string? _script;
    private double _nextCheckAt;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log)
    {
        _log = log;
    }

    public void KeepInstalled(double now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;
        Run(PageCalls.Probe);
    }

    /// <summary>Sends the script to a page that said it has none.</summary>
    public void Install()
    {
        if (Run(Script())) _log("page script sent to the web UI");
    }

    /// <summary>The result of a press, as an Exiled's Toolkit toast; the menu row is then free for the next press.</summary>
    public void Status(string text, ToastKind kind = ToastKind.Info)
    {
        if (Toasts.Show(new ToastRequest(Texts.Source.Pick(GameLanguage.IsChinese()), text) { Kind = kind }).IsNone) _log("toast not shown: " + text);

        Run(PageCalls.Answered);
    }

    /// <summary>The unpack key's name for the Mods menu row; empty when the key is off.</summary>
    public void ShowKey(string name) => Run("window.__slunpackall&&window.__slunpackall.setKey(" + JsText.Literal(name) + ")");

    /// <summary>Names the menu row and its tooltip in the game's language.</summary>
    public void ShowMenuText(string label, string title) =>
        Run("window.__slunpackall&&window.__slunpackall.setText(" + JsText.Literal(label) + "," + JsText.Literal(title) + ")");

    /// <summary>Closes the inventory's Mods menu, which any mod carrying the slmod-menu block in its page.js may have made.</summary>
    public void CloseMenu() => Run("window.__slmodmenu&&window.__slmodmenu.close()");

    private bool Run(string javaScript)
    {
        var webView = GameSingleton.Existing<ReduxUISystem>()?.GetWebUILayer()?.canvasWebViewPrefab?.WebView;

        if (webView == null) return false;

        try
        {
            webView.ExecuteJavaScript(javaScript, null);

            return true;
        }
        catch (Exception ex)
        {
            if (!_hasReportedRunFailure) _log("could not run script in the web UI (reported once): " + ex.Message);

            _hasReportedRunFailure = true;

            return false;
        }
    }

    private string Script()
    {
        if (_script != null) return _script;

        using var stream = typeof(WebPage).Assembly.GetManifestResourceStream(ScriptResource) ?? throw new InvalidOperationException(ScriptResource + " is not embedded");
        using var reader = new StreamReader(stream);

        return _script = reader.ReadToEnd();
    }
}
