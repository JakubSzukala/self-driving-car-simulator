using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject mainCamera;

    public GameObject spawnCar(Vector3 position, Vector3 direction)
    {
        GameObject newCar = Instantiate(carPrefab, position, Quaternion.LookRotation(direction));

        // If camera has a follow script attach it to the car
        AWSIM.FollowCamera cameraFollower = mainCamera.GetComponent<AWSIM.FollowCamera>();
        if (cameraFollower)
        {
            cameraFollower.target = newCar.transform;
        }

        return newCar;
    }
}
