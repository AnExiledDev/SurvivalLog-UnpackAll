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

    /// <summary>
    /// Sets the Pick up entry in the game's click menu: no entry when why is null, a working one when it is empty,
    /// otherwise a greyed-out one whose tip gives the reason.
    /// </summary>
    public static string PickMenu(string? why, string label, string tip) =>
        "window.__slunpackall&&window.__slunpackall.pickMenu(" + (why == null ? "null" : JsText.Literal(why)) + "," + JsText.Literal(label) + "," + JsText.Literal(tip) + ")";
}
