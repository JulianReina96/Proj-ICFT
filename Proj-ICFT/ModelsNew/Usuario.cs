using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

[Index("Usuario1", Name = "IX_Usuarios", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int id { get; set; }

    [Column("Usuario")]
    [StringLength(100)]
    public string Usuario1 { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime DataCriacao { get; set; }

    [InverseProperty("UsuarioCriacao")]
    public virtual ICollection<PacienteICT> PacienteICTs { get; set; } = new List<PacienteICT>();

    [InverseProperty("UsuarioCriacao")]
    public virtual ICollection<Receitum> Receita { get; set; } = new List<Receitum>();
}
