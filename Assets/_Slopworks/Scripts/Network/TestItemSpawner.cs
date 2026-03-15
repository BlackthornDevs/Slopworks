using FishNet.Object;
using UnityEngine;

public class TestItemSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _worldItemPrefab;
    [SerializeField] private int _itemCount = 5;
    [SerializeField] private Vector3 _spawnCenter = new(50f, 0.5f, 50f);
    [SerializeField] private float _spawnRadius = 5f;

    [Header("Auto-fill Storage")]
    [SerializeField] private bool _autoFillStorage = true;
    [SerializeField] private string _autoFillItemId = "iron_scrap";
    [SerializeField] private int _autoFillCount = 20;
    [SerializeField] private string _defaultRecipeId = "smelt_iron";

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnTestItems();

        if (_autoFillStorage)
            Invoke(nameof(AutoFillStorageAndRecipes), 1f);
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

    private void AutoFillStorageAndRecipes()
    {
        var storages = FindObjectsByType<NetworkStorage>(FindObjectsSortMode.None);
        foreach (var storage in storages)
        {
            if (storage.Container == null) continue;
            storage.Container.TryInsertStack(_autoFillItemId, _autoFillCount);
            Debug.Log($"spawner: auto-filled {storage.name} with {_autoFillItemId} x{_autoFillCount}");
        }

        var machines = FindObjectsByType<NetworkMachine>(FindObjectsSortMode.None);
        foreach (var machine in machines)
        {
            if (string.IsNullOrEmpty(machine.ActiveRecipeId))
            {
                machine.CmdSetRecipe(_defaultRecipeId);
                Debug.Log($"spawner: set {machine.name} recipe to {_defaultRecipeId}");
            }
        }
    }
}
