using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using Microsoft.Maui.Graphics;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
namespace MauiApp1.ViewModels.Dashboards
{
    public partial class AlunosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly AlunosService _service;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private CancellationTokenSource _cts = new();
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // Construtor sem parâmetros (caso necessário)
        public AlunosViewModel() : this(CreateDefaultService())
        {
        }
        private static AlunosService CreateDefaultService()
        {
            var config = CreateDefaultConfiguration();
            var factory = new MySqlConnectionFactory(config);
            return new AlunosService(factory);
        }
        private static IConfiguration CreateDefaultConfiguration()
        {
            var inMemoryConfig = new Dictionary<string, string>
    {
        { "MySql:Corporem:Host", "cursoslivres.cl0yia62segf.sa-east-1.rds.amazonaws.com" },
        { "MySql:Corporem:Port", "3306" },
        { "MySql:Corporem:Database", "corporerm_heicomp" },        // ← ALTERE AQUI
        { "MySql:Corporem:User", "heicomp" },             // ← ALTERE AQUI
        { "MySql:Corporem:Password", "heicomp2025" },           // ← ALTERE AQUI
        { "MySql:Corporem:SslMode", "Preferred" },
        { "MySql:Corporem:AllowPublicKeyRetrieval", "true" }
    };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(inMemoryConfig!);
            return configBuilder.Build();
        }


