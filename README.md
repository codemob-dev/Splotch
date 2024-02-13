# Splotch

![Total lines of code in master :)](https://tokei.rs/b1/github/commandblox/splotch?category=lines)

Splotch is a mod loader for the game Bopl Battle. It runs on [Doorstop](https://github.com/NeighTools/UnityDoorstop) and [HarmonyX](https://github.com/BepInEx/HarmonyX).

## Installation
Extract the [latest release](https://github.com/commandblox/Splotch/releases/latest) into the game files of Bopl Battle and start the game. The folder `bopl_mods` should be generated. Put the folder of any mods you want to install and start the game and you should see the names of the mods you installed appear in the bottom right.

## Mod creation
Clone my [template mod](https://github.com/commandblox/Splotch-Mod-Template) to get started. More info on the [wiki](https://github.com/commandblox/Splotch/wiki/Mod-Development).

## Roadmap
 - Mod loading
   - [x] Basic functionality
   - [x] Loading from zip files
   - [ ] Mod dependencies
 - APIs
   - [x] Event API (More events are needed)
   - [ ] BGL (Bopl Graphics Lib) (in progress)
   - [ ] Ability API (in progress)
   - [ ] Networking Lib
 - Other features
   - [x] A general utility class
   - [ ] BepInEx compatibility (in progress)
