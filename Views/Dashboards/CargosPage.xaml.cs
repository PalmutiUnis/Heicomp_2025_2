namespace Heicomp_2025_2.Views.Dashboards;

public partial class CargosPage : ContentPage
{
	public CargosPage()
	{
		InitializeComponent();
	}

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}