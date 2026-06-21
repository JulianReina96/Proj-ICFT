using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

public partial class Frequencium
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public double Peso { get; set; }

    [InverseProperty("Frequencia")]
    public virtual ICollection<PrescricaoMed> PrescricaoMeds { get; set; } = new List<PrescricaoMed>();
}
