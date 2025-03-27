using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace WallCovering
{
    public class ManualEdgeDetection : MonoBehaviour
    {
        public ARRaycastManager arRaycastManager;
        public LineRenderer lineRenderer;
        public GameObject edgePointPrefab;
        public GameObject wallPrefab;

        private List<Vector3> edgePoints = new List<Vector3>();

        private void Update()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 touchPosition = Input.GetTouch(0).position;

                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    edgePoints.Add(hitPose.position);

                    Instantiate(edgePointPrefab, hitPose.position, Quaternion.identity);

                    lineRenderer.positionCount = edgePoints.Count;
                    lineRenderer.SetPositions(edgePoints.ToArray());
                }
            }
        }

        public void CreateWall()
        {
            if (edgePoints.Count > 1)
            {
                GameObject wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);
                Mesh wallMesh = CreateWallMesh(edgePoints);
                wall.GetComponent<MeshFilter>().mesh = wallMesh;
                wall.GetComponent<MeshCollider>().sharedMesh = wallMesh;

                edgePoints.Clear();
                lineRenderer.positionCount = 0;
            }
        }

        private Mesh CreateWallMesh(List<Vector3> points)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[points.Count * 2];
            int[] triangles = new int[(points.Count - 1) * 6];

            for (int i = 0; i < points.Count; i++)
            {
                vertices[i * 2] = points[i];
                vertices[i * 2 + 1] = points[i] + Vector3.up * 2;
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                int ti = i * 6;
                triangles[ti] = i * 2;
                triangles[ti + 1] = i * 2 + 1;
                triangles[ti + 2] = i * 2 + 2;

                triangles[ti + 3] = i * 2 + 2;
                triangles[ti + 4] = i * 2 + 1;
                triangles[ti + 5] = i * 2 + 3;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}