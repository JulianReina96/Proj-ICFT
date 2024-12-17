using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Frequencia")]
    public class Frequencia
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Peso { get; set; }


    }
}
