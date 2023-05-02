using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private ScoreSystem scoreSystem;

    void Update()
    {
        // TODO: Remove after implementing event system
        if (Input.GetKeyDown(KeyCode.M))
        {
            scoreSystem.IncreaseScore(1f);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            raceTrack.CreateRaceTrack();
            raceTrack.CreateRaceTrackCheckPoints();
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
