# Unpack All

One click unpacks every furniture package in your bag and on the floor at home, and in your storages at home if you turn that on. Furniture you craft goes straight to the entryway.

Unpacking furniture packages one at a time is slow, and lining up more unpacks than your home has room for fills the action queue with unpacks that fail. Unpack All unpacks every package in your bag, then picks up the packages lying on the floor at home and unpacks those too, as far as your home has free package spots. With `IncludeStorages` turned on in the config, it then takes packages out of your storages at home and unpacks those as well.

## Features

- **Bag first, then the floor, then storages**: packages in your bag are unpacked first. If spots are left over, packages on the floor inside your home are picked up into your bag and unpacked as well. If spots are still left after that and `IncludeStorages` is on, packages in the storages at home you can see are moved into your bag and unpacked, as long as their stack fits the spots left. This last step is off until you turn it on in the config.
- **Never more than fits**: only as many unpacks are lined up as your home has free package spots, so every one of them can run and the queue bar never fills with unpacks that would fail. The rest stay where they are.
- **The game's own Use action**: each unpack is the same action the inventory's Use button lines up, so the game's own checks and timings apply. Like the Use button, it sends your character to the package point nearest them first.
- **Your queued actions stay**: unpacks join the end of your character's action queue, so actions you queued yourself run first.
- **Clear messages**: a short message at the bottom of the screen says how many packages are unpacking, how many were picked up from the floor or taken from storages (and how many of those wait in your bag), and how many were left and why: no free package spot, a storage stack bigger than your free spots, your bag could not carry it, you are away from home, a window is open (or could not be checked), or the game's own reason for refusing.
- **Follows up**: after starting, the mod checks that the unpacks really reached your character's queue and watches them until they are done. An unpack the game drops is never counted as unpacking: the message says how many were dropped, and a later message says so if an unpack ends without opening its package.
- **Crafted furniture to the entryway**: furniture packages you craft at a workbench go straight to the entryway, as if you had clicked the result window's Unpack to Entryway button. A package that an open Aid Platform commission asks for stays a package so you can hand it in, and when the entryway is full the rest stay packages in your bag (the result window still offers its button for them). A short message says how many were sent. `CraftedToDoor` in the config turns this off.
- **Pick up yard packages**: click a furniture package in the yard and choose **Pick up** to put it back in your bag, for example to hand it in for an Aid Platform commission. If your bag can't carry it, it goes into a shopping bag in the living room. A bought package can no longer be returned to the shop after you pick it up.
- **Mods menu entry**: **Unpack all** in the inventory's Mods menu starts an unpack (see below).
- **Log file**: every press, what was read and lined up, and how each batch ended, is written to `BepInEx\unpack-all.log`.

Unpack All adds no data of its own to your save. The unpacks are real: packages come off the floor or out of your storages into your bag and are unpacked into furniture, exactly as if you had taken each one out and used it yourself. You can remove the mod at any time; packages and furniture stay as they are.

## Hotkeys

No key is set by default: **Unpack all** in the Mods menu does the unpacking. To unpack with a key as well, set `UnpackKey` in the config. A set key works anywhere in a loaded game past the prologue, but not on the title screen, in the prologue, or while you are typing in a text box.

## The Mods menu

The mod adds a **Mods** button (a four-square icon) to the toolbar on the bag side of the inventory screen, between the game's discard and sort buttons. Click it to open a small menu with **Unpack all**, which starts an unpack (and shows the unpack key next to it, if you set one). Esc or a click anywhere else closes the menu, and the game's own hotkeys are held back while it is open so Esc closes only the menu.

On the inventory the Unpack all entry only shows on the bag's item tab, and not in the prologue. The same Mods button is also in the bag panel of the workbench, stove, ration press, greenhouse, drone hub, brewing barrel, dye vat, rat cage, material rack and shop screens (not the shredder or the drone trade); there it unpacks only what is in your bag, since the floor and storages are left alone while a station screen is open. Other mods that use the same Mods menu list their own entries in it, so you get one button instead of one per mod; when no mod has an entry to show, the button is taken away.

The Unpack all entry, its tooltip and the messages follow the game's language, English or Simplified Chinese, and switch within a second when you change it. The shared Mods button and its tooltip stay in English. The game's own refusal reasons are shown as the game words them.

## How it works

Each press reads your bag, the floor at home, your storages at home (when `IncludeStorages` is on) and your home's free package spots. Packages in the bag get spots first. Then, if you are at home, floor packages are picked up while spots are left, and the ones that arrive in your bag get the remaining spots. Then, with `IncludeStorages` on, the storage stacks that together fill the most of the spots still left are moved into your bag, whole, the same way a click moves them with the storage open, and they get the spots that remain. Wave loot on the floor is left for the game's drone.

