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
        Task<(double ict, int prescricaoId)> SalvarPrescricao(string email, SalvarPrescricaoRequest request);
        Task<(double ict, int prescricaoId)> AtualizarPrescricao(string email, int prescricaoId, SalvarPrescricaoRequest request);
    }

}

