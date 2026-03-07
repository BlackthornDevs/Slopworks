using UnityEngine;
using UnityEditor;

/// <summary>
/// Quick utility to check the bounding box sizes of imported Kenney models.
/// Run via Slopworks > Check Kenney Model Sizes.
/// </summary>
public static class KenneyModelSizeCheck
{
    [MenuItem("Slopworks/Check Kenney Model Sizes")]
    public static void CheckSizes()
    {
        string[] modelsToCheck = new[]
        {
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/floor.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/floor-large.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/structure-wall.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/structure-tall.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/structure-doorway.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/structure-window.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/cover.fbx",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/conveyor.fbx",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/barrel.fbx",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/box.fbx",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/chest.fbx",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/fence.fbx",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/workbench.fbx",
        };

        foreach (var path in modelsToCheck)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.Log($"model not found: {path}");
                continue;
            }

            var instance = Object.Instantiate(prefab);
            var renderers = instance.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.Log($"{System.IO.Path.GetFileNameWithoutExtension(path)}: no renderers found");
                Object.DestroyImmediate(instance);
                continue;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Debug.Log($"{System.IO.Path.GetFileNameWithoutExtension(path)}: " +
                      $"size=({bounds.size.x:F2}, {bounds.size.y:F2}, {bounds.size.z:F2}) " +
                      $"center=({bounds.center.x:F2}, {bounds.center.y:F2}, {bounds.center.z:F2})");

            Object.DestroyImmediate(instance);
        }

        Debug.Log("kenney model size check complete");
    }
}
