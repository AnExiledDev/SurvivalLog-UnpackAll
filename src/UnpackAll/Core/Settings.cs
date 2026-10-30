namespace UnpackAll.Core;

/// <summary>The descriptions of the mod's settings: plain English, then Simplified Chinese.</summary>
public static class Settings
{
    public const string UnpackKey =
        "Key that unpacks the packages in your bag first, then the ones on the floor at home, up to your free package spots. If IncludeStorages is true, packages in storages at home are unpacked last. Unbound by default: use Unpack all in the inventory's Mods menu, or set a key here. The key does nothing while you are typing in a text box.\n" +
        "按下此键先拆封背包里的包裹，再拆封家里地上的包裹，数量以空余的包裹位为限。IncludeStorages 设为 true 时，最后还会拆封家里储物家具中的包裹。默认不绑定按键：请使用背包界面 Mods 菜单里的「全部拆封」，或在此设置一个按键。在文本框中输入文字时，此键不起作用。";

    public const string IncludeStorages =
        "When true, packages in storages at home are unpacked too, after the bag and the floor. A storage stack is moved into your bag only if all of it fits your free package spots, and nothing is taken while a storage, workbench, shop or trade window is open. False, the default, leaves storages alone.\n" +
        "设为 true 时，拆完背包和家里地上的包裹后，还会拆封家里储物家具中的包裹。储物家具里的一组包裹只有能全部放进空余的包裹位时，才会整组移进背包；开着储物、工作台、商店或交易界面时不会取出任何东西。设为 false（默认）则不动储物家具。";

    public const string CraftedToDoor =
        "When true, every furniture package you craft at a workbench is sent to the entryway at once, as the result window's Unpack to Entryway button does. A package that an open Aid Platform commission asks for stays a package, and when the entryway is full the rest stay packages in your bag. False leaves crafted packages in your bag.\n" +
        "设为 true 时，在工作台做好的家具包裹会立刻送到玄关，和结果界面的「拆到玄关」按钮一样。援助平台上未完成的委托需要的包裹会保留为包裹；玄关放满后，其余的包裹留在背包里。设为 false 则做好的包裹留在背包里。";
}
