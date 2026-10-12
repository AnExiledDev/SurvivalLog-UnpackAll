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

    /// <summary>The Unpack all button was clicked.</summary>
    public sealed record Unpack : PageCommand;

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
            ("ready", 1) => new Ready(),
            ("noscript", 1) => new NoScript(),
            _ => new Unknown(message),
        };
    }
}
