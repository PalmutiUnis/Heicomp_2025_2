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
            //Colocar a API do Google para fazer a autenticação com um simples toque no botão
            // ... Lógica de autenticação do Google ...

            // Se o login for BEM-SUCEDIDO:
            // Isso substitui a LoginPage (e sua NavigationPage) pelo AppShell
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha na autenticação com o Google: {ex.Message}", "OK");
        }
    }
}
