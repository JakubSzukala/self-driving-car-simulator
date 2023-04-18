using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaceTrackGenerator))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RaceTrackGenerator3DView : MonoBehaviour, IRaceTrackRenderer
{
    public float roadWidth = 5f;
    void Start()
    {

    }

    public void RenderTrack(Vector2[] path)
    {
        GetComponent<MeshFilter>().mesh = GenerateMesh(path);
    }

    public Mesh GenerateMesh(Vector2[] path)
    {
        // https://www.youtube.com/watch?v=Q12sb-sOhdI&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=6
        // For points with index > 0 take the average of directions
        // between next and previous to make the path smoother
        Vector3[] vertices = new Vector3[path.Length * 2];
        int[] meshTriangles = new int[2 * path.Length * 3];
        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int i = 0; i < path.Length; i++)
        {
            int nextIndex = (i + 1) % path.Length;
            Vector3 forward = Vector3.zero;
            if (i > 0)
            {
                // If not first point consider also previous
                forward += (Vector3)path[i] - (Vector3)path[i - 1];
            }
            forward += (Vector3)path[nextIndex] - (Vector3)path[i];
            forward.Normalize();

            // Get the orthogonal points **directions**
            Vector3 left = new Vector3(forward.y, -forward.x) * 0.5f * roadWidth;
            Vector3 right = new Vector3(-forward.y, forward.x) * 0.5f * roadWidth;

            // Actual orthogonal points, that will be vertices of road mesh
            vertices[vertexIndex] = (Vector3)path[i] + left;
            vertices[vertexIndex + 1] = (Vector3)path[i] + right;

            // Assign each vertex to a mesh triangle (see video)
            // Each three members of meshTriangles constitute to vertices of one triangle
            meshTriangles[triangleIndex] = vertexIndex;
            meshTriangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
            meshTriangles[triangleIndex + 2] = vertexIndex + 1;

            meshTriangles[triangleIndex + 3] = vertexIndex + 1;
            meshTriangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
            meshTriangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;

            vertexIndex += 2;
            triangleIndex += 6;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        return mesh;
    }
}
