using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Remedio_Paciente")]
    public class Remedio_Paciente
    {

        public int ID { get; set; }
        public int PacienteID { get; set; }
        public int CategoriaId { get; set; }
        public int TipoId { get; set; }
        //public int? InstrucaoId  { get; set; }
        public int FrequenciaId { get; set; }


        public Remedio_Paciente(int pacienteID, int categoriaId, int tipoId, int frequenciaId)
        {
            PacienteID = pacienteID;
            CategoriaId = categoriaId;
            TipoId = tipoId;           
            FrequenciaId = frequenciaId;
        }

        public virtual PacienteICT Paciente { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual Tipo Tipo { get; set; }
        public virtual ICollection<InstrucoesAdicionais_Paciente> InstrucoesAdicionais_Paciente { get; set; } = new List<InstrucoesAdicionais_Paciente>();
        //public virtual InstrucoesAdicionais Instrucao { get; set; }
        public virtual Frequencia Frequencia { get; set; }

    }
}
