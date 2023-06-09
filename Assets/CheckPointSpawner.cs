using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSpawner : MonoBehaviour
{
    [SerializeField] private GameObject checkPointPrefab;
    [SerializeField] private GameObject checkPointContainer;

    public void SpawnCheckPoint(Vector3 position, Vector3 direction, float width)
    {
        //yield return new WaitForEndOfFrame();
        GameObject newCheckPoint = Instantiate(checkPointPrefab, position, Quaternion.LookRotation(direction), checkPointContainer.transform);
        newCheckPoint.transform.localScale = new Vector3(
            width, newCheckPoint.transform.localScale.y, newCheckPoint.transform.localScale.z);
    }
}
