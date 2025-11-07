using Android.App;
using Android.Content;
using Android.Content.PM;

// CORREÇÃO: O namespace deve corresponder à localização do arquivo
// (Assumindo que seu app se chama MauiApp1)
namespace MauiApp1.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },

        // CORREÇÃO CRÍTICA:
        // O DataScheme DEVE ser o seu GoogleClientId 
        DataScheme = "com.googleusercontent.apps.mfhnjsfgp3bbfk93vgkpsaaolng8rh6o-3371135215601")]
    public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}