using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The game's side of an unpack press. Every call goes through the game's own logic layer (the bag, the floor pickup,
/// the use check and the action queue the inventory's Use button feeds), never through the UI's copy of it. Each call
/// fails soft: an exception is logged once per kind of call and answered as a refusal.
/// </summary>
internal sealed class GamePackages : IPackageGame
{
    private const string NoWorld = "the game world is not loaded";
    private const string NoHome = "your home is not loaded";
    private const string ReadFailed = "the game could not be read, see BepInEx/unpack-all.log";
    private const string CheckFailed = "the game's use check failed, see BepInEx/unpack-all.log";
    private const string NoReasonGiven = "the game would not allow it";

    private readonly Action<string> _log;
    private readonly HashSet<string> _reportedFailures = new();

    /// <summary>Item config id to its unpack action id, or null when the item is not a package.</summary>
    private readonly Dictionary<int, int?> _unpackActions = new();

    private IntPtr _knownConfig;

    public GamePackages(Action<string> log)
    {
        _log = log;
    }

    public PackageRead Read()
    {
        try
        {
            return ReadStock();
        }
        catch (Exception ex)
        {
            ReportOnce("read", ex);

            return new PackageRead.NotReady(ReadFailed);
        }
    }

    public int PickUp(Package floorPackage)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var drops = world?._DropItemManager;
            var role = world?._AgentManager?.GetLeadingRole();

            if (drops == null || role == null) return 0;

            var isAccepted = drops.TryPickupItem(floorPackage.ItemId, role.InstanceId, out var picked, false);
            var left = drops.FindDropItem(floorPackage.ItemId);
            var remaining = left == null || !left.HasItem ? 0 : left.Item.ItemCount;
            var taken = Math.Max(0, floorPackage.Count - remaining);

            // The floor is the truth: the game's own answer is only logged when it disagrees, since its meaning is not documented.
            if (taken != picked || (!isAccepted && taken != 0)) _log($"PICKUP MISMATCH floor package {floorPackage.ItemId}: taken {taken}, picked {picked}, accepted {isAccepted}");

            _log($"picked up floor package {floorPackage.ItemId} (config {floorPackage.ConfigId}): {taken} of {floorPackage.Count}");

            return taken;
        }
        catch (Exception ex)
        {
            ReportOnce("floor pickup", ex);

            return 0;
        }
    }

    public IReadOnlyList<Package> BagPackages()
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var config = GameSingleton.Existing<ConfigManager>();
            var items = world?._ItemManager;
            var role = world?._AgentManager?.GetLeadingRole();

            return items == null || config == null || role == null ? Array.Empty<Package>() : ReadBag(items, config, role.InstanceId);
        }
        catch (Exception ex)
        {
            ReportOnce("bag read", ex);

            return Array.Empty<Package>();
        }
    }

    /// <summary>The game's own check behind the Use button, which answers with the line it would have shown the player.</summary>
    public string? WhyUnusable(Package package)
    {
        try
        {
            var items = GameSingleton.Existing<BattleLogicWorld>()?._ItemManager;

            if (items == null) return NoWorld;
            if (items.VerifyItemUse(package.ItemId, package.ConfigId, out var failMessage)) return null;

            _log($"package {package.ItemId} (config {package.ConfigId}) refused by the use check: {failMessage}");

            return string.IsNullOrWhiteSpace(failMessage) ? NoReasonGiven : failMessage;
        }
        catch (Exception ex)
        {
            ReportOnce("use check", ex);

            return CheckFailed;
        }
    }

    /// <summary>Lines up one use of the package, as the inventory's Use button does, without starting it.</summary>
    public bool TryQueue(Package package)
    {
        try
        {
            var role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();
            var config = GameSingleton.Existing<ConfigManager>();

            if (role == null || config == null || UnpackAction(config, package.ConfigId) is not { } actionId) return false;

            role.AddLeadingRoleAction(actionId, package.ItemId, Vector3.zero, Quaternion.identity);

            return true;
        }
        catch (Exception ex)
        {
            ReportOnce("queue", ex);

            return false;
        }
    }

    /// <summary>
    /// Starts the lined-up uses as one batch. When that throws, the line is cleared so the next press does not read the
    /// stranded uses as an unpack in progress. That empties only the role's not-yet-started action cache, which held
    /// just this press's uses; actions already in the character's queue are not touched.
    /// </summary>
    public bool TryStart()
    {
        LeadingRole? role = null;

        try
        {
            role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();

            if (role == null) return false;

            role.DoLeadingRoleActionCache();

            return true;
        }
        catch (Exception ex)
        {
            ReportOnce("start", ex);

            if (role != null) ClearLine(role);

            return false;
        }
    }

    /// <summary>
    /// Reads the press's stock. The free spot count is read as it stands, without BuildComponent.ReconcilePackagePoints
    /// first: that is a repair pass, not a recount. It writes package points for unplaced bag furniture into the save's
    /// PackagePointMap and logs an error whenever it finds any, and the game already runs it itself after every unpack
    /// and every furniture purchase.
    /// </summary>
    private PackageRead ReadStock()
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var config = GameSingleton.Existing<ConfigManager>();
        var agents = world?._AgentManager;
        var role = agents?.GetLeadingRole();

        if (world == null || config == null || agents == null || role == null || !IsAfterDisaster(world)) return new PackageRead.NoGame();

        var items = world._ItemManager;
        var actions = world._ActionManager;
        var drops = world._DropItemManager;

        if (items == null || actions == null || drops == null) return new PackageRead.NotReady(NoWorld);

        var build = AgentTools.GetAgentComponent<BuildComponent>(role);

        if (build == null) return new PackageRead.NotReady(NoHome);

        var bag = ReadBag(items, config, role.InstanceId);
        var floor = ReadFloor(drops, config);
        var freeSpots = PackageRules.FreeSpots(build.GetPurchaseCount(), build.GetMaxCapacity(), build.GetFreePackagePointCount());
        var alreadyQueued = bag.Select(package => package.ConfigId).Distinct().Sum(configId => actions.CountPendingItemUses(role.InstanceId, configId));
        var queueOnStart = ReadQueueOnStart(role, actions);
        var isHome = IsHome(agents);

        _log($"stock: {Units(bag)} in the bag, {Units(floor)} on the floor at home, {freeSpots} free spots, {alreadyQueued} already lined up, start {queueOnStart}, at home {isHome}");

        return new PackageRead.Ready(new PackageStock(freeSpots, alreadyQueued, isHome, bag, floor, queueOnStart));
    }

    /// <summary>
    /// False in the prologue, before the disaster. This is the game's own test (SceneRatManager.IsPostDisaster reads
    /// the same field), done here without the game's singleton lookup.
    /// </summary>
    private static bool IsAfterDisaster(BattleLogicWorld world) => world._GameTimeManager?.WorldStateType == WorldStateType.PostDisaster;

    /// <summary>
    /// What starting the batch does to the character's queue. The game's start (ActionCacheComponent.DoActionCache)
    /// clears the character's actions first only in the Default action mode; in Queue mode, which the game sets after
    /// the prologue, the batch joins the end of the queue. A mode that cannot be read counts as Default, the one that
    /// can lose something.
    /// </summary>
    private static QueueOnStart ReadQueueOnStart(LeadingRole role, ActionManager actions)
    {
        var mode = AgentTools.GetAgentComponent<ActionCacheComponent>(role)?.Mode ?? ActionMode.Default;

        if (mode == ActionMode.Queue) return QueueOnStart.Appends;

        return actions.HasManualActionInQueue(role.InstanceId) ? QueueOnStart.ReplacesPlayerActions : QueueOnStart.ReplacesOwnActions;
    }

    private IReadOnlyList<Package> ReadBag(ItemManager items, ConfigManager config, long bagId)
    {
        var held = items.GetItemDataList(bagId);
        var packages = new List<Package>();

        if (held == null) return packages;

        for (var i = 0; i < held.Count; i++)
        {
            var item = held[i];

            if (item == null || item.ItemCount <= 0 || item.OwnerId != bagId || UnpackAction(config, item.ItemConfigId) == null) continue;

            packages.Add(new Package(item.InstanceId, item.ItemConfigId, item.ItemCount));
        }

        return packages;
    }

    /// <summary>
    /// Packages lying on the floor of the home map. Wave loot is left out: the game's drone collects it and tracks it
    /// by id.
    /// </summary>
    private IReadOnlyList<Package> ReadFloor(DropItemManager drops, ConfigManager config)
    {
        var found = new Il2CppSystem.Collections.Generic.List<DropItem>();
        var packages = new List<Package>();

        drops.GetDropItemsInHome(found);

        for (var i = 0; i < found.Count; i++)
        {
            var drop = found[i];

            if (drop == null || !drop.HasItem || drop.IsWaveLoot) continue;

            var item = drop.Item;

            if (item.ItemCount <= 0 || UnpackAction(config, item.ItemConfigId) == null) continue;
            if (PickupId(drops, drop) is not { } pickupId) continue;

            packages.Add(new Package(pickupId, item.ItemConfigId, item.ItemCount));
        }

        return packages;
    }

    /// <summary>The id the game's pickup looks a floor item up by: the drop's own id, or failing that its item's.</summary>
    private long? PickupId(DropItemManager drops, DropItem drop)
    {
        if (IsSameDrop(drops.FindDropItem(drop.InstanceId), drop)) return drop.InstanceId;
        if (IsSameDrop(drops.FindDropItem(drop.Item.InstanceId), drop)) return drop.Item.InstanceId;

        LogOnce("floor id", $"floor package {drop.InstanceId} (item {drop.Item.InstanceId}) is not found by either id; such packages are skipped (reported once)");

        return null;
    }

    private static bool IsSameDrop(DropItem? found, DropItem drop) => found != null && found.Pointer == drop.Pointer;

    /// <summary>
    /// Away from home the floor pickup would reach across maps, so a failed check counts as away and leaves the floor
    /// alone.
    /// </summary>
    private bool IsHome(AgentManager agents)
    {
        try
        {
            return agents.IsHomeMap(ExploreSearchStateManager.GetCurrentMapId());
        }
        catch (Exception ex)
        {
            ReportOnce("home check", ex);

            return false;
        }
    }

    /// <summary>
    /// The use action of a package, found the way the unpack effect itself is: item, then its use action, then that
    /// action's effect. Cached per item for as long as the game keeps the same config tables.
    /// </summary>
    private int? UnpackAction(ConfigManager config, int configId)
    {
        ForgetActionsOfOtherConfig(config);

        if (_unpackActions.TryGetValue(configId, out var known)) return known;

        var item = config.Get_Config_Item(configId);
        int? actionId = item != null && PackageRules.IsPackage(item.Category, item.TargetFurnitureID, EffectName(config, item.UseAction)) ? item.UseAction : null;
        _unpackActions[configId] = actionId;

        return actionId;
    }

    private static string? EffectName(ConfigManager config, int actionId)
    {
        if (actionId <= 0) return null;

        var action = config.Get_Config_Action(actionId);

        return action == null || action.EffectConfigID <= 0 ? null : config.Get_Config_Effect(action.EffectConfigID)?.ConfigName;
    }

    private void ForgetActionsOfOtherConfig(ConfigManager config)
    {
        if (config.Pointer == _knownConfig) return;

        _unpackActions.Clear();
        _knownConfig = config.Pointer;
    }

    private void ClearLine(LeadingRole role)
    {
        try
        {
            role.ClearLeadingRoleActionCache();
        }
        catch (Exception ex)
        {
            ReportOnce("clear line", ex);
        }
    }

    private static int Units(IEnumerable<Package> packages) => packages.Sum(package => package.Count);

    private void ReportOnce(string operation, Exception ex) => LogOnce(operation, $"{operation} failed (each kind reported once): {ex}");

    private void LogOnce(string key, string message)
    {
        if (_reportedFailures.Add(key)) _log(message);
    }
}
