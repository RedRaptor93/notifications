using Android.Content;
using System;


namespace Plugin.Notifications
{
    public class Config
    {
        public static string DefaultIcon { get; set; } = "icon";
        public static ActivityFlags DefaultLaunchActivityFlags { get; set; } = ActivityFlags.NewTask | ActivityFlags.ClearTask;
    }
}
