using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RaceTrackGenerator))]
[RequireComponent(typeof(RawImage))]
public class RaceTrackGenerator2DView : MonoBehaviour, IRaceTrackRenderer
{
    private RawImage target;
    public int sizeX = 100;
    public int sizeY = 100;
    public Color pointsColor
    { get; set; } = Color.red;
    public Color pathColor
    { get; set; } = Color.blue;

    void Start()
    {
        this.target = GetComponent<RawImage>();
        if (this.target == null)
        {
            throw new System.ArgumentException("Target RawImage is null.");
        }
    }

    public void RenderTrack(Vector2[] path)
    {
        Texture2D texture = new Texture2D(sizeX, sizeY);
        texture.filterMode = FilterMode.Point;
        TextureFillWhite(ref texture);
        TextureDrawHull(path, ref texture);
        TextureDrawPoints(path, ref texture);
        target.texture = texture;
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
            texture.SetPixel((int)points[i].x, (int)points[i].y, pointsColor);
        }
        texture.Apply();
    }

    public void TextureDrawHull(Vector2[] points, ref Texture2D texture)
    {
        if (points == null || points.Length < 1 || texture == null)
        {
            throw new System.ArgumentException("Invalid arguments.");
        }

        float frac = 1f / points.Length;
        float colorPercentage = -frac;
        for (int i = 0; i < points.Length; i++)
        {
            colorPercentage += frac;
            int nextIndex = (i + 1) % points.Length;
            Color color = Color.Lerp(Color.black, pathColor, colorPercentage);
            DrawLine(points[i], points[nextIndex], color, ref texture);
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
