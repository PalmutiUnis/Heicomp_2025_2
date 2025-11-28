using MauiApp1.ViewModels.Dashboards;

namespace MauiApp1.Views.Dashboards
{
    public partial class GraficosDetalhadosPage : ContentPage
    {
        private readonly GraficosDetalhadosViewModel _viewModel;

        public GraficosDetalhadosPage(GraficosDetalhadosViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        // Sobrescreve o OnAppearing para carregar os dados com segurança
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Só carrega se ainda não tiver dados ou se quiser forçar atualização
            if (_viewModel.GraficoUnidade == null)
            {
                await _viewModel.CarregarDadosAsync();
            }
        }

        private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PainelGestaoPage");
        }
    }
}