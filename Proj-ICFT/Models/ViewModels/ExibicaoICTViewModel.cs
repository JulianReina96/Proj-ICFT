namespace Proj_ICFT.Models.ViewModels
{
    public class ExibicaoICTViewModel
    {
        public List<PacienteICT>? PacienteICT { get; set; }
        public int? Pagina { get; set; }
        public string? Ordenacao { get; set; }
        public bool deleteClicked { get; set; }

        public Alerta? Alerta { get; set; }

        public int id { get; set; }
    }
}
