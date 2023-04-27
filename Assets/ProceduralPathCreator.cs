using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(IPathSmoothing))]
public class ProceduralPathCreator : MonoBehaviour, IPathCreator
{
    public PathAnchorPointsGenerator model;
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
        pathSmoother = GetComponent<IPathSmoothing>();
    }

    public void Regenerate()
    {
        // Track generation
        model.rangeX = rangeX; // Set range in which path will be generated
        model.rangeY = rangeY;

        // Find a path that is valid and renders correctly
        path = model.GenerateConcavePath(
            numberOfPoints, concavePointsPercentage);

        path = pathSmoother.Smooth(path);

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

    public Vector2[] CreatePath()
    {
        Regenerate();
        return path;
    }
}