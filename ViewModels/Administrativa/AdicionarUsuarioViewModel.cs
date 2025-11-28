using MauiApp1.Models.Administrativa;
using MauiApp1.Services.Admin;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp1.ViewModels.Administrativa
{
    public class AdicionarUsuarioViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;

        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _cargoSelecionado = string.Empty;
        private string? _unidadeGrupoSelecionada;

        private List<string> _cargosDisponiveis = new();
        private List<string> _unidadesGrupos = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        // -----------------------------
        // PROPRIEDADES VINCULADAS
        // -----------------------------
        public string Nome
        {
            get => _nome;
            set
            {
                if (_nome == value) return;
                _nome = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email == value) return;
                _email = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public string CargoSelecionado
        {
            get => _cargoSelecionado;
            set
            {
                if (_cargoSelecionado == value) return;
                _cargoSelecionado = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public string? UnidadeGrupoSelecionada
        {
            get => _unidadeGrupoSelecionada;
            set
            {
                if (_unidadeGrupoSelecionada == value) return;
                _unidadeGrupoSelecionada = value;
                OnPropertyChanged();
            }
        }

        public List<string> CargosDisponiveis
        {
            get => _cargosDisponiveis;
            set
            {
                _cargosDisponiveis = value;
                OnPropertyChanged();
            }
        }

        public List<string> UnidadesGrupos
        {
            get => _unidadesGrupos;
            set
            {
                _unidadesGrupos = value;
                OnPropertyChanged();
            }
        }

        // -----------------------------
        // COMMANDS
        // -----------------------------
        public ICommand SalvarCommand { get; }
        public ICommand VoltarCommand { get; }

        // -----------------------------
        // CONSTRUTOR
        // -----------------------------
        public AdicionarUsuarioViewModel(AdminService adminService)
        {
            _adminService = adminService;

            SalvarCommand = new Command(async () => await SalvarUsuario(), PodeSalvar);
            VoltarCommand = new Command(async () => await Voltar());

            CarregarDados();
        }

        // -----------------------------
        // MÉTODOS PRIVADOS
        // -----------------------------
        private void CarregarDados()
        {
            CargosDisponiveis = _adminService.ObterCargosDisponiveis();

            var unidades = _adminService.ObterUnidadesGrupos();

            // Remove "Todas as Unidades" pois não faz sentido como seleção individual
            UnidadesGrupos = unidades.Where(u => u != "Todas as Unidades").ToList();
        }

        private bool PodeSalvar()
        {
            return !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(CargoSelecionado) &&
                   IsEmailValido(Email);
        }

        private bool IsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SalvarUsuario()
        {
            var novoUsuario = new AdminModel
            {
                Nome = Nome.Trim(),
                Cargo = CargoSelecionado,
                UnidadeGrupo = UnidadeGrupoSelecionada
            };

            bool sucesso = await _adminService.AdicionarUsuario(novoUsuario);

            if (sucesso)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso! 🎉",
                    $"Usuário '{novoUsuario.Nome}' adicionado com sucesso!",
                    "OK");

                LimparCampos();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    "Não foi possível adicionar o usuário. Tente novamente.",
                    "OK");
            }
        }

        private async Task Voltar()
        {
            bool temDadosPreenchidos =
                !string.IsNullOrWhiteSpace(Nome) ||
                !string.IsNullOrWhiteSpace(Email) ||
                !string.IsNullOrWhiteSpace(CargoSelecionado);

            if (temDadosPreenchidos)
            {
                bool resposta = await Application.Current.MainPage.DisplayAlert(
                    "Confirmação",
                    "Deseja sair sem salvar?",
                    "Sim",
                    "Não");

                if (!resposta)
                    return;
            }

            LimparCampos();
            await Shell.Current.GoToAsync("..");
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            Email = string.Empty;
            CargoSelecionado = string.Empty;
            UnidadeGrupoSelecionada = null;
        }

        // -----------------------------
        // NOTIFICAÇÃO DE MUDANÇA
        // -----------------------------
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
