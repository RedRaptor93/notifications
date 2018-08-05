﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;


namespace Plugin.Notifications
{
    public class UILocalNotificationsImpl : AbstractAppleNotificationsImpl
    {
        const string NOTIFICATION_ID_KEY = "NotificationID";
        readonly IDisposable dispose;


        public UILocalNotificationsImpl()
        {
            this.dispose = UIApplication
                .Notifications
                .ObserveDidFinishLaunching((sender, launchArgs) =>
                {
                    var native = launchArgs.Notification.Object as UILocalNotification;
                    if (native != null)
                    {
                        var notification = this.FromNative(native);
                        this.OnActivated(notification);
                    }
                });
        }


        public override Task Cancel(int notificationId)
        {
            var key = new NSString(NOTIFICATION_ID_KEY);
            var keyValue = new NSString(notificationId.ToString());

            var notification = UIApplication.SharedApplication.ScheduledLocalNotifications.FirstOrDefault(x =>
                x.UserInfo.ContainsKey(key) &&
                x.UserInfo[key].Equals(keyValue)
            );
            if (notification == null)
                return Task.CompletedTask;

            return this.Invoke(() =>
                UIApplication.SharedApplication.CancelLocalNotification(notification)
            );
        }


        public override Task CancelAll() => this.Invoke(() =>
            UIApplication.SharedApplication.CancelAllLocalNotifications()
        );


        public override Task Send(Notification notification)
        {
            if (notification.Id == null)
                notification.GeneratedNotificationId();

            if (notification.Sound == Notification.PlatformDefault)
                notification.Sound = UILocalNotification.DefaultSoundName;

            var not = new UILocalNotification
            {
                AlertTitle = notification.Title,
                AlertBody = notification.Message,
                AlertLaunchImage = notification.Icon,
                UserInfo = notification.MetadataToNsDictionary(),
                SoundName = notification.Sound
            };

            if (notification.ScheduledDate != null)
                not.FireDate = notification.ScheduledDate.Value.ToNSDate();

            return this.Invoke(() => UIApplication.SharedApplication.ScheduleLocalNotification(not));
        }


        public override Task<IEnumerable<Notification>> GetScheduledNotifications()
        {
            var tcs = new TaskCompletionSource<IEnumerable<Notification>>();
            UIApplication.SharedApplication.InvokeOnMainThread(() => tcs.TrySetResult(
                UIApplication
                    .SharedApplication
                    .ScheduledLocalNotifications
                    .Select(this.FromNative)
            ));
            return tcs.Task;
        }


        public override Task<bool> RequestPermission()
        {
            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                null
            );
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            return Task.FromResult(true);
        }


        protected virtual Notification FromNative(UILocalNotification native)
        {
            var plugin = new Notification
            {
                Title = native.AlertTitle,
                Message = native.AlertBody,
                Sound = native.SoundName,
                ScheduledDate = native.FireDate.ToDateTime(),
                Metadata = native.UserInfo.FromNsDictionary()
            };

            if (plugin.Metadata.ContainsKey(NOTIFICATION_ID_KEY))
            {
                if (Int32.TryParse(plugin.Metadata[NOTIFICATION_ID_KEY], out var id))
                    plugin.Id = id;
            }

            return plugin;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.dispose.Dispose();
        }
    }
}
