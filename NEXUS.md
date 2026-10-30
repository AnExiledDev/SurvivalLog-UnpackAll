Name: Unpack All
Summary: Unpack every furniture package in your bag and on the floor at home with one click, never more than your home has free spots for, and send crafted furniture straight to the entryway. It can also pull packages out of your storages if you turn that on. Uses the game's own Use action and waits behind the orders you already queued.

Description (BBCode):
[center][img]https://sldb.core-directive.com/media/save/1-save-safe.png[/img][/center]

Unpacking furniture packages one at a time is slow, and lining up too many fills the queue with unpacks that fail. Unpack All unpacks everything in your bag and on the floor at home in one press, and never lines up more than your home has room for. It can also take packages out of your storages at home if you turn that on in the config.

[size=5]Main features[/size]
[list]
[*][b]Bag first, then the floor, then storages[/b]: packages on the floor at home are picked up, then, if you turn it on in the config, packages in the storages at home you can see are taken out, and unpacked while spots are left. Storages are off by default.[/*]
[*][b]Never more than fits[/b]: only as many unpacks as your home has free package spots. The rest stay where they are.[/*]
[*][b]Game's own Use action[/b]: the same action as the inventory's Use button, with the game's checks and timings.[/*]
[*][b]Your queued actions stay[/b]: unpacks join the end of the queue, so your own actions run first.[/*]
[*][b]Clear messages[/b]: says how many are unpacking, how many were picked up or taken from storages, and why any were left.[/*]
[*][b]Crafted furniture to the entryway[/b]: furniture you craft at a workbench goes straight to the entryway, as if you had clicked Unpack to Entryway. A package an open Aid Platform commission asks for stays a package, and when the entryway is full the rest stay in your bag. You can turn this off in the config.[/*]
[*][b]Pick up yard packages[/b]: click a furniture package in the yard and choose Pick up to put it back in your bag, for example to hand it in for an Aid Platform commission. If your bag can't carry it, it goes into a shopping bag in the living room. If neither has room, the package stays in the yard. A bought package can no longer be returned to the shop after you pick it up.[/*]
[*][b]Mods menu[/b]: a Mods button on the inventory toolbar, and in the bag panel of most station screens, with an Unpack all entry, shared with other mods that use it.[/*]
[/list]

[size=5]Hotkeys[/size]
No key is set by default: use Unpack all in the Mods menu. You can set an unpack key in the config. It does nothing while you are typing in a text box.

Works in English and Chinese.

The mod adds no data of its own to your save, but the unpacks are real: packages come off the floor or out of your storages into your bag and become furniture. You can remove it at any time; packages and furniture stay as they are.

[size=5]Known issues[/size]
[list]
[*]Packages beyond your free package spots stay where they are. Place some furniture, then press again.[/*]
[*]Floor and storage packages are only taken while you are at home, and nothing leaves a storage while a storage, workbench or shop window is open.[/*]
[*]Locked storages, the drone hub and drones, the neighbour gift basket, the door box and workstation bins are never touched.[/*]
[*]The game picks up a floor stack whole, so some packages can end up waiting in your bag for the next press. A storage stack bigger than your free spots stays in its storage.[/*]
[*]The game's own refusal reasons are shown as the game words them, which can be Chinese.[/*]
[*]The shared Mods button and its tooltip stay in English.[/*]
[/list]

[center][img]https://sldb.core-directive.com/img/mods-banner.jpg[/img]
[size=4][url=https://www.nexusmods.com/survivallog/mods/73]Get Mod Manager[/url] to set up all your mods in game  |  [url=https://sldb.core-directive.com/en/]Open the Survival Log wiki[/url] for items, recipes and maps[/size][/center]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.20151. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]. Install it first.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.12.0 or newer. Unpack All does not load without it. It shows the message after a press as a toast and applies settings changed on the Mod Manager's Mods screen without a restart.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\UnpackAll.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]UnpackAll.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
To set a key, turn storages on or turn crafted furniture to the entryway off, edit [font=Courier New]BepInEx\config\com.dev.survivallog.unpackall.cfg[/font] while the game is closed, or do it in game on the Mod Manager's Mods screen, where changes take effect at once (see [url=https://github.com/AnExiledDev/SurvivalLog-UnpackAll/blob/main/CONFIG.md]CONFIG.md[/url]; the cfg file explains each setting in English and Chinese).

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font] and [font=Courier New]BepInEx\unpack-all.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord; you can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-UnpackAll]github.com/AnExiledDev/SurvivalLog-UnpackAll[/url]

[line]
[size=5]简体中文[/size]

一个一个拆家具包裹很慢，一次排太多又会让行动队列塞满会失败的拆封。Unpack All 一键拆封背包里和家里地上的所有包裹，而且排进队列的数量绝不超过家里的空位。在配置中开启后，还能从家里的储物家具中取出包裹一起拆封。

[size=5]主要功能[/size]
[list]
[*][b]先背包，再地上，最后储物家具[/b]：先拆封背包里的包裹，然后捡起家里地上的包裹；如果在配置中开启，还会取出家里可见储物家具中的包裹，在还有空位时一起拆封。储物家具默认关闭。[/*]
[*][b]绝不超出空位[/b]：排进队列的拆封数量不超过家里空余的包裹位，其余的原地不动。[/*]
[*][b]游戏自己的使用动作[/b]：和背包界面「使用」按钮相同的动作，遵循游戏自己的检查和时间。[/*]
[*][b]保留你排好的行动[/b]：拆封加在队列末尾，你自己排的行动先执行。[/*]
[*][b]清楚的提示[/b]：告诉你正在拆封多少个、从地上捡起或从储物家具取出多少个，以及剩下的为什么没拆。[/*]
[*][b]做好的家具送到玄关[/b]：在工作台做好的家具会直接送到玄关，和点「拆到玄关」一样。援助平台上未完成的委托需要的包裹会保留为包裹；玄关放满后，其余的留在背包里。可以在配置中关闭。[/*]
[*][b]拿起院子里的包裹[/b]：点击屋外空地上的家具包裹，选择「拿起」，就能把它放回背包，比如拿去交援助平台的委托。背包装不下时，会放进客厅的购物袋；两边都没有地方时，包裹留在原地。买来的包裹拿起后就不能再退货了。[/*]
[*][b]Mods 菜单[/b]：背包工具栏上，以及大部分设施界面背包栏里的 Mods 按钮，里面有「全部拆封」一项，和其他使用同一菜单的模组共用。[/*]
[/list]

[size=5]快捷键[/size]
默认不绑定按键：请使用 Mods 菜单里的「全部拆封」。可以在配置中设置拆封键；在文本框中输入文字时，此键不起作用。

模组不会往存档里写入自己的数据，但拆封是真实的：包裹会从地上或储物家具进入背包，再变成家具。你可以随时移除模组，包裹和家具都会保持原样。

支持英文和中文。

[size=5]已知问题[/size]
[list]
[*]超出空余包裹位的包裹会留在原处。摆放一些家具后再按一次。[/*]
[*]只有在家时才会拿取地上和储物家具中的包裹；开着储物、工作台或商店界面时，不会从储物家具中取出任何东西。[/*]
[*]上锁的储物家具、无人机站和无人机、邻居礼物篮、门口箱子以及工作台的料仓永远不会被动用。[/*]
[*]游戏会整组捡起地上的包裹，所以有些包裹可能留在背包里等下次再拆。比空位还大的一组储物包裹会留在储物家具中。[/*]
[*]游戏自己的拒绝原因按游戏原文显示。[/*]
[*]共用的 Mods 按钮及其提示保持英文。[/*]
[/list]

[url=https://www.nexusmods.com/survivallog/mods/73]获取 Mod Manager[/url]，在游戏里设置所有模组。[url=https://sldb.core-directive.com/zh/]打开 Survival Log 维基[/url]，物品、配方、地图一查就有。

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.20151。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]，请先安装。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.12.0 或更新版本。没有它本模组不会加载。它把按下后的提示显示为弹出通知，并让在 Mod Manager 的「模组」界面改动的设置无需重启即可生效。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\UnpackAll.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]UnpackAll.dll[/font]。

[size=5]配置[/size]
可以在游戏内 Mod Manager 的「模组」界面修改，改动立即生效；也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.unpackall.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-UnpackAll/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font] 和 [font=Courier New]BepInEx\unpack-all.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-UnpackAll]github.com/AnExiledDev/SurvivalLog-UnpackAll[/url]
