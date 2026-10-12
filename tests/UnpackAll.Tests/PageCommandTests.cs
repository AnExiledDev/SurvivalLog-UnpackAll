using System.Text.Json;
using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class PageCommandTests
{
    [Fact]
    public void The_button_and_the_script_messages_parse_to_their_commands()
    {
        Assert.Equal(new PageCommand.Unpack(), PageCommand.Parse("slmod|unpackall|unpack"));
        Assert.Equal(new PageCommand.Ready(), PageCommand.Parse("slmod|unpackall|ready"));
        Assert.Equal(new PageCommand.NoScript(), PageCommand.Parse("slmod|unpackall|noscript"));
    }

    [Theory]
    [InlineData("slmod|unpackall|unpack|now")]
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

    [Theory]
    [InlineData("Unpacking 3 packages.")]
    [InlineData("say \"hi\" \\ bye")]
    [InlineData("line\nbreak\r\tand \u2028 \u2029")]
    [InlineData("</script><script>alert(1)</script>")]
    [InlineData("当前无法使用。")]
    [InlineData("")]
    public void Any_text_reaches_the_page_unchanged_and_cannot_break_out_of_its_string(string text)
    {
        var literal = JsText.Literal(text);

        Assert.Equal(text, JsonSerializer.Deserialize<string>(literal));
        Assert.DoesNotContain("<", literal);
    }
}
