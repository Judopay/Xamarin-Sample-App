using System;

using Foundation;
using JudoDotNetXamariniOSSDK;
using SampleApp.Helpers;
using UIKit;
using Xamarin.Forms;

namespace Samples.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new App());

			DependencyService.Register<ClientService>();
			DependencyService.Register<HttpClientHelper>();
			DependencyService.Register<ApplePayService>();

			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

			return base.FinishedLaunching(app, options);
		}

		[Export("setSetting:")]
		public NSString SetSetting(NSString setting)
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
