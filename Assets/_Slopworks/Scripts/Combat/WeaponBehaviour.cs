using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBehaviour : MonoBehaviour
{
    [SerializeField] private WeaponDefinitionSO _weaponDefinition;
    [SerializeField] private Camera _camera;

    private SlopworksControls _controls;
    private WeaponController _weapon;

    public WeaponController Weapon => _weapon;

    private void Awake()
    {
        _controls = new SlopworksControls();
        _weapon = new WeaponController(_weaponDefinition);
    }

    private void OnEnable()
    {
        _controls.Exploration.Enable();
        _controls.Exploration.Fire.performed += OnFire;
        _controls.Exploration.Reload.performed += OnReload;
    }

    private void OnDisable()
    {
        _controls.Exploration.Fire.performed -= OnFire;
        _controls.Exploration.Reload.performed -= OnReload;
        _controls.Exploration.Disable();
    }

    private void Update()
    {
        _weapon.Tick(Time.deltaTime);
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (_camera == null)
            return;

        if (!_weapon.TryFire())
            return;

        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, _weapon.Range, PhysicsLayers.WeaponHitMask))
        {
            var health = hit.collider.GetComponent<HealthBehaviour>();
            if (health != null)
                health.Health.TakeDamage(_weapon.BuildDamageData(gameObject.name));
        }
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        _weapon.Reload();
    }
}
