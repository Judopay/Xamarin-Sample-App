using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Interop;
using Plugin.CurrentActivity;
using SampleApp.Helpers;

namespace Samples.Droid
{
    [Activity(Label = "Judopay", Icon = "@drawable/ic_launcher", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Locked, MainLauncher = true)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        [Export("SetSetting")]
        public string SetSetting(string setting)
        {
            switch (setting)
            {
                case "Maestro":
                    Settings.MaestroAllowed = true;
                    break;
                case "Amex":
                    Settings.AmexAllowed = true;
                    break;
                case "AVS":
                    Settings.AvsEnabled = true;
                    break;
                default:
                    throw new ArgumentException(setting + " is not a recognised setting");
            }
            return setting;
        }
    }
}
