using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Credits for the Bezier and BezierPath classes:
* https://www.youtube.com/watch?v=d9k97JemYbM&list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP&index=5
* https://github.com/SebLague/Curve-Editor
*/

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
    private bool isClosed;

    public BezierPath(Vector2[] anchorPoints, bool isClosed)
    {
        if (anchorPoints.Length < 2)
        {
            throw new System.ArgumentOutOfRangeException("Number of anchor points must be greater than 1.");
        }

        this.points = new List<Vector2>();
        this.isClosed = isClosed;
        AddFirstSegment(anchorPoints[0], anchorPoints[1]);
        for (int i = 2; i < anchorPoints.Length; i++)
        {
            AddSegment(anchorPoints[i]);
        }
        if (isClosed)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add(points[0] * 2 - points[1]);
        }
    }

    public BezierPath()
    {
        this.points = new List<Vector2>();
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int PointsN
    {
        get
        {
            return points.Count;
        }
    }

    public int SegmentsN
    {
        get
        {
            return (points.Count - 4) / 3 + (isClosed ? 2 : 1);
        }
    }

    public bool IsClosed
    {
        get { return isClosed; }
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
            points[LoopIndex(i * 3 + 3)]
            };
        }
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 prevPoint = points[0];
        float distanceSinceLastEvenPoint = 0;
        foreach(var segmentPoints in Segments())
        {
            float controlNetLength = Vector2.Distance(segmentPoints[0], segmentPoints[1]) + Vector2.Distance(segmentPoints[1], segmentPoints[2]) + Vector2.Distance(segmentPoints[2], segmentPoints[3]);
            float estimatedCurveLength = Vector2.Distance(segmentPoints[0], segmentPoints[3]) + controlNetLength / 2f;
            float divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t);
                distanceSinceLastEvenPoint += Vector2.Distance(prevPoint, pointOnCurve);
                while (distanceSinceLastEvenPoint >= spacing)
                {
                    float overshoot = distanceSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (prevPoint - pointOnCurve).normalized * overshoot;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    distanceSinceLastEvenPoint = overshoot;
                    prevPoint = newEvenlySpacedPoint;
                }
                prevPoint = pointOnCurve;
            }
        }
        return evenlySpacedPoints.ToArray();
    }

    public void ControlPointsAutoSet(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
		if (anchorIndex + 3 >= 0 || isClosed)
		{
			Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
			dir -= offset.normalized;
			neighbourDistances[1] = -offset.magnitude;
		}

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
    }

    public void AllControlPointsAutoSet()
    {
        for (int i = 0; i < points.Count; i+=3)
        {
            ControlPointsAutoSet(i);
        }
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}