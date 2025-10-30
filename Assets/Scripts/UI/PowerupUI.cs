using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using ChronoDash.Powerups;

namespace ChronoDash.UI
{
    public class PowerupUI : MonoBehaviour
    {
        [Header("Powerup Icon Sprites")]
        [SerializeField] private Sprite invincibilityIcon;
        [SerializeField] private Sprite speedIcon;
        [SerializeField] private Sprite magnetIcon;
        [SerializeField] private Sprite shieldIcon;
        [SerializeField] private Sprite multiply2xIcon;
        [SerializeField] private Sprite healthIcon;
        
        [Header("UI Settings")]
        [SerializeField] private GameObject powerupSlotPrefab; // Prefab with Image, Radial Timer, Stack Text
        [SerializeField] private Transform powerupContainer; // Vertical layout for powerup slots
        [SerializeField] private float slotSize = 80f; // Size of each powerup icon (increased from 50)
        [SerializeField] private bool useInGameSprites = true; // If true, gets sprites from powerup prefabs
        
        private Dictionary<PowerupType, PowerupSlot> activeSlots = new Dictionary<PowerupType, PowerupSlot>();
        private PowerupEffectsManager effectsManager;
        
        private class PowerupSlot
        {
            public GameObject slotObject;
            public Image iconImage;
            public Image timerFillImage; // Radial timer
            public TextMeshProUGUI stackText;
            public PowerupType type;
            public float duration;
        }
        
        private void Start()
        {
            effectsManager = FindFirstObjectByType<PowerupEffectsManager>();
            
            if (effectsManager != null)
            {
                effectsManager.OnPowerupActivated += OnPowerupActivated;
                effectsManager.OnPowerupExpired += OnPowerupExpired;
            }
            else
            {
                Debug.LogError("❌ PowerupUI: PowerupEffectsManager not found!");
            }
            
            // If using in-game sprites, try to load them from PowerupManager
            if (useInGameSprites)
            {
                LoadSpritesFromPowerupManager();
            }
        }
        
