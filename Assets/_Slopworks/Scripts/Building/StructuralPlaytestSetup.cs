using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Self-contained playtest bootstrapper for the structural building system.
/// Drop on an empty GameObject, hit Play, and test foundation/wall/ramp placement.
/// Uses New Input System (D-003).
///
/// Controls:
///   1 - Foundation tool (drag rectangle)
///   2 - Wall tool (click near snap point)
///   3 - Ramp tool (click near snap point)
///   4 - Delete tool (click to remove)
///   Escape - Cancel / deselect tool
///   PageUp / PageDown - Change active level
///   Right-click + mouse to look, WASD to fly
///
/// Everything is created at runtime -- no prefabs or assets required.
/// </summary>
public class StructuralPlaytestSetup : MonoBehaviour
{
    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    // -- Infrastructure --
    private FactoryGrid _grid;
    private SnapPointRegistry _snapRegistry;
    private StructuralPlacementService _placementService;

    // -- Definitions (created at runtime) --
    private FoundationDefinitionSO _foundationDef;
    private WallDefinitionSO _wallDef;
    private RampDefinitionSO _rampDef;

    // -- Tracking --
    private readonly List<BuildingData> _foundations = new();
    private readonly List<WallData> _walls = new();
    private readonly List<RampData> _ramps = new();

    // -- Tool state --
    private enum ToolMode { None, Foundation, Wall, Ramp, Delete }
    private ToolMode _currentTool = ToolMode.None;
    private int _currentLevel;

    // -- Ramp 2-step placement state --
    private bool _pendingRamp;
    private Vector2Int _pendingRampCell;
    private int _pendingRampDirIndex;
    private GameObject _pendingRampPreview;
    private static readonly string[] DirectionNames = { "North", "East", "South", "West" };

    // -- Foundation drag state --
    private bool _isDragging;
    private Vector2Int _dragStart;
    private Vector2Int _dragEnd;
    private readonly List<GameObject> _ghostPool = new();

    // -- Wall 2-step placement state --
    private bool _pendingWall;
    private Vector2Int _pendingWallCell;
    private int _pendingWallDirIndex;
    private GameObject _pendingWallPreview;

    // -- Ground plane --
    private GameObject _groundPlane;

    // -- Colors --
    private static readonly Color _ghostValidColor = new Color(0f, 1f, 0f, 0.4f);
    private static readonly Color _ghostInvalidColor = new Color(1f, 0f, 0f, 0.4f);

    private void Awake()
    {
        CreateDefinitions();
        CreateInfrastructure();
        CreateGroundPlane();
        SetupCamera();

        Debug.Log("Structural playtest started. Press 1-4 to select tools, PageUp/Down to change level.");
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        HandleToolSelection(kb);
        HandleLevelChange(kb);

        switch (_currentTool)
        {
            case ToolMode.Foundation:
                HandleFoundationInput(mouse);
                break;
            case ToolMode.Wall:
                HandleWallInput(kb, mouse);
                break;
            case ToolMode.Ramp:
                HandleRampInput(kb, mouse);
                break;
            case ToolMode.Delete:
                HandleDeleteInput(mouse);
                break;
        }
    }

    private void OnGUI()
    {
        float x = 10;
        float y = 10;
        float w = 350;
        float h = 22;

        GUI.Box(new Rect(x - 4, y - 4, w + 8, h * 10 + 8), "");

        DrawLine(ref y, x, w, h, "STRUCTURAL BUILDING PLAYTEST", true);
        DrawLine(ref y, x, w, h, $"Tool: {_currentTool}  |  Level: {_currentLevel}");
        y += 4;
        DrawLine(ref y, x, w, h, $"Foundations: {_foundations.Count}");
        DrawLine(ref y, x, w, h, $"Walls: {_walls.Count}");
        DrawLine(ref y, x, w, h, $"Ramps: {_ramps.Count}");
        DrawLine(ref y, x, w, h, $"Snap points: {_snapRegistry.Count}");
        y += 4;
        DrawLine(ref y, x, w, h, "[1] Foundation  [2] Wall  [3] Ramp  [4] Delete");
        DrawLine(ref y, x, w, h, "[PgUp/PgDn] Level  [Esc] Cancel");
        if (_currentTool == ToolMode.Wall)
        {
            if (_pendingWall)
                DrawLine(ref y, x, w, h, $"Wall: {DirectionNames[_pendingWallDirIndex]}  [R] rotate  [Click] confirm  [Esc] cancel");
            else
                DrawLine(ref y, x, w, h, "Click any foundation cell to preview wall");
        }
        if (_currentTool == ToolMode.Ramp)
        {
            if (_pendingRamp)
                DrawLine(ref y, x, w, h, $"Ramp: {DirectionNames[_pendingRampDirIndex]}  [R] rotate  [Click] confirm  [Esc] cancel");
            else
                DrawLine(ref y, x, w, h, "Click any foundation cell to preview ramp");
        }

        if (_isDragging)
        {
            var min = Vector2Int.Min(_dragStart, _dragEnd);
            var max = Vector2Int.Max(_dragStart, _dragEnd);
            var size = max - min + Vector2Int.one;
            DrawLine(ref y, x, w, h, $"Dragging: {size.x}x{size.y} from ({min.x},{min.y})");
        }
    }

