using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGLUnityPlugin;
using AWSIM.LaserFormat;
using System;
using System.Linq;

public class LidarTestingController : MonoBehaviour
{
    [SerializeField] private GameObject car;
    private LidarDataSubscriber lidarSubscriber;

    void Start()
    {
        lidarSubscriber = new LidarDataSubscriber(car.GetComponentInChildren<LidarSensor>());
        lidarSubscriber.newLidarDataArrived.AddListener(OnLidarSubscriberData);
    }

    void Update()
    {
        var maxDistance = lidarSubscriber.Distances.Max();
        Debug.Log($"Max detected distance: {maxDistance}");
    }

    void OnLidarSubscriberData()
    {
        var maxDistance = lidarSubscriber.Distances.Max();
        Debug.Log($"Max detected distance in event system: {maxDistance}");
    }
}
