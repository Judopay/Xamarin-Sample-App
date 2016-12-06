// Helpers/Settings.cs
using JudoDotNetXamarin;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace SampleApp.Helpers
{
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	public static class Settings
	{
		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		#region Setting Constants

		private const string MAESTRO_KEY = "maestro";
		private const string AMEX_KEY = "amex";
		private const string AVS_KEY = "avs";
		private const string CURRENCY_KEY = "currency";
		private const string CARD_TOKEN_KEY = "cardToken";

		#endregion

		public static bool MaestroAllowed
		{
			get
			{
				return AppSettings.GetValueOrDefault<bool>(MAESTRO_KEY, true);
			}
			set
			{
				AppSettings.AddOrUpdateValue<bool>(MAESTRO_KEY, value);
			}
		}

		public static string CardToken
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(CARD_TOKEN_KEY, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue<string>(CARD_TOKEN_KEY, value);
			}
		}

		public static bool AmexAllowed
		{
			get
			{
				return AppSettings.GetValueOrDefault<bool>(AMEX_KEY, true);
			}
			set
			{
				AppSettings.AddOrUpdateValue<bool>(AMEX_KEY, value);
			}
		}

		public static bool AvsEnabled
		{
			get
			{
				return AppSettings.GetValueOrDefault<bool>(AVS_KEY, false);
			}
			set
			{
				AppSettings.AddOrUpdateValue<bool>(AVS_KEY, value);
			}
		}

		public static string Currency
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(CURRENCY_KEY, new BritishPoundCurrency().GetAbbreviation());
			}
			set
			{
				AppSettings.AddOrUpdateValue<string>(CURRENCY_KEY, value);
			}
		}

	}
}