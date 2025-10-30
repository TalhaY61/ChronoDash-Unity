using UnityEngine;

namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Interface for notification data. Allows different notification types.
    /// </summary>
    public interface INotificationData
    {
        string Message { get; }
        Color Color { get; }
    }
}
