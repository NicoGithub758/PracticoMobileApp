using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace PracticoMobileApp.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.companyname.practicomobileapp",
        DataHost = "dev-tohysoy6fqmar1v7.us.auth0.com",
        DataPathPrefix = "/android/com.companyname.practicomobileapp/callback")]
    public class WebAuthCallbackActivity : WebAuthenticatorCallbackActivity
    {
    }
}
