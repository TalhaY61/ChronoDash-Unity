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
            public Image timerFillImage; // Radial timer overlay
            public TextMeshProUGUI timerText; // Countdown text
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
            
            // If using in-game sprites, try to load them from PowerupManager
            if (useInGameSprites)
            {
                LoadSpritesFromPowerupManager();
            }
        }
        
        private void LoadSpritesFromPowerupManager()
        {
            PowerupManager powerupManager = FindFirstObjectByType<PowerupManager>();
            if (powerupManager == null) return;
            
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
                    
                    // Update radial timer fill
                    if (slot.timerFillImage != null)
                    {
                        float timeRatio = effect.remainingTime / effect.duration;
                        slot.timerFillImage.fillAmount = timeRatio; // Drains from 1 to 0
                    }
                    
                    // Update countdown timer text
                    if (slot.timerText != null)
                    {
                        slot.timerText.text = Mathf.Ceil(effect.remainingTime).ToString("F0") + "s";
                    }
                    
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
            Image timerFill = slotObj.transform.Find("TimerFill")?.GetComponent<Image>();
            TextMeshProUGUI timerTxt = slotObj.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stackTxt = slotObj.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();
            
            // If using manual creation, components are already set
            if (iconImg == null) iconImg = slotObj.GetComponent<Image>();
            
            // Set icon sprite
            if (iconImg != null)
            {
                iconImg.sprite = GetPowerupIcon(type);
                iconImg.color = Color.white; // Full opacity
            }
            
            // Configure timer text
            if (timerTxt != null)
            {
                timerTxt.text = Mathf.Ceil(duration).ToString("F0") + "s";
            }
            
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
                timerFillImage = timerFill,
                timerText = timerTxt,
                stackText = stackTxt,
                type = type,
                duration = duration
            };
            
            activeSlots[type] = slot;
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
            
            // Create radial timer fill overlay
            GameObject timerObj = new GameObject("TimerFill");
            timerObj.transform.SetParent(slotObj.transform, false);
            
            Image timerFillImg = timerObj.AddComponent<Image>();
            timerFillImg.sprite = GetPowerupIcon(type); // Use same sprite for fill effect
            timerFillImg.type = Image.Type.Filled;
            timerFillImg.fillMethod = Image.FillMethod.Radial360;
            timerFillImg.fillOrigin = (int)Image.Origin360.Top;
            timerFillImg.fillClockwise = false;
            timerFillImg.fillAmount = 1f;
            timerFillImg.color = new Color(0f, 0f, 0f, 0.5f); // Dark overlay
            
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = Vector2.zero;
            timerRect.anchorMax = Vector2.one;
            timerRect.sizeDelta = Vector2.zero;
            timerRect.anchoredPosition = Vector2.zero;
            
            // Create countdown timer text (center)
            GameObject timerTextObj = new GameObject("TimerText");
            timerTextObj.transform.SetParent(slotObj.transform, false);
            
            TextMeshProUGUI timerTxt = timerTextObj.AddComponent<TextMeshProUGUI>();
            timerTxt.fontSize = 18;
            timerTxt.color = Color.white;
            timerTxt.alignment = TextAlignmentOptions.Center;
            timerTxt.fontStyle = FontStyles.Bold;
            timerTxt.enableAutoSizing = false;
            
            RectTransform timerTextRect = timerTextObj.GetComponent<RectTransform>();
            timerTextRect.anchorMin = Vector2.zero;
            timerTextRect.anchorMax = Vector2.one;
            timerTextRect.sizeDelta = Vector2.zero;
            timerTextRect.anchoredPosition = Vector2.zero;
            
            // Create stack count text (bottom-right corner, bigger size)
            GameObject stackTextObj = new GameObject("StackText");
            stackTextObj.transform.SetParent(slotObj.transform, false);
            
            TextMeshProUGUI stackTxt = stackTextObj.AddComponent<TextMeshProUGUI>();
            stackTxt.text = "x1";
            stackTxt.fontSize = 20; // Increased from 16 to 20
            stackTxt.color = Color.white;
            stackTxt.alignment = TextAlignmentOptions.BottomRight;
            stackTxt.fontStyle = FontStyles.Bold;
            stackTxt.enableAutoSizing = false;
            
            // Add outline for better visibility
            var outline = stackTextObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
            
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
                PowerupType.Heart => healthIcon,
                _ => null
            };
        }
    }
}
