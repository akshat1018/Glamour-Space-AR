using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Add this namespace for PlaneDetectionMode

public class PlaneStabilizer : MonoBehaviour
{
    private ARPlaneManager m_PlaneManager;

    void Start()
    {
        m_PlaneManager = FindObjectOfType<ARPlaneManager>();
        if (m_PlaneManager != null)
        {
            // Set the requested plane detection mode to Horizontal
            m_PlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            Debug.Log("Plane detection mode set to Horizontal.");
        }
        else
        {
            Debug.LogError("ARPlaneManager component not found.");
        }
    }
}