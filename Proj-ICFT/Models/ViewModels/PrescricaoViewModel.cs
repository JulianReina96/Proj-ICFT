namespace Proj_ICFT.Models.ViewModels
{
    public class PrescricaoViewModel
    {
        public string NomePaciente { get; set; } //caso precise salvar, transformar em entidade
        public List<RemedioViewModel> RemedioList { get; set; }

        public Alerta? Alerta { get; set; }

        public double PesoTotal => RemedioList.Sum(r => r.PesoTotal);


    }
}
