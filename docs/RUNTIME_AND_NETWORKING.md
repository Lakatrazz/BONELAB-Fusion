# Runtime and networking flow

This page traces Fusion from MelonLoader startup through frame updates and message delivery. Paths refer to `Lakatrazz/BONELAB-Fusion` commit `4b0505be680b3232f3b2db862a3dcd21bba43de7`.

## 1. Startup

The main entrypoint is `LabFusion/src/Mod.cs`, `FusionMod : MelonMod`.

### `OnEarlyInitializeMelon()`

The early phase establishes data and APIs before most feature discovery:

1. cache the mod/assembly references;
2. remove temporary mod-download directories;
3. initialize the persistent-data path;
4. load Steam API support;
5. initialize player-data control;
6. hook point-item events;
7. initialize the RPC manager.

### `OnInitializeMelon()`

The main phase wires the runtime:

1. load Fusion files and safety lists;
2. initialize local-player and voice state;
3. load base/support modules;
4. scan the Fusion assembly for native message handlers, grab handlers, network layers, gamemodes, point items, achievements and RPCs;
5. discover entity components;
6. initialize network entity/player managers;
7. initialize popups and gamemode managers;
8. initialize preferences, permissions, lobby state and menus;
9. initialize scene loading and the network scene manager;
10. initialize the network layer manager.

The repeated `Register...FromAssembly` / `Load...` pattern matters: extension points are often discovered by type rather than manually added to `Mod.cs`.

## 2. Scene lifecycle

`FusionMod` registers scene-related hooks after core initialization.

When the main scene becomes ready, `OnMainSceneInitialized()`:

- cleans network-entity IDs;
- caches rig information;
- initializes persistent assets and constrainer helpers;
- invokes `MultiplayerHooking.OnMainSceneInitialized`;
- lets `FusionPlayer` perform its scene initialization.

A delayed scene initialization then makes sure the rig exists and creates the Fusion menu.

`GamemodeManager` listens to scene hooks too:

- when a main scene initializes, the host stops an active gamemode if `AutoStopOnSceneLoad` is true;
- when the server target level is loaded, an already-running gamemode receives `OnLevelReady()`.

This distinction is useful for game modes: `OnGamemodeStarted()` owns match-state start, while `OnLevelReady()` is where level-local objects/spawns/settings should be applied.

## 3. Per-frame order

`FusionMod.OnUpdate()` gives the clearest top-level order:

```text
reset byte counters
  -> mod download queue
  -> time references
  -> scene-load state
  -> popup UI
  -> network tick calculation
  -> network players
  -> network entities
  -> local Fusion player
  -> voice
  -> active transport OnUpdateLayer
  -> delayed disconnects
  -> shared multiplayer OnUpdate hooks
  -> active gamemode Update
  -> delayed events
```

`OnFixedUpdate()` handles physics-oriented work:

```text
time fixed update
  -> local player
  -> player-data controller
  -> network players
  -> network entities
  -> multiplayer fixed hooks
  -> active gamemode FixedUpdate
```

`OnLateUpdate()` handles late player/entity work, flushes transport work, runs late multiplayer hooks, then invokes the active gamemode's late update.

## 4. Network tick

`LabFusion/src/Network/NetworkTickManager.cs` defines a 20 Hz tick (`0.05s` between ticks). Systems can use `IsTickThisFrame` to avoid sending state every render frame. The same manager exposes an interpolation decay value for smoothing between ticks.

This is the cadence to consider for replicated state that needs regular updates. Event-style state such as a purchase result or round reward should normally be sent only when it changes.

## 5. Transport abstraction

`LabFusion/src/Network/Layers/NetworkLayer.cs` is the transport contract. It provides:

- host/client state;
- platform identity;
- lobby and matchmaker objects;
- voice manager;
- start/disconnect operations;
- send-to-server, send-from-server and broadcast operations;
- per-frame and late-frame layer updates.

