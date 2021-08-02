using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallControls : StateController.StateListener
{
    [SerializeField]
    public Vector2 MenuVelocity;

    [SerializeField]
    public float ControlForce = 3f;
    
    [SerializeField]
    public float ControlForceDown;
    
    private Vector2 _initialPos;
    private Rigidbody2D _rigidbody;

    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == StateController.State.Game || oldState == StateController.State.Init) OnOther();
        };
    }

    private void Awake() 
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialPos = transform.position;
    }

    private void OnOther()
    {
        transform.position = _initialPos;
        _rigidbody.velocity = MenuVelocity;
        transform.Find("Trail").GetComponent<TrailRenderer>().Clear();
    }

    private void FixedUpdate() 
    {
        if (StateController.CurrentState != StateController.State.Game) return;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            _rigidbody.AddForce(new Vector2(-ControlForce, 0f));
        }
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            _rigidbody.AddForce(new Vector2(ControlForce, 0f));
        }
        
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            _rigidbody.AddForce(new Vector2(0f, -ControlForceDown));
        }
    }
    
}