using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackGeneratorView
{
    public Color drawColor
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

    public void TextureDrawPoints(IReadOnlyList<Vector2> points, ref Texture2D texture)
    {
        if (points == null || points.Count < 1 || texture == null)
        {
            throw new System.ArgumentException("Invalid arguments.");
        }

        for (int i = 0; i < points.Count; i++)
        {
            texture.SetPixel((int)points[i].x, (int)points[i].y, drawColor);
        }
        texture.Apply();
    }

    public void TextureDrawHull(List<Vector2> points, ref Texture2D texture)
    {
        float frac = 1f / points.Count;
        float colorPercentage = -frac;
        for (int i = 0; i < points.Count; i++)
        {
            colorPercentage += frac;
            int nextIndex = (i + 1) % points.Count;
            Color color = Color.Lerp(Color.black, drawColor, colorPercentage);
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
