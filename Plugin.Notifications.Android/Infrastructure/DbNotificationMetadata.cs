using System;
using LiteDB;


namespace Plugin.Notifications.Infrastructure
{
    public class DbNotificationMetadata
    {
        [BsonId(true)]
        public int Id { get; set; }

        public int NotificationId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}