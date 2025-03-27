using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SwitchFloorToWall : MonoBehaviour
{
    [Tooltip("Reference to the AR Plane Manager.")]
    public ARPlaneManager arPlaneManager;

    [Tooltip("Reference to the UI Button for toggling.")]
    public Button toggleButton;

    [Tooltip("Text component to display the current mode.")]
    public Text modeText;

    private bool isHorizontal = true; // Tracks the current mode (horizontal or vertical)

    void Start()
    {
        // Ensure ARPlaneManager is assigned
        if (arPlaneManager == null)
        {
            Debug.LogError("ARPlaneManager is not assigned!");
            return;
        }

        // Ensure the button is assigned
        if (toggleButton == null)
        {
            Debug.LogError("Toggle Button is not assigned!");
            return;
        }

        // Add a listener to the button to handle clicks
        toggleButton.onClick.AddListener(TogglePlaneDetection);

        // Initialize the mode text
        UpdateModeText();
    }

    void TogglePlaneDetection()
    {
        // Toggle between horizontal and vertical plane detection
        isHorizontal = !isHorizontal;

        // Update the ARPlaneManager's detection mode
        if (isHorizontal)
        {
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }
        else
        {
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
        }

        // Update the UI text
        UpdateModeText();

        // Log the current detection mode
        Debug.Log($"Plane Detection Mode: {arPlaneManager.requestedDetectionMode}");

        // Reset the AR session to apply the new detection mode
        ResetARSession();
    }

    void UpdateModeText()
    {
        if (modeText != null)
        {
            modeText.text = isHorizontal ? "Horizontal" : "Vertical";
        }
    }

    void ResetARSession()
    {
        ARSession arSession = FindObjectOfType<ARSession>();
        if (arSession != null)
        {
            arSession.Reset();
            Debug.Log("AR Session Reset.");
        }
    }

    void Update()
    {
        // Debug: Log all detected planes
        foreach (var plane in arPlaneManager.trackables)
        {
            Debug.Log($"Plane detected: {plane.trackableId}, Alignment: {plane.alignment}, Center: {plane.center}");
        }
    }
}