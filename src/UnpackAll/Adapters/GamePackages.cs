using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameCore.HotUpdate.ReduxUI;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// The game's side of an unpack press. Every call goes through the game's own logic layer (the bag, the floor pickup,
/// the storage quick move, the use check and the action queue the inventory's Use button feeds), never through the UI's
/// copy of it. The UI is only read, to see which windows are open. Each call fails soft: an exception is logged once
/// per kind of call and answered as a refusal.
/// </summary>
internal sealed class GamePackages : IPackageGame
{
    /// <summary>
    /// The panels besides the bag that ContainerTabHotKey.GetOpenPanelType knows, each a window over its own item bin.
    /// Looked up on the first press that checks windows, on the main thread, then kept.
    /// </summary>
    private static Il2CppSystem.Type[]? _otherPanels;

    private static Il2CppSystem.Type? _bagPanel;
    private static Il2CppSystem.Type? _bagScreenState;

    private readonly Action<string> _log;
    private readonly Action<string> _detail;
    private readonly HashSet<string> _reportedFailures = new();

    /// <summary>Storage package item id to the storage holding it, as of the last read.</summary>
    private readonly Dictionary<long, long> _storageOwners = new();

    /// <summary>Item config id to its unpack action id, or null when the item is not a package.</summary>
    private readonly Dictionary<int, int?> _unpackActions = new();

    private readonly HashSet<int> _linedUpConfigs = new();

    private IntPtr _knownConfig;

    /// <summary>The package point found for this press: the character does not move while a press lines up its unpacks.</summary>
    private Vector3? _pressPoint;

    private bool _hasPressPoint;

    /// <param name="log">Logs a line to the BepInEx log and the mod's own file.</param>
    /// <param name="detail">Logs a line to the mod's own file only, for the lines a press writes per package.</param>
    public GamePackages(Action<string> log, Action<string> detail)
    {
        _log = log;
        _detail = detail;
    }

    /// <summary>Whether packages in storages at home are unpacked too. Set on the main thread.</summary>
    public bool IncludeStorages { get; set; }

    public PackageRead Read()
    {
        try
        {
            return ReadStock();
        }
        catch (Exception ex)
        {
            ReportOnce("read", ex);

            return new PackageRead.NotReady(Texts.ReadFailed);
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

            _detail($"picked up floor package {floorPackage.ItemId} (config {floorPackage.ConfigId}): {taken} of {floorPackage.Count}");

            return taken;
        }
        catch (Exception ex)
        {
            ReportOnce("floor pickup", ex);

            return 0;
        }
    }

    /// <summary>
    /// The game's own quick move of a whole stack into the bag (ItemManager.TryQuickMoveItem, silent, no mold check),
    /// the move a click on a stack does with the storage open. It weighs the whole stack before moving anything, and
    /// moves each unit out of the storage and into the bag in one step, so a refusal partway leaves every unit in one
    /// place or the other. The storage is the truth: what left it is what is counted.
    /// </summary>
    public int TakeFromStorage(Package storagePackage)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var items = world?._ItemManager;
            var role = world?._AgentManager?.GetLeadingRole();

            if (items == null || role == null || !_storageOwners.TryGetValue(storagePackage.ItemId, out var storageId)) return 0;

            var before = items.GetItemData(storagePackage.ItemId);

            if (before == null || before.OwnerId != storageId || before.ItemCount <= 0) return 0;

            var held = before.ItemCount;
            var isMoved = QuickMoveToBag(items, storagePackage.ItemId, storageId, role.InstanceId);
            var after = items.GetItemData(storagePackage.ItemId);
            var remaining = after != null && after.OwnerId == storageId ? Math.Max(0, after.ItemCount) : 0;
            var taken = Math.Max(0, held - remaining);

            // The game answers false after part of a stack already went onto bag stacks, so its answer is only logged.
            _detail($"took storage package {storagePackage.ItemId} (config {storagePackage.ConfigId}) from storage {storageId}: {taken} of {held}, accepted {isMoved?.ToString() ?? "threw"}");

