using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

public partial class InstrucoesAdicionai
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int Peso { get; set; }

    [InverseProperty("Instrucao")]
    public virtual ICollection<InstrucoesMed> InstrucoesMeds { get; set; } = new List<InstrucoesMed>();
}
