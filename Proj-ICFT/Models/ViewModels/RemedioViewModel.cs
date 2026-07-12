using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models.ViewModels
{
    public class RemedioViewModel
    {

        public Categorium categoria { get; set; }
        public Tipo subcategoria { get; set; }
        public Frequencium frequencia { get; set; }
        public List<InstrucoesAdicionai> instrucoes { get; set; }
        public double PesoTotal => frequencia.Peso + instrucoes.Sum(i => i.Peso);


        public RemedioViewModel(Categorium categoria, Tipo subcategoria, Frequencium frequencia, List<InstrucoesAdicionai> instrucoes)
        {
            this.categoria = categoria;
            this.subcategoria = subcategoria;
            this.frequencia = frequencia;
            this.instrucoes = instrucoes;
        }





    }
}
