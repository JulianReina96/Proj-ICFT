using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Models.Request;
using Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

namespace Proj_ICFT.Controllers;

public class EvolucaoClinicaController : Controller
{
    private readonly IEvolucaoClinicaServices _ev;

    public EvolucaoClinicaController(IEvolucaoClinicaServices ev)
    {
        _ev = ev;
    }

    private string? EmailSessao => HttpContext.Session.GetString("_UserEmail");

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarPorPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarPorPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar evoluções." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> Detalhar(int id)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.Detalhar(id, email);
            if (data == null)
                return Json(new { success = false, message = "Evolução não encontrada." });

            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao carregar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Salvar([FromBody] SalvarEvolucaoRequest dto)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var id = await _ev.Salvar(dto, email);
            return Json(new { success = true, id });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao salvar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Atualizar(int id, [FromBody] SalvarEvolucaoRequest dto)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            await _ev.Atualizar(id, dto, email);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao atualizar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Deletar(int id)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            await _ev.Deletar(id, email);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao deletar evolução." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarCIDsDoPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarCIDsDoPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar CIDs." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarPrescricoesDoPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarPrescricoesDoPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar prescrições." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> SerieTemporal(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ObterSerieTemporal(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao gerar série temporal." });
        }
    }
}
