using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards;

public partial class PainelGestaoPage : ContentPage
{
	public PainelGestaoPage(PainelGestaoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}