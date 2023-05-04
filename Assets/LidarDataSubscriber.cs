using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGLUnityPlugin;
using AWSIM.LaserFormat;
using UnityEngine.Events;
using System;

public class LidarDataSubscriber
{
    private LidarSensor lidarSensor;
    private RGLNodeSequence rglSubGraphScan;
    public float[] Distances
    { get; private set; }

    /* Two approaches to extract data:
     * - use accessor Distances to get the data whenever it is suitable
     *   risk is, that some records may be missed this way, or some may be doubled
     * - subscribe to event that will be invoked on every new data arrival,
     *   this way data will always be new, use the same accessor but in event
     */

    public UnityEvent newLidarDataArrived;

    public LidarDataSubscriber(LidarSensor lidarSensor)
    {
        this.lidarSensor = lidarSensor;
        Distances = new float[lidarSensor.configuration.horizontalSteps];
        rglSubGraphScan = new RGLNodeSequence()
            .AddNodePointsFormat("agent_scan", FormatLaserScan.GetRGLFields());
        lidarSensor.ConnectToLidarFrame(rglSubGraphScan);
        lidarSensor.onNewData += OnNewLidarData;
        newLidarDataArrived = new UnityEvent();
    }

    private void OnNewLidarData()
    {
        byte[] rawData = new byte[0];
        int hitCount = rglSubGraphScan.GetResultDataRaw(ref rawData, 8);
        DecodeData(rawData, hitCount);

        newLidarDataArrived.Invoke();
    }

    private void DecodeData(byte[] rawData, int hitCount)
    {
        Distances = new float[lidarSensor.configuration.horizontalSteps];
        for (var i = 0; i < hitCount; i++)  // RGL does not produce NaNs, thus ray indices have to be taken into account
        {
            int idx = lidarSensor.configuration.horizontalSteps - 1 - (int)BitConverter.ToUInt32(rawData, i * (sizeof(float) + sizeof(UInt32)) + sizeof(float));
            float value = BitConverter.ToSingle(rawData, i * (sizeof(float) + sizeof(UInt32)));
            if (value >= 0) {  // RGL should handle min range
                Distances[idx] = value;
            } // else inf?
        }
    }
}
