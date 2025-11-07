using MauiApp1;
using System.Diagnostics; // Adicionado para Debug.WriteLine


using Duende.IdentityModel.OidcClient;
// (Certifique-se que este namespace está correto)
using Heicomp_2025_2.Views.Auth;


namespace Heicomp_2025_2.Views.Auth;

public partial class LoginPage : ContentPage
{
    // Você pode usar #if ... #endif para trocar em tempo de compilação
    private const string GoogleClientId = "1065125311733-o6hr8gnloaaspkgv39kfbb3pgfsjnhfm.apps.googleusercontent.com";

    // Use o Client Secret da sua credencial WEB
    private const string GoogleClientSecret = "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";

    private OidcClient _oidcClient;

    public LoginPage()
    {
        InitializeComponent();

        // Configura o OidcClient
        var options = new OidcClientOptions
        {
            Authority = "https://accounts.google.com",
            ClientId = GoogleClientId,  // O Client ID Nativo (iOS ou Android)
            ClientSecret = GoogleClientSecret, // O Client Secret da Web
            Scope = "openid profile email", // Escopos que você quer,

            // CORREÇÃO: O namespace Heicomp_2025_2.Views.Auth deve corresponder ao arquivo MauiWebBrowser.cs
            Browser = new Heicomp_2025_2.Views.Auth.MauiWebBrowser(),

            // O RedirectUri DEVE ser o seu ClientID reverso (sem o :/oauth2redirect)
            RedirectUri = "com.googleusercontent.apps.1065125311733-o6hr8gnloaaspkgv39kfbb3pgfsjnhfm"
        };

        _oidcClient = new OidcClient(options);
    }

    private async void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Inicia o Login
            var loginResult = await _oidcClient.LoginAsync(new LoginRequest());

            // 2. Verifica se houve erro
            if (loginResult.IsError)
            {
                Debug.WriteLine($"[Login Erro] {loginResult.Error}");
                await DisplayAlert("Erro", loginResult.Error, "OK");
                return;
            }

            // 3. Sucesso!
            var accessToken = loginResult.AccessToken;
            var idToken = loginResult.IdentityToken;
            var userEmail = loginResult.User?.FindFirst(c => c.Type == "email")?.Value;

            Debug.WriteLine($"[Login Sucesso] Access Token: {accessToken}");
            UserInfoLabel.Text = $"Logado como: {userEmail}";

            // Aqui você pode salvar o Access Token, navegar para outra página, etc.
            // Ex: Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Login Exceção] {ex.Message}");
            await DisplayAlert("Erro", "Ocorreu um erro inesperado.", "OK");
        }
    }
}