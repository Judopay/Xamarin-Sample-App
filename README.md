#Judo Xamarin SDK

The Judopay library lets you integrate card payments into your Xamarin Forms project. It is built to be mobile first with ease of integration in mind. Judopay's SDK enables a faster, simpler and more secure payment experience within your app. Build trust and user loyalty in your app with our secure and intuitive UX.

## Requirements
- Visual Studio for Mac 7.3 / Visual Studio 2017
- Xamarin Forms 2.5.0.122203
- Xcode 9
- Android 8.0 (API 26) SDK and build tools 26.0.3

The SDK is compatible with Android Jelly Bean (4.1) and above and iOS 8 and above.

## Getting started

#### 1. Integration

Add the `Xamarin.JudoPay` NuGet package to your .Net Standard 2.0 Library, Android and iOS projects.

#### 2. Setup

In your Xamarin Forms page, create a new Judo instance:

```csharp
var judo = new Judo()
{
    JudoId = "<JUDO_ID>",
    Token = "<API_TOKEN>",
    Secret = "<API_SECRET>",
    Environment = JudoEnvironment.Sandbox,
    Amount = 1.50m,
    Currency = "GBP",
    ConsumerReference = "YourUniqueReference"
};
```

__Note:__ Please make sure that you are using a unique Consumer Reference for each different consumer.

#### 3. Configure iOS and Android projects

Android and iOS require additional steps to get set up with the Xamarin SDK. See the sample apps for more information.

#### 4. Make a payment

Create a PaymentPage to show the card entry screen for Payment:

```chsarp
var paymentPage = new PaymentPage(judo);
Navigation.PushAsync(paymentPage);
```

#### 4. Check the payment result

Receive the result of the payment:

```csharp
paymentPage.resultHandler += async (sender, result) =>
{
	if ("Success".Equals(result.Response.Result))
	{
		// handle successful payment
		// close payment page
		await Navigation.PopAsync();
	}
};
```

## Next steps

The Judopay Xamarin library supports a range of customization options. For more information on using Judopay for Xamarin see our [wiki documentation](https://github.com/JudoPay/Judo-Xamarin/wiki).
