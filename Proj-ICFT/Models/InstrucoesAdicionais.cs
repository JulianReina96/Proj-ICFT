using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("InstrucoesAdicionais")]
    public class InstrucoesAdicionais
    {
        public int id { get; set; }
        public string name { get; set; }
        public int Peso { get; set; }

    }
}
