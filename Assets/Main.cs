using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            raceTrack.CreateRaceTrack();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 start, direction;
            raceTrack.GetRaceTrackStart(out start, out direction);
            Debug.Log($"Start: {start}, direction: {direction}");
            StartCoroutine(carSpawner.spawnCar(start, direction));
        }
    }
}
