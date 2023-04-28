using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WallSide
{
    Left,
    Right
}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class WallVertexGenerator : MonoBehaviour, IRaceTrackPartRenderable
{
    // DI fields
    [SerializeField] private float wallHeight = 1f;
    [SerializeField] private float roadWidth = 1f;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private WallSide side = WallSide.Left;

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
        Vector3 sideVector = Vector3.zero;
        if (side == WallSide.Left)
        {
            sideVector = new Vector3(forwardXZ.z, 0, -forwardXZ.x) * 0.5f * roadWidth;
        }
        if (side == WallSide.Right)
        {
            sideVector = new Vector3(-forwardXZ.z, 0, forwardXZ.x) * 0.5f * roadWidth;
        }
        vertices[vertexIndex] = (Vector3)pathPointXZ + sideVector;
        vertices[vertexIndex + 1] = (Vector3)pathPointXZ + sideVector + new Vector3(0, wallHeight, 0);

        completionPercentage += 1 / (float)(pathLength - 1);
        float v = -1 - Mathf.Abs(2 * completionPercentage - 1);
        uvs[vertexIndex] = new Vector2(0, v);
        uvs[vertexIndex + 1] = new Vector2(1, v);

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
        if (side == WallSide.Right) triangles = triangles.Reverse().ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    public void Render()
    {
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = wallMaterial;
    }

    public void Reset()
    {
        Initialize(pathLength);
    }
}
