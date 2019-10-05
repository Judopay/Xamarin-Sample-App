using UIKit;

namespace Samples.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // workaround for https://github.com/xamarin/xamarin-macios/issues/6654
            using (var _ = new AddressBook.ABGroup())
            {
            }
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
