using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject raceTrack;
    [SerializeField] private GameObject carSpawner;

    void Start()
    {
        raceTrack.GetComponent<ProceduralPathCreator>().Regenerate();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var generator = raceTrack.GetComponent<ProceduralPathCreator>(); // TODO: Get a reference directly to generator? or nah?
            Vector2 startXY, directionXY;
            //generator.Regenerate();
            generator.GetStart(out startXY, out directionXY);
            Vector3 startXZ = new Vector3(startXY.x, 2, startXY.y);
            Vector3 directionXZ = new Vector3(directionXY.x, 0, directionXY.y);
            StartCoroutine(carSpawner.GetComponent<CarSpawner>().spawnCar(startXZ, directionXZ));
        }

        if (Input.GetMouseButtonDown(1))
        {
            var generator = raceTrack.GetComponent<ProceduralPathCreator>();
            generator.Regenerate();
        }
    }
}
