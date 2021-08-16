using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StateController.State;
using Random = UnityEngine.Random;

/// <summary>
/// Script attached to the root GameObject of the level.
/// Handles random level generation and general game logic.
/// </summary>
public class LevelGen : StateController.StateListener 
{
    [SerializeField]
    [Tooltip("List of all level sections to be used in random generation.")]
    public List<LevelSection> LevelSections;
    
    [SerializeField]
    [Tooltip("Prefab to create gates from.")]
    public GameObject GatePrefab;
    
    [SerializeField]
    [Tooltip("Reference to Text UI for score value.")]
    public Text ScoreText;
    
    [SerializeField]
    [Tooltip("Reference to Text UI for slow charge value.")]
    public Text SlowChargeText;
    
    [SerializeField]
    [Tooltip("Initial Y position of gates when they are spawned.")]
    public float SpawnPosition = -12f;
    
    [SerializeField]
    [Tooltip("Y position that gates will despawn at.")]
    public float DespawnPosition = 12f;
    
    [SerializeField]
    [Tooltip("How far the gates extend to each side horizontally.")]
    public float GateOriginSide = 16f;
    
    [SerializeField]
    [Tooltip("How thick the gates are vertically.")]
    public float GateThickness = 0.2f;
    
    [SerializeField]
    [Tooltip("Multiplier applied to the time scale when player uses slowdown.")]
    public float SlowMultiplier = 0.5f;
    
    [SerializeField]
    [Tooltip("Slowdown charge cost while active per second.")]
    public float SlowCostPerSecond = 10f;
    
    [SerializeField]
    [Tooltip("Slowdown charge the player starts with.")]
    public float InitialSlowCharge = 100f;
    
    // Difficulty Values
    private float _dSpawnInterval;
    private float _dMoveSpeed;
    private float _dGateWidth;
    private float _dGatePosOppChance;
    private float _dGatePosOuterBias;
    private float _dGatePosOuterMax;

    // Internal Vars
    private readonly List<Rigidbody2D> _gates = new List<Rigidbody2D>();
    private LevelSection _currentSection;
    private int _remainingSectionGates;
    private float _lastGatePos;
    private float _timeUntilNextSpawn;
    private float _slowCharge;
    private int _score;

