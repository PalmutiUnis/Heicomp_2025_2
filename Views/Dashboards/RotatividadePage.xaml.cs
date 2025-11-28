using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards;

public partial class RotatividadePage : ContentPage
{
	public RotatividadePage(RotatividadeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}