using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
    private UnityEvent checkpointReached;

    void Awake()
    {
        // Get reference to event
        checkpointReached = GameObject.FindGameObjectWithTag("RaceTrack").GetComponent<RaceTrack>().checkpointReached;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            checkpointReached.Invoke();
            Destroy(gameObject);
        }
    }
}