Storages means the storages at home you can see. Locked storages, the drone hub and drones, the neighbour gift basket, the door box, and the bins of a brew barrel, shredder, compressor, dye vat or greenhouse are left alone. A package the game would refuse to unpack stays in its storage.

Nothing is taken out of a storage while a storage, workbench, shop or trade window is open; close it and press again. Your own bag being open is fine. If the mod cannot tell which windows are open, it takes nothing out of storages and the message says to see the log.

While unpacks from an earlier press are still lined up, another press does nothing but say so: their spots are only freed as each one runs, so a second batch would have nowhere to go.

## Crafted furniture

When a craft at a workbench makes furniture packages, the mod hands them to the game's own Unpack to Entryway, the same as the result window's button, as many as the entryway has free spots for. The game keeps no quest mark on a package, so the mod checks the Aid Platform commissions that are not finished: a package one of them still asks for (dyed or not) stays a package. Stranger requests and casual orders are not checked; turn `CraftedToDoor` off if you craft furniture for those.

## Pick up

Clicking a furniture package standing in the yard opens the game's menu for it, with Install and Destroy. The mod adds **Pick up** to that menu. It puts the package back in your bag as the same item, with the same colour, so you can hand it in, sell it or install it somewhere else later.

- If your bag is too full or too heavy for it, the package goes into one of the shopping bags in the living room, the same ones the shop fills when you buy with a full bag. The game makes a shopping bag if there is none yet.
- If neither your bag nor the shopping bags have room, nothing changes: the package stays in the yard and a message says so.
- A package you bought from the shop can be returned for a refund while it stands in the yard. Once you pick it up, the shop no longer takes it back. The Pick up tooltip and the message after it both say so.
- Pick up shows greyed out, with the reason in its tooltip, while the package is being moved or installed, when you are not at home, or for a package that has no bag form.
- Pick up only shows after the disaster, on packages in the yard. Furniture already installed, the shopping bags and the boxes by the doors don't get it.

There is no setting and no key for Pick up.

## Known issues and limits

- Packages beyond your free package spots stay in your bag, on the floor or in your storages. Place some furniture to free spots, then press again.
- Floor and storage packages are only taken while you are at home.
- Storage packages are taken only after the bag and the floor, so they never take a spot a floor package could use.
- The game moves a storage stack whole and cannot be asked for part of one. When packages stack, a storage stack that holds more packages than the spots left stays in its storage, and the stacks that fill the most spots go instead. The message says how many were left because their stack was too big.
- If your bag is too heavy or full for a storage stack, it stays in its storage and the message says so.
- The game picks up a floor stack whole. When packages stack and a floor stack holds more packages than the spots left, all of it comes into your bag: as many as there are spots unpack, and the message says how many wait in your bag for the next press.
- A floor package the game refuses to unpack once it is in your bag stays in your bag; its spot goes to another package on the next press, not this one.
- Starting a batch never throws away actions you queued. If the game is ever in its replace-the-queue action mode while you have actions queued, a press does nothing and says "Finish or cancel your queued actions first."

## Requirements

- Survival Log 1.1.20151. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.12.0 or newer. Unpack All does not load without it. It shows the message after a press as a toast and applies settings changed on the Mod Manager's Mods screen without a restart.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `UnpackAll-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-UnpackAll/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/58).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\UnpackAll.dll`

## Uninstall

Delete `UnpackAll.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.unpackall.cfg` and `BepInEx\unpack-all.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.unpackall.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Each setting in the cfg file is explained in English and Simplified Chinese. See [CONFIG.md](CONFIG.md) for every setting.

## Troubleshooting

Tested on Survival Log 1.1.20151 with BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Unpack All 0.8.0 started`, followed by `unpack key None`, `storages off` and `crafted furniture to the entryway on`. The same lines, plus every press (what was in the bag, on the floor and in storages, the free spots, each unpack lined up) and how each batch ended, is in `BepInEx\unpack-all.log`, which survives restarts.

If Unpack all does not show in the Mods menu, make sure the inventory's item tab is open on the bag side. If a press says it cannot unpack right now, the rest of the message and the log say why. If storage packages are not taken, check that `IncludeStorages` is `true` in the config and that no storage or workbench window is open.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\unpack-all.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/UnpackAll -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\UnpackAll\bin\Release\UnpackAll.dll`.

The unit tests cover the pure rules (what a package is, how spots are handed out across the bag, the floor and storages, which crafted packages go to the entryway, when a yard package can be picked up and where it goes, when the key fires, the follow-up watch and the messages) and need nothing from the game:

```
dotnet test tests/UnpackAll.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
