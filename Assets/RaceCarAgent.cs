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
    [SerializeField] private float timeElapsedPenalty = .1f;

    // Discrete or continous flag
    [SerializeField] private bool discrete = true;

    // Relevant GameObjects
    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private ScoreSystem scoreSystem;
    [SerializeField] private SimpleCountDownTimer penaltyTimer;
    [SerializeField] private SimpleCountDownTimer episodeTimer;

    // Agent
    private GameObject agentCar;

    // Lidar interface
    private LidarDataSubscriber lidarDataSubscriber;

    // Kepping track of score increments
    private float prevScore = 0f;

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

        // Reward
        // TODO: This could be moved to score system
        float scoreIncrement = scoreSystem.Score - prevScore;
        prevScore = scoreSystem.Score;

        AddReward(scoreIncrement);

        // TODO: Add early stopping here if the agent does nothing

        // If all checkpoints were scored add big reward
        if(raceTrack.checkPointContainer.transform.childCount == 0)
        {
            AddReward(trackFinishedReward);
            EpisodeCleanup();
            EndEpisode();
        }

        // Maybe little bit clearer would be if episode timeout would be stated here
    }

    private void EpisodeCleanup()
    {
        // Reset timers
        episodeTimer.StopTimer(); episodeTimer.ResetTimer();
        penaltyTimer.StopTimer(); penaltyTimer.ResetTimer();

        // Reset score system
        scoreSystem.ResetScore();
        prevScore = 0f;
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
        scoreSystem.IncreaseScore(checkpointReward);
    }

    private void OnTimePenalty()
    {
        scoreSystem.DecreaseScore(timeElapsedPenalty);
    }

    private void OnEpisodeEnd()
    {
        // TODO: Add reward at this point?
        EpisodeCleanup();
        EndEpisode();
    }
}
