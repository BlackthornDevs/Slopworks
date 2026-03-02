using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime bootstrapper for Dev_Test scene HUD components.
/// Initializes InventoryUI after PlayerHUD.Start() has run.
/// Add to the HUD_Canvas alongside PlayerHUD. Wire _playerInventory
/// at editor time via PlaytestSetup.cs (no GameObject.Find at runtime).
/// </summary>
public class DevTestHUDBootstrap : MonoBehaviour
{
    [SerializeField] private PlayerInventory _playerInventory;

    private IEnumerator Start()
    {
        // Wait one frame for PlayerHUD.Start() and PlayerInventory.Awake()
        yield return null;

        var inventoryUI = GetComponent<InventoryUI>();
        if (inventoryUI == null)
        {
            Debug.Log("dev test hud: no InventoryUI found, skipping");
            yield break;
        }

        if (_playerInventory != null)
        {
            inventoryUI.Initialize(_playerInventory);
            Debug.Log("dev test hud: inventory ui initialized");
        }
        else
        {
            Debug.LogWarning("dev test hud: _playerInventory not wired (run Slopworks > Setup Playtest Scene)");
        }
    }
}
