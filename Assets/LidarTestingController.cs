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

    private LidarSensor lidarSensor;
    private RGLNodeSequence rglSubGraphScan;
    private byte[] scanData;
    //private double[] scanData;

    // Start is called before the first frame update
    void Start()
    {
        lidarSensor = car.GetComponentInChildren<LidarSensor>();
        lidarSensor.onNewData += OnNewLidarData;
        rglSubGraphScan = new RGLNodeSequence()
            .AddNodePointsFormat("agent_scan", FormatLaserScan.GetRGLFields());
        lidarSensor.ConnectToLidarFrame(rglSubGraphScan);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnNewLidarData()
    {
        scanData = new byte[0];
        int hitCount = rglSubGraphScan.GetResultDataRaw(ref scanData, 8);

        float[] ranges = new float[lidarSensor.configuration.horizontalSteps];
        for (var i = 0; i < hitCount; i++)  // RGL does not produce NaNs, thus ray indices have to be taken into account
        {
            int idx = lidarSensor.configuration.horizontalSteps - 1 - (int)BitConverter.ToUInt32(scanData, i * (sizeof(float) + sizeof(UInt32)) + sizeof(float));
            float value = BitConverter.ToSingle(scanData, i * (sizeof(float) + sizeof(UInt32)));
            if (value >= 0) {  // RGL should handle min range
                //Debug.Log($"measure is: {value}");
                ranges[idx] = value;
            }
            //Debug.Log("-----------------------");
        }
        Debug.Log($"Avg: {ranges.Max()}");
    }
}
