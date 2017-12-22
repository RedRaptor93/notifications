using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Notifications;
using Android.Content;

namespace Sample.Droid
{
    [Activity(Label = "Sample", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            //CrossNotifications.Current.Activated += CrossNotification_Activated;
            AndroidConfig.TapAction = NotificationTapAction.OpenApp;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        private void CrossNotification_Activated(object sender, Plugin.Notifications.Notification e)
        {
            System.Diagnostics.
                    Debug.WriteLine("*MainActivity* Notification activated! Id: {0}, SendTime: {1}, Metadata count: {2}",
                    e.Id, e.SendTime, e.Metadata.Count);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}

