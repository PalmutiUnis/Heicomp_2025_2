using Heicomp_2025_2.ViewModels.Dashboards;
using Microcharts; // Biblioteca que cria os gráficos
using SkiaSharp; // Biblioteca de desenho 2D (cores, formas, etc)
using SkiaSharp.Views.Maui; // Integração do SkiaSharp com MAUI
using SkiaSharp.Views.Maui.Controls; // Controles visuais do SkiaSharp para MAUI

namespace Heicomp_2025_2.Views.Dashboards;

public partial class AlunosPage : ContentPage
{
    private readonly AlunosViewModel vm;
    private Chart? currentChart;

    public AlunosPage()
    {
        InitializeComponent();

        vm = new AlunosViewModel();
        BindingContext = vm;

        // Escuta mudanças nos dados do gráfico
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.DadosGrafico))
            {
                AtualizarGrafico();
                GraficoAlunos.InvalidateSurface();
            }
        };

        AtualizarGrafico();
    }

    private void AtualizarGrafico()
    {
        if (vm.DadosGrafico == null) return;

        var entries = vm.DadosGrafico.Select(x =>
            new ChartEntry((float)x.Value)
            {
                Label = x.Key,
                ValueLabel = x.Value.ToString("N0"),
                Color = SKColor.Parse("#4A69BD"), // Azul do Figma
                ValueLabelColor = SKColor.Parse("#4A69BD")
            }).ToList();

        currentChart = new BarChart
        {
            Entries = entries,
            LabelTextSize = 35,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelOrientation = Orientation.Horizontal,
            BackgroundColor = SKColors.Transparent,
            Margin = 20
        };
    }

    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        currentChart?.Draw(canvas, e.Info.Width, e.Info.Height);
    }

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        // Volta para a página anterior usando o Shell
        await Shell.Current.GoToAsync("..");
    }
}