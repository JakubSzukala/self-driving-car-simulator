using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRaceTrackRenderer
{
    void PrepareTrackRender(Vector2[] path);

    bool IsTrackRenderValid();

    void RenderTrack();
}

public interface IPathSmoothing
{
    Vector2[] Smooth(Vector2[] path);
}