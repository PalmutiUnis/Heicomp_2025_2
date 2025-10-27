using MauiApp1;

namespace Heicomp_2025_2.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void BotaoLogin(object sender, EventArgs e)
    {

        App.Current.MainPage = new AppShell();

        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}