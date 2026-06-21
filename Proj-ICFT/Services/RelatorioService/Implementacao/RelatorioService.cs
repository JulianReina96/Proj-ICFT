using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.RelatorioService.Interface;

namespace Proj_ICFT.Services.RelatorioService.Implementacao;

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContextNew _db;

    public RelatorioService(AppDbContextNew db)
    {
        _db = db;
    }

    public RelatorioDataViewModel GetDados(RelatorioFiltroViewModel filtro, int userId)
    {
        var de  = filtro.De;
        var ate = filtro.Ate;

        // ── R1: Adesão por paciente ───────────────────────────────────────────
        var r1 = _db.Prescricoes
            .Where(r => r.UsuarioCriacaoID == userId
                     && (de  == null || r.DataCriacao >= de)
                     && (ate == null || r.DataCriacao <= ate))
            .Include(r => r.Paciente)
            .AsEnumerable()
            .GroupBy(r => r.Paciente.NomePaciente)
            .Select(g =>
            {
                var total  = g.Count();
                var aderiu = g.Count(r => r.Adesao);
                return new AdesaoPacienteRow(
                    g.Key,
                    total,
                    aderiu,
                    total > 0 ? Math.Round((double)aderiu / total * 100, 1) : 0
                );
            })
            .OrderBy(r => r.Nome)
            .ToList();

        // ── R2: Complexidade por paciente ─────────────────────────────────────
        var r2 = _db.Prescricoes
            .Where(r => r.UsuarioCriacaoID == userId
                     && (de  == null || r.DataCriacao >= de)
                     && (ate == null || r.DataCriacao <= ate))
            .Include(r => r.Paciente)
            .AsEnumerable()
            .GroupBy(r => r.Paciente.NomePaciente)
            .Select(g => new ComplexidadePacienteRow(
                g.Key,
                Math.Round(g.Average(r => r.ICT), 2),
                Math.Round(g.Max(r => r.ICT), 2),
                g.Count()
            ))
            .OrderByDescending(r => r.IctMedio)
            .ToList();

        // ── R3: Distribuição por categoria ────────────────────────────────────
        var r3 = _db.PrescricaoMeds
            .Where(rm => rm.Prescricao.UsuarioCriacaoID == userId
                      && (de  == null || rm.Prescricao.DataCriacao >= de)
                      && (ate == null || rm.Prescricao.DataCriacao <= ate))
            .Include(rm => rm.Categoria)
            .Include(rm => rm.Prescricao)
            .AsEnumerable()
            .GroupBy(rm => rm.Categoria.Name)
            .Select(g => new DistribuicaoCategoriaRow(g.Key, g.Count()))
            .OrderByDescending(r => r.Quantidade)
            .ToList();

        // ── R4: Prescrições por período ───────────────────────────────────────
        var r4 = _db.Prescricoes
            .Where(r => r.UsuarioCriacaoID == userId
                     && (de  == null || r.DataCriacao >= de)
                     && (ate == null || r.DataCriacao <= ate))
            .AsEnumerable()
            .GroupBy(r => new { r.DataCriacao.Year, r.DataCriacao.Month })
            .Select(g => new PrescricoesPorPeriodoRow(
                $"{g.Key.Year}-{g.Key.Month:D2}",
                g.Count(),
                Math.Round(g.Average(r => r.ICT), 2)
            ))
            .OrderBy(r => r.Periodo)
            .ToList();

        // ── R5: Diagnósticos frequentes ───────────────────────────────────────
        var r5 = _db.PrescricaoCIDs
            .Where(rc => rc.Prescricao.UsuarioCriacaoID == userId
                      && (de  == null || rc.Prescricao.DataCriacao >= de)
                      && (ate == null || rc.Prescricao.DataCriacao <= ate))
            .Include(rc => rc.CategoriaCID)
            .Include(rc => rc.Prescricao)
            .AsEnumerable()
            .GroupBy(rc => new { rc.CategoriaCID?.Code, rc.CategoriaCID?.Title })
            .Select(g => new DiagnosticoFrequenteRow(
                g.Key.Code ?? "-",
                g.Key.Title ?? "-",
                g.Count()
            ))
            .OrderByDescending(r => r.Ocorrencias)
            .Take(10)
            .ToList();

        // ── R6: Correlação ICT × Status ───────────────────────────────────────
        var r6 = _db.EvolucaoClinicas
            .Where(ec => ec.UsuarioCriacaoID == userId
                      && ec.PrescricaoID != null
                      && (de  == null || ec.DataConsulta >= de)
                      && (ate == null || ec.DataConsulta <= ate))
            .Include(ec => ec.Prescricao)
            .AsEnumerable()
            .GroupBy(ec => ec.Status)
            .Select(g => new CorrelacaoICTStatusRow(
                g.Key switch
                {
                    StatusEvolucao.Melhorou => "Melhorou",
                    StatusEvolucao.Estavel  => "Estável",
                    StatusEvolucao.Piorou   => "Piorou",
                    _                       => "Inconclusivo"
                },
                Math.Round(g.Average(ec => ec.Prescricao!.ICT), 2),
                g.Count()
            ))
            .ToList();

        // ── R7: Hospitalizações por paciente ──────────────────────────────────
        var r7 = _db.EvolucaoClinicas
            .Where(ec => ec.UsuarioCriacaoID == userId
                      && (de  == null || ec.DataConsulta >= de)
                      && (ate == null || ec.DataConsulta <= ate))
            .Include(ec => ec.Paciente)
            .AsEnumerable()
            .GroupBy(ec => ec.Paciente.NomePaciente)
            .Select(g => new HospitalizacaoPacienteRow(
                g.Key,
                g.Sum(ec => ec.Hospitalizacoes),
                g.Count()
            ))
            .Where(r => r.TotalHospitalizacoes > 0)
            .OrderByDescending(r => r.TotalHospitalizacoes)
            .ToList();

        // ── R8: Eventos adversos por faixa ICT ───────────────────────────────
        var dadosR8 = _db.EvolucaoClinicas
            .Where(ec => ec.UsuarioCriacaoID == userId
                      && ec.PrescricaoID != null
                      && (de  == null || ec.DataConsulta >= de)
                      && (ate == null || ec.DataConsulta <= ate))
            .Include(ec => ec.Prescricao)
            .AsEnumerable()
            .Select(ec => new { Ict = ec.Prescricao!.ICT, ec.EventosAdversos })
            .ToList();

        static string GetFaixa(double ict) =>
            ict <= 5 ? "0-5" : ict <= 10 ? "6-10" : ict <= 15 ? "11-15" : "16+";

        var r8 = dadosR8
            .GroupBy(x => GetFaixa(x.Ict))
            .Select(g => new EventoAdversoFaixaRow(g.Key, g.Sum(x => x.EventosAdversos), g.Count()))
            .OrderBy(r => r.Faixa)
            .ToList();

        // ── KPIs ──────────────────────────────────────────────────────────────
        var prescricoesFiltradas = _db.Prescricoes
            .Where(r => r.UsuarioCriacaoID == userId
                     && (de  == null || r.DataCriacao >= de)
                     && (ate == null || r.DataCriacao <= ate))
            .AsEnumerable()
            .ToList();

        var totalPacientes = _db.PacienteICTs
            .Count(p => p.UsuarioCriacaoID == userId);

        var adesaoGeralPct = prescricoesFiltradas.Count > 0
            ? Math.Round((double)prescricoesFiltradas.Count(r => r.Adesao) / prescricoesFiltradas.Count * 100, 1)
            : 0.0;

        var ictMedio = prescricoesFiltradas.Count > 0
            ? Math.Round(prescricoesFiltradas.Average(r => r.ICT), 2)
            : 0.0;

        var totalHosp = _db.EvolucaoClinicas
            .Where(ec => ec.UsuarioCriacaoID == userId
                      && (de  == null || ec.DataConsulta >= de)
                      && (ate == null || ec.DataConsulta <= ate))
            .Sum(ec => (int?)ec.Hospitalizacoes) ?? 0;

        return new RelatorioDataViewModel
        {
            AdesaoGeralPct          = adesaoGeralPct,
            IctMedio                = ictMedio,
            TotalHospitalizacoes    = totalHosp,
            TotalPacientes          = totalPacientes,
            AdesaoPorPaciente       = r1,
            ComplexidadePorPaciente = r2,
            DistribuicaoPorCategoria = r3,
            PrescricoesPorPeriodo   = r4,
            DiagnosticosFrequentes  = r5,
            CorrelacaoICTStatus     = r6,
            HospitalizacoesPorPaciente = r7,
            EventosAdversosPorFaixa = r8
        };
    }
}
