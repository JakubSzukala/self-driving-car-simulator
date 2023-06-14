using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using AWSIM.LaserFormat;
using AWSIM;
using RGLUnityPlugin;
using System.Linq;

public class RaceCarAgent : Agent
{
    // Limiations
    [SerializeField] private float maxAcceleration = 1.5f;
    [SerializeField] private float maxSteerAngle = 35;

    // Rewards and penalties
    [SerializeField] private float trackFinishedReward = 10f;
    private float checkpointReward;
    [SerializeField] private float timeElapsedPenalty = -.1f;
    [SerializeField] private float agentFellOffPenalty = -100f;
    [SerializeField] private float minScoreThreshold = -10f;

    // Episode time increment [s]
    [SerializeField] private float episodeTimeIncrement = 10f;

    // Discrete or continous flag
    [SerializeField] private bool discrete = false;

    // Steering angle change limitation for smoother control
    [SerializeField] private bool limitSteeringAngleChange = false;
    [SerializeField] private float maxSteeringAngleChange = 5f;

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

        // Adjust reward for checkpoint, so longer track will award the same amount as short track
        checkpointReward = trackFinishedReward / raceTrack.checkPointContainer.transform.childCount;

        // Randomly change driving direction
        float seed = Random.Range(0f, 1f);
        if (seed > 0.5f) direction = -direction;

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
        float[] normalizedDistances = new float[lidarDataSubscriber.Distances.Length];
        for (int i = 0; i < lidarDataSubscriber.Distances.Length; i++)
        {
            normalizedDistances[i] = lidarDataSubscriber.Distances[i] / agentCar.GetComponentInChildren<LidarSensor>().configuration.maxRange;
        }
        sensor.AddObservation(normalizedDistances);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Apply actions
        float controlSignalAcc, controlSignalSteer;
        controlSignalAcc = maxAcceleration * actionBuffers.ContinuousActions[0];
        controlSignalSteer = maxSteerAngle * actionBuffers.ContinuousActions[1];
        if (limitSteeringAngleChange)
        {
            float steeringAngleChange = controlSignalSteer - agentCar.GetComponent<Vehicle>().SteerAngleInput;
            float clampedSteeringAngleChange = Mathf.Clamp(steeringAngleChange, -maxSteeringAngleChange, maxSteeringAngleChange);
            Debug.Log("Before: " + agentCar.GetComponent<Vehicle>().SteerAngleInput + " and after " + (agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange) + " and change: " + steeringAngleChange);
            agentCar.GetComponent<Vehicle>().SteerAngleInput = agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange;
        }
        else
        {
            agentCar.GetComponent<Vehicle>().SteerAngleInput = controlSignalSteer;
        }
        agentCar.GetComponent<Vehicle>().AccelerationInput = controlSignalAcc;

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
