using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StateController.State;
using Random = UnityEngine.Random;

public class LevelGen : StateController.StateListener 
{
    // Inspector Refs
    public List<LevelSection> LevelSections;
    public GameObject GatePrefab;
    public Text ScoreText;
    public Text SlowChargeText;
    
    // Consts
    public float SpawnPosition = -12f;
    public float DespawnPosition = 12f;
    public float GateOriginSide = 16f;
    public float GateThickness = 0.2f;
    public float SlowMultiplier = 0.5f;
    public float SlowCostPerSecond = 10f;
    public float InitialSlowCharge = 100f;
    
    // Difficulty Values
    private float _dSpawnInterval;
    private float _dMoveSpeed;
    private float _dGateWidth;
    private float _dGatePosOppChance;
    private float _dGatePosOuterBias;
    private float _dGatePosOuterMax;

    // Internal Vars
    private readonly List<GameObject> _gates = new List<GameObject>();
    private LevelSection _currentSection;
    private int _remainingSectionGates;
    private float _lastGatePos;
    private float _timeUntilNextSpawn;
    private float _slowCharge;
    private int _score;

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
    
    private void OnGameEnd()
    {
        ScoreText.gameObject.SetActive(false);
        SlowChargeText.gameObject.SetActive(false);
        foreach (var gate in _gates) Destroy(gate);
        _remainingSectionGates = 0;
        _currentSection = null;
        _gates.Clear();
    }

    private void Update()
    {
        switch (StateController.CurrentState)
        {
            case Game:
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
                    StateController.SwitchTo(Pause);
                }

                break;
            }
            case Pause:
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
                    StateController.SwitchTo(Game);
                }

                break;
            }
        }
        
        if (StateController.CurrentState != Game) return;
        
        var currentSpeedMultiplier = 1f;
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

        for (int i = _gates.Count - 1; i >= 0; i--) 
        {
            var gate = _gates[i];
            var position = gate.transform.position;
            gate.transform.position = new Vector3(position.x, position.y + _dMoveSpeed * Time.deltaTime * currentSpeedMultiplier);
            
            if (position.y >= DespawnPosition) 
            {
                _gates.RemoveAt(i);
                Destroy(gate);
            }
        }

        _timeUntilNextSpawn -= Time.deltaTime * currentSpeedMultiplier;

        if (_timeUntilNextSpawn <= 0f) 
        {
            var leftGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            var rightGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            
            _gates.Add(leftGate);
            _gates.Add(rightGate);

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

    private void UpdateTimeScale()
    {
        Time.timeScale = StateController.CurrentState switch
        {
            Game => 1f,
            Pause => 0f,
            _ => 1f
        };
    }

    private void UpdateUI()
    {
        ScoreText.text = _score.ToString();
        SlowChargeText.text = ((int)_slowCharge).ToString();
    }

    private void UpdateSection()
    {
        if (_remainingSectionGates <= 0)
        {
            _currentSection = LevelSections[Random.Range(0, LevelSections.Count)];
            if (Random.value <= 0.3f) _currentSection = LevelSections[0];
            _remainingSectionGates = Random.Range(_currentSection.MinSectionGates, _currentSection.MaxSectionGates + 1);
        }
        
        var diffVal = _currentSection.EvalScore(_score);
        _dSpawnInterval = _currentSection.SpawnInterval.Eval(diffVal);
        _dMoveSpeed = _currentSection.MoveSpeed.Eval(diffVal);
        _dGateWidth = _currentSection.GateWidth.Eval(diffVal);
        _dGatePosOppChance = _currentSection.GatePosOppChance.Eval(diffVal);
        _dGatePosOuterBias = _currentSection.GatePosOuterBias.Eval(diffVal);
        _dGatePosOuterMax = _currentSection.GatePosOuterMax.Eval(diffVal);
    }

    private void PositionGates(GameObject leftGate, GameObject rightGate)
    {
        var lastGateSide = _lastGatePos < 0f ? -1f : 1f;
        var gateSide = Random.value <= _dGatePosOppChance ? -lastGateSide : lastGateSide;

        var maxPos = (GateOriginSide - _dGateWidth) * _dGatePosOuterMax;
        var gatePos = gateSide * BiasedRandomRange(0f, maxPos, 1 / _dGatePosOuterBias);

        var leftEdge = gatePos - _dGateWidth / 2;
        var rightEdge = gatePos + _dGateWidth / 2;

        var leftSize = leftEdge + GateOriginSide;
        var rightSize = GateOriginSide - rightEdge;

        var leftPos = -GateOriginSide + leftSize / 2;
        var rightPos = rightEdge + rightSize / 2;

        leftGate.transform.position = new Vector3(leftPos, SpawnPosition);
        rightGate.transform.position = new Vector3(rightPos, SpawnPosition);

        leftGate.transform.localScale = new Vector3(leftSize, GateThickness);
        rightGate.transform.localScale = new Vector3(rightSize, GateThickness);

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
