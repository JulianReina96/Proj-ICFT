namespace Proj_ICFT.Models
{
    public class SalvarPrescricaoRequest
    {
        public int? PacienteId { get; set; }
        public bool PacienteAnonimo { get; set; }
        public bool Adesao { get; set; }
        public List<MedicamentoPrescricaoItem> Medicamentos { get; set; } = [];
        public List<int> CidCategorias { get; set; } = [];
    }

    public class MedicamentoPrescricaoItem
    {
        public int MedicamentoId { get; set; }
        public int CategoriaId { get; set; }
        public int SubcategoriaId { get; set; }
        public int FrequenciaId { get; set; }
        public List<int> InstrucoesAdicionais { get; set; } = [];
    }
}
