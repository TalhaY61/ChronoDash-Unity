using UnityEngine;
using System.Collections;
using ChronoDash.Managers;

namespace ChronoDash.Effects {
    /// <summary>
    /// Gravity Flip Effect - Rotates Main Camera 180° on Z-axis (roll).
    /// Creates visual effect of world flipping upside down (player still faces right).
    /// Z-axis rotation keeps camera facing forward (prevents black screen in 2D).
    /// During effect: Obstacles reverse direction (move right in world space).
    /// Duration: 10 seconds (configurable), then returns to normal.
    /// Can happen in ANY world at random intervals.
    /// Follows Single Responsibility Principle - only handles gravity flip visual effect.
    /// </summary>
    public class GravityFlipEffect : MonoBehaviour {
        [Header("Effect Settings")]
        [SerializeField] private float effectDuration = 10f;
        [SerializeField] private float rotationSpeed = 2f;
        
        [Header("Camera Reference")]
        [SerializeField] private Camera mainCamera;
        
        [Header("Audio")]
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip flipSound;
        
        [Header("UI Flipping")]
        [SerializeField] private bool flipUI = true; // Toggle UI flipping
        [SerializeField] private Canvas gameplayCanvas; // Drag gameplay canvas here (health, score, notifications)
        
        private bool isActive = false;
        private bool isFlipped = false;
        private Coroutine currentEffectCoroutine;
        
        public bool IsActive => isActive;
        public bool IsFlipped => isFlipped;
        
        private void Awake() {
            if (mainCamera == null) {
                mainCamera = Camera.main;
            }
            
            // Auto-find canvas if not assigned
            FindAndSetCanvas();
        }
        
        private void FindAndSetCanvas() {
            if (flipUI && gameplayCanvas == null) {
                // Try to find all canvases
                Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                
                foreach (Canvas canvas in allCanvases) {
                    
                    // Prefer Screen Space Overlay (most common for UI)
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                        gameplayCanvas = canvas;
                        break;
                    }
                }
                
                // If no overlay found, try screen space camera
                if (gameplayCanvas == null) {
                    foreach (Canvas canvas in allCanvases) {
                        if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                            gameplayCanvas = canvas;
                            break;
                        }
                    }
                }
                
                // Last resort: use first canvas
                if (gameplayCanvas == null && allCanvases.Length > 0) {
                    gameplayCanvas = allCanvases[0];
                }
            }
        }

        public IEnumerator TriggerEffect(float warningTime) {
            if (isActive) yield break;
            
            isActive = true;
            
            // Step 1: Show warning
            yield return StartCoroutine(ShowWarning(warningTime));
            
            // Step 2: Flip camera
            yield return StartCoroutine(FlipCamera());
            
            // Step 3: Wait duration
            yield return new WaitForSeconds(effectDuration);
            
            // Step 4: Restore camera
            yield return StartCoroutine(RestoreCamera());
            
            isActive = false;
        }
        
        private IEnumerator ShowWarning(float warningTime) {
            // Play warning sound
            if (warningSound != null) {
                AudioManager.Instance?.PlaySFX(warningSound);
            }
            
            yield return new WaitForSeconds(warningTime);
        }
        
        private IEnumerator FlipCamera() {
            if (mainCamera == null) yield break;
            
            // Play flip sound
            if (flipSound != null) {
                AudioManager.Instance?.PlaySFX(flipSound);
            }
            
            // Rotate from 0° to 180° smoothly on Z-axis (roll upside down)
            // Z-axis rotation = camera rolls 180°, world appears upside down but camera still looks forward
            float elapsed = 0f;
            float transitionTime = 1f / rotationSpeed;
            Quaternion startRotation = mainCamera.transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, 180f); // Roll on Z-axis (keeps camera facing forward)
            
            Quaternion uiStartRotation = Quaternion.identity;
            Quaternion uiTargetRotation = Quaternion.Euler(0f, 0f, 180f);
            
            // Re-check Canvas in case it wasn't found in Awake (scene might have loaded later)
            if (flipUI && gameplayCanvas == null) {
                FindAndSetCanvas();
            }
            
            if (flipUI && gameplayCanvas != null) {
                uiStartRotation = gameplayCanvas.transform.rotation;
            }
            
            while (elapsed < transitionTime) {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                
                // Flip UI canvas along with camera
                if (flipUI && gameplayCanvas != null) {
                    gameplayCanvas.transform.rotation = Quaternion.Lerp(uiStartRotation, uiTargetRotation, t);
                }
                
                yield return null;
            }
            
            // Ensure exactly 180° on X-axis
            mainCamera.transform.rotation = targetRotation;
            if (flipUI && gameplayCanvas != null) {
                gameplayCanvas.transform.rotation = uiTargetRotation;
            }
            isFlipped = true;
        }
        
        private IEnumerator RestoreCamera() {
            if (mainCamera == null) yield break;
            
            // Rotate from 180° back to 0° smoothly
            float elapsed = 0f;
            float transitionTime = 1f / rotationSpeed;
            Quaternion startRotation = mainCamera.transform.rotation;
            Quaternion targetRotation = Quaternion.identity;
            
            Quaternion uiStartRotation = Quaternion.Euler(0f, 0f, 180f);
            Quaternion uiTargetRotation = Quaternion.identity;
            
            if (flipUI && gameplayCanvas != null) {
                uiStartRotation = gameplayCanvas.transform.rotation;
            }
            
            while (elapsed < transitionTime) {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                
                // Restore UI canvas
                if (flipUI && gameplayCanvas != null) {
                    gameplayCanvas.transform.rotation = Quaternion.Lerp(uiStartRotation, uiTargetRotation, t);
                }
                
                yield return null;
            }
            
            // Ensure exactly 0°
            mainCamera.transform.rotation = Quaternion.identity;
            if (flipUI && gameplayCanvas != null) {
                gameplayCanvas.transform.rotation = Quaternion.identity;
            }
            isFlipped = false;
        }
        
        /// <summary>
        /// Force stop effect and restore camera immediately
        /// </summary>
        public void ForceStopEffect() {
            if (currentEffectCoroutine != null) {
                StopCoroutine(currentEffectCoroutine);
                currentEffectCoroutine = null;
            }
            
            StopAllCoroutines();
            
            // Restore camera rotation
            if (mainCamera != null) {
                mainCamera.transform.rotation = Quaternion.identity;
            }
            
            // Restore UI rotation
            if (flipUI && gameplayCanvas != null) {
                gameplayCanvas.transform.rotation = Quaternion.identity;
            }
            
            isActive = false;
            isFlipped = false;
        }
        
        public void SetEffectDuration(float duration) {
            effectDuration = Mathf.Max(1f, duration);
        }
        
        public void SetRotationSpeed(float speed) {
            rotationSpeed = Mathf.Max(0.5f, speed);
        }
        
        public float GetEffectDuration() => effectDuration;
        public float GetRotationSpeed() => rotationSpeed;
    }
}
