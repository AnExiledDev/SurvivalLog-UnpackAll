# Unpack All settings

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `UnpackKey` | `F9` | Key that unpacks the packages in your bag first, then the ones on the floor at home, up to your free package spots. If `IncludeStorages` is `true`, packages in storages at home are unpacked last. `None` turns the key off, but Unpack all in the inventory's Mods menu still works. |

The value is a Unity key name, for example `F9`, `F11`, `Insert` or `Keypad5`. With `None`, the Mods menu entry shows no key.

## Unpack

| Setting | Default | What it does |
| --- | --- | --- |
| `IncludeStorages` | `false` | When `true`, packages in storages at home are unpacked too, after the bag and the floor. A storage stack is moved into your bag only if all of it fits your free package spots, and nothing is taken while a storage, workbench, shop or trade window is open. `false`, the default, leaves storages alone. |

Locked storages, the drone hub and drones, the neighbour gift basket, the door box, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are never touched. A storage stack that holds more packages than your free spots stays in its storage until you have room for all of it.
