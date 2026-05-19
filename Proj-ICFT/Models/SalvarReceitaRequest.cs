namespace Proj_ICFT.Models
{
    public class SalvarReceitaRequest
    {
        public int? PacienteId { get; set; }
        public bool PacienteAnonimo { get; set; }
        public bool Adesao { get; set; }
        public List<MedicamentoReceitaItem> Medicamentos { get; set; } = [];
        public List<int> CidCategorias { get; set; } = [];
    }

    public class MedicamentoReceitaItem
    {
        public int MedicamentoId { get; set; }
        public int CategoriaId { get; set; }
        public int SubcategoriaId { get; set; }
        public int FrequenciaId { get; set; }
        public List<int> InstrucoesAdicionais { get; set; } = [];
    }
}
