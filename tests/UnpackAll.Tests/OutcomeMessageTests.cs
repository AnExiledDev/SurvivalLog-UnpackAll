using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class OutcomeMessageTests
{
    [Fact]
    public void A_press_reads_as_what_started_then_what_was_left_and_why()
    {
        var outcome = Done(queued: 3, pickedUp: 1, noSpot: 2);

        Assert.Equal("Unpacking 3 packages. Picked up 1 from the floor. 2 left: no free package spot at home.", OutcomeMessage.Describe(outcome));
    }

    [Fact]
    public void One_package_reads_in_the_singular()
    {
        Assert.Equal("Unpacking 1 package.", OutcomeMessage.Describe(Done(queued: 1)));
        Assert.Equal("Nothing unpacked. 1 left on the floor: your bag could not carry it.", OutcomeMessage.Describe(Done(floorRefused: 1)));
        Assert.Equal("Nothing unpacked. 1 on the floor at home: go home to pick it up.", OutcomeMessage.Describe(Done(floorAway: 1)));
    }

    [Fact]
    public void Every_reason_a_package_was_left_is_counted()
    {
        var outcome = Done(noSpot: 1, floorRefused: 2, floorAway: 3, unusable: 4, unusableReason: "Not while fighting");

        Assert.Equal(
            "Nothing unpacked. 1 left: no free package spot at home. 2 left on the floor: your bag could not carry them. 3 on the floor at home: go home to pick them up. 4 can't be unpacked right now: Not while fighting.",
            OutcomeMessage.Describe(outcome));
    }

    [Theory]
    [InlineData("Not while fighting.")]
    [InlineData("Not while fighting!  ")]
    [InlineData("Not while fighting...")]
    public void The_game_reason_keeps_its_words_but_not_its_closing_mark(string reason)
    {
        Assert.Equal("Nothing unpacked. 1 can't be unpacked right now: Not while fighting.", OutcomeMessage.Describe(Done(unusable: 1, unusableReason: reason)));
    }

    [Fact]
    public void A_chinese_reason_loses_its_full_stop_so_the_line_does_not_end_twice()
    {
        Assert.Equal("Nothing unpacked. 1 can't be unpacked right now: \u5F53\u524D\u65E0\u6CD5\u4F7F\u7528.", OutcomeMessage.Describe(Done(unusable: 1, unusableReason: "\u5F53\u524D\u65E0\u6CD5\u4F7F\u7528\u3002")));
    }

    [Fact]
    public void A_refusal_without_a_reason_still_says_how_many()
    {
        Assert.Equal("Nothing unpacked. 2 can't be unpacked right now.", OutcomeMessage.Describe(Done(unusable: 2, unusableReason: "  ")));
    }

    [Fact]
    public void A_busy_press_says_to_wait()
    {
        Assert.Equal("Already unpacking 2 packages. Press again once they are done.", OutcomeMessage.Describe(new UnpackOutcome.AlreadyUnpacking(2)));
        Assert.Equal("Already unpacking 1 package. Press again once it is done.", OutcomeMessage.Describe(new UnpackOutcome.AlreadyUnpacking(1)));
    }

    [Fact]
    public void Presses_that_did_nothing_say_why()
    {
        Assert.Equal("Can't unpack right now: load a save first.", OutcomeMessage.Describe(new UnpackOutcome.NotReady("load a save first")));
        Assert.Equal("No packages in your bag or on the floor at home.", OutcomeMessage.Describe(new UnpackOutcome.NothingToUnpack()));
        Assert.Equal("No packages in your bag, on the floor or in storages at home.", OutcomeMessage.Describe(new UnpackOutcome.NothingToUnpack(HasLookedInStorages: true)));
        Assert.Equal("Could not start unpacking, see BepInEx/unpack-all.log.", OutcomeMessage.Describe(new UnpackOutcome.StartFailed(2, 0)));
        Assert.Equal("Finish or cancel your queued actions first.", OutcomeMessage.Describe(new UnpackOutcome.QueueBusy()));
    }

    [Fact]
    public void A_press_outside_a_loaded_game_says_nothing()
    {
        Assert.Null(OutcomeMessage.Describe(new UnpackOutcome.NoGame()));
    }

    [Fact]
    public void Picked_up_packages_that_were_not_unpacked_are_said_to_be_waiting_in_the_bag()
    {
        var outcome = Done(queued: 1, pickedUp: 5, waitingInBag: 4, noSpot: 2);

        Assert.Equal("Unpacking 1 package. Picked up 5 from the floor, 4 waiting in your bag. 2 left: no free package spot at home.", OutcomeMessage.Describe(outcome));
    }

    [Fact]
    public void A_start_that_failed_after_a_pickup_says_where_the_picked_up_packages_are()
    {
        Assert.Equal(
            "Could not start unpacking, see BepInEx/unpack-all.log. Picked up 2 from the floor, 2 waiting in your bag.",
            OutcomeMessage.Describe(new UnpackOutcome.StartFailed(3, 2)));
    }

    [Fact]
    public void Packages_taken_from_storages_say_where_they_came_from_and_how_many_wait_in_the_bag()
    {
        Assert.Equal("Unpacking 2 packages. Took 2 from storages.", OutcomeMessage.Describe(Done(queued: 2, fromStorage: 2)));
        Assert.Equal("Unpacking 1 package. Took 3 from storages, 2 waiting in your bag.", OutcomeMessage.Describe(Done(queued: 1, fromStorage: 3, waitingInBag: 2)));
        Assert.Equal(
            "Unpacking 2 packages. Picked up 1 from the floor and took 2 from storages, 1 waiting in your bag.",
            OutcomeMessage.Describe(Done(queued: 2, pickedUp: 1, fromStorage: 2, waitingInBag: 1)));
    }

    [Fact]
    public void Every_reason_a_package_stayed_in_a_storage_is_counted()
    {
        var outcome = Done(storageRefused: 1, storageAway: 2, storageBehindPanel: 3, storageUnchecked: 4, storageTooBig: 5);

        Assert.Equal(
            "Nothing unpacked. 1 left in storages: your bag could not carry it. 2 in storages at home: go home to unpack them. 3 left in storages: close the open window and press again. "
            + "4 left in storages: could not check for open windows, see BepInEx/unpack-all.log. 5 left in storages: a stack is bigger than the free package spots.",
            OutcomeMessage.Describe(outcome));
    }

    [Fact]
    public void A_start_that_failed_after_moves_from_storages_says_the_packages_are_in_the_bag()
    {
        Assert.Equal(
            "Could not start unpacking, see BepInEx/unpack-all.log. Took 1 from storages, 1 waiting in your bag.",
            OutcomeMessage.Describe(new UnpackOutcome.StartFailed(1, 0, 1)));
        Assert.Equal(
            "Could not start unpacking, see BepInEx/unpack-all.log. Picked up 1 from the floor and took 2 from storages, 3 waiting in your bag.",
            OutcomeMessage.Describe(new UnpackOutcome.StartFailed(3, 1, 2)));
    }

    [Theory]
    [InlineData("<color=#ff0000>Not while fighting</color>.", "Not while fighting")]
    [InlineData("<b>Not</b> while <i>fighting</i>", "Not while fighting")]
    [InlineData("<#FF5555>Not while fighting</color>!", "Not while fighting")]
    [InlineData("<color=#FF5555>\u4E0D\u80FD\u5728\u6218\u6597\u4E2D\u4F7F\u7528</color>\u3002", "\u4E0D\u80FD\u5728\u6218\u6597\u4E2D\u4F7F\u7528")]
    public void The_game_reason_loses_the_rich_text_tags_its_own_ui_would_render(string reason, string shown)
    {
        Assert.Equal($"Nothing unpacked. 1 can't be unpacked right now: {shown}.", OutcomeMessage.Describe(Done(unusable: 1, unusableReason: reason)));
    }

    [Fact]
    public void A_not_ready_reason_loses_its_rich_text_tags_too()
    {
        Assert.Equal("Can't unpack right now: your home is not loaded.", OutcomeMessage.Describe(new UnpackOutcome.NotReady("<color=red>your home is not loaded</color>")));
    }

    [Fact]
    public void Unpacks_the_game_dropped_are_never_reported_as_unpacking()
    {
        Assert.Equal("Nothing unpacked. 1 dropped by the game before it started, see BepInEx/unpack-all.log.", OutcomeMessage.Describe(Done(dropped: 1)));
        Assert.Equal("Unpacking 1 package. 2 dropped by the game before they started, see BepInEx/unpack-all.log.", OutcomeMessage.Describe(Done(queued: 1, dropped: 2)));
    }

    [Fact]
    public void A_watch_where_every_unpack_used_its_package_says_nothing()
    {
        Assert.Null(OutcomeMessage.Describe(new WatchStep.Finished(Started: 2, Consumed: 2)));
    }

    [Fact]
    public void A_watch_where_unpacks_ended_without_their_package_says_how_many()
    {
        Assert.Equal("1 unpack ended without opening its package, see BepInEx/unpack-all.log.", OutcomeMessage.Describe(new WatchStep.Finished(Started: 1, Consumed: 0)));
        Assert.Equal("2 of 3 unpacks ended without opening their packages, see BepInEx/unpack-all.log.", OutcomeMessage.Describe(new WatchStep.Finished(Started: 3, Consumed: 1)));
    }

    [Fact]
    public void A_chinese_game_gets_the_whole_toast_in_chinese()
    {
        var outcome = Done(queued: 3, pickedUp: 1, waitingInBag: 1, noSpot: 2, unusable: 1, unusableReason: "战斗中无法使用。");

        Assert.Equal(
            "正在拆封 3 个包裹。从地上捡起 1 个，1 个在背包里等待拆封。2 个未拆：家里没有空余的包裹位。1 个现在无法拆封：战斗中无法使用。",
            OutcomeMessage.Describe(outcome, isChinese: true));
    }

    [Fact]
    public void Every_chinese_storage_line_is_counted()
    {
        var outcome = Done(fromStorage: 2, storageRefused: 1, storageAway: 3, storageBehindPanel: 4, storageUnchecked: 5, storageTooBig: 6);

        Assert.Equal(
            "没有拆封任何包裹。从储物家具取出 2 个。1 个留在储物家具中：背包装不下。3 个在家里的储物家具中：回家后才能拆封。4 个留在储物家具中：请关闭打开的界面后再按一次。5 个留在储物家具中：无法检查是否有打开的界面，详见 BepInEx/unpack-all.log。6 个留在储物家具中：一组包裹超过了空余的包裹位。",
            OutcomeMessage.Describe(outcome, isChinese: true));
    }

    [Fact]
    public void Chinese_presses_that_did_nothing_say_why_in_chinese()
    {
        Assert.Equal("现在无法拆封：先读取存档。", OutcomeMessage.Describe(new UnpackOutcome.NotReady("先读取存档。"), isChinese: true));
        Assert.Equal("正在拆封 2 个包裹，拆完后再按一次。", OutcomeMessage.Describe(new UnpackOutcome.AlreadyUnpacking(2), isChinese: true));
        Assert.Equal("背包和家里地上都没有包裹。", OutcomeMessage.Describe(new UnpackOutcome.NothingToUnpack(), isChinese: true));
        Assert.Equal("背包、家里地上和储物家具中都没有包裹。", OutcomeMessage.Describe(new UnpackOutcome.NothingToUnpack(HasLookedInStorages: true), isChinese: true));
        Assert.Equal("请先完成或取消排队中的行动。", OutcomeMessage.Describe(new UnpackOutcome.QueueBusy(), isChinese: true));
        Assert.Equal(
            "无法开始拆封，详见 BepInEx/unpack-all.log。从地上捡起 1 个，并从储物家具取出 2 个，3 个在背包里等待拆封。",
            OutcomeMessage.Describe(new UnpackOutcome.StartFailed(3, 1, 2), isChinese: true));
    }

    [Fact]
    public void A_chinese_watch_says_how_many_unpacks_left_their_package_unopened()
    {
        Assert.Null(OutcomeMessage.Describe(new WatchStep.Finished(Started: 2, Consumed: 2), isChinese: true));
        Assert.Equal("1 次拆封结束时没有打开包裹，详见 BepInEx/unpack-all.log。", OutcomeMessage.Describe(new WatchStep.Finished(Started: 1, Consumed: 0), isChinese: true));
        Assert.Equal("3 次拆封中有 2 次结束时没有打开包裹，详见 BepInEx/unpack-all.log。", OutcomeMessage.Describe(new WatchStep.Finished(Started: 3, Consumed: 1), isChinese: true));
    }

    private static UnpackOutcome Done(
        int queued = 0,
        int pickedUp = 0,
        int waitingInBag = 0,
        int noSpot = 0,
        int floorRefused = 0,
        int floorAway = 0,
        int unusable = 0,
        string? unusableReason = null,
        int dropped = 0,
        int fromStorage = 0,
        int storageRefused = 0,
        int storageAway = 0,
        int storageBehindPanel = 0,
        int storageUnchecked = 0,
        int storageTooBig = 0) =>
        new UnpackOutcome.Done(
            new UnpackTally(
                queued,
                pickedUp,
                waitingInBag,
                noSpot,
                floorRefused,
                floorAway,
                unusable,
                unusableReason,
                dropped,
                fromStorage,
                storageRefused,
                storageAway,
                storageBehindPanel,
                storageUnchecked,
                storageTooBig));
}
