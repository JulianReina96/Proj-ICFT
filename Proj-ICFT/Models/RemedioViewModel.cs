namespace Proj_ICFT.Models
{
    public class RemedioViewModel
    {

        public Categoria categoria { get; set; }
        public Tipo subcategoria { get; set; }
        public Frequencia frequencia { get; set; }
        public List<InstrucoesAdicionais> instrucoes { get; set; }

        public int PesoTotal =>  frequencia.Peso + instrucoes.Sum(i => i.Peso);


        public RemedioViewModel(Categoria categoria, Tipo subcategoria, Frequencia frequencia, List<InstrucoesAdicionais> instrucoes )
        {
            this.categoria = categoria;
            this.subcategoria = subcategoria;
            this.frequencia = frequencia;
            this.instrucoes = instrucoes;
        }





    }
}
