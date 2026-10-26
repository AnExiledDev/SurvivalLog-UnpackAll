using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class UnpackWatchTests
{
    [Fact]
    public void Nothing_is_said_while_the_started_unpacks_wait_untouched_in_the_queue()
    {
        var watch = new UnpackWatch(started: 2, bagUnits: 2);

        Assert.Equal(new WatchStep.Quiet(), watch.Observe(pending: 2, bagUnits: 2, now: 1));
        Assert.Equal(new WatchStep.Quiet(), watch.Observe(pending: 2, bagUnits: 2, now: 5));
    }

    [Fact]
    public void Each_unpack_that_leaves_the_queue_is_reported_with_the_packages_used_so_far()
    {
        var watch = new UnpackWatch(started: 2, bagUnits: 2);

        Assert.Equal(new WatchStep.Changed(Pending: 1, Consumed: 1), watch.Observe(pending: 1, bagUnits: 1, now: 1));
    }

    [Fact]
    public void An_unpack_that_left_the_queue_and_used_its_package_finishes_clean()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 0, now: 1);

        Assert.Equal(new WatchStep.Finished(Started: 1, Consumed: 1), watch.Observe(pending: 0, bagUnits: 0, now: 1 + UnpackWatch.SettleSeconds));
        Assert.True(watch.IsFinished);
    }

    [Fact]
    public void An_unpack_that_left_the_queue_with_its_package_still_in_the_bag_finishes_as_unused()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        Assert.Equal(new WatchStep.Changed(Pending: 0, Consumed: 0), watch.Observe(pending: 0, bagUnits: 1, now: 1));
        Assert.Equal(new WatchStep.Finished(Started: 1, Consumed: 0), watch.Observe(pending: 0, bagUnits: 1, now: 1 + UnpackWatch.SettleSeconds));
    }

    [Fact]
    public void A_package_used_up_just_after_its_unpack_left_the_queue_still_counts_as_used()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 1, now: 1);

        Assert.Equal(new WatchStep.Quiet(), watch.Observe(pending: 0, bagUnits: 1, now: 1 + (UnpackWatch.SettleSeconds / 2)));
        Assert.Equal(new WatchStep.Changed(Pending: 0, Consumed: 1), watch.Observe(pending: 0, bagUnits: 0, now: 1 + (UnpackWatch.SettleSeconds * 0.75)));
        Assert.Equal(new WatchStep.Finished(Started: 1, Consumed: 1), watch.Observe(pending: 0, bagUnits: 0, now: 1 + UnpackWatch.SettleSeconds));
    }

    [Fact]
    public void A_use_that_comes_back_into_the_queue_holds_off_the_finish()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 1, now: 1);
        watch.Observe(pending: 1, bagUnits: 1, now: 2);

        Assert.Equal(new WatchStep.Quiet(), watch.Observe(pending: 1, bagUnits: 1, now: 1 + (UnpackWatch.SettleSeconds * 3)));
        Assert.False(watch.IsFinished);
    }

    [Fact]
    public void Packages_added_to_the_bag_while_watching_never_count_as_used_below_zero()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 3, now: 1);

        Assert.Equal(new WatchStep.Finished(Started: 1, Consumed: 0), watch.Observe(pending: 0, bagUnits: 3, now: 1 + UnpackWatch.SettleSeconds));
    }

    [Fact]
    public void A_finished_watch_stays_quiet()
    {
        var watch = new UnpackWatch(started: 1, bagUnits: 1);

        watch.Observe(pending: 0, bagUnits: 0, now: 1);
        watch.Observe(pending: 0, bagUnits: 0, now: 1 + UnpackWatch.SettleSeconds);

        Assert.Equal(new WatchStep.Quiet(), watch.Observe(pending: 0, bagUnits: 0, now: 10));
    }

    [Fact]
    public void A_look_that_finds_nothing_changed_makes_no_new_step()
    {
        // The watcher looks twice a second for as long as unpacks run; a quiet look should cost no allocation.
        var watch = new UnpackWatch(started: 2, bagUnits: 2);

        var first = watch.Observe(pending: 2, bagUnits: 2, now: 1);

        Assert.Same(first, watch.Observe(pending: 2, bagUnits: 2, now: 1.5));
    }
}
