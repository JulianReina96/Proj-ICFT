using Firebase.Auth;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.PacienteServices.Interface;
using System.Text;

namespace Proj_ICFT.Controllers
{
    public class PacienteController : Controller
    {
        FirebaseAuthProvider auth;
        private readonly IPacienteServices _pacienteServices;

        public PacienteController(IPacienteServices pacienteServices)
        {
            _pacienteServices = pacienteServices;
            auth = new FirebaseAuthProvider(
                            new FirebaseConfig("AIzaSyCY6ZiTuU3iDVMe37SK2p1oWCCoe7ltEV4"));
        }
        public IActionResult Index()
        {
            return View();
        }

        public string getUsuarioEmail()
        {
            try
            {

            var token = HttpContext.Session.GetString("_UserToken");
            FirebaseToken decodedToken = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token).Result;
            if (token == null)
            {
                return null;
            }
            var user = auth.GetUserAsync(token).Result;
            return user.Email;
            }catch(Exception ex)
            {

               throw ex;
            }
        }

        [SessionFilter]
        public async Task<IActionResult> PacientesAnalisados(string ordenacao, int pagina)
        {

            ExibicaoICTViewModel pacientesSalvos = new ExibicaoICTViewModel();
            try
            {
                var email = getUsuarioEmail();

                pacientesSalvos.PacienteICT = await _pacienteServices.listarPacientesICT(email);

                if (pacientesSalvos.PacienteICT != null)
                {
                    ExibicaoICTViewModel Pacientes = new ExibicaoICTViewModel();
                    Pacientes.Ordenacao = ordenacao;
                    Pacientes.Pagina = pagina;
                    List<PacienteICT> pacienteModel = new List<PacienteICT>();

                    foreach (var item in pacientesSalvos.PacienteICT.OrderBy(i => i.NomePaciente))
                    {
                        pacienteModel = pacientesSalvos.PacienteICT
                    .Select(x => new PacienteICT()
                    {
                        ID = x.ID,
                        NomePaciente = x.NomePaciente,
                        ICT = x.ICT,
                        dataCriacao = x.dataCriacao,
                    }).ToList();

                    }

                    List<PacienteICT> pacienteOrdenado = new List<PacienteICT>();

                    if (ordenacao == "nome")
                    {
                        foreach (var item in pacienteModel.OrderBy(i => i.NomePaciente)/*.ToPagedList(pagina, 5)*/)
                        {
                            pacienteOrdenado.Add(item);

                        }
                        Pacientes.PacienteICT = pacienteOrdenado;

                    }

                    else if (ordenacao == "data")
                    {
                        foreach (var item in pacienteModel.OrderBy(i => i.dataCriacao)/*.ToPagedList(pagina, 5)*/)
                        {
                            pacienteOrdenado.Add(item);

                        }
                        Pacientes.PacienteICT = pacienteOrdenado;
                    }
                    else if (ordenacao == "ict")
                    {
                        foreach (var item in pacienteModel.OrderBy(i => i.ICT)/*.ToPagedList(pagina, 5)*/)
                        {
                            pacienteOrdenado.Add(item);

                        }
                        Pacientes.PacienteICT = pacienteOrdenado;
                    }
                    else
                        Pacientes.PacienteICT = pacienteModel;

                    return View(Pacientes);
                }
                else
                {
                    TempData["ErrorMessage"] = "Não há pacientes cadastrados ";
                    return RedirectToAction("Login", "Account");

                }

            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Houve um erro durante a sua solicitação." + ex.Message;
                return RedirectToAction("Login", "Account");

            }

        }

        [SessionFilter]
        public async Task<IActionResult> DeletarPaciente(ExibicaoICTViewModel paciente)
        {
            try
            {                
                if (paciente.deleteClicked)
                {
                    _pacienteServices.deletarPaciente(paciente.id);
                    TempData["SuccessMessage"] = "Paciente removido com sucesso";
                    return RedirectToAction("PacientesAnalisados", "Paciente");
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao deletar paciente";
                    return RedirectToAction("PacientesAnalisados", "Paciente");
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        [SessionFilter]
        public async Task<JsonResult> ExportarTodosPacientes()
        {
            try
            {
                var email = getUsuarioEmail();
                var pacientes = await _pacienteServices.listarPacientesICT(email);

                List<PacienteICT> pacientesExportacao = new List<PacienteICT>();

                pacientesExportacao = pacientes.Select(x => new PacienteICT()
                {
                    ID = x.ID,
                    NomePaciente = x.NomePaciente,
                    ICT = x.ICT,
                    dataCriacao = x.dataCriacao,
                }).ToList();


                return Json(pacientesExportacao);

            }
            catch (Exception ex)
            {
                throw ex;

            }
        }




    }
} 

