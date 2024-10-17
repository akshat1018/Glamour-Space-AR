using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



public class WallPointPlacer : MonoBehaviour
{
    public ARRaycastManager raycastManager;  // Reference to the AR Raycast Manager
    public GameObject cornerPointPrefab;     // Prefab for the corner points
    public List<Vector3> wallCorners = new List<Vector3>();  // List to store wall corner points
    private List<GameObject> placedPoints = new List<GameObject>(); // Store placed corner objects

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Perform a raycast to the screen's touch position
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinBounds))
                {
                    // Place a point at the raycast hit location
                    Pose hitPose = hits[0].pose;
                    GameObject corner = Instantiate(cornerPointPrefab, hitPose.position, hitPose.rotation);
                    placedPoints.Add(corner);
                    wallCorners.Add(hitPose.position); // Store the corner point in world space

                    Debug.Log("Corner placed at: " + hitPose.position);
                }
            }
        }
    }

    public void ClearWallCorners()
    {
        foreach (var point in placedPoints)
        {
            Destroy(point);
        }
        placedPoints.Clear();
        wallCorners.Clear();
    }
}

