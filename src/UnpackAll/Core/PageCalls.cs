namespace UnpackAll.Core;

/// <summary>
/// The scripts the plugin runs in the web UI. The page script itself (about 24 KB) goes only to a page that said it
/// has none; everything else is a short call that asks for the script when the page lost it.
/// </summary>
public static class PageCalls
{
    public const string NoScriptReply = "window.vuplex&&window.vuplex.postMessage('" + PageCommand.Prefix + "noscript')";

    /// <summary>The heartbeat: nothing when the script is there, a missing-script note when it is not.</summary>
    public const string Probe = "window.__slunpackall||" + NoScriptReply;

    /// <summary>Tells the page a press was answered, so its menu row takes the next press. A page with no script has no press waiting.</summary>
    public const string Answered = "window.__slunpackall&&window.__slunpackall.answered()";
}
