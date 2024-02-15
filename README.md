# Splotch

![Total lines of code in master :)](https://tokei.rs/b1/github/commandblox/splotch?category=lines)
![GitHub repo size](https://img.shields.io/github/repo-size/commandblox/Splotch?style=plastic)
![GitHub Release](https://img.shields.io/github/v/release/commandblox/Splotch?style=plastic&label=latest%20release)

Splotch is a mod loader for the game Bopl Battle. It runs on [Doorstop](https://github.com/NeighTools/UnityDoorstop) and [HarmonyX](https://github.com/BepInEx/HarmonyX).

## Installation
Extract the [latest release](https://github.com/commandblox/Splotch/releases/latest) into the game files of Bopl Battle and start the game. The folder `bopl_mods` should be generated. Put any mods you want to install and start the game and you should see the names of the mods you installed appear in the bottom right. The same procedure can be done to update the game. If you do this with BepInEx installed make sure that the `doorstop.cfg` file is from Splotch.

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
   - [x] BepInEx compatibility
 - [ ] Built-in mod manager
