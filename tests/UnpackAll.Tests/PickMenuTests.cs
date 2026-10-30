using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

/// <summary>
/// The Pick up entry in the game's click menu: there only while the plugin says the menu is on a yard package,
/// greyed out with the reason when the package can't be picked up, and a click sends one press.
/// </summary>
public class PickMenuTests
{
    private const string Picks = "posted.filter(function (m) { return m === 'slmod|unpackall|pickup'; }).length";

    [Fact]
    public void A_menu_on_a_yard_package_gets_a_working_pick_up_entry_with_the_refund_warning()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu("", Texts.PickUpLabel.En, Texts.PickUpTip.En));

        Assert.True(page.Is("pickEntryIn(menu)"));
        Assert.False(page.Is("pickEntryIn(menu).classList.contains('locked')"));
        Assert.Equal(Texts.PickUpLabel.En, page.Text("pickEntryIn(menu).querySelector('.menu-btn-text').textContent"));
        Assert.Equal(Texts.PickUpTip.En, page.Text("pickEntryIn(menu).querySelector('.menu-btn-tooltip').textContent"));
        Assert.True(page.Is("pickEntryIn(menu).parentNode.classList.contains('menu-col')"));
    }

    [Fact]
    public void A_click_sends_one_press_until_the_plugin_answers()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu("", "Pick up", "tip"));
        page.Run("click(pickEntryIn(menu)); click(pickEntryIn(menu));");

        Assert.Equal("1", page.Text(Picks));

        page.Answered();
        page.Run("click(pickEntryIn(menu));");

        Assert.Equal("2", page.Text(Picks));
    }

    [Fact]
    public void A_package_that_cant_be_picked_up_gets_a_greyed_out_entry_that_sends_nothing()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu(Texts.BeingMoved.En, "Pick up", "tip"));
        page.Run("click(pickEntryIn(menu));");

        Assert.True(page.Is("pickEntryIn(menu).classList.contains('locked')"));
        Assert.Equal(Texts.BeingMoved.En, page.Text("pickEntryIn(menu).querySelector('.menu-btn-tooltip').textContent"));
        Assert.Equal("0", page.Text(Picks));
    }

    [Fact]
    public void A_menu_on_anything_else_has_no_entry_and_a_stale_one_is_removed()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu("", "Pick up", "tip"));
        page.Run(PageCalls.PickMenu(null, "Pick up", "tip"));

        Assert.False(page.Is("pickEntryIn(menu)"));
    }

    [Fact]
    public void An_entry_is_redrawn_when_the_game_redraws_its_menu()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu("", "Pick up", "tip"));
        page.Run("var column = menu.querySelector('.menu-col'); column.children.slice().forEach(function (c) { c.remove(); });");
        page.Run("var redrawn = new FakeElement('div'); redrawn.className = 'menu-btn'; column.appendChild(redrawn); poll();");

        Assert.True(page.Is("pickEntryIn(menu)"));
        Assert.Equal("1", page.Text("menu.querySelectorAll('.slua-pickup').length"));
    }

    [Fact]
    public void A_poll_with_the_entry_in_place_searches_nothing()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu();");
        page.Run(PageCalls.PickMenu("", "Pick up", "tip"));
        page.Run("searches = 0; poll();");

        Assert.Equal("0", page.Text("searches"));
    }

    [Fact]
    public void An_empty_menu_gets_no_entry()
    {
        var page = new PageScript();

        page.Run("var menu = openGameMenu(); menu.querySelectorAll('.menu-btn').forEach(function (b) { b.remove(); });");
        page.Run(PageCalls.PickMenu("", "Pick up", "tip"));

        Assert.False(page.Is("pickEntryIn(menu)"));
    }
}
