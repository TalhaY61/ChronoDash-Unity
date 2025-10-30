using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Core notification service responsible for displaying notifications.
    /// Follows Single Responsibility Principle - only handles display logic.
    /// Follows Dependency Inversion Principle - depends on INotificationData interface.
    /// </summary>
    public class NotificationService : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float slideDistance = 50f;
        
        private Queue<INotificationData> notificationQueue = new Queue<INotificationData>();
        private bool isShowingNotification = false;
        private List<INotificationProvider> providers = new List<INotificationProvider>();
        
        private void Start()
        {
            InitializeProviders();
        }
        
        private void OnDestroy()
        {
            CleanupProviders();
        }
        
        /// <summary>
        /// Registers notification providers. Add new providers here to extend functionality.
        /// </summary>
        private void InitializeProviders()
        {
            // Register all providers
            RegisterProvider(new PowerupNotificationProvider());
            RegisterProvider(new GemstoneNotificationProvider());
        }
        
        private void RegisterProvider(INotificationProvider provider)
        {
            providers.Add(provider);
            provider.Initialize(this);
        }
        
        private void CleanupProviders()
        {
            foreach (var provider in providers)
            {
                provider.Cleanup();
            }
            providers.Clear();
        }
        
        /// <summary>
        /// Public method to show a notification. Can be called by any provider or external system.
        /// </summary>
        public void ShowNotification(INotificationData data)
        {
            notificationQueue.Enqueue(data);
            
            if (!isShowingNotification)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }
        
        /// <summary>
        /// Overload for simple notifications without creating a data object.
        /// </summary>
        public void ShowNotification(string message, Color color)
        {
            ShowNotification(new NotificationData(message, color));
        }
        
        private IEnumerator ProcessNotificationQueue()
        {
            isShowingNotification = true;
            
            while (notificationQueue.Count > 0)
            {
                INotificationData data = notificationQueue.Dequeue();
                yield return StartCoroutine(DisplayNotification(data));
            }
            
            isShowingNotification = false;
        }
        
        private IEnumerator DisplayNotification(INotificationData data)
        {
            GameObject notificationObj = CreateNotificationObject();
            if (notificationObj == null) yield break;
            
            // Setup notification
            TextMeshProUGUI text = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
            CanvasGroup canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = notificationObj.AddComponent<CanvasGroup>();
            }
            
            if (text != null)
            {
                text.text = data.Message;
                text.color = data.Color;
            }
            
            // Animate
            yield return StartCoroutine(AnimateNotification(notificationObj, canvasGroup));
            
            Destroy(notificationObj);
        }
        
        private GameObject CreateNotificationObject()
        {
            if (notificationPrefab != null)
            {
                return Instantiate(notificationPrefab, notificationContainer);
            }
            
            return CreateNotificationManually();
        }
        
        private IEnumerator AnimateNotification(GameObject notificationObj, CanvasGroup canvasGroup)
        {
            RectTransform rect = notificationObj.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition;
            Vector2 midPos = startPos + new Vector2(0, slideDistance);
            
            // Fade in and slide up
            yield return StartCoroutine(FadeIn(canvasGroup, rect, startPos, midPos));
            
            // Wait
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out and continue sliding
            Vector2 endPos = midPos + new Vector2(0, slideDistance * 0.5f);
            yield return StartCoroutine(FadeOut(canvasGroup, rect, midPos, endPos));
        }
        
        private IEnumerator FadeIn(CanvasGroup canvasGroup, RectTransform rect, Vector2 startPos, Vector2 endPos)
        {
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
        }
        
        private IEnumerator FadeOut(CanvasGroup canvasGroup, RectTransform rect, Vector2 startPos, Vector2 endPos)
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutTime;
                
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                
                yield return null;
            }
        }
        
        private GameObject CreateNotificationManually()
        {
            GameObject notificationObj = new GameObject("Notification");
            notificationObj.transform.SetParent(notificationContainer);
            
            RectTransform rect = notificationObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 50);
            rect.anchoredPosition = Vector2.zero;
            
            Image bg = notificationObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);
            
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
    }
}
