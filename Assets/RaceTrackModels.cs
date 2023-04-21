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

    public Vector2[] GenerateConcavePath(int nPointsToWrap, float concavePointsPercentage)
    {
        // Validation
        if (nPointsToWrap < 3)
        {
            throw new System.ArgumentException("Number of points must be greater than 2.");
        }
        if (concavePointsPercentage < 0f || concavePointsPercentage > 1f)
        {
            throw new System.ArgumentException("Percentage of concave points must be between [0, 1]");
        }

        // Logic
        GenerateRandomPoints(nPointsToWrap);
        CalculateConvexPath();
        CalculateConcavePath(concavePointsPercentage);
        return path;
    }

    private void GenerateRandomPoints(int numberOfPoints)
    {
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

    private void CalculateConcavePath(float concavePointsPercentage)
    {
        List<Vector2> concaveHull = new List<Vector2>();

        int concavePointsN = Mathf.CeilToInt(path.Length * concavePointsPercentage);
        HashSet<int> concavePointsIndexes = GetUniqueRandomIndexes(concavePointsN, path.Length);

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
            if(concavePointsIndexes.Contains(i)) // Displace a point
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

    private HashSet<int> GetUniqueRandomIndexes(int indexesN, int length)
    {
        HashSet<int> randomIndexes = new HashSet<int>();
        for (int i = 0; i < indexesN; i++)
        {
            int randomInt = Mathf.CeilToInt(Random.Range(0, length));
            while(randomIndexes.Contains(randomInt))
            {
                randomInt = Mathf.CeilToInt(Random.Range(0, length));
            }
            randomIndexes.Add(randomInt);
        }
        return randomIndexes;
    }
}