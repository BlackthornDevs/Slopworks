# Belt Interactions Reference

Every way a belt endpoint can connect, what direction it should produce, and how the system currently handles it.

---

## Terminology

- **startDir / endDir**: direction of item travel at that endpoint, passed to `BeltRouteBuilder.Build()`
- **awayFromMachine**: unit vector from machine center to port position (horizontal only) --- What does horizontal only mean? when would we have vertical belts?
- **port.WorldDirection**: `transform.forward` on the port GameObject
- **fromPort**: true when clicking on an existing port/anchor, false when clicking ground

---

## 1. Ground to Ground (no ports)

- First click on terrain/foundation, second click on terrain/foundation
- Server spawns supports at both endpoints
- `startDir` = R-key rotation (`_placeRotation`)
- `endDir` = derived by `DeriveEndDirection()` from spatial relationship to start
- `fromPort = false` for both

**Expected**: belt routes from support A to support B in the direction the player is aiming. 

---

## 2. Ground to Machine/Storage Port

- First click on ground, second click on machine/storage port
- Server spawns support at start only
- `startDir` = R-key rotation
- `endDir` = returned by `GetPortFlowDirection(port, isStart=false)`
- Currently returns: `-awayFromMachine` (toward machine)

**Expected direction at end**: connection from belt to machine gated by input or output. only can connect to machines input.
**Current behavior**: lets you connect anything.

---

## 3. Machine/Storage Port to Ground

- First click on machine/storage port, second click on ground
- Server spawns support at end only
- `startDir` = returned by `GetPortFlowDirection(port, isStart=true)`
- Currently returns: `awayFromMachine` (away from machine)
- `endDir` = derived by `DeriveEndDirection(_beltStartPos, startDir, endPos)`

**Expected direction at start**: belt can only be placed with the physical location going out of the machine/storage but the flow or startDir/endDir is determined by input or output node you started with. so you can start at an input of a machine and place belts "leaving" the machine but it acts like you started out of the machine and went into it. normal behavior for output start and end dir leaving the output means leaving with that direction away from the machine.
**Current behavior**: current bug. lets you run the belt backwards from one end through the end of another

---

## 4. Machine/Storage Port to Machine/Storage Port

- First click on machine port, second click on another machine port
- No supports spawned
- `startDir` = `GetPortFlowDirection(startPort, isStart=true)` = `awayFromMachine`
- `endDir` = `GetPortFlowDirection(endPort, isStart=false)` = `-awayFromMachine`
- `endDir` is NOT overridden (endFromPort=true skips DeriveEndDirection)

**Expected direction at start**: same as machine/stroage port to ground.
**Expected direction at end**: gated by machine/storage port input or output. cannot place started at input into end input.
**Current behavior**: this is the screenshot bug -- belt routes backward through both machines. --- it also doesnt give a shit about the actual input or output

---

## 5. Machine/Storage Port to Belt Port (belt chaining)

- First click on machine port, second click on existing belt's Input or Output port
- `startDir` = `GetPortFlowDirection(machinePort, isStart=true)` = `awayFromMachine`
- `endDir` = `GetPortFlowDirection(beltPort, isStart=false)`
  - Belt Output port: returns `-port.WorldDirection` = `-endDir_original`
  - Belt Input port: returns `port.WorldDirection` = `-startDir_original` (stored as negative)

**Expected direction at start**: exact concept as #3
**Expected direction at end**: can only connect to another belt port that matches the item flow direction
**Current behavior**: lets you connect anything

---

## 6. Belt Port to Machine/Storage Port

- First click on existing belt port, second click on machine port
- `startDir` = `GetPortFlowDirection(beltPort, isStart=true)`
  - Belt Output port: returns `port.WorldDirection` = `endDir_original`
  - Belt Input port: returns `-port.WorldDirection`... wait, returns `port.WorldDirection` = `-startDir_original`
- `endDir` = `GetPortFlowDirection(machinePort, isStart=false)` = `-awayFromMachine`

**Expected direction at start**: inherit the flow direction from the belt and build away from the support
**Expected direction at end**: gated by input out node. can only build into a machine with the matching flow direction
**Current behavior**: can connect anything

---

## 7. Belt Port to Belt Port (belt-to-belt chaining)

- First click on belt A's port, second click on belt B's port
- `startDir` = `GetPortFlowDirection(portA, isStart=true)`
- `endDir` = `GetPortFlowDirection(portB, isStart=false)`

**Expected direction at start**:inherit the flow direction from the belt and build away from the support
**Expected direction at end**: can only connect to belt that matches the starts flow direction
**Current behavior**: can connect anything

---

## 8. Ground to Belt Port

- First click on ground, second click on existing belt port
- `startDir` = R-key rotation
- `endDir` = `GetPortFlowDirection(beltPort, isStart=false)`

