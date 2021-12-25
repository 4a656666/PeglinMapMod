# PeglinMapMod
A mod for [Peglin](https://store.steampowered.com/app/1296610/Peglin/) that allows you to modify how the map generates.
 * Guarantee a path from the start to the boss of a particular type of room
 * Change the weights of each room during randomization
 * Toggle whether elite fight rooms are allowed near the start

## Installation
 1. Install BepInEx by following [this guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
 2. Download the mod [from the latest release](https://github.com/4a656666/PeglinMapMod/releases/latest) or by [clicking this link](https://github.com/4a656666/PeglinMapMod/releases/latest/download/PeglinMapMod.dll). 
 3. Place the mod in the `BepInEx/plugins` folder

## Configuration
The mod can be configured using the `BepInEx/config/PeglinMapMod.cfg` file.

Note: the config mentions 'event' and 'random' room types. Both appear as a question mark on the map, but 'event' rooms can only be a scenario while 'random' rooms can be an event, battle, elite fight, or relic.
