using UnityEngine;
using UnityEngine.UI;

namespace ChronoDash.Abilities
{
    /// <summary>
    /// Controls the visual shader effect for the Time Bubble.
    /// Creates a circular colored overlay showing the time-slowed area.
    /// </summary>
    public class TimeBubbleShaderEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TimeBubble timeBubble;
        [SerializeField] private GameObject effectOverlay;
        [SerializeField] private Material bubbleMaterial;
        
        [Header("Shader Settings")]
        [SerializeField] private float radiusMultiplier = 100f;
        [SerializeField] private float edgeSoftness = 50f;
        [SerializeField] private Color bubbleColor = new Color(0.5f, 0.8f, 1f, 0.3f);
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        private Image overlayImage;
        private Camera mainCamera;
        private float currentOpacity = 0f;
        private bool isFading = false;
        private float fadeProgress = 0f;
        private bool isVisible = false;
        
        private void Start()
        {
            // Find TimeBubble if not assigned
            if (timeBubble == null)
                timeBubble = FindFirstObjectByType<TimeBubble>();
            
            // Get main camera
            mainCamera = Camera.main;
            
            // Setup overlay
            if (effectOverlay != null)
            {
                overlayImage = effectOverlay.GetComponent<Image>();
                if (overlayImage != null && bubbleMaterial != null)
                {
                    // Create material instance
                    overlayImage.material = new Material(bubbleMaterial);
                }
                
                effectOverlay.SetActive(false);
            }
            
            // Subscribe to bubble events
            if (timeBubble != null)
            {
                timeBubble.OnBubbleStateChanged += OnBubbleStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (timeBubble != null)
            {
                timeBubble.OnBubbleStateChanged -= OnBubbleStateChanged;
            }
        }
        
        private void OnBubbleStateChanged(bool active)
        {
            if (active)
            {
                ShowEffect();
            }
            else
            {
                HideEffect();
            }
        }
        
        private void ShowEffect()
        {
            if (effectOverlay != null)
            {
                effectOverlay.SetActive(true);
                isVisible = true;
                isFading = true;
                fadeProgress = 0f;
            }
        }
        
        private void HideEffect()
        {
            isVisible = false;
            isFading = true;
            fadeProgress = 0f;
        }
        
        private void Update()
        {
            if (!isVisible && !isFading && effectOverlay != null && effectOverlay.activeSelf)
            {
                effectOverlay.SetActive(false);
                return;
            }
            
            // Handle fading
            if (isFading)
            {
                float fadeDuration = isVisible ? fadeInDuration : fadeOutDuration;
                fadeProgress += Time.deltaTime / fadeDuration;
                
                if (fadeProgress >= 1f)
                {
                    fadeProgress = 1f;
                    isFading = false;
                    
                    // Disable overlay after fade out
                    if (!isVisible && effectOverlay != null)
                    {
                        effectOverlay.SetActive(false);
                    }
                }
                
                currentOpacity = isVisible ? fadeProgress : 1f - fadeProgress;
            }
            
            // Update shader properties
            if (timeBubble != null && overlayImage != null && overlayImage.material != null && mainCamera != null)
            {
                Material mat = overlayImage.material;
                
                // Convert world position to screen space
                Vector3 bubbleWorldPos = timeBubble.transform.position;
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(bubbleWorldPos);
                
                // Set shader properties
                mat.SetVector("_BubbleCenter", new Vector4(screenPoint.x, screenPoint.y, 0, 0));
                mat.SetFloat("_BubbleRadius", timeBubble.BubbleRadius * radiusMultiplier);
                mat.SetFloat("_EdgeSoftness", edgeSoftness);
                mat.SetColor("_BubbleColor", bubbleColor);
                mat.SetFloat("_Opacity", currentOpacity);
            }
        }
        
        /// <summary>
        /// Manually set bubble visibility (useful for testing)
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible)
                ShowEffect();
            else
                HideEffect();
        }
    }
}
