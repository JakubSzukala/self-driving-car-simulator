using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RaceTrackGenerator : MonoBehaviour
{
    public RaceTrackGeneratorModel model;
    public RaceTrackGeneratorView view;

    private RawImage map;
    private Texture2D texture;
    [SerializeField] public int mapSizeX = 100;
    [SerializeField] public int mapSizeY = 100;
    [SerializeField] public int numberOfPoints = 3;

    void Start()
    {
        // Create refs
        model = new RaceTrackGeneratorModel(mapSizeX, mapSizeY, numberOfPoints);
        view = new RaceTrackGeneratorView(Color.green);
        map = GetComponent<RawImage>();

        // Create texture with proper filter
        texture = new Texture2D(mapSizeX, mapSizeY);
        texture.filterMode = FilterMode.Point;

        Regenerate();
    }

    public void Regenerate()
    {
        // Track generation
        model.numberOfPoints = numberOfPoints;
        model.rangeX = mapSizeX;
        model.rangeY = mapSizeY;
        model.GenerateTrack();

        // Draw hull from smoothed points and then the points before smoothing
        texture.Reinitialize(mapSizeX, mapSizeY);
        view.TextureFillWhite(ref texture);
        view.drawColor = Color.blue;
        view.TextureDrawHull(model.smoothedPoints, ref texture);
        view.drawColor = Color.red;
        view.TextureDrawPoints(model.concaveHull, ref texture);
        view.drawColor = Color.cyan;
        view.TextureDrawPoints(model.smoothedPoints, ref texture);
        /*
        view.drawColor = Color.blue;
        view.TextureDrawHull(model.concaveHull, ref texture);
        view.drawColor = Color.green;
        view.TextureDrawPoints(model.orthogonalPoints, ref texture);
        view.drawColor = Color.red;
        view.TextureDrawPoints(model.concaveHull, ref texture);
        */
        //view.TextureDrawHull(model.convexHull, ref texture);

        // Set the final result
        map.texture = texture;
    }

    public void OnClick()
    {
        Regenerate();
    }
}