using UnityEngine;

namespace ChronoDash.UI.Notifications
{
    public class EffectNotifications : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NotificationUI notificationUI;

        [Header("Effect Warning Colors")]
        [SerializeField] private Color gravityFlipColor;
        [SerializeField] private Color screenShakeColor;
        [SerializeField] private Color darknessColor;

        // Singleton for GameplayEffectsManager to access
        private static EffectNotifications instance;
        public static EffectNotifications Instance => instance;
        
        private void Awake()
        {
            instance = this;
            
            if (notificationUI == null)
                notificationUI = GetComponent<NotificationUI>();
        }
        
        public void ShowGravityFlipWarning()
        {
            notificationUI.ShowNotification("GRAVITY SHIFT INCOMING!", gravityFlipColor);
        }
        
        public void ShowScreenShakeWarning()
        {
            notificationUI.ShowNotification("EARTHQUAKE INCOMING!", screenShakeColor);
        }
        
        public void ShowDarknessWarning()
        {
            notificationUI.ShowNotification("LIGHTS OUT, CAUTION!", darknessColor);
        }
    }
}
