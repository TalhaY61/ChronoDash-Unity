using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ChronoDash.Effects {
    /// <summary>
    /// Darkness Effect - Creates spotlight vision around player.
    /// Everything outside radius is pitch black.
    /// Uses a shader-based approach for optimal performance.
    /// </summary>
    public class DarknessEffect : MonoBehaviour {
        [Header("Effect Settings")]
        [SerializeField] private float effectDuration = 10f;
        [SerializeField] private float spotlightRadius = 250f; // Screen pixels (increased for visibility)
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 1.5f;
        [SerializeField] private float edgeSoftness = 100f; // Gradient edge size (increased for smoother transition)
        
        // Runtime-created objects (no prefabs needed!)
        private GameObject darknessOverlay;
        private Canvas overlayCanvas;
        private Image darknessImage;
        private Material darknessMaterial;
        private Transform playerTransform;
        private bool isActive = false;
        
        public bool IsActive => isActive;
        
        private void Awake() {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerTransform = player.transform;
            }
            
            // Find or create canvas for overlay
            if (overlayCanvas == null) {
                overlayCanvas = FindCanvasForOverlay();
            }
        }
        
        /// <summary>
        /// Trigger the darkness effect with warning notification
        /// </summary>
        public IEnumerator TriggerEffect(float warningTime) {
            if (isActive) yield break;
            
            if (playerTransform == null) yield break;
            
            isActive = true;
                        
            // Create darkness overlay
            CreateDarknessOverlay();
            
            // Fade in darkness
            yield return FadeDarkness(0f, 1f, fadeInDuration);
            
            // Wait for effect duration - shader updates spotlight automatically
            yield return new WaitForSeconds(effectDuration);
            
            // Fade out darkness
            yield return FadeDarkness(1f, 0f, fadeOutDuration);
            
            // Clean up
            DestroyDarknessOverlay();
            
            isActive = false;
        }
    
        public void ForceStopEffect() {
            StopAllCoroutines();
            DestroyDarknessOverlay();
            isActive = false;
        }
        
        private void CreateDarknessOverlay() {
            if (overlayCanvas == null) return;
            
            // Create overlay GameObject
            darknessOverlay = new GameObject("DarknessOverlay");
            darknessOverlay.transform.SetParent(overlayCanvas.transform, false);
            
            // Add RectTransform - full screen
            RectTransform rectTransform = darknessOverlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Add Image component
            darknessImage = darknessOverlay.AddComponent<Image>();
            darknessImage.color = new Color(0f, 0f, 0f, 0f); // Start transparent
            darknessImage.raycastTarget = false;
            
            // Create shader material
            CreateShaderMaterial();
            darknessImage.material = darknessMaterial;
            
            // Set a white texture (shader will handle the spotlight)
            Texture2D whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
            darknessImage.sprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        }
        
        private void CreateShaderMaterial() {
            // Try to find the custom shader
            Shader shader = Shader.Find("Custom/SpotlightDarkness");
            
            if (shader == null) {
                // Fallback to default UI shader
                shader = Shader.Find("UI/Default");
            }
            
            darknessMaterial = new Material(shader);
            
            // Initialize shader properties
            UpdateShaderProperties();
        }
        
        private void Update() {
            // Update spotlight position every frame (GPU-side, very fast)
            if (isActive && darknessMaterial != null && playerTransform != null) {
                UpdateShaderProperties();
            }
        }
        
        private void UpdateShaderProperties() {
            if (darknessMaterial == null || playerTransform == null) return;
            
            // Get player screen position
            Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
            
            // Set shader properties
            darknessMaterial.SetVector("_SpotlightCenter", new Vector4(playerScreenPos.x, playerScreenPos.y, 0, 0));
            darknessMaterial.SetFloat("_SpotlightRadius", spotlightRadius);
            darknessMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
        }
        
        private IEnumerator FadeDarkness(float fromAlpha, float toAlpha, float duration) {
            if (darknessImage == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                
                // Update image alpha
                Color color = darknessImage.color;
                color.a = alpha;
                darknessImage.color = color;
                
                // Also update shader opacity
                if (darknessMaterial != null) {
                    darknessMaterial.SetFloat("_Opacity", alpha);
                }
                
                yield return null;
            }
            
            // Ensure final alpha
            Color finalColor = darknessImage.color;
            finalColor.a = toAlpha;
            darknessImage.color = finalColor;
            
            if (darknessMaterial != null) {
                darknessMaterial.SetFloat("_Opacity", toAlpha);
            }
        }
        
        private void DestroyDarknessOverlay() {
            if (darknessMaterial != null) {
                Destroy(darknessMaterial);
                darknessMaterial = null;
            }
            
            if (darknessOverlay != null) {
                Destroy(darknessOverlay);
                darknessOverlay = null;
                darknessImage = null;
            }
        }
        
        private Canvas FindCanvasForOverlay() {
            // Find main game canvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            foreach (Canvas canvas in canvases) {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return canvas;
            }
            
            // Create new canvas if none found
            GameObject canvasObj = new GameObject("DarknessCanvas");
            Canvas newCanvas = canvasObj.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 1000; // Render on top of everything
            
            // Add CanvasScaler with proper settings for WebGL
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Match your game's resolution
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            return newCanvas;
        }
    }
}
