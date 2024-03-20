![image](githublogo.png)

# Splotch

[![Total lines of code in master](https://tokei.rs/b1/github/commandblox/splotch?category=lines)](https://github.com/commandblox/Splotch)
[![GitHub repo size](https://img.shields.io/github/repo-size/commandblox/Splotch?style=plastic)](https://github.com/commandblox/Splotch)
[![GitHub Release](https://img.shields.io/github/v/release/commandblox/Splotch?style=plastic&label=latest%20release)](https://github.com/commandblox/Splotch/releases)
[![Contributors](https://img.shields.io/badge/contributors-3-orange?style=plastic)](#)
[![GitHub issues](https://img.shields.io/github/issues/commandblox/splotch?style=plastic)](https://github.com/commandblox/Splotch/issues)
[![Maintenance](https://img.shields.io/badge/maintenance-yes-brightgreen?style=plastic)](#)

Splotch is a mod loader for the game [Bopl Battle](https://zapraygames.com/), running on [Doorstop](https://github.com/NeighTools/UnityDoorstop) and [HarmonyX](https://github.com/BepInEx/HarmonyX).

## Table of Contents
1. [Installation](#installation)
2. [Mod Creation](#mod-creation)
3. [Roadmap](#roadmap)
4. [Contributing](#contributing)
5. [Credits](#credits)
6. [License](#license)
7. [Help](#help)
8. [Statistics](#statistics)

## Installation
To install Splotch, follow these steps:
- Extract the [latest release](https://github.com/commandblox/Splotch/releases/latest) into the game files of Bopl Battle.
- Start the game. A folder named `splotch_mods` should be generated.
- Place any mods you want to install into this folder.
- Start the game again, and you should see the names of the installed mods appear in the bottom right corner.
- The same procedure can be followed to update the game. If using BepInEx, ensure the `doorstop_config.ini` file is from Splotch.

## Mod Creation
To create mods for Splotch, clone the [template mod](https://github.com/commandblox/Splotch-Mod-Template) and refer to the [wiki](https://github.com/commandblox/Splotch/wiki/Mod-Development) for more information.

## Roadmap
### Mod Loading
- [x] Basic functionality
- [x] Loading from zip files
- [ ] Mod dependencies
### APIs
- [x] Event API (More events needed)
- [ ] BGL (Bopl Graphics Lib) (in progress)
- [ ] Ability API (in progress)
- [ ] Networking Lib (in progress)
### Other Features
- [x] General utility class
- [x] BepInEx compatibility
- [ ] Built-in mod manager

## Contributing
Contributions to Splotch are welcome! Feel free to check out the [issues](https://github.com/commandblox/Splotch/issues) and contribute in any way you can.

## Credits
- **Developer**: Codemob
- **Developer**: WackyModder
- **Contributer**: Almafa64
- **Wiki, Docs & Contributer**: Melon

## License
This project is licensed under the [WPFTL License](LICENSE).

## Help
If you need assistance, join our [Discord server](https://discord.gg/official-bopl-battle-modding-comunity-1175164882388275310). You can ask for help or discuss anything related to Splotch and modding for Bopl Battle.
