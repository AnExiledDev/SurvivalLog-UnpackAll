using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

/// <summary>
/// The inventory's Mods menu has an Unpack all button, so the key starts unbound, an unbound key never unpacks, and a
/// bound key never unpacks while the player is typing in a mod's text box.
/// </summary>
public class KeyGateTests
{
    [Fact]
    public void The_key_starts_unbound()
    {
        Assert.Equal("None", KeyGate.DefaultKeyName);
    }

    [Fact]
    public void An_unbound_key_never_fires()
    {
        var asked = false;

        Assert.False(KeyGate.Fires(isBound: false, isPressed: true, () => asked = true));
        Assert.False(asked);
    }

    [Fact]
    public void A_bound_key_fires_when_pressed()
    {
        Assert.True(KeyGate.Fires(isBound: true, isPressed: true, () => false));
        Assert.False(KeyGate.Fires(isBound: true, isPressed: false, () => false));
    }

    [Fact]
    public void A_bound_key_does_not_fire_while_typing()
    {
        Assert.False(KeyGate.Fires(isBound: true, isPressed: true, () => true));
    }

    [Fact]
    public void The_key_description_says_unbound_by_default_in_both_languages()
    {
        var parts = Settings.UnpackKey.Split('\n');

        Assert.Equal(2, parts.Length);
        Assert.Contains("Unbound by default", parts[0]);
        Assert.DoesNotContain("F9", Settings.UnpackKey);
        Assert.Contains("默认不绑定", parts[1]);
    }
}
