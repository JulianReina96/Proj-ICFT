using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Blocos_CID")]
    public class Blocos_CID
    {

        public int Id { get; set; }
        public string BlockId { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string ChapterNo { get; set; }
        public string Grouping1 { get; set; }
        public string Grouping2 { get; set; }
        


    }
}
