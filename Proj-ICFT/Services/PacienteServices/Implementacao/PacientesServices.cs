using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Data;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.Services.PacienteServices.Interface;

namespace Proj_ICFT.Services.FormularioServices.Implementacao
{
    public class PacienteServices : IPacienteServices
    {

        private readonly FrequenciaDbContext _frequenciaDbContext;
        private readonly CategoriaDbContext _categoriaDbContext;
        private readonly InstrucoesAdicionaisDbContext _instrucoesAdicionaisDbContext;
        private readonly TipoDbContext _tipoDbContext;
        private readonly PacienteICTDbContext _pacienteDbContext;

        public PacienteServices(FrequenciaDbContext frequenciaDbContext, CategoriaDbContext categoriaDbContext, InstrucoesAdicionaisDbContext instrucoesAdicionaisDbContext, TipoDbContext tipoDbContext, PacienteICTDbContext pacienteDbContext)
        {
            _frequenciaDbContext = frequenciaDbContext;
            _categoriaDbContext = categoriaDbContext;
            _instrucoesAdicionaisDbContext = instrucoesAdicionaisDbContext;
            _tipoDbContext = tipoDbContext;
            _pacienteDbContext = pacienteDbContext;
        }

        public async Task<List<PacienteICT>> listarPacientesICT()
        {          
            return await _pacienteDbContext.Paciente.ToListAsync();
        }



    }
}
