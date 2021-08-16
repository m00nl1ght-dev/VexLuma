using System;
using UnityEngine;

/// <summary>
/// Utility script that adapts local scale based on screen aspect ratio.
/// </summary>
public class FillScreenSpace : MonoBehaviour {
    
    [SerializeField]
    [Tooltip("The screen size to use as baseline for adaption.")]
    public Vector2 targetSize;
    
    private void Awake() {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;
        var cameraHeight = mainCamera.orthographicSize;
        var cameraSize = new Vector2(mainCamera.aspect * cameraHeight, cameraHeight);

        var scale = Vector3.one;
        var fracX = cameraSize.x / targetSize.x;
        var fracY = cameraSize.y / targetSize.y;
        scale *= Math.Min(fracX, fracY);
        transform.localScale = scale;
    }
}
