using UnityEngine;
using ChronoDash.Gemstones;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Provides gemstone-specific notifications.
    /// Follows Single Responsibility Principle - only handles gemstone notifications.
    /// Follows Open/Closed Principle - can be extended without modifying NotificationService.
    /// </summary>
    public class GemstoneNotificationProvider : INotificationProvider
    {
        private NotificationService service;
        private GemstoneManager gemstoneManager;
        
        // Color configuration
        private readonly Color blueGemColor = new Color(0.2f, 0.6f, 1f); // Blue
        private readonly Color greenGemColor = new Color(0.2f, 1f, 0.4f); // Green
        private readonly Color redGemColor = new Color(1f, 0.3f, 0.3f); // Red
        
        public void Initialize(NotificationService notificationService)
        {
            service = notificationService;
            gemstoneManager = Object.FindFirstObjectByType<GemstoneManager>();
            
            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollectedWithGem += OnGemstoneCollected;
            }
            else
            {
                Debug.LogWarning("⚠️ GemstoneNotificationProvider: GemstoneManager not found!");
            }
        }
        
        public void Cleanup()
        {
            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollectedWithGem -= OnGemstoneCollected;
            }
        }
        
        private void OnGemstoneCollected(Gemstone gemstone)
        {
            string message = GetMessage(gemstone.Type, gemstone.ScoreValue);
            Color color = GetColor(gemstone.Type);
            service.ShowNotification(message, color);
        }
        
        private string GetMessage(GemstoneType type, int scoreValue)
        {
            return type switch
            {
                GemstoneType.Blue => $"+{scoreValue} POINTS",
                GemstoneType.Green => $"+{scoreValue} POINTS",
                GemstoneType.Red => $"+{scoreValue} POINTS + HEAL",
                _ => $"+{scoreValue} POINTS"
            };
        }
        
        private Color GetColor(GemstoneType type)
        {
            return type switch
            {
                GemstoneType.Blue => blueGemColor,
                GemstoneType.Green => greenGemColor,
                GemstoneType.Red => redGemColor,
                _ => Color.white
            };
        }
    }
}
