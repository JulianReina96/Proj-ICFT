using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Proj_ICFT.Models;
using Proj_ICFT.Models.DTO;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Models.ModelUrl;
using Proj_ICFT.Services.CIDServices.Interface;
using Proj_ICFT.Services.MedicamentosServices.Interface;
using System.Text;

namespace Proj_ICFT.Controllers
{
    public class MedicamentosController : Controller
    {
        private readonly IMedicamentoService _IMedicamentoService;

        public MedicamentosController(IMedicamentoService iMedicamentoService)
        {
            _IMedicamentoService = iMedicamentoService;

        }

        [HttpGet]
        public async Task<JsonResult> ListarMedicamentos()
        {
            try
            {
                var medicamentos = await _IMedicamentoService.listarMedicamentos();
                return Json(medicamentos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<JsonResult> BuscarMedicamentos(string? q, int page = 1, int pageSize = 20)
        {
            var (items, hasMore) = await _IMedicamentoService.BuscarMedicamentos(q, page, pageSize);
            return Json(new
            {
                results = items.Select(m => new { id = m.Id, text = m.NOME_PRODUTO }),
                pagination = new { more = hasMore }
            });
        }
    }
}




