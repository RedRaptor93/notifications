using System;
using Android.App;
using Android.Content;


namespace Plugin.Notifications
{

    public static class Helpers
    {
        public static PendingIntent ToPendingIntent(this Notification notification, int id)
        {
            var intent = new Intent(Application.Context, typeof(AlarmBroadcastReceiver)).PutExtra(Constants.NOTIFICATION_ID, id);
            var pending = PendingIntent.GetBroadcast(Application.Context, id, intent, PendingIntentFlags.OneShot);
            return pending;
        }


        public static PendingIntent GetNotificationPendingIntent(int id)
        {
            var intent = new Intent(Application.Context, typeof(AlarmBroadcastReceiver)).PutExtra(Constants.NOTIFICATION_ID, id);
            var pending = PendingIntent.GetBroadcast(Application.Context, id, intent, PendingIntentFlags.OneShot);
            return pending;
        }

        public static int GetResourceIdByName(string iconName) => Application
            .Context
            .Resources
            .GetIdentifier(
                iconName,
                "drawable",
                Application.Context.PackageName
            );


        public static int NotificationId(this Intent intent) => intent.GetIntExtra(Constants.NOTIFICATION_ID, 0);
    }
}