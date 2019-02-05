using System;


namespace Samples.Uwp
{
    class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        public MainPage()
        {
            this.LoadApplication(new Samples.App());
        }
    }
}
