using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkBuildController : NetworkBehaviour
{
    [SerializeField] private float _placementRange = 50f;

    private Camera _camera;
    private bool _buildMode;
    private GameObject _ghost;
    private Vector2Int _lastGhostCell;
    private int _lastGhostLevel;
    private bool _lastGhostValid;

    private static readonly int StructuralMask =
        (1 << PhysicsLayers.Terrain) | (1 << PhysicsLayers.Structures);

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            _buildMode = !_buildMode;
            Debug.Log($"build: mode {(_buildMode ? "ON" : "OFF")}");

            if (!_buildMode && _ghost != null)
            {
                Destroy(_ghost);
                _ghost = null;
            }
        }

        if (!_buildMode) return;

        UpdateGhostPreview();

        if (Mouse.current.leftButton.wasPressedThisFrame && _lastGhostValid)
        {
            GridManager.Instance.CmdPlaceFoundation(_lastGhostCell, _lastGhostLevel);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            GridManager.Instance.CmdRemoveFoundation(_lastGhostCell, _lastGhostLevel);
        }
    }

    private void UpdateGhostPreview()
    {
        var ray = new Ray(_camera.transform.position, _camera.transform.forward);

        if (!Physics.Raycast(ray, out var hit, _placementRange, StructuralMask))
        {
            if (_ghost != null) _ghost.SetActive(false);
            _lastGhostValid = false;
            return;
        }

        var grid = GridManager.Instance.Grid;
        var cell = grid.WorldToCell(hit.point);
        int level = Mathf.RoundToInt(hit.point.y / FactoryGrid.LevelHeight);
        level = Mathf.Clamp(level, 0, FactoryGrid.MaxLevels - 1);

        _lastGhostCell = cell;
        _lastGhostLevel = level;

        bool occupied = grid.GetAt(cell, level) != null;
        bool canPlace = !occupied;
        _lastGhostValid = canPlace;

        if (_ghost == null)
        {
            _ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _ghost.transform.localScale = new Vector3(
                FactoryGrid.CellSize, 0.1f, FactoryGrid.CellSize);
            _ghost.GetComponent<Collider>().enabled = false;
            _ghost.layer = PhysicsLayers.Decal;

            var renderer = _ghost.GetComponent<Renderer>();
            var mat = new Material(renderer.sharedMaterial);
            mat.color = new Color(0f, 1f, 0f, 0.5f);
            renderer.sharedMaterial = mat;
        }

        Vector3 worldPos = grid.CellToWorld(cell, level);
        _ghost.transform.position = worldPos;
        _ghost.SetActive(true);

        var ghostRenderer = _ghost.GetComponent<Renderer>();
        if (canPlace)
            ghostRenderer.sharedMaterial.color = new Color(0f, 1f, 0f, 0.5f);
        else if (occupied)
            ghostRenderer.sharedMaterial.color = new Color(1f, 0.3f, 0f, 0.5f);
        else
            ghostRenderer.sharedMaterial.color = new Color(1f, 0f, 0f, 0.5f);
    }

    private void OnGUI()
    {
        if (!IsOwner) return;

        // Crosshair (always visible)
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        float size = 12f;
        float thickness = 2f;
        GUI.DrawTexture(new Rect(cx - size, cy - thickness / 2, size * 2, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - thickness / 2, cy - size, thickness, size * 2), Texture2D.whiteTexture);

        if (!_buildMode) return;

        GUILayout.BeginArea(new Rect(10, 50, 250, 80));
        GUILayout.Label("BUILD MODE (B to toggle)");
        GUILayout.Label($"Cell: ({_lastGhostCell.x}, {_lastGhostCell.y}) Level: {_lastGhostLevel}");
        GUILayout.Label("LMB: Place  |  RMB: Remove");
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (_ghost != null)
            Destroy(_ghost);
    }
}
