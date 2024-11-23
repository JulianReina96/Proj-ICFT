namespace Proj_ICFT.Models
{
    public class ReceitaViewModel
    {
        public string NomePaciente { get; set; } //caso precise salvar, transformar em entidade
        public List<RemedioViewModel> RemedioList { get; set; }
        
        public int PesoTotal { get; set; }


    }
}
