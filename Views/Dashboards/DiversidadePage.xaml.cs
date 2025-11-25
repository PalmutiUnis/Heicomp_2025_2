using MauiApp1.ViewModels.Dashboards;
using System.Diagnostics;

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
            try
            {
                var picker = (Picker)sender;

                if (picker.SelectedItem == null)
                {
                    Debug.WriteLine("[Page] SelectedItem é null");
                    return;
                }

                var selectedValue = picker.SelectedItem.ToString();
                Debug.WriteLine($"[Page] Ano selecionado: {selectedValue}");

                if (int.TryParse(selectedValue, out int ano))
                {
                    if (BindingContext is DiversidadeViewModel vm)
                    {
                        Debug.WriteLine($"[Page] Atualizando ViewModel para ano: {ano}");
                        vm.AnoSelecionado = ano;
                    }
                    else
                    {
                        Debug.WriteLine("[Page] BindingContext não é DiversidadeViewModel");
                    }
                }
                else
                {
                    Debug.WriteLine($"[Page] Falha ao converter '{selectedValue}' para int");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Page] Erro em OnAnoSelecionado: {ex.Message}");
                Debug.WriteLine($"[Page] StackTrace: {ex.StackTrace}");
            }
        }

        private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Page] Erro ao voltar: {ex.Message}");
            }
        }
    }
}