# Belt Simulation Tick Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make items flow through belts between machines, storage, and other belts in the multiplayer factory simulation.

**Architecture:** Generalize `BeltNetwork` from `BeltSegment`-only connections to `IItemSource`/`IItemDestination` connections. Wire simulation connections at belt placement time using proximity lookup on existing `BeltPort` components. No new classes needed -- existing adapters plug directly into the generalized network.

**Tech Stack:** Unity C#, FishNet NetworkBehaviour, NUnit EditMode tests

---

## What already exists (DO NOT recreate)

- `BeltNetwork` (`Scripts/Automation/BeltNetwork.cs`) -- belt-to-belt transfer with held item retry
- `IItemSource` / `IItemDestination` interfaces (`Scripts/Automation/`)
- Adapters: `BeltOutputAdapter`, `BeltInputAdapter`, `MachineInputAdapter`, `MachineOutputAdapter`
- `StorageContainer` implements `IItemSource` and `IItemDestination` directly
- `NetworkMachine.Machine`, `NetworkStorage.Container`, `NetworkBeltSegment.Segment` -- simulation object accessors
- `FactorySimulation.Tick()` calls `_beltNetwork.Tick()` at step 3
- `NetworkFactorySimulation.FixedUpdate()` calls `_simulation.Tick()` and syncs state
- `BeltItemVisualizer` already wired in `CmdPlaceBelt` to render synced items
- `BeltPort` MonoBehaviour on prefabs with `Direction` (Input/Output), `SlotIndex`, proximity colliders
- `RegisterBeltOnNearbyPorts` in `GridManager` already finds nearby ports via physics overlap

## Key file paths

| File | Purpose |
|------|---------|
| `Assets/_Slopworks/Scripts/Automation/BeltNetwork.cs` | Connection manager to generalize |
| `Assets/_Slopworks/Scripts/Network/GridManager.cs` | Belt placement, wiring connections |
| `Assets/_Slopworks/Scripts/Network/NetworkFactorySimulation.cs` | Server tick orchestrator |
| `Assets/_Slopworks/Scripts/Network/NetworkMachine.cs` | `Machine` accessor |
| `Assets/_Slopworks/Scripts/Network/NetworkStorage.cs` | `StorageContainer` accessor |
| `Assets/_Slopworks/Scripts/Network/NetworkBeltSegment.cs` | `BeltSegment` accessor |
| `Assets/_Slopworks/Scripts/Automation/BeltPort.cs` | Port direction/slot info |
| `Assets/_Slopworks/Tests/Editor/EditMode/BeltNetworkTests.cs` | Existing tests to update |

---

### Task 1: Write failing tests for generalized BeltNetwork

**Files:**
- Modify: `Assets/_Slopworks/Tests/Editor/EditMode/BeltNetworkTests.cs`

**Step 1: Add tests for IItemSource/IItemDestination connections**

Add these tests at the end of `BeltNetworkTests.cs`:

