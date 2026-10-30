using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

/// <summary>
/// A picked-up package's item goes to the bag, else a shopping bag, else nowhere; the yard package is removed only
/// after its item has a home, and never when it has none.
/// </summary>
public class PickUpRunTests
{
    private const long YardId = 501;
    private const int FurnitureId = 300;
    private const int ItemId = 9100;

    [Fact]
    public void The_item_goes_to_the_bag_and_only_then_the_yard_package_is_removed()
    {
        var game = new FakePickUpGame();

        var outcome = PickUpRun.Run(game, YardId);

        Assert.Equal(new PickUpOutcome.PickedUp(PickUpPlace.Bag, WasBought: false), outcome);
        Assert.Equal(new[] { "bag", "remove" }, game.Calls);
        Assert.Equal(new[] { ItemId }, game.InBag);
        Assert.Null(game.Yard);
    }

    [Fact]
    public void A_full_bag_sends_the_item_to_a_shopping_bag_before_the_yard_package_is_removed()
    {
        var game = new FakePickUpGame { BagFits = false };

        var outcome = PickUpRun.Run(game, YardId);

        Assert.Equal(new PickUpOutcome.PickedUp(PickUpPlace.ShoppingBag, WasBought: false), outcome);
        Assert.Equal(new[] { "bag", "shopping", "remove" }, game.Calls);
        Assert.Empty(game.InBag);
        Assert.Equal(new[] { ItemId }, game.InShoppingBag);
        Assert.Null(game.Yard);
    }

    [Fact]
    public void The_shopping_bag_is_not_tried_when_the_bag_took_the_item()
    {
        var game = new FakePickUpGame();

        PickUpRun.Run(game, YardId);

        Assert.DoesNotContain("shopping", game.Calls);
        Assert.Empty(game.InShoppingBag);
    }

    [Fact]
    public void With_no_room_anywhere_the_yard_package_is_never_touched()
    {
        var game = new FakePickUpGame { BagFits = false, ShoppingBagFits = false };

        var outcome = PickUpRun.Run(game, YardId);

        Assert.Equal(new PickUpOutcome.NoRoom(), outcome);
        Assert.Equal(new[] { "bag", "shopping" }, game.Calls);
        Assert.NotNull(game.Yard);
    }

    [Fact]
    public void A_yard_package_that_will_not_go_has_its_new_item_taken_back_out_of_the_bag()
    {
        var game = new FakePickUpGame { RemoveWorks = false };

        var outcome = PickUpRun.Run(game, YardId);

        Assert.Equal(new PickUpOutcome.Failed(TookBack: true), outcome);
        Assert.Equal(new[] { "bag", "remove", "takeback" }, game.Calls);
        Assert.Empty(game.InBag);
        Assert.NotNull(game.Yard);
    }

    [Fact]
    public void The_take_back_goes_to_the_shopping_bag_the_item_was_put_in()
    {
        var game = new FakePickUpGame { BagFits = false, RemoveWorks = false };

        PickUpRun.Run(game, YardId);

        Assert.Equal(new PlacedItem(PickUpPlace.ShoppingBag, FakePickUpGame.ShoppingBagOwner, FakePickUpGame.NewItemId), game.TakenBack);
        Assert.Empty(game.InShoppingBag);
    }

    [Fact]
    public void A_failed_take_back_is_reported_so_the_toast_can_say_so()
    {
        var game = new FakePickUpGame { RemoveWorks = false, TakeBackWorks = false };

        Assert.Equal(new PickUpOutcome.Failed(TookBack: false), PickUpRun.Run(game, YardId));
    }

    [Fact]
    public void A_bought_package_says_it_was_bought_so_the_toast_can_warn_about_the_refund()
    {
        var game = new FakePickUpGame { Yard = new YardPackage(YardId, FurnitureId, ItemId, IsBought: true) };

        Assert.Equal(new PickUpOutcome.PickedUp(PickUpPlace.Bag, WasBought: true), PickUpRun.Run(game, YardId));
    }

    [Fact]
    public void A_refused_package_is_left_alone_with_its_reason()
    {
        var game = new FakePickUpGame { Refusal = Texts.BeingMoved };

        Assert.Equal(new PickUpOutcome.Refused(Texts.BeingMoved), PickUpRun.Run(game, YardId));
        Assert.Empty(game.Calls);
    }

    [Fact]
    public void A_menu_no_longer_on_a_yard_package_does_nothing()
    {
        var game = new FakePickUpGame();

        Assert.Equal(new PickUpOutcome.Gone(), PickUpRun.Run(game, null));
        Assert.Equal(new PickUpOutcome.Gone(), PickUpRun.Run(game, YardId + 1));
        Assert.Empty(game.Calls);
    }

