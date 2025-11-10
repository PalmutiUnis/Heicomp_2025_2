using MauiApp1;

namespace Heicomp_2025_2.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        try
        {
            //Colocar a API do Google para fazer a autentica��o com um simples toque no bot�o
            // ... L�gica de autentica��o do Google ...

            // Se o login for BEM-SUCEDIDO:
            // Resolve AppShell via DI se disponível e substitui a MainPage
            var shell = this.Handler?.MauiContext?.Services?.GetService<AppShell>() ?? new AppShell();
            Application.Current!.MainPage = shell;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha na autentição com o Google: {ex.Message}", "OK");
        }
    }
}
