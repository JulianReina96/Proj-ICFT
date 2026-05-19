using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("Categorias_CID")]


//DOENÇAS
public partial class Categorias_CID
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? TitleEN { get; set; }

    [StringLength(50)]
    public string? BlockId { get; set; }

    [StringLength(10)]
    public string ChapterNo { get; set; } = null!;

    public bool? IsLeaf { get; set; }

    [ForeignKey("ChapterNo")]
    [InverseProperty("Categorias_CIDs")]
    public virtual Capitulos_CID ChapterNoNavigation { get; set; } = null!;

    [InverseProperty("CategoriaCID")]
    public virtual ICollection<ReceitaCID> ReceitaCIDs { get; set; } = new List<ReceitaCID>();
}
