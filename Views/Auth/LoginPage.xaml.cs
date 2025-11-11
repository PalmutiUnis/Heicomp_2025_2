using Duende.IdentityModel.OidcClient;
using System.Diagnostics;

namespace Heicomp_2025_2.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        private const string GoogleClientId = "636539891129-ldsv7n2hf9sv5bt0t8n566hggaau3qhr.apps.googleusercontent.com";
        private const string GoogleClientSecret = "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";
        private OidcClient _oidcClient;

        public LoginPage()
        {
            InitializeComponent();

            var options = new OidcClientOptions
            {
                Authority = "https://oauth2.googleapis.com/device/code",//"https://accounts.google.com",
                ClientId = GoogleClientId,
                ClientSecret = GoogleClientSecret,
                Scope = "openid profile email",
                Browser = new MauiWebBrowser()
            };

            _oidcClient = new OidcClient(options);
        }

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var loginResult = await _oidcClient.LoginAsync(new LoginRequest());

                if (loginResult.IsError)
                {
                    Debug.WriteLine($"[Login Erro] {loginResult.Error}");
                    await DisplayAlert("Erro", loginResult.Error, "OK");
                    return;
                }

                var accessToken = loginResult.AccessToken;
                var userEmail = loginResult.User?.FindFirst(c => c.Type == "email")?.Value;

                Debug.WriteLine($"[Login Sucesso] Access Token: {accessToken}");
                UserInfoLabel.Text = $"Logado como: {userEmail}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login Exceção] {ex.Message}");
                await DisplayAlert("Erro", "Ocorreu um erro inesperado.", "OK");
            }
        }
    }
}