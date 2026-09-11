# Directory guide

This guide is for developers entering Fusion without already knowing the codebase. It is based on `Lakatrazz/BONELAB-Fusion` commit `4b0505be680b3232f3b2db862a3dcd21bba43de7`.

The rule of thumb is:

- `LabFusion/` owns reusable multiplayer infrastructure and Fusion gameplay systems.
- `BonelabSupport/` owns BONELAB-specific hooks and behavior that cannot live in the generic Fusion layer.
- `LabFusionUpdater/` is the updater/plugin.

## `LabFusion/src/`

| Directory | What lives here | Start with |
| --- | --- | --- |
| `Audio/` | Shared audio helpers and references used by Fusion systems. | Follow references from a gamemode or UI feature rather than starting here. |
| `Data/` | Persistent/config data models, file containers, lobby/mod metadata and serialization-oriented data structures. | `PersistentData`, `DataSaver`, then the `Containers/`, `Files/`, `Lobbies/`, `Mods/` and `Serializables/` subfolders. |
| `Debugging/` | Debug-only diagnostics and development helpers. | Useful when adding diagnostics, not a normal feature entrypoint. |
| `Downloading/` | Mod/download coordination. `ModIO/` contains Mod.io-specific download logic. | `ModDownloadManager`, then `ModIO/` for Mod.io flows. |
| `Entities/` | Network entity registration, replicated entities, player entities, props, ownership/validation and component extenders. | `NetworkEntityManager`, then `Player/`, `Props/`, `Components/` and `Extenders/`. |
| `Exceptions/` | Fusion-specific exception types. | Search for the exception from the call site that throws it. |
| `Extensions/` | General C# extension methods. | Treat as utility code; avoid placing feature state here. |
| `Grabbables/` | Grab-group registration and synchronization. | `GrabGroupHandler` and concrete handlers. |
| `Marrow/` | Integration helpers around SLZ Marrow: asset warehouse, audio, circuits, combat, pooling, scenes, zones, serialization and patches. | Use when a feature needs BONELAB/Marrow object behavior but is still generic enough for Fusion. |
| `Math/` | Math helpers used by gameplay/network code. | Utility-only. |
| `Menu/` | Fusion menu model, pages, matchmaking/gamemode settings and popup data. | `MenuCreator`, then `Pages/`, `Matchmaking/` or `Gamemodes/`. |
| `MonoBehaviours/` | Fusion-owned Unity behaviours. | Search by component name from the feature using it. |
| `Network/` | Transport abstraction, connection/lobby state, native messages, serialization and routing. | `NetworkLayer`, `NetworkLayerManager`, `MessageRelay`, `MessageSender`, `NativeMessageHandler`. |
| `Patching/` | Harmony/IL2CPP patches and patch helpers for generic Fusion behavior. | `Patches/` for concrete hooks. |
| `Permissions/` | Server/client permission model. | Check this before exposing a host-only action to clients. |
| `Player/` | Player identity plus local/network player state. | `PlayerID`, `PlayerIDManager`, `Local/`, `Network/`. |
| `Preferences/` | User/server settings and preference registration. | `FusionPreferences`, then `Client/` and `Server/`. |
| `Representation/` | Networked player/avatar representation logic. | Relevant when changing how remote players are represented. |
| `RPC/` | RPC discovery/registration and network asset/spawn request helpers. | `RpcManager` and existing RPCs as examples. |
| `Safety/` | Safety filters, lists, patches and trackers. | Treat as policy/validation infrastructure rather than gameplay state. |
| `Scene/` | Fusion level lifecycle and network scene synchronization. | `FusionSceneManager`, `NetworkSceneManager`, `Scene/Network/`. |
| `SDK/` | Public-ish extension surface: gamemodes, metadata, modules, points, messages, scenes, triggers, cosmetics, achievements and lobbies. | For a new game mode or feature, this is usually the first place to inspect. |
| `Senders/` | Higher-level helpers that construct and relay common network messages. | `Gamemode/`, `Player/`, `Props/`, `Server/`. |
| `Support/` | Support-module discovery/coordination. | `SupportManager`; this is how game-specific support is loaded. |
| `UI/` | Fusion runtime UI helpers/components. | `Popups/`, `Cup Board/`, `Info Box/`. |
| `Utilities/` | General/internal/Fusion-specific utility code and lifecycle hooks. | `Utilities/Fusion/MultiplayerHooking.cs` is especially important. |
| `Voice/` | Voice transport/playback integration. | `VoiceHelper`, voice managers and `Voice/Unity/`. |

## `LabFusion/src/Network/`

Networking is split so gameplay code does not need to know whether the transport is Steam or Proxy.

