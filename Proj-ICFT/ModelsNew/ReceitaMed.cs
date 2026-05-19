using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("ReceitaMed")]
public partial class ReceitaMed
{
    [Key]
    public int ID { get; set; }

    public int CategoriaID { get; set; }

    public int FrequenciaID { get; set; }

    public int ReceitaID { get; set; }

    public int TipoID { get; set; }

    public int MedicamentoID { get; set; }

    [ForeignKey("CategoriaID")]
    [InverseProperty("ReceitaMeds")]
    public virtual Categorium Categoria { get; set; } = null!;

    [ForeignKey("FrequenciaID")]
    [InverseProperty("ReceitaMeds")]
    public virtual Frequencium Frequencia { get; set; } = null!;

    [InverseProperty("Med_Receita")]
    public virtual ICollection<InstrucoesMed> InstrucoesMeds { get; set; } = new List<InstrucoesMed>();

    [ForeignKey("MedicamentoID")]
    [InverseProperty("ReceitaMeds")]
    public virtual Medicamento Medicamento { get; set; } = null!;

    [ForeignKey("ReceitaID")]
    [InverseProperty("ReceitaMeds")]
    public virtual Receitum Receita { get; set; } = null!;

    [ForeignKey("TipoID")]
    [InverseProperty("ReceitaMeds")]
    public virtual Tipo Tipo { get; set; } = null!;
}
