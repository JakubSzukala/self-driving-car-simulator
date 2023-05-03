using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


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

    void Start()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        // Spawn a car and I guess spawn a track first
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add Lidar observations to sensor
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

    }
}
