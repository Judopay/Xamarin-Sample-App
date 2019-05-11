using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Wallet;
using Android.Gms.Wallet.Fragment;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using JudoDotNetXamarin;
using JudoPayDotNet.Models;
using Newtonsoft.Json;

namespace Samples.Droid
{
    [Activity(Label = "Google Pay", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Locked, MainLauncher = false)]
    public class AndroidPayActivity : AppCompatActivity, GoogleApiClient.IOnConnectionFailedListener
    {
        const int MaskedWalletRequestCode = 501;
        const int FullWalletRequestCode = 601;

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
                var walletOptions = new WalletClass.WalletOptions.Builder().SetEnvironment(WalletEnvironment).Build();

                _googleApiClient = new GoogleApiClient.Builder(this)
                    .AddApi(WalletClass.API, walletOptions)
                    .EnableAutoManage(this, this)
                    .Build();

                CreateWalletFragment();
                CheckGooglePayAvailable();
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case MaskedWalletRequestCode:
                    PerformFullWalletRequest((MaskedWallet)data.GetParcelableExtra(WalletConstants.ExtraMaskedWallet));
                    break;

                case FullWalletRequestCode:
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
            Toast.MakeText(this, "Connection failed! " + result.ErrorMessage + ", " + result.ErrorCode, ToastLength.Short).Show();
        }

        void CheckGooglePayAvailable()
        {
            var result = WalletClass.Payments.IsReadyToPay(_googleApiClient, null);
            result.SetResultCallback(new GooglePayCallback(_walletFragment));
        }

        void PerformFullWalletRequest(MaskedWallet maskedWallet)
        {
            var request = FullWalletRequest.NewBuilder()
                .SetGoogleTransactionId(maskedWallet.GoogleTransactionId)
                .SetCart(Cart.NewBuilder()
                        .SetCurrencyCode(_judo.Currency)
                        .SetTotalPrice(_judo.Amount.ToString())
                    .Build())
                .Build();
            WalletClass.Payments.LoadFullWallet(_googleApiClient, request, FullWalletRequestCode);
        }

        async Task PerformJudoPayment(FullWallet fullWallet)
        {
            var androidPayModel = new AndroidPaymentModel
            {
                JudoId = _judo.JudoId,
                Currency = _judo.Currency,
                Amount = _judo.Amount,
                Wallet = new AndroidWalletModel
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
            return _paymentService.AndroidPayPayment(androidPayModel);
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

            _walletFragment = SupportWalletFragment.NewInstance(options);

            var parameters = PaymentMethodTokenizationParameters.NewBuilder()
                .SetPaymentMethodTokenizationType(WalletConstants.PaymentMethodTokenizationTypeNetworkToken)
                .AddParameter("publicKey", Resources.GetString(Resource.String.public_key))
                .Build();

            var walletRequest = MaskedWalletRequest.NewBuilder()
                .SetMerchantName(Resources.GetString(Resource.String.app_name))
                .SetCurrencyCode(_judo.Currency)
                .SetEstimatedTotalPrice(_judo.Amount.ToString())
                .SetPaymentMethodTokenizationParameters(parameters)
                .SetCart(Cart.NewBuilder().SetCurrencyCode(_judo.Currency).SetTotalPrice(_judo.Amount.ToString()).Build())
                .Build();

            var startParams = WalletFragmentInitParams.NewBuilder()
                    .SetMaskedWalletRequest(walletRequest)
                    .SetMaskedWalletRequestCode(MaskedWalletRequestCode)
                    .Build();

            _walletFragment.Initialize(startParams);

            SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.container, _walletFragment)
                .Commit();
        }

        class GooglePayCallback : Java.Lang.Object, IResultCallback
        {
            readonly SupportWalletFragment _walletFragment;

            public GooglePayCallback(SupportWalletFragment walletFragment)
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
                        Toast.MakeText(_walletFragment.Activity, "Google Pay is not available on your device", ToastLength.Short).Show();
                    }
                }
            }
        }
    }
}
