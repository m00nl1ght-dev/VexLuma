using UnityEngine;
using static StateController.State;

public class MenuUI : StateController.StateListener
{
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == Menu) OnMenuHide();
            if (newState == Menu) OnMenuShow();
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
        StateController.SwitchTo(Game);
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