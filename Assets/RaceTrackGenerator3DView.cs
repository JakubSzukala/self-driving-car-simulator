using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WallSide
{
    Left,
    Right
}


[RequireComponent(typeof(RaceTrackGenerator))]
public class RaceTrackGenerator3DView : MonoBehaviour, IRaceTrackRenderer
{
    public float roadWidth = 5f;
    public float wallHeight = 2f;

    // References to children
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject road;

    // Meshes
    private Mesh roadMesh;
    private Mesh wallLeftMesh;
    private Mesh wallRightMesh;

    // Materials
    public Material materialRoad;
    public Material materialWalls;

    public void PrepareTrackRender(Vector2[] path)
    {
        GenerateTrackMeshes(path, out roadMesh, out wallLeftMesh, out wallRightMesh); // TODO: refactor this
    }

    public bool IsTrackRenderValid()
    {
        bool IsTrackRenderValid = true;

        // Track is valid if no mesh overlaps were found
        IsTrackRenderValid &= !RaceTrackMeshArtifactDetector.FindRaceTrackMeshOverlaps(roadMesh).Any();

        return IsTrackRenderValid;
    }

    public void RenderTrack()
    {
        road.GetComponent<MeshFilter>().mesh = roadMesh;
        road.GetComponent<MeshRenderer>().material = materialRoad;

        // Flipping the faces inside out, so they are visible from inside track
        // It will also fix collisions
        wallLeftMesh.triangles = wallLeftMesh.triangles.Reverse().ToArray();
        wallLeft.GetComponent<MeshFilter>().mesh = wallLeftMesh;
        wallLeft.GetComponent<MeshRenderer>().material = materialWalls;
        wallLeft.GetComponent<MeshCollider>().sharedMesh = wallLeftMesh;

        wallRight.GetComponent<MeshFilter>().mesh = wallRightMesh;
        wallRight.GetComponent<MeshRenderer>().material = materialWalls;
        wallRight.GetComponent<MeshCollider>().sharedMesh = wallRightMesh;
    }

    // Add control variable? To allow choosing what to generate?
    private void GenerateTrackMeshes(Vector2[] path, out Mesh roadMesh, out Mesh wallLeft, out Mesh wallRight)
    {
        // https://www.youtube.com/watch?v=Q12sb-sOhdI&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=6
        // TODO: Does it need to be instantiated every time here?
        RoadVertexGenerator roadVertexGenerator = new RoadVertexGenerator(path.Length, roadWidth);
        WallVertexGenerator wallVertexGeneratorLeft = new WallVertexGenerator(path.Length, wallHeight, roadWidth, WallSide.Left);
        WallVertexGenerator wallVertexGeneratorRight = new WallVertexGenerator(path.Length, wallHeight, roadWidth, WallSide.Right);
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
            wallVertexGeneratorLeft.AddVertices(path[i], forward);
            wallVertexGeneratorRight.AddVertices(path[i], forward);
        }
        roadMesh = roadVertexGenerator.GetMesh();
        wallLeft = wallVertexGeneratorLeft.GetMesh();
        wallRight = wallVertexGeneratorRight.GetMesh();
    }

    public Mesh GetRoadMesh()
    {
        return road.GetComponent<MeshFilter>().mesh;
    }

    public Mesh GetWallLeftMesh()
    {
        return wallLeft.GetComponent<MeshFilter>().mesh;
    }

    public Mesh GetWallRightMesh()
    {
        return wallRight.GetComponent<MeshFilter>().mesh;
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

    public void SetPathLength(int pathLength)
    {
        if (pathLength < 1)
        {
            throw new System.ArgumentException("Path length can not be smaller than 1.");
        }
        this.pathLength = pathLength;
    }

    public Mesh GetMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }
}


public class RoadVertexGenerator : RaceTrackVertexGenerator
{
    private float roadWidth;

    public RoadVertexGenerator(int pathLength, float roadWidth) : base(pathLength)
    {
        if (roadWidth <= 0f)
        {
            throw new System.ArgumentException("Road width can not be smaller than 0.");
        }
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

    public override void Reset()
    {
        this.vertices = new Vector3[2 * pathLength];
        this.uvs = new Vector2[this.vertices.Length];
        this.triangles = new int[2 * pathLength * 3];
        this.vertexIndex = 0;
        this.triangleIndex = 0;
        this.completionPercentage = 0;
        this.roadWidth = 0f;
    }

    public void SetRoadWidth(float roadWidth)
    {
        if (roadWidth <= 0f)
        {
            throw new System.ArgumentException("Road width can not be smaller than 0.");
        }
        this.roadWidth = roadWidth;
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
        Vector3 sideVector = Vector3.zero;
        if (side == WallSide.Left)
        {
            sideVector = new Vector3(forward.y, -forward.x) * 0.5f * roadWidth;
        }
        if (side == WallSide.Right)
        {
            sideVector = new Vector3(-forward.y, forward.x) * 0.5f * roadWidth;
        }
        vertices[vertexIndex] = (Vector3)pathPoint + sideVector;
        vertices[vertexIndex + 1] = (Vector3)pathPoint + sideVector + new Vector3(0, 0, wallHeight);

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