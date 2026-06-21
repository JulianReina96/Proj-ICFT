using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.FormularioServices.Interface;

namespace Proj_ICFT.Services.FormularioServices.Implementacao
{
    public class FormularioServices : IFormularioServices
    {
        private readonly AppDbContextNew _appDbContextNew;


        public FormularioServices(AppDbContextNew appDbContextNew)
        {
            _appDbContextNew = appDbContextNew;
        }        


        public async Task<List<Categorium>> listarCategorias()
        {

            return await _appDbContextNew.Categoria.Include(x => x.Tipos).OrderBy(c => c.Name).ToListAsync();

        }

        public async Task<List<Frequencium>> listarFrequencias()
        {
            return await _appDbContextNew.Frequencia.OrderBy(f => f.Name).ToListAsync();
        }

        public async Task<List<InstrucoesAdicionai>> listarInstrucoesAdicionais()
        {
            return await _appDbContextNew.InstrucoesAdicionais.OrderBy(i => i.Name).ToListAsync();
        }

        public async Task<List<Tipo>> listarTipoByCategoriaId(int id)
        {

            return await _appDbContextNew.Tipos.Where(s => s.CategoriaId == id).OrderBy(t => t.Name).ToListAsync();

        }

        public async Task<List<RemedioViewModel>> converterMedicamentos(List<MedicamentosExportacaoModel> listaModel)
        {
            var remedios = new List<RemedioViewModel>();

            foreach (var model in listaModel)
            {
                var categoria    = await _appDbContextNew.Categoria.FindAsync(model.categoria);
                var subcategoria = await _appDbContextNew.Tipos.FindAsync(model.subcategoria);
                var frequencia   = await _appDbContextNew.Frequencia.FindAsync(model.frequencia);
                var instrucoes   = await _appDbContextNew.InstrucoesAdicionais
                    .Where(i => model.instrucoesAdicionais!.Contains(i.id))
                    .ToListAsync();

                remedios.Add(new RemedioViewModel(categoria!, subcategoria!, frequencia!, instrucoes));
            }
            return remedios;
        }

        // Nome reservado para o paciente anônimo por usuário.
        // Usar a mesma string no PacientesServices ao filtrar a listagem.
        public const string NomePacienteAnonimo = "Paciente Anônimo";

        public async Task<(double ict, int prescricaoId)> SalvarPrescricao(string email, SalvarPrescricaoRequest request)
        {
            var usuario = await _appDbContextNew.Usuarios
                .FirstOrDefaultAsync(u => u.Usuario1 == email)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            // ── Resolver paciente ──────────────────────────────────────────────────
            int pacienteId;

            if (request.PacienteAnonimo)
            {
                // Reutiliza o mesmo registro anônimo para o usuário em vez de criar um por prescrição.
                var anonimo = await _appDbContextNew.PacienteICTs
                    .FirstOrDefaultAsync(p => p.UsuarioCriacaoID == usuario.id
                                           && p.NomePaciente == NomePacienteAnonimo);
                if (anonimo == null)
                {
                    anonimo = new PacienteICT(NomePacienteAnonimo, DateTime.Now, usuario.id, "M", 0);
                    _appDbContextNew.PacienteICTs.Add(anonimo);
                    await _appDbContextNew.SaveChangesAsync();
                }
                pacienteId = anonimo.ID;
            }
            else if (request.PacienteId.HasValue)
            {
                pacienteId = request.PacienteId.Value;
            }
            else
            {
                throw new InvalidOperationException("Selecione um paciente ou marque como anônimo.");
            }

            var prescricao = new Prescricao
            {
                PacienteID        = pacienteId,
                UsuarioCriacaoID  = usuario.id,
                DataCriacao       = DateTime.Now,
                Adesao            = request.Adesao,
                ICT               = 0
            };

            var ict = await MontarItensECalcularIct(prescricao, request);

            _appDbContextNew.Prescricoes.Add(prescricao);
            await _appDbContextNew.SaveChangesAsync();

            return (ict, prescricao.Id);
        }

