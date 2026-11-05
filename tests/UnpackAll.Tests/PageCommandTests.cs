using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class PageCommandTests
{
    [Fact]
    public void The_script_messages_parse_to_their_commands()
    {
        Assert.Equal(new PageCommand.Ready(), PageCommand.Parse("slmod|unpackall|ready"));
        Assert.Equal(new PageCommand.NoScript(), PageCommand.Parse("slmod|unpackall|noscript"));
        Assert.Equal(new PageCommand.PickUp(), PageCommand.Parse("slmod|unpackall|pickup"));
    }

    [Theory]
    [InlineData("slmod|unpackall|pickup|now")]
    [InlineData("slmod|unpackall|unpack")]
    [InlineData("slmod|unpackall|menu|1")]
    [InlineData("slmod|unpackall|explode")]
    [InlineData("slmod|unpackall|diag|cannot read the inventory tab state")]
    [InlineData("slmod|unpackall|")]
    public void A_malformed_or_unknown_message_is_never_acted_on(string message)
    {
        Assert.Equal(new PageCommand.Unknown(message), PageCommand.Parse(message));
    }

    [Theory]
    [InlineData("slmod|storagehub|putAwayBag")]
    [InlineData("slmod|unpackal|unpack")]
    [InlineData("")]
    [InlineData(null)]
    public void Only_messages_with_our_prefix_are_claimed_so_other_mods_and_the_game_keep_theirs(string? message)
    {
        Assert.False(PageCommand.IsOurs(message));
    }
}
