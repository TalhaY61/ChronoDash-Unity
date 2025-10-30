using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using ChronoDash.Powerups;

namespace ChronoDash.UI
{
    public class PowerupNotification : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float slideDistance = 50f;
        
        [Header("Notification Text")]
        [SerializeField] private Color invincibilityColor = new Color(1f, 0.84f, 0f); // Gold
        [SerializeField] private Color speedColor = new Color(0f, 1f, 1f); // Cyan
        [SerializeField] private Color magnetColor = new Color(1f, 0.27f, 0f); // Orange-red
        [SerializeField] private Color shieldColor = new Color(0.53f, 0.81f, 0.98f); // Light blue
        [SerializeField] private Color multiply2xColor = new Color(0f, 1f, 0f); // Green
        [SerializeField] private Color healthColor = new Color(1f, 0.08f, 0.58f); // Pink
        
        private PowerupEffectsManager effectsManager;
        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private bool isShowingNotification = false;
        
        private class NotificationData
        {
            public string message;
            public Color color;
        }
        
        private void Start()
        {
            effectsManager = FindFirstObjectByType<PowerupEffectsManager>();
            
            if (effectsManager != null)
            {
                effectsManager.OnPowerupCollected += OnPowerupCollected;
                effectsManager.OnPowerupExpired += OnPowerupExpired;
            }
            else
            {
                Debug.LogError("❌ PowerupNotification: PowerupEffectsManager not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (effectsManager != null)
            {
                effectsManager.OnPowerupCollected -= OnPowerupCollected;
                effectsManager.OnPowerupExpired -= OnPowerupExpired;
            }
        }
        
        private void OnPowerupCollected(PowerupType type, int stackCount)
        {
            string message = GetPowerupCollectedMessage(type, stackCount);
            Color color = GetPowerupColor(type);
            
            // Add to queue
            notificationQueue.Enqueue(new NotificationData { message = message, color = color });
            
            // Start processing queue if not already showing
            if (!isShowingNotification)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }
        
        private void OnPowerupExpired(PowerupType type)
        {
            string message = GetPowerupExpiredMessage(type);
            Color color = GetPowerupColor(type);
            
            // Add to queue
            notificationQueue.Enqueue(new NotificationData { message = message, color = color });
            
            // Start processing queue if not already showing
            if (!isShowingNotification)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }
        
        private IEnumerator ProcessNotificationQueue()
        {
            isShowingNotification = true;
            
            while (notificationQueue.Count > 0)
            {
                NotificationData data = notificationQueue.Dequeue();
                yield return StartCoroutine(ShowNotification(data.message, data.color));
            }
            
            isShowingNotification = false;
        }
        
        private IEnumerator ShowNotification(string message, Color color)
        {
            GameObject notificationObj = null;
            
            if (notificationPrefab != null)
            {
                notificationObj = Instantiate(notificationPrefab, notificationContainer);
            }
            else
            {
                notificationObj = CreateNotificationManually();
            }
            
            if (notificationObj == null) yield break;
            
            // Get components
            TextMeshProUGUI text = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
            CanvasGroup canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = notificationObj.AddComponent<CanvasGroup>();
            }
            
            // Set text and color
            if (text != null)
            {
                text.text = message;
                text.color = color;
            }
            
            RectTransform rect = notificationObj.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, slideDistance);
            
            // Fade in and slide up
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInTime;
                
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            rect.anchoredPosition = endPos;
            
            // Wait
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            elapsed = 0f;
            Vector2 fadeOutStartPos = rect.anchoredPosition;
            Vector2 fadeOutEndPos = fadeOutStartPos + new Vector2(0, slideDistance * 0.5f);
            
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutTime;
                
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                rect.anchoredPosition = Vector2.Lerp(fadeOutStartPos, fadeOutEndPos, t);
                
                yield return null;
            }
            
            Destroy(notificationObj);
        }
        
        private GameObject CreateNotificationManually()
        {
            GameObject notificationObj = new GameObject("Notification");
            notificationObj.transform.SetParent(notificationContainer);
            
            RectTransform rect = notificationObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 50);
            rect.anchoredPosition = Vector2.zero;
            
            // Add background image
            Image bg = notificationObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(notificationObj.transform);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            return notificationObj;
        }
        
        private string GetPowerupCollectedMessage(PowerupType type, int stackCount)
        {
            string baseName = type switch
            {
                PowerupType.Invincibility => "INVINCIBILITY",
                PowerupType.Speed => "SPEED BOOST",
                PowerupType.Magnet => "MAGNET",
                PowerupType.Shield => "SHIELD",
                PowerupType.Multiply2x => "2X SCORE",
                PowerupType.Health => "MAX HEALTH +1",
                _ => "POWERUP"
            };
            
            if (stackCount > 1)
            {
                return $"{baseName} x{stackCount}";
            }
            
            return baseName;
        }
        
        private string GetPowerupExpiredMessage(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => "INVINCIBILITY ENDED",
                PowerupType.Speed => "SPEED BOOST ENDED",
                PowerupType.Magnet => "MAGNET ENDED",
                PowerupType.Shield => "SHIELD DEPLETED",
                PowerupType.Multiply2x => "2X SCORE ENDED",
                _ => "POWERUP ENDED"
            };
        }
        
        private Color GetPowerupColor(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => invincibilityColor,
                PowerupType.Speed => speedColor,
                PowerupType.Magnet => magnetColor,
                PowerupType.Shield => shieldColor,
                PowerupType.Multiply2x => multiply2xColor,
                PowerupType.Health => healthColor,
                _ => Color.white
            };
        }
    }
}