        public async Task<(double ict, int prescricaoId)> AtualizarPrescricao(string email, int prescricaoId, SalvarPrescricaoRequest request)
        {
            var usuario = await _appDbContextNew.Usuarios
                .FirstOrDefaultAsync(u => u.Usuario1 == email)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            var prescricao = await _appDbContextNew.Prescricoes
                .Include(p => p.PrescricaoMeds).ThenInclude(m => m.InstrucoesMeds)
                .Include(p => p.PrescricaoCIDs)
                .FirstOrDefaultAsync(p => p.Id == prescricaoId && p.UsuarioCriacaoID == usuario.id)
                ?? throw new InvalidOperationException("Prescrição não encontrada.");

            // Edição não altera o paciente vinculado — apenas medicamentos, CIDs e adesão.
            // Substitui os filhos antigos pelos novos (remoção explícita: FKs são ClientSetNull).
            foreach (var med in prescricao.PrescricaoMeds)
                _appDbContextNew.InstrucoesMeds.RemoveRange(med.InstrucoesMeds);
            _appDbContextNew.PrescricaoMeds.RemoveRange(prescricao.PrescricaoMeds);
            _appDbContextNew.PrescricaoCIDs.RemoveRange(prescricao.PrescricaoCIDs);
            prescricao.PrescricaoMeds.Clear();
            prescricao.PrescricaoCIDs.Clear();

            prescricao.Adesao = request.Adesao;
            var ict = await MontarItensECalcularIct(prescricao, request);

            await _appDbContextNew.SaveChangesAsync();

            return (ict, prescricao.Id);
        }

        // Monta PrescricaoMeds/InstrucoesMeds/PrescricaoCIDs na prescrição e calcula o ICT (MRCI).
        // Tipo/Forma: somado UMA VEZ por forma distinta na prescrição. Frequência e Instruções: por medicamento.
        // Compartilhado por SalvarPrescricao e AtualizarPrescricao para garantir cálculo idêntico.
        private async Task<double> MontarItensECalcularIct(Prescricao prescricao, SalvarPrescricaoRequest request)
        {
            var tipoIds  = request.Medicamentos.Select(m => m.SubcategoriaId).Distinct().ToList();
            var freqIds  = request.Medicamentos.Select(m => m.FrequenciaId).Distinct().ToList();
            var instrIds = request.Medicamentos
                .SelectMany(m => m.InstrucoesAdicionais)
                .Distinct()
                .ToList();

            var tipos = await _appDbContextNew.Tipos
                .Where(t => tipoIds.Contains(t.id))
                .ToDictionaryAsync(t => t.id);

            var frequencias = await _appDbContextNew.Frequencia
                .Where(f => freqIds.Contains(f.id))
                .ToDictionaryAsync(f => f.id);

            var instrucoes = await _appDbContextNew.InstrucoesAdicionais
                .Where(i => instrIds.Contains(i.id))
                .ToDictionaryAsync(i => i.id);

            double ict = request.Medicamentos
                .Select(m => m.SubcategoriaId)
                .Distinct()
                .Sum(id => tipos.TryGetValue(id, out var t) ? (double)t.Peso : 0);

            foreach (var med in request.Medicamentos)
            {
                frequencias.TryGetValue(med.FrequenciaId, out var freq);

                var instrsDoMed = med.InstrucoesAdicionais
                    .Where(instrucoes.ContainsKey)
                    .Select(id => instrucoes[id])
                    .ToList();

                ict += (freq?.Peso ?? 0.0) + instrsDoMed.Sum(i => i.Peso);

                var prescricaoMed = new PrescricaoMed
                {
                    MedicamentoID = med.MedicamentoId,
                    CategoriaID   = med.CategoriaId,
                    TipoID        = med.SubcategoriaId,
                    FrequenciaID  = med.FrequenciaId
                };

                foreach (var instr in instrsDoMed)
                    prescricaoMed.InstrucoesMeds.Add(new InstrucoesMed { InstrucaoId = instr.id });

                prescricao.PrescricaoMeds.Add(prescricaoMed);
            }

            foreach (var cidId in request.CidCategorias)
                prescricao.PrescricaoCIDs.Add(new PrescricaoCID { CategoriaCID_ID = cidId });

            prescricao.ICT = ict;
            return ict;
        }
    }
}
