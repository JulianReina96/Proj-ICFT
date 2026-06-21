using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("PrescricaoMed")]
public partial class PrescricaoMed
{
    [Key]
    public int ID { get; set; }

    public int CategoriaID { get; set; }

    public int FrequenciaID { get; set; }

    public int PrescricaoID { get; set; }

    public int TipoID { get; set; }

    public int MedicamentoID { get; set; }

    [ForeignKey("CategoriaID")]
    [InverseProperty("PrescricaoMeds")]
    public virtual Categorium Categoria { get; set; } = null!;

    [ForeignKey("FrequenciaID")]
    [InverseProperty("PrescricaoMeds")]
    public virtual Frequencium Frequencia { get; set; } = null!;

    [InverseProperty("Med_Prescricao")]
    public virtual ICollection<InstrucoesMed> InstrucoesMeds { get; set; } = new List<InstrucoesMed>();

    [ForeignKey("MedicamentoID")]
    [InverseProperty("PrescricaoMeds")]
    public virtual Medicamento Medicamento { get; set; } = null!;

    [ForeignKey("PrescricaoID")]
    [InverseProperty("PrescricaoMeds")]
    public virtual Prescricao Prescricao { get; set; } = null!;

    [ForeignKey("TipoID")]
    [InverseProperty("PrescricaoMeds")]
    public virtual Tipo Tipo { get; set; } = null!;
}
