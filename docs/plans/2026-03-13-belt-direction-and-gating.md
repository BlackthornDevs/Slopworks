# Belt Direction Fix and Connection Gating Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix belt routing direction at machine/storage ports so belts always route outward, and add connection gating so only compatible port pairs can connect.

**Architecture:** Two changes to the same resolution pipeline. First, `GetPortFlowDirection` stops guessing direction from `isStart` and instead uses the port's actual data (transform.forward for machines, port.Direction for belt ports). Second, `TryResolveBeltEndpoint` gains a compatibility check that rejects connections between incompatible port types (e.g. Input-to-Input). Both changes are in `NetworkBuildController.cs` with tests in `BeltPortTests.cs`.

**Tech Stack:** Unity C#, NUnit EditMode tests, FishNet (not directly involved -- these are client-side resolution changes)

**Reference doc:** `docs/reference/belt-interactions.md` -- user-annotated expected behavior for all 12 interaction types

---

## Background: Direction Convention

The route builder (`BeltRouteBuilder.Build`) expects `startDir` and `endDir` to both point in the **direction of item travel** at that endpoint. Items flow from start to end. Both vectors point the same way along the belt.

Machine/storage prefabs have BeltPort children baked in:
- **Output port**: `transform.forward` points outward from machine (away from center)
- **Input port**: `transform.forward` also points outward from machine (away from center)
- `port.Direction` enum distinguishes Input vs Output

Belt segment ports are created at runtime by `GridManager.AddBeltPort`:
- **Input port** (at belt start): `transform.forward = -startDir` (faces backward into belt)
- **Output port** (at belt end): `transform.forward = endDir` (faces forward out of belt)

---

## Task 1: Fix GetPortFlowDirection for machine/storage ports

The routing bug. Machine/storage ports always route outward. The direction returned is always `awayFromMachine` (or equivalently `port.WorldDirection` since prefab forwards point outward). The `isStart` parameter should not flip the sign.

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs` — `GetPortFlowDirection` method (~line 1602)
- Test: `Assets/_Slopworks/Tests/Editor/EditMode/BeltPortDirectionTests.cs` (new file)

**Step 1: Write failing tests for machine/storage port direction**

Create `BeltPortDirectionTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests for GetPortFlowDirection logic extracted to a testable static helper.
/// Verifies that machine/storage ports always return outward direction,
/// and belt ports return direction based on port.Direction enum.
/// </summary>
[TestFixture]
public class BeltPortDirectionTests
{
    private GameObject _machine;
    private GameObject _outputPortGo;
    private GameObject _inputPortGo;
    private BeltPort _outputPort;
    private BeltPort _inputPort;

