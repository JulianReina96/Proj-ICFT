using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Proj_ICFT.ModelsNew;

public partial class Medicamento
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string NOME_PRODUTO { get; set; } = null!;

    public string? PRINCIPIO_ATIVO { get; set; }

    [StringLength(100)]
    public string? CATEGORIA_REGULATORIA { get; set; }

    [StringLength(500)]
    public string? CLASSE_TERAPEUTICA { get; set; }

    [StringLength(50)]
    public string? NUMERO_REGISTRO_PRODUTO { get; set; }

    [StringLength(255)]
    public string? EMPRESA_DETENTORA_REGISTRO { get; set; }

    [StringLength(100)]
    public string? SITUACAO_REGISTRO { get; set; }

    [InverseProperty("Medicamento")]
    public virtual ICollection<PrescricaoMed> PrescricaoMeds { get; set; } = new List<PrescricaoMed>();
}
