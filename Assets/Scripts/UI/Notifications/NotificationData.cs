using UnityEngine;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Simple implementation of INotificationData.
    /// </summary>
    public class NotificationData : INotificationData
    {
        public string Message { get; set; }
        public Color Color { get; set; }

        public NotificationData(string message, Color color)
        {
            Message = message;
            Color = color;
        }
    }
}
