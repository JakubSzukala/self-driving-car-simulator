using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaceTrackPathGenerator
{
    private Vector2[] randomPoints;
    private Vector2[] path;

    private int _rangeX;
    public int rangeX
    {
        get { return _rangeX; }
        set
        {
            if (value < 1)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(rangeX), value, "Argument must be greater than 0.");
            }
            _rangeX = value;
        }
    }
    private int _rangeY;
    public int rangeY
    {
        get { return _rangeY; }
        set
        {
            if (value < 1)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(rangeY), value, "Argument must be greater than 0.");
            }
            _rangeY = value;
        }
    }

    public RaceTrackPathGenerator(int rangeX, int rangeY)
    {
        this.rangeX = rangeX;
        this.rangeY = rangeY;
    }

    public Vector2[] GenerateConvexPath(int nPointsToWrap, int smoothingDegree)
    {
        // Validation
        if (nPointsToWrap < 3)
        {
            throw new System.ArgumentException("Number of points must be greater than 2.");
        }
        if (smoothingDegree < 0)
        {
            throw new System.ArgumentException("Smoothing degree must be greater than 0.");
        }

        // Logic
        GenerateRandomPoints(nPointsToWrap);
        CalculateConvexPath();
        for (int i = 0; i < smoothingDegree; i++)
        {
            ChaikinSmoothing();
        }
        return path;
    }

    public Vector2[] GenerateConcavePath(int nPointsToWrap, float pointConcavityProb, int smoothingDegree)
    {
        // Validation
        if (nPointsToWrap < 3)
        {
            throw new System.ArgumentException("Number of points must be greater than 2.");
        }
        if (smoothingDegree < 0)
        {
            throw new System.ArgumentException("Smoothing degree must be greater than 0.");
        }
        if (pointConcavityProb < 0f || pointConcavityProb > 1f)
        {
            throw new System.ArgumentException("Concativity probability must be between 0f and 1f");
        }

        // Logic
        GenerateRandomPoints(nPointsToWrap);
        CalculateConvexPath();
        CalculateConcavePath(pointConcavityProb);
        for (int i = 0; i < smoothingDegree; i++)
        {
            ChaikinSmoothing();
        }
        return path;
    }

    private void GenerateRandomPoints(int numberOfPoints)
    {
        if(numberOfPoints < 3)
        {
            throw new System.ArgumentException("Invalid number of points.");
        }

        randomPoints = new Vector2[numberOfPoints];
        for(int i = 0; i < numberOfPoints; i++)
        {
            randomPoints[i] = new Vector2(Random.Range(0, rangeX), Random.Range(0, rangeY));
        }

        // Consider only unique points
        randomPoints.Distinct().ToArray();
    }

    private void CalculateConvexPath()
    {
        if (randomPoints == null || randomPoints.Length < 3)
        {
            throw new System.ArgumentException("Invalid fields values.");
        }

        // Find the leftmost point, it is guaranteed to be on the hull
        List<Vector2> tempPath = new List<Vector2>();
        int mostLeftPointIdx = 0;
        foreach(var point in randomPoints)
        {
            if(point.x < randomPoints[mostLeftPointIdx].x)
            {
                mostLeftPointIdx = System.Array.IndexOf(randomPoints, point);
            }
        }
        tempPath.Add(randomPoints[mostLeftPointIdx]);

        // Iterate over the points and determine which lay on the convex hull
        int currentHullPointIdx = mostLeftPointIdx;
        int counter = -1; // Just a safety measure to prevent infinite loop
        do
        {
            counter++;
            // Pick a point (make sure it is different from the current hull point)
            int smallestAnglePointIdx = (currentHullPointIdx + 1) % randomPoints.Length;
            Vector2 smallestAnglePointDir = randomPoints[smallestAnglePointIdx] - randomPoints[currentHullPointIdx];
            for (int i = 0; i < randomPoints.Length; i++)
            {
                // Do not check with self and current smallest angle point
                if (i == currentHullPointIdx || i == smallestAnglePointIdx)
                {
                    continue;
                }
                // Point most to the left will have the greatest signed angle value
                Vector2 currentPointDir = randomPoints[i] - randomPoints[currentHullPointIdx];
                if (Vector2.SignedAngle(smallestAnglePointDir, currentPointDir) > 0f)
                {
                    smallestAnglePointIdx = i;
                    smallestAnglePointDir = randomPoints[i] - randomPoints[currentHullPointIdx];
                }
            }
            // This condition being true indicates that all convex hull points were found
            if (smallestAnglePointIdx == mostLeftPointIdx) break;

            tempPath.Add(randomPoints[smallestAnglePointIdx]);
            currentHullPointIdx = smallestAnglePointIdx;

        } while (counter < randomPoints.Length);
        path = tempPath.ToArray();
    }

    public void CalculateConcavePath(float probability)
    {
        if (probability < 0f || probability > 1f)
        {
            throw new System.ArgumentException("Invalid probability value.");
        }
        List<Vector2> concaveHull = new List<Vector2>();

        // Calculate center of mass of a convex hull
        Vector2 accumulate = Vector2.zero;
        foreach(var point in path)
        {
            accumulate += point;
        }
        // Lower bound is the center of mass
        Vector2 innerBound = accumulate / path.Count();

        // For randomly picked points on the hull displace them towards center of mass
        float percentile;
        for(int i = 0; i < path.Count(); i++)
        {
            float seed = Random.Range(0f, 1f);
            if(seed < probability) // Displace a point
            {
                // Clamp gaussian distribution
                do
                {
                    percentile = DrawFromGaussian(1f, 0.5f);
                }
                while (percentile < 0f || percentile > 1f);

                // Lerp between inner and outer bound with percentile drawn from normal distribution
                Vector2 outerBound = path[i];
                concaveHull.Add(Vector2.Lerp(innerBound, outerBound, percentile));
            }
            else // Do not displace a point
            {
                concaveHull.Add(path[i]);
            }
        }
        path = concaveHull.ToArray();
    }

    private float DrawFromGaussian(float stdDev, float mean)
    {
        float v1, v2, s;
        do
        {
            // Draw two random numbers from a uniform distribution
            v1 = 2f * Random.Range(0f, 1f) - 1f;
            v2 = 2f * Random.Range(0f, 1f) - 1f;

            // Check if they are within the unit circle
            s = v1 * v1 + v2 * v2;
        } while (s >= 1 || s == 0);

        // https://en.wikipedia.org/wiki/Marsaglia_polar_method
        s = Mathf.Sqrt((-2f * Mathf.Log(s)) / s);
        return stdDev * v1 * s + mean;
    }

    private void ChaikinSmoothing()
    {
        // http://graphics.cs.ucdavis.edu/education/CAGDNotes/Chaikins-Algorithm/Chaikins-Algorithm.html
        Vector2[] smoothedPath = new Vector2[2 * path.Count()];
        for (int i = 0; i < path.Count(); i++)
        {
            int nextIndex = (i + 1) % path.Count();
            int prevIndex = i > 0 ? i - 1 : i - 1 + path.Count();
            smoothedPath[i * 2] = Vector2.Lerp(path[prevIndex], path[i], 0.75f);
            smoothedPath[(i * 2) + 1] = Vector2.Lerp(path[i], path[nextIndex], 0.25f);
        }
        path = smoothedPath;
    }
}


public class RaceTrackMeshCreator
{
    public static Mesh GenerateMesh(Vector2[] path, float roadWidth)
    {
        // https://www.youtube.com/watch?v=Q12sb-sOhdI&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=6
        // For points with index > 0 take the average of directions
        // between next and previous to make the path smoother
        Vector3[] vertices = new Vector3[path.Length * 2];
        int[] meshTriangles = new int[2 * (path.Length - 1) * 3];
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
            // First triangle
            meshTriangles[triangleIndex] = vertexIndex;
            meshTriangles[triangleIndex + 1] = vertexIndex + 2;
            meshTriangles[triangleIndex + 2] = vertexIndex + 1;

            // Second triangle
            meshTriangles[triangleIndex + 3] = vertexIndex + 1;
            meshTriangles[triangleIndex + 4] = vertexIndex + 2;
            meshTriangles[triangleIndex + 5] = vertexIndex + 3;

            vertexIndex += 2;
            triangleIndex += 6;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        return mesh;
    }
}