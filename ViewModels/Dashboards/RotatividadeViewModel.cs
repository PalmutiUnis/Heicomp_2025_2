using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiApp1.Services;

namespace Heicomp_2025_2.ViewModels.Dashboards
{
    public class RotatividadeViewModel : INotifyPropertyChanged
    {
        private readonly TurnoverRepository _turnoverRepository;
        private bool _isLoading;
        private string _turnover = "0";
        private double _turnoverValue = 0;
        private string _admissoes = "0";
        private string _desligamentos = "0";
        private string _totalColaboradores = "0";

        public event PropertyChangedEventHandler? PropertyChanged;

        
        public RotatividadeViewModel(TurnoverRepository turnoverRepository)
        {
            _turnoverRepository = turnoverRepository;
            LoadDataAsync();
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Turnover
        {
            get => _turnover;
            set
            {
                if (_turnover != value)
                {
                    _turnover = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TurnoverValue
        {
            get => _turnoverValue;
            set
            {
                if (_turnoverValue != value)
                {
                    _turnoverValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Admissoes
        {
            get => _admissoes;
            set
            {
                if (_admissoes != value)
                {
                    _admissoes = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Desligamentos
        {
            get => _desligamentos;
            set
            {
                if (_desligamentos != value)
                {
                    _desligamentos = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TotalColaboradores
        {
            get => _totalColaboradores;
            set
            {
                if (_totalColaboradores != value)
                {
                    _totalColaboradores = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var dados = await _turnoverRepository.GetTurnoverCompletoAsync();

                TurnoverValue = (double)dados.Turnover;
                Turnover = $"{dados.Turnover:F2}%";
                Admissoes = dados.Admissoes.ToString();
                Desligamentos = dados.Desligamentos.ToString();
                TotalColaboradores = dados.TotalColaboradores.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados de rotatividade: {ex.Message}");
                TurnoverValue = 0;
                Turnover = "Erro";
                Admissoes = "Erro";
                Desligamentos = "Erro";
                TotalColaboradores = "Erro";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
