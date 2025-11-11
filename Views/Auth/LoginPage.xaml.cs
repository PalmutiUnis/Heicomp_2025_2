using Duende.IdentityModel.OidcClient;
using System.Diagnostics;

// Precisamos disso para "new AppShell()" e "Application.Current"
using MauiApp1;

namespace Heicomp_2025_2.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        private const string GoogleClientId = "692804257983-4tk2efdcu97scvcrj3ajftbn35hqc6cf.apps.googleusercontent.com";
        //private const string GoogleClientSecret = "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"; // ✅ CORRETO (comentado)
        private OidcClient _oidcClient;

        public LoginPage()
        {
            InitializeComponent();

            var options = new OidcClientOptions
            {
                Authority = "https://accounts.google.com",
                ClientId = GoogleClientId,
                // ClientSecret = GoogleClientSecret, // ✅ CORRETO (comentado)
                Scope = "openid profile email",

                // 🛑 CORREÇÃO FEITA AQUI
                // Trocamos a barra "/" por dois-pontos ":"
                RedirectUri = "com.googleusercontent.apps.692804257983-4tk2efdcu97scvcrj3ajftbn35hqc6cf:/oauth2redirect",

                Browser = new MauiWebBrowser(),

                // ✅ CORRETO: Use Policy (não DiscoveryPolicy)
                Policy = new Policy
                {
                    Discovery = new Duende.IdentityModel.Client.DiscoveryPolicy
                    {
                        ValidateIssuerName = false,
                        ValidateEndpoints = false
                    }
                }
            };

            _oidcClient = new OidcClient(options);
        }

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[Login] Iniciando autenticação...");

                var loginResult = await _oidcClient.LoginAsync(new LoginRequest());

                if (loginResult.IsError)
                {
                    Debug.WriteLine($"[Login Erro] {loginResult.Error}");
                    Debug.WriteLine($"[Login Erro Descrição] {loginResult.ErrorDescription}");
                    await DisplayAlert("Erro", $"{loginResult.Error}\n\n{loginResult.ErrorDescription}", "OK");
                    return;
                }

                var accessToken = loginResult.AccessToken;
                var userEmail = loginResult.User?.FindFirst(c => c.Type == "email")?.Value;
                var userName = loginResult.User?.FindFirst(c => c.Type == "name")?.Value;

                Debug.WriteLine($"[Login Sucesso] Email: {userEmail}");

                // Supondo que você tenha um Label com x:Name="UserInfoLabel" no seu XAML
                // UserInfoLabel.Text = $"Logado como: {userName ?? userEmail}";

                await DisplayAlert("Sucesso", $"Bem-vindo, {userName ?? userEmail}!", "OK");

                // 🛑 MUDANÇA AQUI: Corrigindo o erro "Object reference..."
                // A linha 'await Shell.Current.GoToAsync...' causa o erro que você viu na imagem,
                // porque o Shell ainda não existe.

                // A forma correta é definir o AppShell (que contém sua "PainelGestaoPage") 
                // como a nova página principal do aplicativo.
                Debug.WriteLine("[Login] Sucesso! Carregando o AppShell...");
                Application.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login Exceção] {ex.GetType().Name}");
                Debug.WriteLine($"[Login Exceção] {ex.Message}");
                Debug.WriteLine($"[Login Stack] {ex.StackTrace}");
                await DisplayAlert("Erro", $"Erro ao fazer login:\n{ex.Message}", "OK");
            }
        }
    }
}