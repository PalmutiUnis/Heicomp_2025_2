using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiApp1.Services;
using Microcharts;
using SkiaSharp;

namespace Heicomp_2025_2.ViewModels.Dashboards
{
    public class PainelGestaoViewModel : INotifyPropertyChanged
    {
        private readonly ICargosService _cargosService;
        private readonly IDiversidadeService _diversidadeService;
        private Chart _chart;
        private Chart _genderChart;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PainelGestaoViewModel(ICargosService cargosService, IDiversidadeService diversidadeService)
        {
            _cargosService = cargosService;
            _diversidadeService = diversidadeService;
            _chart = new PieChart();
            _genderChart = new PieChart();
            LoadDataAsync();
        }

        public Chart Chart
        {
            get => _chart;
            set
            {
                if (_chart != value)
                {
                    _chart = value;
                    OnPropertyChanged();
                }
            }
        }

        public Chart GenderChart
        {
            get => _genderChart;
            set
            {
                if (_genderChart != value)
                {
                    _genderChart = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void LoadDataAsync()
        {
            try
            {
                var data = await _cargosService.GetCategoriaTotaisAsync();
                var entries = new List<ChartEntry>();
                var colors = new[] { "#1E40AF", "#7C3AED", "#059669" };
                var colorIndex = 0;

                foreach (var item in data)
                {
                    if (item.Categoria != "Total")
                    {
                        entries.Add(new ChartEntry(item.Total)
                        {
                            Label = item.Categoria,
                            ValueLabel = item.Total.ToString(),
                            Color = SKColor.Parse(colors[colorIndex])
                        });
                        colorIndex++;
                    }
                }

                Chart = new PieChart
                {
                    Entries = entries,
                    LabelTextSize = 30,
                    BackgroundColor = SKColors.Transparent
                };

                var genderData = await _diversidadeService.GetGenderDistributionAsync();
                var genderEntries = new List<ChartEntry>();
                var genderColors = new[] { "#3B82F6", "#EC4899", "#6B7280" };
                var genderColorIndex = 0;

                foreach (var item in genderData)
                {
                    genderEntries.Add(new ChartEntry(item.Value)
                    {
                        Label = item.Key,
                        ValueLabel = item.Value.ToString(),
                        Color = SKColor.Parse(genderColors[genderColorIndex])
                    });
                    genderColorIndex++;
                }

                GenderChart = new PieChart
                {
                    Entries = genderEntries,
                    LabelTextSize = 30,
                    BackgroundColor = SKColors.Transparent
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados de cargos: {ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
