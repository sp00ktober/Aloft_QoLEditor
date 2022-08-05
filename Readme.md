## Aloft_QoLEditor

### What is it
This is a very tiny mod that adds the following QoL features to [Aloft](https://store.steampowered.com/app/1660080/Aloft/):
- Use space and ctrl keys to move the camera up and down
- Use your mouse while the camera is moving to rotate it
- Hold shift while dragging an object to drag it on the Y axis

### How does it work?
The mod uses Harmony and BepInEx to patch the game at runtime. Currently there is no modmanager support present (but this will change in the future hopefully).

### How can i use this?
- Get the latest release of the 5.x version of BepInEx from [here](https://github.com/BepInEx/BepInEx/releases) (must be compatible to your system, most likely x64)
- Extract the .zip and copy the `BepInEx` folder into your games root folder (probably `C\Program Files (x86)\Steam\steamapps\common\Aloft Demo\`)
- Copy the `winhttp.dll` and `doorstop_config.ini` also to your games root folder.
- Start the game once and then close it again.
- In the BepInEx folder you should now find a `plugins` folder, copy the mod .dll inside there

You can also see the official installation guide [here](https://github.com/BepInEx/BepInEx/wiki/Installation)