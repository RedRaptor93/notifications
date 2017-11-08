using System;
using LiteDB;


namespace Plugin.Notifications.Infrastructure
{
    public class DbSettings
    {
        [BsonId(true)]
        public int Id { get; set; }

        public int CurrentBadge { get; set; }
        public int CurrentScheduleId { get; set; }
    }
}