using System;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public static State CurrentState { get; private set; } = State.Init;

    public static event Action<State, State> OnStateChange;

    public static int PendingScore = -1;

    public static void SwitchTo(State state)
    {
        var oldState = CurrentState;
        CurrentState = state;
        OnStateChange?.Invoke(oldState, state);
    }

    [SerializeField]
    private List<StateListener> _stateListeners;

    private void Start()
    {
        foreach (var stateListener in _stateListeners)
        {
            stateListener.RegisterEvents();
        }
        
        SwitchTo(State.Menu);
    }

    public enum State
    {
        Init, Menu, Game, Scores, Pause
    }
    
    public abstract class StateListener : MonoBehaviour
    {
        public abstract void RegisterEvents();
    }
}