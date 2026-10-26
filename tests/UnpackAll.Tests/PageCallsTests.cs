using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class PageCallsTests
{
    private const string Script = "window.__slunpackall={status:function(){}};/* the whole page script */";

    [Fact]
    public void A_toast_is_a_short_call_and_never_carries_the_page_script()
    {
        // Sending the 24 KB script with every toast is what made a press stutter.
        var call = new PageCalls().Status("3 packages are unpacking", now: 10);

        Assert.DoesNotContain("the whole page script", call);
        Assert.Contains("window.__slunpackall.status(", call);
        Assert.Contains("3 packages are unpacking", call);
        Assert.Contains(PageCalls.NoScriptReply, call);
    }

    [Fact]
    public void A_page_that_had_no_script_for_a_toast_gets_the_toast_with_the_script()
    {
        var calls = new PageCalls();

        calls.Status("3 packages are unpacking", now: 10);
        var install = calls.Install(Script, now: 10.5);

        Assert.StartsWith(Script, install);
        Assert.Contains("3 packages are unpacking", install);
    }

    [Fact]
    public void A_toast_goes_along_with_one_install_only()
    {
        var calls = new PageCalls();

        calls.Status("3 packages are unpacking", now: 10);
        calls.Install(Script, now: 10.5);

        Assert.Equal(Script, calls.Install(Script, now: 11));
    }

    [Fact]
    public void An_old_toast_is_not_shown_by_a_later_install()
    {
        var calls = new PageCalls();

        calls.Status("3 packages are unpacking", now: 10);

        Assert.Equal(Script, calls.Install(Script, now: 10 + PageCalls.StatusKeepSeconds + 0.1));
    }

    [Fact]
    public void An_install_with_no_toast_waiting_is_the_script_alone()
    {
        Assert.Equal(Script, new PageCalls().Install(Script, now: 1));
    }

    [Fact]
    public void The_heartbeat_is_a_short_check_that_asks_for_the_script_only_when_it_is_missing()
    {
        Assert.StartsWith("window.__slunpackall||", PageCalls.Probe);
        Assert.EndsWith(PageCalls.NoScriptReply, PageCalls.Probe);
        Assert.True(PageCalls.Probe.Length < 200);
    }
}
