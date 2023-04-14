using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RaceTrackGenerator : MonoBehaviour
{
    public RaceTrackGeneratorModel model;
    public RaceTrackGeneratorView view;

    private RawImage map;
    private Texture2D texture;
    [SerializeField] public int mapSizeX = 100;
    [SerializeField] public int mapSizeY = 100;
    [SerializeField] public int numberOfPoints = 3;

    void Start()
    {
        // Create refs
        model = new RaceTrackGeneratorModel(mapSizeX, mapSizeY, numberOfPoints);
        view = new RaceTrackGeneratorView(Color.green);
        map = GetComponent<RawImage>();

        // Create texture with proper filter
        texture = new Texture2D(mapSizeX, mapSizeY);
        texture.filterMode = FilterMode.Point;

        Regenerate();
    }

    public void Regenerate()
    {
        // Track generation
        model.numberOfPoints = numberOfPoints;
        model.rangeX = mapSizeX;
        model.rangeY = mapSizeY;
        model.GenerateTrack();

        // Draw points directly to the texture
        texture.Reinitialize(mapSizeX, mapSizeY);
        view.TextureFillWhite(ref texture);
        //view.TextureDrawHull(model.convexHull, ref texture);
        view.TextureDrawHull(model.concaveHull, ref texture);
        view.TextureDrawPoints(model.points, ref texture);

        // Set the final result
        map.texture = texture;
    }

    public void OnClick()
    {
        Regenerate();
    }
}

public class RaceTrackGeneratorModel
{
    public int numberOfPoints = 3; // TODO: Convert to property
    public Vector2[] points; // TODO: Convert to property
    public List<Vector2> convexHull; // TODO: Convert to property
    public List<Vector2> concaveHull; // TODO: Convert to property
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
            convexHull.Add(points[smallestAnglePointIdx]);
            currentHullPointIdx = smallestAnglePointIdx;
        } while (currentHullPointIdx != mostLeftPointIdx && counter < points.Length);
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

        // For randomly picked points on the hull displace them towards or against center of mass
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


public class RaceTrackGeneratorView
{
    Color drawColor
    { get; set; }

    public RaceTrackGeneratorView(Color drawColor = default(Color))
    {
        this.drawColor = drawColor;
    }

    public void TextureFillWhite(ref Texture2D texture)
    {
        if (texture == null)
        {
            throw new System.ArgumentException("Texture is null.");
        }

        Color[] background = new Color[texture.width * texture.height];
        System.Array.Fill<Color>(background, Color.white);
        texture.SetPixels(0, 0, texture.width, texture.height, background);
        texture.Apply();
    }

    public void TextureDrawPoints(Vector2[] points, ref Texture2D texture)
    {
        if (points == null || points.Length < 1 || texture == null)
        {
            throw new System.ArgumentException("Invalid arguments.");
        }

        for (int i = 0; i < points.Length; i++)
        {
            texture.SetPixel((int)points[i].x, (int)points[i].y, drawColor);
        }
        texture.Apply();
    }

    public void TextureDrawHull(List<Vector2> points, ref Texture2D texture)
    {
        float frac = 1f / points.Count;
        float colorPercentage = -frac;
        for (int i = 1; i < points.Count; i++)
        {
            colorPercentage += frac;
            int index = i % (points.Count - 1);
            int prevIndex = (i - 1) % (points.Count - 1);
            Color color = Color.Lerp(Color.black, drawColor, colorPercentage);
            DrawLine(points[index], points[prevIndex], color, ref texture);
        }
        texture.Apply();
    }

    private void DrawLine(Vector2 p1, Vector2 p2, Color color, ref Texture2D texture)
    {
        Vector2 brush = p1;
        float brushIncrement = 1/Mathf.Sqrt (Mathf.Pow (p2.x - p1.x, 2) + Mathf.Pow (p2.y - p1.y, 2));
        float linePercentile = 0;
        while(brush.x != p2.x || brush.y != p2.y)
        {
            brush = Vector2.Lerp(p1, p2, linePercentile);
            texture.SetPixel((int)brush.x, (int)brush.y, color);
            linePercentile += brushIncrement;
        }
    }
}
