using UnityEngine;
using System.Collections;

namespace ChronoDash.Effects {
    /// <summary>
    /// Screen Shake Effect - Shakes camera for visual intensity.
    /// Uses random directional shake with gradual decay.
    /// Duration: 5 seconds (configurable, scales with difficulty).
    /// </summary>
    public class ScreenShakeEffect : MonoBehaviour {
        [Header("Effect Settings")]
        [SerializeField] private float effectDuration = 5f;
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeFrequency = 25f; // How fast camera shakes
        [SerializeField] private AnimationCurve decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        
        private bool isActive = false;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        
        public bool IsActive => isActive;
        
        private void Awake() {
            // Find main camera if not assigned
            if (mainCamera == null) {
                mainCamera = Camera.main;
            }
        }
    
        public IEnumerator TriggerEffect(float warningTime) {
            if (isActive) {
                Debug.LogWarning("⚠️ ScreenShakeEffect: Effect already active!");
                yield break;
            }
            
            if (mainCamera == null) {
                yield break;
            }
            
            isActive = true;
            
            Debug.Log($"📳 ScreenShakeEffect: Starting screen shake (duration: {effectDuration}s, intensity: {shakeIntensity})");
            
            // Store original camera position and rotation
            originalCameraPosition = mainCamera.transform.localPosition;
            originalCameraRotation = mainCamera.transform.localRotation;
            
            // Perform screen shake
            yield return ShakeCamera();
            
            // Reset camera to original position
            mainCamera.transform.localPosition = originalCameraPosition;
            mainCamera.transform.localRotation = originalCameraRotation;
            
            isActive = false;
        }
        
        public void ForceStopEffect() {
            StopAllCoroutines();
            
            // Reset camera position
            if (mainCamera != null) {
                mainCamera.transform.localPosition = originalCameraPosition;
                mainCamera.transform.localRotation = originalCameraRotation;
            }
            
            isActive = false;
        }
        
        private IEnumerator ShakeCamera() {
            float elapsed = 0f;
            
            while (elapsed < effectDuration) {
                elapsed += Time.deltaTime;
                
                // Calculate decay multiplier (1.0 at start, 0.0 at end)
                float progress = elapsed / effectDuration;
                float decayMultiplier = decayCurve.Evaluate(progress);
                
                // Generate random shake offset
                float shakeAmount = shakeIntensity * decayMultiplier;
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeAmount,
                    Random.Range(-1f, 1f) * shakeAmount,
                    0f // Don't shake on Z axis (depth)
                );
                
                // Apply shake to camera position
                mainCamera.transform.localPosition = originalCameraPosition + randomOffset;
                
                // Wait for next frame
                yield return null;
            }
            
            // Ensure camera is reset to original position
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }
}
