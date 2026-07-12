using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("Blocos_CID")]
[Index("BlockId", Name = "UQ_Blocks_BlockId", IsUnique = true)]
public partial class Blocos_CID
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? BlockId { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? TitleEN { get; set; }

    [StringLength(10)]
    public string? ChapterNo { get; set; }

    [StringLength(100)]
    public string? Grouping1 { get; set; }

    [StringLength(100)]
    public string? Grouping2 { get; set; }

    [ForeignKey("ChapterNo")]
    [InverseProperty("Blocos_CIDs")]
    public virtual Capitulos_CID? ChapterNoNavigation { get; set; }
}
