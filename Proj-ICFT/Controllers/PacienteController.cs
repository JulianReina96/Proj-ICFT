using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Services.CIDServices.Interface;
using Proj_ICFT.Services.FormularioServices.Implementacao;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.PacienteServices.Interface;

namespace Proj_ICFT.Controllers
{
    public class PacienteController : Controller
    {
        private readonly IPacienteServices _pacienteServices;
        private readonly IFormularioServices _formularioServices;
        private readonly ICIDServices _cidServices;

        public PacienteController(IPacienteServices pacienteServices,
            IFormularioServices formularioServices, ICIDServices cidServices)
        {
            _pacienteServices = pacienteServices;
            _formularioServices = formularioServices;
            _cidServices = cidServices;
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
                    prescricoes = paciente.Prescricoes
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

            // Dados de referência para o modal de edição de prescrição (mesmas fontes do formulário).
            var categorias  = await _formularioServices.listarCategorias();
            var frequencias = await _formularioServices.listarFrequencias();
            var instrucoes  = await _formularioServices.listarInstrucoesAdicionais();
            var blocos      = await _cidServices.listarBlocosCID();

            ViewBag.CategoriasJson = System.Text.Json.JsonSerializer.Serialize(
                categorias.Select(c => new { id = c.Id, name = c.Name }));
            ViewBag.TiposPorCategoriaJson = System.Text.Json.JsonSerializer.Serialize(
                categorias.ToDictionary(c => c.Id.ToString(),
                    c => c.Tipos.Select(t => new { id = t.id, name = t.Name, peso = t.Peso })));
            ViewBag.FrequenciasJson = System.Text.Json.JsonSerializer.Serialize(
                frequencias.Select(f => new { id = f.id, name = f.Name, peso = f.Peso }));
            ViewBag.InstrucoesJson = System.Text.Json.JsonSerializer.Serialize(
                instrucoes.Select(i => new { id = i.id, name = i.Name, peso = i.Peso }));
            ViewBag.BlocosJson = System.Text.Json.JsonSerializer.Serialize(
                blocos.Select(b => new { blockId = b.BlockId, title = (b.Title ?? "").TrimStart('-', ' ') }));

            return View(paciente);
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> DetalharPrescricao(int prescricaoId)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                var prescricao = await _pacienteServices.BuscarPrescricaoCompleta(prescricaoId, email);
                if (prescricao == null)
                    return Json(new { success = false, message = "Prescrição não encontrada." });

                return Json(new
                {
                    success  = true,
                    id       = prescricao.Id,
                    data     = prescricao.DataCriacao.ToString("dd/MM/yyyy"),
                    ict      = prescricao.ICT,
                    adesao   = prescricao.Adesao,
                    medicamentos = prescricao.PrescricaoMeds.Select(rm => new
                    {
                        medicamento = rm.Medicamento.NOME_PRODUTO,
                        categoria   = rm.Categoria.Name,
                        tipo        = rm.Tipo.Name,
                        frequencia  = rm.Frequencia.Name,
                        instrucoes  = rm.InstrucoesMeds.Select(im => im.Instrucao.Name).ToList()
                    }),
                    cids = prescricao.PrescricaoCIDs.Select(rc => new
                    {
                        code  = rc.CategoriaCID.Code,
                        title = rc.CategoriaCID.Title?.TrimStart('-', ' ')
                    })
                });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao carregar prescrição." });
            }
        }

        [HttpGet]
        [SessionFilter]
        public async Task<JsonResult> ObterPrescricaoParaEdicao(int prescricaoId)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                var p = await _pacienteServices.BuscarPrescricaoCompleta(prescricaoId, email);
                if (p == null)
                    return Json(new { success = false, message = "Prescrição não encontrada." });

                return Json(new
                {
                    success = true,
                    id      = p.Id,
                    adesao  = p.Adesao,
                    medicamentos = p.PrescricaoMeds.Select(rm => new
                    {
                        medicamentoId    = rm.MedicamentoID,
                        medicamentoNome  = rm.Medicamento.NOME_PRODUTO,
                        categoriaId      = rm.CategoriaID,
                        categoriaNome    = rm.Categoria.Name,
                        subcategoriaId   = rm.TipoID,
                        subcategoriaNome = rm.Tipo.Name,
                        subcategoriaPeso = rm.Tipo.Peso,
                        frequenciaId     = rm.FrequenciaID,
                        frequenciaNome   = rm.Frequencia.Name,
                        frequenciaPeso   = rm.Frequencia.Peso,
                        instrucoes       = rm.InstrucoesMeds.Select(im => new
                        {
                            id   = im.InstrucaoId,
                            nome = im.Instrucao.Name,
                            peso = im.Instrucao.Peso
                        })
                    }),
                    cids = p.PrescricaoCIDs.Select(rc => new
                    {
                        id    = rc.CategoriaCID_ID,
                        code  = rc.CategoriaCID.Code,
                        title = rc.CategoriaCID.Title != null ? rc.CategoriaCID.Title.TrimStart('-', ' ') : ""
                    })
                });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao carregar prescrição para edição." });
            }
        }

        [HttpPost]
        [SessionFilter]
        public async Task<JsonResult> DeletarPrescricao(int prescricaoId)
        {
            try
            {
                var email = EmailSessao;
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Sessão expirada." });

                await _pacienteServices.DeletarPrescricao(prescricaoId, email);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Erro ao excluir prescrição." });
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
                    prescricoes = paciente.Prescricoes
                        .OrderByDescending(r => r.DataCriacao)
                        .Select(r => new
                        {
                            data   = r.DataCriacao.ToString("dd/MM/yyyy"),
                            ict    = r.ICT,
                            adesao = r.Adesao,
                            medicamentos = r.PrescricaoMeds.Select(rm => new
                            {
                                medicamento = rm.Medicamento.NOME_PRODUTO,
                                categoria   = rm.Categoria.Name,
                                tipo        = rm.Tipo.Name,
                                frequencia  = rm.Frequencia.Name,
                                instrucoes  = string.Join("; ", rm.InstrucoesMeds.Select(im => im.Instrucao.Name))
                            }),
                            cids = string.Join("; ", r.PrescricaoCIDs.Select(rc =>
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
