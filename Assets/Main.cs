using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject raceTrack;

    void Start()
    {
        RaceTrackGenerator generator = raceTrack.GetComponent<RaceTrackGenerator>();
        if(generator) raceTrack.GetComponent<RaceTrackGenerator>().Regenerate();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            raceTrack.GetComponent<RaceTrackGenerator>().Regenerate();
        }
    }
}
