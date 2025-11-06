using UnityEngine;
using ChronoDash.Powerups;

namespace ChronoDash.UI.Notifications {
    public class PowerupNotifications : MonoBehaviour {
        [Header("References")]
        [SerializeField] private NotificationUI notificationUI;

        [Header("Powerup Colors")]
        [SerializeField] private Color invincibilityColor;
        [SerializeField] private Color speedColor;
        [SerializeField] private Color magnetColor;
        [SerializeField] private Color shieldColor;
        [SerializeField] private Color multiply2xColor;
        [SerializeField] private Color healthColor;

        private PowerupEffectsManager powerupManager;
        
        private void Start()
        {
            if (notificationUI == null)
                notificationUI = GetComponent<NotificationUI>();
            
            powerupManager = FindFirstObjectByType<PowerupEffectsManager>();
            
            if (powerupManager != null)
            {
                powerupManager.OnPowerupCollected += OnPowerupCollected;
                powerupManager.OnPowerupExpired += OnPowerupExpired;
            }
        }
        
        private void OnDestroy()
        {
            if (powerupManager != null)
            {
                powerupManager.OnPowerupCollected -= OnPowerupCollected;
                powerupManager.OnPowerupExpired -= OnPowerupExpired;
            }
        }
        
        private void OnPowerupCollected(PowerupType type, int stackCount)
        {
            string message = GetCollectedMessage(type, stackCount);
            Color color = GetColor(type);
            notificationUI.ShowNotification(message, color);
        }
        
        private void OnPowerupExpired(PowerupType type)
        {
            string message = GetExpiredMessage(type);
            Color color = GetColor(type);
            notificationUI.ShowNotification(message, color);
        }
        
        private string GetCollectedMessage(PowerupType type, int stackCount)
        {
            string baseName = type switch
            {
                PowerupType.Invincibility => "YOU ARE INVINCIBLE",
                PowerupType.Speed => "OBJECTS MOVE FASTER",
                PowerupType.Magnet => "CLAIM NEARBY ITEMS",
                PowerupType.Shield => "YOU HAVE A SHIELD",
                PowerupType.Multiply2x => "SCORE MULTIPLIED BY 2",
                PowerupType.Heart => "HEALTH RESTORED",
                _ => "POWERUP"
            };
            
            return stackCount > 1 ? $"{baseName} x{stackCount}" : baseName;
        }
        
        private string GetExpiredMessage(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => "YOU ARE NO LONGER INVINCIBLE",
                PowerupType.Speed => "OBJECTS NO LONGER MOVE FASTER",
                PowerupType.Magnet => "MAGNET EFFECT ENDED",
                PowerupType.Shield => "YOU NO LONGER HAVE A SHIELD",
                PowerupType.Multiply2x => "SCORE MULTIPLIED BY 2 ENDED",
                _ => "POWERUP ENDED"
            };
        }
        
        private Color GetColor(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => invincibilityColor,
                PowerupType.Speed => speedColor,
                PowerupType.Magnet => magnetColor,
                PowerupType.Shield => shieldColor,
                PowerupType.Multiply2x => multiply2xColor,
                PowerupType.Heart => healthColor,
                _ => Color.white
            };
        }
    }
}
