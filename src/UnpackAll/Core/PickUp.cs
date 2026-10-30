using System;
using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>A furniture package standing at one of the yard's package spots, as a Pick up press sees it.</summary>
/// <param name="Id">The yard package's own id, the one the game's click menu was opened on.</param>
/// <param name="FurnitureId">The furniture the package installs as.</param>
/// <param name="ItemConfigId">The bag item the package turns back into, dye included.</param>
/// <param name="IsBought">Bought from the shop, so the shop would still take it back for a refund.</param>
public sealed record YardPackage(long Id, int FurnitureId, int ItemConfigId, bool IsBought);

/// <summary>What the game's click menu is open on, for the Pick up entry.</summary>
public abstract record YardRead
{
    private YardRead()
    {
    }

    /// <summary>Not a furniture package in the yard (other furniture, a shopping bag, nothing): no Pick up entry.</summary>
    public sealed record NotOurs : YardRead;

    /// <summary>A yard package that can't be picked up right now; the entry shows greyed out with the reason.</summary>
    public sealed record Refused(Phrase Reason) : YardRead;

    public sealed record Ready(YardPackage Package) : YardRead;
}

/// <summary>Where a picked-up package's item went.</summary>
public enum PickUpPlace
{
    Bag,
    ShoppingBag,
}

/// <summary>The item a press put somewhere, so it can be taken out again if the yard package will not go.</summary>
public readonly record struct PlacedItem(PickUpPlace Place, long OwnerId, long ItemId);

/// <summary>
/// The game as a Pick up press sees it. Each call is one read or one change the game itself makes elsewhere. None
/// throws: a call that fails answers as if the game had said no.
/// </summary>
public interface IPickUpGame
{
    YardRead Read(long furnitureId);

    /// <summary>Adds one of the item to the player's bag, which checks room and carry weight. Null when it did not fit.</summary>
    PlacedItem? AddToBag(int itemConfigId);

    /// <summary>Adds one of the item to a shopping bag in the living room. Null when neither shopping bag took it.</summary>
    PlacedItem? AddToShoppingBag(int itemConfigId);

    /// <summary>Removes the package from the yard the way the game's Destroy does, without its drops. False when it is still there.</summary>
    bool RemoveFromYard(YardPackage package);

    /// <summary>Takes back an item a press added. False when that failed.</summary>
    bool TakeBack(PlacedItem item);
}

/// <summary>How one Pick up press ended.</summary>
public abstract record PickUpOutcome
{
    private PickUpOutcome()
    {
    }

    /// <summary>The menu is no longer on a yard package.</summary>
    public sealed record Gone : PickUpOutcome;

    public sealed record Refused(Phrase Reason) : PickUpOutcome;

    /// <summary>Neither the bag nor a shopping bag had room; nothing changed.</summary>
    public sealed record NoRoom : PickUpOutcome;

    /// <summary>The yard package would not go, so the item was taken back out (when TookBack) and the package stays.</summary>
    public sealed record Failed(bool TookBack) : PickUpOutcome;

    public sealed record PickedUp(PickUpPlace Place, bool WasBought) : PickUpOutcome;
}

/// <summary>
/// One Pick up press. The package's item is placed first, in the bag or else a shopping bag, and the yard package is
/// removed only once its item has somewhere to be. A package whose item fits nowhere stays in the yard untouched.
/// </summary>
public static class PickUpRun
{
    /// <summary>What the click menu's entry shows: null for none, "" for a working entry, or the reason it is greyed out.</summary>
    public static string? EntryState(IPickUpGame game, long? furnitureId, bool isChinese) =>
        furnitureId is not { } id ? null : game.Read(id) switch
        {
            YardRead.NotOurs => null,
            YardRead.Refused refused => refused.Reason.Pick(isChinese),
            YardRead.Ready => "",
            var read => throw new InvalidOperationException("unknown yard read " + read),
        };

    public static PickUpOutcome Run(IPickUpGame game, long? furnitureId)
    {
        if (furnitureId is not { } id) return new PickUpOutcome.Gone();

        switch (game.Read(id))
        {
            case YardRead.NotOurs:
                return new PickUpOutcome.Gone();
            case YardRead.Refused refused:
                return new PickUpOutcome.Refused(refused.Reason);
            case YardRead.Ready ready:
                return PickUp(game, ready.Package);
            case var read:
                throw new InvalidOperationException("unknown yard read " + read);
        }
    }

    private static PickUpOutcome PickUp(IPickUpGame game, YardPackage package)
    {
        var placed = game.AddToBag(package.ItemConfigId) ?? game.AddToShoppingBag(package.ItemConfigId);

        if (placed is not { } item) return new PickUpOutcome.NoRoom();
        if (!game.RemoveFromYard(package)) return new PickUpOutcome.Failed(game.TakeBack(item));

        return new PickUpOutcome.PickedUp(item.Place, package.IsBought);
    }
}

/// <summary>The game's rules for which yard furniture is a package a press may pick up, and which item it becomes.</summary>
public static class YardRules
{
    private const string HomeSpotPrefix = "HomePackage";

    /// <summary>Only the yard's numbered spots ("HomePackage3"): the car's spot and the boxes by the doors are left alone.</summary>
    public static bool IsHomePackageSpot(string? node)
    {
        if (node is null || node.Length <= HomeSpotPrefix.Length || !node.StartsWith(HomeSpotPrefix, StringComparison.Ordinal)) return false;

        for (var i = HomeSpotPrefix.Length; i < node.Length; i++)
        {
            if (node[i] is < '0' or > '9') return false;
        }

        return true;
    }

    /// <summary>
    /// The package item with the yard package's dye (0 for none), out of every item that installs as its furniture; the
    /// game gives an unpacked package the dye of the item it came from. 0 when no item matches, so the package is not
    /// turned into a different colour or into nothing.
    /// </summary>
    public static int ItemFor(IReadOnlyList<int> packageItemIds, Func<int, int> dyeSlotOf, int dyeSlot)
    {
        foreach (var itemId in packageItemIds)
        {
            if (dyeSlotOf(itemId) == dyeSlot) return itemId;
        }

        return 0;
    }
}
