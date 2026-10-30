using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The game's side of a Pick up press. The item is made with the game's own ItemManager.TryAddItemToBag, the call the
/// game uses to give an item back (RenovationManager.RefundRetiredUnit), which checks the bag's room and carry weight
/// and drops nothing on the floor when it refuses. A full bag sends it to the living room's shopping bags, which the
/// game makes on demand. The yard package is then removed the way BagFurniture.Dismantle removes it, without the
/// materials Dismantle drops. Each call fails soft: an exception is logged once per kind of call and answered as a
/// refusal.
/// </summary>
internal sealed class GamePickUp : IPickUpGame
{
    /// <summary>Shopping bags 0 and 1, the game's two door boxes.</summary>
    private const int ShoppingBagCount = 2;

    /// <summary>The shopping bag furniture, as the game makes it when something is bought with the bag full.</summary>
    private const int ShoppingBagFurnitureId = 1024;

    private const int HoursPerDay = 24;

    private readonly Action<string> _log;
    private readonly HashSet<string> _reportedFailures = new();

    public GamePickUp(Action<string> log)
    {
        _log = log;
    }

    public YardRead Read(long furnitureId)
    {
        try
        {
            return ReadYard(furnitureId);
        }
        catch (Exception ex)
        {
            ReportOnce("yard read", ex);

            return new YardRead.NotOurs();
        }
    }

    public PlacedItem? AddToBag(int itemConfigId)
    {
        try
        {
            var role = Role();

            return role == null ? null : Add(PickUpPlace.Bag, role.InstanceId, itemConfigId);
        }
        catch (Exception ex)
        {
            ReportOnce("add to the bag", ex);

            return null;
        }
    }

    /// <summary>
    /// The first shopping bag that takes the item, made first when the game has not made it yet, the way Pack Rat
    /// fills them. They carry no weight limit, only their own room.
    /// </summary>
    public PlacedItem? AddToShoppingBag(int itemConfigId)
    {
        try
        {
            var agents = World()?._AgentManager;
            var role = agents?.GetLeadingRole();

            if (agents == null || role == null) return null;

            for (var box = 0; box < ShoppingBagCount; box++)
            {
                if (agents.FindDoorBoxFurniture(box) == null && !CreateShoppingBag(role, box)) continue;

                var placed = Add(PickUpPlace.ShoppingBag, VehicleConfig.GetBoxOwnerId(box), itemConfigId);

                if (placed == null) continue;

                role.PushDoorBoxToRedux();

                return placed;
            }

            return null;
        }
        catch (Exception ex)
        {
            ReportOnce("add to a shopping bag", ex);

            return null;
        }
    }

    /// <summary>
    /// BagFurniture.Dismantle's last three steps: the package leaves the yard, its purchase record goes (so the shop
    /// no longer offers its refund), and a pre-disaster purchase leaves the ledger. When that throws, the yard is the
    /// truth: only a package seen still there counts as not removed, so a failure can leave a spare item but never
    /// lose the package.
    /// </summary>
    public bool RemoveFromYard(YardPackage package)
    {
        var role = Role();
        var build = role == null ? null : AgentTools.GetAgentComponent<BuildComponent>(role);

        if (build == null) return false;

        try
        {
            build.DestroyBagFurniture(package.Id);
            build.RemovePurchased(package.Id);
            BagFurniture.RevokePreDisasterFurnitureLedger(package.FurnitureId);

            return true;
        }
        catch (Exception ex)
        {
            ReportOnce("remove from the yard", ex);

            return !IsStillInYard(build, package.Id);
        }
    }

    public bool TakeBack(PlacedItem item)
    {
        if (item.ItemId == 0) return false;

        try
        {
            var world = World();
            var items = world?._ItemManager;

            if (items == null) return false;

            items.CostItem(item.OwnerId, item.ItemId, 1, false);

            if (item.Place == PickUpPlace.ShoppingBag) world!._AgentManager?.GetLeadingRole()?.PushDoorBoxToRedux();

            return true;
        }
        catch (Exception ex)
        {
            ReportOnce("take back", ex);

            return false;
        }
    }

