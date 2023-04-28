using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            raceTrack.CreateRaceTrack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 start, direction;
            raceTrack.GetRaceTrackStart(out start, out direction);
            Debug.Log($"Start: {start}, direction: {direction}");
            StartCoroutine(carSpawner.spawnCar(start, direction));
        }
    }
}
