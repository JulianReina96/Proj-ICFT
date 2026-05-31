using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models.Request;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

namespace Proj_ICFT.Services.EvolucaoClinicaServices.Implementacao;

public class EvolucaoClinicaServices : IEvolucaoClinicaServices
{
    private readonly AppDbContextNew _db;

    public EvolucaoClinicaServices(AppDbContextNew db)
    {
        _db = db;
    }

    private async Task<(Usuario usuario, PacienteICT paciente)> ResolverContexto(int pacienteId, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");
        var paciente = await _db.PacienteICTs
            .FirstOrDefaultAsync(p => p.ID == pacienteId && p.UsuarioCriacaoID == usuario.id)
            ?? throw new InvalidOperationException("Paciente não encontrado ou sem permissão.");
        return (usuario, paciente);
    }

    private static string StatusLabel(StatusEvolucao s) => s switch
    {
        StatusEvolucao.Melhorou     => "Melhorou",
        StatusEvolucao.Estavel      => "Estável",
        StatusEvolucao.Piorou       => "Piorou",
        _                            => "Inconclusivo"
    };

    public async Task<int> Salvar(SalvarEvolucaoRequest r, string email)
    {
        var (usuario, paciente) = await ResolverContexto(r.PacienteID, email);

        if (r.ReceitaID is int rid)
        {
            var ok = await _db.Receita.AnyAsync(x => x.Id == rid && x.PacienteID == paciente.ID);
            if (!ok) throw new InvalidOperationException("Receita inválida para este paciente.");
        }

        if (r.CategoriaCID_ID is int cid && !await _db.Categorias_CIDs.AnyAsync(c => c.Id == cid))
            throw new InvalidOperationException("CID inválido.");

        if (r.DataConsulta > DateTime.Now.AddDays(1))
            throw new InvalidOperationException("Data da consulta não pode ser futura.");

        var ev = new EvolucaoClinica
        {
            PacienteID         = paciente.ID,
            UsuarioCriacaoID   = usuario.id,
            ReceitaID          = r.ReceitaID,
            CategoriaCID_ID    = r.CategoriaCID_ID,
            DataConsulta       = r.DataConsulta,
            DataCriacao        = DateTime.Now,
            Status             = r.Status,
            Subjetivo          = r.Subjetivo,
            Avaliacao          = r.Avaliacao,
            Plano              = r.Plano,
            ObservacaoObjetivo = r.ObservacaoObjetivo,
            PaSistolica        = r.PaSistolica,
            PaDiastolica       = r.PaDiastolica,
            FrequenciaCardiaca = r.FrequenciaCardiaca,
            Peso               = r.Peso,
            Spo2               = r.Spo2,
            Glicemia           = r.Glicemia,
            Hba1c              = r.Hba1c,
            Creatinina         = r.Creatinina,
            EventosAdversos    = r.EventosAdversos,
            Hospitalizacoes    = r.Hospitalizacoes,
            IdasEmergencia     = r.IdasEmergencia
        };
        _db.EvolucaoClinicas.Add(ev);
        await _db.SaveChangesAsync();
        return ev.Id;
    }

    public async Task Atualizar(int id, SalvarEvolucaoRequest r, string email)
    {
        var (_, paciente) = await ResolverContexto(r.PacienteID, email);

        var ev = await _db.EvolucaoClinicas
            .FirstOrDefaultAsync(e => e.Id == id && e.PacienteID == paciente.ID)
            ?? throw new InvalidOperationException("Evolução não encontrada ou sem permissão.");

        if (r.ReceitaID is int rid)
        {
            var ok = await _db.Receita.AnyAsync(x => x.Id == rid && x.PacienteID == paciente.ID);
            if (!ok) throw new InvalidOperationException("Receita inválida para este paciente.");
        }
        if (r.CategoriaCID_ID is int cid && !await _db.Categorias_CIDs.AnyAsync(c => c.Id == cid))
            throw new InvalidOperationException("CID inválido.");
        if (r.DataConsulta > DateTime.Now.AddDays(1))
            throw new InvalidOperationException("Data da consulta não pode ser futura.");

        ev.ReceitaID          = r.ReceitaID;
        ev.CategoriaCID_ID    = r.CategoriaCID_ID;
        ev.DataConsulta       = r.DataConsulta;
        ev.Status             = r.Status;
        ev.Subjetivo          = r.Subjetivo;
        ev.Avaliacao          = r.Avaliacao;
        ev.Plano              = r.Plano;
        ev.ObservacaoObjetivo = r.ObservacaoObjetivo;
        ev.PaSistolica        = r.PaSistolica;
        ev.PaDiastolica       = r.PaDiastolica;
        ev.FrequenciaCardiaca = r.FrequenciaCardiaca;
        ev.Peso               = r.Peso;
        ev.Spo2               = r.Spo2;
        ev.Glicemia           = r.Glicemia;
        ev.Hba1c              = r.Hba1c;
        ev.Creatinina         = r.Creatinina;
        ev.EventosAdversos    = r.EventosAdversos;
        ev.Hospitalizacoes    = r.Hospitalizacoes;
        ev.IdasEmergencia     = r.IdasEmergencia;

        await _db.SaveChangesAsync();
    }

