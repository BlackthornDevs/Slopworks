# Belt Simulation Tick Design

**Date:** 2026-03-14
**Branch:** kevin/belt-simulation-tick
**Status:** Approved

## Problem

Belts are placed and visually connect to machines/storage/other belts, but items don't actually flow. The simulation layer (FactorySimulation) ticks belts and machines, but nothing creates the connections that transfer items between them.

The vertical slice used grid-based PortNodes, ConnectionResolver, and Inserters (with swing timers) to wire things up. The multiplayer system uses world-space BeltPorts with proximity-based connections. The Inserter concept doesn't apply -- belts connect directly to machine/storage ports like Satisfactory, not through a separate placeable arm.

## Design

### Generalize BeltNetwork

BeltNetwork already handles belt-to-belt transfers with the right pattern: extract from source, insert into destination, hold item if rejected, retry next tick. Generalize it to handle all connection types.

Change `BeltConnection` from:
```csharp
struct BeltConnection {
    BeltSegment From;
    BeltSegment To;
    string HeldItemId;
}
```

To:
```csharp
struct BeltConnection {
    IItemSource Source;
    IItemDestination Destination;
    string HeldItemId;
}
```

Tick logic stays identical but calls `Source.TryExtract()` / `Destination.TryInsert()` instead of BeltSegment-specific methods.

Keep the existing `Connect(BeltSegment, BeltSegment)` as a convenience that wraps them in `BeltOutputAdapter`/`BeltInputAdapter`. Add `Connect(IItemSource, IItemDestination)` for belt-to-machine/storage.

Update `Disconnect` to work by source/destination identity.

### Wire connections at placement time

In `GridManager.CmdPlaceBelt`, after `RegisterBeltOnNearbyPorts`, create simulation connections:

1. At each belt endpoint, find the nearby BeltPort via proximity (already done)
2. Determine the belt's own port direction at that endpoint (Input or Output, from `flipBeltPorts`)
3. `GetComponentInParent<NetworkMachine/NetworkStorage/NetworkBeltSegment>()` on the nearby port's parent to get the simulation object
4. If owner not found, log warning and skip: `"belt: port found at (x,y,z) but no simulation owner on parent, skipping connection"`
5. Create the appropriate adapter pair based on directions:
   - Belt Output end + Machine/Storage Input port: `BeltOutputAdapter(beltSegment)` source, `MachineInputAdapter/StorageContainer` destination
   - Belt Input end + Machine/Storage Output port: `MachineOutputAdapter/StorageContainer` source, `BeltInputAdapter(beltSegment)` destination
   - Belt Output end + Belt Input port: `BeltOutputAdapter` source, `BeltInputAdapter` destination (same as current belt-to-belt)
6. Register with `BeltNetwork.Connect(source, destination)`

### What stays the same

- **FactorySimulation tick order**: belts tick, belt network tick, machines tick. No change.
- **NetworkBeltSegment.ServerSyncState()**: pushes item positions to SyncList. No change.
- **BeltItemVisualizer**: already wired in CmdPlaceBelt. Reads SyncList positions. Should work once items flow.
- **Adapters**: BeltOutputAdapter, BeltInputAdapter, MachineOutputAdapter, MachineInputAdapter all exist and work. StorageContainer already implements IItemSource/IItemDestination directly.
- **BeltPort MonoBehaviour**: unchanged. Still tracks visual connections. The simulation connection is separate.

### What becomes dead code in multiplayer

- **Inserter** (swing timer transfer) -- not used, belts connect directly
- **ConnectionResolver** (grid-based auto-wiring) -- replaced by proximity wiring in GridManager
- **PortNodeRegistry** (grid cell lookup) -- replaced by BeltPort proximity
- **PortNode** (grid-based port) -- replaced by BeltPort MonoBehaviour
- **GridManager.AutoWire/TryCreateInserter/GetItemSource/GetItemDestination** -- replaced by the new wiring in CmdPlaceBelt

These stay in the codebase for now. Cleanup is a separate task.

## Connection types

| Belt endpoint | Nearby port | Source | Destination |
|---|---|---|---|
| Belt Output | Machine Input | BeltOutputAdapter | MachineInputAdapter |
| Belt Input | Machine Output | MachineOutputAdapter | BeltInputAdapter |
| Belt Output | Storage Input | BeltOutputAdapter | StorageContainer |
| Belt Input | Storage Output | StorageContainer | BeltInputAdapter |
| Belt Output | Belt Input | BeltOutputAdapter | BeltInputAdapter |
| Belt Input | Belt Output | BeltOutputAdapter | BeltInputAdapter |

## Backpressure

Handled naturally by the existing pattern:
- Belt full at input end: `TryInsertAtStart` returns false, item held in `HeldItemId`, retried next tick
- Machine input buffer full: `MachineInputAdapter.TryInsert` returns false, same hold/retry
- Storage full: `StorageContainer.TryInsert` returns false, same hold/retry
- Machine output buffer empty: `MachineOutputAdapter.TryExtract` returns false, nothing transferred

No special backpressure code needed. The adapter interfaces handle it uniformly.
