using System;

namespace UnpackAll.Core;

/// <summary>
/// What the inventory button's script asks for. It sends "slmod|unpackall|" followed by a verb; anything else parses
/// to Unknown so it can be logged rather than acted on (the script's "diag|..." notes land there on purpose).
/// </summary>
public abstract record PageCommand
{
    public const string Prefix = "slmod|unpackall|";

    private PageCommand()
    {
    }

    /// <summary>The Unpack all row of the inventory's Mods menu was clicked.</summary>
    public sealed record Unpack : PageCommand;

    /// <summary>The inventory toolbar's Mods menu opened or closed; the page gets no keys, so Esc has to close it from here.</summary>
    public sealed record Menu(bool IsOpen) : PageCommand;

    /// <summary>Pick up was clicked in the game's click menu; it acts on whatever the menu is open on at that moment.</summary>
    public sealed record PickUp : PageCommand;

    /// <summary>The page script just installed itself.</summary>
    public sealed record Ready : PageCommand;

    /// <summary>The page has no script, usually because the web view reloaded.</summary>
    public sealed record NoScript : PageCommand;

    public sealed record Unknown(string Text) : PageCommand;

    public static bool IsOurs(string? message) => message is not null && message.StartsWith(Prefix, StringComparison.Ordinal);

    /// <summary>Parses a whole message, prefix included.</summary>
    public static PageCommand Parse(string message)
    {
        if (!IsOurs(message)) return new Unknown(message);

        var parts = message.Substring(Prefix.Length).Split('|');

        return (parts[0], parts.Length) switch
        {
            ("unpack", 1) => new Unpack(),
            ("menu", 2) when parts[1] is "0" or "1" => new Menu(parts[1] == "1"),
            ("pickup", 1) => new PickUp(),
            ("ready", 1) => new Ready(),
            ("noscript", 1) => new NoScript(),
            _ => new Unknown(message),
        };
    }
}
