using System;
using System.Collections.Generic;


namespace Plugin.Notifications
{

    public class Notification
    {
        // null = sound, icon
        // PlatformDefault = use platform option
        public const string PlatformDefault = "..PLATFORM_DEFAULT..";

        public static string DefaultSound { get; set; }
        public static string DefaultIcon { get; set; }


        public int? Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Sound { get; set; } = DefaultSound;
        public string Icon { get; set; } = DefaultIcon;
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public bool Vibrate { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public bool IsScheduled => this.ScheduledDate != null;

    }
}