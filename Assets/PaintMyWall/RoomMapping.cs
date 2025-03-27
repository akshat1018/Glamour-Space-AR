using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class RoomMapping : MonoBehaviour
{
    public ARMeshManager arMeshManager;

    private void OnEnable()
    {
        arMeshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnDisable()
    {
        arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        foreach (var meshFilter in args.added)
        {
            // Visualize the mesh
            MeshRenderer meshRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
    }
}