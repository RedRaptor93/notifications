using System;
using Plugin.Notifications.Infrastructure;


namespace Plugin.Notifications
{
    public static partial class CrossNotifications
    {
        static CrossNotifications()
        {
            Current = new NotificationsImpl();
        }

        static internal INotificationRepository Repository { get; set; } = new SqliteNotificationRepository();
    }
}
