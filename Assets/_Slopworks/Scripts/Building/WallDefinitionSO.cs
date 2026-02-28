using UnityEngine;

/// <summary>
/// Read-only definition for a wall building type.
/// Never mutate at runtime -- SOs are shared across all instances.
/// </summary>
[CreateAssetMenu(menuName = "Slopworks/Buildings/Wall Definition")]
public class WallDefinitionSO : ScriptableObject, IPlaceableDefinition
{
    public string wallId;
    public string displayName;
    public GameObject prefab;
    public Sprite icon;

    public string PlaceableId => wallId;

    /// <summary>
    /// Walls occupy a single edge, not a grid cell. Size is (1,1) for placement purposes.
    /// </summary>
    Vector2Int IPlaceableDefinition.Size => Vector2Int.one;
}
