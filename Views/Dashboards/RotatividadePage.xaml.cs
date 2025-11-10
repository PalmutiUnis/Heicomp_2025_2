using Heicomp_2025_2.ViewModels.Dashboards;

namespace Heicomp_2025_2.Views.Dashboards;

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