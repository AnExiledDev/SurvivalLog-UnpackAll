using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// Sends crafted furniture packages to the entryway as soon as the workbench makes them. ToolTableManager.TryCreateItem
/// collects a craft's new packages with the static CollectNewItemIds into the result window's list; the hook runs after
/// that, hands the chosen ids to the game's own UnpackPackagesToEntrance (what the result window's 「拆到玄关」 button
/// calls) and takes the delivered ones off the list, so the window offers its button only for what is left. The
/// target's first 15 bytes are three register saves to the stack, with no relative jump or call. Static because a
/// Harmony postfix has nowhere else to look; it runs on the game's main thread.
/// </summary>
internal static class CraftedDelivery
{
    /// <summary>AidRequestSave.State of a commission that is finished (AidPlatformManager.CountUnfinished skips it).</summary>
    private const int DoneState = 3;

    private static readonly List<CraftedPackage> Crafted = new();
    private static readonly List<long> Send = new();
    private static readonly Func<int, bool> IsWanted = IsWantedByCommission;

    private static Action<string> _log = _ => { };
    private static Action<string> _detail = _ => { };
    private static bool _hasReportedFailure;
    private static int _sentSinceLastLook;

    public static volatile bool IsOn = true;

    public static void Apply(Harmony harmony, Action<string> log, Action<string> detail)
    {
        _log = log;
        _detail = detail;

        try
        {
            harmony.Patch(AccessTools.Method(typeof(ToolTableManager), nameof(ToolTableManager.CollectNewItemIds)), postfix: new HarmonyMethod(typeof(CraftedDelivery), nameof(AfterCollect)));
            log("crafted furniture packages can go to the entryway");
        }
        catch (Exception ex)
        {
            log("could not hook ToolTableManager.CollectNewItemIds, crafted furniture stays in your bag: " + ex.Message);
        }
    }

    /// <summary>How many packages were sent since the last call, for the toast.</summary>
    public static int TakeSent()
    {
        var sent = _sentSinceLastLook;

        _sentSinceLastLook = 0;

        return sent;
    }

    /// <summary>Never throws into the craft: on any failure the packages stay in the bag as if the mod were not here.</summary>
    private static void AfterCollect(int itemConfigId, Il2CppSystem.Collections.Generic.List<long> result)
    {
        if (!IsOn || result == null || result.Count == 0) return;

        try
        {
            Deliver(itemConfigId, result);
        }
        catch (Exception ex)
        {
            if (_hasReportedFailure) return;

            _hasReportedFailure = true;
            _log("sending crafted furniture to the entryway failed, the packages stay in the bag (reported once): " + ex);
        }
    }

    private static void Deliver(int itemConfigId, Il2CppSystem.Collections.Generic.List<long> result)
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var items = world?._ItemManager;
        var table = world?._ToolTableManager;

        if (items == null || table == null) return;

        // The list holds every package of this craft so far; one of an earlier product may already be gone.
        Crafted.Clear();

        for (var i = 0; i < result.Count; i++)
        {
            var id = result[i];
            var item = items.GetItemData(id);

            if (item != null && ToolTableManager.IsFurniturePackageItem(item.ItemConfigId)) Crafted.Add(new CraftedPackage(id, item.ItemConfigId));
        }

        var room = table.GetEntranceRoom();

        CraftedToDoor.Pick(IsOn, Crafted, IsWanted, room, Send);

        if (Send.Count == 0)
        {
            _detail($"crafted config {itemConfigId}: {Crafted.Count} package(s), entryway room {room}, none sent");

            return;
        }

        var ids = new Il2CppSystem.Collections.Generic.List<long>();

        foreach (var id in Send) ids.Add(id);

        var delivered = table.UnpackPackagesToEntrance(ids);
        var gone = 0;

        // A delivered package is removed from the game; whatever is still there stays on the window's list.
        foreach (var id in Send)
        {
            if (items.GetItemData(id) != null) continue;

            result.Remove(id);
            gone++;
        }

        _sentSinceLastLook += gone;
        _detail($"crafted config {itemConfigId}: {Crafted.Count} package(s), entryway room {room}, sent {Send.Count}, game delivered {delivered}, {gone} left the bag");
    }

    /// <summary>
    /// Whether an open Aid Platform commission still asks for this package. The game keeps no quest mark on an item,
    /// so this reads the commissions themselves and matches with the game's own check, which also counts dyed and
    /// undyed versions of the same furniture.
    /// </summary>
    private static bool IsWantedByCommission(int itemConfigId)
    {
        var requests = GameSingleton.Existing<BattleLogicWorld>()?._AidPlatformManager?.data?.Requests;

        if (requests == null) return false;

        for (var i = 0; i < requests.Count; i++)
        {
            var request = requests[i];

            if (request == null || request.State == DoneState || request.Demands == null) continue;

            var demands = request.Demands;

            for (var j = 0; j < demands.Count; j++)
            {
                var demand = demands[j];

                if (demand != null && demand.ItemId > 0 && demand.Paid < demand.Need && AidPlatformManager.IsDemandItemMatch(demand.ItemId, itemConfigId)) return true;
            }
        }

        return false;
    }
}
