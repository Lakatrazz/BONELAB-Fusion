## **Fusion**
#### Fixed:
- Local VoiceProxy giving incorrect amplitude values
- Connection spoof check mistake
- Destructible catchup not checking if the poolee was in a pool
- Menus using only the profanity filter instead of the general text filter
- Mature mod thumbnails being visible
- RandomObject being host auth instead of ownership auth and not triggering for the owner
- Steam's 50 lobby limit interfering with joining (now mitigated)
- Disabled CrateSpawners throwing errors that occasionally crash
- Game crashing when a mod.io token is not linked
- Base Unity reverb zones causing sound to break with voice chat
- Module and RPC hashing no longer includes the assembly version
- Gamemode score keeper throwing errors with null IDs
- Target network layer not showing properly if unavailable
- BitMart birthday music not functioning properly
- Syncing with rigidbodies that have frozen rotation
- LaserCursors adding network player controllers
#### Removed:
- Mature mod auto download toggle (mature mods are now manual installs only)
#### Added:
- Validation for message sender IDs
- Despawn All no longer despawns circuits
- Zone reverb support for voice chat with no more audio ducking
- Function to load modules from assembly
- GamemodeEvents now trigger on late join
- Body settings are synced again
- Bonelab code is now dynamically loaded in preparation for Boneworks
- Unculling objects can take ownership if needed
- Synced all layers for AnimatorSyncer
- Player controller interpolation
#### Improved:
- Voice chat volume
- Prop sleeping
- Send rate of objects (now consistent and not way too often)

## **Fusion Auto Updater**
- No changes

## **Fusion SDK**
- No changes

## **Fusion Helper**
- [Download Here](https://github.com/Lakatrazz/Fusion-Helper/releases/latest)
