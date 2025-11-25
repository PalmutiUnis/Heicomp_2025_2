using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using MauiApp1.Models;
using MauiApp1.Services;
using Microcharts;
using SkiaSharp;

namespace MauiApp1.ViewModels.Dashboards
{
    public class DiversidadeViewModel : INotifyPropertyChanged
    {
        private readonly DiversidadeService _service;

        // Lista de Anos para o Picker
        public ObservableCollection<string> AnosDisponiveis { get; set; } = new ObservableCollection<string>();

        // Propriedade vinculada ao Picker (String)
        private string _anoSelecionadoString;
        public string AnoSelecionadoString
        {
            get => _anoSelecionadoString;
            set
            {
                if (_anoSelecionadoString != value)
                {
                    _anoSelecionadoString = value;
                    OnPropertyChanged();
                    if (int.TryParse(value, out int ano))
                    {
                        AnoSelecionado = ano;
                    }
                }
            }
        }

        // Ano atual (inteiro) que dispara o carregamento
        private int _anoSelecionado;
        public int AnoSelecionado
        {
            get => _anoSelecionado;
            set
            {
                if (_anoSelecionado != value)
                {
                    _anoSelecionado = value;
                    OnPropertyChanged();
                    MainThread.BeginInvokeOnMainThread(async () => await CarregarDadosAsync());
                }
            }
        }

        // Dados Gerais (Cards e Gráfico de Rosca)
        private DiversidadeGeral _dadosGerais;
        public DiversidadeGeral DadosGerais
        {
            get => _dadosGerais;
            set { _dadosGerais = value; OnPropertyChanged(); }
        }

        // O Gráfico de Rosca (Donut)
        private Chart _chartGenero;
        public Chart ChartGenero
        {
            get => _chartGenero;
            set { _chartGenero = value; OnPropertyChanged(); }
        }

        // Coleções para os gráficos de barra
        public ObservableCollection<DistribuicaoGenero> DistribuicoesGenero { get; set; } = new(); // Mantido caso use em outro lugar
        public ObservableCollection<DistribuicaoRacaEtnia> DistribuicoesRaca { get; set; } = new();
        public ObservableCollection<DistribuicaoPCD> DistribuicoesPCD { get; set; } = new();
        public ObservableCollection<DistribuicaoEstadoCivil> DistribuicoesEstadoCivil { get; set; } = new(); // Mantido

        // Propriedades de Estado
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public DiversidadeViewModel(DiversidadeService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            // 1. Preenche o Picker com anos
            AnosDisponiveis.Clear();
            for (int i = 2025; i >= 2019; i--) AnosDisponiveis.Add(i.ToString());

            // 2. Define o ano inicial
            _anoSelecionadoString = "2024";
            _anoSelecionado = 2024;

            DadosGerais = new DiversidadeGeral();

            MainThread.BeginInvokeOnMainThread(async () => await CarregarDadosAsync());
        }

        public async Task CarregarDadosAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // Busca todos os dados do ano selecionado
                var dadosGerais = await _service.ObterDadosGeraisAsync(AnoSelecionado);
                var generos = await _service.ObterDistribuicaoGeneroAsync(AnoSelecionado); // Mantido
                var racas = await _service.ObterDistribuicaoRacaEtniaAsync(AnoSelecionado);
                var pcds = await _service.ObterDistribuicaoPCDAsync(AnoSelecionado);
                var civis = await _service.ObterDistribuicaoEstadoCivilAsync(AnoSelecionado); // Mantido

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DadosGerais = dadosGerais ?? new DiversidadeGeral();

                    // Atualiza listas
                    DistribuicoesRaca.Clear();
                    if (racas != null) foreach (var item in racas) DistribuicoesRaca.Add(item);

                    DistribuicoesPCD.Clear();
                    if (pcds != null) foreach (var item in pcds) DistribuicoesPCD.Add(item);

                    // Listas extras mantidas do seu código original
                    DistribuicoesGenero.Clear();
                    if (generos != null) foreach (var item in generos) DistribuicoesGenero.Add(item);

                    DistribuicoesEstadoCivil.Clear();
                    if (civis != null) foreach (var item in civis) DistribuicoesEstadoCivil.Add(item);

                    // 3. Recria o gráfico de rosca com a correção
                    CriarGraficoGenero();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() => ErrorMessage = ex.Message);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        private void CriarGraficoGenero()
        {
            if (DadosGerais == null) return;

            var entries = new List<ChartEntry>();

            // Verifica se a soma dos percentuais é maior que zero (para não mostrar cinza vazio)
            var somaTotal = DadosGerais.PercentualHomens + DadosGerais.PercentualMulheres + DadosGerais.PercentualNaoInformado;
            bool temDados = somaTotal > 0.1m; // OBS: O 'm' aqui resolve o erro CS0019

            if (temDados)
            {
                // -- Homens --
                if (DadosGerais.PercentualHomens > 0)
                {
                    entries.Add(new ChartEntry((float)DadosGerais.PercentualHomens)
                    {
                        // TRUQUE: Colocar o texto aqui faz a linha lateral aparecer (Igual à foto)
                        Label = $"Homens: {DadosGerais.PercentualHomens:F1}%",
                        ValueLabel = "",
                        Color = SKColor.Parse("#3B82F6"), // Azul
                        TextColor = SKColor.Parse("#374151")
                    });
                }

                // -- Mulheres --
                if (DadosGerais.PercentualMulheres > 0)
                {
                    entries.Add(new ChartEntry((float)DadosGerais.PercentualMulheres)
                    {
                        Label = $"Mulheres: {DadosGerais.PercentualMulheres:F1}%",
                        ValueLabel = "",
                        Color = SKColor.Parse("#EC4899"), // Rosa
                        TextColor = SKColor.Parse("#374151")
                    });
                }

                // -- Não Informado --
                // Adiciona 'm' para corrigir erro de comparação decimal vs double
                if (DadosGerais.PercentualNaoInformado > 0.1m)
                {
                    entries.Add(new ChartEntry((float)DadosGerais.PercentualNaoInformado)
                    {
                        Label = $"N/I: {DadosGerais.PercentualNaoInformado:F1}%",
                        ValueLabel = "",
                        Color = SKColor.Parse("#9CA3AF"), // Cinza
                        TextColor = SKColor.Parse("#374151")
                    });
                }
            }
            else
            {
                // Placeholder transparente se não houver dados (evita o gráfico cinza gigante)
                entries.Add(new ChartEntry(1) { Color = SKColors.Transparent, Label = "" });
            }

            ChartGenero = new DonutChart
            {
                Entries = entries,
                LabelTextSize = 30,       // Tamanho da fonte
                HoleRadius = 0.55f,       // Tamanho do buraco
                BackgroundColor = SKColors.Transparent,
                Margin = 30,              // Margem para as legendas externas não cortarem
                GraphPosition = GraphPosition.Center
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}