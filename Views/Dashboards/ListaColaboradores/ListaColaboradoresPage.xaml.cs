using Microsoft.Maui.Controls;
using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards.ListaColaboradores
{
    public partial class ListaColaboradoresPage : ContentPage
    {
        private bool _initialized = false;

        public ListaColaboradoresPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized && BindingContext is ListaColaboradoresViewModel vm)
            {
                _initialized = true;
                await vm.InitializeAsync();
            }
        }
    }
}
