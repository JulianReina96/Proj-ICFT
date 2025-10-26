using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Capitulos_CID")]
    public class Capitulos_CID
    {

        public int Id { get; set; }
        public string ChapterNo { get; set; }
        public string Title { get; set; }
        public string TitleEn { get; set; }
        public string FountationURI { get; set; }
        public string LinearizationURI { get; set; }


    }
}
