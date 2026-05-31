using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Services.CIDServices.Interface;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.UsuariosService.Interface;
using System.Diagnostics;

namespace Proj_ICFT.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFormularioServices _formularioServices;
        private readonly ICIDServices _cidServices;
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFormularioServices formularioServices, IUsuarioService usuarioService,
            ILogger<HomeController> logger, ICIDServices cidServices)
        {
            _formularioServices = formularioServices;
            _logger = logger;
            _usuarioService = usuarioService;
            _cidServices = cidServices;
        }

        public IActionResult Index() => View();

        public IActionResult Home() => View();

        [SessionFilter]
        public async Task<IActionResult> Form()
        {
            var formulario = await carregarFormulario();
            return View(formulario);
        }

        [SessionFilter]
        public async Task<JsonResult> ObterSubcategorias(int categoriaId)
        {
            var subcategorias = await _formularioServices.listarTipoByCategoriaId(categoriaId);
            foreach (var s in subcategorias)
                s.Name += $" - Peso( {s.Peso} )";
            return Json(subcategorias);
        }

        [SessionFilter]
        [HttpPost]
        public async Task<JsonResult> SalvarReceita([FromBody] SalvarReceitaRequest request)
        {
            try
            {
                var email = HttpContext.Session.GetString("_UserEmail");
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada. Faça login novamente." });

                var result = await _formularioServices.SalvarReceita(email, request);
                return Json(new { success = true, ict = result.ict, receitaId = result.receitaId });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao salvar a receita." });
            }
        }

        private async Task<FormularioViewModel> carregarFormulario()
        {
            var categorias           = await _formularioServices.listarCategorias();
            var instrucoesAdicionais = await _formularioServices.listarInstrucoesAdicionais();
            var frequencias          = await _formularioServices.listarFrequencias();
            var blocos               = await _cidServices.listarBlocosCID();

            instrucoesAdicionais = instrucoesAdicionais
                .Select(i => { i.Name += $" - Peso( {i.Peso} )"; return i; })
                .ToList();

            frequencias = frequencias
                .Select(f => { f.Name += $" - Peso( {f.Peso} )"; return f; })
                .ToList();

            return new FormularioViewModel(categorias, frequencias, instrucoesAdicionais, blocos);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
