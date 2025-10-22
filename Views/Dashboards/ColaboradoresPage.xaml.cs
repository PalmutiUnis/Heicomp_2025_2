namespace Heicomp_2025_2.Views.Dashboards;

public partial class ColaboradoresPage : ContentPage
{
	public ColaboradoresPage()
	{
		InitializeComponent();
	}

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}