        public AlunosViewModel(AlunosService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);
            _ = CarregarDadosIniciaisAsync();
        }

        public IRelayCommand LimparFiltrosCommand { get; }


        private int _totalAlunos;
        public int TotalAlunos { get => _totalAlunos; set => SetProperty(ref _totalAlunos, value); }

        private string _modalidadeMaisComum = "N/A";
        public string ModalidadeMaisComum { get => _modalidadeMaisComum; set => SetProperty(ref _modalidadeMaisComum, value); }

        private string _cursoComMaisAlunos = "N/A";
        public string CursoComMaisAlunos { get => _cursoComMaisAlunos; set => SetProperty(ref _cursoComMaisAlunos, value); }

        private int _numeroAlunosCursoTop;
        public int NumeroAlunosCursoTop { get => _numeroAlunosCursoTop; set => SetProperty(ref _numeroAlunosCursoTop, value); }

        private List<TurnoLegendaItem> _turnosLegenda = new();
        public List<TurnoLegendaItem> TurnosLegenda { get => _turnosLegenda; set => SetProperty(ref _turnosLegenda, value); }

        private List<ModalidadeLegendaItem> _modalidadesLegenda = new();

        public List<ModalidadeLegendaItem> ModalidadesLegenda { get => _modalidadesLegenda; set => SetProperty(ref _modalidadesLegenda, value); }

        private List<CampusLegendaItem> _campusLegenda = new();
        public List<CampusLegendaItem> CampusLegenda { get => _campusLegenda; set => SetProperty(ref _campusLegenda, value); }


        public ObservableCollection<string> ListaCampus { get; } = new();
        public ObservableCollection<ModalidadeItem> ListaModalidades { get; } = new();
        public ObservableCollection<CursoItem> ListaCursos { get; } = new();
        public ObservableCollection<TurnoItem> ListaTurnos { get; } = new();
        public ObservableCollection<PeriodoItem> ListaPeriodos { get; } = new();

        private string? _campusSelecionado;
        public string? CampusSelecionado
        {
            get => _campusSelecionado;
            set
            {
                if (SetProperty(ref _campusSelecionado, value))
                {
                    if (value != null && _campusDisplayToCode.TryGetValue(value, out string? codigoReal))
                    {
                        _campusCodigoReal = codigoReal;
                    }
                    else
                    {
                        _campusCodigoReal = value;
                    }

                    LimparFiltrosDependentes(1);
                    AtualizarVisibilidadeFiltros();
                    _ = AtualizarGraficosAsync();
                }
            }
        }

        private string? _campusCodigoReal;

        public string? CampusCodigoReal => _campusCodigoReal;

        private ModalidadeItem? _modalidadeSelecionada;
        public ModalidadeItem? ModalidadeSelecionada
        {
            get => _modalidadeSelecionada;
            set
            {
                if (SetProperty(ref _modalidadeSelecionada, value))
                {
                    LimparFiltrosDependentes(2);
                    AtualizarVisibilidadeFiltros();
                    _ = AtualizarGraficosAsync();
                }
            }
        }

        private CursoItem? _cursoSelecionado;
        public CursoItem? CursoSelecionado
        {
            get => _cursoSelecionado;
            set
            {
                if (SetProperty(ref _cursoSelecionado, value))
                {
                    LimparFiltrosDependentes(3);
                    AtualizarVisibilidadeFiltros();
                    _ = AtualizarGraficosAsync();
                }
            }
        }

        private TurnoItem? _turnoSelecionado;
        public TurnoItem? TurnoSelecionado
        {
            get => _turnoSelecionado;
            set
            {
                if (SetProperty(ref _turnoSelecionado, value))
                {
                    LimparFiltrosDependentes(4);
                    AtualizarVisibilidadeFiltros();
                    _ = AtualizarGraficosAsync();
                }
            }
        }

        private PeriodoItem? _periodoSelecionado;
        public PeriodoItem? PeriodoSelecionado
        {
            get => _periodoSelecionado;
            set
            {
                if (SetProperty(ref _periodoSelecionado, value))
                {
                    if (value != null)
                    {
                        _ = CarregarTurmasDoPeriodoAsync();
                    }
                    else
                    {
                        MostrarGraficoTurmas = false;
                        TurmasLegenda = new();
                        DadosGraficoTurmas = null;
                    }
                }
            }
        }

        private bool _mostrarModalidade;
        public bool MostrarModalidade { get => _mostrarModalidade; set => SetProperty(ref _mostrarModalidade, value); }

        private bool _mostrarCurso;
        public bool MostrarCurso { get => _mostrarCurso; set => SetProperty(ref _mostrarCurso, value); }

        private bool _mostrarTurno;
        public bool MostrarTurno { get => _mostrarTurno; set => SetProperty(ref _mostrarTurno, value); }

        private bool _mostrarPeriodo;
        public bool MostrarPeriodo { get => _mostrarPeriodo; set => SetProperty(ref _mostrarPeriodo, value); }

        private bool _mostrarGraficoCampus = true;
        public bool MostrarGraficoCampus { get => _mostrarGraficoCampus; set => SetProperty(ref _mostrarGraficoCampus, value); }

        private bool _mostrarGraficoModalidades = true;
        public bool MostrarGraficoModalidades { get => _mostrarGraficoModalidades; set => SetProperty(ref _mostrarGraficoModalidades, value); }

        private bool _mostrarGraficoCursos = true;
        public bool MostrarGraficoCursos { get => _mostrarGraficoCursos; set => SetProperty(ref _mostrarGraficoCursos, value); }

        private bool _mostrarGraficoTurnos = true;
        public bool MostrarGraficoTurnos { get => _mostrarGraficoTurnos; set => SetProperty(ref _mostrarGraficoTurnos, value); }

        private bool _mostrarGraficoPeriodos;
        public bool MostrarGraficoPeriodos { get => _mostrarGraficoPeriodos; set => SetProperty(ref _mostrarGraficoPeriodos, value); }

        private bool _mostrarBotaoCursos = true;
        public bool MostrarBotaoCursos { get => _mostrarBotaoCursos; set => SetProperty(ref _mostrarBotaoCursos, value); }


        private List<CursoLegendaItem> _top5CursosLegenda = new();
        public List<CursoLegendaItem> Top5CursosLegenda { get => _top5CursosLegenda; set => SetProperty(ref _top5CursosLegenda, value); }

        private List<PeriodoLegendaItem> _periodosLegenda = new();
        public List<PeriodoLegendaItem> PeriodosLegenda
        {
            get => _periodosLegenda; set => SetProperty(ref _periodosLegenda, value);
        }
        private List<TurmaLegendaItem> _turmasLegenda = new();
        public List<TurmaLegendaItem> TurmasLegenda { get => _turmasLegenda; set => SetProperty(ref _turmasLegenda, value); }

        private Dictionary<string, int>? _dadosGraficoTurmas;
        public Dictionary<string, int>? DadosGraficoTurmas { get => _dadosGraficoTurmas; set => SetProperty(ref _dadosGraficoTurmas, value); }

        private readonly Dictionary<string, string> _campusDisplayToCode = new();

        private bool _mostrarGraficoTurmas;
        public bool MostrarGraficoTurmas { get => _mostrarGraficoTurmas; set => SetProperty(ref _mostrarGraficoTurmas, value); }

        private Dictionary<string, int>? _dadosGraficoCampus;
        public Dictionary<string, int>? DadosGraficoCampus { get => _dadosGraficoCampus; set => SetProperty(ref _dadosGraficoCampus, value); }

        private Dictionary<string, int>? _dadosGraficoModalidades;
        public Dictionary<string, int>? DadosGraficoModalidades { get => _dadosGraficoModalidades; set => SetProperty(ref _dadosGraficoModalidades, value); }

        private Dictionary<string, int>? _dadosGraficoCursos;
        public Dictionary<string, int>? DadosGraficoCursos { get => _dadosGraficoCursos; set => SetProperty(ref _dadosGraficoCursos, value); }

        private Dictionary<string, int>? _dadosGraficoTurnos;
        public Dictionary<string, int>? DadosGraficoTurnos { get => _dadosGraficoTurnos; set => SetProperty(ref _dadosGraficoTurnos, value); }

        private Dictionary<string, int>? _dadosGraficoPeriodos;
        public Dictionary<string, int>? DadosGraficoPeriodos { get => _dadosGraficoPeriodos; set => SetProperty(ref _dadosGraficoPeriodos, value); }

        private readonly string[] _coresHex = {
            "#3498db", "#e74c3c", "#2ecc71", "#f1c40f", "#9b59b6",
            "#1abc9c", "#e67e22", "#34495e", "#8e44ad", "#c0392b"
        };

        private async Task CarregarDadosIniciaisAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                IsBusy = true;

                var t1 = _service.GetTotalAlunosAsync();
                var t2 = _service.GetModalidadeMaisAlunosAsync();
                var t3 = _service.GetCursoComMaisAlunosAsync();
                var t4 = _service.GetAlunosPorCampusAsync();
                var t5 = _service.GetDistribuicaoModalidadeGeralAsync();
                var t6 = _service.GetDistribuicaoTurnoGeralAsync();
                var t7 = _service.GetCursosTopAsync(limit: 5);
                var t8 = _service.GetCampusDisponiveisAsync();

                await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);

                TotalAlunos = await t1;
                ModalidadeMaisComum = (await t2).Modalidade;
                CursoComMaisAlunos = (await t3).Curso;
                NumeroAlunosCursoTop = (await t3).Quantidade;

                var campusData = await t4;
                var modalidadeData = await t5;
                var turnoData = await t6;
                var topCursos = await t7;
                var listaCampusCompleta = await t8;


                ListaCampus.Clear();
                _campusDisplayToCode.Clear();

                foreach (var itemCompleto in listaCampusCompleta)
                {
                    var partes = itemCompleto.Split(new[] { " : " }, 2, StringSplitOptions.None);
                    if (partes.Length == 2)
                    {
                        string codigoReal = partes[0].Trim();
                        string descricao = partes[1].Trim();

                        string textoExibicao = $"{codigoReal} : {descricao}";

                        ListaCampus.Add(textoExibicao);
                        _campusDisplayToCode[textoExibicao] = codigoReal;
                    }
                    else
                    {
                        ListaCampus.Add(itemCompleto);
                        _campusDisplayToCode[itemCompleto] = itemCompleto;
                    }
                }

                CriarGraficoCampus(campusData);
                CriarGraficoModalidades(modalidadeData.Select(kvp => (kvp.Key, kvp.Value)).ToList());
                CriarGraficoTurnos(turnoData.Select(kvp => (kvp.Key, kvp.Value)).ToList());
                CriarGraficoTopCursos(topCursos);

                MostrarGraficoCampus = true;
                MostrarGraficoModalidades = true;
                MostrarGraficoCursos = true;
                MostrarGraficoTurnos = true;
                MostrarGraficoPeriodos = false;
                MostrarBotaoCursos = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados iniciais: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                _semaphore.Release();
            }
        }

        private async Task AtualizarGraficosAsync()
        {
            await _semaphore.WaitAsync();
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

            try
            {
                IsBusy = true;

                if (CampusSelecionado == null && ModalidadeSelecionada == null && CursoSelecionado == null && TurnoSelecionado == null)
                    return;

                TotalAlunos = await _service.GetTotalAlunosAsync(CampusCodigoReal, ModalidadeSelecionada?.Codigo, CursoSelecionado?.Codigo, TurnoSelecionado?.Codigo);
                ModalidadeMaisComum = (await _service.GetModalidadeMaisAlunosAsync(CampusCodigoReal, CursoSelecionado?.Codigo, TurnoSelecionado?.Codigo)).Modalidade;
                var cursoTop = await _service.GetCursoComMaisAlunosAsync(CampusCodigoReal, ModalidadeSelecionada?.Codigo, TurnoSelecionado?.Codigo);
                CursoComMaisAlunos = cursoTop.Curso;
                NumeroAlunosCursoTop = cursoTop.Quantidade;

                if (!string.IsNullOrEmpty(CampusSelecionado) && ModalidadeSelecionada == null)
                {
                    ListaModalidades.Clear();
                    var mods = await _service.GetModalidadesDisponiveisAsync(CampusCodigoReal);
                    foreach (var m in mods) ListaModalidades.Add(new ModalidadeItem { Codigo = m.Codigo, Nome = m.Nome });

                    var dist = await _service.GetDistribuicaoModalidadeAsync(CampusCodigoReal);
                    CriarGraficoModalidades(dist);

                    var topCursos = await _service.GetCursosTopAsync(CampusCodigoReal, limit: 5);
                    CriarGraficoTopCursos(topCursos);

                    MostrarGraficoCampus = false;
                    MostrarGraficoModalidades = true;
                    MostrarGraficoCursos = true;
                    MostrarGraficoTurnos = false;
                    MostrarGraficoPeriodos = false;
                }
                else if (ModalidadeSelecionada != null && CursoSelecionado == null)
                {
                    ListaCursos.Clear();
                    var cursos = await _service.GetCursosDisponiveisAsync(CampusCodigoReal, ModalidadeSelecionada.Codigo);
                    foreach (var c in cursos) ListaCursos.Add(new CursoItem { Codigo = c.Codigo, Nome = c.Nome });

                    var topCursos = await _service.GetCursosTopAsync(CampusCodigoReal, ModalidadeSelecionada.Codigo, limit: 5);
                    CriarGraficoTopCursos(topCursos);

                    MostrarGraficoModalidades = false;
                    MostrarGraficoCursos = true;
                    MostrarGraficoTurnos = false;
                    MostrarGraficoPeriodos = false;
                    MostrarBotaoCursos = true;
                }
                else if (CursoSelecionado != null && TurnoSelecionado == null)
                {
                    ListaTurnos.Clear();
                    var turnos = await _service.GetTurnosDisponiveisAsync(CampusCodigoReal, ModalidadeSelecionada!.Codigo, CursoSelecionado.Codigo);
                    foreach (var t in turnos) ListaTurnos.Add(new TurnoItem { Codigo = t.Codigo, Nome = t.Nome });

                    var dist = await _service.GetDistribuicaoTurnoAsync(CampusCodigoReal, ModalidadeSelecionada!.Codigo, CursoSelecionado.Codigo);
                    CriarGraficoTurnos(dist);

                    MostrarGraficoCursos = false;
                    MostrarGraficoTurnos = true;
                    MostrarGraficoPeriodos = false;
                }
                else if (TurnoSelecionado != null)
                {

                    ListaPeriodos.Clear();
                    var periodos = await _service.GetPeriodosDisponiveisAsync(CampusCodigoReal, ModalidadeSelecionada!.Codigo, CursoSelecionado!.Codigo, TurnoSelecionado.Codigo);
                    foreach (var p in periodos) ListaPeriodos.Add(new PeriodoItem { Codigo = p.Codigo, Descricao = p.Descricao });


                    var dist = await _service.GetAlunosPorPeriodoLetivoAsync(CampusCodigoReal, ModalidadeSelecionada!.Codigo, CursoSelecionado!.Codigo, TurnoSelecionado.Codigo);

                    if (dist.Any())
                    {
                        CriarGraficoPeriodos(dist);

                        var coresPeriodo = new[] {
                    Color.FromArgb("#1E3A8A"), Color.FromArgb("#3B82F6"), Color.FromArgb("#60A5FA"),
                    Color.FromArgb("#93C5FD"), Color.FromArgb("#7FB8FD"), Color.FromArgb("#DBEAFE"),
                    Color.FromArgb("#FCA5A5"), Color.FromArgb("#F87171")
                };

                        PeriodosLegenda = dist
                            .Select((x, i) => new PeriodoLegendaItem
                            {
                                Nome = x.PeriodoLetivo,
                                Quantidade = x.Quantidade,
                                Cor = coresPeriodo[i % coresPeriodo.Length]
                            })
                            .OrderByDescending(x => x.Quantidade)
                            .ToList();

                        MostrarGraficoTurnos = false;
                        MostrarGraficoPeriodos = true;
                        MostrarBotaoCursos = true;

                    }
                    else
                    {
                        MostrarGraficoPeriodos = false;
                    }
                }
            }
            finally
            {
                IsBusy = false;
                _semaphore.Release();
            }
        }

        public void LimparFiltros()
        {
            CampusSelecionado = null;
            ModalidadeSelecionada = null;
            CursoSelecionado = null;
            TurnoSelecionado = null;
            PeriodoSelecionado = null;

            ListaModalidades.Clear();
            ListaCursos.Clear();
            ListaTurnos.Clear();
            ListaPeriodos.Clear();

            MostrarGraficoPeriodos = false;

            AtualizarVisibilidadeFiltros();
            _ = CarregarDadosIniciaisAsync();
        }

        private void LimparFiltrosDependentes(int nivel)
        {
            if (nivel <= 1) { ModalidadeSelecionada = null; ListaModalidades.Clear(); }
            if (nivel <= 2) { CursoSelecionado = null; ListaCursos.Clear(); }
            if (nivel <= 3) { TurnoSelecionado = null; ListaTurnos.Clear(); }
            if (nivel <= 4) { PeriodoSelecionado = null; ListaPeriodos.Clear(); }
        }

        private void AtualizarVisibilidadeFiltros()
        {
            MostrarModalidade = !string.IsNullOrEmpty(CampusSelecionado);
            MostrarCurso = ModalidadeSelecionada != null;
            MostrarTurno = CursoSelecionado != null;
            MostrarPeriodo = TurnoSelecionado != null;
            MostrarBotaoCursos = string.IsNullOrEmpty(CampusSelecionado) || ModalidadeSelecionada != null;
        }

        private void CriarGraficoCampus(List<(string Campus, int Quantidade)> dados)
        {
            DadosGraficoCampus = dados.ToDictionary(x => x.Campus, x => x.Quantidade);

            var cores = new Color[] {
        Color.FromArgb("#1E3A8A"),
        Color.FromArgb("#3B82F6"),
        Color.FromArgb("#60A5FA"),
        Color.FromArgb("#93C5FD"),
        Color.FromArgb("#7FB8FD")
    };

            CampusLegenda = dados
                .Select((x, i) => new CampusLegendaItem
                {
                    Nome = x.Campus,
                    Quantidade = x.Quantidade,
                    Cor = cores[i % cores.Length]
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList();
        }

        private void CriarGraficoModalidades(List<(string Modalidade, int Quantidade)> dados)
        {
            DadosGraficoModalidades = dados.ToDictionary(x => x.Modalidade, x => x.Quantidade);

            var cores = new[] {
        Color.FromArgb("#1E3A8A"),
        Color.FromArgb("#3B82F6"),
        Color.FromArgb("#60A5FA")
    };

            ModalidadesLegenda = dados
                .Select((x, i) => new ModalidadeLegendaItem
                {
                    Nome = x.Modalidade,
                    Quantidade = x.Quantidade,
                    Cor = cores[i % cores.Length]
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList();
        }

        private void CriarGraficoTurnos(List<(string Turno, int Quantidade)> dados)
        {
            DadosGraficoTurnos = dados.ToDictionary(x => x.Turno, x => x.Quantidade);
            var cores = new[] { "#1E3A8A", "#3B82F6", "#60A5FA", "#93C5FD", "#7FB8FD", "#DBEAFE" };
            TurnosLegenda = dados.Select((x, i) => new TurnoLegendaItem
            {
                Nome = x.Turno,
                Quantidade = x.Quantidade,
                Cor = Color.FromArgb(cores[i % cores.Length])
            }).OrderByDescending(x => x.Quantidade).ToList();
        }

        private void CriarGraficoTopCursos(List<(string Curso, int Quantidade)> dados)
        {
            DadosGraficoCursos = dados.Take(5).ToDictionary(x => x.Curso, x => x.Quantidade);
            var cores = new[] {
        "#0A4986", "#0C559D", "#0E62B6", "#106FCE", "#117CE6"
    };
            Top5CursosLegenda = dados.Take(5).Select((x, i) => new CursoLegendaItem
            {
                Nome = x.Curso,
                Quantidade = x.Quantidade,
                Cor = Color.FromArgb(cores[i % cores.Length])
            }).ToList();
        }

        private void CriarGraficoPeriodos(List<(string PeriodoLetivo, int Quantidade)> dados) =>
            DadosGraficoPeriodos = dados.ToDictionary(x => x.PeriodoLetivo, x => x.Quantidade);

        private async Task CarregarTurmasDoPeriodoAsync()
        {
            if (PeriodoSelecionado == null || CampusSelecionado == null ||
                CursoSelecionado == null || TurnoSelecionado == null || ModalidadeSelecionada == null)
                return;

            await _semaphore.WaitAsync();
            try
            {
                IsBusy = true;

                var dados = await _service.GetTurmasPorPeriodoAsync(
                    CampusCodigoReal!,
                    ModalidadeSelecionada.Codigo,
                    CursoSelecionado.Codigo,
                    TurnoSelecionado.Codigo,
                    PeriodoSelecionado.Codigo);

                if (dados.Any())
                {
                    var cores = new Color[] {
                        Color.FromArgb("#1E3A8A"), Color.FromArgb("#3B82F6"), Color.FromArgb("#60A5FA"),
                        Color.FromArgb("#93C5FD"), Color.FromArgb("#7FB8FD"), Color.FromArgb("#FCA5A5"),
                        Color.FromArgb("#F87171"), Color.FromArgb("#F472B6"), Color.FromArgb("#EC4899"),
                        Color.FromArgb("#8B5CF6"), Color.FromArgb("#6366F1")
                    };

                    TurmasLegenda = dados
                        .Select((x, i) => new TurmaLegendaItem
                        {
                            Nome = x.Turma,
                            Quantidade = x.Quantidade,
                            Cor = cores[i % cores.Length],
                            VerAlunosCommand = new AsyncRelayCommand<string>(turma => VerAlunosDaTurmaAsync(turma ?? x.Turma))
                        })
                        .OrderByDescending(x => x.Quantidade)
                        .ToList();

                    DadosGraficoTurmas = dados.ToDictionary(x => x.Turma, x => x.Quantidade);

                    MostrarGraficoTurmas = true;
                    MostrarGraficoPeriodos = false;
                }
                else
                {
                    MostrarGraficoTurmas = false;
                    TurmasLegenda = new List<TurmaLegendaItem>();
                    DadosGraficoTurmas = null;
                }
            }
            finally
            {
                IsBusy = false;
                _semaphore.Release();
            }
        }

        private async Task VerAlunosDaTurmaAsync(string codTurma)
        {
            if (string.IsNullOrWhiteSpace(codTurma)) return;

            try
            {
                IsBusy = true;

                var alunos = await _service.GetAlunosDaTurmaAsync(
                    campus: CampusCodigoReal!,
                    modalidade: ModalidadeSelecionada!.Codigo,
                    curso: CursoSelecionado!.Codigo,
                    turno: TurnoSelecionado!.Codigo,
                    periodo: PeriodoSelecionado!.Codigo,
                    turma: codTurma);

                if (!alunos.Any())
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        $"Turma {codTurma}", "Nenhum aluno encontrado.", "OK");
                    return;
                }

                var mensagem = string.Join("\n", alunos.Select(a => $"{a.RA} - {a.Nome}"));

                await Application.Current!.MainPage!.DisplayAlert(
                    $"Alunos da Turma {codTurma} ({alunos.Count})",
                    mensagem,
                    "Fechar");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand AbrirListaCursosCommand => new AsyncRelayCommand(AbrirListaCursosAsync);

        private async Task AbrirListaCursosAsync()
        {
            if (string.IsNullOrEmpty(CampusSelecionado)) return;

            var cursos = await _service.GetCursosTopAsync(CampusCodigoReal, ModalidadeSelecionada?.Codigo, limit: null);

            if (!cursos.Any())
            {
                await Application.Current!.MainPage!.DisplayAlert("Cursos", "Nenhum curso encontrado.", "OK");
                return;
            }

            var msg = string.Join("\n", cursos.Select((c, i) => $"{i + 1}. {c.Curso}: {c.Quantidade} aluno{(c.Quantidade == 1 ? "" : "s")}"));
            await Application.Current!.MainPage!.DisplayAlert($"Todos os cursos - {CampusSelecionado}", msg, "Fechar");
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ModalidadeItem { public string Codigo { get; set; } = ""; public string Nome { get; set; } = ""; }
    public class ModalidadeLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; }
    public class CursoItem { public string Codigo { get; set; } = ""; public string Nome { get; set; } = ""; }
    public class TurnoItem { public int Codigo { get; set; } public string Nome { get; set; } = ""; }
    public class PeriodoItem { public string Codigo { get; set; } = ""; public string Descricao { get; set; } = ""; }
    public class CampusLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; }
    public class CursoLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; }
    public class PeriodoLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; }
    public class TurnoLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; }
    public class TurmaLegendaItem { public string Nome { get; set; } = ""; public int Quantidade { get; set; } public Color Cor { get; set; } = Colors.Transparent; public ICommand VerAlunosCommand { get; set; } = null!; }

}