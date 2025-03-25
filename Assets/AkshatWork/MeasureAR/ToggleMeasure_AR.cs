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

    private void Awake()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(ToggleMode);
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
        measurementTool.enabled = measurementModeActive;
        placeOnPlane.enabled = !measurementModeActive;

        // Clean up states when switching
        if (measurementModeActive)
        {
            //placeOnPlane.DeselectObject(); // Clear any selected placement objects
        }
        else
        {
            measurementTool.ResetMeasurement(); // Clear measurements
        }

        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        var text = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = isMeasurementMode ? "Placement Mode" : "Measurement Mode";
        }

        // Visual feedback
        var colors = toggleButton.colors;
        colors.normalColor = isMeasurementMode ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
        toggleButton.colors = colors;
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleMode);
    }
}