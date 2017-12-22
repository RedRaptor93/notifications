using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;


namespace Plugin.Notifications.Infrastructure
{
    public class LiteDbNotificationRepository : INotificationRepository
    {
        readonly AcrLiteDbConnection conn;
        readonly DbSettings settings;

        public LiteDbNotificationRepository()
        {
            conn = new AcrLiteDbConnection();
            settings = conn.Settings.FindAll().SingleOrDefault();
            if (settings == null)
            {
                settings = new DbSettings();
                conn.Settings.Insert(settings);
            }

            CleanUpOld();
        }


        public int CurrentScheduleId
        {
            get => this.settings.CurrentScheduleId;
            set
            {
                if (this.settings.CurrentScheduleId.Equals(value))
                    return;

                this.settings.CurrentScheduleId = value;
                this.conn.Settings.Update(settings);
            }
        }
        public int CurrentBadge
        {
            get => this.settings.CurrentBadge;
            set
            {
                if (this.settings.CurrentBadge.Equals(value))
                    return;

                this.settings.CurrentBadge = value;
                this.conn.Settings.Update(this.settings);
            }
        }

        // find old notifs. (Scheduled 7+ days ago) and remove them
        private void CleanUpOld()
        {
            var notCount = conn.Notifications.Count();
            Log.Debug(nameof(LiteDbNotificationRepository), "{0} notifications in repo.", notCount);

            if (notCount > 0)
            {
                var oldDate = DateTime.Today.AddDays(-7);
                var query = Query.LT("DateScheduled", oldDate);

                var oldNots = conn.Notifications.Find(query);

                int nDeleted = conn.Notifications.Delete(query);
                Log.Debug(nameof(LiteDbNotificationRepository), "{0} old notifications removed", nDeleted);

                // remove metadata
                if (nDeleted > 0)
                {
                    var mQuery = Query.In("NotificationId", oldNots.Select(on => new BsonValue(on.Id)));
                    int nmDeleted = conn.NotificationMetadata.Delete(mQuery);
                    Log.Debug(nameof(LiteDbNotificationRepository), "{0} old notif. metadata removed", nmDeleted);
                }
            }
        }

        public void Delete(int id)
        {
            conn.Notifications.Delete(id);
            conn.NotificationMetadata.Delete(id);
        }

        public void DeleteAll()
        {
            conn.DropCollection(nameof(DbNotification));
            conn.DropCollection(nameof(DbNotificationMetadata));
        }

        public Notification GetById(int id)
        {
            var dbNot = conn.Notifications.FindById(id);
            if (dbNot == null) return null;

            var dbMeta = conn.NotificationMetadata
                             .Find(Query.EQ("NotificationId", id))
                             .ToList();

            var notif = new Notification
            {
                Id = dbNot.Id,
                Title = dbNot.Title,
                Message = dbNot.Message,
                Sound = dbNot.Sound,
                Vibrate = dbNot.Vibrate,
                Date = dbNot.DateScheduled,
                IconName = dbNot.IconName,
                Metadata = dbMeta
                    .ToDictionary(
                        y => y.Key,
                        y => y.Value
                    )

            };

            return notif;
        }

        public IEnumerable<Notification> GetScheduled()
        {
            var nots = conn.Notifications.FindAll().ToList();
            var meta = conn.NotificationMetadata.FindAll().ToList();

            var scheduled = nots.Select(x => new Notification
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                Sound = x.Sound,
                Vibrate = x.Vibrate,
                Date = x.DateScheduled,
                IconName = x.IconName,
                Metadata = meta
                    .Where(y => y.NotificationId == x.Id)
                    .ToDictionary(
                        y => y.Key,
                        y => y.Value
                    )

            });

            return scheduled;
        }

        public void Insert(Notification notification)
        {
            try
            {
                var db = new DbNotification
                {
                    Id = notification.Id.Value,
                    Title = notification.Title,
                    Message = notification.Message,
                    Sound = notification.Sound,
                    IconName = notification.IconName,
                    Vibrate = notification.Vibrate,
                    DateScheduled = notification.SendTime
                };

                conn.Notifications.Insert(db);

                var dbm = notification.Metadata
                .Select(n => new DbNotificationMetadata
                {
                    NotificationId = db.Id,
                    Key = n.Key,
                    Value = n.Value
                });

                conn.NotificationMetadata.Insert(dbm);

            }
            catch 
            {
                throw;
            }
        }


    }

}