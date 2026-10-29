using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class PageCallsTests
{
    [Fact]
    public void The_heartbeat_is_a_short_check_that_asks_for_the_script_only_when_it_is_missing()
    {
        Assert.StartsWith("window.__slunpackall||", PageCalls.Probe);
        Assert.EndsWith(PageCalls.NoScriptReply, PageCalls.Probe);
        Assert.True(PageCalls.Probe.Length < 200);
    }

    [Fact]
    public void The_answer_to_a_press_is_a_short_call_that_does_nothing_on_a_page_with_no_script()
    {
        // A page that lost the script lost its waiting press with it, so there is nothing to free and nothing to ask for.
        Assert.StartsWith("window.__slunpackall&&", PageCalls.Answered);
        Assert.DoesNotContain("noscript", PageCalls.Answered);
        Assert.True(PageCalls.Answered.Length < 200);
    }
}
