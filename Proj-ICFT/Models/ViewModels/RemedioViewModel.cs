namespace Proj_ICFT.Models.ViewModels
{
    public class RemedioViewModel
    {

        public Categoria categoria { get; set; }
        public Tipo subcategoria { get; set; }
        public Frequencia frequencia { get; set; }
        public InstrucoesAdicionais instrucoes { get; set; }

        public int PesoTotal => frequencia.Peso + instrucoes.Peso;


        public RemedioViewModel(Categoria categoria, Tipo subcategoria, Frequencia frequencia, InstrucoesAdicionais instrucoes)
        {
            this.categoria = categoria;
            this.subcategoria = subcategoria;
            this.frequencia = frequencia;
            this.instrucoes = instrucoes;
        }





    }
}
