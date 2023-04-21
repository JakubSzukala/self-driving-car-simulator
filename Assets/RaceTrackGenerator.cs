using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RaceTrackGenerator : MonoBehaviour
{
    public PathAnchorPointsGenerator model;
    public IRaceTrackRenderer[] views;
    private IPathSmoothing pathSmoother;

    private Texture2D texture;
    [SerializeField] public int rangeX = 100;
    [SerializeField] public int rangeY = 100;
    [SerializeField] public int numberOfPoints = 3;
    [SerializeField] public float pointConcavityProbability = 0.7f;
    [SerializeField] public int smoothingDegree = 1;
    private Vector2[] path;

    void Start()
    {
        // Create refs
        model = new PathAnchorPointsGenerator(rangeX, rangeY);
        views = GetComponents<IRaceTrackRenderer>();
        pathSmoother = new BezierSmoothing();
        Regenerate();
    }

    void Update()
    {
        foreach(var view in views)
        {
            view.RenderTrack(path);
        }
    }

    public void Regenerate()
    {
        // Track generation
        model.rangeX = rangeX; // Set range in which path will be generated
        model.rangeY = rangeY;
        path = model.GenerateConcavePath(
            numberOfPoints, pointConcavityProbability);

        path = pathSmoother.Smooth(path); // TODO: make it into a component?

        foreach(var view in views)
        {
            view.RenderTrack(path);
        }
    }

    public void OnClick()
    {
        Regenerate();
    }
}