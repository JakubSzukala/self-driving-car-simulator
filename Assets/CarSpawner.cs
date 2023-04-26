using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;

    public IEnumerator spawnCar(Vector3 position, Vector3 direction)
    {
        yield return new WaitForEndOfFrame();
        GameObject newCar = Instantiate(carPrefab, position, Quaternion.LookRotation(direction));
    }
}
