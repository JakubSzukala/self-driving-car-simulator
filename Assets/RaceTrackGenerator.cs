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

    private Texture2D texture;
    [SerializeField] private int rangeX = 100;
    [SerializeField] private int rangeY = 100;
    [SerializeField] private int numberOfPoints = 3;
    [SerializeField] private float concavePointsPercentage = 0.25f;
    private Vector2[] path;

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
    }

    public void OnClick()
    {
        Regenerate();
    }
}