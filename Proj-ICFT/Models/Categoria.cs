using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Categoria")]
    public class Categoria
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Tipo> tipos { get; set; }

        public Categoria()
        {
            tipos = new List<Tipo>();
        }


    }
}
