using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = EvaluateQuadratic(a, b, c, t);
        Vector2 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }
}

public class BezierPath
{
    private List<Vector2> points;

    public BezierPath(Vector2[] anchorPoints)
    {
        if (anchorPoints.Length < 2)
        {
            throw new System.ArgumentOutOfRangeException("Number of anchor points must be greater than 1.");
        }
    }

    public BezierPath()
    {
        this.points = new List<Vector2>();
    }

    public int SegmentsN
    {
        get
        {
            return (points.Count - 4) / 3 + 1;
        }
    }

    public void AddFirstSegment(Vector2 anchorPoint1, Vector2 anchorPoint2)
    {
        if (points.Count != 0)
        {
            throw new System.ArgumentException("First segment already exists. Use AddSegment method instead.");
        }

        Vector2 center = Vector2.Lerp(anchorPoint1, anchorPoint2, 0.5f);
        float r = Vector2.Distance(anchorPoint1, center);
        points.Add(anchorPoint1);
        points.Add(center + (Vector2.left + Vector2.up) * 0.5f * r);
        points.Add(center + (Vector2.right + Vector2.down) * 0.5f * r);
        points.Add(anchorPoint2);
    }

    public void AddSegment(Vector2 anchorPoint)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add(Vector2.Lerp(points[points.Count - 1], anchorPoint, 0.5f));
        points.Add(anchorPoint);
    }

    public IEnumerable<Vector2[]> Segments()
    {
        for (int i = 0; i < SegmentsN; i++)
        {
            yield return new Vector2[] {
            points[i * 3],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[i * 3 + 3]
            };
        }
    }

}