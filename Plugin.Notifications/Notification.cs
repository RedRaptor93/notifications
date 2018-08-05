using System;
using System.Collections.Generic;


namespace Plugin.Notifications
{

    public class Notification
    {
        public const string PlatformDefault = "..PLATFORM_DEFAULT..";

        public static string DefaultTitle { get; set; }
        public static string DefaultSound { get; set; }
        public static string DefaultIcon { get; set; }


        public int? Id { get; set; }
        public string Title { get; set; } = DefaultTitle;
        public string Message { get; set; }
        public string Sound { get; set; } = DefaultSound;
        public string Icon { get; set; } = DefaultIcon;
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Only works with Android
        /// </summary>
        public bool Vibrate { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public bool IsScheduled => this.ScheduledDate != null;
        /*
         * TODO: Repeat Intervals
if (notification.Interval != NotificationInterval.None) {
				not.RepeatInterval = notification.Interval == NotificationInterval.Weekly ? NSCalendarUnit.Week : NSCalendarUnit.Day;
			}
         */
    }
}