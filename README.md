# Unpack All

A BepInEx 6 (IL2CPP) plugin for Survival Log: one press unpacks every furniture package in your bag, then picks up the packages lying on the floor at home and unpacks those too, as far as your home has free package spots.

## What it does

- Packages in your bag are unpacked first. If spots are left over, packages on the floor inside your home are picked up into your bag and unpacked as well.
- Only as many unpacks are lined up as your home has free package spots, so every one of them runs and the queue bar never fills with unpacks that would fail. The rest stay where they are.
- Each unpack goes through the game's own Use action for that package, the same one the inventory's Use button lines up, so the game's own checks and timings apply. Like the Use button, it sends your character to the package point nearest them first.
- After starting, the mod checks that the unpacks really reached your character's queue and follows them until they are done. An unpack the game drops is never counted as unpacking: the message says how many were dropped, and a later line says so if an unpack ends without opening its package. The details go to the log.
- A short message at the bottom of the screen says how many packages are unpacking, how many were picked up from the floor (and how many of those wait in your bag), and how many were left and why (no free package spot, your bag could not carry it, you are away from home, or the game's own reason for refusing).

## Controls

- **Unpack all** in the Mods menu on the inventory screen's item tab, the button next to the game's Sort button. Other mods that use the same menu (Storage Hub does) list their actions there too. Esc or a click elsewhere closes it.
- `F9` anywhere in a loaded game does the same. On the title screen and in the prologue it does nothing, and the menu entry is not shown there either.

Unpacks join the end of your character's action queue, so actions you queued yourself (taking adrenaline, say) stay and run first. While unpacks from an earlier press are still lined up, another press does nothing but say so: their spots are only freed as each one runs, so a second batch would have nowhere to go.

## Config

`BepInEx/config/com.dev.survivallog.unpackall.cfg`:

| Setting | Default | What it does |
| --- | --- | --- |
| `Controls/UnpackKey` | `F9` | Key that does the same as Unpack all in the Mods menu, which shows the key next to the entry. `None` turns the key off; the menu entry still works. |

Log: `BepInEx/unpack-all.log`.

## Known limits

- Packages beyond your free package spots stay in your bag or on the floor. Place some furniture to free spots, then press again.
- Floor packages are only picked up while you are at home.
- Packages inside storages are not touched, only your bag and the floor at home.
- Starting the batch never throws away actions you queued. If the game is ever in its replace-the-queue action mode (it uses it before the disaster, where the mod stays off), a press while you have actions queued does nothing and says to finish or cancel them first.
- The game picks up a floor stack whole. When packages stack and a floor stack holds more packages than the spots left, all of it comes into your bag: as many as there are spots unpack, and the message says how many wait in your bag for the next press.
- A floor package the game refuses to unpack once it is in your bag stays in your bag; its spot goes to another floor package on the next press, not this one.
- The game's refusal reasons are shown as the game words them, which can be Chinese. Colour and other rich-text tags in them are removed.
- The message sits where Storage Hub's does; if both show at once, the newer one covers the other.

## Install

Copy `UnpackAll.dll` into `BepInEx/plugins`. Needs BepInEx 6 (IL2CPP build).

## Build and test

```sh
dotnet build src/UnpackAll -c Release -p:GameDir="D:\path\to\Survival Log"
dotnet test tests/UnpackAll.Tests
```

The build references BepInEx and the game's interop assemblies under `GameDir`, so BepInEx must be installed and the game launched once. The tests compile only the pure `Core` code and need nothing from the game.

## License

MIT, see [LICENSE](LICENSE).
