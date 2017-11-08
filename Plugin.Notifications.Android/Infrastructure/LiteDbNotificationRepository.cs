using System.Collections.Generic;
using System.Linq;
using LiteDB;


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

        public void Delete(int id)
        {
            var bsonId = new BsonValue(id);

            conn.Notifications.Delete(bsonId);
            conn.NotificationMetadata.Delete(bsonId);
        }

        public void DeleteAll()
        {
            conn.DropCollection(nameof(DbNotification));
            conn.DropCollection(nameof(DbNotificationMetadata));
        }

        public Notification GetById(int id)
        {
            var bsonId = new BsonValue(id);
            var dbNot = conn.Notifications.FindById(bsonId);
            if (dbNot == null) return null;

            var dbMeta = conn.NotificationMetadata
                             .Find(Query.EQ("NotificationId", bsonId))
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

                foreach (var pair in notification.Metadata)
                {
                    var meta = new DbNotificationMetadata
                    {
                        NotificationId = db.Id,
                        Key = pair.Key,
                        Value = pair.Value
                    };

                    conn.NotificationMetadata.Insert(meta);
                }

            }
            catch 
            {
                throw;
            }
        }


    }

}