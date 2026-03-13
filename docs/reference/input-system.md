# Input system reference

New Input System (com.unity.inputsystem) for all player input in Slopworks. This covers package setup, the three Action Maps, C# class generation, and network integration patterns.

---

## Package choice: New Input System, not legacy

Unity's legacy `Input` class (`Input.GetKey`, `Input.GetAxis`) is deprecated and will be removed. The New Input System provides:
- Action-based input with player rebinding support
- Multiple device abstraction (keyboard/mouse, gamepad)
- Generated C# classes for type-safe access
- Clean separation between input reading and game logic

**Package:** `com.unity.inputsystem 1.7+`

Enable in Project Settings > Player > Active Input Handling: `Input System Package (New)`.

---

## Action Maps

Three Action Maps cover all gameplay contexts. Only one map active at a time -- they share bindings (WASD means different things in each).

### Combat (first-person -- was "Exploration")

Active during FPS movement, combat, and exploration.

```
Move              -> WASD, left stick
Look              -> Mouse, right stick
Jump              -> Space, A button
Fire              -> Left click, right trigger
Aim               -> Right click, left trigger
Interact          -> E key, X button
Sprint            -> Shift, left stick click
Reload            -> R key, Y button
Inventory/Open    -> Tab, Start button
Switch/Isometric  -> V key, Select button
ToggleBuildMode   -> B key (enters Build map)
```

### Build (building and factory placement -- NEW)

Active when build mode is on. Combat map disabled.

```
ToggleBuildMode   -> B key (exits back to Combat map)
SelectTool1-6     -> 1-6 keys
Rotate            -> R key, Y button
DeleteMode        -> X key
ZoopMode          -> Z key
GridOverlay       -> G key
CycleVariant      -> Tab key
NudgeUp           -> PageUp
NudgeDown         -> PageDown
Place             -> Left click, A button
Remove            -> Right click
Cancel            -> Escape, B button (gamepad)
MachineInteract   -> F key, X button (gamepad)
SnapFilterToggle  -> Scroll wheel (Value/Axis)
DebugDump         -> 0 key
```

### Command (isometric view -- was "Factory")

Reserved for top-down factory view. Not currently active in multiplayer.

```
Camera/Pan        -> Mouse drag, WASD
Camera/Zoom       -> Mouse wheel, shoulder buttons
Camera/Rotate     -> Middle mouse drag, right stick
Place/Select      -> Mouse click, A button
Place/Rotate      -> R key, Y button
Place/Cancel      -> Escape, B button
Inventory/Open    -> Tab, Start button
UI/Navigate       -> Arrow keys, d-pad
Switch/FPS        -> V key, Select button
```

---

## Setup

### 1. Create the InputActionAsset

1. Right-click in Project window: Create > Input Actions
2. Name it `SlopworksInput`
3. Save to `Assets/_Slopworks/Input/SlopworksInput.inputactions`
4. Add `Combat`, `Build`, and `Command` Action Maps
5. Add all actions above with bindings

### 2. Generate the C# class

In the InputActionAsset inspector:
- Check **Generate C# Class**
- Class Name: `SlopworksControls`
- Path: `Assets/_Slopworks/Scripts/Input/SlopworksInput.cs`
- Click Apply

This generates a type-safe wrapper. **Never edit the generated file.** Regenerate when the asset changes.

### 3. Use the generated class

```csharp
public class NetworkBuildController : NetworkBehaviour
{
    private SlopworksControls _controls;

    public override void OnStartClient()
    {
        _controls = new SlopworksControls();
        _controls.Combat.Enable();
    }

    private void Update()
    {
        // Toggle build mode from Combat map
        if (_controls.Combat.ToggleBuildMode.WasPressedThisFrame())
        {
            _controls.Combat.Disable();
            _controls.Build.Enable();
        }

        // Build actions from Build map
        if (_controls.Build.Rotate.WasPressedThisFrame()) { ... }
        if (_controls.Build.Place.WasPressedThisFrame()) { ... }

        // Value actions
        float scroll = _controls.Build.SnapFilterToggle.ReadValue<float>();
    }
}
```

### 4. Map switching pattern

```csharp
// Entering build mode:
_controls.Combat.Disable();
_controls.Build.Enable();

// Exiting build mode:
_controls.Build.Disable();
_controls.Combat.Enable();

// Switching to isometric (future):
_controls.Combat.Disable();
_controls.Command.Enable();
```

---

## Multiplayer: input is always local

Input reads happen only on the owning client. The server never reads `SlopworksControls`.

```csharp
// Always guard input handling with IsOwner
public override void OnStartClient()
{
    if (!IsOwner) return;
    _controls = new SlopworksControls();
    _controls.Combat.Enable();
}
```

Never send raw input state over the network. Send **intent** (fire, move to position, interact with machine) via `[ServerRpc]`.

---

## Hotkey reference

See `docs/reference/hotkeys.md` for the full keybinding table.

---

## Pitfall quick reference

| Pitfall | Fix |
|---------|-----|
| Legacy `Input.GetKey` calls | Replace with action callbacks or `WasPressedThisFrame()` |
| Multiple Action Maps enabled at once | Disable one before enabling another |
| Forgetting `-=` on `OnDisable` | Always unsubscribe in `OnDisable` |
| Reading input on non-owner clients | Guard with `if (!IsOwner) return;` |
| Sending raw input over network | Send intent ServerRpc instead |
| Hardcoded `Keyboard.current.*` | Use `_controls.Build.ActionName` instead |
| Shift modifier for nudge | OK to read `Keyboard.current.leftShiftKey.isPressed` directly for modifiers |
