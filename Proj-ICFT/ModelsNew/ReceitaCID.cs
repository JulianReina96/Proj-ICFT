using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("ReceitaCID")]
public partial class ReceitaCID
{
    [Key]
    public int Id { get; set; }

    public int CategoriaCID_ID { get; set; }

    public int ReceitaID { get; set; }

    [ForeignKey("CategoriaCID_ID")]
    [InverseProperty("ReceitaCIDs")]
    public virtual Categorias_CID CategoriaCID { get; set; } = null!;

    [ForeignKey("ReceitaID")]
    [InverseProperty("ReceitaCIDs")]
    public virtual Receitum Receita { get; set; } = null!;
}
