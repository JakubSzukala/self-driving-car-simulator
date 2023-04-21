using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRaceTrackRenderer
{
    void RenderTrack(Vector2[] path);
}

public interface IPathSmoothing
{
    Vector2[] Smooth(Vector2[] path);
}