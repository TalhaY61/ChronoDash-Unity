using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace ChronoDash.UI
{
    /// <summary>
    /// Simple notification UI component - displays text notifications with animations.
    /// Place this in your HUD Canvas, configure styling, and call ShowNotification() from anywhere.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float fontSize = 24f;
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color borderColor = Color.white;
        [SerializeField] private float borderWidth = 2f;
        [SerializeField] private Vector2 size = new Vector2(400, 60);
        
        [Header("Animation")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float slideDistance = 50f;
        
        private Queue<QueuedNotification> queue = new Queue<QueuedNotification>();
        private bool isShowing = false;
        
        private struct QueuedNotification
        {
            public string message;
            public Color textColor;
        }
        
        public void ShowNotification(string message, Color textColor)
        {
            queue.Enqueue(new QueuedNotification { message = message, textColor = textColor });
            
            if (!isShowing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        private IEnumerator ProcessQueue()
        {
            isShowing = true;
            
            while (queue.Count > 0)
            {
                var notification = queue.Dequeue();
                yield return StartCoroutine(DisplayNotification(notification.message, notification.textColor));
            }
            
            isShowing = false;
        }
        
        private IEnumerator DisplayNotification(string message, Color textColor)
        {
            // Create notification
            GameObject notificationObj = CreateNotification(message, textColor);
            if (notificationObj == null) yield break;
            
            CanvasGroup canvasGroup = notificationObj.AddComponent<CanvasGroup>();
            RectTransform rect = notificationObj.GetComponent<RectTransform>();
            
            // Fade in and slide up
            Vector2 startPos = rect.anchoredPosition;
            Vector2 midPos = startPos + new Vector2(0, slideDistance);
            
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInTime;
                canvasGroup.alpha = t;
                rect.anchoredPosition = Vector2.Lerp(startPos, midPos, t);
                yield return null;
            }
            
            // Wait
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            Vector2 endPos = midPos + new Vector2(0, slideDistance * 0.5f);
            elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutTime;
                canvasGroup.alpha = 1f - t;
                rect.anchoredPosition = Vector2.Lerp(midPos, endPos, t);
                yield return null;
            }
            
            Destroy(notificationObj);
        }
        
        private GameObject CreateNotification(string message, Color textColor)
        {
            GameObject obj;

            obj = CreateNotificationFromScratch(message, textColor);

            // Find and update the text component
            TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
                text.color = textColor;
                
                // Apply font if provided and not using prefab
                if (font != null)
                {
                    text.font = font;
                    text.fontSize = fontSize;
                    text.alignment = TextAlignmentOptions.Center;
                    text.fontStyle = FontStyles.Bold;
                }
            }
            
            return obj;
        }
        
        private GameObject CreateNotificationFromScratch(string message, Color textColor)
        {
            GameObject obj = new GameObject("Notification");
            obj.transform.SetParent(notificationContainer, false);
            
            // Background
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            
            Image bg = obj.AddComponent<Image>();
            
            // Use custom sprite if provided, otherwise solid color
            if (backgroundSprite != null)
            {
                bg.sprite = backgroundSprite;
                bg.type = Image.Type.Sliced; // For 9-slice scaling (if your sprite supports it)
                bg.color = Color.white; // DON'T tint the sprite - keep it pure white to show original colors
            }
            else
            {
                bg.color = backgroundColor; // Solid color fallback (only used when no sprite)
            }
            
            // Border (optional, you might not need this with a custom sprite)
            if (borderWidth > 0 && backgroundSprite == null)
            {
                Outline outline = obj.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(borderWidth, borderWidth);
            }
            
            // Disable layout element to prevent Vertical Layout Group from repositioning
            var layoutElement = obj.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.ignoreLayout = true;
            
            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.text = message;
            text.color = textColor;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            return obj;
        }
    }
}
