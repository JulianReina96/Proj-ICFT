namespace Proj_ICFT.Models
{
    public class ICTPaciente
    {
        public string NomePaciente { get; set; }
        public int ICTotal { get; set; }


        public ICTPaciente(string nomePaciente, int icTotal)
        {
            NomePaciente = nomePaciente;
            ICTotal = icTotal;
        }
    }
}
