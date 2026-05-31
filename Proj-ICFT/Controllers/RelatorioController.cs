using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Services.RelatorioService.Interface;
using Proj_ICFT.Services.UsuariosService.Interface;

namespace Proj_ICFT.Controllers;

public class RelatorioController : Controller
{
    private readonly IRelatorioService _relatorioService;
    private readonly IUsuarioService _usuarioService;

    public RelatorioController(IRelatorioService relatorioService, IUsuarioService usuarioService)
    {
        _relatorioService = relatorioService;
        _usuarioService = usuarioService;
    }

    private string? EmailSessao => HttpContext.Session.GetString("_UserEmail");

    [HttpGet, SessionFilter]
    public IActionResult Index() => View();

    [HttpGet, SessionFilter]
    public IActionResult Dados(DateTime? de, DateTime? ate)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var usuario = _usuarioService.GetUsuarioByEmail(email);
            if (usuario == null)
                return Json(new { success = false, message = "Usuário não encontrado." });

            var filtro = new Models.ViewModels.RelatorioFiltroViewModel { De = de, Ate = ate };
            var dados = _relatorioService.GetDados(filtro, usuario.id);
            return Json(new { success = true, data = dados });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
