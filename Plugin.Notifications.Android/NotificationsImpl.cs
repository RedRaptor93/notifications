using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Java.IO;
using Console = System.Console;


namespace Plugin.Notifications
{
    public class NotificationsImpl : AbstractNotificationsImpl
    {
        readonly AlarmManager alarmManager;
        public int AppIconResourceId { get; set; }


        public NotificationsImpl()
        {
            this.AppIconResourceId = Application.Context.Resources.GetIdentifier("icon", "drawable", Application.Context.PackageName);
            this.alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
        }


        public override Task Send(Notification notification)
        {
            var id = NotificationSettings.Instance.CreateScheduleId();

            if (notification.IsScheduled)
            {
                var triggerMs = this.GetEpochMills(notification.SendTime);
                var pending = notification.ToPendingIntent(id);

                this.alarmManager.Set(
                    AlarmType.RtcWakeup,
                    Convert.ToInt64(triggerMs),
                    pending
                );

                return id.ToString();
            }

            //Uri uri= RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);
            //Uri.parse(ContentResolver.SCHEME_ANDROID_RESOURCE+ "://" + getPackageName() + "/raw/kalimba");

            var launchIntent = Application.Context.PackageManager.GetLaunchIntentForPackage(Application.Context.PackageName);
            launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            var builder = new NotificationCompat
                .Builder(Application.Context)
                .SetAutoCancel(true)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Message)
                .SetSmallIcon(this.AppIconResourceId)
                .SetContentIntent(TaskStackBuilder
                    .Create(Application.Context)
                    .AddNextIntent(launchIntent)
                    .GetPendingIntent(id, (int)PendingIntentFlags.OneShot)
                );
            //var pending = PendingIntent.GetBroadcast(Application.Context, id, intent, PendingIntentFlags.UpdateCurrent);

            if (notification.Vibrate)
            {
                builder.SetVibrate(new long[] { 500, 500 });
            }

            if (notification.Sound != null)
            {
                var file = new File(notification.Sound);
                var uri = Android.Net.Uri.FromFile(file);
                builder.SetSound(uri);
            }
            var not = builder.Build();
            NotificationManagerCompat
                .From(Application.Context)
                .Notify(id, not);
            return id.ToString();
        }


        public override Task CancelAll()
        {
            foreach (var id in NotificationSettings.Instance.ScheduleIds)
                this.CancelInternal(id);

            NotificationSettings.Instance.ClearScheduled();
            NotificationManagerCompat
                .From(Application.Context)
                .CancelAll();

            return Task.CompletedTask;
        }


        public override Task<bool> Cancel(string id)
        {
            var @int = 0;
            if (!Int32.TryParse(id, out @int))
                return Task.FromResult(false);

            this.CancelInternal(@int);
            NotificationSettings.Instance.RemoveScheduledId(@int);
            return Task.FromResult(true);
        }


        public override Task<IEnumerable<Notification>> GetScheduledNotifications()
        {
            throw new NotImplementedException();
        }


        public override Task<int> GetBadge() => Task.FromResult(NotificationSettings.Instance.CurrentBadge);

        public override Task SetBadge(int value)
        {
            try
            {
                NotificationSettings.Instance.CurrentBadge = value;
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
            using (var vibrate = (Vibrator)Application.Context.GetSystemService(Context.VibratorService))
            {
                if (!vibrate.HasVibrator)
                    return;

                vibrate.Vibrate(ms);
            }
        }


        protected virtual long GetEpochMills(DateTime sendTime)
        {
            var utc = sendTime.ToUniversalTime();
            var epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            var utcAlarmTimeInMillis = utc.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTimeInMillis;
        }


        void CancelInternal(int notificationId)
        {
            var pending = Helpers.GetNotificationPendingIntent(notificationId);
            pending.Cancel();
            this.alarmManager.Cancel(pending);
            NotificationManagerCompat
                .From(Application.Context)
                .Cancel(notificationId);
        }
    }
}
