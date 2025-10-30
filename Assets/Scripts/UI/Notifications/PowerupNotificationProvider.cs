using UnityEngine;
using ChronoDash.Powerups;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Provides powerup-specific notifications.
    /// Follows Single Responsibility Principle - only handles powerup notifications.
    /// Follows Open/Closed Principle - can be extended without modifying NotificationService.
    /// </summary>
    public class PowerupNotificationProvider : INotificationProvider
    {
        private NotificationService service;
        private PowerupEffectsManager effectsManager;
        
        // Color configuration
        private readonly Color invincibilityColor = new Color(1f, 0.84f, 0f); // Gold
        private readonly Color slowdownColor = new Color(1f, 0.27f, 0f); // Orange-red
        private readonly Color magnetColor = new Color(0.53f, 0.81f, 0.98f); // Light blue
        private readonly Color shieldColor = new Color(0.68f, 0.85f, 0.90f); // Light cyan
        private readonly Color multiply2xColor = new Color(0f, 1f, 0f); // Green
        private readonly Color healthColor = new Color(1f, 0.08f, 0.58f); // Pink
        
        public void Initialize(NotificationService notificationService)
        {
            service = notificationService;
            effectsManager = Object.FindFirstObjectByType<PowerupEffectsManager>();
            
            if (effectsManager != null)
            {
                effectsManager.OnPowerupCollected += OnPowerupCollected;
                effectsManager.OnPowerupExpired += OnPowerupExpired;
            }
            else
            {
                Debug.LogWarning("⚠️ PowerupNotificationProvider: PowerupEffectsManager not found!");
            }
        }
        
        public void Cleanup()
        {
            if (effectsManager != null)
            {
                effectsManager.OnPowerupCollected -= OnPowerupCollected;
                effectsManager.OnPowerupExpired -= OnPowerupExpired;
            }
        }
        
        private void OnPowerupCollected(PowerupType type, int stackCount)
        {
            string message = GetCollectedMessage(type, stackCount);
            Color color = GetColor(type);
            service.ShowNotification(message, color);
        }
        
        private void OnPowerupExpired(PowerupType type)
        {
            string message = GetExpiredMessage(type);
            Color color = GetColor(type);
            service.ShowNotification(message, color);
        }
        
        private string GetCollectedMessage(PowerupType type, int stackCount)
        {
            string baseName = type switch
            {
                PowerupType.Invincibility => "INVINCIBILITY",
                PowerupType.Speed => "SPEED TRAP",
                PowerupType.Magnet => "MAGNET",
                PowerupType.Shield => "SHIELD",
                PowerupType.Multiply2x => "2X SCORE",
                PowerupType.Health => "MAX HEALTH +1",
                _ => "POWERUP"
            };
            
            return stackCount > 1 ? $"{baseName} x{stackCount}" : baseName;
        }
        
        private string GetExpiredMessage(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => "INVINCIBILITY ENDED",
                PowerupType.Speed => "SPEED TRAP ENDED",
                PowerupType.Magnet => "MAGNET ENDED",
                PowerupType.Shield => "SHIELD DEPLETED",
                PowerupType.Multiply2x => "2X SCORE ENDED",
                _ => "POWERUP ENDED"
            };
        }
        
        private Color GetColor(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => invincibilityColor,
                PowerupType.Speed => slowdownColor,
                PowerupType.Magnet => magnetColor,
                PowerupType.Shield => shieldColor,
                PowerupType.Multiply2x => multiply2xColor,
                PowerupType.Health => healthColor,
                _ => Color.white
            };
        }
    }
}
