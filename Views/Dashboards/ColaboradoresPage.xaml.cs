using Microsoft.Maui.Controls;
using Heicomp_2025_2.ViewModels.Dashboards;

namespace Heicomp_2025_2.Views.Dashboards
{
    public partial class ColaboradoresPage : ContentPage
    {
        private readonly ColaboradoresViewModel _viewModel;
        public ColaboradoresPage()
        {
            InitializeComponent();
            _viewModel = new ColaboradoresViewModel();
            BindingContext = _viewModel;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PainelGestaoPage");
        }
    }
}
