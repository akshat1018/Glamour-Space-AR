using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FlooringApp : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pointPrefab; // Prefab for visual markers
    public Material flooringMaterial; // Material for the flooring texture
    public ARPlaneManager arPlaneManager; // Reference to AR Plane Manager
    public Vector3 pointScale = new Vector3(0.05f, 0.05f, 0.05f); // Scale of the point prefab
    public float minDistanceBetweenPoints = 0.1f; // Minimum distance between points
    public float planeUpdateInterval = 2.0f; // Time interval to update plane detection

    [Header("Debug")]
    public bool debugMode = true; // Enable debug logs and visualizations

    private List<Vector3> points = new List<Vector3>(); // List to store point positions
    private List<GameObject> pointObjects = new List<GameObject>(); // List to store point objects
    private ARRaycastManager arRaycastManager; // Reference to AR Raycast Manager
    private float lastPlaneUpdateTime; // Last time plane detection was updated
    private GameObject meshObject; // GameObject for the floor mesh
    private bool isFloorDetected = false; // Flag to check if the floor is detected

    void Start()
    {
        // Initialize ARRaycastManager
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        if (arRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager not found in the scene!");
            return;
        }

        // Initialize ARPlaneManager
        if (arPlaneManager == null)
        {
            Debug.LogError("ARPlaneManager is not assigned!");
            return;
        }

        // Configure ARPlaneManager to detect only horizontal planes
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        // Disable default plane visualization
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        lastPlaneUpdateTime = Time.time;
    }

    void Update()
    {
        // Update plane detection at regular intervals
        if (Time.time - lastPlaneUpdateTime >= planeUpdateInterval && !isFloorDetected)
        {
            UpdatePlaneDetection();
            lastPlaneUpdateTime = Time.time;
        }

        // Detect touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTouchInput(Input.GetTouch(0));
        }
    }

    void UpdatePlaneDetection()
    {
        foreach (var plane in arPlaneManager.trackables)
        {
            // Only allow horizontal planes facing upward (floors)
            if (plane.alignment != PlaneAlignment.HorizontalUp)
            {
                plane.gameObject.SetActive(false); // Hide non-floor planes
                continue;
            }

            // Filter planes based on tracking state
            if (plane.trackingState != TrackingState.Tracking)
            {
                plane.gameObject.SetActive(false); // Hide unstable planes
                continue;
            }

            // Filter planes based on size (e.g., ignore planes smaller than 0.5m x 0.5m)
            if (plane.size.magnitude < 0.5f)
            {
                plane.gameObject.SetActive(false); // Hide small planes
                continue;
            }

            // Enable only large and stable floor planes
            plane.gameObject.SetActive(true);

            // Lock the floor and disable plane detection
            isFloorDetected = true;
            arPlaneManager.enabled = false; // Disable ARPlaneManager to stop plane updates

            if (debugMode)
            {
                Debug.Log($"Floor plane detected and locked: {plane.trackableId}, Center: {plane.center}, Size: {plane.size}");
            }
        }
    }

    void HandleTouchInput(Touch touch)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Perform raycast to detect if the touch hits a horizontal floor plane
        if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinBounds))
        {
            ARPlane plane = arPlaneManager.GetPlane(hits[0].trackableId);

            // Ensure the detected plane is a horizontal floor
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                Pose hitPose = hits[0].pose;

                // Check if the hit point is near an edge using advanced edge detection
                if (!IsPointNearEdge(plane, hitPose.position))
                {
                    // Check surface normal (ensure it's facing upward)
                    if (Vector3.Dot(hitPose.up, Vector3.up) > 0.9f) // Adjust threshold as needed
                    {
                        if (!IsPointTooClose(hitPose.position))
                        {
                            PlacePoint(hitPose.position); // Place a point at the hit position
                        }
                        else if (debugMode)
                        {
                            Debug.Log("Point is too close to an existing point.");
                        }
                    }
                }
                else if (debugMode)
                {
                    Debug.Log("Point is near an edge.");
                }
            }
        }
    }

    bool IsPointNearEdge(ARPlane plane, Vector3 point)
    {
        // Get the plane's boundary points
        var boundary = plane.boundary;

        // Convert the point to the plane's local coordinate system
        Vector3 localPoint = plane.transform.InverseTransformPoint(point);

        // Check if the point is near the boundary
        float edgeThreshold = 0.1f; // Adjust threshold as needed
        foreach (var boundaryPoint in boundary)
        {
            if (Vector3.Distance(localPoint, boundaryPoint) < edgeThreshold)
            {
                return true; // Point is near the edge
            }
        }

        return false; // Point is not near the edge
    }

    bool IsPointTooClose(Vector3 newPoint)
    {
        foreach (var point in points)
        {
            if (Vector3.Distance(newPoint, point) < minDistanceBetweenPoints)
            {
                return true;
            }
        }
        return false;
    }

    void PlacePoint(Vector3 position)
    {
        points.Add(position);
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity);
        pointObj.transform.localScale = pointScale;
        pointObjects.Add(pointObj);

        if (debugMode)
        {
            Debug.Log($"Point placed at: {position}");
        }

        if (points.Count >= 3)
        {
            CreateMesh();
        }
    }

    void CreateMesh()
    {
        if (points.Count < 3)
        {
            Debug.LogWarning("Not enough points to create a polygon.");
            return;
        }

        List<Vector3> orderedPoints = OrderVertices(points);
        Mesh mesh = new Mesh
        {
            vertices = orderedPoints.ToArray(),
            triangles = CalculateTriangles(orderedPoints).ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshObject == null)
        {
            meshObject = new GameObject("FloorMesh");
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = flooringMaterial; // Assign the custom material
            meshObject.AddComponent<MeshCollider>();
        }

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        if (debugMode)
        {
            Debug.Log("Floor mesh created/updated successfully.");
        }
    }

    List<int> CalculateTriangles(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
        return triangles;
    }

    List<Vector3> OrderVertices(List<Vector3> vertices)
    {
        Vector3 center = Vector3.zero;
        foreach (var vertex in vertices)
        {
            center += vertex;
        }
        center /= vertices.Count;

        vertices.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angleB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angleA.CompareTo(angleB);
        });

        return vertices;
    }

    public void ResetARSession()
    {
        ARSession arSession = FindObjectOfType<ARSession>();
        if (arSession != null)
        {
            arSession.Reset();
            Debug.Log("AR Session Reset.");
        }
    }
}