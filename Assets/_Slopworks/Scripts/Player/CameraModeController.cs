using UnityEngine;
using UnityEngine.InputSystem;

public class CameraModeController : MonoBehaviour
{
    [SerializeField] private Camera _fpsCamera;
    [SerializeField] private Camera _isometricCamera;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InteractionController _interactionController;

    private SlopworksControls _controls;
    private bool _isFPS;

    private void Awake()
    {
        _controls = new SlopworksControls();
    }

    private void OnEnable()
    {
        _controls.Combat.SwitchIsometric.performed += OnSwitchToIsometric;
        _controls.Command.SwitchFPS.performed += OnSwitchToFPS;

        // start in FPS mode
        SwitchToFPS();
    }

    private void OnDisable()
    {
        _controls.Combat.SwitchIsometric.performed -= OnSwitchToIsometric;
        _controls.Command.SwitchFPS.performed -= OnSwitchToFPS;

        _controls.Combat.Disable();
        _controls.Command.Disable();
    }

    private void OnSwitchToIsometric(InputAction.CallbackContext ctx)
    {
        SwitchToIsometric();
    }

    private void OnSwitchToFPS(InputAction.CallbackContext ctx)
    {
        SwitchToFPS();
    }

    private void SwitchToFPS()
    {
        _isFPS = true;

        _controls.Command.Disable();
        _controls.Combat.Enable();

        if (_fpsCamera != null) _fpsCamera.gameObject.SetActive(true);
        if (_isometricCamera != null) _isometricCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_playerController != null) _playerController.enabled = true;
        if (_interactionController != null) _interactionController.enabled = true;
    }

    private void SwitchToIsometric()
    {
        _isFPS = false;

        _controls.Combat.Disable();
        _controls.Command.Enable();

        if (_fpsCamera != null) _fpsCamera.gameObject.SetActive(false);
        if (_isometricCamera != null) _isometricCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_playerController != null) _playerController.enabled = false;
        if (_interactionController != null) _interactionController.enabled = false;
    }
}
