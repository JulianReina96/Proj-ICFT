using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Categorias_CID")]
    public class Categorias_CID
    {

        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string BlockID { get; set; }
        public string ChapterNo { get; set; }
        public bool IsLef { get; set; }
                


    }
}
