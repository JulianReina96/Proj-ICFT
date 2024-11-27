using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("PacienteICT")]
    public class PacienteICT
    {
        public string NomePaciente { get; set; }
        public int ICT { get; set; }

        public DateTime dataCriacao { get; set; }


        public PacienteICT(string nomePaciente, int icTotal)
        {
            NomePaciente = nomePaciente;
            ICT = icTotal;
        }
    }
}
