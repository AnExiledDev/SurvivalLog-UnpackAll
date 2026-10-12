using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnpackAll.Adapters;

namespace UnpackAll;

[BepInPlugin(Guid, "Unpack All", Version)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.unpackall";
    private const string Version = "0.1.1";

    public override void Load()
    {
        var unpackKey = Config.Bind("Controls", "UnpackKey", KeyCode.F9, "Key that unpacks every package in your bag, then the ones on the floor at home, up to your free package spots. None turns the key off; Unpack all in the inventory's Mods menu still works.").Value;
        var log = new UnpackLog(Log, Path.Combine(Paths.BepInExRootPath, "unpack-all.log"));
        var harmony = new Harmony(Guid);

        log.Write($"===== Unpack All {Version} started, key {unpackKey}");
        PageMessages.Apply(harmony, log.Write);
        HotkeyGate.Apply(harmony, log.Write);
        UnpackTicker.Session = new UnpackSession(new GamePackages(log.Write), new WebPage(log.Write), unpackKey, log.Write);
        UnpackTicker.Log = log.Write;
        AddComponent<UnpackTicker>();
    }
}
