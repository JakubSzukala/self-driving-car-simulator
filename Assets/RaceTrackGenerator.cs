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
    [SerializeField] public int rangeX = 100;
    [SerializeField] public int rangeY = 100;
    [SerializeField] public int numberOfPoints = 3;
    [SerializeField] public float concavePointsPercentage = 0.25f;
    [SerializeField] public int smoothingDegree = 1;
    private Vector2[] path;

    void Start()
    {
        // Create refs
        model = new PathAnchorPointsGenerator(rangeX, rangeY);
        views = GetComponents<IRaceTrackRenderer>();
        pathSmoother = GetComponent<IPathSmoothing>();
        Regenerate();
    }

    void Update()
    {
        //foreach(var view in views)
        //{
            //view.RenderTrack(path);
        //}

        if (Input.GetMouseButtonDown(0))
        {
            Regenerate();
        }
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