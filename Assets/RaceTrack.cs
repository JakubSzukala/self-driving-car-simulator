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

    public void CreateRaceTrack()
    {
        Vector2[] path = raceTrackPathCreator.CreatePath();

        bool renderIsValid;
        do
        {
            raceTrackRenderer.PrepareTrackRender(path);
            renderIsValid = raceTrackRenderer.IsTrackRenderValid();
        } while (!renderIsValid);
        raceTrackRenderer.RenderTrack();
    }

    public void GetRaceTrackStart(out Vector3 start, out Vector3 direction)
    {
        Vector2 startXY, directionXY;
        raceTrackPathCreator.GetStart(out startXY, out directionXY);
        Vector3 startXZ = new Vector3(startXY.x, 2, startXY.y);
        Vector3 directionXZ = new Vector3(directionXY.x, 0, directionXY.y);
        start = startXZ;
        direction = directionXZ;
    }
}
