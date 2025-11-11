using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input; 
using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using SkiaSharp;
using System.Windows.Input;





public class AlunosViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ========== NÍVEIS DE DRILL-DOWN ==========
    // 1. Unidades → 2. Curso → 3. Turno → 4. Modalidade

    private int _nivelAtual = 1;
    public int NivelAtual
    {
        get => _nivelAtual;
        set
        {
            _nivelAtual = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarUnidades));
            OnPropertyChanged(nameof(MostrarCursos));
            OnPropertyChanged(nameof(MostrarTurnos));
            OnPropertyChanged(nameof(MostrarModalidades));
            AtualizarDados();
        }
    }

    // Propriedades para mostrar/ocultar filtros baseado no nível
    public bool MostrarUnidades => NivelAtual >= 1;
    public bool MostrarCursos => NivelAtual >= 2;
    public bool MostrarTurnos => NivelAtual >= 3;
    public bool MostrarModalidades => NivelAtual >= 4;

    // ========== LISTAS DE FILTROS ==========
    public ObservableCollection<string> Unidades { get; set; }
    public ObservableCollection<string> Cursos { get; set; }
    public ObservableCollection<string> Turnos { get; set; }
    public ObservableCollection<string> Modalidades { get; set; }

    // ========== VALORES SELECIONADOS ==========
    private string? _unidadeSelecionada;
    public string? UnidadeSelecionada
    {
        get => _unidadeSelecionada;
        set
        {
            _unidadeSelecionada = value;
            OnPropertyChanged();
            if (value != null && NivelAtual < 2) NivelAtual = 2;
        }
    }

    private string? _cursoSelecionado;
    public string? CursoSelecionado
    {
        get => _cursoSelecionado;
        set
        {
            _cursoSelecionado = value;
            OnPropertyChanged();
            if (value != null && NivelAtual < 3) NivelAtual = 3;
        }
    }

    private string? _turnoSelecionado;
    public string? TurnoSelecionado
    {
        get => _turnoSelecionado;
        set
        {
            _turnoSelecionado = value;
            OnPropertyChanged();
            if (value != null && NivelAtual < 4) NivelAtual = 4;
        }
    }

    private string? _modalidadeSelecionada;
    public string? ModalidadeSelecionada
    {
        get => _modalidadeSelecionada;
        set
        {
            _modalidadeSelecionada = value;
            OnPropertyChanged();
            AtualizarDados();
        }
    }

    // ========== DADOS DOS CARDS ==========
    private int _totalAlunos;
    public int TotalAlunos
    {
        get => _totalAlunos;
        set { _totalAlunos = value; OnPropertyChanged(); }
    }

    private int _educacaoBasica;
    public int EducacaoBasica
    {
        get => _educacaoBasica;
        set { _educacaoBasica = value; OnPropertyChanged(); }
    }

    private int _educacaoSuperior;
    public int EducacaoSuperior
    {
        get => _educacaoSuperior;
        set { _educacaoSuperior = value; OnPropertyChanged(); }
    }

    // ========== DADOS DO GRÁFICO ==========
    private Dictionary<string, int>? _dadosGrafico;
    public Dictionary<string, int>? DadosGrafico
    {
        get => _dadosGrafico;
        set
        {
            _dadosGrafico = value;
            OnPropertyChanged();
        }
    }

    // ========== CONSTRUTOR ==========
    public AlunosViewModel()
    {
        // Inicializa listas
        Unidades = new ObservableCollection<string>
        {
            "Campus Varginha",
            "Campus Três Pontas",
            "Campus Poços de Caldas",
            "Campus São Lourenço"
        };

        Cursos = new ObservableCollection<string>
        {
            "Informática",
            "Administração",
            "Enfermagem",
            "Engenharia Civil"
        };

        Turnos = new ObservableCollection<string>
        {
            "Matutino",
            "Vespertino",
            "Noturno",
            "Integral"
        };

        Modalidades = new ObservableCollection<string>
        {
            "Presencial",
            "EAD",
            "Híbrido"
        };

        // Carrega dados iniciais
        AtualizarDados();
    }

    // ========== ATUALIZAÇÃO DE DADOS (MOCK) ==========
    private void AtualizarDados()
    {
        var random = new Random();

        // Simula dados baseados nos filtros
        TotalAlunos = random.Next(8000, 15000);
        EducacaoBasica = random.Next(5000, 9000);
        EducacaoSuperior = random.Next(3000, 6000);

        // Dados do gráfico conforme o nível
        DadosGrafico = NivelAtual switch
        {
            1 => new Dictionary<string, int> // Por Unidade
            {
                { "Varginha", random.Next(3000, 5000) },
                { "Três Pontas", random.Next(2000, 4000) },
                { "Poços de Caldas", random.Next(2000, 3500) },
                { "São Lourenço", random.Next(1500, 3000) }
            },
            2 => new Dictionary<string, int> // Por Curso
            {
                { "Informática", random.Next(800, 1500) },
                { "Administração", random.Next(600, 1200) },
                { "Enfermagem", random.Next(700, 1300) },
                { "Eng. Civil", random.Next(500, 1000) }
            },
            3 => new Dictionary<string, int> // Por Turno
            {
                { "Matutino", random.Next(1000, 2000) },
                { "Vespertino", random.Next(800, 1500) },
                { "Noturno", random.Next(1200, 2200) },
                { "Integral", random.Next(500, 1000) }
            },
            4 => new Dictionary<string, int> // Por Modalidade
            {
                { "Presencial", random.Next(2000, 4000) },
                { "EAD", random.Next(1500, 3000) },
                { "Híbrido", random.Next(1000, 2000) }
            },
            _ => new Dictionary<string, int>()
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}