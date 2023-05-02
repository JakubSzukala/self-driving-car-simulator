using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSpawner : MonoBehaviour
{
    [SerializeField] private GameObject checkPointPrefab;

    public IEnumerator SpawnCheckPoint(Vector3 position, Vector3 direction)
    {
        yield return new WaitForEndOfFrame();
        GameObject newCheckPoint = Instantiate(checkPointPrefab, position, Quaternion.LookRotation(direction));
        //newCheckPoint.GetComponent<Plane>().
    }
}
