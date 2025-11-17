using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels.Dashboards
{
    public class DiversidadeViewModel : INotifyPropertyChanged
    {
        private readonly DiversidadeService _service;

        // Dados gerais
        private DiversidadeGeral _dadosGerais;
        public DiversidadeGeral DadosGerais
        {
            get => _dadosGerais;
            set { _dadosGerais = value; OnPropertyChanged(); }
        }

        // Ano selecionado
        private int _anoSelecionado = 2024;
        public int AnoSelecionado
        {
            get => _anoSelecionado;
            set
            {
                if (_anoSelecionado != value)
                {
                    _anoSelecionado = value;
                    OnPropertyChanged();
                    _ = CarregarDadosAsync();
                }
            }
        }

        // Loading
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Coleções observáveis
        public ObservableCollection<DistribuicaoGenero> DistribuicoesGenero { get; set; }
        public ObservableCollection<DistribuicaoPCD> DistribuicoesPCD { get; set; }
        public ObservableCollection<DistribuicaoRacaEtnia> DistribuicoesRaca { get; set; }
        public ObservableCollection<DistribuicaoEstadoCivil> DistribuicoesEstadoCivil { get; set; }

        public DiversidadeViewModel(DiversidadeService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            // Inicializar coleções
            DistribuicoesGenero = new ObservableCollection<DistribuicaoGenero>();
            DistribuicoesPCD = new ObservableCollection<DistribuicaoPCD>();
            DistribuicoesRaca = new ObservableCollection<DistribuicaoRacaEtnia>();
            DistribuicoesEstadoCivil = new ObservableCollection<DistribuicaoEstadoCivil>();

            // Carregar dados iniciais
            _ = CarregarDadosAsync();
        }

        public async Task CarregarDadosAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[ViewModel] Carregando dados do ano {AnoSelecionado}...");

                // Carregar dados gerais
                DadosGerais = await _service.ObterDadosGeraisAsync(AnoSelecionado);

                // Carregar distribuição por gênero
                var generos = await _service.ObterDistribuicaoGeneroAsync(AnoSelecionado);
                DistribuicoesGenero.Clear();
                foreach (var item in generos)
                    DistribuicoesGenero.Add(item);

                // Carregar distribuição PCD
                var pcds = await _service.ObterDistribuicaoPCDAsync(AnoSelecionado);
                DistribuicoesPCD.Clear();
                foreach (var item in pcds)
                    DistribuicoesPCD.Add(item);

                // Carregar distribuição Raça/Etnia
                var racas = await _service.ObterDistribuicaoRacaEtniaAsync(AnoSelecionado);
                DistribuicoesRaca.Clear();
                foreach (var item in racas)
                    DistribuicoesRaca.Add(item);

                // Carregar distribuição Estado Civil
                var estadosCivis = await _service.ObterDistribuicaoEstadoCivilAsync(AnoSelecionado);
                DistribuicoesEstadoCivil.Clear();
                foreach (var item in estadosCivis)
                    DistribuicoesEstadoCivil.Add(item);

                Debug.WriteLine("[ViewModel] Dados carregados com sucesso!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewModel] Erro: {ex.Message}");
                // Você pode adicionar uma propriedade ErrorMessage para exibir na UI
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}