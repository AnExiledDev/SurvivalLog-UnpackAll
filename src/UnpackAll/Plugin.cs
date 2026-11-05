using System;
using System.Threading;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit;
using ExiledToolkit.Game;
using ExiledToolkit.Kit;
using HarmonyLib;
using UnityEngine;
using UnpackAll.Adapters;
using UnpackAll.Core;

namespace UnpackAll;

[BepInPlugin(Guid, "Unpack All", Version)]
[BepInDependency(ToolkitGuid, ToolkitVersion)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.unpackall";
    private const string Version = "0.9.1";

    /// <summary>Exiled's Toolkit: its shared helpers, the Mods menu, toasts and live settings all come from it.</summary>
    private const string ToolkitGuid = "com.dev.survivallog.exiledtoolkit";
    private const string ToolkitVersion = ">=0.19.0";

    private const string LogFileName = "unpack-all.log";

    public override void Load()
    {
        var unpackKey = Config.Bind("Controls", "UnpackKey", Enum.Parse<KeyCode>(KeyGate.DefaultKeyName), Settings.UnpackKey);
        var includeStorages = Config.Bind("Unpack", "IncludeStorages", false, Settings.IncludeStorages);
        var craftedToDoor = Config.Bind("Furniture", "CraftedToDoor", true, Settings.CraftedToDoor);
        var log = new ModLog(Log, LogFileName);
        var hooks = new PatchKit(new Harmony(Guid), log.Write);
        var game = new GamePackages(log.Write, log.WriteFileOnly);
        var session = new UnpackSession(game, new GamePickUp(log.Write), new WebPage(log.Write), log.Write, log.WriteFileOnly);

        log.Write($"===== Unpack All {Version} started");
        CraftedDelivery.Apply(hooks, log.WriteFileOnly);
        MenuOpening.Apply(hooks, session.MenuOpened);
        LiveSettings.Watch(unpackKey, key =>
        {
            session.UseKey(key);
            log.Write($"unpack key {key}");
        });
        LiveSettings.Watch(includeStorages, isOn =>
        {
            game.IncludeStorages = isOn;
            log.Write($"storages {(isOn ? "on" : "off")}");
        });
        LiveSettings.Watch(craftedToDoor, isOn =>
        {
            CraftedDelivery.IsOn = isOn;
            log.Write($"crafted furniture to the entryway {(isOn ? "on" : "off")}");
        });
        UnpackTicker.Session = session;
        UnpackTicker.Log = log.Write;
        AddComponent<UnpackTicker>();
        StartWarmUp();
    }

    /// <summary>
    /// Compiles the press path on a worker, so the first press does not pay for it on the game's frame. The warm-up is
    /// pure .NET and touches nothing of the game; a failure only means the first press compiles as before.
    /// </summary>
    private static void StartWarmUp()
    {
        new Thread(() =>
        {
            try
            {
                WarmUp.Run();
            }
            catch (Exception)
            {
                // Nothing to report: the press itself still works, it only compiles on first use.
            }
        }) { IsBackground = true, Name = "UnpackAll warm-up" }.Start();
    }
}
