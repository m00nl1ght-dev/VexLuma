using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central controller class for static context of the game.
/// Manages game state and allows other scripts to subscribe to State change events.
/// </summary>
public class StateController : MonoBehaviour
{
    /// <summary>
    /// Currently active State, always updated before change events are dispatched.
    /// </summary>
    public static State CurrentState { get; private set; } = State.Init;

    /// <summary>
    /// Event invoked when CurrentState changes.
    /// </summary>
    public static event Action<State, State> OnStateChange;

    /// <summary>
    /// Last score that has not yet been added to the highscore list.
    /// </summary>
    public static int PendingScore = -1;

    /// <summary>
    /// Change the CurrentState to a new value and dispatch respective events.
    /// </summary>
    public static void SwitchTo(State state)
    {
        var oldState = CurrentState;
        CurrentState = state;
        OnStateChange?.Invoke(oldState, state);
    }

    [SerializeField]
    [Tooltip("Collection of all StateListeners that need to be subscribed to State change events.")]
    private List<StateListener> _stateListeners;

    private void Start()
    {
        // Subscribe all listeners
        foreach (var stateListener in _stateListeners)
        {
            stateListener.RegisterEvents();
        }
        
        // Switch to initial state
        SwitchTo(State.Menu);
    }

    /// <summary>
    /// All the possible states for the game to be in.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Initial state active after the game is loaded.
        /// </summary>
        Init, 
        
        /// <summary>
        /// Main menu UI is active.
        /// </summary>
        Menu, 
        
        /// <summary>
        /// Ingame state, player controls are enabled.
        /// </summary>
        Game, 
        
        /// <summary>
        /// Scores UI is active.
        /// </summary>
        Scores, 
        
        /// <summary>
        /// Game is paused and Pause UI is active.
        /// </summary>
        Pause
    }
    
    /// <summary>
    /// Base class for all scripts that need to be subscribed to State change events.
    /// </summary>
    public abstract class StateListener : MonoBehaviour
    {
        /// <summary>
        /// Called during initialisation by the StateController.
        /// Scripts can subscribe to State change events in this method.
        /// </summary>
        public abstract void RegisterEvents();
    }
}