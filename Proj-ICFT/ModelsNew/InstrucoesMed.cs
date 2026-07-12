using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("InstrucoesMed")]
public partial class InstrucoesMed
{
    [Key]
    public int id { get; set; }

    public int Med_PrescricaoID { get; set; }

    public int InstrucaoId { get; set; }

    [ForeignKey("InstrucaoId")]
    [InverseProperty("InstrucoesMeds")]
    public virtual InstrucoesAdicionai Instrucao { get; set; } = null!;

    [ForeignKey("Med_PrescricaoID")]
    [InverseProperty("InstrucoesMeds")]
    public virtual PrescricaoMed Med_Prescricao { get; set; } = null!;
}