    public async Task Deletar(int id, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var ev = await _db.EvolucaoClinicas
            .Include(e => e.Paciente)
            .FirstOrDefaultAsync(e => e.Id == id && e.Paciente.UsuarioCriacaoID == usuario.id)
            ?? throw new InvalidOperationException("Evolução não encontrada ou sem permissão.");

        _db.EvolucaoClinicas.Remove(ev);
        await _db.SaveChangesAsync();
    }

    // Os 5 métodos abaixo serão implementados nas Tasks 11, 12 e 13.
    public async Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        var evolucoes = await _db.EvolucaoClinicas
            .Include(e => e.Receita)
            .Include(e => e.CategoriaCID)
            .Where(e => e.PacienteID == paciente.ID)
            .OrderByDescending(e => e.DataConsulta)
            .ToListAsync();

        return evolucoes.Select(e => new EvolucaoListagemViewModel
        {
            Id             = e.Id,
            DataConsulta   = e.DataConsulta,
            Status         = e.Status,
            StatusLabel    = StatusLabel(e.Status),
            ReceitaID      = e.ReceitaID,
            IctDaReceita   = e.Receita?.ICT,
            ReceitaAdesao  = e.Receita?.Adesao,
            CidCodigo      = e.CategoriaCID?.Code,
            CidTitulo      = e.CategoriaCID?.Title
        }).ToList();
    }

    public async Task<EvolucaoDetalheViewModel?> Detalhar(int id, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var ev = await _db.EvolucaoClinicas
            .Include(e => e.Paciente)
            .Include(e => e.Receita)
            .Include(e => e.CategoriaCID)
            .FirstOrDefaultAsync(e => e.Id == id && e.Paciente.UsuarioCriacaoID == usuario.id);

        if (ev == null) return null;

        return new EvolucaoDetalheViewModel
        {
            Id                 = ev.Id,
            DataConsulta       = ev.DataConsulta,
            Status             = ev.Status,
            StatusLabel        = StatusLabel(ev.Status),
            ReceitaID          = ev.ReceitaID,
            IctDaReceita       = ev.Receita?.ICT,
            ReceitaAdesao      = ev.Receita?.Adesao,
            CategoriaCID_ID    = ev.CategoriaCID_ID,
            CidCodigo          = ev.CategoriaCID?.Code,
            CidTitulo          = ev.CategoriaCID?.Title,
            Subjetivo          = ev.Subjetivo,
            Avaliacao          = ev.Avaliacao,
            Plano              = ev.Plano,
            ObservacaoObjetivo = ev.ObservacaoObjetivo,
            PaSistolica        = ev.PaSistolica,
            PaDiastolica       = ev.PaDiastolica,
            FrequenciaCardiaca = ev.FrequenciaCardiaca,
            Peso               = ev.Peso,
            Spo2               = ev.Spo2,
            Glicemia           = ev.Glicemia,
            Hba1c              = ev.Hba1c,
            Creatinina         = ev.Creatinina,
            EventosAdversos    = ev.EventosAdversos,
            Hospitalizacoes    = ev.Hospitalizacoes,
            IdasEmergencia     = ev.IdasEmergencia
        };
    }

    public async Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        // CIDs vinculados às receitas do paciente (sem duplicar)
        var cids = await _db.ReceitaCIDs
            .Include(rc => rc.CategoriaCID)
            .Where(rc => rc.Receita.PacienteID == paciente.ID)
            .Select(rc => new CIDOpcaoViewModel
            {
                Id     = rc.CategoriaCID.Id,
                Codigo = rc.CategoriaCID.Code ?? "",
                Titulo = rc.CategoriaCID.Title ?? ""
            })
            .Distinct()
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        return cids;
    }

    public async Task<List<ReceitaOpcaoViewModel>> ListarReceitasDoPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        return await _db.Receita
            .Where(r => r.PacienteID == paciente.ID)
            .OrderByDescending(r => r.DataCriacao)
            .Select(r => new ReceitaOpcaoViewModel
            {
                Id          = r.Id,
                DataCriacao = r.DataCriacao,
                Ict         = r.ICT
            })
            .ToListAsync();
    }

    public async Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        var evolucoes = await _db.EvolucaoClinicas
            .Include(e => e.Receita)
            .Where(e => e.PacienteID == paciente.ID)
            .OrderBy(e => e.DataConsulta)
            .ToListAsync();

        var serie = new SerieTemporalViewModel();
        foreach (var e in evolucoes)
        {
            serie.Datas.Add(e.DataConsulta);
            serie.IctPorEvolucao.Add(e.Receita?.ICT);
            serie.StatusPorEvolucao.Add((int)e.Status);

            serie.PaSistolica.Add(e.PaSistolica);
            serie.PaDiastolica.Add(e.PaDiastolica);
            serie.FrequenciaCardiaca.Add(e.FrequenciaCardiaca);
            serie.Peso.Add(e.Peso);
            serie.Spo2.Add(e.Spo2);

            serie.Glicemia.Add(e.Glicemia);
            serie.Hba1c.Add(e.Hba1c);
            serie.Creatinina.Add(e.Creatinina);

            serie.EventosAdversos.Add(e.EventosAdversos);
            serie.Hospitalizacoes.Add(e.Hospitalizacoes);
            serie.IdasEmergencia.Add(e.IdasEmergencia);
        }
        return serie;
    }
}