    [Fact]
    public void The_menu_entry_shows_only_on_a_yard_package_and_is_greyed_out_with_the_reason()
    {
        var game = new FakePickUpGame();

        Assert.Equal("", PickUpRun.EntryState(game, YardId, isChinese: false));
        Assert.Null(PickUpRun.EntryState(game, YardId + 1, isChinese: false));
        Assert.Null(PickUpRun.EntryState(game, null, isChinese: false));

        game.Refusal = Texts.NoBagForm;

        Assert.Equal(Texts.NoBagForm.En, PickUpRun.EntryState(game, YardId, isChinese: false));
        Assert.Equal(Texts.NoBagForm.Zh, PickUpRun.EntryState(game, YardId, isChinese: true));
        Assert.Empty(game.Calls);
    }

    [Fact]
    public void Every_outcome_has_a_toast_in_both_languages()
    {
        var outcomes = new PickUpOutcome[]
        {
            new PickUpOutcome.Gone(), new PickUpOutcome.Refused(Texts.BeingMoved), new PickUpOutcome.NoRoom(), new PickUpOutcome.Failed(true),
            new PickUpOutcome.Failed(false), new PickUpOutcome.PickedUp(PickUpPlace.Bag, false), new PickUpOutcome.PickedUp(PickUpPlace.ShoppingBag, true),
        };

        foreach (var outcome in outcomes)
        {
            Assert.False(string.IsNullOrWhiteSpace(PickUpMessage.Describe(outcome, isChinese: false)));
            Assert.False(string.IsNullOrWhiteSpace(PickUpMessage.Describe(outcome, isChinese: true)));
        }
    }

    [Fact]
    public void The_toast_names_where_the_package_went_and_warns_about_the_lost_refund_only_when_bought()
    {
        Assert.Equal(Texts.PickedIntoBag.En, PickUpMessage.Describe(new PickUpOutcome.PickedUp(PickUpPlace.Bag, false)));
        Assert.Equal(
            Texts.PickedIntoShoppingBag.En + " " + Texts.NoLongerRefundable.En,
            PickUpMessage.Describe(new PickUpOutcome.PickedUp(PickUpPlace.ShoppingBag, true)));
        Assert.Equal(Texts.PickedIntoBag.Zh + Texts.NoLongerRefundable.Zh, PickUpMessage.Describe(new PickUpOutcome.PickedUp(PickUpPlace.Bag, true), isChinese: true));
    }

    /// <summary>One yard package; every call is recorded in order, so a test can see what happened before what.</summary>
    private sealed class FakePickUpGame : IPickUpGame
    {
        public const long PlayerBag = 7;
        public const long ShoppingBagOwner = 1023;
        public const long NewItemId = 4242;

        public YardPackage? Yard { get; set; } = new(YardId, FurnitureId, ItemId, IsBought: false);

        public Phrase? Refusal { get; set; }

        public bool BagFits { get; set; } = true;

        public bool ShoppingBagFits { get; set; } = true;

        public bool RemoveWorks { get; set; } = true;

        public bool TakeBackWorks { get; set; } = true;

        public List<string> Calls { get; } = new();

        public List<int> InBag { get; } = new();

        public List<int> InShoppingBag { get; } = new();

        public PlacedItem? TakenBack { get; private set; }

        public YardRead Read(long furnitureId)
        {
            if (Yard == null || Yard.Id != furnitureId) return new YardRead.NotOurs();

            return Refusal is { } reason ? new YardRead.Refused(reason) : new YardRead.Ready(Yard);
        }

        public PlacedItem? AddToBag(int itemConfigId)
        {
            Calls.Add("bag");

            if (!BagFits) return null;

            InBag.Add(itemConfigId);

            return new PlacedItem(PickUpPlace.Bag, PlayerBag, NewItemId);
        }

        public PlacedItem? AddToShoppingBag(int itemConfigId)
        {
            Calls.Add("shopping");

            if (!ShoppingBagFits) return null;

            InShoppingBag.Add(itemConfigId);

            return new PlacedItem(PickUpPlace.ShoppingBag, ShoppingBagOwner, NewItemId);
        }

        public bool RemoveFromYard(YardPackage package)
        {
            Calls.Add("remove");

            if (!RemoveWorks) return false;

            Yard = null;

            return true;
        }

        public bool TakeBack(PlacedItem item)
        {
            Calls.Add("takeback");
            TakenBack = item;

            if (!TakeBackWorks) return false;

            (item.Place == PickUpPlace.Bag ? InBag : InShoppingBag).RemoveAt(0);

            return true;
        }
    }
}
