# Rift of the NecroManager
This project is a mod for Rift of the NecroDancer which adds a settings menu for viewing and configuring BepInEx mods. Supported configuration options can be modified directly in-game without the need to manually edit hard-to-manage configuration files.

> [!WARNING]
> BepInEx mods are <ins>**not officially supported**</ins> by Rift of the NecroDancer. If you encounter any issues with this mod, please open an issue on this GitHub repository, and do not submit reports to Brace Yourself Games!

The current version is <ins>**v1.0.0**</ins>. Downloads for the latest version can be found [here](https://github.com/96-LB/RiftOfTheNecroManager/releases/latest). The changelog can be found [here](Changelog.md).

## Installation

Rift of the NecroManager runs on BepInEx 5. In order to use this mod, you must first install BepInEx into your Rift of the NecroDancer game folder. A more detailed guide can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html), but a summary is provided below. If BepInEx is already installed, you can skip the next subsection.

### Installing BepInEx
1. Navigate to the latest release of BepInEx 5 [here](https://github.com/BepInEx/BepInEx/releases).

> [!WARNING]
> This mod is only tested for compatibility with BepInEx 5. If the above link takes you to a version of BepInEx 6, check out [the full list of releases](https://github.com/BepInEx/BepInEx/releases).

2. Expand the "Assets" tab at the bottom and download `BepInEx_win_x64_5.X.Y.Z.zip`.

> [!IMPORTANT]
> Download the 64-bit Windows version even if you use a different operating system!, download `BepInEx_win_x64_5.X.Y.Z.zip`.

3. Extract the contents of the `.zip` file into your Rift of the NecroDancer game folder.

> [!TIP]
> You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

4. If you're on Mac or Linux, configure Steam to run BepInEx when you launch your game.

> [!TIP]
> Add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the launch options in the game's properties, or follow the guide [here](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

5. Run Rift of the NecroDancer to set up BepInEx.

6. Check that the installation was successful by looking for a folder called `plugins` inside of your `BepInEx` folder.

### Installing Rift of the NecroManager
1. Navigate to the latest release of Rift of the NecroManager [here](https://github.com/96-LB/RiftOfTheNecroManager/releases/latest).

> [!CAUTION]
> Do NOT download the source code using the button at the top of this page. If you're downloading a `.zip` file, you are at the wrong place.

2. Expand the "Assets" tab at the bottom and download `RiftOfTheNecroManager.dll`.

3. Place `RiftOfTheNecroManager.dll` in the `BepInEx/plugins` directory inside the Rift of the NecroDancer game folder.

> [!TIP]
> You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

4. Check that your mod is working by launching the game and opening the settings menu. You should see a new button labeled 'MODS'.

## Usage

After installation, any other mods will automatically have a settings menu created for them. This menu can be used to modify certain configuration settings, even if the mod was not created with Rift of the NecroManager in mind. Not every setting will be compatible, but this mod aims to work with as many mods as possible. If something isn't working, please open an issue on this GitHub repository or reach out to the original mod developer.

Setting types which can be modified in-game include:
- Booleans
- Enums
- Strings
- Integers with a specified range of acceptable values
- Floats with a specified range of acceptable values
- Colors

Some limitations include:
- **Some settings require a game restart to take effect!** This depends on the mod's code, and Rift of the NecroManager is unable to know whether a configuration option has immediate effect or not.
- Particularly large ranges of integers can be difficult or slow to modify in-game because the settings are implemented as sliders.
- Float granularity is limited for the same reason.
