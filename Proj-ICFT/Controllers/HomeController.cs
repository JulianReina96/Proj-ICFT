using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
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

        public HomeController(ILogger<HomeController> logger, IFormularioServices formularioServices, IPacienteServices pacienteServices)
        {
            _logger = logger;
            _formularioServices = formularioServices;
            _pacienteServices = pacienteServices;
        }

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

        public async Task<IActionResult> Form()
        {
            var formulario = await carregarFormulario();

            
            return View(formulario);
        }

        [HttpPost]
        public async Task<JsonResult> ExportarDados ([FromBody] ExportacaoModel dados)
        {

            var remedios = await _formularioServices.converterMedicamentos(dados.medicamentos);

            int pesoTotal = remedios.Sum(r => r.PesoTotal);

            var subcategorias = remedios.Select(r => r.subcategoria).DistinctBy(c=> c.CategoriaId).ToList();


            pesoTotal = pesoTotal + subcategorias.Sum(s => s.Peso);



            PacienteICT ict = new PacienteICT(dados.nome, pesoTotal);

             _pacienteServices.salvarPaciente(ict, dados.medicamentos);








            return Json(ict);

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
