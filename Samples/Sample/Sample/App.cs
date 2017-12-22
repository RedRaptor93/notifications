using Plugin.Notifications;
using System.Linq;
using Xamarin.Forms;


namespace Sample
{

    public class App : Application
    {
        public static bool InBackground { get; private set; }


        public App()
        {
            this.MainPage = new NavigationPage(new MainPage());
            CrossNotifications.Current.Activated += Notification_Activated;
        }


        private void Notification_Activated(object sender, Notification e)
        {
            System.Diagnostics.
                Debug.WriteLine("*App* Notification activated! Id: {0}, SendTime: {1}, Metadata count: {2}", 
                                e.Id, e.SendTime, e.Metadata.Count);
        }


        protected override void OnResume()
        {
            App.InBackground = false;
            base.OnResume();
        }


        protected override void OnSleep()
        {
            App.InBackground = true;
            base.OnSleep();
        }

    }
}
