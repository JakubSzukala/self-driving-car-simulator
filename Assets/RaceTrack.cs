using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrack : MonoBehaviour
{
    private IPathCreator raceTrackPathCreator;
    private IRaceTrackFullRenderable raceTrackRenderer;

    private Vector2[] path;

    void Awake()
    {
        raceTrackPathCreator = GetComponentInChildren<IPathCreator>();
        raceTrackRenderer = GetComponentInChildren<IRaceTrackFullRenderable>();
    }

    void Start()
    {
        Vector2[] path = raceTrackPathCreator.CreatePath();
        raceTrackRenderer.PrepareTrackRender(path);
        raceTrackRenderer.RenderTrack();
    }
}
