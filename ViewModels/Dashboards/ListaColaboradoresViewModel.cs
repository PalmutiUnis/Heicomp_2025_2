using Heicomp_2025_2.Models.Colaboradores;
using Heicomp_2025_2.Services;
using MauiApp1.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Heicomp_2025_2.ViewModels.Dashboards
{
    [QueryProperty(nameof(Unidade), "unidade")]
    [QueryProperty(nameof(Ano), "ano")]
    public class ListaColaboradoresViewModel : INotifyPropertyChanged
    {
        private readonly ColaboradoresService _service;
        private const int PageSize = 25;
        private int _offset = 0;
        private bool _temMais = true;
        private bool _isLoading = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ✅ Construtor com injeção manual
        public ListaColaboradoresViewModel(ColaboradoresService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            CarregarMaisCommand = new Command(async () => await CarregarMais());
            BuscarCommand = new Command(async () => await Recarregar());
        }

        // ✅ Construtor sem parâmetros (cria o service com a factory)
        public ListaColaboradoresViewModel()
            : this(new ColaboradoresService(new MySqlConnectionFactory(new ConfigurationBuilder().Build())))
        {
        }

        public async Task InitializeAsync()
        {
            _offset = 0;
            Colaboradores.Clear();
            TemMais = true;
            await CarregarMais();
        }

        private string _filtro;
        public string Filtro
        {
            get => _filtro;
            set
            {
                if (_filtro != value)
                {
                    _filtro = value;
                    OnPropertyChanged();
                    _ = Recarregar();
                }
            }
        }

        private ObservableCollection<ColaboradorResumoModel> _colaboradores = new();
        public ObservableCollection<ColaboradorResumoModel> Colaboradores
        {
            get => _colaboradores;
            set { _colaboradores = value; OnPropertyChanged(); }
        }

        public bool TemMais
        {
            get => _temMais;
            set { _temMais = value; OnPropertyChanged(); }
        }

        public ICommand CarregarMaisCommand { get; }
        public ICommand BuscarCommand { get; }

        private async Task Recarregar()
        {
            if (_isLoading) return;

            _offset = 0;
            Colaboradores.Clear();
            TemMais = true;

            if (!string.IsNullOrWhiteSpace(Filtro))
                await CarregarMais();
        }

        private async Task CarregarMais()
        {
            if (!TemMais || _isLoading) return;

            try
            {
                _isLoading = true;

                var unidade = Unidade ?? "TODAS";
                var ano = Ano;

                var novos = await _service.GetListaColaboradoresPaginadoAsync(unidade, ano, _offset, PageSize, Filtro ?? "");

                if (novos == null || novos.Count == 0)
                {
                    TemMais = false;
                    return;
                }

                foreach (var item in novos)
                {
                    if (Colaboradores.Any(c => c.Nome == item.Nome && c.Setor == item.Setor && c.Cargo == item.Cargo))
                        continue;

                    Colaboradores.Add(new ColaboradorResumoModel
                    {
                        Nome = item.Nome,
                        Setor = item.Setor,
                        Cargo = item.Cargo,
                        Status = item.Status
                    });
                }

                _offset += novos.Count;
                if (novos.Count < PageSize)
                    TemMais = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro CarregarMais: {ex.Message}");
                TemMais = false;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private string _unidade = "TODAS";
        public string Unidade { get => _unidade; set { _unidade = value; OnPropertyChanged(); } }

        private int _ano = DateTime.Now.Year;
        public int Ano { get => _ano; set { _ano = value; OnPropertyChanged(); } }
    }
}