    /// <summary>
    /// Called during initialisation by the StateController.
    /// Scripts can subscribe to State change events in this method.
    /// </summary>
    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            UpdateTimeScale();
            if (oldState == Game && newState != Pause) OnGameEnd();
            else if (oldState == Pause && newState != Game) OnGameEnd();
            else if (newState == Game && oldState != Pause) OnGameStart();
        };
    }

    /// <summary>
    /// Called on State change:
    /// !Pause -> Game
    /// </summary>
    private void OnGameStart() 
    {
        _score = 0;
        _slowCharge = InitialSlowCharge;
        ScoreText.gameObject.SetActive(true);
        SlowChargeText.gameObject.SetActive(true);
        
        _currentSection = LevelSections[0];
        _remainingSectionGates = _currentSection.MaxSectionGates;
        UpdateSection();
        
        _lastGatePos = 0f;
        _timeUntilNextSpawn = 0f;
        UpdateUI();
    }
    
    /// <summary>
    /// Called on State change:
    /// Game | Pause -> Any
    /// </summary>
    private void OnGameEnd()
    {
        ScoreText.gameObject.SetActive(false);
        SlowChargeText.gameObject.SetActive(false);
        
        foreach (var gate in _gates) Destroy(gate.gameObject);
        _remainingSectionGates = 0;
        
        _currentSection = null;
        _gates.Clear();
    }

    private void Update()
    {
        // Player presses pause or unpause -> change state
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
            if (StateController.CurrentState == Game) StateController.SwitchTo(Pause);
            else if (StateController.CurrentState == Pause) StateController.SwitchTo(Game);
        }

        if (StateController.CurrentState != Game) return;
        var currentSpeedMultiplier = 1f;
        
        // Player presses slowdown -> update speed multiplier and charge
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            if (_slowCharge >= 1f)
            {
                currentSpeedMultiplier = SlowMultiplier;
                _slowCharge -= Time.deltaTime * SlowCostPerSecond;
            }
        }
        else
        {
            _slowCharge += Time.deltaTime;
        }

        // Update gate positions and despawn them as necessary
        for (int i = _gates.Count - 1; i >= 0; i--) 
        {
            var gate = _gates[i];
            var position = gate.transform.position;
            gate.velocity = new Vector3(0f, _dMoveSpeed * currentSpeedMultiplier);
            
            if (position.y >= DespawnPosition) 
            {
                _gates.RemoveAt(i);
                Destroy(gate.gameObject);
            }
        }

        // Update timer for next spawn
        _timeUntilNextSpawn -= Time.deltaTime * currentSpeedMultiplier;

        // If timer is ready, spawn new gate and position it
        if (_timeUntilNextSpawn <= 0f) 
        {
            var leftGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            var rightGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            
            _gates.Add(leftGate.GetComponent<Rigidbody2D>());
            _gates.Add(rightGate.GetComponent<Rigidbody2D>());

            UpdateSection();
            
            leftGate.GetComponent<SpriteRenderer>().color = _currentSection.GateColor;
            rightGate.GetComponent<SpriteRenderer>().color = _currentSection.GateColor;
            
            PositionGates(leftGate, rightGate);
            
            _timeUntilNextSpawn = _dSpawnInterval / _dMoveSpeed;
            _remainingSectionGates--;
            
            _score++;
            StateController.PendingScore = _score;
        }
        
        UpdateUI();
    }

    /// <summary>
    /// Refreshes the time scale of the physics engine based on current game state.
    /// </summary>
    private void UpdateTimeScale()
    {
        Time.timeScale = StateController.CurrentState switch
        {
            Game => 1f,
            Pause => 0f,
            _ => 1f
        };
    }

    /// <summary>
    /// Refreshes UI element values.
    /// </summary>
    private void UpdateUI()
    {
        ScoreText.text = _score.ToString();
        SlowChargeText.text = ((int)_slowCharge).ToString();
    }

    /// <summary>
    /// Update the current level section used for random generation.
    /// </summary>
    private void UpdateSection()
    {
        // If current section ended, start a new one.
        if (_remainingSectionGates <= 0)
        {
            // Randomly chose new section
            _currentSection = LevelSections[Random.Range(0, LevelSections.Count)];
            
            // Bias towards default section
            if (Random.value <= 0.3f) _currentSection = LevelSections[0];
            
            // Init section length randomly
            _remainingSectionGates = Random.Range(_currentSection.MinSectionGates, _currentSection.MaxSectionGates + 1);
        }
        
        // Calculate current difficuly based on score
        var diffVal = _currentSection.EvalScore(_score);
        
        // Apply all values from current section to internal fields
        _dSpawnInterval = _currentSection.SpawnInterval.Eval(diffVal);
        _dMoveSpeed = _currentSection.MoveSpeed.Eval(diffVal);
        _dGateWidth = _currentSection.GateWidth.Eval(diffVal);
        _dGatePosOppChance = _currentSection.GatePosOppChance.Eval(diffVal);
        _dGatePosOuterBias = _currentSection.GatePosOuterBias.Eval(diffVal);
        _dGatePosOuterMax = _currentSection.GatePosOuterMax.Eval(diffVal);
    }

    /// <summary>
    /// Position newly spawned gates randomly based on current difficulty.
    /// </summary>
    private void PositionGates(GameObject leftGate, GameObject rightGate)
    {
        // Determine which side the new gate will be on based on previous side
        var lastGateSide = _lastGatePos < 0f ? -1f : 1f;
        var gateSide = Random.value <= _dGatePosOppChance ? -lastGateSide : lastGateSide;

        // Determine center position of the new gate
        var maxPos = (GateOriginSide - _dGateWidth) * _dGatePosOuterMax;
        var gatePos = gateSide * BiasedRandomRange(0f, maxPos, 1 / _dGatePosOuterBias);

        // Determine left and right edge positions of the new gate
        var leftEdge = gatePos - _dGateWidth / 2;
        var rightEdge = gatePos + _dGateWidth / 2;

        // Determine left and right wall sizes and positions for the new gate
        var leftSize = leftEdge + GateOriginSide;
        var rightSize = GateOriginSide - rightEdge;
        var leftPos = -GateOriginSide + leftSize / 2;
        var rightPos = rightEdge + rightSize / 2;

        // Apply left and right wall sizes and positions for the new gate
        leftGate.transform.position = new Vector3(leftPos, SpawnPosition);
        rightGate.transform.position = new Vector3(rightPos, SpawnPosition);
        leftGate.transform.localScale = new Vector3(leftSize, GateThickness);
        rightGate.transform.localScale = new Vector3(rightSize, GateThickness);

        // Store position for next gate generation
        _lastGatePos = gatePos;
    }

    /// <summary>
    /// Returns a random value between minValue and maxValue (inclusive) with the given bias.
    /// A bias of 1 represents a uniform distribution.
    /// </summary>
    private static float BiasedRandomRange(float minValue, float maxValue, float bias)
    {
        return minValue + (maxValue - minValue) * Mathf.Pow(Random.value, bias);
    }
    
}
