using static StateController.State;

/// <summary>
/// Script attached to the root RectTransform of the pause menu.
/// Handles UI events and State changes.
/// </summary>
public class PauseUI : StateController.StateListener
{
    /// <summary>
    /// Called during initialisation by the StateController.
    /// Scripts can subscribe to State change events in this method.
    /// </summary>
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == Pause) OnPauseHide();
            if (newState == Pause) OnPauseShow();
        };
    }

    private void Start()
    {
        if (StateController.CurrentState != Pause)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called on State change:
    /// Any -> Pause
    /// </summary>
    private void OnPauseShow() 
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called on State change:
    /// Pause -> Any
    /// </summary>
    private void OnPauseHide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Event handler for "Continue" button.
    /// </summary>
    public void Continue()
    {
        StateController.SwitchTo(Game);
    }
    
    /// <summary>
    /// Event handler for "Leave" button.
    /// </summary>
    public void ToMenu()
    {
        StateController.PendingScore = -1;
        StateController.SwitchTo(Menu);
    }
}