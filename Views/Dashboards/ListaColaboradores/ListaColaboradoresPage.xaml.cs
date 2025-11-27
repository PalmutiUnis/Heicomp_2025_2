using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards.ListaColaboradores
{
    public partial class ListaColaboradoresPage : ContentPage
    {
        private readonly ListaColaboradoresViewModel _viewModel;
        private bool _initialized = false;

        public ListaColaboradoresPage(ListaColaboradoresViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized)
            {
                _initialized = true;
                await _viewModel.InitializeAsync();
            }
        }
    }
}
