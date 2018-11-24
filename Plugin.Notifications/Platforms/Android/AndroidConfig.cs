﻿using System;
using Android.App;
using Android.Content;
using Plugin.Notifications.Infrastructure;


namespace Plugin.Notifications
{
    public static class AndroidConfig
    {
        public static INotificationRepository Repository { get; set; } = new SqliteNotificationRepository();
        public static ActivityFlags LaunchActivityFlags { get; set; } = ActivityFlags.NewTask | ActivityFlags.ClearTask;


        public static int GetResourceIdByName(string iconName) => Application
            .Context
            .Resources
            .GetIdentifier(
                iconName,
                "drawable",
                Application.Context.PackageName
            );
    }
}