# Unpack All settings

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Edit it while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `UnpackKey` | `F9` | Key that unpacks every package in your bag, then the ones on the floor at home, then the ones in your storages at home, up to your free package spots. `None` turns the key off; Unpack all in the inventory's Mods menu still works. |

The value is a Unity key name, for example `F9`, `F11`, `Insert` or `Keypad5`. With `None`, the Mods menu entry shows no key.

## Unpack

| Setting | Default | What it does |
| --- | --- | --- |
| `IncludeStorages` | `true` | Also unpack packages kept in storages at home, after the bag and the floor. They are moved into your bag first, the same as taking them out by hand, and only while you have free package spots. Nothing is taken out while a storage, workbench or shop window is open. `false` leaves storages alone. |

Locked storages, the drone hub and drones, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are never touched. A storage package that is taken out but not unpacked (because the stack was bigger than your free spots) waits in your bag.
