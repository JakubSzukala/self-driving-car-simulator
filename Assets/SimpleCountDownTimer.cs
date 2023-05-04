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
    private bool running = false;

    void Update()
    {
        if (!running) return;
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

    public void StartTimer()
    {
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        targetTime = startTime;
    }
}