    [SetUp]
    public void SetUp()
    {
        // Machine at origin, ports on +Z and -Z sides
        _machine = new GameObject("Machine");
        _machine.transform.position = Vector3.zero;

        _outputPortGo = new GameObject("BeltPort_Output_0");
        _outputPortGo.transform.SetParent(_machine.transform);
        _outputPortGo.transform.localPosition = new Vector3(0, 1, 2);
        _outputPortGo.transform.forward = Vector3.forward; // outward from machine
        _outputPort = _outputPortGo.AddComponent<BeltPort>();
        _outputPort.Direction = BeltPortDirection.Output;

        _inputPortGo = new GameObject("BeltPort_Input_0");
        _inputPortGo.transform.SetParent(_machine.transform);
        _inputPortGo.transform.localPosition = new Vector3(0, 1, -2);
        _inputPortGo.transform.forward = -Vector3.forward; // outward from machine (toward -Z)
        _inputPort = _inputPortGo.AddComponent<BeltPort>();
        _inputPort.Direction = BeltPortDirection.Input;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_machine);
    }

    // -- Machine/storage ports: always outward --

    [Test]
    public void MachineOutputPort_StartingBelt_ReturnsOutward()
    {
        var dir = BeltPortDirectionHelper.GetFlowDirection(_outputPort);
        AssertVectorsEqual(Vector3.forward, dir);
    }

    [Test]
    public void MachineInputPort_StartingBelt_ReturnsOutward()
    {
        // Input port on -Z side, forward = -Z = outward from machine
        var dir = BeltPortDirectionHelper.GetFlowDirection(_inputPort);
        AssertVectorsEqual(-Vector3.forward, dir);
    }

    [Test]
    public void MachineOutputPort_EndingBelt_ReturnsSameAsStarting()
    {
        // Direction should not change based on isStart -- always outward
        var dir = BeltPortDirectionHelper.GetFlowDirection(_outputPort);
        AssertVectorsEqual(Vector3.forward, dir);
    }

    private static void AssertVectorsEqual(Vector3 expected, Vector3 actual, float tolerance = 0.01f)
    {
        Assert.That(Vector3.Distance(expected, actual), Is.LessThan(tolerance),
            $"Expected {expected} but got {actual}");
    }
}
```

**Step 2: Create BeltPortDirectionHelper and extract logic**

The current `GetPortFlowDirection` is a private method on `NetworkBuildController` (a NetworkBehaviour). We can't call it from EditMode tests. Extract the pure logic into a static helper class.

Create a new static class or add to an existing utility. Simplest: add a public static method to a new file.

Create `Assets/_Slopworks/Scripts/Automation/BeltPortDirectionHelper.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Pure logic for determining belt flow direction at a port.
/// Extracted from NetworkBuildController for testability.
/// </summary>
public static class BeltPortDirectionHelper
{
    /// <summary>
    /// Get the direction a belt should travel at this port.
    /// Machine/storage ports: always outward from the building (port.WorldDirection).
    /// Belt ports: Output = forward (travel direction), Input = -forward (stored as reversed).
    /// </summary>
    public static Vector3 GetFlowDirection(BeltPort port)
    {
        var parent = port.transform.parent;

        // Belt ports: transform.forward is the stored spline tangent.
        // Output stores endDir (travel direction). Input stores -startDir (opposite).
        // Normalize: Output = forward, Input = -forward.
        if (parent != null && parent.GetComponent<NetworkBeltSegment>() != null)
        {
            return port.Direction == BeltPortDirection.Output
                ? port.WorldDirection
                : -port.WorldDirection;
        }

        // Machine/storage: always outward. Port transform.forward on the prefab
        // already points away from the machine center.
        return port.WorldDirection;
    }
}
```

Then update `GetPortFlowDirection` in `NetworkBuildController.cs` to delegate:

```csharp
private static Vector3 GetPortFlowDirection(BeltPort port, bool isStart)
{
    return BeltPortDirectionHelper.GetFlowDirection(port);
}
```

The `isStart` parameter is now unused but kept to avoid changing all call sites in this task. It can be removed in a cleanup pass.

**Step 3: Run tests to verify they pass**

Run manually in Unity Test Runner: Window > General > Test Runner > EditMode > Run All.
Expected: all new `BeltPortDirectionTests` pass, all existing tests still pass.

**Step 4: Commit**

```
Belt direction fix: machine/storage ports always route outward

Extract GetPortFlowDirection logic into testable BeltPortDirectionHelper.
Machine/storage ports return port.WorldDirection (outward) regardless of
isStart. Belt ports use port.Direction to determine sign instead of isStart.
Fixes belt routing backward through machines on port-to-port placement.
```

---

## Task 2: Add belt port direction tests for belt-to-belt chaining

Verify the belt port path (Output forward, Input reversed) works correctly.

**Files:**
- Modify: `Assets/_Slopworks/Tests/Editor/EditMode/BeltPortDirectionTests.cs`

**Step 1: Add belt port tests**

These need a fake parent with a `NetworkBeltSegment`. Since `NetworkBeltSegment` is a `NetworkBehaviour`, we can't add it in EditMode. Two options:
- (a) Check for `NetworkBeltSegment` by name convention instead of `GetComponent`
- (b) Use a test seam: make the helper accept a `bool isBeltPort` parameter

Option (b) is simpler and avoids coupling to component presence:

Update `BeltPortDirectionHelper.GetFlowDirection` signature:

```csharp
public static Vector3 GetFlowDirection(BeltPort port, bool isBeltPort = false)
```

Where `isBeltPort` defaults to false. The caller in `NetworkBuildController` passes `parent.GetComponent<NetworkBeltSegment>() != null`.

Add tests:

```csharp
[Test]
public void BeltOutputPort_ReturnsForward()
{
    var belt = new GameObject("Belt");
    var portGo = new GameObject("BeltPort_Output_0");
    portGo.transform.SetParent(belt.transform);
    portGo.transform.forward = Vector3.right; // endDir was right
    var port = portGo.AddComponent<BeltPort>();
    port.Direction = BeltPortDirection.Output;

    var dir = BeltPortDirectionHelper.GetFlowDirection(port, isBeltPort: true);
    AssertVectorsEqual(Vector3.right, dir);

    Object.DestroyImmediate(belt);
}

