using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GameObject _foundationPrefab;

    private FactoryGrid _grid;
    private SnapPointRegistry _snapRegistry;
    private StructuralPlacementService _structuralService;
    private readonly Dictionary<Vector3Int, GameObject> _spawnedObjects = new();

    public FactoryGrid Grid => _grid;

    private void Awake()
    {
        Instance = this;
        _grid = new FactoryGrid();
        _snapRegistry = new SnapPointRegistry();
        _structuralService = new StructuralPlacementService(_grid, _snapRegistry);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CmdPlaceFoundation(Vector2Int cell, int level, NetworkConnection sender = null)
    {
        if (!IsServerInitialized) return;

        var size = Vector2Int.one;
        if (!_grid.CanPlace(cell, size, level))
        {
            Debug.Log($"grid: placement rejected at ({cell.x},{cell.y}) level {level}");
            return;
        }

        var data = new BuildingData("foundation", cell, size, 0, level);
        data.IsStructural = true;
        _grid.Place(cell, size, level, data);

        Vector3 worldPos = _grid.CellToWorld(cell, level);
        var go = Instantiate(_foundationPrefab, worldPos, Quaternion.identity);
        ServerManager.Spawn(go);

        var key = new Vector3Int(cell.x, cell.y, level);
        _spawnedObjects[key] = go;

        Debug.Log($"grid: foundation placed at ({cell.x},{cell.y}) level {level} by {sender?.ClientId}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void CmdRemoveFoundation(Vector2Int cell, int level, NetworkConnection sender = null)
    {
        if (!IsServerInitialized) return;

        var data = _grid.GetAt(cell, level);
        if (data == null || !data.IsStructural)
        {
            Debug.Log($"grid: nothing to remove at ({cell.x},{cell.y}) level {level}");
            return;
        }

        _grid.Remove(cell, data.Size, level);

        var key = new Vector3Int(cell.x, cell.y, level);
        if (_spawnedObjects.TryGetValue(key, out var go))
        {
            ServerManager.Despawn(go);
            _spawnedObjects.Remove(key);
        }

        Debug.Log($"grid: foundation removed at ({cell.x},{cell.y}) level {level} by {sender?.ClientId}");
    }
}
