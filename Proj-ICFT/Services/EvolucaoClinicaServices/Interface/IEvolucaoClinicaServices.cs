using Proj_ICFT.Models.Request;
using Proj_ICFT.Models.ViewModels;

namespace Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

public interface IEvolucaoClinicaServices
{
    Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string emailUsuario);
    Task<EvolucaoDetalheViewModel?> Detalhar(int id, string emailUsuario);
    Task<int> Salvar(SalvarEvolucaoRequest request, string emailUsuario);
    Task Atualizar(int id, SalvarEvolucaoRequest request, string emailUsuario);
    Task Deletar(int id, string emailUsuario);
    Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string emailUsuario);
    Task<List<ReceitaOpcaoViewModel>> ListarReceitasDoPaciente(int pacienteId, string emailUsuario);
    Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string emailUsuario);
}
