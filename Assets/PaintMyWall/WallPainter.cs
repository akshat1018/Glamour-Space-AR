using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class WallPainter : MonoBehaviour
{
    public ARRaycastManager arRaycastManager;
    public GameObject wallPrefab;
    public LayerMask wallLayer;

    public GameObject currentWall; // Made public

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, wallLayer))
            {
                // If a wall is hit, paint it
                currentWall = hit.collider.gameObject;
            }
            else
            {
                // If no wall is hit, place a new wall
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (arRaycastManager.Raycast(Input.GetTouch(0).position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    currentWall = Instantiate(wallPrefab, hitPose.position, hitPose.rotation);
                }
            }
        }
    }
}