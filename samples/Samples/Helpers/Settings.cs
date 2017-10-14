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
        static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        const string MAESTRO_KEY = "maestro";
        const string AMEX_KEY = "amex";
        const string AVS_KEY = "avs";
        const string CURRENCY_KEY = "currency";
        const string CARD_TOKEN_KEY = "cardToken";

        public static bool MaestroAllowed
        {
            get
            {
                return AppSettings.GetValueOrDefault(MAESTRO_KEY, true);
            }
            set
            {
                AppSettings.AddOrUpdateValue(MAESTRO_KEY, value);
            }
        }

        public static string CardToken
        {
            get
            {
                return AppSettings.GetValueOrDefault(CARD_TOKEN_KEY, null);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CARD_TOKEN_KEY, value);
            }
        }

        public static bool AmexAllowed
        {
            get
            {
                return AppSettings.GetValueOrDefault(AMEX_KEY, true);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AMEX_KEY, value);
            }
        }

        public static bool AvsEnabled
        {
            get
            {
                return AppSettings.GetValueOrDefault(AVS_KEY, false);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AVS_KEY, value);
            }
        }

        public static string Currency
        {
            get
            {
                return AppSettings.GetValueOrDefault(CURRENCY_KEY, new BritishPoundCurrency().GetAbbreviation());
            }
            set
            {
                AppSettings.AddOrUpdateValue(CURRENCY_KEY, value);
            }
        }
    }
}
