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
    private const string Version = "0.2.0";

    public override void Load()
    {
        var unpackKey = Config.Bind("Controls", "UnpackKey", KeyCode.F9, "Key that unpacks every package in your bag, then the ones on the floor at home and, if IncludeStorages is on, the ones in storages at home, up to your free package spots. None turns the key off; Unpack all in the inventory's Mods menu still works.").Value;
        var includeStorages = Config.Bind("Unpack", "IncludeStorages", true, "Also unpack packages kept in storages at home you can see, after the bag and the floor. They are moved into your bag first, the same as taking them out by hand, and only a stack that fits your free package spots; a bigger stack stays in its storage. Nothing is taken out while a storage, workbench or shop window is open. False leaves storages alone.").Value;
        var log = new UnpackLog(Log, Path.Combine(Paths.BepInExRootPath, "unpack-all.log"));
        var harmony = new Harmony(Guid);

        log.Write($"===== Unpack All {Version} started, key {unpackKey}, storages {(includeStorages ? "on" : "off")}");
        PageMessages.Apply(harmony, log.Write);
        HotkeyGate.Apply(harmony, log.Write);
        UnpackTicker.Session = new UnpackSession(new GamePackages(log.Write, includeStorages), new WebPage(log.Write), unpackKey, log.Write);
        UnpackTicker.Log = log.Write;
        AddComponent<UnpackTicker>();
    }
}
