using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
[RequireComponent(typeof(IRaceTrackRenderer))]
[RequireComponent(typeof(IPathSmoothing))]
public class RaceTrackGenerator : MonoBehaviour
{
    public PathAnchorPointsGenerator model;
    public IRaceTrackRenderer[] views;
    public IPathSmoothing pathSmoother;

    [SerializeField] private int rangeX = 100;
    [SerializeField] private int rangeY = 100;
    [SerializeField] private int numberOfPoints = 3;
    [SerializeField] private float concavePointsPercentage = 0.25f;
    private Vector2[] path;
    private Vector2 raceTrackStart;
    private Vector2 raceDirection;

    void Awake()
    {
        // Create refs
        model = new PathAnchorPointsGenerator(rangeX, rangeY);
        views = GetComponents<IRaceTrackRenderer>();
        pathSmoother = GetComponent<IPathSmoothing>();
    }

    public void Regenerate()
    {
        // Track generation
        model.rangeX = rangeX; // Set range in which path will be generated
        model.rangeY = rangeY;

        // Find a path that is valid and renders correctly
        bool areRendersValid;
        do
        {
            areRendersValid = true;
            path = model.GenerateConcavePath(
                numberOfPoints, concavePointsPercentage);

            path = pathSmoother.Smooth(path);

            // Prepare renders and check if they are valid
            foreach(var view in views)
            {
                view.PrepareTrackRender(path);
                areRendersValid &= view.IsTrackRenderValid();
            }
        }
        while (!areRendersValid);

        // If all renders are valid, then render them all
        views.ToList().ForEach(v => v.RenderTrack());

        // Set the start and race direction
        raceTrackStart = path[0];
        raceDirection = path[0] - path[1];
        raceDirection.Normalize();
    }

    public void GetStart(out Vector2 start, out Vector2 direction)
    {
        start = raceTrackStart;
        direction = raceDirection;
    }

    public void OnClick()
    {
        Regenerate();
    }
}