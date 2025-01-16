# Murder Mystery
It's like Among Us, but worse! Play this Blade & Sorcery Multiplayer gamemode filled with deciet and murder! Either kill all the townsfolk as the villainous Murderer, or try and uncover the truth as the Detective!

## Gameplay
In Murder Mystery there are three teams: the Murderer, the Citizens, and the Detective. There is only one player on the Murderer and Detective team at a time. The Murderer's job is to kill all the Citizens, while it's the Detective's duty is to stop the Murderer. The Murderer can kill anyone they want, anytime they want. The Detective can also kill anytime they want, however killing the wrong person will result in an instant loss. If the Detective dies for any reason, a new Detective will be selected. Currently the Citizens only duty is to not die.

## Features
- Configurable match and intermission times
- Automatic matchmaking
- Invisible dead players
- 3 different win conditions
- Randomized spawn locations (Only the home level has random spawns)

## Planned Features
- Tasks for Citizens

## Instalation
This is a plugin for [Adammantium's Multiplayer Mod](https://www.nexusmods.com/bladeandsorcery/mods/6888). To use it, you will need to set up your own [dedicated server](https://github.com/AdammantiumMultiplayer/Server). Once that's set up just drag and drop the `MurderMystery.dll` file into the `plugins` folder!

## Config
The config is automatically generated after the initial run of the plugin. It is located in the `plugins` folder and is called `MurderMystery.json`.
| Tag                   | Description                                                                                     | Value Type                             | Default Value             |
|-----------------------|-------------------------------------------------------------------------------------------------|----------------------------------------|---------------------------|
| `requiredPlayerCount` | The minimun number of players required to start a match.                                        | `int`                                  | `2`                       |
| `matchTime`           | How long the match will last. In seconds.                                                       | `float`                                | `300.0`                   |
| `intermissionTime`    | How long between matches. In seconds.                                                           | `float`                                | `30.0`                    |
| 'playerSpawns'        | A dictionary, containing an array of possible spawn locations for the players in a given level. | 'Dictionary<string, List<List<float>>' | 'Default values for home' |
