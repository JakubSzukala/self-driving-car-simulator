using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RoadRenderer : MonoBehaviour, IRaceTrackPartRenderable
{
    // DI fields
    [SerializeField] public float roadWidth = 1f; // TODO: Change this to property, private set public get
    [SerializeField] private Material roadMaterial;

    // Mesh parameters
    private int pathLength;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    private int vertexIndex;
    private int triangleIndex;
    private float completionPercentage;
    public Mesh mesh
    { get; private set; }

    public void Initialize(int pathLength)
    {
        this.pathLength = pathLength;
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[2 * pathLength];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0;
    }

    public void AddVertices(Vector2 pathPoint, Vector2 forward)
    {
        Vector3 pathPointXZ = new Vector3(pathPoint.x, 0, pathPoint.y);
        Vector3 forwardXZ = new Vector3(forward.x, 0, forward.y);

        // Calculate orthogonal points on both sides
        Vector3 left = new Vector3(forwardXZ.z, 0, -forwardXZ.x) * 0.5f * roadWidth;
        Vector3 right = new Vector3(-forwardXZ.z, 0, forwardXZ.x) * 0.5f * roadWidth;

        // Add them as vertices
        vertices[vertexIndex] = pathPointXZ + left;
        vertices[vertexIndex + 1] = pathPointXZ + right;

        // Set uvs, take into account completion percentage of entire track
        completionPercentage += 1 / (float)(pathLength - 1);
        float v = -1 - Mathf.Abs(2 * completionPercentage - 1);
        uvs[vertexIndex] = new Vector2(0, v);
        uvs[vertexIndex + 1] = new Vector2(1, v);

        // Set triangles
        triangles[triangleIndex] = vertexIndex;
        triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
        triangles[triangleIndex + 2] = vertexIndex + 1;

        triangles[triangleIndex + 3] = vertexIndex + 1;
        triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
        triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;

        vertexIndex += 2;
        triangleIndex += 6;
    }

    public void SetUpMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        triangles = triangles.Reverse().ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    public void Render()
    {
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = roadMaterial;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void Reset()
    {
        Initialize(pathLength);
    }
}
