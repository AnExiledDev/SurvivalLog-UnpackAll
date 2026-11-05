using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

/// <summary>The inventory's Mods menu has an Unpack all button, so the key starts unbound and its description says so.</summary>
public class KeyGateTests
{
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
