# BONELAB Fusion codebase map

Source inspected: `Lakatrazz/BONELAB-Fusion` at `4b0505be680b3232f3b2db862a3dcd21bba43de7`.

This is a focused developer map, not a complete API reference. It answers the questions from issue #107: where the main pieces live, how the runtime enters and updates them, how networking is layered, and where a gameplay/economy feature would plug in.

Companion pages on this branch:

- [`DIRECTORY_GUIDE.md`](DIRECTORY_GUIDE.md) - directory-by-directory orientation for `LabFusion/src/` and `BonelabSupport/`.
- [`RUNTIME_AND_NETWORKING.md`](RUNTIME_AND_NETWORKING.md) - startup, frame order, scene/gamemode lifecycle and message flow.
- [`CS_ECONOMY_EXTENSION_GUIDE.md`](CS_ECONOMY_EXTENSION_GUIDE.md) - concrete host-authoritative design for the Counter-Strike-style economy mentioned in issue #107.

## Top-level projects

- `LabFusion/` - main Fusion mod. This contains the MelonLoader entrypoint, networking stack, player/entity replication, gamemode SDK, menus, point shop, voice, scene synchronization and most common multiplayer behavior.
- `BonelabSupport/` - BONELAB-specific support module. It contains game-specific patches, messages and extenders for BONELAB controllers, arena/campaign behavior, props, player vitals and other BONELAB-only integration points.
- `LabFusionUpdater/` - updater/plugin project used to keep Fusion releases current.
- `LabFusion.sln` - solution entrypoint for the C# projects.

Inside `LabFusion/`, the most useful source roots for feature work are:

- `src/Network/` - transport abstraction, lobby/matchmaking interfaces, connection lifecycle, serialization and message routing.
- `src/Entities/` - networked entities, players, props, ownership/registration and per-frame entity updates.
- `src/Player/` - local/network player behavior and player IDs.
- `src/Scene/` - scene/level lifecycle and network scene synchronization.
- `src/SDK/Gamemodes/` - base gamemode API, built-in gamemodes, teams and gamemode metadata.
- `src/SDK/Points/` - persistent local point/"bit" balance, purchasable point items and point-shop state.
- `src/SDK/Messages/` and `src/Network/Messages/` - module/native message contracts and handlers.
- `src/Senders/` - higher-level helpers that construct and relay common messages.
- `src/RPC/` - RPC registration and network asset spawning/request helpers.
- `src/Menu/` and `src/UI/` - Fusion menu data/pages, matchmaking UI and popup UI.
- `src/Support/` - support-module loading/coordination.
- `src/Utilities/Fusion/MultiplayerHooking.cs` - common multiplayer lifecycle hooks used by systems such as gamemodes.

## Runtime entrypoint and lifecycle

The main entrypoint is `LabFusion/src/Mod.cs`, class `FusionMod : MelonMod`.

### Early initialization

`FusionMod.OnEarlyInitializeMelon()` performs the earliest state setup:

1. stores the mod/assembly references;
2. deletes temporary mod-download directories;
3. initializes the persistent-data path;
4. loads the Steam API;
5. initializes player-data control;
6. hooks point-item events;
7. initializes the RPC manager.

### Main initialization

`FusionMod.OnInitializeMelon()` wires the systems that the rest of the runtime depends on. In rough order it:

1. loads Fusion files and safety lists;
2. initializes local-player and voice state;
3. loads base/support modules;
4. discovers message handlers, grab handlers, network layers, gamemodes, point items, achievements and RPCs by scanning the Fusion assembly;
5. initializes network-entity and network-player managers;
6. initializes popups and gamemode managers;
7. creates preferences/permissions/lobby/menu state;
8. initializes scene loading and `NetworkSceneManager`;
9. initializes `NetworkLayerManager`.

The important architectural pattern is that many feature types are **registered from the assembly**, rather than hard-coded into one central switch. For a new gameplay system, look for the registration base class/attribute first before adding global initialization code.

## What happens every frame

`FusionMod.OnUpdate()` is the clearest high-level execution trace:

1. network byte counters reset;
2. queued mod downloads update;
3. time references update;
4. scene-load state advances;
5. popup UI updates;
6. `NetworkTickManager.OnUpdate()` decides whether this frame is a network tick;
7. network-player and network-entity managers update;
8. local Fusion player state updates;
9. voice chat updates;
10. the active network transport's `OnUpdateLayer()` runs;
11. delayed disconnects update through `NetworkConnectionManager`;
12. shared multiplayer update hooks fire;
13. the active gamemode updates;
14. delayed events run at the end of the frame.

`FusionMod.OnFixedUpdate()` runs physics-oriented work:

- local player fixed update;
- player-data controller fixed update;
- network player/entity fixed updates;
- multiplayer fixed-update hooks;
- gamemode fixed update.

`FusionMod.OnLateUpdate()` runs player/entity late updates, flushes leftover network-layer work, invokes late-update hooks, then updates the gamemode late phase.

