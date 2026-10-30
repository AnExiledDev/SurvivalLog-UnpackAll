# Changelog

## 0.8.0

- New: click a furniture package in the yard and choose Pick up to put it back in your bag, for example to hand it in for an Aid Platform commission.
- If your bag can't carry the package, it goes into a shopping bag in the living room instead. If neither has room, the package stays in the yard and a message says so.
- A package you bought from the shop can no longer be returned for a refund once you pick it up. The Pick up tooltip says so.
- Made for Survival Log 1.1.20151.

## 0.7.0

- New: furniture you craft at a workbench goes straight to the entryway, as if you had clicked Unpack to Entryway. A package an open Aid Platform commission asks for stays a package, and when the entryway is full the rest stay in your bag. Turn it off with `CraftedToDoor` under Furniture.
- New installs start with no unpack key, since Unpack all in the Mods menu does the same. If you already had the mod, your config keeps the key it had; set `UnpackKey` to `None` to drop it.
- The unpack key does nothing while you are typing in a text box.
- Exiled's Toolkit 0.12.0 or newer is now required.
- Made for Survival Log 1.1.20151.

## 0.6.1

- The message after a press is now an Exiled's Toolkit toast, so it appears beside the game's HUD and in the Notifications history, at the Toolkit's UI size.
- Removed: the `UiScale` setting under Interface, since the Toolkit now draws the message.
- Exiled's Toolkit 0.11.0 or newer is now required.
- Made for Survival Log 1.1.19611.

## 0.6.0

- New: `UiScale` under Interface sets the size of Unpack All's message after a press on its own, from 75% to 200%, whatever Exiled's Toolkit's UI size is. At 0, the default, it follows the Toolkit's. Needs Exiled's Toolkit 0.11.0 or newer.
- Made for Survival Log 1.1.19611.

## 0.5.2

- The message after a press can be drawn bigger or smaller with the UI size setting of Exiled's Toolkit 0.10.0 or newer. A change shows at once.
- The Mods button and its menu keep the game's size, like the rest of the toolbar.
- Made for Survival Log 1.1.19611.

## 0.5.1

- Unpacking many packages at once no longer makes the game stutter, and neither does the first press after starting the game.
- Less work in the background while you play.
- Made for Survival Log 1.1.19611.

## 0.5.0

- Unpack all is now also in the Mods button on the workbench, stove, ration press, greenhouse, drone hub, brewing barrel, dye vat, rat cage, material rack and shop screens.
- While one of those screens is open, it only unpacks what's in your bag. Packages on the floor and in storages wait until you close it.
- Made for Survival Log 1.1.19611.

## 0.4.1

- After a game update, Mod Manager can tell you if this mod may need an update.
- Made for Survival Log 1.1.19131.

## 0.4.0

- Mod Manager's Mods screen links to this changelog.
- Settings changed on the Mod Manager's Mods screen take effect at once.
- Mod Manager's Mods screen shows the mod's name, description, category and links.
- Exiled's Toolkit 0.4.0 or newer is optional. With it, settings you change on the Mods screen take effect right away. Without it, they take effect the next time you start the game.
- When the game is in Chinese, every reason a package can't be unpacked is shown in Chinese too.

## 0.3.1

- First public release.
- Press F9, or use the inventory's Mods menu, to unpack every furniture package in your bag, then the ones on the floor at home.
- It can also take packages out of your storages, if you turn that on.
- It never unpacks more than your home has free package spots for.
