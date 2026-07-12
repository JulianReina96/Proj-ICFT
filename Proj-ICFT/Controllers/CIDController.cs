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
using Proj_ICFT.Services.FormularioServices.Implementacao;
using System.Text;

namespace Proj_ICFT.Controllers
{
    public class CIDController : Controller
    {
        private readonly ICIDServices _ICIDServices;

        public CIDController(ICIDServices cIDServices)
        {
            _ICIDServices = cIDServices;

        }

        [HttpGet]
        public async Task<JsonResult> ListarBlocos()
        {
            try
            {
                var blocos = await _ICIDServices.listarBlocosCID();
                return Json(blocos);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet]
        public async Task<JsonResult> ListarCategorias()
        {
            try
            {
                var categorias = await _ICIDServices.listarCategoriasCID();
                return Json(categorias);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<JsonResult> ListarCapitulos()
        {
            try
            {
                var capitulos = await _ICIDServices.listarCapitulosCID();
                return Json(capitulos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<JsonResult> ListarCategoriasPorBloco(string blocoId)
        {
            var categorias = await _ICIDServices.listarCategoriasPorBloco(blocoId);
            var resultado = categorias
                .Select(c => new { id = c.Id, code = c.Code, title = c.Title?.TrimStart('-', ' ') ?? string.Empty })
                .Where(c => !string.IsNullOrWhiteSpace(c.title))
                .OrderBy(c => c.title)
                .ToList();
            return Json(resultado);
        }
    }
}
        

    

