using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierSmoothing : MonoBehaviour, IPathSmoothing
{
    BezierPath path;
    public Vector2[] Smooth(Vector2[] path)
    {
        this.path = new BezierPath(path, true);
        this.path.AllControlPointsAutoSet();
        return this.path.CalculateEvenlySpacedPoints(4f);
    }
}
