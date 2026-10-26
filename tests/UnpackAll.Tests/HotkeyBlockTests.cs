using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class HotkeyBlockTests
{
    [Fact]
    public void Nothing_is_held_before_the_menu_ever_opens()
    {
        // The hook runs every frame; with nothing held it must return without reading the frame.
        Assert.False(new HotkeyBlock().IsHeld);
    }

    [Fact]
    public void The_game_hotkeys_are_held_while_the_mods_menu_is_open()
    {
        var block = new HotkeyBlock();

        block.MenuOpened();

        Assert.True(block.IsHeld);
        Assert.True(block.IsBlocked(5));
        Assert.True(block.IsBlocked(500));
    }

    [Fact]
    public void Closing_the_menu_holds_the_hotkeys_through_the_next_frame_then_lets_go()
    {
        // The Esc that closed the menu must not also close the inventory on the next frame.
        var block = new HotkeyBlock();

        block.MenuOpened();
        block.MenuClosed(frame: 10);

        Assert.True(block.IsBlocked(10));
        Assert.True(block.IsBlocked(11));
        Assert.False(block.IsBlocked(12));
        Assert.False(block.IsHeld);
    }

    [Fact]
    public void A_close_for_a_menu_that_was_not_open_holds_nothing()
    {
        // The page and the reload path both report a close; only a real one holds the next frame.
        var block = new HotkeyBlock();

        block.MenuClosed(frame: 10);

        Assert.False(block.IsHeld);
        Assert.False(block.IsBlocked(11));
    }

    [Fact]
    public void A_menu_opened_again_after_a_close_holds_the_hotkeys_again()
    {
        var block = new HotkeyBlock();

        block.MenuOpened();
        block.MenuClosed(frame: 10);
        block.IsBlocked(20);
        block.MenuOpened();

        Assert.True(block.IsHeld);
        Assert.True(block.IsBlocked(21));
    }
}
