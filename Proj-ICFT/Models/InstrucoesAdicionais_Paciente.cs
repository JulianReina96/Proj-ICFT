using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("InstrucoesAdicionais_Paciente")]
    public class InstrucoesAdicionais_Paciente
    {
        public int Id { get; set; }
        public int InstrucaoID { get; set; }
        public int RemedioPacienteID { get; set; }

        public virtual InstrucoesAdicionais Instrucao { get; set; }
        public virtual Remedio_Paciente RemedioPaciente { get; set; }
    }
}
