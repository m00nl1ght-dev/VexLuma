using UnityEngine;
using static StateController.State;

public class PauseUI : StateController.StateListener
{
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

    private void OnPauseShow() 
    {
        gameObject.SetActive(true);
    }

    private void OnPauseHide()
    {
        gameObject.SetActive(false);
    }
    
    public void Continue()
    {
        StateController.SwitchTo(Game);
    }
    
    public void ToMenu()
    {
        StateController.PendingScore = -1;
        StateController.SwitchTo(Menu);
    }
}