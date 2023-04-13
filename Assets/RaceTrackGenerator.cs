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
        view = new RaceTrackGeneratorView();
        map = GetComponent<RawImage>();

        // Create texture with proper filter
        texture = new Texture2D(mapSizeX, mapSizeY);
        texture.filterMode = FilterMode.Point;

        // Draw points directly to the texture
        view.DrawConvexHull(model.convexHull, ref texture, true);
        view.TextureDrawPoints(model.points, ref texture, false);

        // Set the final result
        map.texture = texture;
    }

    void Update()
    {

    }

    public void OnClick()
    {
        // Track generation
        model.numberOfPoints = numberOfPoints;
        model.GenerateTrack();

        // Draw points directly to the texture
        view.DrawConvexHull(model.convexHull, ref texture, true);
        view.TextureDrawPoints(model.points, ref texture, false);

        // Set the final result
        map.texture = texture;
    }
}

public class RaceTrackGeneratorModel
{
    public int numberOfPoints = 3; // TODO: Convert to property
    public Vector2[] points; // TODO: Convert to property
    public List<Vector2> convexHull; // TODO: Convert to property
    private int rangeX;
    private int rangeY;

    public RaceTrackGeneratorModel(int rangeX, int rangeY, int numberOfPoints)
    {
        this.rangeX = rangeX;
        this.rangeY = rangeY;
        this.numberOfPoints = numberOfPoints;
        GenerateTrack();
    }

    public void GenerateTrack()
    {
        GenerateRandomPoints(numberOfPoints);
        GenerateConvexHull();
    }

    private void GenerateRandomPoints(int numberOfPoints)
    {
        if(numberOfPoints < 3)
        {
            Debug.LogError("Number of points must be greater than 2");
            points = null;
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
}


public class RaceTrackGeneratorView
{
    // TODO: Create a color property and refer to it when drawing
    public void TextureDrawPoints(Vector2[] points, ref Texture2D texture, bool whiteBg)
    {
        if (whiteBg) // TODO: Change this to Texture2D.whiteTexture
        {
            Color[] whiteBackground = new Color[texture.width * texture.height];
            System.Array.Fill<Color>(whiteBackground, new Color(1f, 1f, 1f));
            texture.SetPixels(0, 0, texture.width, texture.height, whiteBackground);
            texture.Apply();
        }

        for (int i = 0; i < points.Length; i++)
        {
            texture.SetPixel((int)points[i].x, (int)points[i].y, Color.red);
        }
        texture.Apply();
    }

    public void DrawConvexHull(List<Vector2> points, ref Texture2D texture, bool whiteBg)
    {
        if(whiteBg)
        {
            Color[] whiteBackground = new Color[texture.width * texture.height];
            System.Array.Fill<Color>(whiteBackground, new Color(1f, 1f, 1f));
            texture.SetPixels(0, 0, texture.width, texture.height, whiteBackground);
            texture.Apply();
        }

        float frac = 1f / points.Count;
        float bluePercentage = -frac;
        for (int i = 1; i < points.Count; i++)
        {
            bluePercentage += frac;
            int index = i % (points.Count - 1);
            int prevIndex = (i - 1) % (points.Count - 1);
            Color color = new Color(0f, 0f, bluePercentage);
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
