using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Services.PacienteServices.Interface
{
    public interface IPacienteServices
    {
        Task<List<PacienteICT>> listarPacientesICT(string email);
        Task<List<PacienteICT>> listarPacientesComAnonimo(string email);
        Task<PacienteICT> CadastrarPaciente(string email, string nome, int idade, string sexo);
        Task<PacienteICT?> BuscarPaciente(int id, string email);
        Task DeletarPaciente(int id, string email);
        Task<Prescricao?> BuscarPrescricaoCompleta(int prescricaoId, string email);
        Task DeletarPrescricao(int prescricaoId, string email);
        Task<PacienteICT?> BuscarPacienteParaExport(int id, string email);
    }
}