[Test]
public void BeltInputPort_ReturnsNegativeForward()
{
    // Input port stores -startDir as forward. If startDir was +Z,
    // forward = -Z. GetFlowDirection should return -(-Z) = +Z.
    var belt = new GameObject("Belt");
    var portGo = new GameObject("BeltPort_Input_0");
    portGo.transform.SetParent(belt.transform);
    portGo.transform.forward = -Vector3.forward; // stored as -startDir
    var port = portGo.AddComponent<BeltPort>();
    port.Direction = BeltPortDirection.Input;

    var dir = BeltPortDirectionHelper.GetFlowDirection(port, isBeltPort: true);
    AssertVectorsEqual(Vector3.forward, dir); // travel direction = startDir

    Object.DestroyImmediate(belt);
}
```

**Step 2: Update helper to accept isBeltPort parameter**

```csharp
public static Vector3 GetFlowDirection(BeltPort port, bool isBeltPort = false)
{
    if (isBeltPort)
    {
        return port.Direction == BeltPortDirection.Output
            ? port.WorldDirection
            : -port.WorldDirection;
    }

    // Machine/storage: always outward
    return port.WorldDirection;
}
```

Update caller in `NetworkBuildController`:

```csharp
private static Vector3 GetPortFlowDirection(BeltPort port, bool isStart)
{
    var parent = port.transform.parent;
    bool isBeltPort = parent != null && parent.GetComponent<NetworkBeltSegment>() != null;
    return BeltPortDirectionHelper.GetFlowDirection(port, isBeltPort);
}
```

**Step 3: Run tests**

All `BeltPortDirectionTests` pass. All existing tests still pass.

**Step 4: Commit**

```
Add belt-to-belt direction tests, isBeltPort parameter on helper

Belt Output ports return transform.forward (travel direction).
Belt Input ports return -transform.forward (flip stored reverse tangent).
```

---

## Task 3: Connection gating -- reject incompatible port pairs

This enforces the rules from the belt-interactions doc:
- Belt start at a machine port: can start from Output OR Input (both route outward physically)
- Belt end at a machine port: can only connect to the port that matches flow direction
  - If start was from Output: end must be Input (items flow out -> in)
  - If start was from Input: end must be Output (items flow in -> out, reversed belt)
- Belt-to-belt: Output connects to Input, not Output-to-Output or Input-to-Input
- Ground endpoints: no gating (no port to check)

The rule simplifies to: **when connecting to a port as the END of a belt, that port's Direction must be opposite to the start port's Direction.** If the start wasn't a port (ground), any end port is fine.

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs` — store start port direction, check end port compatibility
- Test: `Assets/_Slopworks/Tests/Editor/EditMode/BeltConnectionGatingTests.cs` (new file)

**Step 1: Write failing tests**

Create `BeltConnectionGatingTests.cs`:

