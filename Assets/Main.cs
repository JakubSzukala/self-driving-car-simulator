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
            var generator = raceTrack.GetComponent<RaceTrackGenerator>();
            Vector2 start, direction;
            generator.Regenerate();
            generator.GetStart(out start, out direction);
            Debug.Log($"Start: {start}, direction: {direction}");
        }
    }
}
