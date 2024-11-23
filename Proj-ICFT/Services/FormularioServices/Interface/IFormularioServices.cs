using Proj_ICFT.Models;

namespace Proj_ICFT.Services.FormularioServices.Interface
{
    public interface IFormularioServices
    {
        public Task<List<Categoria>> listarCategorias();
        public Task<List<Tipo>> listarTipoByCategoriaId(int id);
        public Task<List<Frequencia>> listarFrequencias();
        public Task<List<InstrucoesAdicionais>> listarInstrucoesAdicionais();
        public Task<List<RemedioViewModel>> converterMedicamentos(List<MedicamentosExportacaoModel> listaModel);
    }
}
