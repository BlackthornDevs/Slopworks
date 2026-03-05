using System;
using UnityEngine;

/// <summary>
/// Interactable elevator terminal placed at each tower floor's elevator position.
/// Opens TowerElevatorUI on interact, allowing floor selection and extraction.
/// </summary>
public class TowerElevatorBehaviour : MonoBehaviour, IInteractable
{
    private TowerElevatorUI _elevatorUI;
    private TowerController _towerController;
    private PlayerInventory _playerInventory;
    private Action<int> _onFloorSelected;
    private Action _onExtract;

    public void Initialize(
        TowerElevatorUI elevatorUI, TowerController towerController,
        PlayerInventory playerInventory,
        Action<int> onFloorSelected, Action onExtract)
    {
        _elevatorUI = elevatorUI;
        _towerController = towerController;
        _playerInventory = playerInventory;
        _onFloorSelected = onFloorSelected;
        _onExtract = onExtract;
    }

    public string GetInteractionPrompt()
    {
        if (_elevatorUI == null)
            return "";

        return "press E to use elevator";
    }

    public void Interact(GameObject player)
    {
        if (_elevatorUI == null || _towerController == null)
            return;

        if (_elevatorUI.IsOpen)
            return;

        _elevatorUI.Open(_towerController, _playerInventory, _onFloorSelected, _onExtract);
    }
}
