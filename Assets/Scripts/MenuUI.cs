using UnityEngine;

public class MenuUI : StateController.StateListener
{
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == StateController.State.Menu) OnMenuHide();
            if (newState == StateController.State.Menu) OnMenuShow();
        };
    }

    private void OnMenuShow() 
    {
        gameObject.SetActive(true);
    }
    
    private void OnMenuHide()
    {
        gameObject.SetActive(false);
    }

    public void Play()
    {
        StateController.SwitchTo(StateController.State.Game);
    }
    
    public void Scores()
    {
        StateController.SwitchTo(StateController.State.Scores);
    }
    
    public void Exit()
    {
        Application.Quit();
    }
}