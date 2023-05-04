using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using AWSIM.LaserFormat;
using RGLUnityPlugin;


// It will need to be replacement for main controller
// used for testing up to this point
public class RaceCarAgent : Agent
{
    [SerializeField] private float checkpointReward = 1f;
    [SerializeField] private float timeElapsedPenalty = .1f;

    [SerializeField] private RaceTrack raceTrack;
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private ScoreSystem scoreSystem;
    [SerializeField] private SimpleCountDownTimer timer;

    [SerializeField] private GameObject agentCar;

    private LidarDataSubscriber lidarDataSubscriber;

    void Awake()
    {
        agentCar = GameObject.FindGameObjectWithTag("AgentCar");
        lidarDataSubscriber = new LidarDataSubscriber(agentCar.GetComponentInChildren<LidarSensor>());
    }

    public override void OnEpisodeBegin()
    {
        // Spawn race track
        raceTrack.CreateRaceTrack(true);

        // Set the agent car at the start
        Vector3 start, direction;
        raceTrack.GetRaceTrackStart(out start, out direction);
        agentCar.transform.position = start;
        agentCar.transform.rotation = Quaternion.LookRotation(direction);

        // Reset velocities
        agentCar.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        agentCar.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agentCar.transform.rotation = Quaternion.LookRotation(direction);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lidarDataSubscriber.Distances);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

    }
}