            return taken;
        }
        catch (Exception ex)
        {
            ReportOnce("storage move", ex);

            return 0;
        }
    }

    /// <summary>
    /// The move alone, so a throw after part of the stack already left still reaches the read-back that counts what
    /// left. Null when the move threw; the exception is logged once.
    /// </summary>
    private bool? QuickMoveToBag(ItemManager items, long itemId, long storageId, long bagId)
    {
        try
        {
            return items.TryQuickMoveItem(itemId, storageId, bagId, true, false);
        }
        catch (Exception ex)
        {
            ReportOnce("storage move", ex);

            return null;
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
    public Phrase? WhyUnusable(Package package)
    {
        try
        {
            var items = GameSingleton.Existing<BattleLogicWorld>()?._ItemManager;

            if (items == null) return Texts.NoWorld;
            if (items.VerifyItemUse(package.ItemId, package.ConfigId, out var failMessage)) return null;

            _detail($"package {package.ItemId} (config {package.ConfigId}) refused by the use check: {failMessage}");

            return string.IsNullOrWhiteSpace(failMessage) ? Texts.NoReasonGiven : Phrase.FromGame(failMessage);
        }
        catch (Exception ex)
        {
            ReportOnce("use check", ex);

            return Texts.CheckFailed;
        }
    }

    /// <summary>
    /// Lines up one use of the package, as the inventory's Use button does, without starting it. The Use button sends
    /// the character to the package point nearest it (InstallPlanManager.OnSetPendingBagFurnitureItem, then
    /// LogicAdapter.OnStartBagFurnitureItemInstall), so this does too. The game puts a walk to the target in front of
    /// the unpack, and a use aimed at the world's origin instead left the queue at once with its package unopened. With
    /// no package point the Use button refuses with "bag furniture full", and so does this.
    /// </summary>
    public bool TryQueue(Package package)
    {
        try
        {
            var role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();
            var config = GameSingleton.Existing<ConfigManager>();

            if (role == null || config == null || UnpackAction(config, package.ConfigId) is not { } actionId) return false;

            if (PressPoint(config, role) is not { } point)
            {
                LogOnce("no package point", "the game has no package point to send an unpack to, so nothing is lined up (reported once)");

                return false;
            }

            role.AddLeadingRoleAction(actionId, package.ItemId, point, Quaternion.identity);
            _linedUpConfigs.Add(package.ConfigId);

            _detail($"lined up package {package.ItemId} (config {package.ConfigId}, action {actionId}) to unpack at package point {point}");

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

    /// <summary>The game's own count of item uses with the character, summed over the packages' kinds, logged with what the queue holds.</summary>
    public int? UsesInQueue(IReadOnlyList<Package> packages)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();
            var actions = world?._ActionManager;

            if (role == null || actions == null) return null;

            var configIds = packages.Select(package => package.ConfigId).Distinct().ToArray();
            var inQueue = configIds.Sum(configId => actions.CountPendingItemUses(role.InstanceId, configId));

            _log($"after start: {inQueue} use(s) of config {string.Join(",", configIds)} with the character; {DescribeQueue(actions, role.InstanceId)}");

            return inQueue;
        }
        catch (Exception ex)
        {
            ReportOnce("queue count", ex);

            return null;
        }
    }

    /// <summary>The package kinds lined up since the last read, for the watch over the press that lined them up.</summary>
    public IReadOnlyCollection<int> LinedUpConfigs => _linedUpConfigs;

    /// <summary>
    /// One look for the watch over started unpacks: the uses of these kinds still with the character and the units of
    /// them in the bag. Null when the game cannot be read. The watch looks twice a second, so this builds no list and
    /// no log line; the kinds are package kinds already, so the bag is counted without asking which items unpack.
    /// </summary>
    public StartedLook? LookAtStarted(int[] configIds)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();
            var actions = world?._ActionManager;
            var items = world?._ItemManager;

            if (role == null || actions == null || items == null) return null;

            var roleId = role.InstanceId;
            var pending = 0;

            for (var i = 0; i < configIds.Length; i++) pending += actions.CountPendingItemUses(roleId, configIds[i]);

            return new StartedLook(pending, BagUnits(items, roleId, configIds));
        }
        catch (Exception ex)
        {
            ReportOnce("watch", ex);

            return null;
        }
    }

    /// <summary>The units of these kinds in the bag, counted the way ReadBag lists packages.</summary>
    private static int BagUnits(ItemManager items, long bagId, int[] configIds)
    {
        var held = items.GetItemDataList(bagId);
        var units = 0;

        for (var i = 0; held != null && i < held.Count; i++)
        {
            var item = held[i];

            if (item == null) continue;

            var count = item.ItemCount;

            if (count <= 0 || item.OwnerId != bagId || Array.IndexOf(configIds, item.ItemConfigId) < 0) continue;

            units += count;
        }

        return units;
    }

    /// <summary>What the character's queue holds, as a log line, read only when a line is written.</summary>
    public string QueueLine()
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();
            var actions = world?._ActionManager;

            return role == null || actions == null ? "queue: unreadable" : DescribeQueue(actions, role.InstanceId);
        }
        catch (Exception ex)
        {
            ReportOnce("queue description", ex);

            return "queue: unreadable";
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

        _linedUpConfigs.Clear();
        _storageOwners.Clear();
        _hasPressPoint = false;
        _pressPoint = null;

        if (world == null || config == null || agents == null || role == null || !IsAfterDisaster(world)) return new PackageRead.NoGame();

        var items = world._ItemManager;
        var actions = world._ActionManager;
        var drops = world._DropItemManager;

        if (items == null || actions == null || drops == null) return new PackageRead.NotReady(Texts.NoWorld);

        var build = AgentTools.GetAgentComponent<BuildComponent>(role);

        if (build == null) return new PackageRead.NotReady(Texts.NoHome);

        var bag = ReadBag(items, config, role.InstanceId);
        var floor = ReadFloor(drops, config);
        var storageAccess = ReadStorageAccess();
        var storages = storageAccess == StorageAccess.Off ? Array.Empty<Package>() : ReadStorages(agents, items, config);
        var freeSpots = PackageRules.FreeSpots(build.GetPurchaseCount(), build.GetMaxCapacity(), build.GetFreePackagePointCount());
        var alreadyQueued = bag.Concat(storages).Select(package => package.ConfigId).Distinct().Sum(configId => actions.CountPendingItemUses(role.InstanceId, configId));
        var queueOnStart = ReadQueueOnStart(role, actions);
        var isHome = IsHome(agents);

        _log(
            $"stock: {Units(bag)} in the bag, {Units(floor)} on the floor at home, {Units(storages)} in {_storageOwners.Values.Distinct().Count()} storage(s) ({storageAccess}), "
            + $"{freeSpots} free spots, {alreadyQueued} already lined up, start {queueOnStart}, at home {isHome}");

        return new PackageRead.Ready(new PackageStock(freeSpots, alreadyQueued, isHome, bag, floor, queueOnStart, storages, storageAccess));
    }

    /// <summary>
    /// False in the prologue, before the disaster. This is the game's own test (SceneRatManager.IsPostDisaster reads
    /// the same field), done here without the game's singleton lookup.
    /// </summary>
    internal static bool IsAfterDisaster(BattleLogicWorld world) => world._GameTimeManager?.WorldStateType == WorldStateType.PostDisaster;

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

        if (_reportedFailures.Add("floor id")) _log($"floor package {drop.InstanceId} (item {drop.Item.InstanceId}) is not found by either id; such packages are skipped (reported once)");

        return null;
    }

    private static bool IsSameDrop(DropItem? found, DropItem drop) => found != null && found.Pointer == drop.Pointer;

    /// <summary>
    /// Packages in the home's storages: every visible furniture with a bag on the home map, as the game's own list
    /// gives them (AgentManager.GetFurnituresWithBag), less the ones the cost pool leaves out (locked storages and the
    /// bins of a brew barrel, shredder, compressor, dye vat or greenhouse), the drone hub and drones, whose bags hold
    /// trade cargo, and the neighbour gift basket and the door box, which the game treats apart. A storage that cannot
    /// be read is skipped. Each package's storage is kept for the move.
    /// </summary>
    private IReadOnlyList<Package> ReadStorages(AgentManager agents, ItemManager items, ConfigManager config)
    {
        var found = agents.GetFurnituresWithBag(agents.GetHomeMapId(), true, true);
        var packages = new List<Package>();

        for (var i = 0; found != null && i < found.Count; i++)
        {
            var storage = found[i];

            if (storage == null || ItemManager.IsExcludedFromCostPool(storage.InstanceId) || IsDroneBay(storage) || IsSetApart(storage)) continue;

            foreach (var package in ReadBag(items, config, storage.InstanceId))
            {
                _storageOwners[package.ItemId] = storage.InstanceId;
                packages.Add(package);
            }
        }

        return packages;
    }

    private static bool IsDroneBay(Furniture furniture) =>
        StrangerTradeActiveManager.IsDroneHub(furniture.AgentConfigId) || StrangerTradeActiveManager.TripOf(furniture) != null;

    /// <summary>
    /// The neighbour gift basket, which the game keeps out of its own storage lists (ItemManager.IsNeighborBasketForbidden
    /// and Furniture.IsRobotContainerAllowed compare against NeighborRules.GetBasketFurnitureId, which is 0 when there
    /// is none), and the door box, whose bag the game handles apart (VehicleConfig.IsBoxOwner). The door box is a
    /// BagFurniture with its own owner id, so the list should never hold it; the check only makes sure.
    /// </summary>
    private static bool IsSetApart(Furniture furniture)
    {
        var basketId = NeighborRules.GetBasketFurnitureId();

        return (basketId > 0 && furniture.AgentConfigId == basketId) || VehicleConfig.IsBoxOwner(furniture.InstanceId);
    }

    /// <summary>
    /// Storages are only touched with the config on, and only moved from while no other window is open: a storage (the
    /// bag screen showing two bags), a workstation, shop or trade panel the container hotkeys know, or any window the
    /// Escape key would close. Moving items under such a window could leave it showing stacks that are gone. A check
    /// that fails moves nothing either, but says so apart from an open window.
    /// </summary>
    private StorageAccess ReadStorageAccess()
    {
        if (!IncludeStorages) return StorageAccess.Off;

        try
        {
            var ui = GameSingleton.Existing<ReduxUISystem>();

            if (ui == null)
            {
                LogOnce("window check ui", "the game's window system is not found, so storages are left alone (reported once)");

                return StorageAccess.Unknown;
            }

            var bagPanel = _bagPanel ??= Il2CppType.Of<WebUI_BackpackUI>();
            var otherPanels = _otherPanels ??= OtherPanels();

            if (ui.HasModalUIOpenExcept(bagPanel)) return StorageAccess.PanelOpen;

            for (var i = 0; i < otherPanels.Length; i++)
            {
                if (ui.IsUIActive(otherPanels[i])) return StorageAccess.PanelOpen;
            }

            if (!ui.IsUIActive(bagPanel)) return StorageAccess.Allowed;

            var bagScreen = ui.GetState<State_Web_BackpackUI>(_bagScreenState ??= Il2CppType.Of<State_Web_BackpackUI>());

            if (bagScreen == null)
            {
                LogOnce("window check bag", "the open bag screen's state is not found, so storages are left alone (reported once)");

                return StorageAccess.Unknown;
            }

            return bagScreen.IsDualBag ? StorageAccess.PanelOpen : StorageAccess.Allowed;
        }
        catch (Exception ex)
        {
            ReportOnce("window check", ex);

            return StorageAccess.Unknown;
        }
    }

    private static Il2CppSystem.Type[] OtherPanels() =>
        new[]
        {
            Il2CppType.Of<WebUI_Brew>(),
            Il2CppType.Of<WebUI_Compressor>(),
            Il2CppType.Of<WebUI_Cooking>(),
            Il2CppType.Of<WebUI_DroneHub>(),
            Il2CppType.Of<WebUI_Dye>(),
            Il2CppType.Of<WebUI_GreenhouseBuild>(),
            Il2CppType.Of<WebUI_MaterialRack>(),
            Il2CppType.Of<WebUI_RatCage>(),
            Il2CppType.Of<WebUI_ShopUI>(),
            Il2CppType.Of<WebUI_Shredder>(),
            Il2CppType.Of<WebUI_ToolTable>(),
            Il2CppType.Of<WebUI_TradeUI>(),
        };

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

    /// <summary>
    /// The nearest package point, found once per press: the press lines up all its unpacks in one frame, the character
    /// stands still meanwhile, and the walk over every point of the home map cost a game call per point per unpack.
    /// </summary>
    private Vector3? PressPoint(ConfigManager config, LeadingRole role)
    {
        if (_hasPressPoint) return _pressPoint;

        _pressPoint = NearestPackagePoint(config, role);
        _hasPressPoint = true;

        return _pressPoint;
    }

    /// <summary>
    /// The package point nearest the character, measured straight, as the Use button picks it. The game's map of
    /// package points lists every point of the home map, taken or not; the unpack places its furniture where the
    /// character stands when it runs, so the point is only where the character walks to. A character the game cannot
    /// place is measured from the origin, as the game does.
    /// </summary>
    private static Vector3? NearestPackagePoint(ConfigManager config, LeadingRole role)
    {
        var points = config.GetPackageNodeMap();

        if (points == null || points.Count == 0) return null;

        var from = AgentTools.GetAgentComponent<TransformComponent>(role)?.Position ?? Vector3.zero;
        Vector3? nearest = null;
        var nearestDistance = float.MaxValue;

        foreach (var point in points.Values)
        {
            var distance = (point - from).sqrMagnitude;

            if (distance >= nearestDistance) continue;

            nearest = point;
            nearestDistance = distance;
        }

        return nearest;
    }

    /// <summary>What the character's queue holds, for the log: the running action, the action lists waiting, and whether a walk comes next.</summary>
    private string DescribeQueue(ActionManager actions, long roleId)
    {
        try
        {
            return $"queue: running action {actions.GetCurrentActionIdForGm(roleId)}, {actions.GetQueueLengthForGm(roleId)} action list(s), next is a walk {actions.IsNextPendingActionMove(roleId)}";
        }
        catch (Exception ex)
        {
            ReportOnce("queue description", ex);

            return "queue: unreadable";
        }
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

    /// <summary>The exception's text is built only for the first report: a failure can repeat on every look of the watch.</summary>
    private void ReportOnce(string operation, Exception ex)
    {
        if (_reportedFailures.Add(operation)) _log($"{operation} failed (each kind reported once): {ex}");
    }

    private void LogOnce(string key, string message)
    {
        if (_reportedFailures.Add(key)) _log(message);
    }
}

/// <summary>One look at started unpacks: uses still with the character and units in the bag.</summary>
internal readonly record struct StartedLook(int Pending, int BagUnits);
