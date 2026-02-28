---
name: slopworks-patterns
description: This skill should be used when the user is about to write, review, or refactor C# code in Slopworks. Load it when working on inventory, items, belts, conveyors, factory machines, networking, multiplayer sync, scene communication, or any server/client logic. Essential reference before writing any new MonoBehaviour or NetworkBehaviour in this project.
version: 0.1.0
---

# Slopworks development patterns

Check these patterns before writing any C# code in this project. Getting these wrong causes shared-state corruption, network desync, or broken multiplayer that is hard to debug.

---

## 1. ItemDefinitionSO + ItemInstance split

**Rule: never mutate ScriptableObjects at runtime.**

`ItemDefinitionSO` is a read-only static definition shared by every item of that type. Mutating it in a build corrupts the shared state for all players. Mutating it in the Editor corrupts the asset on disk.

```csharp
// Read-only definition — shared, never modified at runtime
[CreateAssetMenu(menuName = "Items/Item Definition")]
public class ItemDefinitionSO : ScriptableObject {
    public string itemId;       // stable string ID used everywhere
    public string displayName;
    public bool isStackable;
    public int maxStackSize;
    public float maxDurability;
    public ItemCategory category;
}

// Per-instance runtime state — serialize and sync this, not the SO
[Serializable]
public struct ItemInstance : INetworkSerializable, IEquatable<ItemInstance> {
    public string definitionId;   // lookup key into ItemRegistry
    public string instanceId;     // null/empty for stackable commodities
    public float durability;
    public int quality;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter {
        s.SerializeValue(ref definitionId);
        s.SerializeValue(ref instanceId);
        s.SerializeValue(ref durability);
        s.SerializeValue(ref quality);
    }
}
```

To get the definition from an instance: `ItemRegistry.Get(instance.definitionId)`.

Never serialize SO references in save data — they break across builds. Serialize `definitionId` strings only.

---

## 2. NetworkVariable vs RPC — decision tree

```
Does a late-joining client need this value?
  YES → SyncVar or SyncList (persistent state, auto-sent on join)
  NO  → can use RPC

Is this a one-time command or event?
  YES → ServerRpc (client → server) or ObserversRpc (server → all)
  NO  → SyncVar (ongoing state)

Is this a list of items that changes incrementally?
  YES → SyncList (sends only deltas, not the full list)
  NO  → SyncVar

Is this per-frame or high-frequency data?
  HIGH FREQUENCY (movement prediction) → client-predicted, reconcile server
  LOW FREQUENCY (machine status) → SyncVar is fine
```

**Do not use RPCs for persistent state.** Late joiners don't receive RPCs that fired before they connected. Use SyncVar/SyncList for anything that must be visible to a new client on join — machine status, craft progress, active recipe, belt contents.

**SyncVar before spawn is dropped.** State assigned before `NetworkObject.Spawn()` is not sent to clients. Set initial SyncVar values in `OnStartServer`, not in `Awake` or `Start`.

---

## 3. Belt item sync

**Never spawn a NetworkObject per belt item.**

At factory scale, thousands of items on belts would mean thousands of NetworkObjects. This is too expensive and kills performance.

The correct pattern: belt contents are a `SyncList<BeltItem>` on the belt segment's single NetworkObject. The server runs the simulation; clients read the list and interpolate positions visually.

```csharp
// On the belt segment NetworkObject (server-owned)
[SyncObject]
private readonly SyncList<BeltItem> _items = new();

// BeltItem is a plain struct — item type + integer distance offset
public struct BeltItem {
    public ItemType Type;
    public ushort DistanceToNext;  // integer subdivisions, not float
}
```

Use integer distances, not floats. Float math is not deterministic across platforms. Integer subdivisions (e.g. 100 per tile) keep simulation identical on all machines.

In steady-state flow, only `terminalGap` changes each tick — O(1) update cost.

---

## 4. Server-authoritative simulation

**Factory simulation runs on the server only. Clients display, never simulate.**

```csharp
private void FixedUpdate() {
    if (!IsServerInitialized) return;  // server guard — this is the pattern
    // all simulation logic below this line
}
```

