using System;
using UnityEngine;

/// <summary>
/// Data container for level generation settings.
/// One level section is active at a time and swapped out after some amount of gates have been spawned.
/// </summary>
[CreateAssetMenu]
public class LevelSection : ScriptableObject
{
    [Tooltip("Upper bounds of the difficulty scaling for this section.")]
    public int MaxDifficultyScore = 1000;
    [Tooltip("Lower bound of the randomly selected amount of gates for this section.")]
    public int MinSectionGates = 20;
    [Tooltip("Upper bound of the randomly selected amount of gates for this section.")]
    public int MaxSectionGates = 30;

    [Tooltip("Time until another gate is spawned in seconds after the last spawn.")]
    public Curve SpawnInterval = new Curve();
    [Tooltip("Velocity of the gates moving upwards through the level.")]
    public Curve MoveSpeed = new Curve();
    [Tooltip("Horizontal width of the gates spawned in this section.")]
    public Curve GateWidth = new Curve();
    [Tooltip("Probability for a new gate to spawn on the opposite half of the level than the previous one.")]
    public Curve GatePosOppChance = new Curve();
    [Tooltip("Bias for the calculation of gate positions towards the outside, 1 -> neutral.")]
    public Curve GatePosOuterBias = new Curve();
    [Tooltip("Maximum distance for new gates from the center of the level.")]
    public Curve GatePosOuterMax = new Curve();
    
    [Tooltip("Color of gates spawned in this section.")]
    public Color GateColor = Color.white;
    
    /// <summary>
    /// Calculates a difficulty value in [0...1] for this section based on current score.
    /// </summary>
    public float EvalScore(float score) => Mathf.Clamp01(score / MaxDifficultyScore);
    
    /// <summary>
    /// Helper class for dynamic float values based on interpolation with relative difficulty in [0...1].
    /// </summary>
    [Serializable]
    public class Curve
    {
        [Tooltip("Value returned when relative difficulty is 0.")]
        public float LowerValue;
        
        [Tooltip("Value returned when relative difficulty is 1.")]
        public float UpperValue = 1f;
        
        [Tooltip("Defines how values are interpolated when relative difficulty is between 0 and 1.")]
        public AnimationCurve Interpolation = new AnimationCurve();

        /// <summary>
        /// Calculates the float value based on bounds and interpolation.
        /// </summary>
        public float Eval(float val)
        {
            return LowerValue + (UpperValue - LowerValue) * Interpolation.Evaluate(val);
        }
    }
    
}