```csharp
// -- Generalized connections (IItemSource/IItemDestination) --

[Test]
public void Connect_SourceAndDestination_IsConnectedReturnsTrue()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var machine = new Machine(CreateMachineDefinition());

    var source = new BeltOutputAdapter(belt);
    var dest = new MachineInputAdapter(machine, 0);

    network.Connect(source, dest);

    Assert.AreEqual(1, network.ConnectionCount);
}

[Test]
public void Tick_BeltToMachine_TransfersItem()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var machine = new Machine(CreateMachineDefinition());

    PlaceItemAtEnd(belt, IronOre);

    var source = new BeltOutputAdapter(belt);
    var dest = new MachineInputAdapter(machine, 0);
    network.Connect(source, dest);
    network.Tick();

    Assert.IsTrue(belt.IsEmpty, "Item should be extracted from belt");
    Assert.IsFalse(machine.GetInput(0).IsEmpty, "Item should be in machine input");
    Assert.AreEqual(IronOre, machine.GetInput(0).item.definitionId);
}

[Test]
public void Tick_MachineToBelt_TransfersItem()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var def = CreateMachineDefinition();
    var machine = new Machine(def);

    // Manually place item in machine output buffer
    machine.ForceOutput(0, ItemInstance.Create(IronOre), 1);

    var source = new MachineOutputAdapter(machine, 0);
    var dest = new BeltInputAdapter(belt);
    network.Connect(source, dest);
    network.Tick();

    Assert.IsTrue(machine.GetOutput(0).IsEmpty, "Item should be extracted from machine");
    Assert.AreEqual(1, belt.ItemCount, "Item should be on belt");
}

[Test]
public void Tick_BeltToStorage_TransfersItem()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var storage = new StorageContainer(12, 64);

    PlaceItemAtEnd(belt, IronOre);

    var source = new BeltOutputAdapter(belt);
    network.Connect(source, storage);
    network.Tick();

    Assert.IsTrue(belt.IsEmpty, "Item should be extracted from belt");
    Assert.IsFalse(storage.IsEmpty, "Item should be in storage");
}

[Test]
public void Tick_StorageToBelt_TransfersItem()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var storage = new StorageContainer(12, 64);
    storage.TryInsert(IronOre);

    var dest = new BeltInputAdapter(belt);
    network.Connect(storage, dest);
    network.Tick();

    Assert.IsTrue(storage.IsEmpty, "Item should be extracted from storage");
    Assert.AreEqual(1, belt.ItemCount, "Item should be on belt");
}

[Test]
public void Tick_GeneralizedConnection_HoldsItemOnReject()
{
    var network = new BeltNetwork();
    var belt = CreateBelt();
    var storage = new StorageContainer(1, 1); // 1 slot, stack of 1

    // Fill storage so it rejects
    storage.TryInsert(CopperOre);

    PlaceItemAtEnd(belt, IronOre);

    var source = new BeltOutputAdapter(belt);
    network.Connect(source, storage);
    network.Tick();

    Assert.IsTrue(belt.IsEmpty, "Item extracted from belt");
    // Storage still has only copper, iron is held in transit
    Assert.AreEqual(CopperOre, storage.PeekItemId());
}

private MachineDefinitionSO CreateMachineDefinition()
{
    var def = UnityEngine.ScriptableObject.CreateInstance<MachineDefinitionSO>();
    def.machineId = "test_smelter";
    def.machineType = "smelter";
    def.size = UnityEngine.Vector2Int.one;
    def.inputBufferSize = 1;
    def.outputBufferSize = 1;
    def.processingSpeed = 1f;
    return def;
}
```

**Step 2: Run tests to verify they fail**

Tell user to run tests in Unity Test Runner. Expected: 6 new tests fail because `BeltNetwork.Connect(IItemSource, IItemDestination)` overload doesn't exist yet.

**Step 3: Commit**

```
git add Assets/_Slopworks/Tests/Editor/EditMode/BeltNetworkTests.cs
git commit -m "Add failing tests for generalized BeltNetwork connections"
```

---

### Task 2: Generalize BeltNetwork to accept IItemSource/IItemDestination

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Automation/BeltNetwork.cs`

**Step 1: Replace BeltConnection struct**

Replace the existing `BeltConnection` struct (lines 15-20) with:

```csharp
private struct BeltConnection
{
    public IItemSource Source;
    public IItemDestination Destination;
    public string HeldItemId;
}
```

**Step 2: Add the generalized Connect method**

Add after the existing `Connect(BeltSegment, BeltSegment)` method (after line 44):

```csharp
/// <summary>
/// Register a connection from any item source to any item destination.
/// Used for belt-to-machine, belt-to-storage, and similar connections.
/// </summary>
public void Connect(IItemSource source, IItemDestination destination)
{
    if (source == null || destination == null)
        return;

    _connections.Add(new BeltConnection
    {
        Source = source,
        Destination = destination,
        HeldItemId = null
    });
}
```

**Step 3: Update the existing belt-to-belt Connect to use adapters**

Replace the existing `Connect(BeltSegment, BeltSegment)` method body (lines 30-44):

```csharp
public void Connect(BeltSegment from, BeltSegment to)
{
    if (from == null || to == null)
        return;

    if (IsConnected(from, to))
        return;

    Connect(new BeltOutputAdapter(from), new BeltInputAdapter(to));
}
```

**Step 4: Update IsConnected to work with adapters**

The existing `IsConnected` checks `From == from && To == to` which won't match adapters wrapping the same belt. Replace (lines 64-72):

```csharp
public bool IsConnected(BeltSegment from, BeltSegment to)
{
    for (int i = 0; i < _connections.Count; i++)
    {
        if (_connections[i].Source is BeltOutputAdapter srcAdapter
            && _connections[i].Destination is BeltInputAdapter dstAdapter
            && srcAdapter.Belt == from
            && dstAdapter.Belt == to)
            return true;
    }
    return false;
}
```

This requires exposing the wrapped belt on the adapters. Add a public property to each:

In `BeltOutputAdapter.cs` (after line 10):
```csharp
public BeltSegment Belt => _belt;
```

In `BeltInputAdapter.cs` (after line 10):
```csharp
public BeltSegment Belt => _belt;
```

**Step 5: Update Disconnect to work with adapters**

Replace the `Disconnect` method (lines 49-59):

```csharp
public void Disconnect(BeltSegment from, BeltSegment to)
{
    for (int i = _connections.Count - 1; i >= 0; i--)
    {
        if (_connections[i].Source is BeltOutputAdapter srcAdapter
            && _connections[i].Destination is BeltInputAdapter dstAdapter
            && srcAdapter.Belt == from
            && dstAdapter.Belt == to)
        {
            _connections.RemoveAt(i);
            return;
        }
    }
}
```

**Step 6: Update Tick to use Source/Destination**

Replace the `Tick()` method body (lines 81-113):

```csharp
public void Tick()
{
    for (int i = 0; i < _connections.Count; i++)
    {
        var conn = _connections[i];

        if (conn.HeldItemId != null)
        {
            if (conn.Destination.TryInsert(conn.HeldItemId))
            {
                conn.HeldItemId = null;
                _connections[i] = conn;
            }
            continue;
        }

        if (!conn.Source.HasItemAvailable)
            continue;

        if (!conn.Source.TryExtract(out string itemId))
            continue;

        if (!conn.Destination.TryInsert(itemId))
        {
            conn.HeldItemId = itemId;
        }

        _connections[i] = conn;
    }
}
```

**Step 7: Run tests**

Tell user to run all BeltNetwork tests. Expected: all existing tests pass (belt-to-belt still works via adapter wrapper), all new tests pass.

**Step 8: Commit**

```
git add Assets/_Slopworks/Scripts/Automation/BeltNetwork.cs Assets/_Slopworks/Scripts/Automation/BeltOutputAdapter.cs Assets/_Slopworks/Scripts/Automation/BeltInputAdapter.cs
git commit -m "Generalize BeltNetwork to IItemSource/IItemDestination connections

