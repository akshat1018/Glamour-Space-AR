using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallThemeManager : MonoBehaviour
{
    public Material[] wallThemes;   // Array of wall themes (textures/colors)
    private MeshRenderer wallRenderer;

    void Start()
    {
        wallRenderer = GetComponent<MeshRenderer>();  // Assumes a mesh renderer is on the wall object
    }

    public void ApplyTheme(int themeIndex)
    {
        if (themeIndex >= 0 && themeIndex < wallThemes.Length)
        {
            wallRenderer.material = wallThemes[themeIndex];
            Debug.Log("Applied theme: " + wallThemes[themeIndex].name);
        }
    }

    // Call this function after wall points are defined
    public void GenerateWallMesh(List<Vector3> wallCorners)
    {
        // Create the wall mesh using the corners defined by the user
        Mesh wallMesh = new Mesh();
        Vector3[] vertices = wallCorners.ToArray();
        int[] triangles = new int[(wallCorners.Count - 2) * 3];

        // Define mesh vertices and triangles (you'll need to create this based on user input)
        // Assign the generated mesh to a mesh filter

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = wallMesh;
    }
}


