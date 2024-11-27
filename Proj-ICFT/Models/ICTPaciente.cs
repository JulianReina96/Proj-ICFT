using Proj_ICFT.Models.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("PacienteICT")]
    public class PacienteICT
    {
        public int ID { get; set; }
        public string NomePaciente { get; set; }
        public int ICT { get; set; }

        public DateTime dataCriacao { get; set; }
        
        public virtual ICollection<RemedioViewModel> Medicamentos { get; set; } = new List<RemedioViewModel>();


        public PacienteICT(string nomePaciente, int icTotal)
        {
            NomePaciente = nomePaciente;
            ICT = icTotal;
        }

        public PacienteICT() { }
    }
}