```csharp
using NUnit.Framework;

[TestFixture]
public class BeltConnectionGatingTests
{
    // Output-to-Input = valid (normal flow)
    [Test]
    public void OutputToInput_IsCompatible()
    {
        Assert.IsTrue(BeltConnectionGating.AreCompatible(
            BeltPortDirection.Output, BeltPortDirection.Input));
    }

    // Input-to-Output = valid (reversed belt)
    [Test]
    public void InputToOutput_IsCompatible()
    {
        Assert.IsTrue(BeltConnectionGating.AreCompatible(
            BeltPortDirection.Input, BeltPortDirection.Output));
    }

    // Output-to-Output = invalid
    [Test]
    public void OutputToOutput_IsIncompatible()
    {
        Assert.IsFalse(BeltConnectionGating.AreCompatible(
            BeltPortDirection.Output, BeltPortDirection.Output));
    }

    // Input-to-Input = invalid
    [Test]
    public void InputToInput_IsIncompatible()
    {
        Assert.IsFalse(BeltConnectionGating.AreCompatible(
            BeltPortDirection.Input, BeltPortDirection.Input));
    }

    // Ground start (null direction) = any end port is fine
    [Test]
    public void NullStartDirection_AnyEndIsCompatible()
    {
        Assert.IsTrue(BeltConnectionGating.IsEndPortAllowed(
            null, BeltPortDirection.Input));
        Assert.IsTrue(BeltConnectionGating.IsEndPortAllowed(
            null, BeltPortDirection.Output));
    }

    // Port start with matching end
    [Test]
    public void PortStart_CompatibleEnd_IsAllowed()
    {
        Assert.IsTrue(BeltConnectionGating.IsEndPortAllowed(
            BeltPortDirection.Output, BeltPortDirection.Input));
    }

    // Port start with incompatible end
    [Test]
    public void PortStart_IncompatibleEnd_IsRejected()
    {
        Assert.IsFalse(BeltConnectionGating.IsEndPortAllowed(
            BeltPortDirection.Output, BeltPortDirection.Output));
    }
}
```

**Step 2: Implement BeltConnectionGating**

Create `Assets/_Slopworks/Scripts/Automation/BeltConnectionGating.cs`:

```csharp
/// <summary>
/// Rules for which belt port pairs can connect.
/// Output-to-Input and Input-to-Output are valid. Same-to-same is not.
/// </summary>
public static class BeltConnectionGating
{
    /// <summary>
    /// Check if two port directions can connect (start port -> end port).
    /// </summary>
    public static bool AreCompatible(BeltPortDirection startDir, BeltPortDirection endDir)
    {
        return startDir != endDir;
    }

    /// <summary>
    /// Check if an end port is allowed given the start port direction.
    /// Null start direction means the start was ground (no port) -- any end is allowed.
    /// </summary>
    public static bool IsEndPortAllowed(BeltPortDirection? startDir, BeltPortDirection endDir)
    {
        if (!startDir.HasValue)
            return true;
        return AreCompatible(startDir.Value, endDir);
    }
}
```

**Step 3: Run tests, verify pass**

**Step 4: Commit**

```
Add belt connection gating rules

Output-to-Input and Input-to-Output are valid connections.
Same direction pairs (Output-Output, Input-Input) are rejected.
Ground starts allow any end port.
```

---

## Task 4: Wire gating into NetworkBuildController

Store the start port's direction when the first click happens, check compatibility on the end click.

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs`

**Step 1: Add start port direction field**

Near the other belt placement state fields (around line 55):

```csharp
private BeltPortDirection? _beltStartPortDirection;
```

**Step 2: Store start port direction on first click**

In `HandleBeltStart` (around line 1203), after `TryResolveBeltEndpoint` succeeds, store the port direction if the start was a port. This requires `TryResolveBeltEndpoint` to also output the port it resolved to, or we store the direction separately.

Simplest approach: add an `out BeltPortDirection?` parameter to `TryResolveBeltEndpoint`.

Update the signature:

```csharp
private bool TryResolveBeltEndpoint(Ray ray, bool isStart,
    out Vector3 pos, out Vector3 dir, out bool fromPort,
    out BeltPortDirection? portDirection, bool log = false)