**Expected direction at end**: can only connect to belt that matches the starts flow direction
**Current behavior**: can connect anything

---

## 9. Belt Port to Ground

- First click on existing belt port, second click on ground
- `startDir` = `GetPortFlowDirection(beltPort, isStart=true)`
- `endDir` = derived by `DeriveEndDirection()`

**Expected direction at start**: inherit the flow direction from the belt and build away from the support
**Current behavior**: i think we inherit the flow but im not sure. the build always goes out from that point though.

---

## 10. Support Anchor to Support Anchor

- First click on existing support's BeltSnapAnchor, second click on another support's anchor
- `startDir` = `anchor.WorldDirection` (transform.forward on anchor)
- `endDir` = `anchor.WorldDirection` on second anchor
- Both `fromPort = true`

**Expected**: belt routes between the two supports using their facing directions
**Current behavior**: i think it does it 

---

## 11. Support Anchor to Ground (and reverse)

- One click on anchor, other click on ground
- Anchor side: `dir = anchor.WorldDirection`, `fromPort = true`
- Ground side: `startDir` = R-key or `endDir` = DeriveEndDirection

**Expected**: flow = direction of belt drawn
**Current behavior**: i think it does that

---

## 12. Support Anchor to Machine/Storage Port (and reverse)

- One click on support anchor, other click on machine port
- Anchor side: `dir = anchor.WorldDirection`
- Machine side: `dir = GetPortFlowDirection()`

**Expected**: flow = direction of belt drawn
**Current behavior**: i think it does that

---

## Notes

- `DeriveEndDirection()` only runs when `!endFromPort` -- it uses `startDir` as the reference axis
- If `startDir` is wrong (pointing into a machine), `DeriveEndDirection` computes everything relative to that bad axis, compounding the error
- `AddBeltPort` stores: Input port forward = `-startDir`, Output port forward = `endDir`
- The route builder (`BeltRouteBuilder.Build`) expects both `startDir` and `endDir` to point in the direction of item travel at that endpoint






=== BELT DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1683)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  belt state: Dragging
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1684)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  belt tool: Belt
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1685)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  routing mode: Straight
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1686)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  placeRotation: 0
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1687)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  START pos: (57.00, 22.43, 36.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1691)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  START groundPos: (57.00, 22.43, 36.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1692)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  START dir: (1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1693)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  START fromPort: True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1694)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  START nearby port: Output on MACHINE/STORAGE 'CONSTRUCTOR(Clone) (4)' at (57.00, 22.43, 36.00) fwd=(1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1704)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  RAYCAST hit: 'BeltPort_Output_0' layer=21 pos=(63.66, 22.32, 34.16)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1717)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    parent: 'STORAGE CONTAINER(Clone) (5)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1718)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    grandparent: 'FAKE BASE'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1719)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    BELTPORT found: Output on machine/storage 'STORAGE CONTAINER(Clone) (5)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1731)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      port.WorldPosition: (64.00, 22.43, 34.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1732)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      port.WorldDirection (transform.forward): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1733)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      parent.position (center): (64.00, 21.43, 38.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1734)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      awayFromMachine (raw): (0.00, 0.00, -3.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1735)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      awayFromMachine (normalized): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1736)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      GetPortFlowDirection(isStart=true):  (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1737)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      GetPortFlowDirection(isStart=false): (0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1738)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

      port.IsFull: False (0 connections)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1739)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    FindNearbyPort(isStart=true):  Output at (64.00, 22.43, 34.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1753)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    FindNearbyPort(isStart=false): Output at (64.00, 22.43, 34.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1754)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    FindNearbyAnchor: null
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1758)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  END resolved: pos=(64.00, 22.43, 34.50) dir=(0.00, 0.00, -1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1765)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  END endFromPort=True -> DeriveEndDirection SKIPPED=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1766)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  END port type: Output on MACHINE/STORAGE
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1779)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  DeriveEndDirection (for comparison): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1789)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  DIRECTION MISMATCH: no
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1790)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  ROUTE (current endDir):  4 waypoints, length=7.91m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1797)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  ROUTE (derived endDir):  4 waypoints, length=7.91m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1798)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    current first wp: pos=(57.00, 22.43, 36.00) tanOut=(2.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1799)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    current last wp:  pos=(64.00, 22.43, 34.50) tanIn=(0.00, 0.00, 0.17)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1800)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    derived first wp: pos=(57.00, 22.43, 36.00) tanOut=(2.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1801)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

    derived last wp:  pos=(64.00, 22.43, 34.50) tanIn=(0.00, 0.00, 0.17)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1802)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  ROUTE closest to end machine center: 2.24m (passes through if < 1.0)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1817)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  Validation (current): valid=True error=None
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1823)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

  Validation (derived): valid=True error=None
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1824)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

=== END DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1846)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:153)

