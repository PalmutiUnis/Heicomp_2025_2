// Local: Platforms/Android/WebAuthenticatorCallbackActivity.cs
using Android.App;
using Android.Content;
using Android.Content.PM;

namespace MauiApp1.Plataforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    // Este DataScheme está CORRETO (é o seu Reverse Client ID)
    DataScheme = "com.googleusercontent.apps.692804257983-4tk2efdcu97scvcrj3ajftbn35hqc6cf")]
public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}