using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Creates URP Lit materials for each Kenney kit using their color palette textures,
/// then assigns them to all Kenney prefab instances in the scene.
/// Run via Slopworks > Apply Kenney Materials.
/// </summary>
public static class KenneyMaterialSetup
{
    private const string MaterialFolder = "Assets/_Slopworks/Materials/Kenney";
    private const string ShaderName = "Universal Render Pipeline/Lit";

    private struct KitInfo
    {
        public string Name;
        public string TexturePath;
        public string ModelFolder;
        public float Smoothness;
        public float Metallic;

        public KitInfo(string name, string texPath, string modelFolder, float smoothness = 0.3f, float metallic = 0f)
        {
            Name = name; TexturePath = texPath; ModelFolder = modelFolder;
            Smoothness = smoothness; Metallic = metallic;
        }
    }

    private static readonly KitInfo[] Kits = {
        new("Survival",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Textures/variation-a.png",
            "Assets/_Slopworks/Art/Kenney/survival-kit/Models/",
            0.2f, 0f),
        new("Conveyor",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Textures/variation-a.png",
            "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/",
            0.4f, 0.3f),  // industrial metal — slightly metallic and smoother
        new("Blaster",
            "Assets/_Slopworks/Art/Kenney/blaster-kit/Textures/variation-a.png",
            "Assets/_Slopworks/Art/Kenney/blaster-kit/Models/",
            0.5f, 0.4f),
        new("TowerDefense",
            "Assets/_Slopworks/Art/Kenney/tower-defense-kit/Textures/variation-a.png",
            "Assets/_Slopworks/Art/Kenney/tower-defense-kit/Models/",
            0.3f, 0.1f),
    };

    [MenuItem("Slopworks/Apply Kenney Materials")]
    public static void Apply()
    {
        EnsureFolder(MaterialFolder);

        // create materials
        var kitMaterials = new Dictionary<string, Material>();
        foreach (var kit in Kits)
        {
            var mat = CreateOrUpdateMaterial(kit);
            if (mat != null)
                kitMaterials[kit.ModelFolder] = mat;
        }

        // find all renderers in scene and assign materials
        int assigned = 0;
        var allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var renderer in allRenderers)
        {
            // check if this is a Kenney prefab instance
            var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(renderer.gameObject);
            if (prefabSource == null)
            {
                // might be nested — check parent
                var rootPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(renderer.gameObject);
                if (rootPrefab != null)
                    prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(rootPrefab);
            }

            if (prefabSource == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(prefabSource);
            if (string.IsNullOrEmpty(assetPath)) continue;

            // find which kit this belongs to
            foreach (var kvp in kitMaterials)
            {
                if (assetPath.StartsWith(kvp.Key))
                {
                    var mats = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = kvp.Value;
                    renderer.sharedMaterials = mats;
                    assigned++;
                    break;
                }
            }
        }

        // also handle non-prefab mesh renderers that might be under HomeBaseScenery
        // (in case some were instantiated differently)
        var scenery = GameObject.Find("HomeBaseScenery");
        if (scenery != null)
        {
            AssignByHierarchyName(scenery.transform, kitMaterials, ref assigned);
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"kenney materials applied to {assigned} renderers");
    }

    private static void AssignByHierarchyName(Transform root, Dictionary<string, Material> kitMaterials, ref int count)
    {
        foreach (Transform child in root)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // check if already assigned a kenney material
                bool alreadyAssigned = false;
                foreach (var kvp in kitMaterials)
                {
                    if (renderer.sharedMaterial == kvp.Value)
                    {
                        alreadyAssigned = true;
                        break;
                    }
                }
                if (alreadyAssigned)
                {
                    AssignByHierarchyName(child, kitMaterials, ref count);
                    continue;
                }
            }

            // try matching by prefab source
            var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(child.gameObject);
            if (prefabRoot != null)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
                if (source != null)
                {
                    string path = AssetDatabase.GetAssetPath(source);
                    foreach (var kvp in kitMaterials)
                    {
                        if (path.StartsWith(kvp.Key))
                        {
                            var renderers = child.GetComponentsInChildren<Renderer>();
                            foreach (var r in renderers)
                            {
                                var mats = new Material[r.sharedMaterials.Length];
                                for (int i = 0; i < mats.Length; i++)
                                    mats[i] = kvp.Value;
                                r.sharedMaterials = mats;
                                count++;
                            }
                            break;
                        }
                    }
                }
            }

            AssignByHierarchyName(child, kitMaterials, ref count);
        }
    }

    private static Material CreateOrUpdateMaterial(KitInfo kit)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(kit.TexturePath);
        if (texture == null)
        {
            Debug.LogWarning($"texture not found for {kit.Name}: {kit.TexturePath}");
            return null;
        }

        // ensure texture is sRGB and point-filtered (color palette, not photographic)
        string texPath = AssetDatabase.GetAssetPath(texture);
        var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (texImporter != null)
        {
            bool changed = false;
            if (texImporter.filterMode != FilterMode.Point)
            {
                texImporter.filterMode = FilterMode.Point;
                changed = true;
            }
            if (!texImporter.sRGBTexture)
            {
                texImporter.sRGBTexture = true;
                changed = true;
            }
            if (changed)
                texImporter.SaveAndReimport();
        }

        string matPath = $"{MaterialFolder}/Kenney_{kit.Name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        var shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogError($"shader not found: {ShaderName}");
            return null;
        }

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        }
        else
        {
            mat.shader = shader;
        }

        // set properties
        mat.SetTexture("_BaseMap", texture);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Smoothness", kit.Smoothness);
        mat.SetFloat("_Metallic", kit.Metallic);

        // enable GPU instancing for batching all the scattered props
        mat.enableInstancing = true;

        EditorUtility.SetDirty(mat);
        Debug.Log($"material created: {matPath} (smoothness={kit.Smoothness}, metallic={kit.Metallic})");

        return mat;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
