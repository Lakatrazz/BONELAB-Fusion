## v1.15.0
Added:
- Gamemode wrist watch
- New UI API to help creating menus from code
- New Equippable system separate from BitMart items
- Mod cache system
- Toggles for certain notifications
- Singleplayer Only tags to Fusion machines
- Changelog to Thunderstore release
- Toggle to enable untrusted video players (off by default)
- Cosmetic auto downloading
- Downloading a level will now show you the level name while in the regular loading screen
- Visualization for props that aren't downloaded yet
- Propagation of object ownership to constrained objects
- Icon for seekers in Hide and Seek
- More safety checks for NaN object positions
- Prop desync teleporting
- Hiding lobbies with impossible information
- Disabling drag on objects owned by other players
- Prop interpolation to improve smoothness
- Rotation prediction

Changed:
- Made Hide and Seek players immortal
- Made Gamemode Markers add/remove themselves when enabled or disabled instead of awake/destroy
- Invalidated existing cosmetics to prevent issues with the new system
- Cosmetic points now have a few body points with many directional options
- Consolidated username filters to make changing them easier
- Reordered prop updates for performance
- Decreased max prediction time and limited player prediction
- Made guns still fire even if a magazine isn't loaded locally

Improved:
- Player desync teleporting
- Players getting stuck outside of vehicles with unsynced doors
- PD controller force calculations
- Impersonation check, again

Fixed:
- Rubberbanding when players are moving extremely fast
- Mods still installing even if the user ran out of space
- Avatar download bar disappearing before finishing
- Download progress for one instance of a mod not applying to all instances
- GamemodeMarker respawns causing the player to get stuck in the floor if rotated
- Player positional offsets due to ground friction
- Players being able to modify their own permission metadata variable visually
- Player blood decals not clearing properly
- Avatar SurfaceData for modded avatars

Removed:
- Link censoring
- Unnecessary writing of embedded txt files to disk

## v1.14.2
Changed:
- Old mod.io API to the new API

## v1.14.1
Improved:
- Additional changes to impersonation check

## v1.14.0
Fixed:
- Tick rate slowing based on time scale
- Thumbstick axis losing all magnitude
- Messages being relayed even if a user does not have a PlayerID
- OwnershipEvents not triggering for the first set

Adjusted:
- Made SimpleGripEvents execute locally for the owner again
- Blocked all links in descriptions/names

Improved:
- Impersonation check

## v1.13.1
Fixed:
- Outdated credits

Improved:
- Animator syncer smoothness
- Voice chat peaking 

## v1.13.0
Fixed:
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

Removed:
- Mature mod auto download toggle (mature mods are now manual installs only)

Added:
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

Improved:
- Voice chat volume
- Prop sleeping
- Send rate of objects (now consistent and not way too often)

## v1.12.2
Fixed:
- Notifications not appearing with the content not installed

Improved:
- Prevented rich text in lobby names except for color

## v1.12.1
Fixed:
- Late joiners not being given 0 stocks and 0 score in Smash Bones
- Avatar stats always being refreshed breaking certain stat changes

## v1.12.0
Fixed:
- Gamemode issues with lingering players in score/team after leaving
- Spawn/Nimbus guns not cleaning up at all

Rewrote:
- Switch all byte writing to use BinaryPrimitives BigEndian for consistency

## v1.11.2
Fixed:
- Smash bones stockless/damageless bugs (potentially)

## v1.11.1
Fixed:
- Issues with mod blacklisting
- Nimbus/Spawn guns despawning while in your hand

## v1.11.0
Hotfix:
- Fixed grabbing and ragdolling issues on quest
- Fixed module messages being broken on quest

## v1.10.2
Hotfix:
- Fixed quest crashing issues

## v1.10.1
Hotfix:
- Fixed issue with server settings breaking with too many players

## v1.10.0
Added:
- Avatar surface data instead of always blood
- Arena/gun range spawner sync
- Health bar for specific gamemodes
- Juggernaut gamemode
- Smash Bones gamemode
- Profanity filter
- Seat catchup
- Holster catchup
- Destructible catchup
- Constraint catchup
- Voice chat muffling behind walls
- Visible Platform ID on players
- Global ban list for malicious client users
- Avatar height limit setting
- Hidden mod downloading
- Saved inventory items spawning
- Level queue settings for gamemodes
- Default pose for players instead of mirroring
- Time between rounds setting for gamemodes

Fixed:
- Player volume setting
- Invalid steam_api64.dll remaining broken
- Corrupted ban lists breaking the mod
- Invincible avatars
- Teleporting requiring higher permissions
- Jaw movement being strange and jittery
- Voice chat delay
- Deathmatch using team spawns/vice versa
- Certain CrateSpawners in encounters being broken
- Broken teleporting
- Swimming rig pose
- Gachapons not spawning
- Voice Chat randomly breaking, fixed on level reload if broken
- Arena crashes
- CrateSpawner onSpawnEvent not running for spawnables/avatars

