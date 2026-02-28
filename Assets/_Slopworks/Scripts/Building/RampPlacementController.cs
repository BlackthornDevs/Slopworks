using UnityEngine;

/// <summary>
/// Finds valid ramp placement positions at foundation edges.
/// Plain C# class (D-004).
/// </summary>
public class RampPlacementController
{
    private readonly SnapPointRegistry _snapRegistry;
    private readonly FactoryGrid _grid;

    public SnapPoint SelectedBaseSnap { get; private set; }
    public Vector2Int RampDirection { get; private set; }
    public bool IsValid { get; private set; }

    public RampPlacementController(SnapPointRegistry snapRegistry, FactoryGrid grid)
    {
        _snapRegistry = snapRegistry;
        _grid = grid;
    }

    /// <summary>
    /// Find the best ramp placement near the cursor. The ramp extends outward from
    /// the foundation edge into empty space on the lower level.
    /// </summary>
    public void UpdateFromCursor(Vector3 cursorWorldPos, int level, int footprintLength)
    {
        SelectedBaseSnap = null;
        RampDirection = Vector2Int.zero;
        IsValid = false;

        var cursorCell = _grid.WorldToCell(cursorWorldPos);

        float bestDist = float.MaxValue;
        SnapPoint bestSnap = null;

        // Search cursor cell and neighbors for foundation edge snap points
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                var checkCell = new Vector2Int(cursorCell.x + dx, cursorCell.y + dz);
                var available = _snapRegistry.GetAvailableAt(checkCell, level);

                foreach (var snap in available)
                {
                    if (snap.Type != SnapPointType.FoundationEdge)
                        continue;

                    var snapWorldPos = WallPlacementController.GetSnapWorldPosition(snap, _grid);
                    float dist = Vector3.Distance(cursorWorldPos, snapWorldPos);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestSnap = snap;
                    }
                }
            }
        }

        if (bestSnap == null)
            return;

        // Ramp extends outward from the foundation edge
        var direction = bestSnap.EdgeDirection;

        // Check if all ramp footprint cells are empty on the base level
        var rampStart = bestSnap.Cell + direction;
        for (int i = 0; i < footprintLength; i++)
        {
            var cell = rampStart + direction * i;
            if (!_grid.CanPlace(cell, Vector2Int.one, level))
                return;
        }

        // Check that the upper level is valid
        if (level + 1 >= FactoryGrid.MaxLevels)
            return;

        SelectedBaseSnap = bestSnap;
        RampDirection = direction;
        IsValid = true;
    }
}
