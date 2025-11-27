using MauiApp1.ViewModels.Dashboards;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MauiApp1.Services;


namespace MauiApp1.Views.Dashboards;

public partial class AlunosPage : ContentPage
{
    private readonly AlunosViewModel vm;

    
    public AlunosPage() : this(new AlunosViewModel())
    {
    }
    public AlunosPage(AlunosViewModel viewModel)
    {
        InitializeComponent();

        vm = viewModel;
        BindingContext = vm;

        vm.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(vm.DadosGraficoCampus):
                    GraficoCampus?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoModalidades):
                    GraficoModalidades?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoCursos):
                case nameof(vm.Top5CursosLegenda):
                    GraficoCursos?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoTurnos):
                    GraficoTurnos?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoPeriodos):
                case nameof(vm.PeriodosLegenda):
                    GraficoPeriodos?.InvalidateSurface();
                    break;

                case nameof(vm.DadosGraficoTurmas):
                case nameof(vm.TurmasLegenda):
                    GraficoTurmas?.InvalidateSurface();
                    break;
            }
        };
    }

    private void OnGraficoCampusPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.CampusLegenda == null || vm.CampusLegenda.Count == 0) return;

        var entries = vm.CampusLegenda.OrderByDescending(x => x.Quantidade).ToList();

        float width = e.Info.Width;
        float height = e.Info.Height - 120;

        int count = entries.Count;
        float padding = 40;
        float availableWidth = width - (padding * 2);
        float barWidth = availableWidth / count * 0.65f;
        float spacing = availableWidth / count * 0.35f;

        float maxValue = entries.Max(x => x.Quantidade);

        float topPadding = 80;
        float graphHeight = height - topPadding - 40;

        using var paintFundo = new SKPaint { Color = SKColors.LightGray.WithAlpha(80) };
        using var paintBarra = new SKPaint { IsAntialias = true };
        using var paintTexto = new SKPaint
        {
            TextSize = 36,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            float x = padding + i * (barWidth + spacing) + spacing / 2;

            float barHeight = (entry.Quantidade / maxValue) * graphHeight;
            float barBottom = height - 40;
            float barTop = barBottom - barHeight;

            canvas.DrawRoundRect(x, barBottom - graphHeight, barWidth, graphHeight, 12, 12, paintFundo);

            paintBarra.Color = entry.Cor.ToSKColor();
            canvas.DrawRoundRect(x, barTop, barWidth, barHeight, 12, 12, paintBarra);

            string texto = entry.Quantidade.ToString("N0");
            paintTexto.Color = entry.Cor.ToSKColor();

            float textoY = barTop - 18;


            if (textoY < 50) textoY = barTop + 20;

            canvas.DrawText(texto, x + barWidth / 2, textoY, paintTexto);
        }
    }

    private void OnGraficoModalidadesPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.DadosGraficoModalidades == null || vm.DadosGraficoModalidades.Count == 0) return;

        var entries = vm.DadosGraficoModalidades.ToList();
        var coresSkia = vm.ModalidadesLegenda.Select(item => item.Cor.ToSKColor()).ToArray();

        float width = e.Info.Width;
        float height = e.Info.Height - 120;
        float centerX = width / 2;
        float centerY = height / 2;
        float radius = Math.Min(width, height) / 2.8f;
        float holeRadius = radius * 0.5f;

        float total = entries.Sum(x => x.Value);
        float startAngle = -90;

        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

        if (entries.Count == 1)
        {
            paint.Color = coresSkia[0];
            canvas.DrawCircle(centerX, centerY, radius, paint);
        }
        else
        {
            for (int i = 0; i < entries.Count; i++)
            {
                float sweepAngle = (entries[i].Value / total) * 360;
                paint.Color = coresSkia[i % coresSkia.Length];

                using var path = new SKPath();
                path.MoveTo(centerX, centerY);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();
                canvas.DrawPath(path, paint);

                startAngle += sweepAngle;
            }
        }

        paint.Color = Application.Current?.RequestedTheme == AppTheme.Dark
            ? SKColor.Parse("#1F2937")
            : SKColors.White;
        canvas.DrawCircle(centerX, centerY, holeRadius, paint);
    }

    private void OnGraficoCursosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.Top5CursosLegenda == null || vm.Top5CursosLegenda.Count == 0) return;

        var entries = vm.Top5CursosLegenda.OrderByDescending(x => x.Quantidade).ToList();

        float width = e.Info.Width;
        float height = e.Info.Height;

        int count = entries.Count;
        float padding = 40;
        float availableWidth = width - (padding * 2);
        float barWidth = availableWidth / count * 0.65f;
        float spacing = availableWidth / count * 0.35f;

        float maxValue = entries.Max(x => x.Quantidade);
        float graphHeight = height - 100;

        using var paintFundo = new SKPaint { Color = SKColors.LightGray.WithAlpha(80) };
        using var paintBarra = new SKPaint();
        using var paintTexto = new SKPaint
        {
            TextSize = 36,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            IsAntialias = true
        };

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            float x = padding + i * (barWidth + spacing) + spacing / 2;
            float barHeight = (entry.Quantidade / maxValue) * graphHeight;
            float y = height - 60 - barHeight;

            canvas.DrawRoundRect(x, height - 60 - graphHeight, barWidth, graphHeight, 12, 12, paintFundo);

            paintBarra.Color = entry.Cor.ToSKColor();
            canvas.DrawRoundRect(x, y, barWidth, barHeight, 12, 12, paintBarra);

            string texto = entry.Quantidade.ToString("N0");
            var textBounds = new SKRect();
            paintTexto.Color = entry.Cor.ToSKColor();
            paintTexto.MeasureText(texto, ref textBounds);

            canvas.DrawText(texto, x + barWidth / 2 - textBounds.MidX, y - 10, paintTexto);
        }
    }

    private void OnGraficoTurnosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.DadosGraficoTurnos == null || vm.DadosGraficoTurnos.Count == 0) return;

        var entries = vm.DadosGraficoTurnos.ToList();

        float width = e.Info.Width;
        float height = e.Info.Height;

        float chartHeight = height - 120;
        float centerX = width / 2;
        float centerY = chartHeight / 2;
        float radius = Math.Min(width, chartHeight) / 2.8f;
        float holeRadius = radius * 0.5f;

        var coresSkia = vm.TurnosLegenda.Select(item => item.Cor.ToSKColor()).ToArray();

        float total = entries.Sum(x => x.Value);
        float startAngle = -90;

        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

        if (entries.Count == 1)
        {
            paint.Color = coresSkia[0];
            canvas.DrawCircle(centerX, centerY, radius, paint);
        }
        else
        {
            for (int i = 0; i < entries.Count; i++)
            {
                float sweepAngle = (entries[i].Value / total) * 360;
                paint.Color = coresSkia[i % coresSkia.Length];

                using var path = new SKPath();
                path.MoveTo(centerX, centerY);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();
                canvas.DrawPath(path, paint);

                startAngle += sweepAngle;
            }
        }

        paint.Color = Application.Current?.RequestedTheme == AppTheme.Dark
            ? SKColor.Parse("#1F2937")
            : SKColors.White;
        canvas.DrawCircle(centerX, centerY, holeRadius, paint);
    }

    private void OnGraficoPeriodosPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.PeriodosLegenda == null || !vm.PeriodosLegenda.Any()) return;

        var entries = vm.PeriodosLegenda.OrderByDescending(x => x.Quantidade).ToList();

        float width = e.Info.Width;
        float height = e.Info.Height;
        int count = entries.Count;

        float paddingLeftRight = 40;
        float paddingBottom = 40;
        float paddingTop = 60;

        float availableWidth = width - (paddingLeftRight * 2);
        float availableHeight = height - paddingBottom - paddingTop;

        float barWidth = availableWidth / count * 0.65f;
        float spacing = availableWidth / count * 0.35f;

        float maxValue = entries.Max(x => x.Quantidade);

        using var paintFundo = new SKPaint { Color = SKColors.LightGray.WithAlpha(80) };
        using var paintBarra = new SKPaint { IsAntialias = true };
        using var paintTextoValor = new SKPaint
        {
            TextSize = 28,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            float x = paddingLeftRight + i * (barWidth + spacing);
            float barHeight = (entry.Quantidade / maxValue) * availableHeight;
            float barTop = paddingTop + (availableHeight - barHeight);

            canvas.DrawRoundRect(x, paddingTop, barWidth, availableHeight, 12, 12, paintFundo);

            paintBarra.Color = entry.Cor.ToSKColor();
            canvas.DrawRoundRect(x, barTop, barWidth, barHeight, 12, 12, paintBarra);

            string valorTexto = entry.Quantidade.ToString("N0");
            paintTextoValor.Color = entry.Cor.ToSKColor();
            canvas.DrawText(valorTexto, x + barWidth / 2, barTop - 10, paintTextoValor);
        }
    }

    private void OnGraficoTurmasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (vm.TurmasLegenda == null || !vm.TurmasLegenda.Any()) return;

        var entries = vm.TurmasLegenda.OrderByDescending(x => x.Quantidade).ToList();

        float width = e.Info.Width;
        float height = e.Info.Height;

        float paddingLeftRight = 40;
        float paddingTop = 60;
        float paddingBottom = 40;
        float availableHeight = height - paddingTop - paddingBottom;
        float availableWidth = width - (paddingLeftRight * 2);

        int count = entries.Count;
        float barWidth = availableWidth / count * 0.7f;
        float spacing = availableWidth / count * 0.3f;

        float maxValue = entries.Max(x => x.Quantidade);

        using var paintFundo = new SKPaint { Color = SKColors.LightGray.WithAlpha(80) };
        using var paintBarra = new SKPaint { IsAntialias = true };
        using var paintTexto = new SKPaint
        {
            TextSize = 28,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            float x = paddingLeftRight + i * (barWidth + spacing);
            float barHeight = (entry.Quantidade / maxValue) * availableHeight;
            float barTop = paddingTop + (availableHeight - barHeight);

            canvas.DrawRoundRect(x, paddingTop, barWidth, availableHeight, 12, 12, paintFundo);

            paintBarra.Color = entry.Cor.ToSKColor();
            canvas.DrawRoundRect(x, barTop, barWidth, barHeight, 12, 12, paintBarra);

            string texto = entry.Quantidade.ToString("N0");
            paintTexto.Color = entry.Cor.ToSKColor();
            canvas.DrawText(texto, x + barWidth / 2, barTop - 10, paintTexto);
        }
    }

    private async void OnLimparFiltrosClicked(object sender, EventArgs e)
    {
        vm.LimparFiltros();

        await Task.Delay(300);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            GraficoCampus?.InvalidateSurface();
            GraficoModalidades?.InvalidateSurface();
            GraficoCursos?.InvalidateSurface();
            GraficoTurnos?.InvalidateSurface();
            GraficoPeriodos?.InvalidateSurface();
        });
    }

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}