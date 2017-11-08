using System;
using System.Text;
using System.IO;
using LiteDB;


namespace Plugin.Notifications.Infrastructure
{
    public class AcrLiteDbConnection : LiteDatabase
    {

        public LiteCollection<DbNotification> Notifications { get; }
        public LiteCollection<DbNotificationMetadata> NotificationMetadata { get; }
        public LiteCollection<DbSettings> Settings { get; }

        public AcrLiteDbConnection()
            : base(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "notifications.ldb"))
        {
            Notifications = GetCollection<DbNotification>();
            //Notifications.EnsureIndex("Id", true);

            NotificationMetadata = GetCollection<DbNotificationMetadata>();
            //NotificationMetadata.EnsureIndex("Id", true);

            Settings = GetCollection<DbSettings>();
            //Settings.EnsureIndex("Id", true);
        }

    }

}