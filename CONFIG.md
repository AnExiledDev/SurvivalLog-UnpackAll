# Unpack All settings

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `UnpackKey` | `None` | Key that unpacks the packages in your bag first, then the ones on the floor at home, up to your free package spots. If `IncludeStorages` is `true`, packages in storages at home are unpacked last. Unbound by default: use Unpack all in the inventory's Mods menu, or set a key here. The key does nothing while you are typing in a text box. |

The value is a Unity key name, for example `F11`, `Insert` or `Keypad5`. With `None`, the default, there is no key and the Mods menu entry shows none.

## Unpack

| Setting | Default | What it does |
| --- | --- | --- |
| `IncludeStorages` | `false` | When `true`, packages in storages at home are unpacked too, after the bag and the floor. A storage stack is moved into your bag only if all of it fits your free package spots, and nothing is taken while a storage, workbench, shop or trade window is open. `false`, the default, leaves storages alone. |

Locked storages, the drone hub and drones, the neighbour gift basket, the door box, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are never touched. A storage stack that holds more packages than your free spots stays in its storage until you have room for all of it.

## Furniture

| Setting | Default | What it does |
| --- | --- | --- |
| `CraftedToDoor` | `true` | When `true`, every furniture package you craft at a workbench is sent to the entryway at once, as the result window's Unpack to Entryway button does. A package that an open Aid Platform commission asks for stays a package, and when the entryway is full the rest stay packages in your bag. `false` leaves crafted packages in your bag. |

Only Aid Platform commissions are checked. Stranger requests and casual orders are not, so turn this off if you craft furniture to hand in for those.
