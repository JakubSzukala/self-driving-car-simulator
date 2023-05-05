using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaceTrack : MonoBehaviour
{
    private Vector2[] path; // TODO: I am not sure if this should be stored here
    private IPathCreator raceTrackPathCreator;
    private IRaceTrackFullRenderable raceTrackRenderer;
    [SerializeField] private CheckPointSpawner checkPointSpawner;
    [SerializeField] public GameObject checkPointContainer;
    public UnityEvent checkpointReached;

    void Awake()
    {
        raceTrackPathCreator = GetComponentInChildren<IPathCreator>();
        raceTrackRenderer = GetComponentInChildren<IRaceTrackFullRenderable>();
    }

    // TODO: Make so that values can be set in race track and then passed
    // down the hierarchy, values like race track width, wall height, concavity etc
    public void CreateRaceTrack(bool addCheckPoints)
    {
        bool renderIsValid;
        do
        {
            path = raceTrackPathCreator.CreatePath();
            raceTrackRenderer.PrepareTrackRender(path);
            renderIsValid = raceTrackRenderer.IsTrackRenderValid();
        }
        while (!renderIsValid);
        raceTrackRenderer.RenderTrack();

        // Destroy leftover checkpoints
        if (checkPointContainer.transform.childCount > 0)
        {
            DestroyRaceTrackCheckPoints();
        }

        if (addCheckPoints)
        {
            CreateRaceTrackCheckPoints();
        }
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

    public void CreateRaceTrackCheckPoints()
    {
        for (int i = 0; i < path.Length; i++)
        {
            int nextIndex = (i + 1) % path.Length;
            Vector3 pointXZ = new Vector3(path[i].x, 0, path[i].y);
            Vector3 direction = path[nextIndex] - path[i];
            direction = new Vector3(direction.x, 0, direction.y);
            direction.Normalize();
            float width = GetComponentInChildren<RoadRenderer>().roadWidth;
            checkPointSpawner.SpawnCheckPoint(pointXZ, direction, width);
        }
    }

    public void DestroyRaceTrackCheckPoints()
    {
        for (int i = 0; i < checkPointContainer.transform.childCount; i++)
        {
            Destroy(checkPointContainer.transform.GetChild(i).gameObject);
        }
    }
}