    private YardRead ReadYard(long furnitureId)
    {
        var world = World();
        var config = GameSingleton.Existing<ConfigManager>();
        var agents = world?._AgentManager;
        var role = agents?.GetLeadingRole();

        if (world == null || config == null || agents == null || role == null || !GamePackages.IsAfterDisaster(world)) return new YardRead.NotOurs();

        var build = AgentTools.GetAgentComponent<BuildComponent>(role);
        var spots = build?._PackagePointMap_k__BackingField;

        if (build == null || spots == null || !spots.TryGetValue(furnitureId, out var spot) || !YardRules.IsHomePackageSpot(spot)) return new YardRead.NotOurs();

        var bag = agents.GetAgent(furnitureId)?.TryCast<BagFurniture>();

        if (bag == null || bag._IsDoorBox_k__BackingField) return new YardRead.NotOurs();
        if (world.GameModeKey != GameMode.Default || !agents.IsHomeMap(ExploreSearchStateManager.GetCurrentMapId())) return new YardRead.Refused(Texts.NoHome);

        // A package being carried or placed is hidden; IsSelected is only the hover highlight every menu target has.
        if (!bag._IsShow_k__BackingField) return new YardRead.Refused(Texts.BeingMoved);

        var furnitureConfigId = bag._FurnitureConfigId_k__BackingField;

        if (BagFurniture.DismantleBlockedPackages?.Contains(furnitureConfigId) == true) return new YardRead.Refused(Texts.NoBagForm);

        var itemConfigId = YardRules.ItemFor(PackageItems(config, furnitureConfigId), config.GetDyedPackageSlot, bag._DyeSlot_k__BackingField);

        if (itemConfigId == 0) return new YardRead.Refused(Texts.NoBagForm);

        var isBought = build._PurchaseSourceMap_k__BackingField?.ContainsKey(furnitureId) == true;

        return new YardRead.Ready(new YardPackage(furnitureId, furnitureConfigId, itemConfigId, isBought));
    }

    /// <summary>Every package item that installs as this furniture, dyed ones included; story furniture has none.</summary>
    private static List<int> PackageItems(ConfigManager config, int furnitureConfigId)
    {
        var found = config.GetFurniturePackageItemIds(furnitureConfigId);
        var ids = new List<int>();

        if (found == null) return ids;

        for (var i = 0; i < found.Count; i++) ids.Add(found[i]);

        return ids;
    }

    /// <summary>
    /// Adds one item the way RenovationManager.RefundRetiredUnit gives one back: fresh, from this hour. The stack that
    /// grew is found by comparing the owner's stacks of the item before and after, so a take-back takes the right one.
    /// </summary>
    private PlacedItem? Add(PickUpPlace place, long ownerId, int itemConfigId)
    {
        var world = World();
        var items = world?._ItemManager;
        var life = GameSingleton.Existing<ConfigManager>()?.Get_Config_Item(itemConfigId)?.Life ?? 0;

        if (world == null || items == null) return null;

        var before = Stacks(items, ownerId, itemConfigId);
        var hour = world._GameTimeManager?.GetHour() ?? 0;
        var timeLeft = life <= 0 ? -1 : HoursPerDay * life;

        if (!items.TryAddItemToBag(ownerId, itemConfigId, 1, hour, timeLeft, 0, 1f, null, 0, null, null, 0, 0, false, false, false, false, 0)) return null;

        var itemId = GrownStack(Stacks(items, ownerId, itemConfigId), before);

        if (itemId == 0) _log($"added item {itemConfigId} to {ownerId} but could not find its stack, so it could not be taken back");

        return new PlacedItem(place, ownerId, itemId);
    }

    private static Dictionary<long, int> Stacks(ItemManager items, long ownerId, int itemConfigId)
    {
        var stacks = new Dictionary<long, int>();
        var all = items.GetItemsByConfigId(itemConfigId);

        if (all == null) return stacks;

        for (var i = 0; i < all.Count; i++)
        {
            var item = all[i];

            if (item != null && item.OwnerId == ownerId) stacks[item.InstanceId] = item.ItemCount;
        }

        return stacks;
    }

    private static long GrownStack(Dictionary<long, int> after, Dictionary<long, int> before)
    {
        foreach (var stack in after)
        {
            if (!before.TryGetValue(stack.Key, out var count) || stack.Value > count) return stack.Key;
        }

        return 0;
    }

    /// <summary>The game picks the shopping bag spot nearest the given position on the home map.</summary>
    private static bool CreateShoppingBag(LeadingRole role, int box)
    {
        var build = AgentTools.GetAgentComponent<BuildComponent>(role);
        var position = AgentTools.GetAgentComponent<TransformComponent>(role)?.Position ?? Vector3.zero;

        return build != null && build.CreateBagFurniture(ShoppingBagFurnitureId, position, true, box) != 0;
    }

    private bool IsStillInYard(BuildComponent build, long id)
    {
        try
        {
            return build._PackagePointMap_k__BackingField?.ContainsKey(id) == true;
        }
        catch (Exception ex)
        {
            ReportOnce("yard check", ex);

            return false;
        }
    }

    private static BattleLogicWorld? World() => GameSingleton.Existing<BattleLogicWorld>();

    private static LeadingRole? Role() => World()?._AgentManager?.GetLeadingRole();

    private void ReportOnce(string operation, Exception ex)
    {
        if (_reportedFailures.Add(operation)) _log($"pick up: {operation} failed (each kind reported once): {ex}");
    }
}
