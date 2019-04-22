using System.Collections.Generic;
using JudoDotNetXamarin;
using SampleApp.Helpers;
using Xamarin.Forms;

namespace SampleApp
{
    public partial class SettingsPage : ContentPage
    {
        private List<string> _currencies = new List<string>
        {
            new BritishPoundCurrency().GetAbbreviation(),
            new EuroCurrency().GetAbbreviation(),
            new USDollarCurrency().GetAbbreviation()
        };

        public SettingsPage()
        {
            InitializeComponent();
            InitializeView();
        }

        public void InitializeView()
        {
            maestroSwitch.On = Settings.MaestroAllowed;
            maestroSwitch.PropertyChanged += (sender, e) =>
            {
                Settings.MaestroAllowed = maestroSwitch.On;
            };

            amexSwitch.On = Settings.AmexAllowed;
            amexSwitch.PropertyChanged += (sender, e) =>
            {
                Settings.AmexAllowed = amexSwitch.On;
            };

            avsSwitch.On = Settings.AvsEnabled;
            avsSwitch.PropertyChanged += (sender, e) =>
            {
                Settings.AvsEnabled = avsSwitch.On;
            };

            foreach (var currency in _currencies)
            {
                currencyPicker.Items.Add(currency);
            }

            currencyPicker.SelectedIndex = _currencies.IndexOf(Settings.Currency);
            currencyPicker.SelectedIndexChanged += (sender, e) =>
            {
                Settings.Currency = currencyPicker.Items[currencyPicker.SelectedIndex];
            };
        }
    }
}