using System;
using Android.App;
using Android.Content;
using Android.OS;
using Debug = System.Diagnostics.Debug;

namespace Plugin.Notifications
{
    [Service]
    public class NotificationActionService : IntentService
    {
        public override bool OnUnbind(Intent intent)
        {
            Debug.WriteLine("OnUnbind", nameof(NotificationActionService));
            return base.OnUnbind(intent);
        }

        public override void OnCreate()
        {
            Debug.WriteLine("OnCreate", nameof(NotificationActionService));
            base.OnCreate();
        }

        protected override void OnHandleIntent(Intent intent)
        {
            Debug.WriteLine($"Intent extras: {string.Join(", ", intent.Extras.KeySet())}", nameof(NotificationActionService));

            if (intent.HasExtra(Constants.ACTION_KEY))
            {
                var notificationId = intent.GetIntExtra(Constants.NOTIFICATION_ID, 0);
                (CrossNotifications.Current as IAndroidNotificationReceiver)?.TriggerNotification(notificationId);
            }
        }
    }

    //[BroadcastReceiver(Enabled = true, Label = "Notifications Action Receiver")]
    //public class NotificationActionReceiver : BroadcastReceiver
    //{
    //    public override void OnReceive(Context context, Intent intent)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}