BeltConnection now stores IItemSource/IItemDestination instead of
BeltSegment. Belt-to-belt Connect wraps segments in adapters.
Tick uses interface methods for all connection types uniformly.
Enables belt-to-machine and belt-to-storage transfers."
```

---

### Task 3: Check if Machine.ForceOutput exists, add if needed

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Automation/Machine.cs` (if `ForceOutput` doesn't exist)

The `Tick_MachineToBelt_TransfersItem` test calls `machine.ForceOutput(0, ItemInstance.Create(IronOre), 1)` to place an item in the output buffer for testing. Check if `Machine` has this method. If not, add it:

**Step 1: Check for ForceOutput**

Search `Machine.cs` for `ForceOutput`.

**Step 2: Add if missing**

Add a test-only method to `Machine.cs`:

```csharp
/// <summary>
/// Directly place an item in the output buffer. For testing only.
/// </summary>
public void ForceOutput(int slotIndex, ItemInstance item, int count)
{
    _outputBuffer[slotIndex] = new ItemSlot { item = item, count = count };
}
```

**Step 3: Commit if changed**

```
git add Assets/_Slopworks/Scripts/Automation/Machine.cs
git commit -m "Add Machine.ForceOutput for test support"
```

---

### Task 4: Wire simulation connections in GridManager.CmdPlaceBelt

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Network/GridManager.cs:452-456` (after RegisterBeltOnNearbyPorts)

**Step 1: Add WireBeltSimulationConnection method**

Add after `RegisterBeltOnNearbyPorts` (after line 504):

```csharp
/// <summary>
/// Create a simulation connection between a belt endpoint and a nearby port.
/// The belt's port direction at this endpoint determines flow: belt Output connects
/// to nearby Input, belt Input connects to nearby Output.
/// </summary>
private void WireBeltSimulationConnection(
    NetworkBeltSegment netBelt, Vector3 endpointPos, BeltPortDirection beltPortDir, float radius)
{
    if (_factorySimulation?.Simulation?.BeltNetwork == null) return;
    if (netBelt?.Segment == null) return;

    var colliders = Physics.OverlapSphere(endpointPos, radius, 1 << PhysicsLayers.BeltPorts);
    foreach (var col in colliders)
    {
        var port = col.GetComponentInParent<BeltPort>();
        if (port == null) continue;
        if (port.transform.IsChildOf(netBelt.transform)) continue;

        // Belt Output connects to nearby Input, belt Input connects to nearby Output
        if (beltPortDir == BeltPortDirection.Output && port.Direction != BeltPortDirection.Input)
            continue;
        if (beltPortDir == BeltPortDirection.Input && port.Direction != BeltPortDirection.Output)
            continue;

        // Find the simulation object on the port's owner
        IItemSource source = null;
        IItemDestination destination = null;

        var netMachine = port.GetComponentInParent<NetworkMachine>();
        var netStorage = port.GetComponentInParent<NetworkStorage>();
        var otherBelt = port.GetComponentInParent<NetworkBeltSegment>();

        if (beltPortDir == BeltPortDirection.Output)
        {
            // Belt output -> nearby input: belt is source, neighbor is destination
            source = new BeltOutputAdapter(netBelt.Segment);

            if (netMachine?.Machine != null)
                destination = new MachineInputAdapter(netMachine.Machine, port.SlotIndex);
            else if (netStorage?.Container != null)
                destination = netStorage.Container;
            else if (otherBelt?.Segment != null)
                destination = new BeltInputAdapter(otherBelt.Segment);
        }
        else
        {
            // Belt input -> nearby output: neighbor is source, belt is destination
            destination = new BeltInputAdapter(netBelt.Segment);

            if (netMachine?.Machine != null)
                source = new MachineOutputAdapter(netMachine.Machine, port.SlotIndex);
            else if (netStorage?.Container != null)
                source = netStorage.Container;
            else if (otherBelt?.Segment != null)
                source = new BeltOutputAdapter(otherBelt.Segment);
        }

        if (source == null || destination == null)
        {
            Debug.Log($"belt: port at {endpointPos} has no simulation owner, skipping connection");
            continue;
        }

        _factorySimulation.Simulation.BeltNetwork.Connect(source, destination);
        Debug.Log($"belt: wired {beltPortDir} at {endpointPos} to {port.Direction} on {port.transform.parent?.name}");
        return; // one connection per endpoint
    }
}
```

**Step 2: Call WireBeltSimulationConnection from CmdPlaceBelt**

In `CmdPlaceBelt`, after line 456 (`if (endFromPort) RegisterBeltOnNearbyPorts(...)`), add:

```csharp
// Wire simulation connections for item flow
if (startFromPort)
    WireBeltSimulationConnection(netBelt, beltStartPos, startPortDir, 0.6f);
if (endFromPort)
    WireBeltSimulationConnection(netBelt, beltEndPos, endPortDir, 0.6f);
```

Note: `startPortDir` and `endPortDir` are already computed on lines 447-448 from `flipBeltPorts`.

**Step 3: Commit**

```
git add Assets/_Slopworks/Scripts/Network/GridManager.cs
git commit -m "Wire belt simulation connections at placement time

When a belt is placed connecting to a machine, storage, or another
belt port, create the appropriate IItemSource/IItemDestination
adapters and register with BeltNetwork for item transfer."
```

---

### Task 5: Verify end-to-end in multiplayer

This is a manual playtest, not automated tests. Tell the user to:

1. Open HomeBase scene, hit Play, click Host
2. Place a storage container and a machine (constructor/smelter)
3. Use `CmdInsertItem` or a test spawner to put `iron_scrap` items into the storage
4. Set the machine recipe to `smelt_iron`
5. Place a belt from the storage Output port to the machine Input port
6. Watch the console for `belt: wired` log messages confirming the connection
7. Observe: items should flow from storage onto the belt and into the machine
8. Place a second belt from the machine Output port to another storage
9. Observe: machine crafts `iron_ingot`, outputs onto belt, flows into second storage

If items don't flow, check:
- Console for `belt: wired` logs (confirms connections created)
- Console for `factory: X test recipes created` (confirms simulation started)
- That the belt endpoints physically overlap with the machine/storage ports (0.6m radius)

**Step 1: Commit any fixes from playtesting**

---

### Task 6: Commit and push

```
git push origin kevin/belt-simulation-tick
```

---

## What does NOT need to change

- **FactorySimulation.Tick()** -- already calls `_beltNetwork.Tick()` at step 3. The generalized connections tick automatically.
- **NetworkFactorySimulation.FixedUpdate()** -- already calls `_simulation.Tick()` and syncs state to clients.
- **BeltItemVisualizer** -- already wired, reads `_syncItems` from `NetworkBeltSegment`.
- **BeltPort MonoBehaviour** -- unchanged, still tracks visual connections separately.
- **Adapters** -- BeltOutputAdapter, BeltInputAdapter, MachineInputAdapter, MachineOutputAdapter all stay as-is (except adding `Belt` property to the belt adapters).
- **Inserter, ConnectionResolver, PortNodeRegistry, PortNode** -- untouched, dead code in multiplayer. Cleanup is a separate task.
