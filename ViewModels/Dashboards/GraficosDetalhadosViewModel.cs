using MauiApp1.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels.Dashboards
{
    public class GraficosDetalhadosViewModel : INotifyPropertyChanged
    {
        private readonly GraficosDetalhadosServices _service;

        // [NOVIDADE 1] Variáveis para guardar os números. 
        // Assim podemos redesenhar o gráfico ao mudar o tema sem consultar o banco de novo.
        private int _cachedHomens;
        private int _cachedMulheres;

        public event PropertyChangedEventHandler PropertyChanged;

        public GraficosDetalhadosViewModel(GraficosDetalhadosServices service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            CarregarOpcoesPicker();
            CarregarGraficoCommand = new Command(async () => await CarregarDadosAsync());
            ItemSelecionado = ListaUnidades.FirstOrDefault();

            // [NOVIDADE 2] Escutador de Tema
            // Sempre que o usuário mudar entre Claro/Escuro, isso roda automaticamente.
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                // Redesenha o gráfico na hora usando os dados guardados
                MainThread.BeginInvokeOnMainThread(() => MontarGrafico(_cachedHomens, _cachedMulheres));
            };
        }

        // --- PROPRIEDADES DE ESTADO ---
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand CarregarGraficoCommand { get; }

        private Chart? _graficoUnidade;
        public Chart? GraficoUnidade
        {
            get => _graficoUnidade;
            private set { _graficoUnidade = value; OnPropertyChanged(); }
        }

        // --- LISTA PARA O BLOCO VERDE ---
        public ObservableCollection<SituacaoDto> ListaSituacoes { get; } = new ObservableCollection<SituacaoDto>();

        // --- PICKER E SELEÇÃO ---
        public List<UnidadeItem> ListaUnidades { get; set; }

        private UnidadeItem _itemSelecionado;
        public UnidadeItem ItemSelecionado
        {
            get => _itemSelecionado;
            set
            {
                if (_itemSelecionado != value)
                {
                    _itemSelecionado = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        UnidadeSelecionada = value.ValorInterno;
                        _ = CarregarDadosAsync();
                    }
                }
            }
        }

        private string _unidadeSelecionada;
        public string UnidadeSelecionada
        {
            get => _unidadeSelecionada;
            set
            {
                if (_unidadeSelecionada != value)
                {
                    _unidadeSelecionada = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- LÓGICA DE CARREGAMENTO ---
        public async Task CarregarDadosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GraficoUnidade = null;
                    ListaSituacoes.Clear();
                });

                string unidadeParaBuscar = UnidadeSelecionada ?? "TODAS";

                var taskHomens = _service.GetTotalHomensAsync(unidadeParaBuscar);
                var taskMulheres = _service.GetTotalMulheresAsync(unidadeParaBuscar);
                var taskSituacoes = _service.GetResumoSituacoesAsync(unidadeParaBuscar);

                await Task.WhenAll(taskHomens, taskMulheres, taskSituacoes);

                int totalHomens = await taskHomens;
                int totalMulheres = await taskMulheres;
                var dadosSituacoes = await taskSituacoes;

                // [NOVIDADE 3] Salva os dados no cache para usar na troca de tema depois
                _cachedHomens = totalHomens;
                _cachedMulheres = totalMulheres;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ListaSituacoes.Clear();
                    if (dadosSituacoes != null)
                    {
                        foreach (var item in dadosSituacoes)
                        {
                            ListaSituacoes.Add(item);
                        }
                    }

                    MontarGrafico(totalHomens, totalMulheres);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Falha ao carregar dados: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void MontarGrafico(int homens, int mulheres)
        {
            // Essa verificação agora vai pegar o tema ATUALIZADO quando o evento disparar
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

            var corDoTexto = isDark ? SKColors.White : SKColors.Black;
            var corHomem = SKColor.Parse("#4285F4");
            var corMulher = SKColor.Parse("#F06292");

            var entries = new List<ChartEntry>
            {
                new ChartEntry(homens)
                {
                    Label = "Homens",
                    ValueLabel = homens.ToString(),
                    Color = corHomem,
                    TextColor = corDoTexto,
                    ValueLabelColor = corDoTexto
                },
                new ChartEntry(mulheres)
                {
                    Label = "Mulheres",
                    ValueLabel = mulheres.ToString(),
                    Color = corMulher,
                    TextColor = corDoTexto,
                    ValueLabelColor = corDoTexto
                }
            };

            GraficoUnidade = new BarChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 35,
                Margin = 15,
                AnimationDuration = TimeSpan.FromMilliseconds(500),
                ValueLabelOrientation = Orientation.Horizontal,
                LabelOrientation = Orientation.Horizontal,
                MinValue = 0
            };
        }

        private void CarregarOpcoesPicker()
        {
            ListaUnidades = new List<UnidadeItem>
            {
                new UnidadeItem { NomeExibicao = "TODAS", ValorInterno = "TODAS" },
                new UnidadeItem { NomeExibicao = "Cidade Universitária", ValorInterno = "UNIS" },
                new UnidadeItem { NomeExibicao = "FATEPS", ValorInterno = "FATEPS" },
                new UnidadeItem { NomeExibicao = "FABE", ValorInterno = "FABE" },
                new UnidadeItem { NomeExibicao = "FIC", ValorInterno = "FIC" },
                new UnidadeItem { NomeExibicao = "Três Corações (Nova Geração)", ValorInterno = "NOVAGERACAO" },
                new UnidadeItem { NomeExibicao = "FPA", ValorInterno = "FPA" },
                new UnidadeItem { NomeExibicao = "Portal Educação", ValorInterno = "PE" },
                new UnidadeItem { NomeExibicao = "Colégio CRA", ValorInterno = "CRA" },
                new UnidadeItem { NomeExibicao = "Faculdade Unis São Lourenço", ValorInterno = "SAOLOURENCO" },
                new UnidadeItem { NomeExibicao = "Educação Executiva", ValorInterno = "EDUEXE" },
                new UnidadeItem { NomeExibicao = "CMV - Varginha", ValorInterno = "CMV" }
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UnidadeItem
    {
        public string NomeExibicao { get; set; }
        public string ValorInterno { get; set; }
        public override string ToString() => NomeExibicao;
    }
}