### Network tick rate

`LabFusion/src/Network/NetworkTickManager.cs` defines a `20f` tick rate (`0.05s` between ticks). It exposes `IsTickThisFrame`, which systems can use to avoid sending state every render frame, plus an interpolation decay value for smoothing between updates.

## Networking architecture

Fusion separates gameplay messages from the concrete transport.

### Transport abstraction

`LabFusion/src/Network/Layers/NetworkLayer.cs` is the transport contract. A layer supplies:

- platform identity;
- host/client state;
- lobby and matchmaker implementations;
- voice manager;
- login/logout;
- start/disconnect operations;
- send-to-server, server-to-client and broadcast operations;
- per-frame/late-frame transport updates.

`NetworkLayerManager` owns the currently active layer, handles layer login/logout events and initializes/deinitializes the selected transport. `NetworkLayerDeterminer` selects the target layer; current concrete implementations live under `src/Network/Layers/Steam/` and `src/Network/Layers/Proxy/`.

That means game logic should normally depend on `NetworkInfo`, `MessageRelay` and message handlers, not directly on a Steam socket class.

### Sending a message

The common path is:

`gameplay/system code`
→ `MessageRelay.RelayNative(...)` or `RelayModule(...)`
→ serialize data with `NetWriter`
→ create a `NetMessage`
→ choose route/channel (`ToServer`, `ToClients`, `ToOtherClients`, `ToTarget`, etc.)
→ `MessageSender`
→ active `NetworkLayer` transport.

If the host is also the logical recipient, `MessageSender` can feed the message back through local handling rather than requiring an actual loopback network packet.

### Receiving/handling

Incoming messages ultimately reach registered handlers. `MessageHandler` provides common validation/lifecycle behavior:

- optional `NetAttribute` gates run before handling;
- awaitable attributes can delay the final handler;
- handlers declare expected server/client receiver type;
- exceptions are caught and logged around the concrete `Handle(...)` implementation;
- server-side handlers can reject a message before relay through `OnPreRelayMessage(...)`.

The actual message payload types are grouped by purpose in `src/Network/Messages/` (server connection/state, entities, representation, scene levels, spawning, gamemodes, point shop, interaction, SDK RPCs, etc.).

## Where a Counter-Strike-style economy would fit

There are two existing systems that are easy to confuse:

1. **Gamemode state** (`src/SDK/Gamemodes/`), which is already network-oriented and host-authoritative through gamemode metadata/messages.
2. **Point-shop bits** (`src/SDK/Points/`), whose balance is persisted locally in `point_shop.dat` by `PointSaveManager`.

For a competitive round economy, the second system should **not automatically be treated as the player's authoritative match money**. `PointSaveManager` persists a local cross-session bit balance and is designed around unlock/equip/upgrade state. A match economy such as `$800 start → round reward → weapon purchase → reset on new match` should instead be owned by the active gamemode/server and synchronized as match state.

### Recommended first code path to inspect

- `SDK/Gamemodes/Gamemode.cs`
  - lifecycle callbacks: `OnGamemodeStarted`, `OnGamemodeStopped`, `OnLevelReady`, player join/leave and update phases;
  - `Metadata` is already wired so the host can set/remove replicated gamemode values.
- `SDK/Gamemodes/GamemodeManager.cs`
  - selects/starts/stops the active gamemode and advances its update loop.
- `SDK/Gamemodes/Teams/TeamManager.cs`
  - maps players to teams through gamemode metadata and provides team assignment/query helpers.
- existing built-in team/round gamemodes under `SDK/Gamemodes/Built In/`
  - concrete examples for round lifecycle, player events and team-aware rules.
- `Network/Messages/Gamemodes/` and `Senders/Gamemode/`
  - existing synchronized gamemode control/metadata message patterns.

### Suggested architecture for match money

Keep authoritative balances on the host, keyed by `PlayerID.SmallID` (or another stable per-match player key), then expose only the minimum replicated state needed by clients. A basic sequence would be:

1. initialize balances when the gamemode starts / a late player joins;
2. server changes balances on validated events (round result, kill/objective reward, purchase);
3. reject purchase requests when balance, phase or inventory rules fail;
4. replicate the resulting balance/purchase outcome to clients;
5. reset or carry balances according to explicit round/match rules;
6. keep the persistent point-shop `BitCount` separate unless the feature deliberately wants cross-session progression.

This uses Fusion's existing host-authority and message-routing model instead of making every client independently calculate its own economy state.

## Next documentation slices

If this map is useful, the next high-value pages would be:

1. a directory-by-directory reference for `LabFusion/src/` and `BonelabSupport/`;
2. a message-flow diagram from socket receive → native/module handler → gameplay state;
3. a scene-load / join / catch-up sequence;
4. a gamemode extension guide using one built-in gamemode as a worked example;
5. a BONELAB-specific patch/extender map showing which functionality lives outside the generic Fusion layer.

Those pages would turn this map into the fuller onboarding documentation requested in issue #107.
