using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

public partial class Categorium
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("Categoria")]
    public virtual ICollection<ReceitaMed> ReceitaMeds { get; set; } = new List<ReceitaMed>();

    [InverseProperty("Categoria")]
    public virtual ICollection<Tipo> Tipos { get; set; } = new List<Tipo>();
}
