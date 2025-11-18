using Heicomp_2025_2.ViewModels.Dashboards;
using Microcharts; // Biblioteca que cria os gráficos
using SkiaSharp; // Biblioteca de desenho 2D (cores, formas, etc)
using SkiaSharp.Views.Maui; // Integração do SkiaSharp com MAUI
using SkiaSharp.Views.Maui.Controls; // Controles visuais do SkiaSharp para MAUI

namespace Heicomp_2025_2.Views.Dashboards;



/// <summary>
/// Página de Dashboard de Alunos com sistema de Drill-Down em 5 níveis
/// Campus ? Modalidade ? Curso ? Turno ? Período
/// </summary>
public partial class AlunosPage : ContentPage
{
    // ViewModel que contém toda a lógica de dados e filtros
    private readonly AlunosViewModel vm;

    // Gráficos que serão desenhados nos canvas
    private Chart? chartCampus;
    private Chart? chartModalidades;
    private Chart? chartCursos;
    private Chart? chartTurnos;
    private Chart? chartPeriodos;

    // ========================================
    // CONSTRUTOR
    // ========================================

    public AlunosPage()
    {
        InitializeComponent();

        // Inicializa o ViewModel
        vm = new AlunosViewModel();
        BindingContext = vm;

        // Escuta mudanças nas propriedades do ViewModel
        // Quando os dados dos gráficos mudarem, redesenha os gráficos
        vm.PropertyChanged += (s, e) =>
        {
            // Atualiza o gráfico correspondente quando seus dados mudam
            switch (e.PropertyName)
            {
                case nameof(vm.DadosGraficoCampus):
                    AtualizarGraficoCampus();
                    GraficoCampus?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoModalidades):
                    AtualizarGraficoModalidades();
                    GraficoModalidades?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoCursos):
                    AtualizarGraficoCursos();
                    GraficoCursos?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoTurnos):
                    AtualizarGraficoTurnos();
                    GraficoTurnos?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoPeriodos):
                    AtualizarGraficoPeriodos();
                    GraficoPeriodos?.InvalidateSurface();
                    break;
            }
        };

        // Desenha todos os gráficos iniciais
        AtualizarTodosGraficos();
    }

    // ========================================
    // ATUALIZAÇÃO DOS GRÁFICOS
    // ========================================

    /// <summary>
    /// Atualiza todos os gráficos de uma vez (chamado na inicialização)
    /// </summary>
    private void AtualizarTodosGraficos()
    {
        AtualizarGraficoCampus();
        AtualizarGraficoModalidades();
        AtualizarGraficoCursos();
        AtualizarGraficoTurnos();
        AtualizarGraficoPeriodos();
    }

    // ========================================
    // GRÁFICO 1: TOTAL POR CAMPUS (BARRAS HORIZONTAIS)
    // ========================================

    /// <summary>
    /// Prepara o gráfico de barras horizontais para total de alunos por campus
    /// </summary>
    private void AtualizarGraficoCampus()
    {
        if (vm.DadosGraficoCampus == null) return;

        // Converte os dados para o formato que o Microcharts entende
        var entries = vm.DadosGraficoCampus.Select(x =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,                          // Nome do campus
                ValueLabel = x.Value.ToString("N0"),    // Número formatado (ex: 5.432)
                Color = SKColor.Parse("#1E3A8A"),       // Azul escuro (cor principal)
                ValueLabelColor = SKColor.Parse("#1E3A8A")
            }).ToList();

        // Cria o gráfico de barras
        chartCampus = new BarChart
        {
            Entries = entries,
            LabelTextSize = 32,                              // Tamanho do texto dos labels
            ValueLabelOrientation = Orientation.Horizontal,   // Números na horizontal
            LabelOrientation = Orientation.Horizontal,        // Labels na horizontal
            BackgroundColor = SKColors.Transparent,           // Fundo transparente
            Margin = 20,                                      // Margem interna
            MinValue = 0                                      // Começa do zero
        };
    }

    /// <summary>
    /// Evento chamado quando o canvas precisa ser desenhado/redesenhado
    /// </summary>
    private void OnGraficoCampusPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(); // Limpa o canvas

        // Desenha o gráfico no canvas
        chartCampus?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    // ========================================
    // GRÁFICO 2: TOTAL POR MODALIDADE (ROSCA)
    // ========================================

    /// <summary>
    /// Prepara o gráfico de rosca para distribuição por modalidade
    /// </summary>
    private void AtualizarGraficoModalidades()
    {
        if (vm.DadosGraficoModalidades == null) return;

        // Cores diferentes para cada modalidade
        var cores = new[]
        {
            SKColor.Parse("#1E3A8A"), // Azul escuro - Presencial
            SKColor.Parse("#3B82F6"), // Azul médio - EAD
            SKColor.Parse("#60A5FA")  // Azul claro - Híbrido
        };

        var entries = vm.DadosGraficoModalidades.Select((x, index) =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,
                ValueLabel = x.Value.ToString("N0"),
                Color = cores[index % cores.Length],      // Usa as cores do array
                ValueLabelColor = cores[index % cores.Length]
            }).ToList();

        // Cria o gráfico de rosca (donut)
        chartModalidades = new DonutChart
        {
            Entries = entries,
            LabelTextSize = 32,
            BackgroundColor = SKColors.Transparent,
            HoleRadius = 0.5f,  // Tamanho do buraco no meio (0 a 1)
            GraphPosition = GraphPosition.Center
        };
    }

    private void OnGraficoModalidadesPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        chartModalidades?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    // ========================================
    // GRÁFICO 3: TOTAL POR CURSO (BARRAS VERTICAIS)
    // ========================================

    /// <summary>
    /// Prepara o gráfico de barras verticais para total de alunos por curso
    /// </summary>
    private void AtualizarGraficoCursos()
    {
        if (vm.DadosGraficoCursos == null) return;

        // Gradiente de azul (do mais escuro ao mais claro)
        var cores = new[]
        {
            SKColor.Parse("#1E3A8A"),
            SKColor.Parse("#2563EB"),
            SKColor.Parse("#3B82F6"),
            SKColor.Parse("#60A5FA"),
            SKColor.Parse("#93C5FD"),
            SKColor.Parse("#BFDBFE")
        };

        var entries = vm.DadosGraficoCursos.Select((x, index) =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,
                ValueLabel = x.Value.ToString("N0"),
                Color = cores[index % cores.Length],
                ValueLabelColor = cores[index % cores.Length]
            }).ToList();

        chartCursos = new BarChart
        {
            Entries = entries,
            LabelTextSize = 30,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelOrientation = Orientation.Horizontal,
            BackgroundColor = SKColors.Transparent,
            Margin = 20,
            MinValue = 0
        };
    }

    private void OnGraficoCursosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        chartCursos?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    // ========================================
    // GRÁFICO 4: QUANTIDADE POR TURNO (ROSCA)
    // ========================================

    /// <summary>
    /// Prepara o gráfico de rosca para distribuição por turno
    /// </summary>
    private void AtualizarGraficoTurnos()
    {
        if (vm.DadosGraficoTurnos == null) return;

        // Cores para cada turno
        var cores = new[]
        {
            SKColor.Parse("#F59E0B"), // Laranja - Manhã (sol da manhã)
            SKColor.Parse("#3B82F6"), // Azul - Tarde (céu da tarde)
            SKColor.Parse("#1E3A8A")  // Azul escuro - Noite (escuridão)
        };

        var entries = vm.DadosGraficoTurnos.Select((x, index) =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,
                ValueLabel = x.Value.ToString("N0"),
                Color = cores[index % cores.Length],
                ValueLabelColor = cores[index % cores.Length]
            }).ToList();

        chartTurnos = new DonutChart
        {
            Entries = entries,
            LabelTextSize = 32,
            BackgroundColor = SKColors.Transparent,
            HoleRadius = 0.5f,
            GraphPosition = GraphPosition.Center
        };
    }

    private void OnGraficoTurnosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        chartTurnos?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    // ========================================
    // GRÁFICO 5: DISTRIBUIÇÃO POR PERÍODO (BARRAS VERTICAIS)
    // ========================================

    /// <summary>
    /// Prepara o gráfico de barras verticais para distribuição por período
    /// </summary>
    private void AtualizarGraficoPeriodos()
    {
        if (vm.DadosGraficoPeriodos == null) return;

        // Todos os períodos com a mesma cor (azul principal)
        var entries = vm.DadosGraficoPeriodos.Select(x =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,
                ValueLabel = x.Value.ToString("N0"),
                Color = SKColor.Parse("#1E3A8A"),
                ValueLabelColor = SKColor.Parse("#1E3A8A")
            }).ToList();

        chartPeriodos = new BarChart
        {
            Entries = entries,
            LabelTextSize = 30,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelOrientation = Orientation.Horizontal,
            BackgroundColor = SKColors.Transparent,
            Margin = 20,
            MinValue = 0
        };
    }

    private void OnGraficoPeriodosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        chartPeriodos?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    // 
    // EVENTOS DE BOTÕES
    // 

    /// <summary>
    /// Botão "Limpar Filtros" - Reseta todos os filtros e volta para a visão geral
    /// </summary>
    private void OnLimparFiltrosClicked(object sender, EventArgs e)
    {
        vm.LimparFiltros();

        // Redesenha todos os gráficos
        AtualizarTodosGraficos();
        GraficoCampus?.InvalidateSurface();
        GraficoModalidades?.InvalidateSurface();
        GraficoCursos?.InvalidateSurface();
        GraficoTurnos?.InvalidateSurface();
        GraficoPeriodos?.InvalidateSurface();
    }

    /// <summary>
    /// Botão "Voltar" - Retorna para o Painel de Gestão
    /// </summary>
    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}