```

In each resolution path:
- BeltPort hit: `portDirection = beltPort.Direction`
- FindNearbyPort hit: `portDirection = nearbyPort.Direction`
- BeltSnapAnchor hit: `portDirection = null` (anchors don't have direction)
- Ground fallback: `portDirection = null`

In `HandleBeltStart`, store it:

```csharp
_beltStartPortDirection = portDirection;
```

**Step 3: Check compatibility on end click**

In `HandleBeltDragging`, after `TryResolveBeltEndpoint` succeeds for the end, add the gating check:

```csharp
// Connection gating: reject incompatible port pairs
if (endPortDirection.HasValue &&
    !BeltConnectionGating.IsEndPortAllowed(_beltStartPortDirection, endPortDirection.Value))
{
    isValid = false;
    if (log) Debug.Log($"belt: REJECTED, incompatible ports: start={_beltStartPortDirection} end={endPortDirection}");
}
```

This goes right after the endpoint is resolved and before the existing validation checks, so the line turns red.

**Step 4: Update all TryResolveBeltEndpoint call sites**

There are ~4 call sites. Add `out _` for the new parameter where the direction isn't needed (preview hover), or capture it where it is needed (start click, end click).

**Step 5: Clear state on cancel**

In `CancelBeltPlacement`:

```csharp
_beltStartPortDirection = null;
```

**Step 6: Run tests, playtest manually**

All existing tests pass. Manual test:
- Place machine, try connecting Output-to-Output across two machines -> red line, rejected
- Connect Output-to-Input -> green line, accepted
- Connect ground-to-Input -> accepted
- Connect ground-to-Output -> accepted

**Step 7: Commit**

```
Wire connection gating into belt placement

Store start port direction on first click. Reject end ports with
incompatible direction (Output-Output, Input-Input). Ground starts
allow any end port. Preview line turns red on incompatible connections.
```

---

## Task 5: Update FindNearbyPort to prefer compatible ports

`FindNearbyPort` currently prefers direction based on `isStart` (Output for start, Input for end). With gating, it should prefer ports compatible with the start port's direction when resolving the end.

This is a refinement -- the gating in Task 4 already rejects bad connections, but FindNearbyPort might snap to the wrong port when both Input and Output ports are nearby (e.g. two ports on the same machine side).

**Files:**
- Modify: `Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs` — `FindNearbyPort`

**Step 1: Update FindNearbyPort to accept start port direction**

```csharp
private static BeltPort FindNearbyPort(Vector3 position, BeltPortDirection? startPortDir, float radius)
```

Preference logic:
- If `startPortDir` has a value: prefer the opposite direction (Output start -> prefer Input end)
- If null (ground start): prefer based on legacy `isStart` behavior, or just take closest

**Step 2: Update call sites**

Pass `_beltStartPortDirection` when resolving end, `null` when resolving start.

**Step 3: Playtest, commit**

```
FindNearbyPort prefers direction-compatible ports

When resolving belt end, prefers ports opposite to the start port's
direction. Prevents snapping to an Output when an Input is equally close.
```

---

## Task 6: Update belt-interactions.md with final behavior

**Files:**
- Modify: `docs/reference/belt-interactions.md`

Fill in all "Current behavior" fields with the fixed behavior. Remove the `???` placeholders. This becomes the living reference for belt connection rules.

**Step 1: Update the doc**

**Step 2: Commit**

```
Update belt-interactions.md with implemented behavior
```

---

## Summary of all changes

| File | Change |
|------|--------|
| `Scripts/Automation/BeltPortDirectionHelper.cs` | New: pure static helper for port flow direction |
| `Scripts/Automation/BeltConnectionGating.cs` | New: compatibility rules for port pairs |
| `Scripts/Player/NetworkBuildController.cs` | Delegate to helper, store start port dir, gate end port, update FindNearbyPort |
| `Tests/Editor/EditMode/BeltPortDirectionTests.cs` | New: direction tests for machine and belt ports |
| `Tests/Editor/EditMode/BeltConnectionGatingTests.cs` | New: gating rule tests |
| `docs/reference/belt-interactions.md` | Updated with final behavior |

## What this does NOT change

- Item flow simulation (not implemented yet -- this is routing/placement only)
- Belt port creation on belt segments (`GridManager.AddBeltPort` unchanged)
- Machine/storage prefab ports (already correct)
- BeltRouteBuilder (direction convention unchanged)
- BeltPlacementValidator (geometry validation unchanged)
