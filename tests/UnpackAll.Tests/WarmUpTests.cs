using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class WarmUpTests
{
    [Fact]
    public void The_warm_up_press_walks_the_bag_the_floor_the_storage_and_the_use_check()
    {
        // The warm-up exists to compile every step of a press before the player's first one; a press that stopped
        // early would leave the later steps to compile on the game's frame.
        var outcome = Assert.IsType<UnpackOutcome.Done>(WarmUp.Run());

        Assert.Equal(3, outcome.Tally.Queued);
        Assert.Equal(1, outcome.Tally.PickedUp);
        Assert.Equal(1, outcome.Tally.FromStorage);
        Assert.Equal(1, outcome.Tally.Unusable);
    }
}
