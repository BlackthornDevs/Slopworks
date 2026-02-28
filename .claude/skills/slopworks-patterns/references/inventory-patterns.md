# Inventory patterns — detailed reference

## Full ItemSlot struct

```csharp
[Serializable]
public struct ItemSlot : INetworkSerializable, IEquatable<ItemSlot> {
    public ItemInstance item;
    public int count;
    public bool isEmpty => count == 0;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter {
        item.NetworkSerialize(s);
        s.SerializeValue(ref count);
    }
    public bool Equals(ItemSlot other) => item.Equals(other.item) && count == other.count;
}
```

## ItemRegistry

```csharp
public static class ItemRegistry {
    private static Dictionary<string, ItemDefinitionSO> _items;

    public static void Initialize(IEnumerable<ItemDefinitionSO> defs) {
        _items = defs.ToDictionary(d => d.itemId);
    }

    public static ItemDefinitionSO Get(string id) =>
        _items.TryGetValue(id, out var def) ? def : null;
}
```

Load all `ItemDefinitionSO` assets at startup via `Resources.LoadAll<ItemDefinitionSO>()` or Addressables. Never scan at runtime. The registry must be initialized before any code that calls `ItemRegistry.Get()`.

## Inventory types

| Type | Pattern | Notes |
|------|---------|-------|
| Player backpack | Slot array, 36 slots | Owned by player's NetworkObject |
| Hotbar | Slot array, 9 slots | Subset of backpack or separate |
| Machine input buffer | Typed slots (filter per slot) | Server-owned, not directly player-accessible |
| Machine output buffer | Typed slots | Server-owned, players extract via inserter |
| Storage container | Configurable size | Readable by all players in session |

## Slot filter predicates

Machine buffers restrict which items can enter:

```csharp
public class InventorySlot {
    public Func<ItemDefinitionSO, bool> acceptFilter;  // null = accept anything
    public ItemSlot contents;
}

// Example: furnace input slot only accepts ore
inputSlot.acceptFilter = def => def.category == ItemCategory.Ore;
```

## Multiplayer sync

```csharp
public class NetworkInventory : NetworkBehaviour {
    [SyncObject]
    private readonly SyncList<ItemSlot> _slots = new();

    [ServerRpc(RequireOwnership = true)]
    public void RequestPickupServerRpc(ulong itemNetworkId) {
        // validate: item exists, in range, slot available
        // modify _slots — SyncList auto-replicates delta to clients
    }

    [ServerRpc(RequireOwnership = true)]
    public void RequestCraftServerRpc(string recipeId, ulong workstationId) {
        // validate: player near workstation, has ingredients
        // consume inputs, start craft timer
    }
}
```

### Visibility rules

- Player backpack: `NetworkVariableReadPermission.Owner` — only the owning client sees it
- Storage containers: all players in the session can read
- Machine buffers: all players can read (for UI), server only can write

## Stacking rules

When adding a stackable item:
1. Find an existing slot with the same `definitionId` that has room
2. Increment `count` on that slot
3. Only create a new slot if no existing slot can accept it

Do not create two slots with the same `definitionId`. Keep `instanceId` null for stackable commodities (ore, wood, etc.).

## Save serialization

Never serialize ScriptableObject references — they break across builds. Serialize `definitionId` strings only.

```json
{
  "saveVersion": 1,
  "slots": [
    { "definitionId": "iron_ore", "count": 64, "instanceId": null, "durability": 1.0 },
    { "definitionId": "iron_pickaxe", "count": 1, "instanceId": "abc-123", "durability": 0.73 }
  ],
  "discoveredRecipes": ["basic_plate", "iron_gear", "conveyor_belt"]
}
```

Use a `saveVersion` field in every save document. Increment when save format changes. Write a migration path from version N to N+1.

## Recipe discovery

Recipe discovery is per-player. Do not sync it globally. Mark a recipe discovered when:
- Player first picks up any ingredient, OR
- Explicit research unlock

The UI filters the full recipe registry to show only discovered ones. The server does not enforce recipe discovery — it's a UI-only filter.

## Late joiner behavior

`SyncList` state is automatically sent to new clients on join. No manual "send full state" RPC is needed. This is why inventories must use SyncList, not RPCs.
