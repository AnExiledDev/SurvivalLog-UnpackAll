> **AI-written from operator direction.** The intent is the operator's; the wording and scope are an agent's.
> `session: 127889d4-f8d8-4d17-8ded-f6f46da456fc | 2026-10-12 | source: op:2026-10-11-2128-6483`

# Unpack All

A BepInEx 6 (IL2CPP) plugin for Survival Log: one press unpacks every furniture package in your bag, then picks up the packages lying on the floor at home and unpacks those too, as far as your home has free package spots.

## What it does

- Packages in your bag are unpacked first. If spots are left over, packages on the floor inside your home are picked up into your bag and unpacked as well.
- Only as many unpacks are lined up as your home has free package spots, so every one of them runs and the queue bar never fills with unpacks that would fail. The rest stay where they are.
- Each unpack goes through the game's own Use action for that package, the same one the inventory's Use button lines up, so the game's own checks and timings apply.
- A short message at the bottom of the screen says how many packages are unpacking, how many were picked up from the floor, and how many were left and why (no free package spot, your bag could not carry it, you are away from home, or the game's own reason for refusing).

## Controls

- **Unpack all** button on the inventory screen's item tab, next to the game's Sort button.
- `F9` anywhere in the game does the same.

While unpacks from an earlier press are still lined up, another press does nothing but say so, because starting a new batch would restart the one in progress.

## Config

`BepInEx/config/com.dev.survivallog.unpackall.cfg`:

| Setting | Default | What it does |
| --- | --- | --- |
| `Controls/UnpackKey` | `F9` | Key that does the same as the Unpack all button. `None` turns the key off; the button still works. |

Log: `BepInEx/unpack-all.log`.

## Known limits

- Packages beyond your free package spots stay in your bag or on the floor. Place some furniture to free spots, then press again.
- Floor packages are only picked up while you are at home.
- Packages inside storages are not touched, only your bag and the floor at home.
- Starting the batch works like the game's own Use button, so it replaces whatever your character was doing.
- A floor package the game refuses to unpack once it is in your bag stays in your bag; its spot goes to another floor package on the next press, not this one.
- The game's refusal reasons are shown as the game words them, which can be Chinese.
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

## Provenance

Operator, 2026-10-11 (op:2026-10-11-2128-6483):

> Can we also create a mod to "unpack all" which also unpacks what's on the floor, for like placeable objects.

From the same message, on the queue bar:

> When queueing actions, like taking adrenaline or unpacking things, it queues it up many times, fills the queue bar, I just took 3 adrenaline at once lol.
