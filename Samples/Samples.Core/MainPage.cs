using Plugin.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Samples
{
    class MainPage : ContentPage
    {
        static readonly string SEP = new string('-', 30);

        ScrollView root;
        StackLayout layout;

        Button btnPermission, btnSetBadge, btnClearBadge,
            btnTestNotifDefIcon, btnTestNotifCustomIcon,
            btnMultipleMsgs, btnCancelAll, btnVibrate;

        Label lblTestNotif, lblBadges, lblOthers;

        private void InitComponents()
        {
            // init components
            root = new ScrollView();

            btnPermission = new Button
            {
                Text = "Request Permission",
                Command = new Command(async () =>
                {
                    var result = await CrossNotifications.Current.RequestPermission();
                    btnPermission.Text = result ? "Permission Granted" : "Permission Denied";
                })
            };
            lblBadges = new Label
            {
                Text = "Set and clear Badge numbers",
                Margin = new Thickness(10, 5)
            };
            btnSetBadge = new Button
            {
                Text = "Set Badge",
                Command = new Command(() => CrossNotifications.Current.SetBadge(new Random().Next(100)))
            };
            btnClearBadge = new Button
            {
                Text = "Clear Badge",
                Command = new Command(() => CrossNotifications.Current.SetBadge(0))
            };
            lblTestNotif = new Label
            {
                Text = "Send notifications, Press the buttons below and exit App withing 10 seconds:",
                Margin = new Thickness(10, 5)
            };
            btnTestNotifDefIcon = new Button
            {
                Text = "Notification w/ default icon",
                Command = new Command(() =>
                    CrossNotifications.Current.Send(new Notification
                    {
                        Title = "HELLO!",
                        Message = "Hello from the ACR Sample Notification App, you should see the App's icon displayed",
                        Vibrate = true,
                        When = TimeSpan.FromSeconds(10),
                    }))
            };
            btnTestNotifCustomIcon = new Button
            {
                Text = "Notification w/ custom icon",
                Command = new Command(() =>
                    CrossNotifications.Current.Send(new Notification
                    {
                        Title = "HELLO!",
                        Message = "Hello from the ACR Sample Notification App, you should see a custom icon displayed",
                        Vibrate = true,
                        When = TimeSpan.FromSeconds(10),
                        IconName = "ic_addcard"
                    }))
            };
            lblOthers = new Label
            {
                Text = SEP + "  Others  " + SEP,
                HorizontalOptions = LayoutOptions.Center
            };
            btnMultipleMsgs = new Button
            {
                Text = "Multiple Timed Messages (10 messages x 5 seconds apart)",
                Command = new Command(() =>
                {
                    CrossNotifications.Current.Send(new Notification
                    {
                        Title = "Samples",
                        Message = "Starting Sample Schedule Notifications"
                    });
                    for (var i = 1; i < 11; i++)
                    {
                        var seconds = i * 5;
                        var id = CrossNotifications.Current.Send(new Notification
                        {
                            Message = $"Message {i}",
                            When = TimeSpan.FromSeconds(seconds)
                        });
                        Debug.WriteLine($"Notification ID: {id}");
                    }
                })
            };
            btnCancelAll = new Button
            {
                Text = "Cancel All Notifications",
                Command = new Command(() => CrossNotifications.Current.CancelAll())
            };
            btnVibrate = new Button
            {
                Text = "Vibrate",
                Command = new Command(() => CrossNotifications.Current.Vibrate())
            };

            // set layout
            layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,

                Children =
                {
                    btnPermission,
                    lblBadges,
                    btnSetBadge,
                    btnClearBadge,
                    lblTestNotif,
                    btnTestNotifDefIcon,
                    btnTestNotifCustomIcon,
                    lblOthers,
                    btnMultipleMsgs,
                    btnCancelAll,
                    btnVibrate
                }
            };
            root.Content = layout;

            // set content to root
            this.Content = root;
        } 


        public MainPage()
        {
            InitComponents();
            Title = "Notifications";
            Notification.DefaultTitle = "Test Title";
        }

    }
}
