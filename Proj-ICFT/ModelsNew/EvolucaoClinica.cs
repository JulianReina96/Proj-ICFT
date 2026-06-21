using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.ModelsNew;

[Table("EvolucaoClinica")]
public partial class EvolucaoClinica
{
    [Key]
    public int Id { get; set; }

    // Relações estruturais
    public int PacienteID { get; set; }
    public int UsuarioCriacaoID { get; set; }
    public int? PrescricaoID { get; set; }
    public int? CategoriaCID_ID { get; set; }

    // Temporal
    [Column(TypeName = "datetime")]
    public DateTime DataConsulta { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DataCriacao { get; set; }

    // SOAP — texto livre
    [StringLength(2000)] public string? Subjetivo { get; set; }
    [StringLength(2000)] public string? Avaliacao { get; set; }
    [StringLength(2000)] public string? Plano { get; set; }
    [StringLength(2000)] public string? ObservacaoObjetivo { get; set; }

    // Desfecho categórico
    public StatusEvolucao Status { get; set; }

    // OBJETIVO — Sinais vitais (opcionais)
    public int?    PaSistolica       { get; set; }
    public int?    PaDiastolica      { get; set; }
    public int?    FrequenciaCardiaca{ get; set; }
    public double? Peso              { get; set; }
    public int?    Spo2              { get; set; }

    // OBJETIVO — Laboratoriais (opcionais)
    public double? Glicemia   { get; set; }
    public double? Hba1c      { get; set; }
    public double? Creatinina { get; set; }

    // OBJETIVO — Contadores de impacto (default 0)
    public int EventosAdversos { get; set; }
    public int Hospitalizacoes { get; set; }
    public int IdasEmergencia  { get; set; }

    // Navegação
    [ForeignKey("PacienteID")]
    public virtual PacienteICT Paciente { get; set; } = null!;

    [ForeignKey("UsuarioCriacaoID")]
    public virtual Usuario UsuarioCriacao { get; set; } = null!;

    [ForeignKey("PrescricaoID")]
    public virtual Prescricao? Prescricao { get; set; }

    [ForeignKey("CategoriaCID_ID")]
    public virtual Categorias_CID? CategoriaCID { get; set; }
}
