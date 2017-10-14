using System;
using System.Collections.Generic;
using JudoDotNetXamarin;
using JudoPayDotNet.Enums;
using JudoPayDotNet.Models;
using Newtonsoft.Json;
using SampleApp.Helpers;
using Samples;
using Xamarin.Forms;

namespace SampleApp
{
    public partial class SamplesPage : KeyboardAwarePage
    {
        IApplePayService _applePayService;

        public SamplesPage()
        {
            InitializeComponent();
            InitializeView();

            NavigationPage.SetBackButtonTitle(this, "Back");

            ToolbarItems.Add(new ToolbarItem("Settings", "", () =>
            {
                Navigation.PushAsync(new SettingsPage());
            }));
        }

        Judo BuildJudo()
        {
            return new Judo()
            {
                JudoId = "<JUDO_ID>",
                Token = "<API_TOKEN>",
                Secret = "<API_SECRET>",
                Environment = JudoEnvironment.Sandbox,
                Amount = 0.01m,
                Currency = Settings.Currency,
                MaestroAccepted = Settings.MaestroAllowed,
                AmexAccepted = Settings.AmexAllowed,
                AvsEnabled = Settings.AvsEnabled,
                ConsumerReference = "XamarinSdkConsumerRef",
                /*Theme = new Theme
				{
					PrimaryColor = Color.White,
					BackgroundColor = Color.FromHex("273338"),
					ButtonBackgroundColor = Color.FromHex("F0C559"),
					ButtonTextColor = Color.FromHex("273338"),
					HintTextColor = Color.FromHex("A8ADAE"),
					LabelTextColor = Color.White,
					SecondaryTextColor = Color.Fuchsia,
					ButtonLabel = "PAY NOW!",
					EntryLabelPLaceholderColor = Color.FromHex("F0C559")
				}*/
            };
        }

        Judo BuildJudoForApplePay()
        {
            var judo = BuildJudo();

            judo.JudoId = "<JUDO_ID>";
            judo.Token = "<API_TOKEN>";
            judo.Secret = "<API_SECRET>";
            judo.Environment = JudoEnvironment.Live;

            return judo;
        }

        public void InitializeView()
        {
            payButton.Clicked += ShowPaymentForm;
            addCardButton.Clicked += ShowAddCard;
            tokenPaymentButton.Clicked += ShowTokenPaymentForm;
            tokenPreAuthButton.Clicked += ShowTokenPreAuthForm;
            preAuthButton.Clicked += ShowPreAuthForm;

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
            {
                _applePayService = DependencyService.Get<IApplePayService>();

                if (_applePayService.IsApplePayAvailable(BuildJudo()))
                {
                    applePayPaymentButton.Clicked += PerformApplePayPayment;
                    applePayPreAuthButton.Clicked += PerformApplePayPreAuth;

                    applePayPaymentButton.IsVisible = true;
                    applePayPaymentButton.IsEnabled = true;
                    applePayPreAuthButton.IsVisible = true;
                    applePayPreAuthButton.IsEnabled = true;
                }
            }

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                var service = DependencyService.Get<IAndroidPayService>();

                androidPayPaymentButton.IsVisible = true;
                androidPayPaymentButton.Clicked += (sender, e) =>
                {
                    service.payment(BuildJudo());
                };

                androidPayPreAuthButton.IsVisible = true;
                androidPayPreAuthButton.Clicked += (sender, e) =>
                {
                    service.preAuth(BuildJudo());
                };
            }
        }

        void ShowPreAuthForm(object sender, EventArgs e)
        {
            var preAuthPage = new PreAuthPage(BuildJudo());
            preAuthPage.resultHandler += PreAuthHandler;
            Navigation.PushAsync(preAuthPage);
        }

        void ShowPaymentForm(object sender, EventArgs e)
        {
            var paymentPage = new PaymentPage(BuildJudo());
            paymentPage.resultHandler += Handler;
            Navigation.PushAsync(paymentPage);
        }

        void ShowAddCard(object sender, EventArgs e)
        {
            var page = new RegisterCardPage(BuildJudo());
            page.resultHandler += RegisterCardHandler;
            Navigation.PushAsync(page);
        }

