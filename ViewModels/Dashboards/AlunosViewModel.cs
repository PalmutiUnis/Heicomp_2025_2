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


namespace Heicomp_2025_2.ViewModels.Dashboards;

/// <summary>
/// ViewModel para o Dashboard de Alunos com sistema de Drill-Down em 5 níveis:
/// Campus → Modalidade → Curso → Turno → Período
/// </summary>
public class AlunosViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ========================================
    // CONTROLE DE NÍVEIS DE DRILL-DOWN
    // ========================================

    /// <summary>
    /// Controla qual nível de drill-down está ativo (1 a 5)
    /// Nível 1: Visão geral
    /// Nível 2: Campus selecionado
    /// Nível 3: Modalidade selecionada
    /// Nível 4: Curso selecionado
    /// Nível 5: Turno selecionado
    /// Nível 6: Período selecionado
    /// </summary>
    private int _nivelAtual = 1;
    public int NivelAtual
    {
        get => _nivelAtual;
        set
        {
            _nivelAtual = value;
            OnPropertyChanged();
            AtualizarVisibilidadeFiltros();
            AtualizarVisibilidadeGraficos();
            AtualizarDados();
        }
    }

    // ========================================
    // VISIBILIDADE DOS FILTROS
    // ========================================

    private bool _mostrarCampus = true;
    public bool MostrarCampus
    {
        get => _mostrarCampus;
        set { _mostrarCampus = value; OnPropertyChanged(); }
    }

    private bool _mostrarModalidade;
    public bool MostrarModalidade
    {
        get => _mostrarModalidade;
        set { _mostrarModalidade = value; OnPropertyChanged(); }
    }

    private bool _mostrarCurso;
    public bool MostrarCurso
    {
        get => _mostrarCurso;
        set { _mostrarCurso = value; OnPropertyChanged(); }
    }

    private bool _mostrarTurno;
    public bool MostrarTurno
    {
        get => _mostrarTurno;
        set { _mostrarTurno = value; OnPropertyChanged(); }
    }

    private bool _mostrarPeriodo;
    public bool MostrarPeriodo
    {
        get => _mostrarPeriodo;
        set { _mostrarPeriodo = value; OnPropertyChanged(); }
    }

    // ========================================
    // LISTAS DE FILTROS
    // ========================================

    public ObservableCollection<string> ListaCampus { get; set; }
    public ObservableCollection<string> ListaModalidades { get; set; }
    public ObservableCollection<string> ListaCursos { get; set; }
    public ObservableCollection<string> ListaTurnos { get; set; }
    public ObservableCollection<string> ListaPeriodos { get; set; }

    // ========================================
    // VALORES SELECIONADOS
    // ========================================

    private string? _campusSelecionado;
    public string? CampusSelecionado
    {
        get => _campusSelecionado;
        set
        {
            _campusSelecionado = value;
            OnPropertyChanged();
            if (value != null && NivelAtual < 2) NivelAtual = 2;
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
            if (value != null && NivelAtual < 3) NivelAtual = 3;
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
            if (value != null && NivelAtual < 4) NivelAtual = 4;
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
            if (value != null && NivelAtual < 5) NivelAtual = 5;
        }
    }

    private string? _periodoSelecionado;
    public string? PeriodoSelecionado
    {
        get => _periodoSelecionado;
        set
        {
            _periodoSelecionado = value;
            OnPropertyChanged();
            if (value != null && NivelAtual < 6) NivelAtual = 6;
        }
    }

    // ========================================
    // DADOS DOS CARDS
    // ========================================

    /// <summary>
    /// Total de alunos (muda conforme filtros)
    /// </summary>
    private int _totalAlunos;
    public int TotalAlunos
    {
        get => _totalAlunos;
        set { _totalAlunos = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Nome da modalidade mais comum
    /// </summary>
    private string _modalidadeMaisComum = "";
    public string ModalidadeMaisComum
    {
        get => _modalidadeMaisComum;
        set { _modalidadeMaisComum = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Nome do curso com mais alunos
    /// </summary>
    private string _cursoComMaisAlunos = "";
    public string CursoComMaisAlunos
    {
        get => _cursoComMaisAlunos;
        set { _cursoComMaisAlunos = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Número de alunos no curso com mais alunos
    /// </summary>
    private int _numeroAlunosCursoTop;
    public int NumeroAlunosCursoTop
    {
        get => _numeroAlunosCursoTop;
        set { _numeroAlunosCursoTop = value; OnPropertyChanged(); }
    }

    // ========================================
    // VISIBILIDADE DOS GRÁFICOS
    // ========================================

    private bool _mostrarGraficoCampus = true;
    public bool MostrarGraficoCampus
    {
        get => _mostrarGraficoCampus;
        set { _mostrarGraficoCampus = value; OnPropertyChanged(); }
    }

    private bool _mostrarGraficoModalidades = true;
    public bool MostrarGraficoModalidades
    {
        get => _mostrarGraficoModalidades;
        set { _mostrarGraficoModalidades = value; OnPropertyChanged(); }
    }

    private bool _mostrarGraficoCursos = true;
    public bool MostrarGraficoCursos
    {
        get => _mostrarGraficoCursos;
        set { _mostrarGraficoCursos = value; OnPropertyChanged(); }
    }

    private bool _mostrarGraficoTurnos = true;
    public bool MostrarGraficoTurnos
    {
        get => _mostrarGraficoTurnos;
        set { _mostrarGraficoTurnos = value; OnPropertyChanged(); }
    }

    private bool _mostrarGraficoPeriodos = true;
    public bool MostrarGraficoPeriodos
    {
        get => _mostrarGraficoPeriodos;
        set { _mostrarGraficoPeriodos = value; OnPropertyChanged(); }
    }

    // ========================================
    // DADOS DOS GRÁFICOS
    // ========================================

    /// <summary>
    /// Dados para o gráfico de total por campus (barras horizontais)
    /// </summary>
    private Dictionary<string, int>? _dadosGraficoCampus;
    public Dictionary<string, int>? DadosGraficoCampus
    {
        get => _dadosGraficoCampus;
        set { _dadosGraficoCampus = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Dados para o gráfico de modalidades (rosca)
    /// </summary>
    private Dictionary<string, int>? _dadosGraficoModalidades;
    public Dictionary<string, int>? DadosGraficoModalidades
    {
        get => _dadosGraficoModalidades;
        set { _dadosGraficoModalidades = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Dados para o gráfico de cursos (barras verticais)
    /// </summary>
    private Dictionary<string, int>? _dadosGraficoCursos;
    public Dictionary<string, int>? DadosGraficoCursos
    {
        get => _dadosGraficoCursos;
        set { _dadosGraficoCursos = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Dados para o gráfico de turnos (rosca)
    /// </summary>
    private Dictionary<string, int>? _dadosGraficoTurnos;
    public Dictionary<string, int>? DadosGraficoTurnos
    {
        get => _dadosGraficoTurnos;
        set { _dadosGraficoTurnos = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Dados para o gráfico de períodos (barras verticais)
    /// </summary>
    private Dictionary<string, int>? _dadosGraficoPeriodos;
    public Dictionary<string, int>? DadosGraficoPeriodos
    {
        get => _dadosGraficoPeriodos;
        set { _dadosGraficoPeriodos = value; OnPropertyChanged(); }
    }

    // ========================================
    // CONSTRUTOR
    // ========================================

    public AlunosViewModel()
    {
        // Inicializa listas de filtros com dados fixos
        ListaCampus = new ObservableCollection<string>
        {
            "Campus Varginha",
            "Campus Três Pontas",
            "Campus Poços de Caldas",
            "Campus São Lourenço"
        };

        ListaModalidades = new ObservableCollection<string>
        {
            "Presencial",
            "EAD",
            "Híbrido"
        };

        ListaCursos = new ObservableCollection<string>
        {
            "Informática",
            "Administração",
            "Enfermagem",
            "Engenharia Civil",
            "Direito",
            "Pedagogia"
        };

        ListaTurnos = new ObservableCollection<string>
        {
            "Manhã",
            "Tarde",
            "Noite"
        };

        ListaPeriodos = new ObservableCollection<string>
        {
            "1º Período",
            "2º Período",
            "3º Período",
            "4º Período",
            "5º Período",
            "6º Período",
            "7º Período",
            "8º Período"
        };

        // Carrega dados iniciais
        AtualizarDados();
    }

    // ========================================
    // COMANDO PARA LIMPAR FILTROS
    // ========================================

    /// <summary>
    /// Reseta todos os filtros e volta para a visão geral (Nível 1)
    /// </summary>
    public void LimparFiltros()
    {
        CampusSelecionado = null;
        ModalidadeSelecionada = null;
        CursoSelecionado = null;
        TurnoSelecionado = null;
        PeriodoSelecionado = null;
        NivelAtual = 1;
    }

    // ========================================
    // LÓGICA DE VISIBILIDADE DOS FILTROS
    // ========================================

    /// <summary>
    /// Controla quais filtros devem aparecer baseado no nível atual
    /// </summary>
    private void AtualizarVisibilidadeFiltros()
    {
        MostrarCampus = NivelAtual >= 1;
        MostrarModalidade = NivelAtual >= 2;
        MostrarCurso = NivelAtual >= 3;
        MostrarTurno = NivelAtual >= 4;
        MostrarPeriodo = NivelAtual >= 5;
    }

    // ========================================
    // LÓGICA DE VISIBILIDADE DOS GRÁFICOS
    // ========================================

    /// <summary>
    /// Controla quais gráficos devem aparecer baseado nos filtros selecionados
    /// </summary>
    private void AtualizarVisibilidadeGraficos()
    {
        // Gráfico de campus: some quando um campus é selecionado
        MostrarGraficoCampus = string.IsNullOrEmpty(CampusSelecionado);

        // Gráfico de modalidades: some quando uma modalidade é selecionada
        MostrarGraficoModalidades = string.IsNullOrEmpty(ModalidadeSelecionada);

        // Gráfico de cursos: some quando um curso é selecionado
        MostrarGraficoCursos = string.IsNullOrEmpty(CursoSelecionado);

        // Gráfico de turnos: some quando um turno é selecionado
        MostrarGraficoTurnos = string.IsNullOrEmpty(TurnoSelecionado);

        // Gráfico de períodos: sempre visível até o último nível
        MostrarGraficoPeriodos = true;
    }

    // ========================================
    // ATUALIZAÇÃO DE DADOS (MOCK - VALORES FIXOS)
    // ========================================

    /// <summary>
    /// Atualiza todos os dados dos cards e gráficos com base nos filtros selecionados
    /// </summary>
    private void AtualizarDados()
    {
        // ========== DADOS DOS CARDS ==========

        if (string.IsNullOrEmpty(CampusSelecionado))
        {
            // Visão geral (sem filtros)
            TotalAlunos = 12847;
            ModalidadeMaisComum = "Presencial";
            CursoComMaisAlunos = "Administração";
            NumeroAlunosCursoTop = 2340;
        }
        else if (string.IsNullOrEmpty(ModalidadeSelecionada))
        {
            // Campus selecionado
            TotalAlunos = 5432; // Total deste campus
            ModalidadeMaisComum = "Presencial";
            CursoComMaisAlunos = "Informática";
            NumeroAlunosCursoTop = 890;
        }
        else if (string.IsNullOrEmpty(CursoSelecionado))
        {
            // Campus + Modalidade selecionados
            TotalAlunos = 3210;
            ModalidadeMaisComum = ModalidadeSelecionada ?? "";
            CursoComMaisAlunos = "Enfermagem";
            NumeroAlunosCursoTop = 560;
        }
        else if (string.IsNullOrEmpty(TurnoSelecionado))
        {
            // Campus + Modalidade + Curso selecionados
            TotalAlunos = 890;
            ModalidadeMaisComum = ModalidadeSelecionada ?? "";
            CursoComMaisAlunos = CursoSelecionado ?? "";
            NumeroAlunosCursoTop = 890;
        }
        else if (string.IsNullOrEmpty(PeriodoSelecionado))
        {
            // Campus + Modalidade + Curso + Turno selecionados
            TotalAlunos = 320;
            ModalidadeMaisComum = ModalidadeSelecionada ?? "";
            CursoComMaisAlunos = CursoSelecionado ?? "";
            NumeroAlunosCursoTop = 320;
        }
        else
        {
            // Todos os filtros selecionados (período específico)
            TotalAlunos = 45;
            ModalidadeMaisComum = ModalidadeSelecionada ?? "";
            CursoComMaisAlunos = CursoSelecionado ?? "";
            NumeroAlunosCursoTop = 45;
        }

        // ========== DADOS DOS GRÁFICOS ==========

        // Gráfico 1: Total por Campus (barras horizontais)
        if (MostrarGraficoCampus)
        {
            DadosGraficoCampus = new Dictionary<string, int>
            {
                { "Varginha", 5432 },
                { "Três Pontas", 3821 },
                { "Poços de Caldas", 2104 },
                { "São Lourenço", 1490 }
            };
        }

        // Gráfico 2: Total por Modalidade (rosca)
        if (MostrarGraficoModalidades)
        {
            if (string.IsNullOrEmpty(CampusSelecionado))
            {
                // Dados gerais
                DadosGraficoModalidades = new Dictionary<string, int>
                {
                    { "Presencial", 7200 },
                    { "EAD", 3450 },
                    { "Híbrido", 2197 }
                };
            }
            else
            {
                // Dados filtrados por campus
                DadosGraficoModalidades = new Dictionary<string, int>
                {
                    { "Presencial", 3210 },
                    { "EAD", 1550 },
                    { "Híbrido", 672 }
                };
            }
        }

        // Gráfico 3: Total por Curso (barras verticais)
        if (MostrarGraficoCursos)
        {
            if (string.IsNullOrEmpty(CampusSelecionado))
            {
                // Top 6 cursos geral
                DadosGraficoCursos = new Dictionary<string, int>
                {
                    { "Administração", 2340 },
                    { "Informática", 2100 },
                    { "Enfermagem", 1980 },
                    { "Eng. Civil", 1750 },
                    { "Direito", 1560 },
                    { "Pedagogia", 1340 }
                };
            }
            else
            {
                // Cursos do campus selecionado
                DadosGraficoCursos = new Dictionary<string, int>
                {
                    { "Informática", 890 },
                    { "Administração", 780 },
                    { "Enfermagem", 650 },
                    { "Direito", 540 }
                };
            }
        }

        // Gráfico 4: Quantidade por Turno (rosca)
        if (MostrarGraficoTurnos)
        {
            DadosGraficoTurnos = new Dictionary<string, int>
            {
                { "Manhã", 4320 },
                { "Tarde", 3890 },
                { "Noite", 4637 }
            };
        }

        // Gráfico 5: Distribuição por Período (barras verticais)
        if (MostrarGraficoPeriodos)
        {
            DadosGraficoPeriodos = new Dictionary<string, int>
            {
                { "1º", 1890 },
                { "2º", 1750 },
                { "3º", 1620 },
                { "4º", 1540 },
                { "5º", 1320 },
                { "6º", 1180 },
                { "7º", 980 },
                { "8º", 867 }
            };
        }
    }

    // ========================================
    // NOTIFICAÇÃO DE MUDANÇAS
    // ========================================

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}