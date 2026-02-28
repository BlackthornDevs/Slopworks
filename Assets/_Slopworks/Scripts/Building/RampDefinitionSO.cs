using UnityEngine;

/// <summary>
/// Read-only definition for a ramp building type.
/// Ramps connect level N to level N+1 for player traversal.
/// Never mutate at runtime -- SOs are shared across all instances.
/// </summary>
[CreateAssetMenu(menuName = "Slopworks/Buildings/Ramp Definition")]
public class RampDefinitionSO : ScriptableObject, IPlaceableDefinition
{
    public string rampId;
    public string displayName;
    public GameObject prefab;
    public Sprite icon;

    /// <summary>
    /// Number of cells the ramp occupies on the lower level (depth along direction).
    /// Default 3 cells for 45-degree slope with LevelHeight = 3.0f.
    /// </summary>
    public int footprintLength = 3;

    public string PlaceableId => rampId;

    /// <summary>
    /// Ramp is 1 cell wide, footprintLength cells deep.
    /// </summary>
    Vector2Int IPlaceableDefinition.Size => new Vector2Int(1, footprintLength);
}
