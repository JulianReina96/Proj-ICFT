using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Usuarios")]
    public class Usuarios
    {
        public int id { get; set; }

        public string Usuario { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
