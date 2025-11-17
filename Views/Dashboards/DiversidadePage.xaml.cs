using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards
{
    public partial class DiversidadePage : ContentPage
    {
        public DiversidadePage(DiversidadeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnAnoSelecionado(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedItem != null &&
                int.TryParse(picker.SelectedItem.ToString(), out int ano))
            {
                if (BindingContext is DiversidadeViewModel vm)
                {
                    vm.AnoSelecionado = ano;
                }
            }
        }

        private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}