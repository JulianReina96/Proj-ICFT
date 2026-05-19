using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Services.CIDServices.Interface
{
    public interface ICIDServices
    {
        public Task<List<Blocos_CID>> listarBlocosCID();

        public Task<List<Capitulos_CID>> listarCapitulosCID();

        public Task<List<Categorias_CID>> listarCategoriasCID();

        public Task<List<Categorias_CID>> listarCategoriasPorBloco(string blocoId);

        public Task<List<GrupoEnfermidadesViewModel>> agruparEnfermidadesPorBloco();
    }
}