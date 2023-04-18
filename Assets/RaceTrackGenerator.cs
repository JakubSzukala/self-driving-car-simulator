using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RaceTrackGenerator : MonoBehaviour
{
    public RaceTrackPathGenerator model;
    public IRaceTrackRenderer view;

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
        model = new RaceTrackPathGenerator(rangeX, rangeY);
        view = GetComponent<IRaceTrackRenderer>();

        Regenerate();
    }

    void Update()
    {
        view.RenderTrack(path);
    }

    public void Regenerate()
    {
        // Track generation
        model.rangeX = rangeX; // Set range in which path will be generated
        model.rangeY = rangeY;
        path = model.GenerateConcavePath(
            numberOfPoints, pointConcavityProbability, smoothingDegree);

        view.RenderTrack(path);
    }

    public void OnClick()
    {
        Regenerate();
    }
}