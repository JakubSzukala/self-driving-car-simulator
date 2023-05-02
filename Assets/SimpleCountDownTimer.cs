using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleCountDownTimer : MonoBehaviour
{
    public UnityEvent timeoutEvent;
    private float targetTime = 1f;
    public float startTime = 1f;
    public bool loop = true;

    void Update()
    {
        if (targetTime > 0)
        {
            targetTime -= Time.deltaTime;
        }
        else
        {
            timeoutEvent.Invoke();
            if (loop) ResetTimer();
        }
    }

    public void ResetTimer()
    {
        targetTime = startTime;
    }
}