    private void OnDestroy()
    {
        if (_foundationDef != null) DestroyImmediate(_foundationDef);
        if (_wallDef != null) DestroyImmediate(_wallDef);
        if (_rampDef != null) DestroyImmediate(_rampDef);
    }

    // -- Setup --

    private void CreateDefinitions()
    {
        _foundationDef = ScriptableObject.CreateInstance<FoundationDefinitionSO>();
        _foundationDef.foundationId = "foundation_1x1";
        _foundationDef.displayName = "Foundation 1x1";
        _foundationDef.size = Vector2Int.one;
        _foundationDef.generatesSnapPoints = true;

        _wallDef = ScriptableObject.CreateInstance<WallDefinitionSO>();
        _wallDef.wallId = "wall_basic";
        _wallDef.displayName = "Basic Wall";

        _rampDef = ScriptableObject.CreateInstance<RampDefinitionSO>();
        _rampDef.rampId = "ramp_basic";
        _rampDef.displayName = "Basic Ramp";
        _rampDef.footprintLength = 3;
    }

    private void CreateInfrastructure()
    {
        _grid = new FactoryGrid();
        _snapRegistry = new SnapPointRegistry();
        _placementService = new StructuralPlacementService(_grid, _snapRegistry);
    }

    private void CreateGroundPlane()
    {
        _groundPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _groundPlane.name = "GridPlane";
        _groundPlane.layer = PhysicsLayers.GridPlane;
        _groundPlane.transform.position = new Vector3(
            FactoryGrid.Width * FactoryGrid.CellSize * 0.5f,
            -0.05f,
            FactoryGrid.Height * FactoryGrid.CellSize * 0.5f);
        _groundPlane.transform.localScale = new Vector3(
            FactoryGrid.Width * FactoryGrid.CellSize,
            0.1f,
            FactoryGrid.Height * FactoryGrid.CellSize);

        var renderer = _groundPlane.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float centerX = 10f * FactoryGrid.CellSize;
        float centerZ = 10f * FactoryGrid.CellSize;
        cam.transform.position = new Vector3(centerX, 20f, centerZ - 12f);
        cam.transform.LookAt(new Vector3(centerX, 0f, centerZ));

        // Add fly camera controller if not already present
        if (cam.GetComponent<PlaytestCameraController>() == null)
            cam.gameObject.AddComponent<PlaytestCameraController>();
    }

    // -- Input handling (New Input System) --

    private void HandleToolSelection(Keyboard kb)
    {
        if (kb.digit1Key.wasPressedThisFrame)
        {
            CancelAllPending();
            _currentTool = ToolMode.Foundation;
        }
        else if (kb.digit2Key.wasPressedThisFrame)
        {
            CancelAllPending();
            _currentTool = ToolMode.Wall;
        }
        else if (kb.digit3Key.wasPressedThisFrame)
        {
            CancelAllPending();
            _currentTool = ToolMode.Ramp;
        }
        else if (kb.digit4Key.wasPressedThisFrame)
        {
            CancelAllPending();
            _currentTool = ToolMode.Delete;
        }
        else if (kb.escapeKey.wasPressedThisFrame)
        {
            CancelAllPending();
            _currentTool = ToolMode.None;
        }
    }

    private void HandleLevelChange(Keyboard kb)
    {
        if (kb.pageUpKey.wasPressedThisFrame)
        {
            _currentLevel = Mathf.Min(_currentLevel + 1, FactoryGrid.MaxLevels - 1);
            UpdateGroundPlaneHeight();
            Debug.Log($"Level: {_currentLevel}");
        }
        else if (kb.pageDownKey.wasPressedThisFrame)
        {
            _currentLevel = Mathf.Max(_currentLevel - 1, 0);
            UpdateGroundPlaneHeight();
            Debug.Log($"Level: {_currentLevel}");
        }
    }

