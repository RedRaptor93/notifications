using System;
using Android.App;
using Plugin.Notifications.Infrastructure;


namespace Plugin.Notifications
{
    public static class AndroidConfig
    {
        public static int AppIconResourceId { get; set; } = GetResourceIdByName("icon");
        internal static INotificationRepository Repository { get; } = new LiteDbNotificationRepository();
        public static NotificationTapAction TapAction { get; set; } = NotificationTapAction.OpenApp;


        public static int GetResourceIdByName(string iconName) => Application
            .Context
            .Resources
            .GetIdentifier(
                iconName,
                "drawable",
                Application.Context.PackageName
            );
    }

    public enum NotificationTapAction
    {
        OpenApp,
        RaiseEvent
    }
}