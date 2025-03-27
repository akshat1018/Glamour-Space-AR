// --- START OF FILE SimplePlaneLogger.cs ---
using UnityEngine;
using UnityEngine.XR.ARFoundation; // Required for ARPlane

// Make sure ARPlane is also required if ARPlaneMeshVisualizer depends on it
[RequireComponent(typeof(ARPlaneMeshVisualizer), typeof(ARPlane))]
public class SimplePlaneLogger : MonoBehaviour
{
    void Start()
    {
        // Get the ARPlane component from the same GameObject
        ARPlane plane = GetComponent<ARPlane>();

        // Use the ARPlane component to get the trackableId
        if (plane != null) {
            Debug.Log($"--- SimplePlaneLogger: Visualizer instantiated for Plane ID: {plane.trackableId} ---", this.gameObject);
        } else {
            Debug.LogWarning("--- SimplePlaneLogger: Could not find ARPlane component on the same GameObject. ---", this.gameObject);
        }
    }
}
// --- END OF FILE SimplePlaneLogger.cs ---