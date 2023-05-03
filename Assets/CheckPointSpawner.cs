using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSpawner : MonoBehaviour
{
    [SerializeField] private GameObject checkPointPrefab;

    public IEnumerator SpawnCheckPoint(Vector3 position, Vector3 direction, float width)
    {
        yield return new WaitForEndOfFrame();
        GameObject newCheckPoint = Instantiate(checkPointPrefab, position, Quaternion.LookRotation(direction), transform);
        newCheckPoint.transform.localScale = new Vector3(
            width, newCheckPoint.transform.localScale.y, newCheckPoint.transform.localScale.z);
    }
}
