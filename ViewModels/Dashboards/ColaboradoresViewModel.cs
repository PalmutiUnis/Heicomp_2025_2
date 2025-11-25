using MauiApp1.Models.Colaboradores;
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

namespace MauiApp1.ViewModels.Dashboards
{
    public class ColaboradoresViewModel : INotifyPropertyChanged
    {
        private readonly ColaboradoresService _service;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ✅ Construtor com injeção manual
        public ColaboradoresViewModel(ColaboradoresService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            AbrirPopupCommand = new Command(async () => await AbrirPopup());
            FecharPopupCommand = new Command(() => MostrarTodosSetores = false);
            AbrirListaCompletaCommand = new Command(async () => await AbrirListaCompleta());
            MostrarValorCommand = new Command<string>(MostrarValor);
            FecharPopupColaboradoresCommand = new Command(() => MostrarTodosColaboradores = false);

            _ = CarregarDadosIniciais();
        }

        // ✅ Construtor sem parâmetros (cria o service com a connection factory)
        public ColaboradoresViewModel()
            : this(new ColaboradoresService(new MySqlConnectionFactory(new ConfigurationBuilder().Build())))
        {
        }

        // --------- Propriedades ---------
        private ObservableCollection<string> _unidades;
        public ObservableCollection<string> Unidades
        {
            get => _unidades;
            set { _unidades = value; OnPropertyChanged(); }
        }

        private ObservableCollection<int> _anos;
        public ObservableCollection<int> Anos
        {
            get => _anos;
            set { _anos = value; OnPropertyChanged(); }
        }

        private string _unidadeSelecionada = "TODAS";
        public string UnidadeSelecionada
        {
            get => _unidadeSelecionada;
            set
            {
                if (_unidadeSelecionada != value)
                {
                    _unidadeSelecionada = value;
                    OnPropertyChanged();
                    _ = AtualizarDados();
                }
            }
        }

        private int? _anoSelecionado = DateTime.Now.Year;
        public int? AnoSelecionado
        {
            get => _anoSelecionado;
            set
            {
                if (_anoSelecionado != value)
                {
                    _anoSelecionado = value;
                    OnPropertyChanged();
                    if (value.HasValue)
                        _ = AtualizarDados();
                }
            }
        }

        private int _totalColaboradores;
        public int TotalColaboradores
        {
            get => _totalColaboradores;
            set { _totalColaboradores = value; OnPropertyChanged(); }
        }

        private ObservableCollection<GeneroDistribuicaoModel> _distribuicaoGenero;
        public ObservableCollection<GeneroDistribuicaoModel> DistribuicaoGenero
        {
            get => _distribuicaoGenero;
            set { _distribuicaoGenero = value; OnPropertyChanged(); }
        }

        private string _generoHomensDisplay;
        public string GeneroHomensDisplay
        {
            get => _generoHomensDisplay;
            set { _generoHomensDisplay = value; OnPropertyChanged(); }
        }

        private string _generoMulheresDisplay;
        public string GeneroMulheresDisplay
        {
            get => _generoMulheresDisplay;
            set { _generoMulheresDisplay = value; OnPropertyChanged(); }
        }

