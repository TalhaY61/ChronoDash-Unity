using UnityEngine;
using ChronoDash.Managers;
using ChronoDash.Powerups;
using ChronoDash.Gemstones;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Handles viewer interaction notifications for Arena Arcade.
    /// Shows green notifications when viewers send items, powerups, or effects.
    /// Uses its own dedicated NotificationUI panel (separate from game notifications).
    /// </summary>
    public class ArenaViewerNotifications : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign the NotificationUI component from the ViewerNotificationPanel")]
        [SerializeField] private NotificationUI viewerNotificationUI;

        [Header("Viewer Notification Color")]
        [SerializeField] private Color viewerColor = new Color(0.2f, 0.9f, 0.3f); // Bright green

        private ArenaManager arenaManager;

        private void Start()
        {
            // Validate that we have a dedicated NotificationUI
            if (viewerNotificationUI == null)
            {
                UnityEngine.Debug.LogError("⚠️ ArenaViewerNotifications: ViewerNotificationUI is not assigned! Please assign the NotificationUI from ViewerNotificationPanel.");
                return;
            }

            // Find ArenaManager
            arenaManager = FindFirstObjectByType<ArenaManager>();

            if (arenaManager != null)
            {
                // Subscribe to Arena events
                arenaManager.OnPowerupSpawned += OnViewerSentPowerup;
                arenaManager.OnGemstonesSpawned += OnViewerSentGemstones;
                arenaManager.OnEffectTriggered += OnViewerSentEffect;
            }
        }

        private void OnDestroy()
        {
            if (arenaManager != null)
            {
                arenaManager.OnPowerupSpawned -= OnViewerSentPowerup;
                arenaManager.OnGemstonesSpawned -= OnViewerSentGemstones;
                arenaManager.OnEffectTriggered -= OnViewerSentEffect;
            }
        }

        private void OnViewerSentPowerup(string viewerName, PowerupType powerupType)
        {
            if (viewerNotificationUI == null) return;
            
            string powerupName = GetPowerupDisplayName(powerupType);
            string message = $"{viewerName} sent {powerupName}!";
            viewerNotificationUI.ShowNotification(message, viewerColor);
        }

        private void OnViewerSentGemstones(string viewerName, GemstoneType gemType, int quantity)
        {
            if (viewerNotificationUI == null) return;
            
            string gemName = GetGemstoneDisplayName(gemType);
            string message = quantity > 1 
                ? $"{viewerName} sent {quantity}x {gemName}!" 
                : $"{viewerName} sent a {gemName}!";
            viewerNotificationUI.ShowNotification(message, viewerColor);
        }

        private void OnViewerSentEffect(string viewerName, string effectId)
        {
            if (viewerNotificationUI == null) return;
            
            string effectName = GetEffectDisplayName(effectId);
            string message = $"{viewerName} activated {effectName}!";
            viewerNotificationUI.ShowNotification(message, viewerColor);
        }

        private string GetPowerupDisplayName(PowerupType type)
        {
            return type switch
            {
                PowerupType.Invincibility => "Invincibility",
                PowerupType.Speed => "Speed Trap",
                PowerupType.Magnet => "Magnet",
                PowerupType.Shield => "Shield",
                PowerupType.Multiply2x => "2x Score Boost",
                PowerupType.Heart => "Health Boost",
                _ => "Powerup"
            };
        }

        private string GetGemstoneDisplayName(GemstoneType type)
        {
            return type switch
            {
                GemstoneType.Blue => "Blue Gem",
                GemstoneType.Green => "Green Gem",
                GemstoneType.Red => "Red Gem",
                _ => "Gem"
            };
        }

        private string GetEffectDisplayName(string effectId)
        {
            return effectId switch
            {
                "gravity_flip" => "Gravity Flip",
                "screen_shake" => "Screen Shake",
                "darkness" => "Darkness",
                _ => "Effect"
            };
        }
    }
}
