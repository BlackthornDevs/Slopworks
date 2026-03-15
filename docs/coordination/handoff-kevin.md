# Kevin's Claude -- Session Handoff

Last updated: 2026-03-14
Branch: kevin/belt-simulation-tick
Last commit: a1561a8 Update prefabs, scenes, and belt docs from prior work

## What was completed this session

### Belt port flow inheritance (kevin/belts, merged to master via PR #72)
- `CmdPlaceBelt` accepts `flipBeltPorts` parameter (`GridManager.cs`)
- When starting from an Input port, belt start becomes Output and end becomes Input
- Per-endpoint port direction validation: each belt end must be opposite to the port it connects to (`NetworkBuildController.cs`)

### Belt simulation tick (kevin/belt-simulation-tick)
- **Generalized BeltNetwork** (`BeltNetwork.cs`): `BeltConnection` now stores `IItemSource`/`IItemDestination` instead of `BeltSegment`. All connection types (belt-to-machine, belt-to-storage, belt-to-belt) tick uniformly.
- **Belt property on adapters** (`BeltOutputAdapter.cs`, `BeltInputAdapter.cs`): exposed for disconnect/identity matching
- **Machine.ForceOutput** (`Machine.cs`): test-only method for placing items in output buffer
- **6 new EditMode tests** (`BeltNetworkTests.cs`): belt-to-machine, machine-to-belt, belt-to-storage, storage-to-belt, hold-on-reject
- **WireBeltSimulationConnection** (`GridManager.cs`): creates simulation connections at belt placement time via proximity lookup on BeltPort components
- **TestItemSpawner** (`TestItemSpawner.cs`): T key fills all storage with iron_scrap, sets machines to smelt_iron
- **Prefabs updated**: CONSTRUCTOR, STORAGE CONTAINER, BELT SUPPORT prefabs committed with scene changes
- **User verified**: items flow from storage through belt into machine in multiplayer host mode

## What's in progress (not yet committed)
- Terrain data and water material changes are unstaged (user said don't commit)

## Next task to pick up
1. **PR for kevin/belt-simulation-tick** -- create PR to master, wait for checks, merge
2. **Add NetworkMachine and NetworkStorage to prefabs** -- user added them manually this session but the prefabs need to be committed with those components permanently. Verify they survived the session.
3. **Ghost preview mesh** -- replace line renderer with actual belt mesh during placement
4. **Belt item visuals** -- items render as orange cubes, should eventually use item-specific meshes/colors
5. **Machine UI** -- recipe selection and status display for multiplayer (currently F cycles recipes with no visual feedback)
6. **Storage UI** -- inventory view for storage containers in multiplayer

## Blockers or decisions needed
- None

## Test status
- Tests not run via MCP (corrupts FishNet DefaultPrefabObjects)
- 6 new tests added for generalized BeltNetwork
- Run manually: Window > General > Test Runner > EditMode > Run All

## Key context the next session needs
- **Branch:** `kevin/belt-simulation-tick` (not yet merged to master)
- **Belt simulation is verified working** -- user confirmed items flow in multiplayer host mode
- **Inserter class is dead code in multiplayer** -- BeltNetwork handles all transfers directly (Satisfactory-style, no swing timer). Don't use Inserter, ConnectionResolver, PortNodeRegistry, or PortNode in multiplayer code.
- **TestItemSpawner T key** is a debug tool -- press T to fill all storage and set recipes. Delete the script when real UI exists.
- **NetworkMachine/NetworkStorage must be on prefabs** -- without them, simulation objects don't exist and belt connections have nothing to wire to. User added them manually this session.
- **Per-endpoint port validation** -- each belt end checks its direction is opposite to the port it connects to. Validated at belt-to-machine, belt-to-belt, belt-to-storage level.
