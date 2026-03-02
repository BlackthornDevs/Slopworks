using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime bootstrapper for Dev_Test scene HUD components.
/// Initializes InventoryUI after PlayerHUD.Start() has run.
/// Add to the HUD_Canvas alongside PlayerHUD.
/// </summary>
public class DevTestHUDBootstrap : MonoBehaviour
{
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

        var player = GameObject.Find("PlayerCharacter");
        if (player == null)
        {
            Debug.LogWarning("dev test hud: PlayerCharacter not found");
            yield break;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventoryUI.Initialize(inventory);
            Debug.Log("dev test hud: inventory ui initialized");
        }
        else
        {
            Debug.LogWarning("dev test hud: PlayerInventory not found on player");
        }
    }
}
