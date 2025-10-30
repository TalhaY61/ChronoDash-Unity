namespace ChronoDash.UI.Notifications
{
    /// <summary>
    /// Interface for notification providers.
    /// Providers can subscribe to events and push notifications to the service.
    /// Follows Open/Closed Principle - new notification types can be added without modifying existing code.
    /// </summary>
    public interface INotificationProvider
    {
        void Initialize(NotificationService service);
        void Cleanup();
    }
}
