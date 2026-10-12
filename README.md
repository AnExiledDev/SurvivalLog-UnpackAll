# Unpack All

One press unpacks every furniture package in your bag, on the floor at home and in your storages at home.

Unpacking furniture packages one at a time is slow, and lining up more unpacks than your home has room for fills the action queue with unpacks that fail. Unpack All unpacks every package in your bag, then picks up the packages lying on the floor at home, then takes packages out of your storages at home, and unpacks those too, as far as your home has free package spots.

## Features

- **Bag first, then the floor, then storages**: packages in your bag are unpacked first. If spots are left over, packages on the floor inside your home are picked up into your bag and unpacked as well. If spots are still left after that, packages in your storages at home are moved into your bag and unpacked. This last step can be turned off in the config.
- **Never more than fits**: only as many unpacks are lined up as your home has free package spots, so every one of them can run and the queue bar never fills with unpacks that would fail. The rest stay where they are.
- **The game's own Use action**: each unpack is the same action the inventory's Use button lines up, so the game's own checks and timings apply. Like the Use button, it sends your character to the package point nearest them first.
- **Your queued actions stay**: unpacks join the end of your character's action queue, so actions you queued yourself run first.
- **Clear messages**: a short message at the bottom of the screen says how many packages are unpacking, how many were picked up from the floor or taken from storages (and how many of those wait in your bag), and how many were left and why: no free package spot, your bag could not carry it, you are away from home, a window is open, or the game's own reason for refusing.
- **Follows up**: after starting, the mod checks that the unpacks really reached your character's queue and watches them until they are done. An unpack the game drops is never counted as unpacking: the message says how many were dropped, and a later message says so if an unpack ends without opening its package.
- **Mods menu entry**: **Unpack all** in the inventory's Mods menu does the same as the key (see below).
- **Log file**: every press, what was read and lined up, and how each batch ended, is written to `BepInEx\unpack-all.log`.

Unpack All adds no data of its own to your save. The unpacks are real: packages come off the floor or out of your storages into your bag and are unpacked into furniture, exactly as if you had taken each one out and used it yourself. You can remove the mod at any time; packages and furniture stay as they are.

## Hotkeys

| Key | What it does |
| --- | --- |
| `F9` | Unpacks every package in your bag, then the ones on the floor at home, then the ones in your storages at home, up to your free package spots. |

`F9` works anywhere in a loaded game past the prologue. On the title screen and in the prologue it does nothing. It can be changed or turned off in the config.

## The Mods menu

The mod adds a **Mods** button (a four-square icon) to the toolbar on the bag side of the inventory screen, between the game's discard and sort buttons. Click it to open a small menu with **Unpack all**, which shows the unpack key next to it and does the same as the key. Esc or a click anywhere else closes the menu, and the game's own hotkeys are held back while it is open so Esc closes only the menu.

The Unpack all entry only shows on the bag's item tab, and not in the prologue. Other mods that use the same Mods menu list their own entries in it, so you get one button instead of one per mod; when no mod has an entry to show, the button is taken away.

The mod's menu entry and messages are in English. The game's own refusal reasons are shown as the game words them, which can be Chinese.

## How it works

Each press reads your bag, the floor at home, your storages at home and your home's free package spots. Packages in the bag get spots first. Then, if you are at home, floor packages are picked up while spots are left, and the ones that arrive in your bag get the remaining spots. Then storage packages are moved into your bag while spots are still left, the same way a click moves them with the storage open, and they get the spots that remain. Wave loot on the floor is left for the game's drone.

Storages means every storage at home the game lets you build and craft from. Locked storages, the drone hub and drones, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are left alone. A package the game would refuse to unpack stays in its storage.

Nothing is taken out of a storage while a storage, workbench, shop or trade window is open; close it and press again. Your own bag being open is fine.

While unpacks from an earlier press are still lined up, another press does nothing but say so: their spots are only freed as each one runs, so a second batch would have nowhere to go.

## Known issues and limits

This mod picks up items from the floor and unpacks them. Back up your save before trying it for the first time.

- Packages beyond your free package spots stay in your bag, on the floor or in your storages. Place some furniture to free spots, then press again.
- Floor and storage packages are only taken while you are at home.
- Storage packages are taken only after the bag and the floor, so they never take a spot a floor package could use.
- The game moves a storage stack whole, like a floor stack. When packages stack and a storage stack holds more packages than the spots left, all of it comes into your bag, and the extra ones wait there for the next press.
- If your bag is too heavy or full for a storage stack, it stays in its storage and the message says so.
- The game picks up a floor stack whole. When packages stack and a floor stack holds more packages than the spots left, all of it comes into your bag: as many as there are spots unpack, and the message says how many wait in your bag for the next press.
- A floor package the game refuses to unpack once it is in your bag stays in your bag; its spot goes to another package on the next press, not this one.
- Starting a batch never throws away actions you queued. If the game is ever in its replace-the-queue action mode while you have actions queued, a press does nothing and says "Finish or cancel your queued actions first."
- Colour and other formatting tags in the game's refusal reasons are removed.
- `BepInEx\unpack-all.log` is never trimmed. Delete it while the game is closed if it gets large.

If a package goes missing, an unpack does not run, or the message does not match what happened, please tell me (see [Feedback](#feedback-and-bug-reports)).

## Requirements

- Survival Log 1.1.18317. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Download `UnpackAll.dll` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-UnpackAll/releases/latest) and put it in the `BepInEx\plugins` folder inside the game folder (the folder with the game .exe). Full path example: `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\UnpackAll.dll`

## Uninstall

Delete `UnpackAll.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.unpackall.cfg` and `BepInEx\unpack-all.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Edit it while the game is closed; changes apply at the next game start. See [CONFIG.md](CONFIG.md) for every setting.

## Troubleshooting

Tested on Survival Log 1.1.18317 with BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Unpack All 0.2.0 started, key F9, storages on`. The same line, plus every press (what was in the bag, on the floor and in storages, the free spots, each unpack lined up) and how each batch ended, is in `BepInEx\unpack-all.log`, which survives restarts.

If Unpack all does not show in the Mods menu, make sure the inventory's item tab is open on the bag side. If a press says it cannot unpack right now, the rest of the message and the log say why. If storage packages are not taken, check that no storage or workbench window is open and that `IncludeStorages` is `true` in the config.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also leave a comment on the Nexus page or [open an issue](https://github.com/AnExiledDev/SurvivalLog-UnpackAll/issues). When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\unpack-all.log`.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/UnpackAll -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\UnpackAll\bin\Release\UnpackAll.dll`.

The unit tests cover the pure rules (what a package is, how spots are handed out across the bag, the floor and storages, the follow-up watch and the messages) and need nothing from the game:

```
dotnet test tests/UnpackAll.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
