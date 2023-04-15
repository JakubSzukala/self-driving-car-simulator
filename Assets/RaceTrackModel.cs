using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaceTrackGeneratorModel
{
    public int numberOfPoints = 3; // TODO: Convert to property
    public Vector2[] points; // TODO: Convert to property
    public List<Vector2> convexHull; // TODO: Convert to property
    public List<Vector2> concaveHull; // TODO: Convert to property
    public Vector2[] orthogonalPoints;
    public int rangeX;
    public int rangeY;

    public RaceTrackGeneratorModel(int rangeX, int rangeY, int numberOfPoints)
    {
        if (rangeX < 1 || rangeY < 1 || numberOfPoints < 3)
        {
            throw new System.ArgumentException("Invalid model arguments.");
        }

        this.rangeX = rangeX;
        this.rangeY = rangeY;
        this.numberOfPoints = numberOfPoints;
        GenerateTrack();
    }

    public void GenerateTrack()
    {
        GenerateRandomPoints(numberOfPoints);
        GenerateConvexHull();
        ConvexToConcave(0.5f);
        GenerateOrthogonalPoints(3f);
    }

    private void GenerateRandomPoints(int numberOfPoints)
    {
        if(numberOfPoints < 3)
        {
            throw new System.ArgumentException("Invalid number of points.");
        }

        points = new Vector2[numberOfPoints];
        for(int i = 0; i < numberOfPoints; i++)
        {
            points[i] = new Vector2(Random.Range(0, rangeX), Random.Range(0, rangeY));
        }

        // Consider only unique points
        points = points.Distinct().ToArray();
    }

    // TODO: Add return value indicating success
    private void GenerateConvexHull()
    {
        if (points == null || points.Length < 3)
        {
            throw new System.ArgumentException("Invalid fields values.");
        }

        // Find the leftmost point, it is guaranteed to be on the hull
        convexHull = new List<Vector2>();
        int mostLeftPointIdx = 0;
        foreach(var point in points)
        {
            if(point.x < points[mostLeftPointIdx].x)
            {
                mostLeftPointIdx = System.Array.IndexOf(points, point);
            }
        }
        convexHull.Add(points[mostLeftPointIdx]);

        // Iterate over the points and determine which lay on the convex hull
        int currentHullPointIdx = mostLeftPointIdx;
        int counter = -1; // Just a safety measure to prevent infinite loop
        do
        {
            counter++;
            // Pick a point (make sure it is different from the current hull point)
            int smallestAnglePointIdx = (currentHullPointIdx + 1) % points.Length;
            Vector2 smallestAnglePointDir = points[smallestAnglePointIdx] - points[currentHullPointIdx];
            for (int i = 0; i < points.Length; i++)
            {
                // Do not check with self and current smallest angle point
                if (i == currentHullPointIdx || i == smallestAnglePointIdx)
                {
                    continue;
                }
                // Point most to the left will have the greatest signed angle value
                Vector2 currentPointDir = points[i] - points[currentHullPointIdx];
                if (Vector2.SignedAngle(smallestAnglePointDir, currentPointDir) > 0f)
                {
                    smallestAnglePointIdx = i;
                    smallestAnglePointDir = points[i] - points[currentHullPointIdx];
                }
            }
            // This condition being true indicates that all convex hull points were found
            if (smallestAnglePointIdx == mostLeftPointIdx) break;

            convexHull.Add(points[smallestAnglePointIdx]);
            currentHullPointIdx = smallestAnglePointIdx;
        } while (counter < points.Length);
    }

    public void ConvexToConcave(float probability)
    {
        if (probability < 0f || probability > 1f)
        {
            throw new System.ArgumentException("Invalid probability value.");
        }
        concaveHull = new List<Vector2>();

        // Calculate center of mass of a convex hull
        Vector2 accumulate = Vector2.zero;
        foreach(var point in convexHull)
        {
            accumulate += point;
        }
        // Lower bound is the center of mass
        Vector2 innerBound = accumulate / convexHull.Count;

        // For randomly picked points on the hull displace them towards center of mass
        float percentile;
        for(int i = 0; i < convexHull.Count; i++)
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
                Vector2 outerBound = convexHull[i];
                concaveHull.Add(Vector2.Lerp(innerBound, outerBound, percentile));
            }
            else // Do not displace a point
            {
                concaveHull.Add(convexHull[i]);
            }
        }
    }

    private void GenerateOrthogonalPoints(float distance)
    {
        // https://www.youtube.com/watch?v=Q12sb-sOhdI&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=6
        // For points with index > 0 take the average of directions
        // between next and previous to make the path smoother
        orthogonalPoints = new Vector2[concaveHull.Count * 2];
        for (int i = 0; i < concaveHull.Count; i++)
        {
            // Overflow safe, will overflow to 0
            int nextIndex = (i + 1) % concaveHull.Count;
            Vector2 forward = Vector2.zero;
            if (i > 0)
            {
                // If not first point consider also previous
                forward += concaveHull[i] - concaveHull[i - 1];
            }
            forward += concaveHull[nextIndex] - concaveHull[i];
            forward.Normalize();

            Vector2 left = new Vector2(forward.y, -forward.x) * distance;
            Vector2 right = new Vector2(-forward.y, forward.x) * distance;

            orthogonalPoints[i * 2] = concaveHull[i] + left;
            orthogonalPoints[i * 2 + 1] = concaveHull[i] + right;
        }
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
