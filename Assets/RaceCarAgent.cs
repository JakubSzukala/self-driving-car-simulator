using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using AWSIM.LaserFormat;
using AWSIM;
using RGLUnityPlugin;

public class RaceCarAgent : Agent
{
    // Limiations
    [SerializeField] private float maxAcceleration = 1.5f;
    [SerializeField] private float maxSteerAngle = 35;

    // Rewards and penalties
    [SerializeField] private float checkpointReward = 1f;
    [SerializeField] private float trackFinishedReward = 10f;
    [SerializeField] private float timeElapsedPenalty = -.1f;
    [SerializeField] private float agentFellOffPenalty = -100f;
    [SerializeField] private float minScoreThreshold = -10f;

    // Episode time increment [s]
    [SerializeField] private float episodeTimeIncrement = 10f;

    // Discrete or continous flag
    [SerializeField] private bool discrete = false;

    // Relevant GameObjects
    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private SimpleCountDownTimer penaltyTimer;
    [SerializeField] private SimpleCountDownTimer episodeTimer;
    [SerializeField] private ScoreSystem scoreUI; // TODO: Simplify it so it actually only is ui

    // Agent
    private GameObject agentCar;

    // Lidar interface
    private LidarDataSubscriber lidarDataSubscriber;

    void Awake()
    {
        raceTrack.checkpointReached.AddListener(OnCheckpointReached);
        penaltyTimer.timeoutEvent.AddListener(OnTimePenalty);
        episodeTimer.timeoutEvent.AddListener(OnEpisodeEnd);
    }

    public override void OnEpisodeBegin()
    {
        // Spawn race track
        raceTrack.CreateRaceTrack(true);

        // Set the agent car at the start
        Vector3 start, direction;
        raceTrack.GetRaceTrackStart(out start, out direction);

        // Instantiate a car, get reference to it and assign lidar subscriber
        GameObject.Destroy(agentCar);
        agentCar = carSpawner.spawnCar(start, direction);
        lidarDataSubscriber = new LidarDataSubscriber(agentCar.GetComponentInChildren<LidarSensor>());

        // Start timers
        penaltyTimer.StartTimer();
        episodeTimer.StartTimer();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lidarDataSubscriber.Distances);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Apply actions
        float controlSignalAcc, controlSignalSteer;
        controlSignalAcc = maxAcceleration * (discrete ? actionBuffers.DiscreteActions[0] : actionBuffers.ContinuousActions[0]);
        controlSignalSteer = maxSteerAngle * (discrete ? actionBuffers.DiscreteActions[1] : actionBuffers.ContinuousActions[1]);
        agentCar.GetComponent<Vehicle>().AccelerationInput = controlSignalAcc;
        agentCar.GetComponent<Vehicle>().SteerAngleInput = controlSignalSteer;

        // If all checkpoints were scored add big reward
        if(raceTrack.checkPointContainer.transform.childCount == 0)
        {
            AddReward(trackFinishedReward);
            EpisodeCleanup(); // Keep this ordering due to race conditions
            EndEpisode();
        }

        // If wall collision is disabled, car can fall out of track, add penalty for that
        if (agentCar.transform.position.y < 0)
        {
            AddReward(agentFellOffPenalty);
            EpisodeCleanup();
            EndEpisode();
        }

        float score = GetCumulativeReward();
        scoreUI.Score = score;
        if (score <= minScoreThreshold)
        {
            EpisodeCleanup();
            EndEpisode();
        }
    }

    private void EpisodeCleanup()
    {
        // Reset timers
        episodeTimer.StopTimer(); episodeTimer.ResetTimer();
        penaltyTimer.StopTimer(); penaltyTimer.ResetTimer();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // TODO: Change to discrete also
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    // Callbacks
    private void OnCheckpointReached()
    {
        AddReward(checkpointReward);
    }

    private void OnTimePenalty()
    {
        AddReward(timeElapsedPenalty);
    }

    private void OnEpisodeEnd()
    {
        EpisodeCleanup();
        episodeTimer.startTime += episodeTimeIncrement;
        EndEpisode();
    }
}
