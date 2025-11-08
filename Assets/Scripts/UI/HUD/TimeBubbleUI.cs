using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Abilities;

namespace ChronoDash.UI
{
    /// <summary>
    /// Manages the Time Bubble icon display in the PowerupPanel
    /// Shows at the top of powerup list when active or on cooldown
    /// - Active: Full color with duration countdown
    /// - Cooldown: Dimmed/grayed with cooldown timer
    /// - Ready: Hidden until activated again
    /// </summary>
    public class TimeBubbleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TimeBubble timeBubble;
        [SerializeField] private Transform powerupContainer; // Same container as PowerupUI
        [SerializeField] private Sprite timeBubbleIcon;
        
        [Header("UI Settings")]
        [SerializeField] private float slotSize = 80f;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Dimmed gray
        
        // UI Components
        private GameObject slotObject;
        private Image iconImage;
        private Image timerFillImage; // Radial fill for duration/cooldown
        private TextMeshProUGUI timerText; // Shows remaining time
        
        // State
        private bool isDisplayed = false;
        
        private void Start()
        {
            if (timeBubble == null)
            {
                timeBubble = FindFirstObjectByType<TimeBubble>();
            }
            
            if (timeBubble != null)
            {
                // Subscribe to events
                timeBubble.OnBubbleStateChanged += OnBubbleStateChanged;
            }
            else
            {
                UnityEngine.Debug.LogError("TimeBubbleUI: TimeBubble reference not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (timeBubble != null)
            {
                timeBubble.OnBubbleStateChanged -= OnBubbleStateChanged;
            }
        }
        
        private void Update()
        {
            if (!isDisplayed || timeBubble == null) return;
            
            if (timeBubble.IsActive)
            {
                // Active state - show duration countdown
                UpdateActiveDisplay();
            }
            else if (timeBubble.IsOnCooldown)
            {
                // Cooldown state - show cooldown countdown
                UpdateCooldownDisplay();
            }
            else
            {
                // Not active and not on cooldown - should be hidden
                HideTimeBubbleSlot();
            }
        }
        
        private void OnBubbleStateChanged(bool isActive)
        {
            if (isActive)
            {
                // Bubble activated - show at top
                if (!isDisplayed)
                {
                    CreateTimeBubbleSlot();
                }
                SetActiveAppearance();
            }
            else
            {
                // Bubble deactivated - switch to cooldown appearance
                if (isDisplayed)
                {
                    SetCooldownAppearance();
                }
            }
        }
        
        private void CreateTimeBubbleSlot()
        {
            if (slotObject != null) return; // Already created
            
            // Create slot container
            slotObject = new GameObject("TimeBubbleSlot");
            slotObject.transform.SetParent(powerupContainer, false);
            
            // IMPORTANT: Set as first sibling to appear at TOP
            slotObject.transform.SetSiblingIndex(0);
            
            RectTransform slotRect = slotObject.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);
            
            // Create icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObject.transform, false);
            
            iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = timeBubbleIcon;
            iconImage.preserveAspect = true;
            iconImage.color = activeColor;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;
            
            // Create radial timer fill overlay
            GameObject timerObj = new GameObject("TimerFill");
            timerObj.transform.SetParent(slotObject.transform, false);
            
            timerFillImage = timerObj.AddComponent<Image>();
            timerFillImage.sprite = timeBubbleIcon; // Use same sprite for fill effect
            timerFillImage.type = Image.Type.Filled;
            timerFillImage.fillMethod = Image.FillMethod.Radial360;
            timerFillImage.fillOrigin = (int)Image.Origin360.Top;
            timerFillImage.fillClockwise = false;
            timerFillImage.fillAmount = 1f;
            timerFillImage.color = new Color(0f, 0f, 0f, 0.5f); // Dark overlay
            
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = Vector2.zero;
            timerRect.anchorMax = Vector2.one;
            timerRect.sizeDelta = Vector2.zero;
            timerRect.anchoredPosition = Vector2.zero;
            
            // Create timer text (shows remaining seconds)
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(slotObject.transform, false);
            
            timerText = textObj.AddComponent<TextMeshProUGUI>();
            timerText.fontSize = 16;
            timerText.color = Color.white;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.fontStyle = FontStyles.Bold;
            timerText.enableAutoSizing = false;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            isDisplayed = true;
        }
        
        private void SetActiveAppearance()
        {
            if (iconImage != null)
            {
                iconImage.color = activeColor; // Full color
            }
            
            if (timerFillImage != null)
            {
                timerFillImage.color = new Color(0f, 0f, 0f, 0.5f); // Dark overlay for duration
            }
        }
        
        private void SetCooldownAppearance()
        {
            if (iconImage != null)
            {
                iconImage.color = cooldownColor; // Dimmed gray
            }
            
            if (timerFillImage != null)
            {
                timerFillImage.color = new Color(0f, 0f, 0f, 0.7f); // Darker overlay for cooldown
            }
        }
        
        private void UpdateActiveDisplay()
        {
            float activeRemaining = timeBubble.ActiveTimeRemaining;
            float totalDuration = timeBubble.Duration;
            
            if (timerFillImage != null)
            {
                // Fill drains from 1 to 0 as duration decreases
                float fillRatio = Mathf.Clamp01(activeRemaining / totalDuration);
                timerFillImage.fillAmount = fillRatio;
            }
            
            if (timerText != null)
            {
                // Show remaining seconds
                timerText.text = Mathf.Ceil(activeRemaining).ToString("F0") + "s";
            }
        }
        
        private void UpdateCooldownDisplay()
        {
            float cooldownRemaining = timeBubble.CooldownRemaining;
            float cooldownProgress = timeBubble.CooldownProgress;
            
            if (timerFillImage != null)
            {
                // Fill grows from 0 to 1 during cooldown (showing progress toward ready)
                timerFillImage.fillAmount = 1f - cooldownProgress;
            }
            
            if (timerText != null)
            {
                // Show cooldown remaining
                timerText.text = Mathf.Ceil(cooldownRemaining).ToString("F0") + "s";
            }
        }
        
        private void HideTimeBubbleSlot()
        {
            if (slotObject != null)
            {
                Destroy(slotObject);
                slotObject = null;
                iconImage = null;
                timerFillImage = null;
                timerText = null;
            }
            
            isDisplayed = false;
        }
        
        /// <summary>
        /// Force the Time Bubble slot to always be at the top of the powerup container
        /// Call this if other powerups are added/removed to maintain ordering
        /// </summary>
        public void EnsureTopPosition()
        {
            if (slotObject != null && slotObject.transform.GetSiblingIndex() != 0)
            {
                slotObject.transform.SetSiblingIndex(0);
            }
        }
    }
}
