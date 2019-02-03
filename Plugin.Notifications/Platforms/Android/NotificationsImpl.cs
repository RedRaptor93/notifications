using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;


namespace Plugin.Notifications
{
    public class NotificationsImpl : AbstractNotificationsImpl, IAndroidNotificationReceiver
    {
        public NotificationsImpl()
        {
            NotificationBroadcastReceiver.Register();
        }


        public override Task Send(Notification notification)
        {
            if (notification.Id == null)
            {
                CrossNotifications.Repository.CurrentScheduleId++;
                notification.Id = CrossNotifications.Repository.CurrentScheduleId;
            }

            if (notification.IsScheduled)
            {
                var triggerMs = this.GetEpochMills(notification.ScheduledDate.Value);
                var pending = notification.ToPendingIntent(notification.Id.Value);

                var alarmMgr = (AlarmManager) Application.Context.GetSystemService(Context.AlarmService);
                alarmMgr.Set(
                    AlarmType.RtcWakeup,
                    Convert.ToInt64(triggerMs),
                    pending
                );
                CrossNotifications.Repository.Insert(notification);
            }
            else
            {
                var launchIntent = Application
                    .Context
                    .PackageManager
                    .GetLaunchIntentForPackage(Application.Context.PackageName)
                    .SetAction(Constants.ACTION_KEY)
                    .SetFlags(Config.DefaultLaunchActivityFlags);

                foreach (var pair in notification.Metadata)
                    launchIntent.PutExtra(pair.Key, pair.Value);

                var pendingIntent = TaskStackBuilder
                    .Create(Application.Context)
                    .AddNextIntent(launchIntent)
                    .GetPendingIntent(notification.Id.Value, (int)(PendingIntentFlags.OneShot | PendingIntentFlags.CancelCurrent));

                var builder = new NotificationCompat.Builder(Application.Context)
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle(notification.Title)
                    .SetContentText(notification.Message)
                    ;

                if (notification.Icon != null)
                {
                    int iconId;

                    if (notification.Icon == Notification.PlatformDefault)
                    {
                        iconId = Helpers.GetResourceIdByName(Config.DefaultIcon);
                    }
                    else
                    {
                        iconId = Helpers.GetResourceIdByName(notification.Icon);
                    }

                    builder.SetSmallIcon(iconId);
                }

                if (notification.Vibrate)
                    builder.SetVibrate(new long[] {500, 500});

                if (notification.Sound != null)
                {
                    Android.Net.Uri uri;

                    if (notification.Sound == Notification.PlatformDefault)
                    {
                        // Fallback to the system default notification sound
                        uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
                    }
                    else if (!notification.Sound.Contains("://"))
                    {
                        notification.Sound = $"{ContentResolver.SchemeAndroidResource}://{Application.Context.PackageName}/raw/{notification.Sound}";
                        uri = Android.Net.Uri.Parse(notification.Sound);
                    }
                    else
                    {
                        uri = Android.Net.Uri.Parse(notification.Sound);
                    }

                    builder.SetSound(uri);
                }
                var not = builder.Build();
                NotificationManagerCompat
                    .From(Application.Context)
                    .Notify(notification.Id.Value, not);
            }
            return Task.CompletedTask;
        }


        public override Task CancelAll()
        {
            var notifications = CrossNotifications.Repository.GetScheduled();
            foreach (var notification in notifications)
                this.CancelInternal(notification.Id.Value);

            CrossNotifications.Repository.DeleteAll();

            NotificationManagerCompat
                .From(Application.Context)
                .CancelAll();

            return Task.CompletedTask;
        }


        public override Task Cancel(int notificationId)
        {
            CrossNotifications.Repository.Delete(notificationId);
            this.CancelInternal(notificationId);
            return Task.FromResult(true);
        }


        public override Task<IEnumerable<Notification>> GetScheduledNotifications()
            => Task.FromResult(CrossNotifications.Repository.GetScheduled());


        public override Task<int> GetBadge() => Task.FromResult(CrossNotifications.Repository.CurrentBadge);

        public override Task SetBadge(int value)
        {
            try
            {
                CrossNotifications.Repository.CurrentBadge = value;
                if (value <= 0)
                {
                    ME.Leolin.Shortcutbadger.ShortcutBadger.RemoveCount(Application.Context);
                }
                else
                {
                    ME.Leolin.Shortcutbadger.ShortcutBadger.ApplyCount(Application.Context, value);
                }
            }
            catch
            {
            }
            return Task.CompletedTask;
        }


        public override void Vibrate(int ms)
        {
            using (var vib = Vibrator.FromContext(Application.Context))
            {
                if (!vib.HasVibrator)
                    return;

                vib.Vibrate(VibrationEffect.CreateOneShot(ms, VibrationEffect.DefaultAmplitude));
            }
        }


        public void TriggerNotification(int id)
        {
            var notification = CrossNotifications.Repository.GetById(id);
            if (notification != null)
                this.OnActivated(notification);
        }


        public void TriggerScheduledNotification(int notificationId)
        {
            var notification = CrossNotifications.Repository.GetById(notificationId);
            if (notification == null)
                return;

            CrossNotifications.Repository.Delete(notificationId);

            // resend without schedule so it goes through normal mechanism
            notification.ScheduledDate = null;
            this.Send(notification);
        }


        protected virtual long GetEpochMills(DateTime sendTime)
        {
            var utc = sendTime.ToUniversalTime();
            var epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            var utcAlarmTimeInMillis = utc.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTimeInMillis;
        }


        protected virtual void CancelInternal(int notificationId)
        {
            var pending = Helpers.GetNotificationPendingIntent(notificationId);
            pending.Cancel();

            var alarmMgr = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            alarmMgr.Cancel(pending);

            NotificationManagerCompat
                .From(Application.Context)
                .Cancel(notificationId);
        }
    }
}
