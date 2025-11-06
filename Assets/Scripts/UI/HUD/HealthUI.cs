using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    public class HealthUI : MonoBehaviour
    {
        [Header("Heart Sprites")]
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;
        
        [Header("UI Settings")]
        [SerializeField] private GameObject heartPrefab; // Image component with heart sprite
        [SerializeField] private Transform heartsContainer; // Parent transform for hearts
        [SerializeField] private float heartSpacing = 30f;
        [SerializeField] private float heartSize = 64f; // New: Size of each heart (increased from 32)
        
        private List<Image> heartImages = new List<Image>();
        private HealthManager healthManager;
        
        private void Start()
        {
            healthManager = FindFirstObjectByType<HealthManager>();
            
            if (healthManager != null)
            {
                healthManager.OnHealthChanged += UpdateHealthDisplay;
                
                // Initial display
                InitializeHearts(healthManager.MaxHealth);
                UpdateHealthDisplay(healthManager.CurrentHealth, healthManager.MaxHealth);
            }
        }
        
        private void OnDestroy()
        {
            if (healthManager != null)
            {
                healthManager.OnHealthChanged -= UpdateHealthDisplay;
            }
        }
        
        private void InitializeHearts(int maxHealth)
        {
            // Clear existing hearts
            foreach (var heart in heartImages)
            {
                if (heart != null)
                {
                    Destroy(heart.gameObject);
                }
            }
            heartImages.Clear();
            
            // Create heart sprites for max health
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject heartObj = null;
                
                if (heartPrefab != null)
                {
                    // Use prefab if provided
                    heartObj = Instantiate(heartPrefab, heartsContainer);
                }
                else
                {
                    // Create heart GameObject manually
                    heartObj = new GameObject($"Heart_{i}");
                    heartObj.transform.SetParent(heartsContainer);
                    
                    Image heartImage = heartObj.AddComponent<Image>();
                    heartImage.sprite = fullHeartSprite;
                    heartImage.preserveAspect = true;
                    
                    RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(heartSize, heartSize); // Use configurable size
                }
                
                // Position heart and resize if using prefab
                RectTransform rect = heartObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(i * heartSpacing, 0);
                rect.sizeDelta = new Vector2(heartSize, heartSize); // Apply size to all hearts
                
                // Get or add Image component
                Image img = heartObj.GetComponent<Image>();
                if (img == null)
                {
                    img = heartObj.AddComponent<Image>();
                }
                
                heartImages.Add(img);
            }
            
        }
        
        private void UpdateHealthDisplay(int currentHealth, int maxHealth)
        {
            // If max health changed, recreate hearts
            if (heartImages.Count != maxHealth)
            {
                InitializeHearts(maxHealth);
            }
            
            // Update heart sprites
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].sprite = i < currentHealth ? fullHeartSprite : emptyHeartSprite;
                }
            }
        }
    }
}