    private void CancelAllPending()
    {
        CancelDrag();
        CancelPendingWall();
        CancelPendingRamp();
    }

    private void UpdateGroundPlaneHeight()
    {
        if (_groundPlane != null)
        {
            var pos = _groundPlane.transform.position;
            pos.y = _currentLevel * FactoryGrid.LevelHeight - 0.05f;
            _groundPlane.transform.position = pos;
        }
    }

    // -- Foundation batch placement --

    private void HandleFoundationInput(Mouse mouse)
    {
        var cell = GetCellUnderCursor(mouse);
        if (!cell.HasValue)
        {
            HideGhosts();
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _dragStart = cell.Value;
            _dragEnd = cell.Value;
        }

        if (_isDragging)
        {
            _dragEnd = cell.Value;
            UpdateFoundationGhosts();
        }

        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            PlaceFoundationRectangle();
            _isDragging = false;
            HideGhosts();
        }
    }

    private void PlaceFoundationRectangle()
    {
        var min = Vector2Int.Min(_dragStart, _dragEnd);
        var max = Vector2Int.Max(_dragStart, _dragEnd);

        int placed = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int z = min.y; z <= max.y; z++)
            {
                var cellPos = new Vector2Int(x, z);
                var data = _placementService.PlaceFoundation(_foundationDef, cellPos, _currentLevel);
                if (data != null)
                {
                    _foundations.Add(data);
                    SpawnFoundationVisual(data, cellPos, _currentLevel);
                    placed++;
                }
            }
        }

        if (placed > 0)
            Debug.Log($"Placed {placed} foundations at level {_currentLevel}");
    }

    private void UpdateFoundationGhosts()
    {
        var min = Vector2Int.Min(_dragStart, _dragEnd);
        var max = Vector2Int.Max(_dragStart, _dragEnd);
        int needed = (max.x - min.x + 1) * (max.y - min.y + 1);

        while (_ghostPool.Count < needed)
        {
            var ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ghost.name = "Ghost";
            ghost.transform.localScale = new Vector3(0.95f, 0.1f, 0.95f);
            var collider = ghost.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            ghost.SetActive(false);
            _ghostPool.Add(ghost);
        }

        int idx = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int z = min.y; z <= max.y; z++)
            {
                var cellPos = new Vector2Int(x, z);
                bool valid = _grid.CanPlace(cellPos, Vector2Int.one, _currentLevel);
                var worldPos = _grid.CellToWorld(cellPos, _currentLevel) + Vector3.up * 0.05f;

                var ghost = _ghostPool[idx];
                ghost.SetActive(true);
                ghost.transform.position = worldPos;
                SetColor(ghost, valid ? _ghostValidColor : _ghostInvalidColor);
                idx++;
            }
        }

        for (int i = idx; i < _ghostPool.Count; i++)
            _ghostPool[i].SetActive(false);
    }

    private void HideGhosts()
    {
        for (int i = 0; i < _ghostPool.Count; i++)
            _ghostPool[i].SetActive(false);
    }

    private void CancelDrag()
    {
        _isDragging = false;
        HideGhosts();
    }

    // -- Wall 2-step placement: click foundation cell -> preview -> R rotate -> click confirm --

    private void HandleWallInput(Keyboard kb, Mouse mouse)
    {
        if (!_pendingWall)
        {
            // Step 1: click any foundation cell
            if (mouse.leftButton.wasPressedThisFrame)
            {
                var cell = GetCellUnderCursor(mouse);
                if (!cell.HasValue)
                    return;

                var building = _grid.GetAt(cell.Value, _currentLevel);
                if (building == null || !building.IsStructural)
                {
                    Debug.Log("Walls must be placed on a foundation cell");
                    return;
                }

                _pendingWall = true;
                _pendingWallCell = cell.Value;
                _pendingWallDirIndex = 0;
                UpdatePendingWallPreview();
                Debug.Log($"Wall at ({cell.Value.x},{cell.Value.y}): {DirectionNames[_pendingWallDirIndex]} -- R to rotate, click to confirm, Esc to cancel");
            }
        }
        else
        {
            if (kb.rKey.wasPressedThisFrame)
            {
                _pendingWallDirIndex = (_pendingWallDirIndex + 1) % 4;
                UpdatePendingWallPreview();
                Debug.Log($"Wall direction: {DirectionNames[_pendingWallDirIndex]}");
            }

            if (kb.escapeKey.wasPressedThisFrame)
            {
                CancelPendingWall();
                Debug.Log("Wall placement cancelled");
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                var dir = CardinalDirections[_pendingWallDirIndex];
                var wallData = _placementService.PlaceWall(_wallDef, _pendingWallCell, _currentLevel, dir);
                if (wallData != null)
                {
                    _walls.Add(wallData);
                    SpawnWallVisual(wallData);
                    Debug.Log($"Wall placed at ({_pendingWallCell.x},{_pendingWallCell.y}) edge {DirectionNames[_pendingWallDirIndex]}");
                }
                else
                {
                    Debug.Log($"Cannot place wall at ({_pendingWallCell.x},{_pendingWallCell.y}) edge {DirectionNames[_pendingWallDirIndex]}");
                }
                CancelPendingWall();
            }
        }
    }

    private void UpdatePendingWallPreview()
    {
        if (_pendingWallPreview != null)
            Destroy(_pendingWallPreview);

        if (!_pendingWall)
            return;

        var dir = CardinalDirections[_pendingWallDirIndex];
        var cellCenter = _grid.CellToWorld(_pendingWallCell, _currentLevel);
        var edgeOffset = new Vector3(
            dir.x * 0.5f * FactoryGrid.CellSize, 0f,
            dir.y * 0.5f * FactoryGrid.CellSize);
        var wallPos = cellCenter + edgeOffset + Vector3.up * FactoryGrid.LevelHeight * 0.5f;
        float yRotation = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "WallPreview";
        var collider = wall.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        wall.transform.position = wallPos;
        wall.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        wall.transform.localScale = new Vector3(0.95f, FactoryGrid.LevelHeight, 0.1f);
        SetColor(wall, _ghostValidColor);

        _pendingWallPreview = wall;
    }

    private void CancelPendingWall()
    {
        _pendingWall = false;
        if (_pendingWallPreview != null)
        {
            Destroy(_pendingWallPreview);
            _pendingWallPreview = null;
        }
    }

    // -- Ramp 2-step placement: click foundation cell -> preview -> R rotate -> click confirm --

    private void HandleRampInput(Keyboard kb, Mouse mouse)
    {
        if (!_pendingRamp)
        {
            // Step 1: click any foundation cell to start
            if (mouse.leftButton.wasPressedThisFrame)
            {
                var cell = GetCellUnderCursor(mouse);
                if (!cell.HasValue)
                    return;

                // Must click on a foundation
                var building = _grid.GetAt(cell.Value, _currentLevel);
                if (building == null || !building.IsStructural)
                {
                    Debug.Log("Ramps must start from a foundation cell");
                    return;
                }

                _pendingRamp = true;
                _pendingRampCell = cell.Value;
                _pendingRampDirIndex = 0;
                UpdatePendingRampPreview();
                Debug.Log($"Ramp at ({cell.Value.x},{cell.Value.y}): {DirectionNames[_pendingRampDirIndex]} -- R to rotate, click to confirm, Esc to cancel");
            }
        }
        else
        {
            // Step 2: R to rotate, click to confirm, Escape to cancel
            if (kb.rKey.wasPressedThisFrame)
            {
                _pendingRampDirIndex = (_pendingRampDirIndex + 1) % 4;
                UpdatePendingRampPreview();
                Debug.Log($"Ramp direction: {DirectionNames[_pendingRampDirIndex]}");
            }

            if (kb.escapeKey.wasPressedThisFrame)
            {
                CancelPendingRamp();
                Debug.Log("Ramp placement cancelled");
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                var dir = CardinalDirections[_pendingRampDirIndex];
                var rampData = _placementService.PlaceRamp(_rampDef, _pendingRampCell, _currentLevel, dir);
                if (rampData != null)
                {
                    _ramps.Add(rampData);
                    SpawnRampVisual(rampData);
                    Debug.Log($"Ramp placed: {DirectionNames[_pendingRampDirIndex]} at ({rampData.BaseCell.x},{rampData.BaseCell.y}) level {rampData.BaseLevel}");
                }
                else
                {
                    Debug.Log($"Cannot place ramp {DirectionNames[_pendingRampDirIndex]}: cells blocked or out of bounds");
                }
                CancelPendingRamp();
            }
        }
    }

    private void UpdatePendingRampPreview()
    {
        if (_pendingRampPreview != null)
            Destroy(_pendingRampPreview);

        if (!_pendingRamp)
            return;

        var dir2D = CardinalDirections[_pendingRampDirIndex];
        var rampStart = _pendingRampCell + dir2D;

        // Build preview from edge of source cell to far edge of last ramp cell
        var startPos = SnapPointToWorld(new SnapPoint(_pendingRampCell, _currentLevel, dir2D, SnapPointType.FoundationEdge, null));
        startPos.y = _currentLevel * FactoryGrid.LevelHeight;

        var endPos = startPos
            + new Vector3(dir2D.x, 0f, dir2D.y) * _rampDef.footprintLength * FactoryGrid.CellSize;
        endPos.y = (_currentLevel + 1) * FactoryGrid.LevelHeight;

        var midpoint = (startPos + endPos) * 0.5f;
        var dir3D = (endPos - startPos).normalized;
        var length = Vector3.Distance(startPos, endPos);

        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "RampPreview";
        var collider = ramp.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        ramp.transform.position = midpoint;
        ramp.transform.rotation = Quaternion.LookRotation(dir3D);
        ramp.transform.localScale = new Vector3(0.95f, 0.1f, length);

        // Check validity for color
        bool valid = CanPlaceRampAt(_pendingRampCell, _currentLevel, dir2D);
        SetColor(ramp, valid ? _ghostValidColor : _ghostInvalidColor);

        _pendingRampPreview = ramp;
    }

    private bool CanPlaceRampAt(Vector2Int sourceCell, int level, Vector2Int dir)
    {
        var start = sourceCell + dir;
        for (int i = 0; i < _rampDef.footprintLength; i++)
        {
            var cell = start + dir * i;
            if (!_grid.IsInBounds(cell))
                return false;
            var existing = _grid.GetAt(cell, level);
            if (existing != null && !existing.IsStructural)
                return false;
            foreach (var r in _ramps)
            {
                if (r.BaseLevel == level && r.OccupiedCells.Contains(cell))
                    return false;
            }
        }
        return true;
    }

    private void CancelPendingRamp()
    {
        _pendingRamp = false;
        if (_pendingRampPreview != null)
        {
            Destroy(_pendingRampPreview);
            _pendingRampPreview = null;
        }
    }

    private static string DirName(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return "North";
        if (dir == Vector2Int.right) return "East";
        if (dir == Vector2Int.down) return "South";
        if (dir == Vector2Int.left) return "West";
        return dir.ToString();
    }

    // -- Delete mode --

    private void HandleDeleteInput(Mouse mouse)
    {
        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        var cell = GetCellUnderCursor(mouse);
        if (!cell.HasValue)
            return;

        // Check walls first (they must be removed before foundations)
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            var wall = _walls[i];
            if (wall.Cell == cell.Value && wall.Level == _currentLevel)
            {
                _placementService.RemoveWall(wall);
                if (wall.Instance != null) Destroy(wall.Instance);
                _walls.RemoveAt(i);
                Debug.Log("Wall removed");
                return;
            }
        }

        // Check ramps
        for (int i = _ramps.Count - 1; i >= 0; i--)
        {
            var ramp = _ramps[i];
            if (ramp.OccupiedCells.Contains(cell.Value) && ramp.BaseLevel == _currentLevel)
            {
                _placementService.RemoveRamp(ramp);
                if (ramp.Instance != null) Destroy(ramp.Instance);
                _ramps.RemoveAt(i);
                Debug.Log("Ramp removed");
                return;
            }
        }

        // Check foundations
        for (int i = _foundations.Count - 1; i >= 0; i--)
        {
            var foundation = _foundations[i];
            if (foundation.Level != _currentLevel)
                continue;

            bool inFootprint = cell.Value.x >= foundation.Origin.x
                && cell.Value.x < foundation.Origin.x + foundation.Size.x
                && cell.Value.y >= foundation.Origin.y
                && cell.Value.y < foundation.Origin.y + foundation.Size.y;

            if (!inFootprint)
                continue;

            if (_placementService.RemoveFoundation(foundation))
            {
                if (foundation.Instance != null) Destroy(foundation.Instance);
                _foundations.RemoveAt(i);
                Debug.Log("Foundation removed");
            }
            else
            {
                Debug.Log("Cannot remove foundation: walls still attached");
            }
            return;
        }
    }

    // -- Visual spawning --

    private void SpawnFoundationVisual(BuildingData data, Vector2Int cell, int level)
    {
        var worldPos = _grid.CellToWorld(cell, level);
        var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = $"Foundation_{cell.x}_{cell.y}_L{level}";
        tile.transform.position = worldPos + Vector3.up * 0.05f;
        tile.transform.localScale = new Vector3(0.95f, 0.1f, 0.95f);
        SetColor(tile, Color.white);
        data.Instance = tile;
    }

    private void SpawnWallVisual(WallData wallData)
    {
        var cellCenter = _grid.CellToWorld(wallData.Cell, wallData.Level);
        var edgeDir = wallData.EdgeDirection;
        var edgeOffset = new Vector3(
            edgeDir.x * 0.5f * FactoryGrid.CellSize, 0f,
            edgeDir.y * 0.5f * FactoryGrid.CellSize);
        var wallPos = cellCenter + edgeOffset + Vector3.up * FactoryGrid.LevelHeight * 0.5f;

        float yRotation = Mathf.Atan2(edgeDir.x, edgeDir.y) * Mathf.Rad2Deg;

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Wall_{wallData.Cell.x}_{wallData.Cell.y}_L{wallData.Level}";
        wall.transform.position = wallPos;
        wall.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        wall.transform.localScale = new Vector3(0.95f, FactoryGrid.LevelHeight, 0.1f);
        SetColor(wall, new Color(0.6f, 0.6f, 0.6f));
        wallData.Instance = wall;
    }

    private void SpawnRampVisual(RampData rampData)
    {
        // Use edge-to-edge positioning for correct 45-degree angle and no gap to foundation.
        // Start: the edge between the foundation cell and the first ramp cell (snap point position).
        // End: the far edge of the last ramp cell at the upper level.
        var dir2D = rampData.Direction;
        var snapCell = rampData.BaseCell - dir2D; // the foundation cell the ramp attached to
        var startPos = SnapPointToWorld(new SnapPoint(snapCell, rampData.BaseLevel, dir2D, SnapPointType.FoundationEdge, null));
        startPos.y = rampData.BaseLevel * FactoryGrid.LevelHeight;

        var endPos = startPos
            + new Vector3(dir2D.x, 0f, dir2D.y) * rampData.FootprintLength * FactoryGrid.CellSize;
        endPos.y = (rampData.BaseLevel + 1) * FactoryGrid.LevelHeight;

        var midpoint = (startPos + endPos) * 0.5f;
        var dir3D = (endPos - startPos).normalized;
        var length = Vector3.Distance(startPos, endPos);

        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = $"Ramp_{rampData.BaseCell.x}_{rampData.BaseCell.y}_L{rampData.BaseLevel}";
        ramp.transform.position = midpoint;
        ramp.transform.rotation = Quaternion.LookRotation(dir3D);
        ramp.transform.localScale = new Vector3(0.95f, 0.1f, length);
        SetColor(ramp, new Color(0.76f, 0.6f, 0.42f));
        rampData.Instance = ramp;
    }

    // -- Helpers --

    private Vector2Int? GetCellUnderCursor(Mouse mouse)
    {
        var worldPos = GetWorldPosUnderCursor(mouse);
        if (!worldPos.HasValue)
            return null;
        return _grid.WorldToCell(worldPos.Value);
    }

    private Vector3? GetWorldPosUnderCursor(Mouse mouse)
    {
        var camera = Camera.main;
        if (camera == null)
            return null;

        var mousePos = mouse.position.ReadValue();
        var ray = camera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, PhysicsLayers.PlacementMask))
            return hit.point;

        return null;
    }

    private Vector3 SnapPointToWorld(SnapPoint sp)
    {
        var cellCenter = _grid.CellToWorld(sp.Cell, sp.Level);
        var edgeOffset = new Vector3(
            sp.EdgeDirection.x * 0.5f * FactoryGrid.CellSize, 0f,
            sp.EdgeDirection.y * 0.5f * FactoryGrid.CellSize);
        return cellCenter + edgeOffset;
    }

    private static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    private static void DrawLine(ref float y, float x, float w, float h, string text, bool bold = false)
    {
        var style = GUI.skin.label;
        if (bold)
            style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
        GUI.Label(new Rect(x, y, w, h), text, style);
        y += h;
    }
}
