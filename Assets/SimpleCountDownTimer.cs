using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleCountDownTimer : MonoBehaviour
{
    public UnityEvent timeoutEvent;
    [SerializeField] private float targetTime = 1f;
    public float startTime = 1f;
    [SerializeField] public bool loop = true;
    [SerializeField] private bool running = false;

    void Start()
    {
        targetTime = startTime;
    }

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
        if (targetTime <= 0) targetTime = startTime;
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
