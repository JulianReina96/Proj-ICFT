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
                .Include(p => p.Prescricoes)
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
                .Include(p => p.Prescricoes)
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
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id);
        }

        public async Task DeletarPaciente(int id, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            var paciente = await _appDbContextNew.PacienteICTs
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.InstrucoesMeds)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoCIDs)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id)
                ?? throw new InvalidOperationException("Paciente não encontrado.");

            if (paciente.NomePaciente == FormularioServices.NomePacienteAnonimo)
                throw new InvalidOperationException("O paciente anônimo não pode ser excluído.");

            // ClientSetNull em todos os FKs — remoção manual na ordem de dependência
            foreach (var prescricao in paciente.Prescricoes)
            {
                foreach (var med in prescricao.PrescricaoMeds)
                    _appDbContextNew.InstrucoesMeds.RemoveRange(med.InstrucoesMeds);

                _appDbContextNew.PrescricaoMeds.RemoveRange(prescricao.PrescricaoMeds);
                _appDbContextNew.PrescricaoCIDs.RemoveRange(prescricao.PrescricaoCIDs);
            }

            _appDbContextNew.Prescricoes.RemoveRange(paciente.Prescricoes);
            _appDbContextNew.PacienteICTs.Remove(paciente);

            await _appDbContextNew.SaveChangesAsync();
        }

        public async Task<Prescricao?> BuscarPrescricaoCompleta(int prescricaoId, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return null;

            return await _appDbContextNew.Prescricoes
                .Include(r => r.PrescricaoMeds)
                    .ThenInclude(rm => rm.Medicamento)
                .Include(r => r.PrescricaoMeds)
                    .ThenInclude(rm => rm.Categoria)
                .Include(r => r.PrescricaoMeds)
                    .ThenInclude(rm => rm.Tipo)
                .Include(r => r.PrescricaoMeds)
                    .ThenInclude(rm => rm.Frequencia)
                .Include(r => r.PrescricaoMeds)
                    .ThenInclude(rm => rm.InstrucoesMeds)
                        .ThenInclude(im => im.Instrucao)
                .Include(r => r.PrescricaoCIDs)
                    .ThenInclude(rc => rc.CategoriaCID)
                .FirstOrDefaultAsync(r => r.Id == prescricaoId && r.UsuarioCriacaoID == user.id);
        }

        public async Task DeletarPrescricao(int prescricaoId, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            var prescricao = await _appDbContextNew.Prescricoes
                .Include(p => p.PrescricaoMeds).ThenInclude(m => m.InstrucoesMeds)
                .Include(p => p.PrescricaoCIDs)
                .FirstOrDefaultAsync(p => p.Id == prescricaoId && p.UsuarioCriacaoID == user.id)
                ?? throw new InvalidOperationException("Prescrição não encontrada.");

            // Remoção manual na ordem de dependência (FKs ClientSetNull).
            // EvolucaoClinica que referencia esta prescrição tem FK ON DELETE SET NULL no banco.
            foreach (var med in prescricao.PrescricaoMeds)
                _appDbContextNew.InstrucoesMeds.RemoveRange(med.InstrucoesMeds);
            _appDbContextNew.PrescricaoMeds.RemoveRange(prescricao.PrescricaoMeds);
            _appDbContextNew.PrescricaoCIDs.RemoveRange(prescricao.PrescricaoCIDs);
            _appDbContextNew.Prescricoes.Remove(prescricao);

            await _appDbContextNew.SaveChangesAsync();
        }

        public async Task<PacienteICT?> BuscarPacienteParaExport(int id, string email)
        {
            var user = await _appDbContextNew.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email);
            if (user == null) return null;

            return await _appDbContextNew.PacienteICTs
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.Medicamento)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.Categoria)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.Tipo)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.Frequencia)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoMeds)
                        .ThenInclude(rm => rm.InstrucoesMeds)
                            .ThenInclude(im => im.Instrucao)
                .Include(p => p.Prescricoes)
                    .ThenInclude(r => r.PrescricaoCIDs)
                        .ThenInclude(rc => rc.CategoriaCID)
                .FirstOrDefaultAsync(p => p.ID == id && p.UsuarioCriacaoID == user.id);
        }
    }
}
