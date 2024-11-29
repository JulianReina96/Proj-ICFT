using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;

namespace Proj_ICFT.Services.PacienteServices.Interface
{
    public interface IPacienteServices
    {
        public Task<List<PacienteICT>> listarPacientesICT();
        public void salvarPaciente(PacienteICT paciente, List<MedicamentosExportacaoModel> remedios);
        //public Task<PacienteICT> buscarPacienteICT(int id);
        //public Task<PacienteICT> salvarPacienteICT(PacienteICT paciente);
        //public Task<PacienteICT> atualizarPacienteICT(PacienteICT paciente);
        //public Task<PacienteICT> deletarPacienteICT(int id);

    }
}