Improved:
- Cache mod thumbnails and delete on level change
- Player positions being offset
- Voice chat data consumption (using G711 encoding)
- Disabling of certain matchmaking tabs based on the networking layer
- Low gravity slow mo getting stuck (potentially)
- Clearing of gamemode data on gamemode end
- Notification spam (similar notifications cancel each other)
- BitMart UI

Removed:
- Voice muting on death
- Voice chat in loading screens

SDK:
- Cosmetic alignment previews
- AnimatorSyncer script
- New message relay system
- Code based Rpc attributes
- GetOwner and SetOwner in OwnershipEvents
- Catchup of RPCVariables
- Relays for RPC events
- Event when all players load in
- Spawnables with a Singleplayer Only tag don't spawn
- Module messages changed from ushorts to hashed longs (host no longer needs the module if relayed)
- Voice chat proxy

## v1.9.3
Added:
- CrateSpawner support to Desyncer script

Improved:
- Lobby sorting

Fixed:
- Random player teleports
- DM scoring issues (potentially)
- Player element hooks not being cleared
- Victory Trophy not being rewarded
- Dev Tools not disabling in Gamemodes
- Hub and Reset buttons not being disabled

Removed:
- Gamemode late join prevention

## v1.9.2
Added:
- Player vitality setting to DM/TDM
- Time limit to Hide and Seek
- Death triggers in Fusion SDK
- Desyncer script in Fusion SDK

Improved:
- Priority of level downloads

Fixed:
- Error when LobbyInfo isn't available
- Incompatability with scene bootstrap overriding
- Knockout getting you stuck in the kill barrier
- Extreme host lag when players join servers
- Fix thumbnail download errors when changing scenes

## v1.9.1
Fixed:
- Not being able to join servers on Quest

## v1.9.0
Added:
- Custom UI
- Log-in menu
- Circuit sync
- RandomObject sync
- Spawn Gun VFX in multiplayer
- Mature mod toggle
- Friendly fire toggle
- Player join/leave VFX
- "Knockout" mode (ragdoll instead of dying)
- Level download waiting scene
- Mod download blacklist
- Matchmaking system
- Gamemode ready system
- Bit rewards for Hide and Seek

Improved:
- Crash prevention stability

Fixed:
- Quest support
- Auto updater on Quest
- Mod download failure (potentially)
- Breaking without microphone access
- Players getting flung into the void
- Team Deathmatch

Removed:
- Platform discrimination
- Vote kicking
- BoneLib dependency

## v1.8.0
Added:
- Patch 6 Support
- Input Devices setting for Voice Chat (no longer just Default)

Fixed:
- Mine Dive not creating multiple minecarts
- Monogon Motorway not creating multiple gokarts
- Hide and Seek blinding parenting to the skull instead of the camera, so avatar proportions affected the functionality
- Muting causing voice chat to stop functioning
- Dying inside of a vehicle causing you to get stuck ragdolled on respawn

Improved:
- Default maximum download file size increased
- Mute UI is now a spawnable instead of loaded from the asset bundle
- Matchmaking tab lets you know when you don't have the Fusion Content pallet installed or updated

## v1.7.0
Added:
- Patch 5 Support
- Quest 2 Voice Chat
- Jaw Movement in Mirrors
- Built-In Mod Downloading (uses your login from the mod.io menu, options to disable and limit file size)
- Hide and Seek Gamemode
- Player blood effects and decals
- Synced physics culling
- Power Puncher now ragdolls players for a small amount of time
- Extra APIs for code modders to make use of
- RPC Events for sdk modders to make use of (will be available in the next Fusion SDK release)

Rewrote:
- Microphone System
- Synced Entities (now use a modular NetworkEntity system)
- Synced Players (make use of the NetworkEntity system, making systems much more universal)
- Gamemodes
- Spawn Syncing
- Cosmetics
- Bundles (now moved to an SDK mod, can be downloaded within the in-game Downloading tab)
- Head UI (automatically aligns when more elements are added, though some systems still need to be converted)

Removed:
- Gravity sync
- Lots and lots of duplicate code
- Old fixes for bugs/issues that are no longer in BoneLab

Improved:
- Microphone Quality
- Object Physics
- SteamID spoofing
- Board gun manually synced
- Performance (Heavily)

Known Issues:
- Fusion SDK not updated (saved for a future update)
- Arena not fully fixed
- No blacklist for mod downloading

## v1.6.3
Added:
- Automatic scene reloading if the screen becomes black

Fixed:
- Major crashing bugs from 1.6.0
- Broken static impact properties while in a server

Improved:
- Spawning in the ground (likely)

## v1.6.0
Added:
- Thrown objects now do blunt/stab damage
- Can no longer shoot guns while dead
- Different damage per body part
- Synced loading screens
- Special events near Fusion's birthday

Fixed:
- Spawning bugged in the ground when respawned
- Players standing up right before respawning
- Specific cosmetics not rendering on Quest
- Guest permission causing broken permissions
- Stat changer notification sending to all players instead of host
- Switched grabbing logic back to harmony patch (more reliable)

