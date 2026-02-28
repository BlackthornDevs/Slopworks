using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

public static class PlaytestSetup
{
    [MenuItem("Slopworks/Setup Playtest Scene")]
    public static void SetupPlaytest()
    {
        FixGround();
        var weaponDef = CreateWeaponDefinition();
        var faunaDef = CreateFaunaDefinition();
        var enemyDiedEvent = CreateEnemyDiedEvent();
        WirePlayerWeapon(weaponDef);
        WirePlayerHealth();
        WireEnemyDefinition(faunaDef, enemyDiedEvent);
        BakeNavMesh();

        Debug.Log("playtest setup complete");
    }

    private static WeaponDefinitionSO CreateWeaponDefinition()
    {
        string folder = "Assets/_Slopworks/ScriptableObjects/Weapons";
        EnsureFolder(folder);

        string path = folder + "/Test_Rifle.asset";
        var existing = AssetDatabase.LoadAssetAtPath<WeaponDefinitionSO>(path);
        if (existing != null)
        {
            Debug.Log("weapon definition already exists at " + path);
            return existing;
        }

        var def = ScriptableObject.CreateInstance<WeaponDefinitionSO>();
        def.weaponId = "test_rifle";
        def.damage = 25f;
        def.fireRate = 2f;
        def.range = 50f;
        def.damageType = DamageType.Kinetic;
        def.magazineSize = 12;
        def.reloadTime = 1.5f;

        AssetDatabase.CreateAsset(def, path);
        Debug.Log("created weapon definition at " + path);
        return def;
    }

    private static FaunaDefinitionSO CreateFaunaDefinition()
    {
        string folder = "Assets/_Slopworks/ScriptableObjects/Fauna";
        EnsureFolder(folder);

        string path = folder + "/Test_Grunt.asset";
        var existing = AssetDatabase.LoadAssetAtPath<FaunaDefinitionSO>(path);
        if (existing != null)
        {
            Debug.Log("fauna definition already exists at " + path);
            return existing;
        }

        var def = ScriptableObject.CreateInstance<FaunaDefinitionSO>();
        def.faunaId = "test_grunt";
        def.maxHealth = 50f;
        def.moveSpeed = 3f;
        def.attackDamage = 10f;
        def.attackRange = 2.5f;
        def.attackCooldown = 1.5f;
        def.sightRange = 15f;
        def.sightAngle = 120f;
        def.hearingRange = 8f;
        def.attackDamageType = DamageType.Kinetic;

        AssetDatabase.CreateAsset(def, path);
        Debug.Log("created fauna definition at " + path);
        return def;
    }

    private static void WirePlayerWeapon(WeaponDefinitionSO weaponDef)
    {
        var player = GameObject.Find("PlayerCharacter");
        if (player == null)
        {
            Debug.LogError("PlayerCharacter not found in scene");
            return;
        }

        // add HealthBehaviour if missing (for enemy attacks)
        var weapon = player.GetComponent<WeaponBehaviour>();
        if (weapon == null)
            weapon = player.AddComponent<WeaponBehaviour>();

        var fpsCam = player.transform.Find("FPSCamera");
        if (fpsCam == null)
        {
            Debug.LogError("FPSCamera not found on player");
            return;
        }

        var so = new SerializedObject(weapon);
        so.FindProperty("_weaponDefinition").objectReferenceValue = weaponDef;
        so.FindProperty("_camera").objectReferenceValue = fpsCam.GetComponent<Camera>();
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(player);
        Debug.Log("player weapon wired");
    }

    private static void WirePlayerHealth()
    {
        var player = GameObject.Find("PlayerCharacter");
        if (player == null) return;

        var health = player.GetComponent<HealthBehaviour>();
        if (health == null)
        {
            health = player.AddComponent<HealthBehaviour>();
            var so = new SerializedObject(health);
            so.FindProperty("_maxHealth").floatValue = 100f;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(player);
        Debug.Log("player health wired");
    }

    private static GameEventSO CreateEnemyDiedEvent()
    {
        string folder = "Assets/_Slopworks/ScriptableObjects/Events";
        EnsureFolder(folder);

        string path = folder + "/EnemyDied.asset";
        var existing = AssetDatabase.LoadAssetAtPath<GameEventSO>(path);
        if (existing != null)
        {
            Debug.Log("enemy died event already exists at " + path);
            return existing;
        }

        var evt = ScriptableObject.CreateInstance<GameEventSO>();
        AssetDatabase.CreateAsset(evt, path);
        Debug.Log("created enemy died event at " + path);
        return evt;
    }

    private static void WireEnemyDefinition(FaunaDefinitionSO faunaDef, GameEventSO enemyDiedEvent)
    {
        var enemy = GameObject.Find("Enemy_Basic");
        if (enemy == null)
        {
            Debug.LogWarning("Enemy_Basic not found in scene — skipping fauna wiring");
            return;
        }

        var controller = enemy.GetComponent<FaunaController>();
        if (controller == null)
        {
            Debug.LogError("FaunaController not found on Enemy_Basic");
            return;
        }

        var so = new SerializedObject(controller);
        so.FindProperty("_def").objectReferenceValue = faunaDef;
        so.FindProperty("_onDeathEvent").objectReferenceValue = enemyDiedEvent;
        so.ApplyModifiedProperties();

        // also wire HealthBehaviour max health to match definition
        var health = enemy.GetComponent<HealthBehaviour>();
        if (health != null)
        {
            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("_maxHealth").floatValue = faunaDef.maxHealth;
            healthSo.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(enemy);
        Debug.Log("enemy fauna definition wired");
    }

    private static void FixGround()
    {
        var existing = GameObject.Find("Ground");
        if (existing != null)
        {
            var mf = existing.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Debug.Log("ground already has valid mesh");
                return;
            }
            Object.DestroyImmediate(existing);
            Debug.Log("deleted broken ground object");
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.layer = PhysicsLayers.Terrain;
        ground.isStatic = true;
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        EditorUtility.SetDirty(ground);
        Debug.Log("created ground plane (100x100, layer Terrain, static)");
    }

    private static void BakeNavMesh()
    {
        NavMeshBuilder.BuildNavMesh();
        Debug.Log("navmesh baked");
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
