namespace UnpackAll.Core;

/// <summary>
/// The scripts the plugin runs in the web UI. The page script itself (about 24 KB) goes only to a page that said it
/// has none; everything else is a short call that asks for the script when the page lost it. A toast that reached a
/// page with no script is kept for a few seconds and goes along with the script the page then asks for, so a page
/// that reloaded just before a press still shows its result.
/// </summary>
public sealed class PageCalls
{
    /// <summary>How long a toast that may have found no script waits to go along with the next install.</summary>
    public const double StatusKeepSeconds = 5;

    public const string NoScriptReply = "window.vuplex&&window.vuplex.postMessage('" + PageCommand.Prefix + "noscript')";

    /// <summary>The heartbeat: nothing when the script is there, a missing-script note when it is not.</summary>
    public const string Probe = "window.__slunpackall||" + NoScriptReply;

    private string? _pendingStatus;
    private double _pendingUntil;

    /// <summary>Shows a toast, or asks for the script when the page has none; the toast then waits for <see cref="Install"/>.</summary>
    public string Status(string text, double now)
    {
        var call = "window.__slunpackall.status(" + JsText.Literal(text) + ")";

        _pendingStatus = call;
        _pendingUntil = now + StatusKeepSeconds;

        return "window.__slunpackall?" + call + ":" + NoScriptReply;
    }

    /// <summary>The page script, followed by the last toast when it is recent enough. The toast goes along once.</summary>
    public string Install(string script, double now)
    {
        var pending = _pendingStatus != null && now <= _pendingUntil ? _pendingStatus : null;

        _pendingStatus = null;

        return pending == null ? script : script + "\n;window.__slunpackall&&" + pending + ";";
    }
}
