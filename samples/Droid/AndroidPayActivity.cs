﻿using Android.App;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.V7.App;
using Android.Gms.Wallet;
using Android.Gms.Common;
using Android.Content;
using JudoDotNetXamarin;
using JudoPayDotNet.Models;
using Android.Gms.Wallet.Fragment;
using Android.Widget;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Android.Content.PM;

namespace Samples.Droid
{
    [Activity(Label = "Android Pay", Theme = "@style/JudoTheme.ActionBar", ScreenOrientation = ScreenOrientation.Locked, MainLauncher = false)]
    public class AndroidPayActivity : AppCompatActivity, GoogleApiClient.IOnConnectionFailedListener
    {
        const int MaskedWalletRequest = 501;
        const int FullWalletRequest = 601;

        public const string JudoExtra = "Judo";
        public const string IsPreAuthExtra = "IsPreAuth";

        const int WalletEnvironment = WalletConstants.EnvironmentTest;

        GoogleApiClient _googleApiClient;
        IPaymentService _paymentService;
        SupportWalletFragment _walletFragment;
        Judo _judo;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AndroidPay);

            _judo = JsonConvert.DeserializeObject<Judo>(Intent.GetStringExtra(JudoExtra));

            _paymentService = new PaymentService(_judo);

            if (savedInstanceState == null)
            {
                _googleApiClient = new GoogleApiClient.Builder(this)
                     .AddApi(WalletClass.API, new WalletClass.WalletOptions.Builder()
                     .SetEnvironment(WalletEnvironment)
                     .Build())
                    .EnableAutoManage(this, this)
                    .Build();

                CreateWalletFragment();
                CheckAndroidPayAvailable();
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case MaskedWalletRequest:
                    PerformFullWalletRequest((MaskedWallet)data.GetParcelableExtra(WalletConstants.ExtraMaskedWallet));
                    break;

                case FullWalletRequest:
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    PerformJudoPayment((FullWallet)data.GetParcelableExtra(WalletConstants.ExtraFullWallet));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    break;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            _googleApiClient.Connect();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _googleApiClient.Disconnect();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            ShowToast("Connection failed! " + result.ErrorMessage + ", " + result.ErrorCode);
        }

        void ShowToast(string text)
        {
            Toast.MakeText(this, text, ToastLength.Short).Show();
        }

        void CheckAndroidPayAvailable()
        {
            var result = WalletClass.Payments.IsReadyToPay(_googleApiClient, null);
            result.SetResultCallback(new AndroidPayCallback(_walletFragment));
        }

        void PerformFullWalletRequest(MaskedWallet maskedWallet)
        {
            var request = Android.Gms.Wallet.FullWalletRequest.NewBuilder()
                                           .SetGoogleTransactionId(maskedWallet.GoogleTransactionId)
                                           .SetCart(Cart.NewBuilder()
                                                    .SetCurrencyCode(_judo.Currency)
                                                    .SetTotalPrice(_judo.Amount.ToString())
                                                    .Build())
                                           .Build();
            WalletClass.Payments.LoadFullWallet(_googleApiClient, request, FullWalletRequest);
        }

        async Task PerformJudoPayment(FullWallet fullWallet)
        {
            var androidPayModel = new AndroidPaymentModel()
            {
                JudoId = _judo.JudoId,
                Currency = _judo.Currency,
                Amount = _judo.Amount,
                Wallet = new AndroidWalletModel()
                {
                    Environment = WalletEnvironment,
                    PublicKey = Resources.GetString(Resource.String.public_key),
                    GoogleTransactionId = fullWallet.GoogleTransactionId,
                    InstrumentDetails = fullWallet.GetInstrumentInfos()[0].InstrumentDetails,
                    InstrumentType = fullWallet.GetInstrumentInfos()[0].InstrumentType,
                    PaymentMethodToken = fullWallet.PaymentMethodToken.Token
                }
            };
            var result = await PerformTransaction(androidPayModel);

            if (result.HasError || "Success".Equals(result.Response.Result))
            {
                Toast.MakeText(_walletFragment.Activity, "Payment successful", ToastLength.Short).Show();
            }
            else if ("Declined".Equals(result.Response.Result))
            {
                Toast.MakeText(_walletFragment.Activity, "Payment declined", ToastLength.Short).Show();
            }
        }

        Task<IResult<ITransactionResult>> PerformTransaction(AndroidPaymentModel androidPayModel)
        {
            if (Intent.Extras.GetBoolean(IsPreAuthExtra))
            {
                return _paymentService.AndroidPayPreAuth(androidPayModel);
            }
            else
            {
                return _paymentService.AndroidPayPayment(androidPayModel);
            }
        }

        void CreateWalletFragment()
        {
            var walletStyle = new WalletFragmentStyle()
                .SetBuyButtonText(WalletFragmentStyle.BuyButtonText.LogoOnly)
                .SetBuyButtonAppearance(WalletFragmentStyle.BuyButtonAppearance.AndroidPayLightWithBorder)
                .SetBuyButtonWidth(WalletFragmentStyle.Dimension.MatchParent);

            var options = WalletFragmentOptions.NewBuilder()
                                               .SetEnvironment(WalletEnvironment)
                                               .SetTheme(WalletConstants.ThemeDark)
                                               .SetFragmentStyle(walletStyle)
                                               .SetMode(WalletFragmentMode.BuyButton)
                                               .Build();

            var parameters = PaymentMethodTokenizationParameters.NewBuilder()
                                                                .SetPaymentMethodTokenizationType(PaymentMethodTokenizationType.NetworkToken)
                                                                .AddParameter("publicKey", Resources.GetString(Resource.String.public_key))
                                                                .Build();

            _walletFragment = SupportWalletFragment.NewInstance(options);

            var walletRequest = Android.Gms.Wallet.MaskedWalletRequest.NewBuilder()
                                                   .SetMerchantName(Resources.GetString(Resource.String.app_name))
                                       .SetCurrencyCode(_judo.Currency)
                                       .SetEstimatedTotalPrice(_judo.Amount.ToString())
                                                   .SetPaymentMethodTokenizationParameters(parameters)
                                                   .SetCart(Cart.NewBuilder()
                                                            .SetCurrencyCode(_judo.Currency)
                                                            .SetTotalPrice(_judo.Amount.ToString())
                                                            .Build())
                                                    .Build();

            var startParams = WalletFragmentInitParams.NewBuilder()
                    .SetMaskedWalletRequest(walletRequest)
                    .SetMaskedWalletRequestCode(MaskedWalletRequest)
                    .Build();

            _walletFragment.Initialize(startParams);

            SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.container, _walletFragment)
                .Commit();
        }

        class AndroidPayCallback : Java.Lang.Object, IResultCallback
        {
            readonly SupportWalletFragment _walletFragment;

            public AndroidPayCallback(SupportWalletFragment walletFragment)
            {
                _walletFragment = walletFragment;
            }

            public void OnResult(Java.Lang.Object result)
            {
                if (_walletFragment != null)
                {
                    var enabled = ((BooleanResult)result).Status.IsSuccess && ((BooleanResult)result).Value;
                    _walletFragment.SetEnabled(enabled);

                    if (!enabled)
                    {
                        Toast.MakeText(_walletFragment.Activity, "Android Pay is not available on your device", ToastLength.Short).Show();
                    }
                }
            }
        }
    }
}