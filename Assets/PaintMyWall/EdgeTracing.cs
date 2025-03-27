using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class EdgeTracing : MonoBehaviour
{
    public ARRaycastManager arRaycastManager; // AR Raycast Manager for plane detection
    public LineRenderer lineRenderer; // LineRenderer to visualize traced edges
    public GameObject wallPrefab; // Prefab for creating walls

    private List<Vector3> edgePoints = new List<Vector3>(); // List to store traced points

    private void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position; // Get touch position in screen coordinates

            // Perform AR raycast to detect AR planes
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose; // Get the pose of the hit point
                edgePoints.Add(hitPose.position); // Add the 3D point to the list

                // Update the LineRenderer to visualize the traced edges
                lineRenderer.positionCount = edgePoints.Count;
                lineRenderer.SetPositions(edgePoints.ToArray());
            }
        }
    }

    // Method to create a wall from the traced points
    public void CreateWall()
    {
        if (edgePoints.Count > 1) // Ensure there are enough points to create a wall
        {
            // Create a wall GameObject
            GameObject wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);

            // Generate a mesh for the wall
            Mesh wallMesh = CreateWallMesh(edgePoints);
            wall.GetComponent<MeshFilter>().mesh = wallMesh;
            wall.GetComponent<MeshCollider>().sharedMesh = wallMesh;

            // Clear the traced points for the next wall
            edgePoints.Clear();
            lineRenderer.positionCount = 0;
        }
    }

    // Method to create a mesh from a list of points
    private Mesh CreateWallMesh(List<Vector3> points)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Count * 2]; // Vertices for the top and bottom edges
        int[] triangles = new int[(points.Count - 1) * 6]; // Triangles to connect the vertices

        // Create vertices for the top and bottom edges
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i * 2] = points[i]; // Bottom vertex
            vertices[i * 2 + 1] = points[i] + Vector3.up * 2; // Top vertex (adjust height as needed)
        }

        // Create triangles to form the wall mesh
        for (int i = 0; i < points.Count - 1; i++)
        {
            int ti = i * 6; // Triangle index
            triangles[ti] = i * 2; // Bottom left
            triangles[ti + 1] = i * 2 + 1; // Top left
            triangles[ti + 2] = i * 2 + 2; // Bottom right

            triangles[ti + 3] = i * 2 + 2; // Bottom right
            triangles[ti + 4] = i * 2 + 1; // Top left
            triangles[ti + 5] = i * 2 + 3; // Top right
        }

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Recalculate normals for proper lighting

        return mesh;
    }
}