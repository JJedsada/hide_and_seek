using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController 
{
    public State CurrentState { get; private set; }

    private bool isInitialize;

    public void Initialize()
    {
        isInitialize = true;
    }

    public void ChangeState(State nextState)
    {
        if (CurrentState == nextState) return;

        if (CurrentState != null)
            CurrentState.ExitState();

        CurrentState = nextState; 
        CurrentState.EnterState();
    }

    public void Update(float deltaTime)
    {
        if (!isInitialize)
        {
            return;
        }

        CurrentState?.Update(deltaTime);
    }
}
