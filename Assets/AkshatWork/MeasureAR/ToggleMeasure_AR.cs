using UnityEngine;
using UnityEngine.UI;

public class ARModeToggler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject measurementUI;
    public GameObject objectPlacementUI;

    [Header("AR Component References")]
    public ARMeasurementTool measurementTool;
    public PlaceOnPlane placeOnPlane;

    private Button toggleButton;
    private bool isMeasurementMode;
    private Color originalImageColor; // Store the original image color

    private void Awake()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(ToggleMode);
        
        // Store the original image color
        var image = toggleButton.GetComponent<Image>();
        if (image != null)
        {
            originalImageColor = image.color;
        }
    }

    private void Start()
    {
        // Initialize with measurement mode OFF (placement mode ON)
        SetMode(false);
    }

    private void ToggleMode()
    {
        SetMode(!isMeasurementMode);
    }

    private void SetMode(bool measurementModeActive)
    {
        isMeasurementMode = measurementModeActive;

        // Toggle UIs
        measurementUI.SetActive(measurementModeActive);
        objectPlacementUI.SetActive(!measurementModeActive);

        // Toggle AR components
        measurementTool.SetMeasurementMode(measurementModeActive);
        placeOnPlane.SetPlacementMode(!measurementModeActive);

        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        var text = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = isMeasurementMode ? "Switch to Placement" : "Switch to Measurement";
            text.color = isMeasurementMode ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
        }

        // Restore the original image color
        var image = toggleButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = originalImageColor;
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleMode);
    }
}