using Microsoft.Maui.Controls;
using Heicomp_2025_2.ViewModels.Dashboards;

namespace Heicomp_2025_2.Views.Dashboards.ListaColaboradores
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
