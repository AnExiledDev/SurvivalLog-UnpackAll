using UnpackAll.Core;
using Xunit;
using static UnpackAll.Tests.FakeGame;

namespace UnpackAll.Tests;

public class UnpackRunTests
{
    [Fact]
    public void Bag_packages_are_unpacked_up_to_the_free_spots_and_the_rest_wait_in_the_bag()
    {
        var game = new FakeGame { FreeSpots = 2, Bag = { Pkg(1), Pkg(2), Pkg(3), Pkg(4) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 2 }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(2, tally.Queued);
        Assert.Equal(2, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void Bag_packages_take_the_spots_before_any_floor_package_is_picked_up()
    {
        var game = new FakeGame { FreeSpots = 1, Bag = { Pkg(1) }, Floor = { Pkg(10) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1 }, game.Unpacking);
        Assert.Empty(game.PickupsTried);
        Assert.Equal(new[] { Pkg(10) }, game.Floor);
        Assert.Equal(1, tally.NoSpot);
    }

    [Fact]
    public void Floor_packages_are_picked_up_only_for_the_spots_the_bag_leaves()
    {
        var game = new FakeGame { FreeSpots = 3, Bag = { Pkg(1) }, Floor = { Pkg(10), Pkg(11), Pkg(12) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { 1, 10 + BagIdOffset, 11 + BagIdOffset }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(new[] { Pkg(12) }, game.Floor);
        Assert.Equal(2, tally.PickedUp);
        Assert.Equal(0, tally.WaitingInBag);
        Assert.Equal(1, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void Picked_up_packages_are_unpacked_by_the_id_they_get_in_the_bag()
    {
        var game = new FakeGame { FreeSpots = 1, Floor = { Pkg(10) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { 10 + BagIdOffset }, game.Unpacking);
        Assert.Equal(1, tally.Queued);
    }

    [Fact]
    public void With_no_free_spot_nothing_leaves_the_floor_and_the_character_keeps_its_current_action()
    {
        var game = new FakeGame { FreeSpots = 0, StartClearsQueue = true, Bag = { Pkg(1) }, Floor = { Pkg(10) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Empty(game.PickupsTried);
        Assert.Equal(new[] { Walking }, game.Running);
        Assert.Equal(2, tally.NoSpot);
        Assert.Equal(0, tally.Queued);
    }

    [Fact]
    public void A_second_press_while_unpacking_neither_lines_up_more_nor_touches_the_floor()
    {
        var game = new FakeGame { FreeSpots = 3, Bag = { Pkg(1), Pkg(2) } };

        UnpackRun.Run(game);
        game.Floor.Add(Pkg(10));

        var outcome = UnpackRun.Run(game);

        Assert.Equal(new UnpackOutcome.AlreadyUnpacking(2), outcome);
        Assert.Equal(new long[] { 1, 2 }, game.Unpacking);
        Assert.Empty(game.LinedUp);
        Assert.Empty(game.PickupsTried);
    }

    [Fact]
    public void A_package_the_game_refuses_hands_its_spot_to_the_next_one()
    {
        var game = new FakeGame { FreeSpots = 1, Bag = { Pkg(1), Pkg(2) }, Refusals = { [1] = "Not now." } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 2 }, game.Unpacking);
        Assert.Equal(1, tally.Unusable);
        Assert.Equal("Not now.", tally.UnusableReason);
        Assert.Equal(0, tally.NoSpot);
    }

    [Fact]
    public void A_floor_package_the_bag_cannot_carry_stays_on_the_floor_and_the_next_one_is_tried()
    {
        var game = new FakeGame { FreeSpots = 1, Floor = { Pkg(10), Pkg(11) }, TooHeavy = { 10 } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { 11 + BagIdOffset }, game.Unpacking);
        Assert.Equal(new[] { Pkg(10) }, game.Floor);
        Assert.Equal(1, tally.FloorRefused);
        Assert.Equal(1, tally.PickedUp);
    }

    [Fact]
    public void Part_of_a_floor_stack_the_bag_cannot_carry_is_counted_as_left_on_the_floor()
    {
        var game = new FakeGame { FreeSpots = 3, BagRoom = 2, Floor = { Pkg(10, count: 3) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(2, tally.PickedUp);
        Assert.Equal(1, tally.FloorRefused);
        Assert.Equal(2, tally.Queued);
        Assert.Equal(new[] { Pkg(10, count: 1) }, game.Floor);
    }

    [Fact]
    public void A_stack_takes_one_spot_per_package_in_it()
    {
        var game = new FakeGame { FreeSpots = 2, Bag = { Pkg(1, count: 3) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 1 }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(1, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void A_floor_package_that_lands_on_a_bag_stack_is_still_unpacked()
    {
        var game = new FakeGame { FreeSpots = 2, Bag = { Pkg(1) }, Floor = { Pkg(10) }, MergesInto = { [10] = 1 } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 1 }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(1, tally.PickedUp);
        Assert.Equal(2, tally.Queued);
        Assert.Equal(0, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void A_floor_package_that_lands_on_a_bag_stack_is_unpacked_once_even_with_spots_to_spare()
    {
        var game = new FakeGame { FreeSpots = 3, Bag = { Pkg(1) }, Floor = { Pkg(10) }, MergesInto = { [10] = 1 } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 1 }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(2, tally.Queued);
        Assert.Equal(0, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void A_floor_stack_that_lands_on_a_bag_stack_fills_the_spots_left_and_the_rest_wait_in_the_bag()
    {
        var game = new FakeGame { FreeSpots = 3, Bag = { Pkg(1, count: 2) }, Floor = { Pkg(10, count: 4) }, MergesInto = { [10] = 1 } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 1, 1 }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(3, tally.Queued);
        Assert.Equal(4, tally.PickedUp);
        Assert.Equal(3, tally.WaitingInBag);
        Assert.Equal(3, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void A_picked_up_stack_bigger_than_the_spots_left_waits_in_the_bag_and_stops_further_pickups()
    {
        var game = new FakeGame { FreeSpots = 1, Floor = { Pkg(10, count: 3), Pkg(11) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { 10 + BagIdOffset }, game.Unpacking);
        Assert.Empty(game.OverQueued);
        Assert.Equal(new[] { Pkg(11) }, game.Floor);
        Assert.Equal(3, tally.PickedUp);
        Assert.Equal(2, tally.WaitingInBag);
        Assert.Equal(3, tally.NoSpot);
        Assert.Equal(0, tally.Unusable);
    }

    [Fact]
    public void Away_from_home_the_floor_is_left_alone_and_the_bag_still_unpacks()
    {
        var game = new FakeGame { FreeSpots = 2, IsHome = false, Bag = { Pkg(1) }, Floor = { Pkg(10) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Empty(game.PickupsTried);
        Assert.Equal(new long[] { 1 }, game.Unpacking);
        Assert.Equal(1, tally.FloorAway);
    }

    [Fact]
    public void When_nothing_can_be_lined_up_the_character_keeps_doing_what_it_was_doing()
    {
        var game = new FakeGame { FreeSpots = 2, StartClearsQueue = true, Bag = { Pkg(1) }, Refusals = { [1] = "Not now." } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { Walking }, game.Running);
        Assert.Equal(0, tally.Queued);
        Assert.Equal(1, tally.Unusable);
    }

    [Fact]
    public void A_package_the_game_will_not_line_up_is_reported_with_its_whole_stack()
    {
        var game = new FakeGame { FreeSpots = 2, StartClearsQueue = true, RefusesToQueue = true, Bag = { Pkg(1, count: 2) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new[] { Walking }, game.Running);
        Assert.Equal(2, tally.Unusable);
        Assert.Equal(UnpackRun.QueueRefusedReason, tally.UnusableReason);
    }

    [Fact]
    public void Unpacks_join_the_end_of_the_queue_and_the_actions_the_player_queued_stay()
    {
        var game = new FakeGame { FreeSpots = 2, Bag = { Pkg(1), Pkg(2) }, Running = { Adrenaline } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { Walking, Adrenaline, 1, 2 }, game.Running);
        Assert.Equal(2, tally.Queued);
    }

    [Fact]
    public void When_starting_would_throw_away_actions_the_player_queued_nothing_is_touched()
    {
        var game = new FakeGame { FreeSpots = 3, StartClearsQueue = true, Bag = { Pkg(1) }, Floor = { Pkg(10) }, Running = { Adrenaline } };

        Assert.Equal(new UnpackOutcome.QueueBusy(), UnpackRun.Run(game));
        Assert.Equal(new long[] { Walking, Adrenaline }, game.Running);
        Assert.Empty(game.LinedUp);
        Assert.Empty(game.PickupsTried);
    }

    [Fact]
    public void When_starting_replaces_a_queue_the_player_put_nothing_in_the_unpacks_start()
    {
        var game = new FakeGame { FreeSpots = 2, StartClearsQueue = true, Bag = { Pkg(1), Pkg(2) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Equal(new long[] { 1, 2 }, game.Running);
        Assert.Equal(2, tally.Queued);
    }

    [Fact]
    public void Outside_a_loaded_game_a_press_does_nothing()
    {
        var game = new FakeGame { IsNoGame = true, FreeSpots = 2, Bag = { Pkg(1) }, Floor = { Pkg(10) } };

        Assert.Equal(new UnpackOutcome.NoGame(), UnpackRun.Run(game));
        Assert.Equal(new[] { Walking }, game.Running);
        Assert.Empty(game.LinedUp);
        Assert.Empty(game.PickupsTried);
    }

    [Fact]
    public void A_game_that_is_not_ready_is_reported_and_nothing_is_touched()
    {
        var game = new FakeGame { NotReadyReason = "your home is not loaded", FreeSpots = 2, Bag = { Pkg(1) } };

        Assert.Equal(new UnpackOutcome.NotReady("your home is not loaded"), UnpackRun.Run(game));
        Assert.Equal(new[] { Walking }, game.Running);
        Assert.Empty(game.LinedUp);
    }

    [Fact]
    public void An_empty_bag_and_floor_say_there_is_nothing_to_unpack()
    {
        var game = new FakeGame { FreeSpots = 2 };

        Assert.Equal(new UnpackOutcome.NothingToUnpack(), UnpackRun.Run(game));
    }

    [Fact]
    public void A_batch_the_game_will_not_start_is_reported()
    {
        var game = new FakeGame { FreeSpots = 2, FailsToStart = true, Bag = { Pkg(1) } };

        Assert.Equal(new UnpackOutcome.StartFailed(1, 0), UnpackRun.Run(game));
    }

    [Fact]
    public void A_batch_the_game_will_not_start_still_reports_the_packages_it_picked_up()
    {
        var game = new FakeGame { FreeSpots = 2, FailsToStart = true, Bag = { Pkg(1) }, Floor = { Pkg(10) } };

        Assert.Equal(new UnpackOutcome.StartFailed(2, 1), UnpackRun.Run(game));
        Assert.Contains(Pkg(10 + BagIdOffset), game.Bag);
    }

    [Fact]
    public void Negative_free_spots_from_a_confused_read_unpack_nothing()
    {
        var game = new FakeGame { FreeSpots = -3, Bag = { Pkg(1) } };

        var tally = Done(UnpackRun.Run(game));

        Assert.Empty(game.Unpacking);
        Assert.Empty(game.LinedUp);
        Assert.Equal(1, tally.NoSpot);
    }

    private static UnpackTally Done(UnpackOutcome outcome) => Assert.IsType<UnpackOutcome.Done>(outcome).Tally;
}
