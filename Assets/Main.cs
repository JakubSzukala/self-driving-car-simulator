using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject raceTrack;
    [SerializeField] private GameObject carSpawner;

    void Start()
    {
        RaceTrackGenerator generator = raceTrack.GetComponent<RaceTrackGenerator>();
        if(generator) raceTrack.GetComponent<RaceTrackGenerator>().Regenerate();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var generator = raceTrack.GetComponent<RaceTrackGenerator>(); // TODO: Get a reference directly to generator? or nah?
            Vector2 startXY, directionXY;
            //generator.Regenerate();
            generator.GetStart(out startXY, out directionXY);
            Vector3 startXZ = new Vector3(startXY.x, 2, startXY.y);
            Vector3 directionXZ = new Vector3(directionXY.x, 0, directionXY.y);
            StartCoroutine(carSpawner.GetComponent<CarSpawner>().spawnCar(startXZ, directionXZ));
        }

        if (Input.GetMouseButtonDown(1))
        {
            var generator = raceTrack.GetComponent<RaceTrackGenerator>();
            generator.Regenerate();
        }
    }
}
