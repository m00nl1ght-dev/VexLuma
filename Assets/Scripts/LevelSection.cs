using System;
using UnityEngine;

[CreateAssetMenu]
public class LevelSection : ScriptableObject
{
    public int MaxDifficultyScore = 1000;
    public int MinSectionGates = 20;
    public int MaxSectionGates = 30;

    public Curve SpawnInterval = new Curve();
    public Curve MoveSpeed = new Curve();
    public Curve GateWidth = new Curve();
    public Curve GatePosOppChance = new Curve();
    public Curve GatePosOuterBias = new Curve();
    public Curve GatePosOuterMax = new Curve();
    
    public Color GateColor = Color.white;
    
    public float EvalScore(float score) => Mathf.Clamp01(score / MaxDifficultyScore);
    
    [Serializable]
    public class Curve
    {
        public float LowerValue;
        public float UpperValue = 1f;
        public AnimationCurve Interpolation = new AnimationCurve();

        public float Eval(float val)
        {
            return LowerValue + (UpperValue - LowerValue) * Interpolation.Evaluate(val);
        }
    }
    
}