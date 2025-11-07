using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ChronoDash.Managers {
    /// <summary>
    /// Manages animated background cycling for all menu scenes (MainMenu, Settings, HowToPlay).
    /// Uses DontDestroyOnLoad to persist across scenes and maintain timing.
    /// Fades between world backgrounds every 30 seconds.
    /// Follows Single Responsibility Principle - only handles menu backgrounds.
    /// </summary>
    public class MenuBackgroundManager : MonoBehaviour {
        [System.Serializable]
        public class BackgroundData {
            public string name;
            public Sprite sprite;
        }
        
        [Header("Background Sprites")]
        [SerializeField] private BackgroundData[] backgrounds;
        
        [Header("Timing")]
        [SerializeField] private float changeInterval = 30f;
        
        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 1.5f;
        
        [Header("UI References (Auto-found)")]
        private Image currentBackgroundImage;
        private Image nextBackgroundImage;
        private CanvasGroup currentCanvasGroup;
        private CanvasGroup nextCanvasGroup;
        
        private int currentBackgroundIndex = 0;
        private float timer = 0f;
        private bool isFading = false;
        
        public static MenuBackgroundManager Instance { get; private set; }
        
        private void Awake() {
            // Singleton pattern
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start() {
            if (backgrounds == null || backgrounds.Length == 0) {
                enabled = false;
                return;
            }
            
            // Start with random background
            currentBackgroundIndex = Random.Range(0, backgrounds.Length);
            
            // Find and setup background images in current scene
            SetupBackgroundImages();
        }
        
        private void Update() {
            if (backgrounds.Length <= 1 || isFading) return;
            
            timer += Time.deltaTime;
            
            if (timer >= changeInterval) {
                timer = 0f;
                StartCoroutine(FadeToNextBackground());
            }
        }
        
        /// <summary>
        /// Called when a new scene loads to re-find background images
        /// </summary>
        public void OnSceneLoaded() {
            SetupBackgroundImages();
        }
        
        private void SetupBackgroundImages() {
            // Find background images by name or tag
            GameObject bgContainer = GameObject.Find("MenuBackground");
            
            if (bgContainer == null) return;
            
            // Get both image layers
            Transform currentBG = bgContainer.transform.Find("CurrentBackground");
            Transform nextBG = bgContainer.transform.Find("NextBackground");
            
            if (currentBG == null || nextBG == null) return;
            
            currentBackgroundImage = currentBG.GetComponent<Image>();
            nextBackgroundImage = nextBG.GetComponent<Image>();
            currentCanvasGroup = currentBG.GetComponent<CanvasGroup>();
            nextCanvasGroup = nextBG.GetComponent<CanvasGroup>();
            
            if (currentBackgroundImage == null || nextBackgroundImage == null) return;
            
            // Set initial background
            currentBackgroundImage.sprite = backgrounds[currentBackgroundIndex].sprite;
            currentCanvasGroup.alpha = 1f;
            nextCanvasGroup.alpha = 0f;
        }
        
        private IEnumerator FadeToNextBackground() {
            if (isFading || backgrounds.Length <= 1) yield break;
            
            isFading = true;
            
            // Select next background (ensure it's different)
            int nextIndex = currentBackgroundIndex;
            do {
                nextIndex = Random.Range(0, backgrounds.Length);
            } while (nextIndex == currentBackgroundIndex && backgrounds.Length > 1);
                        
            // Set next background sprite
            if (nextBackgroundImage != null) {
                nextBackgroundImage.sprite = backgrounds[nextIndex].sprite;
            }
            
            // Fade out current, fade in next
            float elapsed = 0f;
            
            while (elapsed < fadeDuration) {
                elapsed += Time.deltaTime;
                float alpha = elapsed / fadeDuration;
                
                if (currentCanvasGroup != null) {
                    currentCanvasGroup.alpha = 1f - alpha;
                }
                
                if (nextCanvasGroup != null) {
                    nextCanvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            // Ensure final values
            if (currentCanvasGroup != null) currentCanvasGroup.alpha = 0f;
            if (nextCanvasGroup != null) nextCanvasGroup.alpha = 1f;
            
            // Swap references
            var tempImage = currentBackgroundImage;
            currentBackgroundImage = nextBackgroundImage;
            nextBackgroundImage = tempImage;
            
            var tempGroup = currentCanvasGroup;
            currentCanvasGroup = nextCanvasGroup;
            nextCanvasGroup = tempGroup;
            
            currentBackgroundIndex = nextIndex;
            isFading = false;
        }
        
        /// <summary>
        /// Manually trigger background change (for testing)
        /// </summary>
        public void ForceChangeBackground() {
            if (!isFading) {
                timer = changeInterval; // Will trigger on next Update
            }
        }
        
        /// <summary>
        /// Get current background name
        /// </summary>
        public string GetCurrentBackgroundName() {
            if (currentBackgroundIndex >= 0 && currentBackgroundIndex < backgrounds.Length) {
                return backgrounds[currentBackgroundIndex].name;
            }
            return "Unknown";
        }
        
        /// <summary>
        /// Get current background index for syncing with WorldManager
        /// </summary>
        public int GetCurrentBackgroundIndex() {
            return currentBackgroundIndex;
        }
    }
}
