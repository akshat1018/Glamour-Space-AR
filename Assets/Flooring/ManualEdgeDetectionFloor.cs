//--- START OF FILE ManualEdgeDetectionFloor.cs ---

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager), typeof(ARAnchorManager))]
public class ManualEdgeDetectionFloor : MonoBehaviour
{
    // --- Critical Setup Notes ---
    // 1. This script MUST be on a single GameObject like AR Session Origin, NOT on the plane prefab.
    // 2. ARPlaneManager's "Plane Prefab" slot MUST be assigned a SIMPLE visualizer prefab
    //    (MeshFilter, MeshRenderer, ARPlaneMeshVisualizer ONLY) - ideally the bright magenta/cyan one.
    // ---

    [Header("AR Components (Should be on this GameObject)")]
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;
    public ARAnchorManager arAnchorManager;

    [Header("Visuals & Prefabs (Used by THIS script)")]
    public LineRenderer lineRenderer;        // For drawing the trace line
    public GameObject edgePointPrefab;     // Marker for tapped points
    public Material floorMaterial;         // Material for the FINAL manually created floor

    [Header("Floor Settings")]
    [Range(0.001f, 0.1f)] public float lineWidth = 0.01f;
    public float vertexSnapDistance = 0.15f;
    public float floorOffset = 0.001f;

    [Header("UI Components (Canvas Elements)")]
    public Button startButton;
    public Button createFloorButton;
    public Button resetButton;
    public Text debugText;

    // --- Private State ---
    private List<Vector3> edgePoints = new List<Vector3>();
    private List<GameObject> edgePointInstances = new List<GameObject>();
    private GameObject floorInstance; // The manually created floor
    private ARAnchor floorAnchor;
    private bool isTracing = false;
    private bool setupErrorDetected = false; // Flag to halt operation on error

    void Start()
    {
        Debug.Log("--- ManualEdgeDetectionFloor Start() ---", this.gameObject);

        // --- Essential Setup Verification ---
        if (arRaycastManager == null) arRaycastManager = GetComponent<ARRaycastManager>();
        if (arPlaneManager == null) arPlaneManager = GetComponent<ARPlaneManager>();
        if (arAnchorManager == null) arAnchorManager = GetComponent<ARAnchorManager>();

        // Check if components are found on THIS game object
        if (arRaycastManager == null || arPlaneManager == null || arAnchorManager == null) {
            Debug.LogError("CRITICAL SETUP ERROR: Required AR Managers not found on the same GameObject as ManualEdgeDetectionFloor!", this.gameObject);
            setupErrorDetected = true;
        }

        // Check UI assignments
        if (startButton == null || createFloorButton == null || resetButton == null || debugText == null) {
            Debug.LogError("CRITICAL SETUP ERROR: One or more UI Components are not assigned in the Inspector!", this.gameObject);
            setupErrorDetected = true;
        }

        // Check essential prefabs/materials needed by THIS script
        if (lineRenderer == null) {
             Debug.LogError("CRITICAL SETUP ERROR: Line Renderer not assigned!", this.gameObject);
             setupErrorDetected = true;
        }
        if (edgePointPrefab == null) Debug.LogWarning("Edge Point Prefab not assigned. Markers won't appear.", this.gameObject);
        if (floorMaterial == null) Debug.LogWarning("Floor Material not assigned. Final floor may be invisible/pink.", this.gameObject);


        // THE MOST IMPORTANT CHECK: Verify the ARPlaneManager's assigned prefab
        if (arPlaneManager != null && arPlaneManager.planePrefab != null)
        {
            // Check if the assigned plane prefab incorrectly contains THIS script
            if (arPlaneManager.planePrefab.GetComponent<ManualEdgeDetectionFloor>() != null)
            {
                Debug.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Debug.LogError("CRITICAL SETUP ERROR: The prefab assigned to ARPlaneManager's 'Plane Prefab' slot ('" + arPlaneManager.planePrefab.name + "') contains the ManualEdgeDetectionFloor script itself! This WILL cause automatic floor instantiation. Assign a SIMPLE visualization prefab (like TEMP_PlaneVisualizer) instead!", arPlaneManager.planePrefab);
                Debug.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                setupErrorDetected = true;
            }
             // Check if the assigned plane prefab might be using the FINAL floor material (confusing)
            MeshRenderer visRenderer = arPlaneManager.planePrefab.GetComponent<MeshRenderer>();
            if (visRenderer != null && floorMaterial != null && visRenderer.sharedMaterial == floorMaterial) {
                Debug.LogWarning("SETUP WARNING: The ARPlaneManager's 'Plane Prefab' seems to be using the same material assigned as the final 'Floor Material'. This might be confusing. Consider using a distinct, simple, transparent material (like the bright magenta/cyan one) for the Plane Prefab.", arPlaneManager.planePrefab);
            }
             // Check if it lacks the necessary visualizer script
            if (arPlaneManager.planePrefab.GetComponent<ARPlaneMeshVisualizer>() == null) {
                 Debug.LogWarning("SETUP WARNING: The ARPlaneManager's 'Plane Prefab' does not have an ARPlaneMeshVisualizer component. It might not display correctly.", arPlaneManager.planePrefab);
            }

        } else if (arPlaneManager != null && arPlaneManager.planePrefab == null) {
             Debug.LogWarning("ARPlaneManager 'Plane Prefab' is not assigned. Plane visualization will be disabled, making tracing very difficult.", this.gameObject);
        }

        // Halt operation if critical errors found
        if (setupErrorDetected) {
            if(debugText != null) debugText.text = "SETUP ERROR! Check Console!";
            Debug.LogError("--- ManualEdgeDetectionFloor HALTED due to setup errors. ---", this.gameObject);
            // Disable component to prevent further execution
            this.enabled = false;
            // Optionally disable UI
             if(startButton != null) startButton.interactable = false;
             if(createFloorButton != null) createFloorButton.interactable = false;
             if(resetButton != null) resetButton.interactable = false;
            return; // Stop further initialization
        }

        // --- If Setup Seems OK, Proceed with Initialization ---
        ConfigureLineRenderer();
        SetupUI();
        Debug.Log("--- ManualEdgeDetectionFloor Initialization Complete ---", this.gameObject);
    }

