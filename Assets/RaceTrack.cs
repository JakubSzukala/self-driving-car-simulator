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

        // Shift spawn point forward so agent won't spawn on checkpoint
        startXY = Vector2.Lerp(path[0], path[1], 0.5f);
        directionXY = path[1] - startXY;

        // Convert from XY plane to XZ plane
        Vector3 startXZ = new Vector3(startXY.x, 0, startXY.y);
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

    public float GetCheckPointsSpacing()
    {
        return Vector2.Distance(path[0], path[1]);
    }

    public void DestroyRaceTrackCheckPoints()
    {
        for (int i = 0; i < checkPointContainer.transform.childCount; i++)
        {
            Destroy(checkPointContainer.transform.GetChild(i).gameObject);
        }
    }
}
