using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Master HUD controller. Creates all HUD elements at runtime on the Canvas.
/// Wires to player systems (health, inventory, interaction).
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private HealthBehaviour _playerHealth;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private Camera _playerCamera;

    private HealthBarUI _healthBar;
    private InteractionPromptUI _interactionPrompt;
    private Image _crosshairImage;
    private TextMeshProUGUI _buildModeText;
    private TextMeshProUGUI _waveWarningText;
    private HotbarSlotUI[] _hotbarSlots;

    private void Start()
    {
        CreateCrosshair();
        CreateHealthBar();
        CreateInteractionPrompt();
        CreateBuildModeIndicator();
        CreateWaveWarning();
        CreateHotbar();
        WireReferences();
    }

    public void Initialize(HealthBehaviour health, PlayerInventory inventory, Camera cam)
    {
        _playerHealth = health;
        _playerInventory = inventory;
        _playerCamera = cam;
        WireReferences();
    }

    private void WireReferences()
    {
        if (_playerHealth != null)
            _healthBar?.Initialize(_playerHealth.Health);

        if (_playerCamera != null && _interactionPrompt != null)
        {
            var promptText = _interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            _interactionPrompt.Setup(promptText, _playerCamera);
        }

        if (_playerInventory != null && _hotbarSlots != null)
        {
            for (int i = 0; i < _hotbarSlots.Length; i++)
                _hotbarSlots[i].Bind(_playerInventory, i);
        }
    }

    private void Update()
    {
        _healthBar?.UpdateDisplay();
        UpdateHotbarSelection();
    }

    public void ShowWaveWarning(string message)
    {
        if (_waveWarningText != null)
        {
            _waveWarningText.text = message;
            _waveWarningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWaveWarning));
            Invoke(nameof(HideWaveWarning), 3f);
        }
    }

    private void HideWaveWarning()
    {
        if (_waveWarningText != null)
            _waveWarningText.gameObject.SetActive(false);
    }

    private void UpdateHotbarSelection()
    {
        if (_playerInventory == null || _hotbarSlots == null) return;
        for (int i = 0; i < _hotbarSlots.Length; i++)
            _hotbarSlots[i].SetSelected(i == _playerInventory.SelectedHotbarIndex);
    }

    private void CreateCrosshair()
    {
        var obj = new GameObject("Crosshair");
        obj.transform.SetParent(transform, false);
        _crosshairImage = obj.AddComponent<Image>();
        _crosshairImage.color = new Color(1f, 1f, 1f, 0.7f);
        _crosshairImage.raycastTarget = false;
        var rect = _crosshairImage.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(4, 4);
        rect.anchoredPosition = Vector2.zero;
    }

    private void CreateHealthBar()
    {
        // Background
        var bgObj = new GameObject("HealthBarBG");
        bgObj.transform.SetParent(transform, false);
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgImage.raycastTarget = false;
        var bgRect = bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.sizeDelta = new Vector2(200, 24);
        bgRect.anchoredPosition = new Vector2(16, -16);

        // Fill
        var fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        var fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;
        var fillRect = fillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);

        // Text
        var textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bgObj.transform, false);
        var healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.fontSize = 14;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;
        healthText.raycastTarget = false;
        var textRect = healthText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _healthBar = bgObj.AddComponent<HealthBarUI>();
        _healthBar.Setup(fillImage, healthText);
    }

    private void CreateInteractionPrompt()
    {
        var obj = new GameObject("InteractionPrompt");
        obj.transform.SetParent(transform, false);

        var text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(400, 30);
        rect.anchoredPosition = new Vector2(0, -30);

        obj.SetActive(false);

        _interactionPrompt = obj.AddComponent<InteractionPromptUI>();
        _interactionPrompt.Setup(text, _playerCamera);
    }

    private void CreateBuildModeIndicator()
    {
        var obj = new GameObject("BuildModeIndicator");
        obj.transform.SetParent(transform, false);

        _buildModeText = obj.AddComponent<TextMeshProUGUI>();
        _buildModeText.fontSize = 18;
        _buildModeText.alignment = TextAlignmentOptions.Center;
        _buildModeText.color = new Color(1f, 0.9f, 0.3f);
        _buildModeText.text = "BUILD MODE";
        _buildModeText.raycastTarget = false;

        var rect = _buildModeText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(200, 30);
        rect.anchoredPosition = new Vector2(0, -16);

        obj.SetActive(false);
    }

    private void CreateWaveWarning()
    {
        var obj = new GameObject("WaveWarning");
        obj.transform.SetParent(transform, false);

        _waveWarningText = obj.AddComponent<TextMeshProUGUI>();
        _waveWarningText.fontSize = 28;
        _waveWarningText.alignment = TextAlignmentOptions.Center;
        _waveWarningText.color = new Color(1f, 0.3f, 0.3f);
        _waveWarningText.raycastTarget = false;

        var rect = _waveWarningText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(600, 40);
        rect.anchoredPosition = new Vector2(0, -60);

        obj.SetActive(false);
    }

    private void CreateHotbar()
    {
        var containerObj = new GameObject("HotbarContainer");
        containerObj.transform.SetParent(transform, false);

        var containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.sizeDelta = new Vector2(PlayerInventory.HotbarSlots * 56, 56);
        containerRect.anchoredPosition = new Vector2(0, 16);

        var layout = containerObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        _hotbarSlots = new HotbarSlotUI[PlayerInventory.HotbarSlots];
        for (int i = 0; i < PlayerInventory.HotbarSlots; i++)
            _hotbarSlots[i] = HotbarSlotUI.Create(containerObj.transform, i);
    }
}
