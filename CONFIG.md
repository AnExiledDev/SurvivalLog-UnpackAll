# Unpack All settings

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Edit it while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `UnpackKey` | `F9` | Key that unpacks every package in your bag, then the ones on the floor at home, up to your free package spots. `None` turns the key off; Unpack all in the inventory's Mods menu still works. |

The value is a Unity key name, for example `F9`, `F11`, `Insert` or `Keypad5`. With `None`, the Mods menu entry shows no key.
