using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("Prescricao")]
public partial class Prescricao
{
    [Key]
    public int Id { get; set; }

    public int PacienteID { get; set; }

    public int UsuarioCriacaoID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DataCriacao { get; set; }

    public bool Adesao { get; set; }

    public double ICT { get; set; }

    [ForeignKey("PacienteID")]
    [InverseProperty("Prescricoes")]
    public virtual PacienteICT Paciente { get; set; } = null!;

    [InverseProperty("Prescricao")]
    public virtual ICollection<PrescricaoCID> PrescricaoCIDs { get; set; } = new List<PrescricaoCID>();

    [InverseProperty("Prescricao")]
    public virtual ICollection<PrescricaoMed> PrescricaoMeds { get; set; } = new List<PrescricaoMed>();

    [InverseProperty("Prescricao")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();

    [ForeignKey("UsuarioCriacaoID")]
    [InverseProperty("Prescricoes")]
    public virtual Usuario UsuarioCriacao { get; set; } = null!;
}
