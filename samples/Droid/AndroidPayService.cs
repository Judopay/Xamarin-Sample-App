using System;
using Android.Content;
using JudoDotNetXamarin;
using JudoPayDotNet.Enums;
using Newtonsoft.Json;
using Samples.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidPayService))]
namespace Samples.Droid
{
	public class AndroidPayService : IAndroidPayService
	{
		public AndroidPayService() { }

		public void payment(Judo judo)
		{
			Intent intent = new Intent(Xamarin.Forms.Forms.Context, typeof(AndroidPayActivity));
			var json = JsonConvert.SerializeObject(judo);

			intent.PutExtra(AndroidPayActivity.JudoExtra, json);

			Xamarin.Forms.Forms.Context.StartActivity(intent);
		}

		public void preAuth(Judo judo)
		{
			Intent intent = new Intent(Xamarin.Forms.Forms.Context, typeof(AndroidPayActivity));
			var json = JsonConvert.SerializeObject(judo);

			intent.PutExtra(AndroidPayActivity.JudoExtra, json);
			intent.PutExtra(AndroidPayActivity.IsPreAuthExtra, true);

			Xamarin.Forms.Forms.Context.StartActivity(intent);
		}
	}
}