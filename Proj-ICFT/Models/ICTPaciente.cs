using Proj_ICFT.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("PacienteICT")]
    public class PacienteICT
    {
        public int ID { get; set; }
        public string NomePaciente { get; set; }
        public double ICT { get; set; }
        public string? UsuarioCriacao { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime dataCriacao { get; set; }
        
        public virtual List<Remedio_Paciente> Remedio_Paciente { get; set; }


        public PacienteICT(string nomePaciente, double icTotal, string usuario)
        {
            NomePaciente = nomePaciente;
            ICT = icTotal;
            UsuarioCriacao = usuario;
        }

        public PacienteICT() { }
    }
}
