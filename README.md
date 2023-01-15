# SIT.Core

## Disclaimer

This is by no means designed for cheats or illegally downloading the game. This is purely for educational and game modification purposes. You must buy the game to use this. 
You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)

## SPT-AKI
Stay in Tarkov requires the latest AKI Server to run. You can learn about SPT-Aki here.

## Summary

The Stay in Tarkov handles almost everything to create the Single Player experience of Escape from Tarkov.
Including but not limited to:
- Turning off BattlEye (lets be honest, it doesn't work anyway BSG, please change to something else!)
- Turning off FileChecker (this is BSG's own checker, this needs to be turned off to allow us to mod the game) - See FileChecker
- Setting up Auto Singleplayer mode (see Menus)
- Fixing bots / AI to shoot each other (see AI)
- Fixing bots / AI to become "PMC" (see AI)
- Fixing "offline" mode to use only the designated online spawn points
- Fixing "offline" mode to save Progression of the character (something I used was in Live in some form!)
- Fixing "offline" mode to save Health of the character (something I used was in Live in some form!)
- Lots more

## How to compile? 
1. Deobfuscate latest Assembly-CSharp via [SIT.Launcher](https://github.com/paulov-t/SIT.Tarkov.Launcher)
2. All the following assembly references must be placed in Tarkov.References in the parent folder of this project. You can copy-paste from your Tarkov Install.
- Assembly-CSharp (deobfuscated via [SIT.Launcher](https://github.com/paulov-t/SIT.Tarkov.Launcher))
- bsg.componentace.compression.libs.zlib
- com.unity.multiplayer-hlapi.Runtime
- Comfort
- Comfort.Unity
- DissonanceVoip
- FilesChecker
- Sirenix.Serialization.Config
- Sirenix.Serialization
- Sirenix.Utilities
- Unity.ScriptableBuildPipeline
- UnityEngine.AssetBundleModule
- UnityEngine.CoreModule
- UnityEngine
- UnityEngine.UI
3. You will need BepInEx Nuget Feed installed on your PC by running the following command in a terminal. 
```
dotnet new -i BepInEx.Templates --nuget-source https://nuget.bepinex.dev/v3/index.json
```
4. Open the .sln with Visual Studio 2022
5. Rebuild Solution (This should download and install all nuget packages on compilation)

## Which version of BepInEx is this built for?
Version 5

# How to install BepInEx
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## Install to Tarkov
BepInEx 5 must be installed and configured first (see How to install BepInEx)
Place the built .dll in the BepInEx plugins folder

## Test in Tarkov
- Browse to where BepInEx is installed within your Tarkov folder
- Open config
- Open BepInEx.cfg
- Change the following setting [Logging.Console] Enabled to True
- Save the config file
- Run Tarkov through a launcher or bat file like this one (replacing the token with your ID)
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=AID062158106353313252ruc -config={"BackendUrl":"http://localhost:6969","Version":"live"}
```
- If BepInEx is working a console should open and display the module "plugin" as started

## Thanks List
- SPT-Aki team

## License

>>SOME<< of the original core functionality completed by SPT-Aki teams. There may be licenses pertaining to them within this source.