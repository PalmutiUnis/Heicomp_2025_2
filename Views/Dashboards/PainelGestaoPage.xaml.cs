using Heicomp_2025_2.ViewModels.Dashboards;

namespace Heicomp_2025_2.Views.Dashboards;

public partial class PainelGestaoPage : ContentPage
{
	public PainelGestaoPage(PainelGestaoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}