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

        public async Task<List<PacienteICT>> listarPacientesICT(string email)
        {
            try
            {

            return await _pacienteDbContext.Paciente.Include(r=> r.Remedio_Paciente).Where(r=> r.UsuarioCriacao == email).ToListAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void salvarPaciente(PacienteICT paciente, List<MedicamentosExportacaoModel> remedios)
        {


            //paciente.dataCriacao = DateTime.Now;   
            //paciente.Remedio_Paciente = new List<Remedio_Paciente>();
            //foreach (var remedio in remedios)
            //{ //mudar para instrucao nao ser mais lista
            //    paciente.Remedio_Paciente.Add(new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, remedio.instrucoesAdicionais[0], remedio.frequencia));
            //}

            // _pacienteDbContext.Add(paciente);
            //_pacienteDbContext.SaveChanges();

            try
            {
            paciente.dataCriacao = DateTime.Now;
            paciente.Remedio_Paciente = new List<Remedio_Paciente>();

            _pacienteDbContext.Add(paciente);
            _pacienteDbContext.SaveChanges();

            // Agora que o ID do paciente foi gerado, podemos atualizar os registros de Remedio_Paciente
            foreach (var remedio in remedios)
                {
                    if(remedio.instrucoesAdicionais.Count!=0)
                    paciente.Remedio_Paciente.Add(new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, remedio.instrucoesAdicionais[0], remedio.frequencia));
                    else
                    paciente.Remedio_Paciente.Add(new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, null, remedio.frequencia));
                }


                _pacienteDbContext.SaveChanges();
            } catch (Exception ex)
            {
                throw ex;
            }


        }


        public void deletarPaciente(int id)
        {

            var paciente = _pacienteDbContext.Paciente.Include(r=> r.Remedio_Paciente).FirstOrDefault(p=> p.ID == id);

            if(paciente!=null)
            {
                foreach(var item in paciente.Remedio_Paciente)
                {
                    _pacienteDbContext.Remedio_Pacientes.Remove(item);
                }

            _pacienteDbContext.Paciente.Remove(paciente);

            _pacienteDbContext.SaveChanges();
            }


        }



    }
}
