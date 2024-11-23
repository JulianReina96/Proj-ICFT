using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("tipo")]
    public class Tipo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Peso { get; set; }
        public int CategoriaId { get; set; }

    }
}
