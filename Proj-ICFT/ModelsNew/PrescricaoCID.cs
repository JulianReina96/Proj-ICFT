using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("PrescricaoCID")]
public partial class PrescricaoCID
{
    [Key]
    public int Id { get; set; }

    public int CategoriaCID_ID { get; set; }

    public int PrescricaoID { get; set; }

    [ForeignKey("CategoriaCID_ID")]
    [InverseProperty("PrescricaoCIDs")]
    public virtual Categorias_CID CategoriaCID { get; set; } = null!;

    [ForeignKey("PrescricaoID")]
    [InverseProperty("PrescricaoCIDs")]
    public virtual Prescricao Prescricao { get; set; } = null!;
}
