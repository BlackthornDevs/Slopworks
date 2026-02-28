# Kevin's Claude -- Session Handoff

Last updated: 2026-02-28 (session end)
Branch: kevin/main
Last commit: f2e4aef Wire all subsystems into FactorySimulation tick loop, add playtest

## What was completed this session

### Automation connectivity systems (796377b)
- `Scripts/Automation/IItemSource.cs` / `IItemDestination.cs` -- universal item transfer interfaces
- `Scripts/Automation/BeltOutputAdapter.cs`, `BeltInputAdapter.cs` -- belt endpoint adapters
- `Scripts/Automation/MachineOutputAdapter.cs`, `MachineInputAdapter.cs` -- machine slot adapters
- `Scripts/Automation/Inserter.cs` + `InserterBehaviour.cs` -- grab/swing/deposit transfer arm
- `Scripts/Automation/StorageContainer.cs` + `StorageDefinitionSO.cs` + `StorageBehaviour.cs` -- slot-based stacking storage
- `Scripts/Automation/BeltNetwork.cs` + `BeltNetworkBehaviour.cs` -- belt-to-belt connections with held-item-in-transit
- `Scripts/Automation/IPowerNode.cs`, `PowerNetwork.cs`, `PowerNetworkManager.cs`, `SimplePowerNode.cs`, `PowerNetworkBehaviour.cs` -- BFS flood-fill power grid
- Tests: InserterTests (33), StorageContainerTests (26), BeltNetworkTests (15), PowerNetworkTests (30)

### FactorySimulation orchestration (f2e4aef)
- `Scripts/Automation/FactorySimulation.cs` -- now orchestrates ALL subsystems in tick order:
  1. Power network rebuild (if dirty)
  2. Belt segments tick
  3. Belt network transfers
  4. Inserters tick
  5. Machines tick
- `Scripts/Automation/FactorySimulationBehaviour.cs` -- exposes belt speed config
- `Scripts/Automation/FactoryPlaytestSetup.cs` -- self-contained playtest MonoBehaviour
- `Scenes/Playtest.unity` -- has FactoryPlaytestSetup component, camera positioned
- 14 new integration tests including full end-to-end pipeline (storage -> belt -> machine -> belt -> storage)

### Infrastructure fixes (9580bc9)
- Moved `Assets/_Slopworks/Input/` to `Assets/_Slopworks/Scripts/Input/` (inside asmdef scope)
- Added TMPro GUID to `Slopworks.Runtime.asmdef` references
- Fixed jawn's player script compilation errors

### Jawn task assignment (466b16c, 03d986e)
- Wrote J-003 through J-006 in `docs/coordination/tasks-joe.md` (Phase 3 combat systems)
- Resolved merge conflict with jawn's J-002 completion notes

## What's in progress (not yet committed)
- Deleted TMP font material .meta files (jawn's TMP cleanup remnants, harmless)
- New `.claude/skills/slopworks-handoff/` skill (will commit with this handoff)

## Next task to pick up

**Build the PortNode / spatial connection system** -- this is the bridge between the simulation layer (which is complete and tested) and actual gameplay.

Specifically (Phase 1, Task 1.6 completion):
1. **PortNode system** -- A component or plain C# class representing a spatial connection point. Every machine, storage, and belt endpoint exposes typed nodes (input/output) at world positions derived from MachinePort.localOffset + building rotation.
2. **Connection resolver** -- When a belt endpoint is placed adjacent to a port node, snap to it and auto-create the inserter or belt network link.
3. **Belt placement on grid** -- Click-drag belt segments that align to the grid and snap to port nodes.
4. **Machine/storage placement** -- Place buildings via BuildModeController, spawning port nodes.
5. **Visual belt items** -- Simple GameObjects moving along belts (GPU instancing deferred).

The port node system is the linchpin -- build it first.

Reference: `docs/plans/2026-02-27-vertical-slice-plan.md` Phase 1 Tasks 1.2, 1.3, 1.6.

## Blockers or decisions needed
- None. The user confirmed the approach (port nodes with snap connections).

## Test status
- 276/276 passing, 0 failures, 0 skipped
- 0 compilation errors, 3 warnings (all pre-existing: FishNet deprecation, jawn's unused field)

## Key context the next session needs
- `MachinePort` struct already exists with `localOffset`, `direction`, and `PortType` (Input/Output) -- use this as the data source for spatial port nodes
- `MachineDefinitionSO.ports` is the array of port definitions per machine type
- The adapters (BeltInputAdapter, MachineInputAdapter, etc.) already exist -- the connection resolver just needs to create the right adapter + Inserter and register it with FactorySimulation
- StorageContainer implements both IItemSource and IItemDestination directly -- no adapter needed
- Belt speed at 50Hz: speed 2 = 1 tile/sec, speed 4 = 2 tiles/sec
- Inserter swing duration is in seconds, belt tick speed is in subdivisions per tick -- they're on different time scales by design
- The playtest scene creates everything in code -- the real build system will use the grid + port nodes instead
