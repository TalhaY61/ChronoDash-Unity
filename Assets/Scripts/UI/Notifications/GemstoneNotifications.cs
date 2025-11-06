using UnityEngine;
using ChronoDash.Gemstones;

namespace ChronoDash.UI.Notifications
{
    public class GemstoneNotifications : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NotificationUI notificationUI;

        [Header("Gemstone Colors")]
        [SerializeField] private Color blueGemColor;
        [SerializeField] private Color greenGemColor;
        [SerializeField] private Color redGemColor;
        
        private GemstoneManager gemstoneManager;
        
        private void Start()
        {
            if (notificationUI == null)
                notificationUI = GetComponent<NotificationUI>();
            
            gemstoneManager = FindFirstObjectByType<GemstoneManager>();
            
            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollectedWithGem += OnGemstoneCollected;
            }
        }
        
        private void OnDestroy()
        {
            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollectedWithGem -= OnGemstoneCollected;
            }
        }
        
        private void OnGemstoneCollected(Gemstone gemstone)
        {
            string message = gemstone.Type switch
            {
                GemstoneType.Blue => $"+{gemstone.ScoreValue} POINTS",
                GemstoneType.Green => $"+{gemstone.ScoreValue} POINTS",
                GemstoneType.Red => $"+{gemstone.ScoreValue} POINTS +  1 HEAL",
                _ => $"+{gemstone.ScoreValue} POINTS"
            };
            
            Color color = gemstone.Type switch
            {
                GemstoneType.Blue => blueGemColor,
                GemstoneType.Green => greenGemColor,
                GemstoneType.Red => redGemColor,
                _ => Color.white
            };
            
            notificationUI.ShowNotification(message, color);
        }
    }
}
