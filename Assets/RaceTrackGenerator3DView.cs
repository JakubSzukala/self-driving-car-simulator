using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WallSide
{
    Left,
    Right
}

public class RaceTrackGenerator3DView : MonoBehaviour, IRaceTrackFullRenderable
{
    [SerializeField] private List<IRaceTrackPartRenderable> renderables;

    void Awake()
    {
        renderables = GetComponentsInChildren<IRaceTrackPartRenderable>().ToList();
    }

    public void PrepareTrackRender(Vector2[] path)
    {
        // Initialize
        renderables.ForEach((r) => r.Initialize(path.Length));

        // Add vertices
        for (int i = 0; i < path.Length; i++)
        {
            // Calculate the forward direction
            int nextIndex = (i + 1) % path.Length;
            Vector3 forward = Vector3.zero;
            if (i > 0)
            {
                // If not first point consider also previous
                forward += (Vector3)path[i] - (Vector3)path[i - 1];
            }
            forward += (Vector3)path[nextIndex] - (Vector3)path[i];
            forward.Normalize();

            // Add vertex to each renderable
            foreach (var renderable in renderables)
            {
                renderable.AddVertices(path[i], forward);
            }
        }
    }

    public bool IsTrackRenderValid()
    {
        bool IsTrackRenderValid = true;

        foreach(var renderable in renderables)
        {
            // It is enough to check for road
            var renderableRoad = renderable as RoadVertexGenerator;
            if (renderableRoad)
            {
                // Track is valid if no mesh overlaps were found
                IsTrackRenderValid &= !RaceTrackMeshArtifactDetector.FindRaceTrackMeshOverlaps(renderableRoad.mesh).Any();
            }
        }

        return IsTrackRenderValid;
    }

    public void RenderTrack()
    {
        foreach (var renderable in renderables)
        {
            renderable.Render();
        }
    }
}

/*
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
        if (pathLength < 1)
        {
            throw new System.ArgumentException("Path length can not be smaller than 1.");
        }
        this.pathLength = pathLength;
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[this.vertices.Length];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0;
    }

    public abstract void AddVertices(Vector2 pathPoint, Vector2 forward);

    public abstract void Reset();

    public abstract Mesh GetMesh();

    public void SetPathLength(int pathLength)
    {
        if (pathLength < 1)
        {
            throw new System.ArgumentException("Path length can not be smaller than 1.");
        }
        this.pathLength = pathLength;
    }
}

public class WallVertexGenerator : RaceTrackVertexGenerator
{

    private float wallHeight;
    private float roadWidth;
    private WallSide side;

    public WallVertexGenerator(int pathLength, float wallHeight, float roadWidth, WallSide side) : base(pathLength)
    {
        // TODO: Clean up this a little
        if (wallHeight <= 0f || roadWidth <= 0f)
        {
            throw new System.ArgumentException("Arguments must be greater than zero.");
        }
        this.wallHeight = wallHeight;
        this.roadWidth = roadWidth;
        this.side = side;
    }

    public override void AddVertices(Vector2 pathPoint, Vector2 forward)
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

    public override void Reset()
    {
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[this.vertices.Length];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0f;
        this.roadWidth = 0f;
        this.wallHeight = 0f;
    }

    public override Mesh GetMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        if (side == WallSide.Right) triangles = triangles.Reverse().ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }

    public void SetRoadWidth(float roadWidth)
    {
        if (roadWidth <= 0f)
        {
            throw new System.ArgumentException("Road width can not be smaller than 0.");
        }
        this.roadWidth = roadWidth;
    }

    public void SetWallHeight(float wallHeight)
    {
        if (wallHeight <= 0f)
        {
            throw new System.ArgumentException("Wall height can not be smaller than 0.");
        }
        this.wallHeight = wallHeight;
    }

    public void SetWallSide(WallSide side) => this.side = side;
}
*/