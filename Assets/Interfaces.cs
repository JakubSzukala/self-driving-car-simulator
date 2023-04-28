using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRaceTrackFullRenderable
{
    void PrepareTrackRender(Vector2[] path);

    bool IsTrackRenderValid();

    void RenderTrack();
}

public interface IPathCreator
{
    Vector2[] CreatePath();
}

public interface IPathSmoothing
{
    Vector2[] Smooth(Vector2[] path);
}

public interface IRaceTrackPartRenderable
{
    void Initialize(int pathLength);

    void AddVertices(Vector2 pathPoint, Vector2 forward);

    void SetUpMesh();

    void Render();

    void Reset();
}