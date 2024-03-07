using System;
using UnityEngine;

public struct CountDown 
{
    public float currentDuration;
    public float startDuration {  get; private set; }

    public Action onStart;
    public Action<float> onUpdate;
    public Action onComplete;

    private bool Inited;
    private bool IsComplete;

    public void Start(float duration)
    {
        Inited = true;
        IsComplete = false;

        currentDuration = duration;
        startDuration = duration;
        onStart?.Invoke();
    }

    public void Update(float deltaTime)
    {
        if (!Inited || IsComplete)
            return;

        currentDuration -= deltaTime;
        onUpdate?.Invoke(currentDuration);

        if (currentDuration <= 0)
        {
            Complete();
        }
    }

    public void Complete()
    {
        onComplete?.Invoke();
        IsComplete = true;
    }
}
