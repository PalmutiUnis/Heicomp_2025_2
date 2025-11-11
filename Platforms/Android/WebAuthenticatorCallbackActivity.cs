// Local: Platforms/Android/WebAuthenticatorCallbackActivity.cs
using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Heicomp_2025_2.Plataforms.Android; // <-- Mude para o namespace do seu app

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    // Este DataScheme DEVE ser o seu Reverse Client ID
    DataScheme = "com.googleusercontent.apps.636539891129-ldsv7n2hf9sv5bt0t8n566hggaau3qhr")]
public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}