        private StatusColaboradoresModel _status;
        public StatusColaboradoresModel Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SetorModel> _setores;
        public ObservableCollection<SetorModel> Setores
        {
            get => _setores;
            set { _setores = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ColaboradorResumoModel> _colaboradores;
        public ObservableCollection<ColaboradorResumoModel> Colaboradores
        {
            get => _colaboradores;
            set { _colaboradores = value; OnPropertyChanged(); }
        }

        private bool _mostrarTodosSetores;
        public bool MostrarTodosSetores
        {
            get => _mostrarTodosSetores;
            set { _mostrarTodosSetores = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SetorModel> _todosSetores;
        public ObservableCollection<SetorModel> TodosSetores
        {
            get => _todosSetores;
            set { _todosSetores = value; OnPropertyChanged(); }
        }

        private bool _mostrarTodosColaboradores;
        public bool MostrarTodosColaboradores
        {
            get => _mostrarTodosColaboradores;
            set { _mostrarTodosColaboradores = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ColaboradorResumoModel> _todosColaboradores;
        public ObservableCollection<ColaboradorResumoModel> TodosColaboradores
        {
            get => _todosColaboradores;
            set { _todosColaboradores = value; OnPropertyChanged(); }
        }

        // --------- Comandos ---------
        public ICommand AbrirPopupCommand { get; }
        public ICommand FecharPopupCommand { get; }
        public ICommand AbrirListaCompletaCommand { get; }
        public ICommand FecharPopupColaboradoresCommand { get; }
        public ICommand MostrarValorCommand { get; }

        private void MostrarValor(string index)
        {
            MostrarValor1 = MostrarValor2 = MostrarValor3 = MostrarValor4 = MostrarValor5 = false;
            switch (index)
            {
                case "1": MostrarValor1 = true; break;
                case "2": MostrarValor2 = true; break;
                case "3": MostrarValor3 = true; break;
                case "4": MostrarValor4 = true; break;
                case "5": MostrarValor5 = true; break;
            }
        }

        private bool _mostrarValor1, _mostrarValor2, _mostrarValor3, _mostrarValor4, _mostrarValor5;
        public bool MostrarValor1 { get => _mostrarValor1; set { _mostrarValor1 = value; OnPropertyChanged(); } }
        public bool MostrarValor2 { get => _mostrarValor2; set { _mostrarValor2 = value; OnPropertyChanged(); } }
        public bool MostrarValor3 { get => _mostrarValor3; set { _mostrarValor3 = value; OnPropertyChanged(); } }
        public bool MostrarValor4 { get => _mostrarValor4; set { _mostrarValor4 = value; OnPropertyChanged(); } }
        public bool MostrarValor5 { get => _mostrarValor5; set { _mostrarValor5 = value; OnPropertyChanged(); } }

        // --------- Métodos principais ---------
        private async Task CarregarDadosIniciais()
        {
            try
            {
                var testConn = await _service.TestarConexaoAsync();
                if (testConn != null && testConn.State == System.Data.ConnectionState.Open)
                {
                    await Application.Current.MainPage.DisplayAlert("Banco de Dados", "✅ Conectou com sucesso!", "OK");
                    await testConn.CloseAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Banco de Dados", "❌ Falha na conexão", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }

            try
            {
                var unidades = await _service.GetUnidadesAsync();
                var anos = await _service.GetAnosAsync();

                Unidades = new ObservableCollection<string>(unidades);
                Anos = new ObservableCollection<int>(anos);

                UnidadeSelecionada = "TODAS";
                AnoSelecionado = anos.Count > 0 ? anos[0] : DateTime.Now.Year;

                await AtualizarDados();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro CarregarDadosIniciais: {ex.Message}");
            }
        }

        private async Task AtualizarDados()
        {
            try
            {
                var unidade = UnidadeSelecionada ?? "TODAS";
                var ano = AnoSelecionado ?? DateTime.Now.Year;

                TotalColaboradores = await _service.GetTotalColaboradoresAsync(unidade, ano);

                var dist = await _service.GetDistribuicaoGeneroAsync(unidade, ano);
                DistribuicaoGenero = new ObservableCollection<GeneroDistribuicaoModel>(
                    dist.Select(d => new GeneroDistribuicaoModel { Sexo = d.Sexo, Quantidade = d.Quantidade, Percentual = d.Percentual })
                );

                GeneroHomensDisplay = DistribuicaoGenero.Count > 0 ? $"{DistribuicaoGenero[0].Quantidade} ({DistribuicaoGenero[0].Percentual:F1}%)" : "0 (0%)";
                GeneroMulheresDisplay = DistribuicaoGenero.Count > 1 ? $"{DistribuicaoGenero[1].Quantidade} ({DistribuicaoGenero[1].Percentual:F1}%)" : "0 (0%)";

                var s = await _service.GetStatusColaboradoresAsync(unidade, ano);
                Status = new StatusColaboradoresModel { Ativos = s.Ativos, EmLicenca = s.EmLicenca, Estagiarios = s.Estagiarios, Pcd = s.Pcd };

                var setores = await _service.GetColaboradoresPorSetorAsync(unidade, ano, true);
                Setores = new ObservableCollection<SetorModel>(
                    setores.Select(item => new SetorModel
                    {
                        Setor = item.Setor,
                        Quantidade = item.Quantidade,
                        Altura = Math.Clamp(item.Quantidade / 2.0, 3, 200)
                    })
                );

                var lista = await _service.GetListaColaboradoresAsync(unidade, ano, true);
                Colaboradores = new ObservableCollection<ColaboradorResumoModel>(
                    lista.Select(item => new ColaboradorResumoModel
                    {
                        Nome = item.Nome,
                        Setor = item.Setor,
                        Cargo = item.Cargo,
                        Status = item.Status
                    })
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro AtualizarDados: {ex.Message}");
            }
        }

        private async Task AbrirPopup()
        {
            try
            {
                var unidade = UnidadeSelecionada ?? "TODAS";
                var ano = AnoSelecionado ?? DateTime.Now.Year;
                var setores = await _service.GetColaboradoresPorSetorAsync(unidade, ano, false);

                TodosSetores = new ObservableCollection<SetorModel>(
                    setores.Select(s => new SetorModel { Setor = s.Setor, Quantidade = s.Quantidade })
                );
                MostrarTodosSetores = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro AbrirPopup: {ex.Message}");
            }
        }

        private async Task AbrirListaCompleta()
        {
            try
            {
                var unidade = Uri.EscapeDataString(UnidadeSelecionada ?? "TODAS");
                var ano = AnoSelecionado ?? DateTime.Now.Year;
                await Shell.Current.GoToAsync($"ListaColaboradoresPage?unidade={unidade}&ano={ano}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro AbrirListaCompleta: {ex.Message}");
            }
        }
    }
}
