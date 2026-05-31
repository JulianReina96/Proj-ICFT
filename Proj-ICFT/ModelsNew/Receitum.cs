using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

public partial class Receitum
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
    [InverseProperty("Receita")]
    public virtual PacienteICT Paciente { get; set; } = null!;

    [InverseProperty("Receita")]
    public virtual ICollection<ReceitaCID> ReceitaCIDs { get; set; } = new List<ReceitaCID>();

    [InverseProperty("Receita")]
    public virtual ICollection<ReceitaMed> ReceitaMeds { get; set; } = new List<ReceitaMed>();

    [InverseProperty("Receita")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();

    [ForeignKey("UsuarioCriacaoID")]
    [InverseProperty("Receita")]
    public virtual Usuario UsuarioCriacao { get; set; } = null!;
}
