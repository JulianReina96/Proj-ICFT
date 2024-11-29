using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.PacienteServices.Interface;
using System.Text;

namespace Proj_ICFT.Controllers
{
    public class PacienteController : Controller
    {
        private readonly IPacienteServices _pacienteServices;

        public PacienteController(IPacienteServices pacienteServices)
        {
            _pacienteServices = pacienteServices;
        }
        public IActionResult Index()
        {
            return View();
        }



        public async Task<IActionResult> PacientesAnalisados(string ordenacao, int pagina)
        {

            ExibicaoICTViewModel pacientesSalvos = new ExibicaoICTViewModel();
            try
            {

                pacientesSalvos.PacienteICT = await _pacienteServices.listarPacientesICT();

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
    } 
}