        private void LoadSpritesFromPowerupManager()
        {
            PowerupManager powerupManager = FindFirstObjectByType<PowerupManager>();
            if (powerupManager == null)
            {
                Debug.LogWarning("⚠️ PowerupUI: PowerupManager not found, cannot load sprites automatically");
                return;
            }
            
            // Use reflection to get prefab fields from PowerupManager
            var managerType = powerupManager.GetType();
            var fields = managerType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(GameObject))
                {
                    GameObject prefab = field.GetValue(powerupManager) as GameObject;
                    if (prefab != null)
                    {
                        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && spriteRenderer.sprite != null)
                        {
                            string fieldName = field.Name.ToLower();
                            
                            if (fieldName.Contains("invincibility") && invincibilityIcon == null)
                                invincibilityIcon = spriteRenderer.sprite;
                            else if (fieldName.Contains("speed") && speedIcon == null)
                                speedIcon = spriteRenderer.sprite;
                            else if (fieldName.Contains("magnet") && magnetIcon == null)
                                magnetIcon = spriteRenderer.sprite;
                            else if (fieldName.Contains("shield") && shieldIcon == null)
                                shieldIcon = spriteRenderer.sprite;
                            else if (fieldName.Contains("multiply") && multiply2xIcon == null)
                                multiply2xIcon = spriteRenderer.sprite;
                            else if (fieldName.Contains("health") && healthIcon == null)
                                healthIcon = spriteRenderer.sprite;
                        }
                    }
                }
            }
            
            Debug.Log("✨ PowerupUI: Loaded sprites from PowerupManager prefabs");
        }
        
        private void OnDestroy()
        {
            if (effectsManager != null)
            {
                effectsManager.OnPowerupActivated -= OnPowerupActivated;
                effectsManager.OnPowerupExpired -= OnPowerupExpired;
            }
        }
        
        private void Update()
        {
            if (effectsManager == null) return;
            
            // Update timer displays for all active powerups
            var activePowerups = effectsManager.GetActivePowerups();
            
            foreach (var kvp in activePowerups)
            {
                PowerupType type = kvp.Key;
                var effect = kvp.Value;
                
                if (activeSlots.ContainsKey(type))
                {
                    PowerupSlot slot = activeSlots[type];
                    
                    // Fade icon alpha as time runs out (instead of overlay)
                    if (slot.iconImage != null)
                    {
                        float timeRatio = effect.remainingTime / effect.duration;
                        // Fade from full opacity (1.0) to 50% opacity (0.5) as timer runs out
                        float alpha = Mathf.Lerp(0.5f, 1f, timeRatio);
                        Color iconColor = slot.iconImage.color;
                        iconColor.a = alpha;
                        slot.iconImage.color = iconColor;
                    }
                    
                    // Note: timerFillImage removed - using icon alpha instead
                    
                    // Update stack count text
                    if (slot.stackText != null)
                    {
                        if (effect.stackCount > 1)
                        {
                            slot.stackText.text = $"x{effect.stackCount}";
                            slot.stackText.gameObject.SetActive(true);
                        }
                        else
                        {
                            slot.stackText.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        
        private void OnPowerupActivated(PowerupType type, float duration, int stackCount)
        {
            if (activeSlots.ContainsKey(type))
            {
                // Update existing slot
                PowerupSlot slot = activeSlots[type];
                slot.duration = duration;
                
                if (slot.stackText != null && stackCount > 1)
                {
                    slot.stackText.text = $"x{stackCount}";
                    slot.stackText.gameObject.SetActive(true);
                }
            }
            else
            {
                // Create new slot
                CreatePowerupSlot(type, duration, stackCount);
            }
        }
        
        private void OnPowerupExpired(PowerupType type)
        {
            if (activeSlots.ContainsKey(type))
            {
                PowerupSlot slot = activeSlots[type];
                
                if (slot.slotObject != null)
                {
                    Destroy(slot.slotObject);
                }
                
                activeSlots.Remove(type);
                Debug.Log($"⏱️ PowerupUI: Removed {type} slot");
            }
        }
        
        private void CreatePowerupSlot(PowerupType type, float duration, int stackCount)
        {
            GameObject slotObj = null;
            
            if (powerupSlotPrefab != null)
            {
                // Use prefab
                slotObj = Instantiate(powerupSlotPrefab, powerupContainer);
            }
            else
            {
                // Create slot manually
                slotObj = CreateSlotManually(type);
            }
            
            if (slotObj == null) return;
            
            // Get components
            Image iconImg = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI stackTxt = slotObj.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();
            
            // If using manual creation, components are already set
            if (iconImg == null) iconImg = slotObj.GetComponent<Image>();
            
            // Set icon sprite
            if (iconImg != null)
            {
                iconImg.sprite = GetPowerupIcon(type);
                iconImg.color = Color.white; // Full opacity initially
            }
            
            // Note: Timer overlay removed - using icon alpha fading instead
            
            // Configure stack text
            if (stackTxt != null && stackCount > 1)
            {
                stackTxt.text = $"x{stackCount}";
                stackTxt.gameObject.SetActive(true);
            }
            else if (stackTxt != null)
            {
                stackTxt.gameObject.SetActive(false);
            }
            
            // Store slot
            PowerupSlot slot = new PowerupSlot
            {
                slotObject = slotObj,
                iconImage = iconImg,
                timerFillImage = null, // No longer using timer overlay
                stackText = stackTxt,
                type = type,
                duration = duration
            };
            
            activeSlots[type] = slot;
            Debug.Log($"✨ PowerupUI: Created slot for {type}");
        }
        
        private GameObject CreateSlotManually(PowerupType type)
        {
            // Create slot container
            GameObject slotObj = new GameObject($"PowerupSlot_{type}");
            slotObj.transform.SetParent(powerupContainer);
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);
            
            // Create icon background
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform);
            
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GetPowerupIcon(type);
            iconImg.preserveAspect = true;
            iconImg.color = Color.white; // Ensure no tinting
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;
            
            // NOTE: Timer overlay removed - icons are fully transparent now
            // Timer functionality moved to alpha blending on the icon itself
            
            // Create stack count text
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(slotObj.transform);
            
            TextMeshProUGUI stackTxt = stackTextObj.AddComponent<TextMeshProUGUI>();
            stackTxt.text = "x1";
            stackTxt.fontSize = 16;
            stackTxt.color = Color.white;
            stackTxt.alignment = TextAlignmentOptions.BottomRight;
            stackTxt.fontStyle = FontStyles.Bold;
            
            RectTransform stackRect = stackTextObj.GetComponent<RectTransform>();
            stackRect.anchorMin = Vector2.zero;
            stackRect.anchorMax = Vector2.one;
            stackRect.sizeDelta = new Vector2(-5, -5);
            stackRect.anchoredPosition = Vector2.zero;
            
            stackTextObj.SetActive(false);
            
            return slotObj;
        }
        
        private Sprite GetPowerupIcon(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => invincibilityIcon,
                PowerupType.Speed => speedIcon,
                PowerupType.Magnet => magnetIcon,
                PowerupType.Shield => shieldIcon,
                PowerupType.Multiply2x => multiply2xIcon,
                PowerupType.Health => healthIcon,
                _ => null
            };
        }
    }
}
