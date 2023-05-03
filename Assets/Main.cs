using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private float checkpointReward = 1f;
    [SerializeField] private float timeElapsedPenalty = .1f;

    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private ScoreSystem scoreSystem;
    [SerializeField] private SimpleCountDownTimer timer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            raceTrack.CreateRaceTrack(true);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 start, direction;
            raceTrack.GetRaceTrackStart(out start, out direction);
            StartCoroutine(carSpawner.spawnCar(start, direction));
            raceTrack.checkpointReached.AddListener(OnCheckpointReached);
            timer.timeoutEvent.AddListener(OnTimeElapsed);
        }
    }

    private void OnCheckpointReached()
    {
        scoreSystem.IncreaseScore(checkpointReward);
    }

    private void OnTimeElapsed()
    {
        scoreSystem.DecreaseScore(timeElapsedPenalty);
    }
}
