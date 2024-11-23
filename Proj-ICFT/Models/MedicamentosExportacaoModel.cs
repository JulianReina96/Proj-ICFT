namespace Proj_ICFT.Models
{
    public class MedicamentosExportacaoModel
    {
        public int? categoria { get; set; }
        public int? subcategoria { get; set; }
        public int? frequencia { get; set; }

        public List<int>? instrucoesAdicionais { get; set; }

    }
}
