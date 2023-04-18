using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaceTrackGenerator))]
//[RequireComponent(typeof(MeshRenderer))]
//[RequireComponent(typeof(MeshFilter))]
public class RaceTrackGenerator3DView : MonoBehaviour, IRaceTrackRenderer
{
    public float roadWidth = 5f;
    public Material materialRoad;

    // References to children
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject road;

    void Start()
    {

    }

    public void RenderTrack(Vector2[] path)
    {
        Mesh roadMesh;
        GenerateRoadMesh(path, out roadMesh);

        road.GetComponent<MeshFilter>().mesh = roadMesh;
        road.GetComponent<MeshRenderer>().material = materialRoad;
    }

    private void GenerateRoadMesh(Vector2[] path, out Mesh roadMesh)
    {
        // https://www.youtube.com/watch?v=Q12sb-sOhdI&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=6
        RoadVertexGenerator roadVertexGenerator = new RoadVertexGenerator(path.Length, roadWidth);
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

            roadVertexGenerator.AddVertices(path[i], forward);
        }
        roadMesh = roadVertexGenerator.GetMesh();
    }
}


public abstract class RaceTrackVertexGenerator
{
    protected int pathLength;
    protected Vector3[] vertices;
    protected Vector2[] uvs;
    protected int[] triangles;
    protected int vertexIndex;
    protected int triangleIndex;
    protected float completionPercentage;
    protected Mesh mesh;

    public RaceTrackVertexGenerator(int pathLength)
    {
        this.pathLength = pathLength;
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[this.vertices.Length];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0;
    }

    public abstract void AddVertices(Vector2 pathPoint, Vector2 forward);

    public abstract Mesh GetMesh();

    public abstract void Reset();

    public abstract void SetPathLength(int pathLength);
}


public class RoadVertexGenerator : RaceTrackVertexGenerator
{
    private float roadWidth;

    public RoadVertexGenerator(int pathLength, float roadWidth) : base(pathLength)
    {
        this.roadWidth = roadWidth;
    }

    public override void AddVertices(Vector2 pathPoint, Vector2 forward)
    {
        // Calculate orthogonal points on both sides
        Vector3 left = new Vector3(forward.y, -forward.x) * 0.5f * roadWidth;
        Vector3 right = new Vector3(-forward.y, forward.x) * 0.5f * roadWidth;

        // Add them as vertices
        vertices[vertexIndex] = (Vector3)pathPoint + left;
        vertices[vertexIndex + 1] = (Vector3)pathPoint + right;

        // Set uvs, take into account completion percentage of entire track
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

    public override Mesh GetMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }

    public override void Reset()
    {
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[this.vertices.Length];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0;
    }

    public override void SetPathLength(int pathLength)
    {
        if (pathLength < 1)
        {
            throw new System.ArgumentException("Path length can not be smaller than 1.");
        }
        this.pathLength = pathLength;
    }

    public void SetRoadWidth(float roadWidth)
    {
        if (roadWidth < 0f)
        {
            throw new System.ArgumentException("Road width can not be smaller than 0.");
        }
        this.roadWidth = roadWidth;
    }
}