using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelGen : StateController.StateListener 
{
    // Inspector Refs
    public List<LevelSection> LevelSections;
    public GameObject GatePrefab;
    public Text ScoreText;
    
    // Dimensions and Positions
    public float SpawnPosition = -12f;
    public float DespawnPosition = 12f;
    public float GateOriginSide = 16f;
    public float GateThickness = 0.2f;
    
    // Difficulty Values
    private float _dSpawnInterval;
    private float _dMoveSpeed;
    private float _dGateWidth;
    private float _dGatePosOppChance;
    private float _dGatePosOuterBias;

    // Internal Vars
    private readonly List<GameObject> _gates = new List<GameObject>();
    private LevelSection _currentSection;
    private int _remainingSectionGates;
    private float _lastGatePos;
    private float _lastSpawnTime;
    private int _score;

    public override void RegisterEvents()
    {
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == StateController.State.Game) OnGameEnd();
            if (newState == StateController.State.Game) OnGameStart();
        };
    }

    private void OnGameStart() 
    {
        _score = 0;
        ScoreText.text = _score.ToString();
        ScoreText.gameObject.SetActive(true);
        _currentSection = LevelSections[0];
        _remainingSectionGates = _currentSection.MaxSectionGates;
        UpdateSection();
        _lastGatePos = 0f;
        _lastSpawnTime = Time.time - _dSpawnInterval / _dMoveSpeed;
    }
    
    private void OnGameEnd()
    {
        ScoreText.gameObject.SetActive(false);
        foreach (var gate in _gates) Destroy(gate);
        _remainingSectionGates = 0;
        _currentSection = null;
        _gates.Clear();
    }

    private void Update()
    {
        if (StateController.CurrentState != StateController.State.Game) return;
        
        for (int i = _gates.Count - 1; i >= 0; i--) 
        {
            var gate = _gates[i];
            var position = gate.transform.position;
            gate.transform.position = new Vector3(position.x, position.y + _dMoveSpeed * Time.deltaTime);
            
            if (position.y >= DespawnPosition) 
            {
                _gates.RemoveAt(i);
                Destroy(gate);
            }
        }

        if (Time.time - _lastSpawnTime >= _dSpawnInterval / _dMoveSpeed) 
        {
            var leftGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            var rightGate = Instantiate(GatePrefab, new Vector3(0f, SpawnPosition), Quaternion.identity, transform);
            
            _gates.Add(leftGate);
            _gates.Add(rightGate);

            UpdateSection();
            
            leftGate.GetComponent<SpriteRenderer>().color = _currentSection.GateColor;
            rightGate.GetComponent<SpriteRenderer>().color = _currentSection.GateColor;
            
            PositionGates(leftGate, rightGate);
            
            _lastSpawnTime = Time.time;
            _remainingSectionGates--;
            
            _score++;
            StateController.PendingScore = _score;
            ScoreText.text = _score.ToString();
        }
        
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
    }

    private void PositionGates(GameObject leftGate, GameObject rightGate)
    {
        var lastGateSide = _lastGatePos < 0f ? -1f : 1f;
        var gateSide = Random.value <= _dGatePosOppChance ? -lastGateSide : lastGateSide;

        var maxPos = GateOriginSide - _dGateWidth;
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