Improved:
- Replaced some temp array allocations with stackalloc
- Swap Unity Vector3 operations to System Vector3
- Made asset loading asynchronous
- Synced object movement

## v1.5.1
Fixed:
- Gap between module messages causing broken handling (thanks notnotnotswipez)

## v1.5.0
Platforms:
- Meta Quest 2 Support

Fixed:
- Module messages being broken
- TDM bit rewarding being broken
- Jaw on players being jittery
- Constraint remover mods/bugs

Added:
- Notification when Auto-Updater is missing
- Player scaling support
- Custom notification icons
- Player list on lobbies
- SLZ's UI system
- Achievements
- Sync MineDive minecart amount
- Server tags
- DM spawnpoints to every map (thanks idkbythispoint)
- 30 new cosmetics
- Stat cheat detection
- Vote kicking
- Server Gamemode marking
- Mute icon
- Made level load/elevator buttons not function (thanks BreadSoup)

Adjusted:
- Made player constraints off by default
- Physics interactions

SDK:
- Made bit SDK script have separate function for removing bits
- Scripts for avatar cosmetic attach points
- Added achievement machine placer

Internal:
- VC code cleanup
- Optimized coroutine usage

## v1.4.1
- Fix Despawn All button despawning the player on custom maps

## v1.4.0
- TeamDeathmatch rewrite (thanks adamdev)
- Removed alpha value from custom name colors
- Prevented rich text in notifications unless a bool is enabled
- Cleaned up notification creation (Fusion addons require update)
- Improved some grip logic
- Added support for custom server names
- Made the BoneMenu category appear at the top of the menu
- Made Gun.EjectMagazine only work if the local player is holding the gun
- Improved impersonation check
- Added basic upgrading system to point items
- Added option to specifically disable player constraining
- Added server button to despawn all props

## v1.3.2
- Fixed steam_api64.dll not loading correctly on some computers

## v1.3.1
- Fixed static grip events not syncing
- Fixed custom gun UIs falling out of the world
- Fixed grips breaking when a player who's holding them disconnects
- Optimized GameObject finding and path getting
- Made GameObject finding and path getting use tasks
- Make errors from failed network layers log why they failed
- Added checks to pasting text to make sure you aren't pasting a file
- Added point shop item for passive income (see BitMiner)
- Added LoDs to point shop items to make them disappear
- Added SteamID checking for important users to prevent impersonation
- Prevented users from joining a server if they don't have the custom map

## v1.3.0
- Made player voicechat quarter volume when its 2D
- Fixed the multiple go-karts in Monogon Motorway not spawning
- Fixed spamming join in BoneMenu crashing your game
- Fixed many causes of crashes by disabling AsyncCallbacks
- Made the body log disable on players if they haven't unlocked it
- Added character limit to usernames and nicknames
- Added server option to force base game avatars
- Implemented Array Pooling (better memory usage, more performance)
- Implemented fixed pointer buffers (better memory usage, more performance)

## v1.2.1
- Made lobby metadata update on a timer
- Made lobby metadata update when gamemodes are changed
- Fixed steam_api_64.dll writing into the incorrect folder
- Fixed version comparison requiring the patch number to be the same

## v1.2.0
- Marked other multiplayer mods as incompatible
- Actually removed wacky willy
- Removed STEAM network layer and made SteamVR the default
- Improved performance in a few areas
- Fixed BONELAB Hub unloading parts of the level when rapidly moving through chunks
- Added SDK functions for altering team scores
- Added catchup for descent events
- Actually fixed spawned objects in campaign levels not being caught up
- Disabled grips on players after death for a few seconds to prevent flinging
- Added Deathmatch and Team Deathmatch spawnpoints to Halfway Park
- Added SDK option to force disable late joining
- Added SDK proxy script to clear the player's inventory
- Added checks for the player rep's avatars becoming the incorrect avatar
- Added SDK ult events for when the player becomes part of a team
- Added notification when quick muting/unmuting
- Publicized Fusion asset classes for modders to use
- Added server setting to set required permission for constrainer
- Added Auto-Updater
- Increased text capacity of changelog

## v1.1.1
- Removed wacky willy
- Fixed issue with changing sorting mode while refreshing lobbies

## v1.1.0
- Fixed joining in progress gamemodes not giving you ammo or setting spawns
- Fixed descent noose damaging everyone
- Improved catchup spawns for campaign levels
- Fixed deleting guns in your hand causing UI to break
- Synced curr_Health value for mod creators
- Added quick mute button in radial menu
- Added info in gamemodes tab when not in a server
- Changed gamemode late joining to be on by default
- Made ammo box collection client side
- Made public lobby refreshing happen over a few frames so you can see progress
- Halved voice volume in loading screens
- Made weapons auto holster in gamemodes

## v1.0.1
- Made public lobbies default to sorting by level
- Fixed gamemodes sometimes not functioning after joining a server
- Improved the amount of lobbies that can be searched
- Added notice when an empty networking layer is selected

## v1.0.0
- Initial release