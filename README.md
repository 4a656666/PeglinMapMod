# PeglinMapMod
A mod for [Peglin](https://store.steampowered.com/app/1296610/Peglin/) that allows you to modify how the map generates.
 * Guarantee a path from the start to the boss of a particular type of room
 * Change the weights of each room type during randomization
 * Toggle whether elite fight rooms are allowed near the start
 * Control which individual rooms are allowed to be used
 * Change the shape of the map
   * Fix the inefficient edges on the side of of the map where you could skip a room
   * Enable a custom map which has a choice of two boss rooms at the end
   * Extend the map

## Installation
 1. Install BepInEx by following [this guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
 2. Download the mod [from the latest release](https://github.com/4a656666/PeglinMapMod/releases/latest) or by [clicking this link](https://github.com/4a656666/PeglinMapMod/releases/latest/download/PeglinMapMod.dll). 
 3. Place the mod in the `BepInEx/plugins` folder

## Configuration
The mod can be configured using the `BepInEx/config/PeglinMapMod.cfg` file.  
A list of every individual room can be found [here](Rooms.md).

Note: the config mentions 'event' and 'random' room types. Both appear as a question mark on the map, but 'event' rooms can only be a text scenario while 'random' rooms can be an scenario, battle, or relic.
