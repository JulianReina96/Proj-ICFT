using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Table("PacienteICT")]
public partial class PacienteICT
{
    [Key]
    public int ID { get; set; }

    [StringLength(100)]
    public string NomePaciente { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime DataCriacao { get; set; }

    public int UsuarioCriacaoID { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Sexo { get; set; } = null!;

    public int Idade { get; set; }

    [InverseProperty("Paciente")]
    public virtual ICollection<Receitum> Receita { get; set; } = new List<Receitum>();

    [ForeignKey("UsuarioCriacaoID")]
    [InverseProperty("PacienteICTs")]
    public virtual Usuario UsuarioCriacao { get; set; } = null!;

    public PacienteICT(string nomePaciente, DateTime dataCriacao, int usuarioCriacaoID, string sexo, int idade)
    {
        NomePaciente = nomePaciente;
        DataCriacao = dataCriacao;
        UsuarioCriacaoID = usuarioCriacaoID;
        Sexo = sexo;
        Idade = idade;
    }
}
