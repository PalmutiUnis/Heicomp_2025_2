using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiApp1.Services;
using Microcharts;
using SkiaSharp;
namespace MauiApp1.ViewModels.Dashboards
{
    public class CargosViewModel : INotifyPropertyChanged
    {
        private readonly ICargosService _cargosService;

        public ObservableCollection<CargoCategoriaDto> CargoCategorias { get; } = new();
        public ObservableCollection<CategoriaTotalDto> CategoriaTotais { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private int _totalColaboradores;
        public int TotalColaboradores
        {
            get => _totalColaboradores;
            private set => SetProperty(ref _totalColaboradores, value);
        }

        private Chart? _pieChart;
        public Chart? PieChart
        {
            get => _pieChart;
            private set => SetProperty(ref _pieChart, value);
        }

        public CargosViewModel(ICargosService cargosService)
        {
            _cargosService = cargosService;
        }

        public async Task CarregarDadosAsync(CancellationToken ct = default)
        {
            IsBusy = true;
            try
            {
                CargoCategorias.Clear();
                CategoriaTotais.Clear();

                var cargos = await _cargosService.GetCargoCategoriasAsync(ct);
                foreach (var item in cargos)
                    CargoCategorias.Add(item);

                var totais = await _cargosService.GetCategoriaTotaisAsync(ct);
                foreach (var item in totais)
                    CategoriaTotais.Add(item);

                AtualizarIndicadores();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AtualizarIndicadores()
        {
            var totalRow = CategoriaTotais.FirstOrDefault(x => string.Equals(x.Categoria, "Total", StringComparison.OrdinalIgnoreCase));
            TotalColaboradores = totalRow?.Total ?? CategoriaTotais.Sum(x => x.Total);

            //Variáveis criadas para ajustar as cores do gráfico conforme o tema (claro/escuro) da aplicação
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            var corDoTexto = isDark ? SKColors.White : SKColors.Black;

            var entries = CategoriaTotais
                .Where(x => !string.Equals(x.Categoria, "Total", StringComparison.OrdinalIgnoreCase))
                .Select(x => new Microcharts.ChartEntry(x.Total)
                {
                    Label = x.Categoria,
                    TextColor = corDoTexto,
                    ValueLabel = x.Total.ToString(),
                    ValueLabelColor = corDoTexto,
                    Color = CategoriaToColor(x.Categoria)
                    
                })
                .ToList();

            PieChart = new Microcharts.PieChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent
            };
        }

        private static SKColor CategoriaToColor(string categoria)
        {
            var key = (categoria ?? string.Empty).Trim().ToLowerInvariant();
            return key switch
            {
                "docente" => SKColor.Parse("#2563EB"),
                "administrativo" => SKColor.Parse("#10B981"),
                "dois cargos" => SKColor.Parse("#F59E0B"),
                _ => SKColor.Parse("#6B7280")
            };
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
