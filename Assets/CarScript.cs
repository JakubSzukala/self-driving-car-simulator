using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    public Rigidbody myRigidBody;
    public List<AxleInfo> axlesInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    void Start()
    {

    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach(var axleInfo in axlesInfos)
        {
            if(axleInfo.steering)
            {
                axleInfo.rightWheel.steerAngle = steering;
                axleInfo.leftWheel.steerAngle = steering;
            }

            if(axleInfo.motor)
            {
                axleInfo.rightWheel.motorTorque = motor;
                axleInfo.leftWheel.motorTorque = motor;
            }
        }

        //if(Input.GetKeyDown(KeyCode.W))
        //{
            //myRigidBody.velocity = Vector3.forward * 10;
            ////myRigidBody.AddForceAtPosition()
            ////myRigidBody.AddForce(Vector3.forward * 100);
        //}

        //if(Input.GetKeyDown(KeyCode.A))
        //{
            ////myRigidBody.AddForce(Vector3.)
        //}
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider rightWheel;
    public WheelCollider leftWheel;
    public bool motor;
    public bool steering;
}
