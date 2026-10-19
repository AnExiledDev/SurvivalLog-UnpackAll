using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnpackAll.Adapters;

namespace UnpackAll;

[BepInPlugin(Guid, "Unpack All", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.unpackall";
    private const string Version = "0.4.0";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var unpackKey = Config.Bind("Controls", "UnpackKey", KeyCode.F9,
            "Key that unpacks the packages in your bag first, then the ones on the floor at home, up to your free package spots. If IncludeStorages is true, packages in storages at home are unpacked last. None turns the key off, but Unpack all in the inventory's Mods menu still works.\n" +
            "按下此键先拆封背包里的包裹，再拆封家里地上的包裹，数量以空余的包裹位为限。IncludeStorages 设为 true 时，最后还会拆封家里储物家具中的包裹。设为 None 可关闭此键，背包界面 Mods 菜单里的「全部拆封」仍可使用。");
        var includeStorages = Config.Bind("Unpack", "IncludeStorages", false,
            "When true, packages in storages at home are unpacked too, after the bag and the floor. A storage stack is moved into your bag only if all of it fits your free package spots, and nothing is taken while a storage, workbench, shop or trade window is open. False, the default, leaves storages alone.\n" +
            "设为 true 时，拆完背包和家里地上的包裹后，还会拆封家里储物家具中的包裹。储物家具里的一组包裹只有能全部放进空余的包裹位时，才会整组移进背包；开着储物、工作台、商店或交易界面时不会取出任何东西。设为 false（默认）则不动储物家具。");
        var log = new UnpackLog(Log, Path.Combine(Paths.BepInExRootPath, "unpack-all.log"));
        var harmony = new Harmony(Guid);
        var game = new GamePackages(log.Write);
        var session = new UnpackSession(game, new WebPage(log.Write), log.Write);

        log.Write($"===== Unpack All {Version} started");
        PageMessages.Apply(harmony, log.Write);
        HotkeyGate.Apply(harmony, log.Write);
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
        UnpackTicker.Session = session;
        UnpackTicker.Log = log.Write;
        AddComponent<UnpackTicker>();
    }
}
