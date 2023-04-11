using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackGenerator : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }
}

[System.Serializable]
public class RaceTrackGeneratorModel
{
    public int numberOfPoints;
    public int rangeX;
    public int rangeY;

    public void GenerateTrack()
    {

    }

    private Vector2[] GenerateRandomPoints(int numberOfPoints)
    {
        if(numberOfPoints < 2)
        {
            Debug.LogError("Number of points must be greater than 2");
            return null;
        }

        Vector2 [] points = new Vector2[numberOfPoints];
        for(int i = 0; i < numberOfPoints; i++)
        {
            points[i] = new Vector2(Random.Range(0, rangeX), Random.Range(0, rangeY));
        }

        return points;
    }
}


public class RaceTrackGeneratorView
{

}