`NetworkLayerManager` owns the active layer. Gameplay code should normally use `NetworkInfo`, `MessageRelay`, `MessageSender` and registered message handlers rather than talking directly to Steam/Proxy transport implementation classes.

## 6. Native message send path

For a typed payload implementing `INetSerializable`, the common path is:

```text
feature code
  -> MessageRelay.RelayNative(data, tag, route)
  -> NetWriter
  -> NetMessage.Create(...)
  -> route switch
  -> MessageSender
  -> active NetworkLayer
```

`MessageRoute` controls where the message goes:

- `None` / `ToServer` -> server;
- `ToClients` -> host broadcasts, client sends to server;
- `ToOtherClients` -> host broadcasts except sender, client sends to server;
- `ToTarget` / `ToTargets` -> host can target specific clients; a client sends the request through the server.

`MessageSender` also handles host loopback. If the host logically receives a server-targeted message, it can feed the message directly through `NativeMessageHandler.ReadMessage(...)` instead of requiring a real network round trip.

## 7. Receive/handler path

Incoming native messages are dispatched through the registered message-handler system.

`MessageHandler` provides the common pipeline:

1. initialize `NetAttribute` guards;
2. stop early if a guard rejects handling;
3. wait for an awaitable guard if needed;
4. verify expected server/client conditions;
5. call the concrete `Handle(...)` implementation;
6. catch/log exceptions around handler execution.

On server relay paths, a handler can reject a message before it is forwarded by overriding `OnPreRelayMessage(...)`.

That is the correct layer for validating a client-originating purchase request: do not trust a client-provided final balance or price.

## 8. Gamemode synchronized metadata

Each `Gamemode` owns a `NetworkMetadata` instance.

A normal `TrySetMetadata(key, value)` from the gamemode delegates into `Gamemode.OnTrySetMetadata(...)`, which refuses non-host callers and then sends a `GamemodeMetadataSet` message through `GamemodeSender`.

So the flow is:

```text
host gamemode code
  -> Metadata.TrySetMetadata(key, value)
  -> Gamemode.OnTrySetMetadata
  -> GamemodeSender.SendGamemodeMetadataSet
  -> reliable message to clients
  -> client metadata updated
  -> OnMetadataChanged callbacks fire
```

`TeamManager` and the score keepers already use this pattern. It is appropriate for relatively small replicated state such as team membership, score, mode settings and player balances.

## 9. Gamemode lifecycle

`GamemodeManager` owns the active gamemode.

Host-only state transitions are reflected through gamemode metadata:

- `SelectGamemode()` -> selected metadata key;
- `ReadyGamemode()` / `UnreadyGamemode()` -> ready key;
- `StartGamemode()` / `StopGamemode()` -> started key.

When those metadata values change locally, callbacks invoke the corresponding gamemode lifecycle methods. A started mode then receives `Update`, `FixedUpdate` and `LateUpdate` from the main Fusion loop.

A new gameplay mode should therefore put match rules inside the gamemode lifecycle rather than adding another global update system.

## 10. Join and late-join implications

`Gamemode.GamemodeRegistered()` subscribes the mode to `MultiplayerHooking.OnPlayerJoined` and `OnPlayerLeft`. The protected `OnPlayerJoined(PlayerID)` and `OnPlayerLeft(PlayerID)` callbacks are only invoked while that gamemode is started.

For a match economy this is where the host should initialize/remove per-player match state. `TeamDeathmatch` also demonstrates a useful late-join pattern: if the host receives a player join while the match is active, it assigns that player to the smallest team.

## Practical debugging path

When a synchronized feature does not behave as expected, check in this order:

1. Is the active gamemode actually selected/started?
2. Is the mutation running only on the host when it should be authoritative?
3. Is the metadata/message request being created?
4. Is the route correct for server/client direction?
5. Is the active `NetworkLayer` present and connected?
6. Does the handler expect server, clients or both?
7. Is an attribute guard stopping/delaying handling?
8. Does the recipient update local metadata/state and fire the expected callback?
