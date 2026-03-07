using UnityEngine;
using UnityEditor;

/// <summary>
/// Spawns a TerrainExplorer player into the active scene.
/// Run via Slopworks > Spawn Terrain Explorer.
/// Uses string-based AddComponent to avoid cross-assembly reference to Slopworks.Runtime.
/// </summary>
public static class SpawnTerrainExplorer
{
    [MenuItem("Slopworks/Spawn Terrain Explorer")]
    public static void Spawn()
    {
        // remove any existing explorer
        var explorerType = System.Type.GetType("TerrainExplorer, Slopworks.Runtime");
        if (explorerType == null)
            explorerType = System.Type.GetType("TerrainExplorer");

        if (explorerType != null)
        {
            var existing = Object.FindObjectOfType(explorerType) as Component;
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log("removed existing terrain explorer");
            }
        }

        var go = new GameObject("TerrainExplorer");

        if (explorerType != null)
        {
            go.AddComponent(explorerType);
        }
        else
        {
            Debug.LogError("TerrainExplorer type not found — is TerrainExplorer.cs compiled?");
            Object.DestroyImmediate(go);
            return;
        }

        // find terrain to place player on it
        var terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            var td = terrain.terrainData;
            var terrainPos = terrain.transform.position;
            float cx = terrainPos.x + td.size.x * 0.5f;
            float cz = terrainPos.z + td.size.z * 0.5f;
            float y = terrain.SampleHeight(new Vector3(cx, 0f, cz)) + terrainPos.y + 2f;
            go.transform.position = new Vector3(cx, y, cz);
            Debug.Log($"terrain explorer spawned at ({cx:F1}, {y:F1}, {cz:F1})");
        }
        else
        {
            go.transform.position = new Vector3(0f, 5f, 0f);
            Debug.Log("terrain explorer spawned at (0, 5, 0) — no terrain found");
        }

        // disable any other cameras so they don't conflict
        foreach (var cam in Object.FindObjectsOfType<Camera>())
        {
            if (cam.transform.parent != go.transform)
            {
                cam.enabled = false;
                Debug.Log($"disabled camera: {cam.gameObject.name}");
            }
        }

        Selection.activeGameObject = go;
        EditorUtility.SetDirty(go);

        Debug.Log("terrain explorer ready — hit Play to walk around");
    }
}