- `Layers/` defines and selects transports.
  - `NetworkLayer.cs` is the abstract transport contract.
  - `NetworkLayerManager.cs` owns the active layer.
  - `NetworkLayerDeterminer.cs` chooses a target layer.
  - transport implementations live below `Layers/Steam/` and `Layers/Proxy/`.
- `Helpers/` is the normal gameplay-facing network API.
  - `MessageRelay.cs` serializes typed data and chooses a route.
  - `MessageSender.cs` bridges routing decisions into the active `NetworkLayer`.
  - `NetworkInfo.cs` exposes host/server/connection state.
  - `MetadataHelper.cs` and other helpers support common synchronized-state patterns.
- `Messages/` contains the message framework plus native message groups.
  - `NetMessage`, `ReceivedMessage`, `ReadableMessage` are the envelope/receive structures.
  - `MessageHandler` is the common handler base.
  - `NativeMessageHandler` discovers and dispatches native handlers.
  - feature-specific messages live in subdirectories such as `Gamemodes/`.
- `Serialization/` contains `NetReader`, `NetWriter` and serializable contracts.
- `Lobbies/` contains lobby/matchmaker contracts and lobby metadata.
- `Data/` contains compact payload structures shared by network code.
- `Internal/` contains lower-level layer/server helpers used by the runtime loop.
- `Fallback/` contains the empty/fallback layer.

## `LabFusion/src/SDK/Gamemodes/`

This is the best reference for a Counter-Strike-style mode.

- `Gamemode.cs` is the base lifecycle and synchronized metadata owner.
- `GamemodeManager.cs` selects, readies, starts, stops and updates the active mode.
- `GamemodeRegistration.cs` discovers/registers gamemode implementations.
- `GamemodeConditionsChecker.cs` drives ready-condition checks.
- `GamemodeRoundManager.cs` handles configured level rotations and inter-round progression.
- `Teams/` provides `Team`, `TeamManager`, team logos and team music.
- `Score/` provides player/team score keepers backed by gamemode metadata.
- `Levels/` contains level rotation data.
- `Music/` contains gamemode music helpers.
- `Built In/` contains complete working examples: `Deathmatch`, `TeamDeathmatch`, `HideAndSeek`, `Juggernaut`, `SmashBones`, `Entangled`.

For an economy-heavy team mode, read `TeamDeathmatch.cs` first. It demonstrates team registration, host-only score mutation, late-join team assignment, per-frame host checks, settings, round start/stop behavior and client-facing feedback.

## `LabFusion/src/SDK/Points/`

This is a **persistent progression/shop system**, not a ready-made authoritative match-currency system.

- `PointSaveManager.cs` stores bought/equipped/upgraded items and `BitCount` in `point_shop.dat`.
- `PointItemManager.cs` discovers point items and handles rewards/purchases/equipment.
- `PointItem.cs` is the item abstraction.
- `PointShopHelper.cs` supports shop behavior.
- `BitEconomy.cs` currently only defines a high sentinel price (`PricelessValue`).
- `Built In/Passive/BitMiner.cs` is an example built-in point item.

A match economy should not directly reuse `PointSaveManager.BitCount` unless cross-session currency is intentionally part of the design.

## `BonelabSupport/`

`BonelabSupport` is where Fusion adapts generic multiplayer infrastructure to BONELAB-specific game objects and systems.

| Directory | Purpose |
| --- | --- |
| `AssetWarehouse/` | BONELAB-specific asset references and backlot/spawnable/avatar/disc lookups. |
| `Extenders/` | Entity/component/player extenders for BONELAB objects. |
| `Messages/` | BONELAB-specific network messages grouped by arena, campaign, interaction, level, player and props. |
| `Patching/` | BONELAB-specific patches grouped by arena, campaign, interaction, level, props, spawning, triggers and UI. |
| `SDK/` | BONELAB-facing SDK additions such as BitMart/music integration and achievements. |
| `Scene/` | BONELAB scene event integration, including arena/campaign/general/parkour/sandbox/tac-trial event groups. |
| `Serialization/` | BONELAB-specific serialized data such as body vitals. |

`BonelabSupport/BonelabModule.cs` is the module-level entrypoint to read before changing this project.

## Where to place a new feature

Use the narrowest layer that owns the concept:

- Generic synchronized game rules: `LabFusion/src/SDK/Gamemodes/`.
- Generic message contract: `LabFusion/src/Network/Messages/` plus a sender helper when useful.
- Generic local UI: `LabFusion/src/Menu/` or `LabFusion/src/UI/`.
- BONELAB-only patch/object behavior: `BonelabSupport/`.
- Cross-session unlock/shop progression: `LabFusion/src/SDK/Points/`.
- One match's authoritative score/money/round phase: active gamemode state, not the persistent point save.
