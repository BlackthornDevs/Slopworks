using UnityEngine;

/// <summary>
/// Data for a building placed on the factory grid.
/// Plain C# class -- no MonoBehaviour dependency.
/// </summary>
public class BuildingData
{
    public string BuildingId { get; set; }
    public Vector2Int Origin { get; set; }
    public Vector2Int Size { get; set; }
    public int Rotation { get; set; }
    public int Level { get; set; }
    public GameObject Instance { get; set; }

    /// <summary>
    /// Structural buildings (foundations) act as a base layer that other things build on top of.
    /// Non-structural buildings (machines, storage) block cell placement normally.
    /// </summary>
    public bool IsStructural { get; set; }

    public BuildingData(string buildingId, Vector2Int origin, Vector2Int size, int rotation = 0, int level = 0)
    {
        BuildingId = buildingId;
        Origin = origin;
        Size = size;
        Rotation = rotation;
        Level = level;
    }
}
