using Xunit;

namespace UnpackAll.Tests;

/// <summary>The toast after a press is drawn at the player's UI size from Exiled's Toolkit, and never shrunk to fit.</summary>
public class ToastUiSizeTests
{
    // Stands in for Exiled's Toolkit's window.ExiledUi and keeps what each call handed it.
    private const string FakeUi = "var handed = []; window.ExiledUi = { size: function (el, options) { handed.push({ el: el, options: options }); } }";

    [Fact]
    public void The_toast_is_handed_to_the_toolkit_once_and_never_shrunk()
    {
        var page = new PageScript();
        page.Run(FakeUi);

        page.Status("3 packages unpacked");
        page.Status("Nothing to unpack");

        // The second message reuses the toast it built, so the Toolkit is not handed it twice.
        Assert.Equal("1", page.Text("handed.length"));
        Assert.Equal("1", page.Text("handed.filter(function (h) { return h.options.mod === 'UnpackAll'; }).length"));
        Assert.True(page.Is("handed[0].el === document.getElementById('ua-toast')"));
        Assert.True(page.Is("handed[0].options.fit === false"));
    }

    [Fact]
    public void A_toast_the_page_lost_is_handed_again_when_it_is_built_anew()
    {
        var page = new PageScript();
        page.Run(FakeUi);
        page.Status("3 packages unpacked");

        page.Run("document.getElementById('ua-toast').remove()");
        page.Status("Nothing to unpack");

        Assert.Equal("2", page.Text("handed.length"));
        Assert.True(page.Is("handed[1].el === document.getElementById('ua-toast')"));
        Assert.True(page.Is("handed[1].options.fit === false"));
    }

    [Fact]
    public void Before_the_toolkit_has_loaded_the_toast_waits_in_the_list_it_reads()
    {
        var page = new PageScript();

        page.Status("3 packages unpacked");

        Assert.Equal("1", page.Text("window.__exiledUiWait.length"));
        Assert.True(page.Is("window.__exiledUiWait[0][0] === document.getElementById('ua-toast')"));
        Assert.True(page.Is("window.__exiledUiWait[0][1].fit === false"));
    }
}
