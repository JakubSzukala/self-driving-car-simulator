using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaikinSmoothing : MonoBehaviour
{
    public Vector2[] Smooth(Vector2[] path)
    {
        Debug.LogWarning("Chaikin Smoothing will create artifacts on the track, especially if walls are added.");
        Vector2[] smoothedPath = new Vector2[2 * path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            int nextIndex = (i + 1) % path.Length;
            int prevIndex = i > 0 ? i - 1 : i - 1 + path.Length;
            smoothedPath[i * 2] = Vector2.Lerp(path[prevIndex], path[i], 0.75f);
            smoothedPath[(i * 2) + 1] = Vector2.Lerp(path[i], path[nextIndex], 0.25f);
        }
        return smoothedPath;
    }
}
