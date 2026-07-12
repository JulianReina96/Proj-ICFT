using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("Tipo")]
public partial class Tipo
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public int CategoriaId { get; set; }

    public int Peso { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("Tipos")]
    public virtual Categorium Categoria { get; set; } = null!;

    [InverseProperty("Tipo")]
    public virtual ICollection<PrescricaoMed> PrescricaoMeds { get; set; } = new List<PrescricaoMed>();
}
