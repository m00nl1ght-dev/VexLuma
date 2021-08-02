using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<BallControls>(out _))
        {
            if (StateController.CurrentState == StateController.State.Game)
            {
                StateController.SwitchTo(StateController.State.Scores);
            }
        }
    }
}
