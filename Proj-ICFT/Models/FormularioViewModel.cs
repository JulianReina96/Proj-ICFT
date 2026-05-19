using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models
{
    public class FormularioViewModel
    {
        public List<Blocos_CID> Blocos { get; set; }
        public List<Categorium> Categorias { get; set; }
        public List<Frequencium> Frequencias { get; set; }
        public List<InstrucoesAdicionai> Instrucoes { get; set; }
        public Alerta? Alerta { get; set; }

        public FormularioViewModel(List<Categorium> categorias, List<Frequencium> frequencias,
            List<InstrucoesAdicionai> instrucoes, List<Blocos_CID> blocos)
        {
            Categorias = categorias;
            Frequencias = frequencias;
            Instrucoes = instrucoes;
            Blocos = blocos;
        }
    }
}
