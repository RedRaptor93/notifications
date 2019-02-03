using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Plugin.Notifications
{

    public abstract class AbstractNotificationsImpl : INotificationManager, IDisposable
    {
        ~AbstractNotificationsImpl() => this.Dispose(false);


        public event EventHandler<Notification> Activated;
        public abstract Task Cancel(int notificationId);
        public abstract Task Send(Notification notification);
        public abstract Task<IEnumerable<Notification>> GetScheduledNotifications();
        public abstract Task<int> GetBadge();
        public abstract Task SetBadge(int value);

        public virtual void Vibrate(int ms) { /* noop */ }

        public virtual Task<bool> RequestPermission() => Task.FromResult(true);


        public virtual async Task CancelAll()
        {
            var notifications = await this.GetScheduledNotifications();
            foreach (var notification in notifications)
            {
                await this.Cancel(notification.Id.Value);
            }
        }


        protected virtual void OnActivated(Notification notification)
            => this.Activated?.Invoke(this, notification);


        public void Dispose() => this.Dispose(true);
        protected virtual void Dispose(bool disposing) { /* noop */ }
    }
}
