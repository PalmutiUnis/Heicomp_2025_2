using Microsoft.Maui.Controls;
using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards
{
    public partial class ColaboradoresPage : ContentPage
    {
        private readonly ColaboradoresViewModel _viewModel;
        public ColaboradoresPage(ColaboradoresViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PainelGestaoPage");
        }
    }
}
