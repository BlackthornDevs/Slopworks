# Kevin's Claude -- Session Handoff

Last updated: 2026-03-08 23:00
Branch: kevin/multiplayer-step1
Last commit: b8a4921 Snap point sphere colliders and data-driven placement

## What was completed this session

### Snap point sphere collider system
- Each `BuildingSnapPoint` child now gets a `SphereCollider` (radius 0.5, isTrigger=true) on layer 20 (SnapPoints)
- Build raycasts hit snap colliders directly instead of relying on distance-based FindNearest
- Added SnapPoints layer 20 to `PhysicsLayers.cs` and `TagManager.asset`
- Configured physics collision matrix: SnapPoints collides with nothing (triggers only)
- `NetworkBuildController.RaycastPlacement()` checks for direct snap point hit first, falls back to FindNearest with hitNormal

### Data-driven snap placement (removed hardcoded category logic)
- `ComputeSnapSurfaceY()` simplified to just `return snap.transform.position.y` -- no category-based sitsOnTop logic
- `GridManager.GetSnapPlacementPosition()` horizontal Y uses `snapPos.y + baseOffset` instead of `surfaceY + baseOffset`
- Rotation always comes from caller (`rotationDeg` param), no autoYaw fallback
- Wall-to-wall and ramp-to-ramp inherit existing building's yaw as base, R-key adds offset

### Ramp snap point reduction
- Ramps get 7 snap points (4 cardinal mid + Top_Center + HighEdge + LowEdge) instead of 14+2
- Both `BuildingSnapPoint.GenerateFromBounds()` and `SnapPointPrefabSetup` support `isRamp` parameter

### Editor tool: SnapPointPrefabSetup
- New editor tool at `Tools > Slopworks > Add Snap Points to Prefabs`
- Preserves existing snap points (skips prefabs that already have them) so manual edits survive
- Generates sphere colliders on each snap point child

### New building prefabs
- `SLAB_1m/2m/4m` (foundations), `WALL_0.5m`, `RAMP 4x1/4x2`
- All have snap point children with sphere collider triggers

### Test updates
- `SnapPlacementTests.cs`: explicit autoYaw for east snap rotation
- `UnifiedPlacementTests.cs`: new test file covering east/south/corner snaps with explicit rotation
- `GridPlacementTests.cs`: updated for new API

## What's in progress (not yet committed)
- Unstaged terrain data, metal materials, old prefab deletions, "test 1" prefabs, FBX Raw assets, Recovery assets -- NOT part of snap point work

## Next task to pick up

### Known bugs (from playtest)
1. **Zoop ghost shifts to different grid after first click** -- ghost jumps to wrong cell when zoop starts
2. **Ramp zoop should change elevation** -- currently places flat copies. Should increment elevation (high-to-low or low-to-high) so zooped ramps build upward
3. **Ramp HighEdge/LowEdge snap points need manual positioning** -- auto-detection from mesh bounds doesn't match actual slope. Editor tool preserves manual edits.
4. **Wall-to-floor snap positioning** -- foundation top flush with wall top requires correct snap point positioning on wall prefabs (tuning, not code)

### After bugs
- Continue Step 4: Machines + Belts + Simulation network wrappers
- Steps 5-7: Combat, Tower+Buildings, Supabase

## Blockers or decisions needed
- None

## Test status
- Tests not run via MCP this session (MCP run_tests corrupts FishNet DefaultPrefabObjects)
- Run manually: Window > General > Test Runner > EditMode > Run All

## Key context the next session needs
- **Branch:** `kevin/multiplayer-step1`, NOT `kevin/main`
- **NEVER use MCP run_tests** -- triggers recompilation that corrupts FishNet DefaultPrefabObjects.asset
- **SnapPoints layer 20**: trigger colliders only, no physics collisions. Raycasts hit them via Physics.queriesHitTriggers
- **Snap point Y drives placement**: no category-based surfaceY logic. Snap point position encodes the geometry.
- **Editor tool preserves manual edits**: SnapPointPrefabSetup skips prefabs that already have snap points
- **placeSurfaceY includes nudge**: `_surfaceY + _nudgeOffset` used for all placement commands
- **Zoop skipped during snap detection**: `if (!_zoopMode)` guards snap point checking
- **Ghost system**: Prefab-based ghosts via `CreateGhostFromPrefab()` / `EnsurePrefabGhost()`
- **Camera-facing edge**: `GetFacingEdgeDirection()` determines wall/ramp edge based on camera direction