The simulation tick (16–20ms, decoupled from render frame rate) runs on server. Each machine tick:
1. Check inputs — are required items in input buffer?
2. If yes, consume inputs, start production timer
3. When timer completes, try to push output — if output buffer full, go BLOCKED

Clients receive machine status changes (IDLE/WORKING/BLOCKED) via SyncVar and display accordingly. They never know about or run the tick logic.

Players interact with machines by sending commands via ServerRpc. The server processes them on the next tick.

```csharp
[ServerRpc]
public void SetRecipeServerRpc(string recipeId) {
    if (!IsServerInitialized) return;
    _activeRecipeId.Value = recipeId;
    _status.Value = MachineStatus.IDLE;
}
```

---

## 5. Cross-scene communication

**Never hold direct references to objects in other scenes.** Unity doesn't allow it, and the three-scene structure (Home Base, Buildings, Overworld) means objects in one scene cannot safely reference objects in another.

Use the ScriptableObject event bus pattern:

```csharp
// Define events as ScriptableObject assets
[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEventSO : ScriptableObject {
    private List<GameEventListenerSO> _listeners = new();
    public void Raise() => _listeners.ForEach(l => l.OnEventRaised());
    public void RegisterListener(GameEventListenerSO l) => _listeners.Add(l);
    public void UnregisterListener(GameEventListenerSO l) => _listeners.Remove(l);
}
```

Events like `PlayerDied`, `BuildingClaimed`, `WaveStarted`, `SceneTransitionRequested` are ScriptableObject assets in `Assets/_Slopworks/ScriptableObjects/Events/`. Any scene can raise or subscribe to them without knowing about the other scenes.

**No singletons for cross-scene state.** Use ScriptableObject references instead — they survive scene loads and don't require FindObjectOfType.

---

## 6. Three-scene structure

Home Base, Reclaimed Buildings, and the Overworld are separate loaded scenes, not a streaming open world.

```
Scenes/
  HomeBase/
    HomeBase_Terrain.unity       — ground, resource nodes
    HomeBase_Grid.unity          — factory grid, belt network
    HomeBase_UI.unity            — HUD, build menu, inventory
    HomeBase_Lighting.unity      — baked GI
  Buildings/
    Building_Template.unity      — base for all reclaimed buildings
    [BuildingName].unity         — one scene per building
  Overworld/
    Overworld_Map.unity          — territory, supply lines
    Overworld_UI.unity           — overworld HUD
  Core/
    Core_Network.unity           — NetworkManager, FishNet setup
    Core_GameManager.unity       — game state, session management
```

`Core_Network.unity` is always loaded first and never unloaded. The NetworkManager lives there.

The factory simulation only runs when `HomeBase_Grid.unity` is loaded. Supply lines from connected buildings are abstracted to a network resource flow — not physical belts spanning scenes.

Scene transitions are host-initiated: `NetworkManager.SceneManager.LoadScene` loads the same scene for all connected clients simultaneously. The factory simulation pauses during transition and resumes on load.

---

## Pitfall quick reference

| Pitfall | Correct pattern |
|---------|----------------|
| `_so.someField = value` at runtime | Read `_so.someField`, write to `_instance.someField` |
| `NetworkVariable<Dictionary<...>>` | `SyncList<ItemSlot>` — only changed slots over wire |
| `Spawn()` then set SyncVar | Set SyncVar in `OnStartServer`, then spawn |
| NetworkObject per belt item | `SyncList<BeltItem>` on one segment NetworkObject |
| RPC for machine status | SyncVar — late joiners need this |
| Client runs FixedUpdate simulation | `if (!IsServerInitialized) return;` guard |
| Direct cross-scene reference | ScriptableObject event bus |
| Singleton for shared state | ScriptableObject reference |
| Float distances on belts | `ushort` integer subdivisions |

## Additional references

- **`references/inventory-patterns.md`** — Full inventory sync patterns, slot filter predicates, save serialization format
- **`references/belt-deep-dive.md`** — Belt simulation details, performance targets, GPU instancing for item rendering
