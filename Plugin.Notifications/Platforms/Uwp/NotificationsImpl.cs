using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Notifications;
using System.Net;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Devices.Haptics;


namespace Plugin.Notifications
{
    //https://blogs.msdn.microsoft.com/tiles_and_toasts/2015/07/08/quickstart-sending-a-local-toast-notification-and-handling-activations-from-it-windows-10/
    public class NotificationsImpl : AbstractNotificationsImpl
    {
        readonly BadgeUpdater badgeUpdater;
        readonly ToastNotifier toastNotifier;


        public NotificationsImpl()
        {
            this.badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            this.toastNotifier = ToastNotificationManager.CreateToastNotifier();
        }


        public override Task Cancel(int notificationId)
        {
            var id = notificationId.ToString();

            var notification = this.toastNotifier
                .GetScheduledToastNotifications()
                .FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (notification == null)
                return Task.FromResult(false);

            this.toastNotifier.RemoveFromSchedule(notification);
            return Task.FromResult(true);
        }


        public override async Task CancelAll()
        {
            await this.SetBadge(0);

            var list = this.toastNotifier
                .GetScheduledToastNotifications()
                .ToList();

            foreach (var item in list)
                this.toastNotifier.RemoveFromSchedule(item);
        }


        public override Task Send(Notification notification)
        {
            if (notification.Id == null)
                notification.Id = this.GetNotificationId();

            var toastContent = new ToastContent
            {
                Launch = ToQueryString(notification.Metadata),
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = notification.Title,
                                HintStyle = AdaptiveTextStyle.Title
                            },
                            new AdaptiveText
                            {
                                Text = notification.Message,
                                HintStyle = AdaptiveTextStyle.Body
                            },
                        }
                    }
                }
            };

            if (IsAudioSupported)
            {
                if (notification.Sound == Notification.PlatformDefault)
                {
                    toastContent.Audio = new ToastAudio
                    {
                        // https://docs.microsoft.com/en-us/uwp/schemas/tiles/toastschema/element-audio#attributes-and-elements
                        Src = new Uri("ms-winsoundevent:Notification.Looping.Alarm")
                    };
                }
                else if (!String.IsNullOrWhiteSpace(notification.Sound))
                {
                    if (!notification.Sound.StartsWith("ms-appx:"))
                        notification.Sound = $"ms-appx:///Assets/Audio/{notification.Sound}.m4a";

                    toastContent.Audio = new ToastAudio
                    {
                        Src = new Uri(notification.Sound)
                    };
                }
            }

            if (notification.ScheduledDate == null)
            {
                var toast = new ToastNotification(toastContent.GetXml());
                toast.Activated += (sender, args) => this.OnActivated(notification);
                this.toastNotifier.Show(toast);
            }
            else
            {
                //https://msdn.microsoft.com/library/74ba3513-0a52-46a0-8769-ed58abe7c05a
                var schedule = new ScheduledToastNotification(toastContent.GetXml(), notification.ScheduledDate.Value)
                {
                    Id = notification.Id.Value.ToString()
                };
                this.toastNotifier.AddToSchedule(schedule);
            }
            return Task.CompletedTask;
        }


        public override Task<int> GetBadge() => Task.FromResult(this.CurrentBadge);


        public override Task SetBadge(int value)
        {
            this.CurrentBadge = value;
            if (value == 0)
            {
                this.badgeUpdater.Clear();
            }
            else
            {
                this.badgeUpdater.Update(new BadgeNotification(new BadgeNumericContent((uint)value).GetXml()));
            }
            return Task.CompletedTask;
        }


        public override Task<IEnumerable<Notification>> GetScheduledNotifications()
        {
            var list = this.toastNotifier
                .GetScheduledToastNotifications()
                .Select(x => {
                    var text_nodes = x.Content.SelectNodes("/toast/visual/binding/text");
                    var audio_node = x.Content.SelectSingleNode("/toast/audio");

                    return new Notification
                    {
                        Id = int.Parse(x.Id),
                        ScheduledDate = x.DeliveryTime.LocalDateTime,
                        Title = text_nodes[0].InnerText,
                        Message = text_nodes[1].InnerText,
                        Metadata = FromQueryString(x.Content.Attributes.GetNamedItem("launch").InnerText),
                        Sound = audio_node?.Attributes.GetNamedItem("src").InnerText
                    };
                });

            return Task.FromResult(list);
        }


        public override async void Vibrate(int ms)
        {
            if (await VibrationDevice.RequestAccessAsync() != VibrationAccessStatus.Allowed)
                return;

            var device = await VibrationDevice.GetDefaultAsync();
            if (device != null && device.SimpleHapticsController.IsPlayDurationSupported)
            {
                var feedback = device.SimpleHapticsController.SupportedFeedback[0];
                device.SimpleHapticsController.SendHapticFeedbackForDuration(feedback, 1.0, TimeSpan.FromMilliseconds(ms));
            }
        }


        const string BADGE_KEY = "acr.notifications.badge";
        protected int CurrentBadge
        {
            get
            {
                var values = ApplicationData.Current.LocalSettings.Values;
                var id = 0;
                if (values.ContainsKey(BADGE_KEY))
                    Int32.TryParse(values[BADGE_KEY] as string, out id);

                return id;
            }
            set => ApplicationData.Current.LocalSettings.Values[BADGE_KEY] = value.ToString();
        }


        const string CFG_KEY = "acr.notifications";
        protected virtual int GetNotificationId()
        {
            var id = 0;
            var s = ApplicationData.Current.LocalSettings.Values;
            if (s.ContainsKey(CFG_KEY))
            {
                id = Int32.Parse((string)s[CFG_KEY]);
            }
            id++;
            s[CFG_KEY] = id.ToString();
            return id;
        }


        protected virtual string ToQueryString(IDictionary<string, string> dict)
        {
            if (dict.Count == 0) return null;

            var qs = new System.Text.StringBuilder();
            foreach (var pair in dict)
            {
                qs.AppendFormat("{0}={1}&", pair.Key, pair.Value);
            }
            qs.Length -= 1; // remove trailing ambersand

            var xmlstr = WebUtility.HtmlEncode(qs.ToString());
            return xmlstr;
        }


        protected virtual IDictionary<string, string> FromQueryString(string queryString)
        {
            var dict = new Dictionary<string, string>();

            var qs = WebUtility.HtmlDecode(queryString);
            var pairStrs = qs.Split('&');
            foreach (var str in pairStrs)
            {
                var a = str.Split('=');
                dict.Add(a[0], a[1]);
            }

            return dict;
        }


        protected virtual bool IsAudioSupported =>
            AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop") &&
            !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2);
    }
}