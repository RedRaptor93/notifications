using Xamarin.Forms;


namespace Sample
{

    public class App : Application
    {
        public static bool IsInBackgrounded { get; private set; }


        public App()
        {
            this.MainPage = new NavigationPage(new MainPage());
        }


        protected override void OnResume()
        {
            base.OnResume();
            App.IsInBackgrounded = false;
        }


        protected override void OnSleep()
        {
            base.OnSleep();
            App.IsInBackgrounded = true;
        }

    }
}
