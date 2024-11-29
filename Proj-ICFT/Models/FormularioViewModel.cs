namespace Proj_ICFT.Models
{
    public class FormularioViewModel
    {
        public List<Categoria> categorias {  get; set; }
        public List<Frequencia> frequencias { get; set; }

        public List<InstrucoesAdicionais> instrucoes { get; set; }

        public Alerta? Alerta { get; set; }

        public FormularioViewModel(List<Categoria> categorias, List<Frequencia> frequencias, List<InstrucoesAdicionais> instrucoes)
        {
            this.categorias = categorias;
            this.frequencias = frequencias;
            this.instrucoes = instrucoes;
        }
                
    }
}
