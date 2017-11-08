using System;
using LiteDB;

namespace Plugin.Notifications.Infrastructure
{
    public class DbNotification
    {
        [BsonId(true)]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Sound { get; set; }
        public bool Vibrate { get; set; }
        public string IconName { get; set; }

        public DateTime DateScheduled { get; set; }
    }
}