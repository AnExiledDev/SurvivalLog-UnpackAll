using System;

namespace UnpackAll.Core;

/// <summary>
/// When the unpack key counts as a press. The inventory's Mods menu has an Unpack all button, so the key starts
/// unbound; an unbound key never fires, and a bound one does nothing while a text box in a mod's page has the
/// keyboard.
/// </summary>
public static class KeyGate
{
    /// <summary>The Unity key name the setting starts with.</summary>
    public const string DefaultKeyName = "None";

    /// <param name="isTyping">Asked only for a press of a bound key, since it calls into the Toolkit.</param>
    public static bool Fires(bool isBound, bool isPressed, Func<bool> isTyping) => isBound && isPressed && !isTyping();
}
