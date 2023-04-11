using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class RaceTrackGenerator : MonoBehaviour
{
    public RaceTrackGeneratorModel model;
    public RaceTrackGeneratorView view;

    private RawImage map;
    [SerializeField] public int mapSizeX = 100;
    [SerializeField] public int mapSizeY = 100;

    void Start()
    {
        // Create refs
        model = new RaceTrackGeneratorModel(mapSizeX, mapSizeY);
        view = new RaceTrackGeneratorView();
        map = GetComponent<RawImage>();

        // Draw to the texture
        Texture2D texture = new Texture2D(mapSizeX, mapSizeY);
        texture.filterMode = FilterMode.Point;
        view.DrawPoints(model.points, texture, true);
        map.texture = texture;
    }

    void Update()
    {

    }

    public void OnClick()
    {
        // Track generation
        model.GenerateTrack();

        // Views update
        Texture2D texture = new Texture2D(mapSizeX, mapSizeY);
        texture.filterMode = FilterMode.Point;

        // Draw points
        view.DrawPoints(model.points, texture, true);
        map.texture = texture;
    }
}

public class RaceTrackGeneratorModel
{
    [SerializeField] public int numberOfPoints = 3;
    public Vector2[] points;
    private int rangeX;
    private int rangeY;

    public RaceTrackGeneratorModel(int rangeX, int rangeY)
    {
        this.rangeX = rangeX;
        this.rangeY = rangeY;
        GenerateRandomPoints(numberOfPoints);
    }

    public void GenerateTrack()
    {
        GenerateRandomPoints(numberOfPoints);
    }

    private void GenerateRandomPoints(int numberOfPoints)
    {
        if(numberOfPoints < 3)
        {
            Debug.LogError("Number of points must be greater than 2");
            points = null;
        }

        points = new Vector2[numberOfPoints];
        for(int i = 0; i < numberOfPoints; i++)
        {
            points[i] = new Vector2(Random.Range(0, rangeX), Random.Range(0, rangeY));
        }
    }
}


public class RaceTrackGeneratorView
{
    public void DrawPoints(Vector2[] points, Texture2D texture, bool whiteBg)
    {
        if(whiteBg)
        {
            Color[] whiteBackground = new Color[texture.width * texture.height];
            System.Array.Fill<Color>(whiteBackground, new Color(1f, 1f, 1f));
            texture.SetPixels(0, 0, texture.width, texture.height, whiteBackground);
            texture.Apply();
        }

        for(int i = 0; i < points.Length; i++)
        {
            texture.SetPixel((int)points[i].x, (int)points[i].y, Color.black);
        }
        texture.Apply();
    }
}
