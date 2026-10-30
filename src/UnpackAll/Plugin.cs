using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnpackAll.Adapters;
using UnpackAll.Core;

namespace UnpackAll;

[BepInPlugin(Guid, "Unpack All", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, OptionalLive.ToolkitVersion)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.unpackall";
    private const string Version = "0.7.0";

    public override void Load()
    {
        var unpackKey = Config.Bind("Controls", "UnpackKey", Enum.Parse<KeyCode>(KeyGate.DefaultKeyName), Settings.UnpackKey);
        var includeStorages = Config.Bind("Unpack", "IncludeStorages", false, Settings.IncludeStorages);
        var craftedToDoor = Config.Bind("Furniture", "CraftedToDoor", true, Settings.CraftedToDoor);
        var log = new UnpackLog(Log, Path.Combine(Paths.BepInExRootPath, "unpack-all.log"));
        var harmony = new Harmony(Guid);
        var game = new GamePackages(log.Write, log.Detail);
        var session = new UnpackSession(game, new WebPage(log.Write), log.Write, log.Detail);

        log.Write($"===== Unpack All {Version} started");
        PageMessages.Apply(harmony, log.Write);
        HotkeyGate.Apply(harmony, log.Write);
        CraftedDelivery.Apply(harmony, log.Write, log.Detail);
        OptionalLive.Watch(unpackKey, key =>
        {
            session.UseKey(key);
            log.Write($"unpack key {key}");
        });
        OptionalLive.Watch(includeStorages, isOn =>
        {
            game.IncludeStorages = isOn;
            log.Write($"storages {(isOn ? "on" : "off")}");
        });
        OptionalLive.Watch(craftedToDoor, isOn =>
        {
            CraftedDelivery.IsOn = isOn;
            log.Write($"crafted furniture to the entryway {(isOn ? "on" : "off")}");
        });
        UnpackTicker.Session = session;
        UnpackTicker.Log = log.Write;
        UnpackTicker.Quit = log.Finish;
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
