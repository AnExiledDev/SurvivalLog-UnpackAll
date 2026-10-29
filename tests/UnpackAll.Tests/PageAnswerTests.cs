using Xunit;

namespace UnpackAll.Tests;

/// <summary>The result of a press is an Exiled's Toolkit toast; the page only holds the menu row until the plugin answers.</summary>
public class PageAnswerTests
{
    private const string Press = "menuEntries['unpackall.unpack'].run()";
    private const string Presses = "posted.filter(function (m) { return m === 'slmod|unpackall|unpack'; }).length";

    [Fact]
    public void A_second_press_before_the_answer_is_not_sent()
    {
        var page = new PageScript();

        page.Run(Press);
        page.Run(Press);

        Assert.Equal("1", page.Text(Presses));
    }

    [Fact]
    public void After_the_answer_the_next_press_is_sent()
    {
        var page = new PageScript();

        page.Run(Press);
        page.Answered();
        page.Run(Press);

        Assert.Equal("2", page.Text(Presses));
    }

    [Fact]
    public void The_page_draws_no_toast_of_its_own()
    {
        var page = new PageScript();

        page.Run(Press);
        page.Answered();

        Assert.False(page.Is("document.getElementById('ua-toast')"));
        Assert.False(page.Is("window.__slunpackall.status"));
        Assert.False(page.Is("window.__exiledUiWait && window.__exiledUiWait.length"));
    }
}
