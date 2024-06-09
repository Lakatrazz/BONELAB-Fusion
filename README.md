# BONELAB Fusion Release
A multiplayer mod for BONELAB featuring support for all platforms.
[You can view a basic installation guide here.](INSTALLATION.md)

![](https://i.imgur.com/1ZpMfei.png)

## Networking
This mod is networked and built around Steam, but the networking system can be swapped out using a Networking Layer.

## Modules
Fusion supports a system called "Modules". This allows other code mods to add on and sync their own events in Fusion.
Fusion also has an SDK for integrating features into Marrow™ SDK items, maps, and more.

## Marrow SDK Integration
NOTICE:
When using the integration, if you have the extended sdk installed, delete the "MarrowSDK" script that comes with the extended sdk.
It can be found in "Scripts/SLZ.Marrow.SDK/SLZ/Marrow/MarrowSDK".
The reason for this is that this is already included in the real sdk, and Fusion uses some values from it, causing it to get confused.

You can download the integration unity package from [the Releases tab of this repository](https://github.com/Lakatrazz/BONELAB-Fusion/releases/latest).
Alternatively, you can download the files raw [here](https://github.com/Lakatrazz/BONELAB-Fusion/tree/main/Core/marrow-integration).

## Module Example
The module example can be found here:
https://github.com/Lakatrazz/Fusion-Module-Example

## Credits
- BoneLib AutoUpdater: https://github.com/yowchap/BoneLib
- Testing/Development Credits In Game

## Licensing
- The source code of [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks) is used partially under the MIT License. The full license can be found [here](https://github.com/Facepunch/Facepunch.Steamworks/blob/master/LICENSE).
- The source code of [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) is used partially under the MIT License. The full license can be found [here](https://github.com/rlabrecque/Steamworks.NET/blob/master/LICENSE.txt).
- The source code of [LiteNetLib](https://github.com/RevenantX/LiteNetLib) is included under the MIT License. The full license can be found [here](https://github.com/RevenantX/LiteNetLib/blob/master/LICENSE.txt).

## Setting up the Source Code
1. Clone the git repo into a folder
2. Navigate to the "LabFusion" Folder
3. Drag the dlls from Melonloader/Managed into the dependencies folder.
4. Drag MelonLoader.dll and 0Harmony.dll into the dependencies folder.
5. You're done!

## Disclaimer

#### THIS PROJECT IS NOT AFFILIATED WITH ANY OTHER MULTIPLAYER MODS OR STRESS LEVEL ZERO! This is its own standalone mod and shares no code with others, any similarities are coincidental!