        async void ShowTokenPaymentForm(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.CardToken))
            {
                var page = new TokenPaymentPage(BuildJudo(), GetTokenViewModel());
                page.resultHandler += Handler;

                await Navigation.PushAsync(page);
            }
            else
            {
                await DisplayAlert("No card saved", "Add card before attempting token payment", "OK");
            }
        }

        async void ShowTokenPreAuthForm(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.CardToken))
            {
                var page = new TokenPreAuthPage(BuildJudo(), GetTokenViewModel());
                page.resultHandler += PreAuthHandler;

                await Navigation.PushAsync(page);
            }
            else
            {
                await DisplayAlert("No card saved", "Add card before attempting token payment", "OK");
            }
        }

        TokenPaymentDefaultsViewModel GetTokenViewModel()
        {
            var token = JsonConvert.DeserializeObject<TokenPaymentViewModel>(Settings.CardToken);
            return new TokenPaymentDefaultsViewModel(token.LastFour, token.ExpiryDate, token.Token, token.ConsumerToken, token.CardType);
        }

        void PerformApplePayPayment(object sender, EventArgs e)
        {
            _applePayService.Payment(BuildJudoForApplePay(), BuildWalletModel(), ApplePaySucces, ApplePayFailure);
        }

        void PerformApplePayPreAuth(object sender, EventArgs e)
        {
            _applePayService.PreAuth(BuildJudoForApplePay(), BuildWalletModel(), ApplePaySucces, ApplePayFailure);
        }

        public async void ApplePaySucces(PaymentReceiptModel receipt)
        {
            await Navigation.PopAsync();
            await DisplayAlert("Payment successful", "Receipt ID: " + receipt.ReceiptId, "OK");
        }

        public async void ApplePayFailure(JudoError error, PaymentReceiptModel receipt = null)
        {
            await Navigation.PopAsync();
            await DisplayAlert("Payment error", "", "OK");
        }

        internal async void Handler(object sender, IResult<ITransactionResult> result)
        {
            await Navigation.PopAsync();
            if (result.HasError)
            {
                await DisplayAlert("Payment error", "Code: " + result.Error.Code, "OK");
            }
            else if ("Success".Equals(result.Response.Result))
            {
                await DisplayAlert("Payment successful", "Receipt ID: " + result.Response.ReceiptId, "OK");
            }
        }

        internal async void PreAuthHandler(object sender, IResult<ITransactionResult> result)
        {
            await Navigation.PopAsync();
            if (result.HasError)
            {
                await DisplayAlert("Pre Auth error", "Code: " + result.Error.Code, "OK");
            }
            else if ("Success".Equals(result.Response.Result))
            {
                await DisplayAlert("Pre Auth successful", "Receipt ID: " + result.Response.ReceiptId, "OK");
            }
        }

        internal async void RegisterCardHandler(object sender, IResult<ITransactionResult> result)
        {
            await Navigation.PopAsync();

            if (result.HasError)
            {
                await DisplayAlert("Error adding card", "Code: " + result.Error.Code, "OK");
            }
            else if ("Success".Equals(result.Response.Result))
            {
                var receipt = result.Response as PaymentReceiptModel;
                var cardType = (CardNetwork)Enum.Parse(typeof(CardNetwork), receipt.CardDetails.CardType.ToString());

                var token = new TokenPaymentViewModel
                {
                    LastFour = receipt.CardDetails.CardLastfour,
                    ExpiryDate = receipt.CardDetails.EndDate,
                    CardType = cardType,
                    Token = receipt.CardDetails.CardToken,
                    ConsumerToken = receipt.Consumer.ConsumerToken,
                    ConsumerReference = receipt.Consumer.YourConsumerReference
                };

                Settings.CardToken = JsonConvert.SerializeObject(token);

                if (await DisplayAlert("Card added", "Perform token payment?", "Yes", "No"))
                {
                    var page = new TokenPaymentPage(BuildJudo(), new TokenPaymentDefaultsViewModel(token.LastFour, token.ExpiryDate, token.Token, token.ConsumerToken, token.CardType));
                    page.resultHandler += Handler;
                    await Navigation.PushAsync(page);
                }
            }
        }

        ApplePayModel BuildWalletModel()
        {
            return new ApplePayModel
            {
                Items = new List<ApplePayItem>
                {
                    new ApplePayItem("Pie and Mash", 0.01m),
                    new ApplePayItem("Soup", 0.01m)
                },
                ConsumerRef = "Myconsumerref",
                CountryCode = "GB",
                CurrencyCode = "GBP",
                ItemsSummaryLabel = "Shopping basket",
                MerchantIdentifier = "merchant.com.judo.XamarinFormsSample",
                SupportedCardNetworks = new List<ApplePayCardNetwork>
                {
                    ApplePayCardNetwork.Amex,
                    ApplePayCardNetwork.Mastercard,
                    ApplePayCardNetwork.Visa
                }
            };
        }
    }
}