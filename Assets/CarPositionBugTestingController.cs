using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPositionBugTestingController : MonoBehaviour
{
    public GameObject car;
    void Start()
    {
        //car.transform.position = Vector3.zero;
    }

    void Update()
    {
        car.GetComponent<Rigidbody>().WakeUp();
        car.transform.position = car.transform.position + Vector3.forward;
        Debug.Log($"CarPos: {car.transform.position}");
    }
}
