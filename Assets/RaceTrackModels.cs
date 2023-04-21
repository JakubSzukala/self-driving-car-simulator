using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathAnchorPointsGenerator
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

    public PathAnchorPointsGenerator(int rangeX, int rangeY)
    {
        this.rangeX = rangeX;
        this.rangeY = rangeY;
    }

    public Vector2[] GenerateConvexPath(int nPointsToWrap)
    {
        // Validation
        if (nPointsToWrap < 3)
        {
            throw new System.ArgumentException("Number of points must be greater than 2.");
        }

        // Logic
        GenerateRandomPoints(nPointsToWrap);
        CalculateConvexPath();
        return path;
    }

    public Vector2[] GenerateConcavePath(int nPointsToWrap, float pointConcavityProb)
    {
        // Validation
        if (nPointsToWrap < 3)
        {
            throw new System.ArgumentException("Number of points must be greater than 2.");
        }
        if (pointConcavityProb < 0f || pointConcavityProb > 1f)
        {
            throw new System.ArgumentException("Concativity probability must be between 0f and 1f");
        }

        // Logic
        GenerateRandomPoints(nPointsToWrap);
        CalculateConvexPath();
        CalculateConcavePath(pointConcavityProb);
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
}