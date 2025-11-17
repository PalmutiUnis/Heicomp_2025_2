namespace MauiApp1.Models
{
    /// <summary>
    /// Dados gerais do dashboard de diversidade
    /// </summary>
    public class DiversidadeGeral
    {
        public int TotalColaboradores { get; set; }
        public decimal PercentualHomens { get; set; }
        public decimal PercentualMulheres { get; set; }
        public decimal PercentualNaoInformado { get; set; }

        // Propriedades formatadas para exibição
        public string TotalColaboradoresFormatado => TotalColaboradores.ToString("#,##0");
        public string PercentualHomensFormatado => $"{PercentualHomens:F1}%";
        public string PercentualMulheresFormatado => $"{PercentualMulheres:F1}%";
        public string PercentualNaoInformadoFormatado => $"{PercentualNaoInformado:F1}%";
    }

    /// <summary>
    /// Distribuição por gênero
    /// </summary>
    public class DistribuicaoGenero
    {
        public string Sexo { get; set; }
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }

        public string Descricao => $"{Sexo}: {Quantidade} ({Percentual:F1}%)";
    }

    /// <summary>
    /// Distribuição de Pessoas com Deficiência
    /// </summary>
    public class DistribuicaoPCD
    {
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }

        public string DescricaoCompleta => $"{Descricao}: {Quantidade} ({Percentual:F1}%)";
    }

    /// <summary>
    /// Distribuição por Raça/Etnia
    /// </summary>
    public class DistribuicaoRacaEtnia
    {
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }

        public string DescricaoCompleta => $"{Descricao}: {Quantidade} ({Percentual:F1}%)";

        // Para criar barras visuais proporcionais ao percentual
        public double LarguraBarra => (double)Percentual * 4; // Escala visual
    }

    /// <summary>
    /// Distribuição por Estado Civil
    /// </summary>
    public class DistribuicaoEstadoCivil
    {
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }

        public string DescricaoCompleta => $"{Descricao}: {Quantidade} ({Percentual:F1}%)";
    }
}