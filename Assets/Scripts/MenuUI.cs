using UnityEngine;
using static StateController.State;

/// <summary>
/// Script attached to the root RectTransform of the main menu.
/// Handles UI events and State changes.
/// </summary>
public class MenuUI : StateController.StateListener
{
    /// <summary>
    /// Called during initialisation by the StateController.
    /// Scripts can subscribe to State change events in this method.
    /// </summary>
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == Menu) OnMenuHide();
            if (newState == Menu) OnMenuShow();
        };
    }

    /// <summary>
    /// Called on State change:
    /// Any -> Menu
    /// </summary>
    private void OnMenuShow() 
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Called on State change:
    /// Menu -> Any
    /// </summary>
    private void OnMenuHide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Event handler for "Play" button.
    /// </summary>
    public void Play()
    {
        StateController.SwitchTo(Game);
    }
    
    /// <summary>
    /// Event handler for "Scores" button.
    /// </summary>
    public void Scores()
    {
        StateController.SwitchTo(StateController.State.Scores);
    }
    
    /// <summary>
    /// Event handler for "Exit" button.
    /// </summary>
    public void Exit()
    {
        Application.Quit();
    }
}