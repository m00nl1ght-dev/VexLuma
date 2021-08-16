using UnityEngine;
using static StateController.State;

/// <summary>
/// Script attached to the ball controlled by the player.
/// Handles player input and initial positioning.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BallControls : StateController.StateListener
{
    [SerializeField]
    [Tooltip("The velocity the ball starts with in menu screens.")]
    public Vector2 MenuVelocity;

    [SerializeField]
    [Tooltip("The force applied to the ball when the player presses left or right.")]
    public float ControlForce = 3f;
    
    [SerializeField]
    [Tooltip("The force applied to the ball when the player presses down.")]
    public float ControlForceDown;
    
    private Vector2 _initialPos;
    private Rigidbody2D _rigidbody;

    /// <summary>
    /// Called during initialisation by the StateController.
    /// Scripts can subscribe to State change events in this method.
    /// </summary>
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == Game && newState == Pause) return;
            if (oldState == Pause && newState == Game) return;
            if (oldState == Game || oldState == Init || oldState == Pause) OnOther();
        };
    }

    private void Awake() 
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialPos = transform.position;
    }

    /// <summary>
    /// Called on State change:
    /// Game | Init | Pause -> Any
    /// </summary>
    private void OnOther()
    {
        transform.position = _initialPos;
        _rigidbody.velocity = MenuVelocity;
        transform.Find("Trail").GetComponent<TrailRenderer>().Clear();
    }

    private void FixedUpdate() 
    {
        if (StateController.CurrentState != Game) return;
        
        // Player controls -> move left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            _rigidbody.AddForce(new Vector2(-ControlForce, 0f));
        }
        
        // Player controls -> move right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            _rigidbody.AddForce(new Vector2(ControlForce, 0f));
        }
        
        // Player controls -> move down
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            _rigidbody.AddForce(new Vector2(0f, -ControlForceDown));
        }
    }
    
}
