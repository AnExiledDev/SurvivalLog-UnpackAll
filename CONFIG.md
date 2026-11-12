# Unpack All settings

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `UnpackKey` | `None` | A key that unpacks the packages in your bag, then the ones on the floor at home, as many as you have free package spots for. If `IncludeStorages` is `true`, packages in your storages at home go last. No key is set by default, since Unpack all in the inventory's Mods menu does the same thing. The key does nothing while you are typing in a text box. |

The value is a Unity key name, for example `F11`, `Insert` or `Keypad5`. With `None`, the default, there is no key and the Mods menu entry shows no key next to it.

## Unpack

| Setting | Default | What it does |
| --- | --- | --- |
| `IncludeStorages` | `false` | When `true`, packages in your storages at home are unpacked too, after the bag and the floor. A stack only comes out of a storage if your free package spots can take the whole stack. Nothing is taken while a storage, workbench, shop or trade window is open. `false`, the default, leaves your storages alone. |

Locked storages, the drone hub and drones, the neighbour gift basket, the door box, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are never touched. A storage stack that holds more packages than your free spots stays in its storage until you have room for all of it.

## Furniture

| Setting | Default | What it does |
| --- | --- | --- |
| `CraftedToDoor` | `false` | When `true`, every furniture package you craft at a workbench goes straight to the entryway, like the result window's Unpack to Entryway button. A package that an open Aid Platform commission asks for stays a package. When the entryway is full, the rest stay in your bag. `false`, the default, leaves crafted packages in your bag. |

Only Aid Platform commissions are checked. Stranger requests and casual orders are not, so turn this off if you craft furniture to hand in for those.

## Pick up

Pick up, in the click menu of a furniture package in the yard, has no setting and no key.
