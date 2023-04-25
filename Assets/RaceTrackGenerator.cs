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
        foreach(var view in views)
        {
            view.RenderTrack(path);
        }

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
        path = model.GenerateConcavePath(
            numberOfPoints, concavePointsPercentage);

        path = pathSmoother.Smooth(path);

        // Render in all available views
        foreach(var view in views)
        {
            view.RenderTrack(path);
            RaceTrackGenerator3DView temp = view as RaceTrackGenerator3DView;
            if (temp)
            {
                Vector2[] overlaps = RaceTrackMeshArtifactDetector.FindRaceTrackMeshOverlaps(temp.temp);
                if (overlaps.Any())
                {
                    Debug.Log("OMG OVERLAP");
                    foreach(Vector2 overlapPosition in overlaps)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = overlapPosition;
                    }
                    //sphere.transform()
                }
            }
        }
    }

    public void OnClick()
    {
        Regenerate();
    }
}