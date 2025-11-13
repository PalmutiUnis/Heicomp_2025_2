namespace Heicomp_2025_2.Models.Colaboradores
{
    public class ColaboradorResumoModel
    {
        public string Nome { get; set; }
        public string Setor { get; set; }
        public string Cargo { get; set; }
        public string Status { get; set; }

        // Para exibição "RH ° Analista"
        public string SetorCargo => $"{Setor} ° {Cargo}";
    }
}
