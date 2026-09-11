# Counter-Strike-style economy extension guide

Issue #107 specifically mentions adding a Counter-Strike-style economy. Fusion already has most of the infrastructure needed, but the existing persistent point shop should not be used as the authoritative match wallet.

## Existing systems to reuse

### Gamemode lifecycle

`LabFusion/src/SDK/Gamemodes/Gamemode.cs` and `GamemodeManager.cs` provide:

- match selection/start/stop;
- player join/leave callbacks while the mode is active;
- per-frame/fixed/late update phases;
- host-authoritative synchronized metadata;
- trigger relay support;
- ready-condition checks.

### Teams and scores

`TeamDeathmatch.cs` is the closest built-in example to read first.

It demonstrates:

- a `TeamManager` registered to the gamemode;
- two teams stored through synchronized gamemode metadata;
- host-only score mutation;
- player-action events for kills;
- late-join team assignment;
- mode start/stop and level-ready setup;
- mode-specific settings and local UI feedback.

### Existing persistent points

`PointSaveManager` stores `BitCount`, unlocked/equipped items and upgrades in `point_shop.dat`. `Deathmatch` and `TeamDeathmatch` award Bits after matches through `PointItemManager.RewardBits(...)`.

That system is useful for **persistent progression**, but it has different semantics from CS match money:

- it survives between sessions;
- it is stored locally;
- it is tied to point-shop unlock/equip/upgrade state;
- built-in modes award it as a post-match reward.

Do not make `$800 -> purchases -> round reward -> reset/carry` depend on that file unless persistent cross-session money is deliberately desired.

## Recommended match state

Keep the authoritative economy state on the host, owned by the active gamemode.

A minimal model is:

```text
Dictionary<byte, int> balances              // PlayerID.SmallID -> match money
Dictionary<byte, PurchaseState> purchases   // optional round purchase state
RoundPhase phase                            // Buy, Live, RoundEnd, MatchEnd
```

The client should never send a final balance. It should only request an action such as `Buy("weapon-barcode")`.

The host then validates:

1. the sender is a connected player;
2. the gamemode is active;
3. the round is in a buy-allowed phase;
4. the requested item exists in the server's catalog;
5. the player meets team/loadout restrictions;
6. the player has sufficient match balance;
7. duplicate/limit rules allow the purchase.

Only after validation should the host subtract money, grant/spawn the purchase and replicate the result.

## Two synchronization options

### Option A: gamemode metadata

Use per-player keys, for example:

```text
economy.balance.<smallId> = "800"
economy.phase = "Buy"
```

Advantages:

- host-only `TrySetMetadata(...)` is already enforced by `Gamemode`;
- reliable replication already exists through `GamemodeSender`;
- clients automatically get change callbacks;
- late join/catch-up can use the same gamemode state.

This is the simplest choice for balances, phase and other small state.

### Option B: custom purchase messages

Use a custom client -> server message for purchase requests, then a server -> client result message.

This is better when the request needs structured input or explicit rejection reasons. The existing network stack gives the pattern:

```text
client BuyRequest(itemId)
  -> MessageRelay ... ToServer
  -> server handler
  -> validate against host state
  -> mutate host balance/inventory
  -> reliable BuyResult / metadata update
  -> client UI reflects authoritative result
```

The server handler should use `ExpectedReceiverType.ServerOnly` and/or `OnPreRelayMessage(...)`/other validation hooks where appropriate.

A practical design is to use **custom messages for commands** and **gamemode metadata for state**.

## Suggested lifecycle

### `OnGamemodeStarted()`

Host:

- clear any previous match economy state;
- initialize all connected players with starting money;
- initialize round phase;
- initialize loss-streak/team-economy state if the design uses it.

All clients:

- initialize economy UI hooks/state listeners;
- clear stale local presentation state.

### `OnPlayerJoined(PlayerID)`

Host:

- assign the correct starting/late-join balance;
- publish the new player's balance;
- send/ensure current round phase and catalog rules are available.

### Round start

Host:

- move phase to `Buy`;
- apply per-round income/carry rules;
- clear round-only purchase limits;
- publish balances changed by round settlement.

### Buy period

Client:

- request a purchase.

Host:

- validate the request;
- deduct price;
- grant the item through an appropriate server-authoritative path;
- publish the new balance and purchase result.

### Round end

Host:

- calculate winner/loser/objective rewards;
- update loss streaks if used;
- update each balance with explicit max-money clamping;
- transition phase to round end/intermission.

### `OnGamemodeStopped()`

- remove economy-specific metadata or let the gamemode's nonpersistent metadata clear;
- remove local UI hooks/overrides;
- do **not** write the match balance into `PointSaveManager` unless the design explicitly converts some result into persistent Bits.

## Concrete files to inspect/change

For a new economy-based mode:

1. `LabFusion/src/SDK/Gamemodes/Gamemode.cs`
   - lifecycle and metadata rules.
2. `LabFusion/src/SDK/Gamemodes/Built In/TeamDeathmatch.cs`
   - strongest built-in example of teams + host-authoritative scoring + player actions.
3. `LabFusion/src/SDK/Gamemodes/Teams/TeamManager.cs`
   - synchronized team membership.
4. `LabFusion/src/SDK/Gamemodes/Score/*.cs`
   - metadata-backed synchronized score patterns.
5. `LabFusion/src/SDK/Metadata/NetworkMetadata.cs`
   - synchronized key/value state.
6. `LabFusion/src/SDK/Metadata/MetadataVariable.cs`
   - typed/serialized metadata convenience layer.
7. `LabFusion/src/Senders/Gamemode/GamemodeSender.cs`
   - existing gamemode metadata/trigger network sends.
8. `LabFusion/src/Network/Messages/Gamemodes/`
   - existing gamemode network handlers.
9. `LabFusion/src/Network/Helpers/MessageRelay.cs`
   - routing a new command/result message.
10. `LabFusion/src/Network/Messages/MessageHandler.cs`
    - server/client expectations and pre-relay validation.

If actual BONELAB item spawning/granting needs BONELAB-only behavior, then follow the existing patterns in `BonelabSupport/` rather than putting game-specific patches into generic gamemode infrastructure.

## What not to do

- Do not let clients calculate and submit their own final balance.
- Do not use `PointSaveManager.SetBitCount()` as the match wallet just because it already stores an integer currency.
- Do not run economy mutation on every client in `OnUpdate()` and hope values stay in sync.
- Do not put a BONELAB-only spawn patch into generic `LabFusion` if it belongs in `BonelabSupport`.
- Do not send balance state every render frame; update it on economy events.

## Minimal implementation sequence

A low-risk first implementation can be built in this order:

1. new gamemode with host-owned `balances` dictionary;
2. balance replication through gamemode metadata;
3. start money + late-join initialization;
4. one hard-coded buyable item and a validated purchase request;
5. round-end reward and max-money clamping;
6. client display of authoritative balance;
7. catalog/settings extraction;
8. team-specific pricing/loadout rules if needed.

That produces a testable vertical slice before adding a full CS-like shop.
