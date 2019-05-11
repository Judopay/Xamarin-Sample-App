using Android.Content;
using JudoDotNetXamarin;
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using Samples.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidPayService))]
namespace Samples.Droid
{
    public class AndroidPayService : IAndroidPayService
    {
        public void Payment(Judo judo)
        {
            Intent intent = new Intent(CrossCurrentActivity.Current.Activity, typeof(AndroidPayActivity));
            var json = JsonConvert.SerializeObject(judo);

            intent.PutExtra(AndroidPayActivity.JudoExtra, json);

            CrossCurrentActivity.Current.Activity.StartActivity(intent);
        }

        public void PreAuth(Judo judo)
        {
            Intent intent = new Intent(CrossCurrentActivity.Current.Activity, typeof(AndroidPayActivity));
            var json = JsonConvert.SerializeObject(judo);

            intent.PutExtra(AndroidPayActivity.JudoExtra, json);
            intent.PutExtra(AndroidPayActivity.IsPreAuthExtra, true);

            CrossCurrentActivity.Current.Activity.StartActivity(intent);
        }
    }
}
