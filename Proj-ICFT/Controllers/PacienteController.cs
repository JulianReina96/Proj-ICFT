using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Services.FormularioServices.Implementacao;
using Proj_ICFT.Services.PacienteServices.Interface;

namespace Proj_ICFT.Controllers
{
    public class PacienteController : Controller
    {
        private readonly IPacienteServices _pacienteServices;

        public PacienteController(IPacienteServices pacienteServices)
        {
            _pacienteServices = pacienteServices;
        }

        private string? EmailSessao => HttpContext.Session.GetString("_UserEmail");

        [HttpGet]
        [SessionFilter]
        public async Task<IActionResult> PacientesAnalisados()
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var pacientes = await _pacienteServices.listarPacientesComAnonimo(email);
            ViewBag.NomePacienteAnonimo = FormularioServices.NomePacienteAnonimo;
            return View(pacientes);
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> ListarPacientes()
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new List<object>());

            // Exclui anônimo do dropdown do formulário — usuário usa checkbox para isso
            var pacientes = await _pacienteServices.listarPacientesICT(email);
            return Json(pacientes.Select(p => new { id = p.ID, nome = p.NomePaciente }));
        }

        [HttpPost]
        [SessionFilter]
        public async Task<JsonResult> CadastrarPaciente([FromBody] CadastroPacienteRequest dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nome) || dto.Idade <= 0 || string.IsNullOrWhiteSpace(dto.Sexo))
                    return Json(new { success = false, message = "Dados inválidos." });

                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                await _pacienteServices.CadastrarPaciente(email, dto.Nome.Trim(), dto.Idade, dto.Sexo);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao cadastrar paciente." });
            }
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> DetalharPaciente(int id)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                var paciente = await _pacienteServices.BuscarPaciente(id, email);

                if (paciente == null)
                    return Json(new { success = false, message = "Paciente não encontrado." });

                return Json(new
                {
                    success = true,
                    id = paciente.ID,
                    nome = paciente.NomePaciente,
                    idade = paciente.Idade,
                    sexo = paciente.Sexo,
                    dataCriacao = paciente.DataCriacao.ToString("dd/MM/yyyy"),
                    anonimo = paciente.NomePaciente == FormularioServices.NomePacienteAnonimo,
                    receitas = paciente.Receita
                        .OrderByDescending(r => r.DataCriacao)
                        .Select(r => new
                        {
                            r.Id,
                            ict = r.ICT,
                            data = r.DataCriacao.ToString("dd/MM/yyyy"),
                            r.Adesao
                        })
                });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao carregar paciente." });
            }
        }

        [HttpGet]
        [SessionFilter]
        public async Task<IActionResult> VerPaciente(int id)
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var paciente = await _pacienteServices.BuscarPaciente(id, email);
            if (paciente == null)
                return RedirectToAction("PacientesAnalisados");

            ViewBag.NomePacienteAnonimo = FormularioServices.NomePacienteAnonimo;
            return View(paciente);
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> DetalharReceita(int receitaId)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                var receita = await _pacienteServices.BuscarReceitaCompleta(receitaId, email);
                if (receita == null)
                    return Json(new { success = false, message = "Receita não encontrada." });

                return Json(new
                {
                    success  = true,
                    id       = receita.Id,
                    data     = receita.DataCriacao.ToString("dd/MM/yyyy"),
                    ict      = receita.ICT,
                    adesao   = receita.Adesao,
                    medicamentos = receita.ReceitaMeds.Select(rm => new
                    {
                        medicamento = rm.Medicamento.NOME_PRODUTO,
                        categoria   = rm.Categoria.Name,
                        tipo        = rm.Tipo.Name,
                        frequencia  = rm.Frequencia.Name,
                        instrucoes  = rm.InstrucoesMeds.Select(im => im.Instrucao.Name).ToList()
                    }),
                    cids = receita.ReceitaCIDs.Select(rc => new
                    {
                        code  = rc.CategoriaCID.Code,
                        title = rc.CategoriaCID.Title?.TrimStart('-', ' ')
                    })
                });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao carregar receita." });
            }
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> ExportarDadosPaciente(int id)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                var paciente = await _pacienteServices.BuscarPacienteParaExport(id, email);
                if (paciente == null)
                    return Json(new { success = false, message = "Paciente não encontrado." });

                return Json(new
                {
                    success  = true,
                    paciente = new { nome = paciente.NomePaciente, sexo = paciente.Sexo, idade = paciente.Idade },
                    receitas = paciente.Receita
                        .OrderByDescending(r => r.DataCriacao)
                        .Select(r => new
                        {
                            data   = r.DataCriacao.ToString("dd/MM/yyyy"),
                            ict    = r.ICT,
                            adesao = r.Adesao,
                            medicamentos = r.ReceitaMeds.Select(rm => new
                            {
                                medicamento = rm.Medicamento.NOME_PRODUTO,
                                categoria   = rm.Categoria.Name,
                                tipo        = rm.Tipo.Name,
                                frequencia  = rm.Frequencia.Name,
                                instrucoes  = string.Join("; ", rm.InstrucoesMeds.Select(im => im.Instrucao.Name))
                            }),
                            cids = string.Join("; ", r.ReceitaCIDs.Select(rc =>
                                $"{rc.CategoriaCID.Code} – {rc.CategoriaCID.Title?.TrimStart('-', ' ')}"))
                        })
                });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao exportar dados do paciente." });
            }
        }

        [HttpDelete]
        [SessionFilter]
        public async Task<JsonResult> DeletarPaciente(int id)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                await _pacienteServices.DeletarPaciente(id, email);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao excluir paciente." });
            }
        }

        public record CadastroPacienteRequest(string Nome, int Idade, string Sexo);
    }
}
