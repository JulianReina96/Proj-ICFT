using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Services.FormularioServices.Interface
{
    public interface IFormularioServices
    {
        Task<List<Categorium>> listarCategorias();
        Task<List<Tipo>> listarTipoByCategoriaId(int id);
        Task<List<Frequencium>> listarFrequencias();
        Task<List<InstrucoesAdicionai>> listarInstrucoesAdicionais();
        Task<List<RemedioViewModel>> converterMedicamentos(List<MedicamentosExportacaoModel> listaModel);
        Task<double> SalvarReceita(string email, SalvarReceitaRequest request);
    }

}

