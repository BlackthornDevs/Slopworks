# Joe's session handoff

Updated by Kevin's Claude on 2026-03-13.

---

## Last updated: 2026-03-13

### What changed since your last session

**Your PR #65 (visor HUD) has been merged to master.** Along with it, a new input system migration (PR #66) landed. Here's what's new on master:

1. **Action maps renamed**: `Exploration` -> `Combat`, `Factory` -> `Command`. If you have any code referencing `.Exploration.` or `.Factory.`, change to `.Combat.` / `.Command.`.

2. **New Build action map** added to `SlopworksInput.inputactions` with 20 actions (tool select, rotate, delete, zoop, place, remove, snap filter, nudge, debug dump, etc.). `SlopworksInput.cs` regenerated.

3. **UI contract created** -- two new files in `Scripts/Core/`:
   - `BuildStateSnapshot.cs` -- plain data struct Kevin's `NetworkBuildController` pushes on every state change
   - `IBuildStateReceiver.cs` -- 3-method interface (`OnBuildStateChanged`, `OnBuildModeEntered`, `OnBuildModeExited`)

4. **`hotkeys.md`** added to `docs/reference/` -- human-readable keybinding reference mirroring the `.inputactions` asset. Update this when you add/change bindings.

5. **`input-system.md`** updated to reflect the three action maps.

6. **Consumer files updated** for the rename: NetworkPlayerController, PlayerController, InventoryUI, InteractionController, WeaponBehaviour, CameraModeController all now use `.Combat.` / `.Command.` instead of `.Exploration.` / `.Factory.`.

### New tasks assigned to you

**Check `docs/coordination/tasks-joe.md`** -- two new tasks added:

- **J-030 (High)**: Create `VisorBuildAdapter.cs` -- implements `IBuildStateReceiver`, translates `BuildStateSnapshot` into calls to your ReticleController, BuildTooltipUI, and VisorHUD components. This is the bridge between Kevin's build controller and your visor UI. Attach as child of player prefab.

- **J-031 (Medium)**: Update `VisorAutoBootstrap` to skip auto-spawn when `NetworkBuildController` exists in the scene. One-line guard.

### How it all fits together

```
NetworkBuildController (Kevin)
    |
    | calls PushState() at 9 transition points
    v
IBuildStateReceiver (shared interface)
    |
    | GetComponentInChildren<IBuildStateReceiver>()
    v
VisorBuildAdapter (Joe) -- YOUR new file
    |
    | translates snapshot fields to UI calls
    v
ReticleController / BuildTooltipUI / VisorHUD (Joe)
```

Kevin's controller already calls `_buildUI?.OnBuildStateChanged(snapshot)` etc. If no adapter is attached, all calls are no-ops. Once you create VisorBuildAdapter and attach it to the player hierarchy, it starts receiving state.

### ToolName values Kevin sends

`"Foundation"`, `"Wall"`, `"Ramp"`, `"Belt"`, `"Machine"`, `"Storage"`, `"Delete"`

### Other snapshot fields

- `BeltRoutingMode`: `"Default"`, `"Straight"`, `"Curved"`
- `SnapFilter`: `"CENTER"`, `"EDGE"`, `"FOUNDATION"`, `"MACHINE/STORAGE"`
- `ValidationError`: null when valid, error string when placement blocked
- `KeycapLabels`: string array from Input Action display names (supports rebinding)
- `RotationDegrees`: current rotation offset (0, 90, 180, 270)
- `ZoopMode`, `ZoopStartSet`, `DeleteMode`: booleans

### What to do first

1. `git fetch origin master && git merge origin/master` -- pick up all the new stuff
2. Read `docs/coordination/tasks-joe.md` for J-030 and J-031 details
3. Start J-030 (VisorBuildAdapter)

### Blockers

None. J-030 and J-031 are fully unblocked -- all dependencies are on master.

### Key context

- **Branch**: merge master into `joe/main` before starting
- **Kevin's parallel work**: `kevin/input-system-migration` has the NetworkBuildController rewrite (hardcoded keys -> input actions + IBuildStateReceiver calls). Not on master yet but the contract it calls against (BuildStateSnapshot, IBuildStateReceiver) IS on master.
- **Your existing test harness still works**: ReticleTestSetup keys (F/T/Z/C/V) are independent of the Input Action system. Once VisorBuildAdapter is wired, the real build controller will drive the same UI components.
