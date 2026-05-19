using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.PacienteServices.Interface;

namespace Proj_ICFT.Services.FormularioServices.Implementacao
{
    public class PacienteServices : IPacienteServices
    {
   private readonly AppDbContextNew _appDbContextNew;


        public PacienteServices(AppDbContextNew appDbContextNew)
        {
            _appDbContextNew = appDbContextNew;

        }                    
        public async Task<List<PacienteICT>> listarPacientesICT(string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return new List<PacienteICT>();

            // Exclui o paciente anônimo — registro interno gerenciado pelo sistema.
            return await _appDbContextNew.PacienteICTs
                .Include(p => p.Receita)
                .Where(p => p.UsuarioCriacaoID == user.id
                         && p.NomePaciente != FormularioServices.NomePacienteAnonimo)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<PacienteICT>> listarPacientesComAnonimo(string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return new List<PacienteICT>();

            return await _appDbContextNew.PacienteICTs
                .Include(p => p.Receita)
                .Where(p => p.UsuarioCriacaoID == user.id)
                .OrderBy(p => p.NomePaciente == FormularioServices.NomePacienteAnonimo ? 1 : 0)
                .ThenByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<PacienteICT> CadastrarPaciente(string email, string nome, int idade, string sexo)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) throw new InvalidOperationException("Usuário não encontrado.");

            var paciente = new PacienteICT(nome, DateTime.Now, user.id, sexo, idade);
            _appDbContextNew.PacienteICTs.Add(paciente);
            await _appDbContextNew.SaveChangesAsync();
            return paciente;
        }

        public async Task<PacienteICT?> BuscarPaciente(int id, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return null;

            return await _appDbContextNew.PacienteICTs
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id);
        }

        public async Task DeletarPaciente(int id, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            var paciente = await _appDbContextNew.PacienteICTs
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.InstrucoesMeds)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaCIDs)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id)
                ?? throw new InvalidOperationException("Paciente não encontrado.");

            if (paciente.NomePaciente == FormularioServices.NomePacienteAnonimo)
                throw new InvalidOperationException("O paciente anônimo não pode ser excluído.");

            // ClientSetNull em todos os FKs — remoção manual na ordem de dependência
            foreach (var receita in paciente.Receita)
            {
                foreach (var med in receita.ReceitaMeds)
                    _appDbContextNew.InstrucoesMeds.RemoveRange(med.InstrucoesMeds);

                _appDbContextNew.ReceitaMeds.RemoveRange(receita.ReceitaMeds);
                _appDbContextNew.ReceitaCIDs.RemoveRange(receita.ReceitaCIDs);
            }

            _appDbContextNew.Receita.RemoveRange(paciente.Receita);
            _appDbContextNew.PacienteICTs.Remove(paciente);

            await _appDbContextNew.SaveChangesAsync();
        }

        public async Task<Receitum?> BuscarReceitaCompleta(int receitaId, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return null;

            return await _appDbContextNew.Receita
                .Include(r => r.ReceitaMeds)
                    .ThenInclude(rm => rm.Medicamento)
                .Include(r => r.ReceitaMeds)
                    .ThenInclude(rm => rm.Categoria)
                .Include(r => r.ReceitaMeds)
                    .ThenInclude(rm => rm.Tipo)
                .Include(r => r.ReceitaMeds)
                    .ThenInclude(rm => rm.Frequencia)
                .Include(r => r.ReceitaMeds)
                    .ThenInclude(rm => rm.InstrucoesMeds)
                        .ThenInclude(im => im.Instrucao)
                .Include(r => r.ReceitaCIDs)
                    .ThenInclude(rc => rc.CategoriaCID)
                .FirstOrDefaultAsync(r => r.Id == receitaId && r.UsuarioCriacaoID == user.id);
        }

        public async Task<PacienteICT?> BuscarPacienteParaExport(int id, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return null;

            return await _appDbContextNew.PacienteICTs
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.Medicamento)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.Categoria)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.Tipo)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.Frequencia)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaMeds)
                        .ThenInclude(rm => rm.InstrucoesMeds)
                            .ThenInclude(im => im.Instrucao)
                .Include(p => p.Receita)
                    .ThenInclude(r => r.ReceitaCIDs)
                        .ThenInclude(rc => rc.CategoriaCID)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id);
        }

        //public void salvarPaciente(PacienteICT paciente, List<MedicamentosExportacaoModel> remedios)
        //{


        //    //paciente.dataCriacao = DateTime.Now;   
        //    //paciente.Remedio_Paciente = new List<Remedio_Paciente>();
        //    //foreach (var remedio in remedios)
        //    //{ //mudar para instrucao nao ser mais lista
        //    //    paciente.Remedio_Paciente.Add(new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, remedio.instrucoesAdicionais[0], remedio.frequencia));
        //    //}

        //    // _pacienteDbContext.Add(paciente);
        //    //_pacienteDbContext.SaveChanges();

        //    try
        //    {
        //    paciente.DataCriacao = DateTime.Now;
        //    paciente.Receita = new List<Receitum>();

        //        _appDbContextNew.PacienteICTs.Add(paciente);
        //        _appDbContextNew.SaveChanges();

        //    // Agora que o ID do paciente foi gerado, podemos atualizar os registros de Remedio_Paciente
        //    foreach (var remedio in remedios)
        //        {
        //            if (remedio.instrucoesAdicionais.Count != 0)
        //            {
        //                var remedio_paciente = new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, remedio.frequencia);

        //                foreach(var instrucao in remedio.instrucoesAdicionais)
        //                {
        //                    var instrucao1 = new InstrucoesAdicionais_Paciente { InstrucaoID = instrucao, RemedioPaciente = remedio_paciente };

        //                    remedio_paciente.InstrucoesAdicionais_Paciente.Add(instrucao1);
        //                }


        //            paciente.Remedio_Paciente.Add(remedio_paciente);
        //            }
        //            else
        //            paciente.Remedio_Paciente.Add(new Remedio_Paciente(paciente.ID, remedio.categoria, remedio.subcategoria, remedio.frequencia));

        //        }


        //        _pacienteDbContext.SaveChanges();
        //    } catch (Exception ex)
        //    {
        //        throw ex;
        //    }


        //}


        //public void deletarPaciente(int id)
        //{

        //    var paciente = _pacienteDbContext.Paciente.Include(r=> r.Remedio_Paciente).FirstOrDefault(p=> p.ID == id);

        //    if(paciente!=null)
        //    {
        //        foreach(var item in paciente.Remedio_Paciente)
        //        {

        //            _pacienteDbContext.Remedio_Pacientes.Remove(item);

        //        }

        //    _pacienteDbContext.Paciente.Remove(paciente);

        //    _pacienteDbContext.SaveChanges();
        //    }


        //}



    }
}
