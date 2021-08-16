using UnityEngine;

/// <summary>
/// Script attached to the upper boundary of the level.
/// When the ball enters the collider on this GameObject, this script ends the game.
/// </summary>
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
