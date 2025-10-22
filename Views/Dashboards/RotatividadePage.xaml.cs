namespace Heicomp_2025_2.Views.Dashboards;

public partial class RotatividadePage : ContentPage
{
	public RotatividadePage()
	{
		InitializeComponent();
	}

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}