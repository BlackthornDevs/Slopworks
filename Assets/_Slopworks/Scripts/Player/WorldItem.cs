using UnityEngine;

/// <summary>
/// Universal walk-over pickup. Placed on the Interactable physics layer.
/// Any item type uses this -- loot, fragments, resources. All go to PlayerInventory.
/// </summary>
public class WorldItem : MonoBehaviour
{
    [SerializeField] private ItemDefinitionSO _definition;
    [SerializeField] private int _count = 1;

    public ItemDefinitionSO Definition => _definition;
    public int Count => _count;

    public void Initialize(ItemDefinitionSO definition, int count)
    {
        _definition = definition;
        _count = count;
    }

    private void Start()
    {
        gameObject.layer = PhysicsLayers.Interactable;

        // Ensure we have a trigger SphereCollider -- remove any leftover solid collider first
        var existing = GetComponent<Collider>();
        if (existing != null && existing is not SphereCollider)
            DestroyImmediate(existing);

        var sphere = GetComponent<SphereCollider>();
        if (sphere == null)
            sphere = gameObject.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 0.5f;

    }

    public bool TryCollect(PlayerInventory inventory)
    {
        if (_definition == null || _count <= 0) return false;

        var instance = ItemInstance.Create(_definition.itemId);
        if (!inventory.TryAdd(instance, _count))
            return false;

        Debug.Log($"picked up {_count}x {_definition.displayName}");
        Destroy(gameObject);
        return true;
    }
}
