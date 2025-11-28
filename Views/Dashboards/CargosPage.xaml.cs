using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards
{
    public partial class CargosPage : ContentPage
    {
        public CargosPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PainelGestaoPage");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Resolve ViewModel via DI when page is displayed (works with Shell instantiation)
            if (BindingContext is not CargosViewModel vm)
            {
                var services = Application.Current?.Handler?.MauiContext?.Services;
                vm = services?.GetService<CargosViewModel>();
                if (vm != null)
                    BindingContext = vm;
            }

            if (BindingContext is CargosViewModel currentVm)
            {
                try
                {
                    await currentVm.CarregarDadosAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados de Cargos: {ex}");
                }
            }
        }
    }
}