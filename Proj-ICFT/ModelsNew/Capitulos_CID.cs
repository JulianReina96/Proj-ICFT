using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("Capitulos_CID")]
[Index("ChapterNo", Name = "UQ_Chapters_ChapterNo", IsUnique = true)]
public partial class Capitulos_CID
{
    [Key]
    public int Id { get; set; }

    [StringLength(10)]
    public string ChapterNo { get; set; } = null!;

    [StringLength(500)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? TitleEN { get; set; }

    [StringLength(255)]
    public string? FoundationURI { get; set; }

    [StringLength(255)]
    public string? LinearizationURI { get; set; }

    [InverseProperty("ChapterNoNavigation")]
    public virtual ICollection<Blocos_CID> Blocos_CIDs { get; set; } = new List<Blocos_CID>();

    [InverseProperty("ChapterNoNavigation")]
    public virtual ICollection<Categorias_CID> Categorias_CIDs { get; set; } = new List<Categorias_CID>();
}