    void ConfigureLineRenderer() {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")) { color = Color.green };
        lineRenderer.useWorldSpace = true;
    }

    void SetupUI()
    {
        // Listeners are added only if setup is okay
        createFloorButton.interactable = false;
        startButton.onClick.AddListener(StartTracing);
        createFloorButton.onClick.AddListener(CreateTriangulatedFloor);
        resetButton.onClick.AddListener(ResetTracing);
        debugText.text = "Press 'Start Tracing' to begin";
    }

    void Update()
    {
        // Do nothing if setup failed or not tracing
        if (setupErrorDetected || !isTracing) return;

        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.isPressed) return;
        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        if (IsPointerOverUI(touchPosition)) return;

        var hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) {
            Vector3 newPoint = hits[0].pose.position;
            if (CheckForLoopClosing(newPoint)) return;
            if (edgePoints.Count > 0 && Vector3.Distance(newPoint, edgePoints.Last()) < 0.05f) return;
            AddEdgePoint(newPoint);
        }
    }

    // --- Tracing Logic (Largely Unchanged) ---

    bool CheckForLoopClosing(Vector3 newPoint) {
        if (edgePoints.Count >= 3 && Vector3.Distance(newPoint, edgePoints[0]) < vertexSnapDistance) {
            AddEdgePoint(edgePoints[0]); // Snap exactly
            isTracing = false;
            createFloorButton.interactable = true;
            startButton.interactable = false;
            debugText.text = "Loop closed! Press 'Create Floor'";
            return true;
        }
        return false;
    }

    void AddEdgePoint(Vector3 point) {
         // Only instantiate marker if prefab is assigned
        if (edgePointPrefab != null) {
            var edgePointObject = Instantiate(edgePointPrefab, point, Quaternion.identity);
            edgePointInstances.Add(edgePointObject);
        } else {
             // Log if prefab missing, but still add point data
             // Debug.Log("Adding edge point data without visual marker (prefab missing).");
        }

        edgePoints.Add(point);
        UpdateLineRenderer();

        bool loopClosed = edgePoints.Count >= 4 && edgePoints.First() == edgePoints.Last();
        createFloorButton.interactable = loopClosed;

        if (!loopClosed && edgePoints.Count >= 3) {
            debugText.text = $"Points: {edgePoints.Count}. Tap near start ({edgePoints[0]:F2})";
        } else if (!loopClosed) {
             debugText.text = $"Points: {edgePoints.Count}. Need >=3 points & close loop.";
        }
    }

     void UpdateLineRenderer() {
        if(lineRenderer == null) return; // Check if line renderer exists
        lineRenderer.positionCount = edgePoints.Count;
        lineRenderer.SetPositions(edgePoints.ToArray());
    }

    // --- Manual Floor Creation Logic (Triggered by Button) ---

    public void CreateTriangulatedFloor() {
        if(setupErrorDetected) return; // Don't run if setup failed

        Debug.Log($"CreateTriangulatedFloor: Button pressed. Attempting floor creation with {edgePoints.Count} points.");
        if (edgePoints.Count < 4 || edgePoints[0] != edgePoints.Last()) {
            debugText.text = "Loop not valid! Reset and try again.";
            Debug.LogError("CreateTriangulatedFloor: Validation failed.");
            return;
        }

        CleanupFloor(); // Clear old floor

        // Prep vertices
        float floorY = edgePoints.Average(p => p.y) + floorOffset;
        List<Vector3> polygonVertices = new List<Vector3>();
        for(int i = 0; i < edgePoints.Count - 1; i++) {
            polygonVertices.Add(new Vector3(edgePoints[i].x, floorY, edgePoints[i].z));
        }

        // Generate mesh
        Mesh floorMesh = GenerateFloorMesh(polygonVertices);
        if (floorMesh == null) {
            debugText.text = "Mesh generation failed! Check console.";
            return;
        }

        // Create GameObject instance
        floorInstance = new GameObject("ManualFloor_" + System.DateTime.Now.Ticks);
        floorInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); // Place at origin

        // Add components
        var meshFilter = floorInstance.AddComponent<MeshFilter>();
        meshFilter.mesh = floorMesh;
        var meshRenderer = floorInstance.AddComponent<MeshRenderer>();
        meshRenderer.material = floorMaterial; // Use the FINAL floor material here
        if (floorMaterial == null) Debug.LogError("Floor Material is NULL, created floor will be invisible/pink!");

        // Adjust UV Tiling (Optional)
         if (floorMaterial != null && meshRenderer.material.HasProperty("_MainTex")) {
            float tiling = 2.0f;
            float scaleX = floorMesh.bounds.size.x > 0.01f ? floorMesh.bounds.size.x / tiling : 1f;
            float scaleZ = floorMesh.bounds.size.z > 0.01f ? floorMesh.bounds.size.z / tiling : 1f;
            meshRenderer.material.mainTextureScale = new Vector2(scaleX, scaleZ);
         }

        floorInstance.AddComponent<MeshCollider>().sharedMesh = floorMesh; // Add collider

        // Attach Anchor
        TryAttachAnchor();

        // Update UI and hide tracing visuals
        debugText.text = $"Floor created! Tris: {floorMesh.triangles.Length / 3}";
        Debug.Log($"--- Manual Floor '{floorInstance.name}' CREATED successfully. ---");
        if(lineRenderer != null) lineRenderer.enabled = false;
        foreach(var instance in edgePointInstances) { if(instance != null) instance.SetActive(false); }
        createFloorButton.interactable = false;
    }

    void TryAttachAnchor() {
        ARRaycastHit anchorHit = default;
        bool foundAnchorPlaneHit = false;

        // Try raycast from first point
        if (edgePoints.Count > 0 && Camera.main != null) {
            var hits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(Camera.main.WorldToScreenPoint(edgePoints[0]), hits, TrackableType.PlaneWithinPolygon) && hits.Count > 0) {
                anchorHit = hits[0]; foundAnchorPlaneHit = true;
            }
        }
        // Fallback: screen center
        if (!foundAnchorPlaneHit) {
             var hits = new List<ARRaycastHit>();
             if (arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon) && hits.Count > 0) {
                 anchorHit = hits[0]; foundAnchorPlaneHit = true; Debug.Log("Anchor: Using fallback screen center hit.");
             }
        }

        if (foundAnchorPlaneHit) {
            ARPlane plane = arPlaneManager.GetPlane(anchorHit.trackableId);
            if (plane != null) {
                floorAnchor = arAnchorManager.AttachAnchor(plane, anchorHit.pose);
                if (floorAnchor != null) {
                    Debug.Log($"Anchor: Attached anchor {floorAnchor.name} to plane {plane.trackableId}.");
                    floorInstance.transform.SetParent(floorAnchor.transform, true);
                } else { Debug.LogError("Anchor: AttachAnchor(Plane, Pose) failed."); floorAnchor = floorInstance.AddComponent<ARAnchor>(); }
            } else { Debug.LogWarning($"Anchor: GetPlane failed for {anchorHit.trackableId}."); floorAnchor = floorInstance.AddComponent<ARAnchor>(); }
        } else { Debug.LogWarning("Anchor: No plane hit found for anchoring."); floorAnchor = floorInstance.AddComponent<ARAnchor>(); }
    }


    Mesh GenerateFloorMesh(List<Vector3> polygonVertices) {
        // (This function remains the same as previous versions - handles triangulation, UVs, etc.)
         if (polygonVertices == null || polygonVertices.Count < 3) { Debug.LogError("GenerateFloorMesh: Not enough vertices."); return null; }
        Triangulator tr = new Triangulator(polygonVertices);
        int[] indices = tr.Triangulate();
        if (indices == null || indices.Length == 0) {
            Debug.LogError("GenerateFloorMesh: Triangulation failed. Trying reversed...");
             List<Vector3> reversed = new List<Vector3>(polygonVertices); reversed.Reverse();
             tr = new Triangulator(reversed); indices = tr.Triangulate();
             if(indices == null || indices.Length == 0) { Debug.LogError("GenerateFloorMesh: Triangulation failed even after reversing."); return null; }
             Debug.LogWarning("GenerateFloorMesh: Triangulation succeeded after reversing vertices."); polygonVertices = reversed;
        }
        Vector3[] vertices = polygonVertices.ToArray();
        Vector2[] uvs = new Vector2[vertices.Length];
        Bounds bounds = new Bounds(vertices[0], Vector3.zero);
        for(int i = 1; i < vertices.Length; i++) bounds.Encapsulate(vertices[i]);
        float uvScaleX = bounds.size.x > 0.01f ? 1.0f / bounds.size.x : 1.0f; float uvScaleZ = bounds.size.z > 0.01f ? 1.0f / bounds.size.z : 1.0f;
        for (int i = 0; i < vertices.Length; i++) { uvs[i] = new Vector2((vertices[i].x - bounds.min.x) * uvScaleX, (vertices[i].z - bounds.min.z) * uvScaleZ); }
        Mesh mesh = new Mesh { name = "ManualFloor_Mesh" };
        mesh.vertices = vertices; mesh.triangles = indices; mesh.uv = uvs;
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    // --- Button Actions / State Resets ---

    public void StartTracing() {
        if(setupErrorDetected) return;
        Debug.Log("StartTracing: Button pressed.");
        ResetTracing(); // Clear previous state
        isTracing = true;
        startButton.interactable = false;
        if(lineRenderer != null) lineRenderer.enabled = true;
        debugText.text = "Tap on detected planes (bright colors) to place points.";
    }

    public void ResetTracing() {
        if(setupErrorDetected && startButton != null) { // Allow reset even if setup failed
             startButton.interactable = false; // Keep start disabled if error
        } else if (!setupErrorDetected && startButton != null) {
            startButton.interactable = true; // Re-enable start if no error
        }
        if(createFloorButton != null) createFloorButton.interactable = false;
        if(debugText != null) debugText.text = "Press 'Start Tracing' to begin";
         if(setupErrorDetected && debugText != null) debugText.text = "SETUP ERROR! Check Console!"; // Show error on reset too

        Debug.Log("ResetTracing: Clearing state.");
        isTracing = false;
        edgePoints.Clear();

        // Reset line
        if(lineRenderer != null) {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = true; // Keep visible for next trace
        }

        // Destroy markers
        foreach (var instance in edgePointInstances) { if (instance != null) Destroy(instance); }
        edgePointInstances.Clear();

        // Destroy floor
        CleanupFloor();
    }

    void CleanupFloor() {
         if (floorAnchor != null) { Debug.Log($"CleanupFloor: Destroying anchor {floorAnchor.name} and children."); Destroy(floorAnchor.gameObject); floorAnchor = null; }
         if (floorInstance != null) { Debug.LogWarning($"CleanupFloor: Destroying dangling floor instance {floorInstance.name}."); Destroy(floorInstance); floorInstance = null; }
    }

    bool IsPointerOverUI(Vector2 position) {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = position };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

     void OnDestroy() {
        Debug.Log("--- ManualEdgeDetectionFloor OnDestroy() ---", this.gameObject);
        // Clean up listeners if buttons still exist
        if (startButton != null) startButton.onClick.RemoveAllListeners();
        if (createFloorButton != null) createFloorButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        CleanupFloor(); // Ensure floor is destroyed
    }
}
//--- END OF FILE ManualEdgeDetectionFloor.cs ---