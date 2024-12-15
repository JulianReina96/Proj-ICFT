using Firebase.Auth;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Services.FormularioServices.Implementacao;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.PacienteServices.Interface;
using System.Diagnostics;

namespace Proj_ICFT.Controllers
{
    public class HomeController : Controller
    {

        private readonly IFormularioServices _formularioServices;
        private readonly IPacienteServices _pacienteServices;
        private readonly ILogger<HomeController> _logger;

        FirebaseAuthProvider auth;

        public HomeController(IFormularioServices formularioServices, IPacienteServices pacienteServices, ILogger<HomeController> logger)
        {
            _formularioServices = formularioServices;
            _pacienteServices = pacienteServices;
            _logger = logger;

            auth = new FirebaseAuthProvider(
                           new FirebaseConfig("AIzaSyCY6ZiTuU3iDVMe37SK2p1oWCCoe7ltEV4"));
        }


        [SessionFilter]
        public async Task<JsonResult> ObterSubcategorias(int categoriaId)
        {
            // Lógica para buscar as subcategorias no banco de dados
            var subcategorias = await _formularioServices.listarTipoByCategoriaId(categoriaId);

            foreach (var subcategoria in subcategorias)
            {
                subcategoria.Name += " - Peso( " + subcategoria.Peso + " )";
            }
                
               
            return Json(subcategorias);
        }
        public IActionResult Index()
        {
            return View();
        }

        [SessionFilter]
        public async Task<IActionResult> Form()
        {
            var formulario = await carregarFormulario();

            
            return View(formulario);
        }
        [SessionFilter]
        [HttpPost]
        public async Task<IActionResult> ExportarDados ([FromBody] ExportacaoModel dados)
        {

            var remedios = await _formularioServices.converterMedicamentos(dados.medicamentos);

            int pesoTotal = remedios.Sum(r => r.PesoTotal);

            var subcategorias = remedios.Select(r => r.subcategoria).DistinctBy(c=> c.CategoriaId).ToList();


            pesoTotal = pesoTotal + subcategorias.Sum(s => s.Peso);

            var token = HttpContext.Session.GetString("_UserToken");
            FirebaseToken decodedToken = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token).Result;
            if (token == null)
            {
                TempData["WarningMessage"] = "É preciso estar logado para realizar a inserção de pacientes";
                return RedirectToAction("Login", "Account");

            }

            var user = await auth.GetUserAsync(token);
            string email = user.Email;

            PacienteICT ict = new PacienteICT(dados.nome, pesoTotal, email);

             _pacienteServices.salvarPaciente(ict, dados.medicamentos);








            return Ok();

        }


        public async Task<FormularioViewModel> carregarFormulario()
        {

            var categorias = await _formularioServices.listarCategorias();
            var instrucoesAdicionais = await _formularioServices.listarInstrucoesAdicionais();
            var frequencias = await _formularioServices.listarFrequencias();

            instrucoesAdicionais = instrucoesAdicionais.Select(i =>
            {
                i.name += " - Peso( " + i.Peso + " )";
                return i;
            }).ToList();

            frequencias = frequencias.Select(f =>
            {
                f.Name += " - Peso( " + f.Peso + " )";
                return f;
            }).ToList();

            FormularioViewModel formularioViewModel = new FormularioViewModel(categorias, frequencias, instrucoesAdicionais);
            
            return formularioViewModel;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
