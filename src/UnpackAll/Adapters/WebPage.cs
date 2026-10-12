using System;
using System.IO;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using UnpackAll.Core;
using Vuplex.WebView;

namespace UnpackAll.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// adds the Unpack all button to the inventory toolbar and shows the result toast. The web view can reload at any time
/// and lose the script, so it is checked for every couple of seconds and put back when the page says it is missing.
/// </summary>
internal sealed class WebPage
{
    private const float CheckSeconds = 2f;
    private const string ScriptResource = "UnpackAll.page.js";
    private const string NoScriptReply = "window.vuplex&&window.vuplex.postMessage('" + PageCommand.Prefix + "noscript')";

    private readonly Action<string> _log;
    private string? _script;
    private float _nextCheckAt;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log)
    {
        _log = log;
    }

    public void KeepInstalled(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;
        Run("window.__slunpackall||" + NoScriptReply);
    }

    public void Install()
    {
        if (Run(Script())) _log("page script sent to the web UI");
    }

    /// <summary>
    /// The result of a press, as a toast over the game. The script travels with it, so a page that reloaded since the
    /// last check still shows the toast, and so does one whose missing-script note never reached the mod. The script's
    /// own guard makes the copy a no-op when it is already there.
    /// </summary>
    public void Status(string text) => Run(Script() + "\n;window.__slunpackall&&window.__slunpackall.status(" + JsText.Literal(text) + ");");

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
