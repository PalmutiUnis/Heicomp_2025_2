using MauiApp1.Models.Administrativa;
using MauiApp1.Services.Admin;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp1.ViewModels.Administrativa
{
    public class AreaAdministrativaViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;

        private ObservableCollection<AdminModel> _usuarios;
        private ObservableCollection<AdminModel> _usuariosFiltrados;

        private List<string> _unidadesGrupos;
        private string _unidadeSelecionada;
        private string _situacaoSelecionada;
        private string _textoBusca = string.Empty;

        private bool _isRefreshing = false;
        private bool _isBusy = false;

        private int _totalUsuarios = 0;
        private int _totalTrabalhando = 0;
        private int _totalDemitidos = 0;
        private int _totalAposentados = 0;
        private int _totalAuxilioDoenca = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        // ------------------- PROPRIEDADES -------------------

        public ObservableCollection<AdminModel> Usuarios
        {
            get => _usuarios;
            set { _usuarios = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AdminModel> UsuariosFiltrados
        {
            get => _usuariosFiltrados;
            set
            {
                _usuariosFiltrados = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ContagemUsuarios));
            }
        }

        public List<string> UnidadesGrupos
        {
            get => _unidadesGrupos;
            set { _unidadesGrupos = value; OnPropertyChanged(); }
        }

        public string UnidadeSelecionada
        {
            get => _unidadeSelecionada;
            set
            {
                _unidadeSelecionada = value;
                OnPropertyChanged();
                AplicarFiltrosLocal();
            }
        }

        public string SituacaoSelecionada
        {
            get => _situacaoSelecionada;
            set
            {
                _situacaoSelecionada = value;
                OnPropertyChanged();
                AplicarFiltrosLocal();
            }
        }

        public string TextoBusca
        {
            get => _textoBusca;
            set
            {
                _textoBusca = value;
                OnPropertyChanged();
                AplicarFiltrosLocal();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string ContagemUsuarios => $"{UsuariosFiltrados?.Count ?? 0} usuários cadastrados";

        // Contadores
        public int TotalUsuarios { get => _totalUsuarios; set { _totalUsuarios = value; OnPropertyChanged(); } }
        public int TotalTrabalhando { get => _totalTrabalhando; set { _totalTrabalhando = value; OnPropertyChanged(); } }
        public int TotalDemitidos { get => _totalDemitidos; set { _totalDemitidos = value; OnPropertyChanged(); } }
        public int TotalAposentados { get => _totalAposentados; set { _totalAposentados = value; OnPropertyChanged(); } }
        public int TotalAuxilioDoenca { get => _totalAuxilioDoenca; set { _totalAuxilioDoenca = value; OnPropertyChanged(); } }

        // ------------------- COMANDOS -------------------

        public ICommand AtualizarListaCommand { get; }
        public ICommand VerDetalhesCommand { get; }
        public ICommand DeletarUsuarioCommand { get; }

        // ------------------- CONSTRUTOR -------------------

        public AreaAdministrativaViewModel(AdminService adminService)
        {
            _adminService = adminService;

            _usuarios = new ObservableCollection<AdminModel>();
            _usuariosFiltrados = new ObservableCollection<AdminModel>();
            _unidadesGrupos = new List<string>();

            _unidadeSelecionada = "Todas as Unidades";
            _situacaoSelecionada = "Todos";

            AtualizarListaCommand = new Command(async () => await CarregarDadosAsync());
            VerDetalhesCommand = new Command<AdminModel>(async (u) => await VerDetalhes(u));
            DeletarUsuarioCommand = new Command<AdminModel>(async (u) => await DeletarUsuario(u));
        }

        // ------------------- MÉTODO PRINCIPAL -------------------

        public async Task CarregarDadosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            IsRefreshing = true;

            try
            {
                var listaUsuarios = await _adminService.ObterTodosUsuariosAsync();

                Usuarios = new ObservableCollection<AdminModel>(listaUsuarios);

                TotalUsuarios = Usuarios.Count;
                TotalTrabalhando = Usuarios.Count(u => u.EstaAtivo);
                TotalDemitidos = Usuarios.Count(u => u.EstaDemitido);
                TotalAposentados = Usuarios.Count(u => u.EstaAposentadoPorInvalidez);
                TotalAuxilioDoenca = Usuarios.Count(u => u.EstaEmAuxilioDoenca);

                var unidades = await _adminService.ObterListaUnidadesAsync();
                UnidadesGrupos = unidades;

                AplicarFiltrosLocal();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro ao carregar dados",
                    $"Ocorreu um erro:\n{ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // ------------------- FILTROS -------------------

        private void AplicarFiltrosLocal()
        {
            if (Usuarios == null || Usuarios.Count == 0)
            {
                UsuariosFiltrados = new ObservableCollection<AdminModel>();
                return;
            }

            var query = Usuarios.AsEnumerable();

            // --- Filtrar Situação ---
            if (!string.IsNullOrEmpty(SituacaoSelecionada) && SituacaoSelecionada != "Todos")
            {
                switch (SituacaoSelecionada.ToUpper())
                {
                    case "TRABALHANDO":
                        query = query.Where(u => u.EstaAtivo);
                        break;

                    case "DEMITIDOS":
                    case "DEMITIDO":
                        query = query.Where(u => u.EstaDemitido);
                        break;

                    case "APOSENTADOS":
                    case "APOSENTADORIA POR INVALIDEZ":
                        query = query.Where(u => u.EstaAposentadoPorInvalidez);
                        break;

                    case "AUXÍLIO DOENÇA":
                    case "AUXILIO DOENÇA":
                        query = query.Where(u => u.EstaEmAuxilioDoenca);
                        break;
                }
            }

            // --- Filtrar Unidade ---
            if (!string.IsNullOrEmpty(UnidadeSelecionada) &&
                UnidadeSelecionada != "Todas as Unidades")
            {
                query = query.Where(u => u.UnidadeGrupo == UnidadeSelecionada);
            }

            // --- Busca ---
            if (!string.IsNullOrWhiteSpace(TextoBusca))
            {
                var busca = TextoBusca.ToLower();

                query = query.Where(u =>
                    u.Nome.ToLower().Contains(busca) ||
                    u.Cargo.ToLower().Contains(busca) ||
                    (u.UnidadeGrupo?.ToLower().Contains(busca) ?? false)
                );
            }

            UsuariosFiltrados = new ObservableCollection<AdminModel>(query.ToList());
        }

        // ------------------- Ações da UI -------------------

        private async Task VerDetalhes(AdminModel usuario)
        {
            if (usuario == null) return;

            string detalhes =
                $"📋 INFORMAÇÕES DO COLABORADOR\n\n" +
                $"ID: {usuario.Id}\n" +
                $"Nome: {usuario.Nome}\n" +
                $"Email: {usuario.Email}\n" +
                $"Cargo: {usuario.Cargo}\n" +
                $"{usuario.StatusEmoji} Situação: {usuario.StatusDescricao}\n";

            if (!string.IsNullOrEmpty(usuario.UnidadeGrupo))
                detalhes += $"Unidade: {usuario.UnidadeGrupo}\n";

            detalhes += "\n📊 STATUS:\n" +
                        $"Situação Real: {usuario.DescricaoSituacao}\n" +
                        $"Ativo: {(usuario.EstaAtivo ? "Sim" : "Não")}\n" +
                        $"Demitido: {(usuario.EstaDemitido ? "Sim" : "Não")}\n" +
                        $"Aposentado: {(usuario.EstaAposentadoPorInvalidez ? "Sim" : "Não")}\n" +
                        $"Auxílio Doença: {(usuario.EstaEmAuxilioDoenca ? "Sim" : "Não")}\n";

            await Application.Current!.MainPage!.DisplayAlert(
                "Detalhes do Colaborador",
                detalhes,
                "Fechar");
        }

        private async Task DeletarUsuario(AdminModel usuario)
        {
            if (usuario == null) return;

            bool confirmar = await Application.Current!.MainPage!.DisplayAlert(
                "Excluir Usuário",
                $"Deseja realmente excluir '{usuario.Nome}'?",
                "Sim",
                "Cancelar"
            );

            if (!confirmar) return;

            try
            {
                Usuarios.Remove(usuario);
                AplicarFiltrosLocal();

                // Atualiza contadores locais
                TotalUsuarios--;
                if (usuario.EstaAtivo) TotalTrabalhando--;
                if (usuario.EstaDemitido) TotalDemitidos--;
                if (usuario.EstaAposentadoPorInvalidez) TotalAposentados--;
                if (usuario.EstaEmAuxilioDoenca) TotalAuxilioDoenca--;

                await Application.Current!.MainPage!.DisplayAlert(
                    "Sucesso",
                    "Usuário removido localmente.",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro ao excluir",
                    ex.Message,
                    "OK");
            }
        }

        // ------------------- NOTIFICAÇÃO -------------------

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
