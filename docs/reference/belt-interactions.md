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
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1876)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt state: Dragging
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1877)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt tool: Belt
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1878)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  routing mode: Curved
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1879)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  placeRotation: 0
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1880)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START pos: (53.00, 22.43, 32.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1884)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START groundPos: (53.00, 22.43, 32.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1885)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START dir: (-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1886)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START fromPort: True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1887)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START nearby port: Input on MACHINE/STORAGE 'CONSTRUCTOR(Clone) (3)' at (53.00, 22.43, 32.00) fwd=(-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1897)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  RAYCAST hit: 'BeltPort_Input_0' layer=21 pos=(50.94, 22.52, 38.01)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1910)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    parent: 'STORAGE CONTAINER(Clone)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1911)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    grandparent: '(none)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1912)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    BELTPORT found: Input on machine/storage 'STORAGE CONTAINER(Clone)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1924)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.WorldPosition: (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1925)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.WorldDirection (transform.forward): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1926)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      parent.position (center): (51.00, 21.43, 42.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1927)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      awayFromMachine (raw): (0.00, 0.00, -3.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1928)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      awayFromMachine (normalized): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1929)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      GetPortFlowDirection(isStart=true):  (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1930)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      GetPortFlowDirection(isStart=false): (0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1931)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.IsFull: False (0 connections)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1932)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=true):  Input at (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1946)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=false): Input at (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1947)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyAnchor: null
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1951)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END raw: pos=(51.00, 22.43, 38.50) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1958)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END adjusted: pos=(51.00, 22.43, 38.50) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1968)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END port type: Input on MACHINE/STORAGE
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1981)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START: pos=(53.00, 22.43, 32.00) dir=(-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1991)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  ROUTE: 4 waypoints, length=7.45m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1995)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[0]: pos=(53.00, 22.43, 32.00) tanIn=(0.00, 0.00, 0.00) tanOut=(-0.07, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[1]: pos=(52.80, 22.43, 32.00) tanIn=(0.07, 0.00, 0.00) tanOut=(-0.99, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[2]: pos=(51.00, 22.43, 33.80) tanIn=(0.00, 0.00, -0.99) tanOut=(0.00, 0.00, 1.57)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[3]: pos=(51.00, 22.43, 38.50) tanIn=(0.00, 0.00, -1.57) tanOut=(0.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  ROUTE closest to end machine center: 3.50m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2014)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  Validate: valid=True error=None
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2018)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

=== END DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2043)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)




=== BELT DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1876)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt state: Dragging
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1877)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt tool: Belt
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1878)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  routing mode: Curved
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1879)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  placeRotation: 0
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1880)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START pos: (53.00, 22.43, 32.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1884)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START groundPos: (53.00, 22.43, 32.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1885)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START dir: (-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1886)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START fromPort: True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1887)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START nearby port: Input on MACHINE/STORAGE 'CONSTRUCTOR(Clone) (3)' at (53.00, 22.43, 32.00) fwd=(-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1897)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  RAYCAST hit: 'BeltSnapAnchor' layer=21 pos=(50.92, 22.46, 34.88)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1910)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    parent: 'BELT SUPPORT(Clone)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1911)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    grandparent: '(none)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1912)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    ANCHOR found: pos=(51.00, 22.43, 35.00) dir=(0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1939)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      IsFull: False (0 connections)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1940)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=true):  null
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1946)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=false): null
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1947)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyAnchor: pos=(51.00, 22.43, 35.00) full=False
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1951)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END raw: pos=(51.00, 22.43, 35.00) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1958)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END adjusted: pos=(51.00, 22.43, 35.00) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1968)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END port type: ANCHOR
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1986)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START: pos=(53.00, 22.43, 32.00) dir=(-1.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1991)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  ROUTE: 4 waypoints, length=3.95m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1995)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[0]: pos=(53.00, 22.43, 32.00) tanIn=(0.00, 0.00, 0.00) tanOut=(-0.07, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[1]: pos=(52.80, 22.43, 32.00) tanIn=(0.07, 0.00, 0.00) tanOut=(-0.99, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[2]: pos=(51.00, 22.43, 33.80) tanIn=(0.00, 0.00, -0.99) tanOut=(0.00, 0.00, 0.40)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[3]: pos=(51.00, 22.43, 35.00) tanIn=(0.00, 0.00, -0.40) tanOut=(0.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  Validate: valid=True error=None
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2018)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

=== END DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2043)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

=== BELT DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1876)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt state: Dragging
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1877)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  belt tool: Belt
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1878)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  routing mode: Curved
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1879)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  placeRotation: 0
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1880)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START pos: (51.00, 22.43, 35.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1884)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START groundPos: (51.00, 22.43, 35.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1885)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START dir: (0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1886)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START fromPort: True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1887)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START nearby port: Output on BELT 'Belt(Clone)' at (51.00, 22.43, 35.00) fwd=(0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1897)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START nearby anchor: pos=(51.00, 22.43, 35.00) dir=(0.00, 0.00, 1.00) full=False
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1901)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  RAYCAST hit: 'BeltPort_Input_0' layer=21 pos=(50.75, 22.60, 38.10)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1910)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    parent: 'STORAGE CONTAINER(Clone)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1911)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    grandparent: '(none)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1912)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    BELTPORT found: Input on machine/storage 'STORAGE CONTAINER(Clone)'
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1924)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.WorldPosition: (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1925)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.WorldDirection (transform.forward): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1926)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      parent.position (center): (51.00, 21.43, 42.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1927)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      awayFromMachine (raw): (0.00, 0.00, -3.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1928)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      awayFromMachine (normalized): (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1929)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      GetPortFlowDirection(isStart=true):  (0.00, 0.00, -1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1930)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      GetPortFlowDirection(isStart=false): (0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1931)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

      port.IsFull: False (0 connections)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1932)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=true):  Input at (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1946)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyPort(isStart=false): Input at (51.00, 22.43, 38.50)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1947)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    FindNearbyAnchor: null
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1951)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END raw: pos=(51.00, 22.43, 38.50) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1958)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END adjusted: pos=(51.00, 22.43, 38.50) dir=(0.00, 0.00, 1.00) fromPort=True
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1968)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  END port type: Input on MACHINE/STORAGE
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1981)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  START: pos=(51.00, 22.43, 35.00) dir=(0.00, 0.00, 1.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1991)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  ROUTE: 2 waypoints, length=3.50m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1995)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[0]: pos=(51.00, 22.43, 35.00) tanIn=(0.00, 0.00, 0.00) tanOut=(0.00, 0.00, 1.17)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

    wp[1]: pos=(51.00, 22.43, 38.50) tanIn=(0.00, 0.00, -1.17) tanOut=(0.00, 0.00, 0.00)
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:1999)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  ROUTE closest to end machine center: 3.50m
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2014)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

  Validate: valid=True error=None
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2018)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

=== END DIAGNOSTIC DUMP ===
UnityEngine.Debug:Log (object)
NetworkBuildController:DumpBeltDiagnostics () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:2043)
NetworkBuildController:Update () (at Assets/_Slopworks/Scripts/Player/NetworkBuildController.cs:200)

