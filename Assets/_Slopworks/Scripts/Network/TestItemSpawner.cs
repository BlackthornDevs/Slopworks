using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestItemSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _worldItemPrefab;
    [SerializeField] private int _itemCount = 5;
    [SerializeField] private Vector3 _spawnCenter = new(50f, 0.5f, 50f);
    [SerializeField] private float _spawnRadius = 5f;

    [Header("TEST: Fill Storage (T key)")]
    [SerializeField] private string _testFillItemId = "iron_scrap";
    [SerializeField] private int _testFillCount = 20;
    [SerializeField] private string _testRecipeId = "smelt_iron";

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnTestItems();
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            TestFillStorageAndRecipes();
    }

    private void SpawnTestItems()
    {
        string[] itemIds = { "iron_scrap", "iron_ingot", "copper_scrap" };

        for (int i = 0; i < _itemCount; i++)
        {
            float angle = i * (360f / _itemCount);
            Vector3 pos = _spawnCenter + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * _spawnRadius,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * _spawnRadius
            );

            var go = Instantiate(_worldItemPrefab, pos, Quaternion.identity);
            var worldItem = go.GetComponent<NetworkWorldItem>();
            worldItem.Setup(itemIds[i % itemIds.Length], Random.Range(1, 5));
            ServerManager.Spawn(go);

            Debug.Log($"spawner: {worldItem.ItemId} x{worldItem.Count} at {pos}");
        }
    }

    private void TestFillStorageAndRecipes()
    {
        int storageCount = 0;
        int machineCount = 0;

        var storages = FindObjectsByType<NetworkStorage>(FindObjectsSortMode.None);
        foreach (var storage in storages)
        {
            if (storage.Container == null) continue;
            storage.Container.TryInsertStack(_testFillItemId, _testFillCount);
            storageCount++;
        }

        var machines = FindObjectsByType<NetworkMachine>(FindObjectsSortMode.None);
        foreach (var machine in machines)
        {
            if (string.IsNullOrEmpty(machine.ActiveRecipeId))
            {
                machine.CmdSetRecipe(_testRecipeId);
                machineCount++;
            }
        }

        Debug.Log($"TEST: filled {storageCount} storage with {_testFillItemId} x{_testFillCount}, set recipe on {machineCount} machines to {_testRecipeId}");
    }
}
