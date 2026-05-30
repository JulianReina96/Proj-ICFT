# Próximos Passos — Requisitos Pendentes
**Projeto:** Proj-ICFT (TCC)  
**Última revisão:** 24/05/2026  
**Referência:** Requisitos Funcionais e Não-Funcionais do sistema ICFT

---

## Visão Geral do Estado Atual

| # | Requisito | Estado |
|---|-----------|--------|
| RF01 | Cadastro de Paciente | ✅ Implementado |
| RF02 | Listagem de Pacientes | ✅ Implementado |
| RF03 | Autenticação e Acesso Seguro | ✅ Implementado |
| RF04 | Exclusão de Paciente | ✅ Implementado ¹ |
| RF05 | Cadastro de Receita com Cálculo de Complexidade | ✅ Implementado |
| RF06 | Visualização de Receitas por Paciente | ✅ Implementado |
| RF07 | Exportação de Receita em PDF (na criação) | ✅ Implementado |
| RF08 | Cadastro de Usuários | ✅ Implementado |
| RF09 | Exportação de Receitas em Excel por Paciente | ✅ Implementado |
| RF10 | Exibição de Pesos no Cadastro da Receita | ✅ Implementado |
| RF11 | **Consulta e Seleção de Medicamentos ("Outros")** | ⚠️ Parcial |
| RF12 | Associação de Receita a Doença (CID) | ✅ Implementado |
| RF13 | **Geração de Relatórios Analíticos Exportáveis** | ❌ Pendente |
| RF14 | Classificação de Adesão da Receita | ✅ Implementado |
| RF15 | Cadastro de Receita com Paciente Anônimo | ✅ Implementado |
| RNF01 | **Confirmação de Ações Críticas** | ⚠️ Parcial |
| RNF02 | Recuperação e Troca de Senha | ✅ Implementado ² |
| RNF03 | **Exportação de Receitas Salvas em PDF** | ❌ Pendente |
| RNF04 | Feedback Imediato de Operações | ✅ Implementado |
| RNF05 | Isolamento e Privacidade de Dados por Usuário | ✅ Implementado |
| RNF06 | **Prevenção de Cadastro Duplicado de Pacientes** | ❌ Pendente |

> ¹ **RF04 — Divergência de regra:** O requisito original diz "somente se não tiver receitas vinculadas". A implementação atual faz **cascade delete** — exibe aviso ao usuário sobre as receitas que serão perdidas, mas permite a exclusão. Avaliar com o orientador se é necessário reverter para o comportamento restritivo.

> ² **RNF02 — Implementação client-side:** A recuperação de senha usa o Firebase SDK JavaScript diretamente na view (`ForgotPassword.cshtml`), sem passar pelo servidor C#. Funciona, mas difere da abordagem planejada (POST no `AccountController`). Não requer alteração.

---

## Bug identificado — Label "Aderiu" em VerPaciente

**Arquivo:** `Views/Paciente/VerPaciente.cshtml` — linhas 147-155  
**Problema:** O ramo `else` (paciente **não** aderiu) exibe o texto "Aderiu" em vez de "Não aderiu".

```html
@if (r.Adesao)
{
    <i class="bi bi-check-circle-fill"></i> <span>Aderiu</span>
}
else
{
    <i class="bi bi-x-circle-fill"></i> <span>Aderiu</span>  ← deveria ser "Não aderiu"
}
```

**Correção:** Alterar o `<span>` dentro do `else` para `Não aderiu`.

---

## Itens Pendentes — Detalhamento e Próximos Passos

---

### RF11 — Consulta e Seleção de Medicamentos: opção "Outros"
**Regra:** Caso o medicamento não esteja na base, o usuário deve poder selecionar "Outros" e informar o nome manualmente.

**O que está funcionando:** Busca AJAX com Select2 implementada (`/Medicamentos/BuscarMedicamentos`), paginação e cache.

**O que falta:** A opção "Outros" com campo de entrada manual não existe. Se o medicamento não aparecer na busca, o usuário não tem saída.

**Próximos passos:**
1. Em `MedicamentosController.BuscarMedicamentos`, sempre incluir ao final do array de resultados o item `{ id = -1, text = "Outros (informar manualmente)" }` — independente do termo buscado
2. No JS do Select2 em `Form.cshtml`, detectar seleção de `id == -1` e exibir `<input type="text" id="remedioManual" placeholder="Nome do medicamento...">` abaixo do select (ocultar quando outro valor for selecionado)
3. Ao adicionar o medicamento, se `remedioId == -1`, usar o texto do campo manual como `remedioNome` e enviar `medicamentoId: null` no payload
4. Em `FormularioServices.SalvarReceita`, aceitar `MedicamentoID` nulo — avaliar se `ReceitaMed.MedicamentoID` deve ser anulável no model (`ModelsNew/ReceitaMed.cs`) ou se cria um registro "Outros" fixo na tabela `Medicamentos` (abordagem mais simples, sem alteração de migration)

**Arquivos a modificar:**
- `Controllers/MedicamentosController.cs`
- `Views/Home/Form.cshtml`
- `ModelsNew/ReceitaMed.cs` (se optar por FK anulável)
- `Services/FormularioServices/Implementacao/FormularioServices.cs`

---

### RF13 — Geração de Relatórios Analíticos Exportáveis
**Regra:** Relatórios relacionais exportáveis (CSV/Excel) com métricas como % adesão, complexidade por paciente, distribuição por via de administração, etc.

**O que falta:** Feature completamente ausente — sem controller, serviço, view ou exportação.

**Próximos passos:**
1. Criar `RelatorioController` com endpoints para cada relatório
2. Criar `RelatorioServices` (Interface + Implementação) com queries agregadas via EF Core
3. Métricas sugeridas conforme requisito:
   - Adesão por paciente: `% receitas com Adesao = true` por paciente
   - Complexidade média: `AVG(ICT)` por paciente e geral
   - Distribuição por categoria/tipo de medicamento
   - Receitas por período (mês/ano)
   - Diagnósticos mais frequentes (CID mais associado)
4. Exportar em Excel com `xlsx.full.min.js` já carregado globalmente no layout
5. Criar `Views/Relatorio/Index.cshtml` com seleção de filtros e botões de exportação
6. Registrar `RelatorioServices` no `Program.cs`

**Arquivos a criar/modificar:**
- `Controllers/RelatorioController.cs`
- `Services/RelatorioServices/Interface/IRelatorioServices.cs`
- `Services/RelatorioServices/Implementacao/RelatorioServices.cs`
- `Views/Relatorio/Index.cshtml`
- `Program.cs`

---

### RNF01 — Confirmação de Ações Críticas
**Regra:** Exibir diálogo de confirmação *antes* de efetivar ações críticas.

**O que está funcionando:** Exclusão de paciente em `PacientesAnalisados.cshtml` já exibe SweetAlert2 com aviso detalhado sobre receitas vinculadas antes de efetivar o DELETE.

**O que falta:** Confirmação *antes* de salvar a receita em `Form.cshtml`. O botão "Obter ICT" chama `fetch('/Home/SalvarReceita')` diretamente, sem nenhuma etapa de confirmação.

**Próximos passos:**
1. Em `Form.cshtml`, dentro do handler `$exportarBtn.on('click', ...)`, antes do `fetch('/Home/SalvarReceita')`, adicionar:
   ```js
   const pacienteNome = $('#pacienteAnonimo').is(':checked')
       ? 'Paciente Anônimo'
       : ($('#pacienteId option:selected').text().trim() || 'não identificado');

   const confirm = await Swal.fire({
       icon: 'question',
       title: 'Confirmar salvamento?',
       html: '<p>Paciente: <strong>' + pacienteNome + '</strong></p>' +
             '<p>' + medicamentosAdicionados.length + ' medicamento(s) serão salvos.</p>',
       showCancelButton: true,
       confirmButtonText: 'Salvar receita',
       cancelButtonText: 'Revisar',
       confirmButtonColor: '#198754'
   });
   if (!confirm.isConfirmed) return;
   ```
2. Tornar o handler `async` (já que usa `await Swal.fire`)

**Arquivos a modificar:**
- `Views/Home/Form.cshtml`

---

### RNF03 — Exportação de Receitas Salvas em PDF
**Regra:** O PDF deve ser gerado a partir de receitas já salvas no histórico do paciente.

**O que está funcionando:** PDF gerado no momento da criação (`Form.cshtml` + html2pdf.js). A view `VerPaciente.cshtml` já possui todos os dados necessários via `DetalharReceita` (medicamentos, CIDs, ICT, adesão, data).

**O que falta:** O modal de detalhes de `VerPaciente.cshtml` não tem botão "Gerar PDF". O `modal-footer` contém apenas o botão "Fechar".

**Próximos passos:**
1. No `modal-footer` do modal `#modalReceita` em `VerPaciente.cshtml`, adicionar:
   ```html
   <button type="button" class="btn btn-primary" id="btnGerarPdfReceita" onclick="gerarPdfReceita()" style="display:none">
       <i class="bi bi-file-earmark-pdf"></i> Gerar PDF
   </button>
   ```
2. Em `abrirReceita(id)`, após montar o HTML do modal (`body.innerHTML = ...`), exibir o botão e armazenar os dados da receita em uma variável de escopo externo (ex.: `let receitaAtual = data`)
3. Implementar `gerarPdfReceita()` montando um template HTML com os dados de `receitaAtual` (mesma estrutura do PDF gerado em `Form.cshtml`) e chamando `html2pdf().from(html).save()`
4. Ao fechar o modal, ocultar o botão e limpar `receitaAtual`

**Arquivos a modificar:**
- `Views/Paciente/VerPaciente.cshtml`

---

### RNF06 — Prevenção de Cadastro Duplicado de Pacientes
**Regra:** Impedir cadastro duplicado de pacientes para o mesmo usuário.

**Situação atual:** `PacientesServices.CadastrarPaciente` não realiza nenhuma verificação de duplicidade — insere diretamente sem checar se já existe um paciente com o mesmo nome para aquele usuário.

**Decisão necessária antes de implementar:**
Avaliar com o orientador se:
- A deduplicação usa apenas o **nome exato** (case-insensitive) — sem impacto no banco, menos rigoroso, mas cobre a maioria dos casos
- Ou o **CPF** deve ser adicionado ao modelo — exige migration de banco, dado sensível, conflita com pacientes anônimos

**Se a decisão for validar por nome (caminho mais simples):**
1. Em `PacientesServices.CadastrarPaciente`, antes de inserir, verificar:
   ```csharp
   var existe = await _appDbContextNew.PacienteICTs
       .AnyAsync(p => p.UsuarioCriacaoID == user.id
                   && p.NomePaciente.ToLower() == nome.ToLower());
   if (existe) throw new InvalidOperationException("Já existe um paciente com esse nome.");
   ```
2. O controller já captura `InvalidOperationException` e retorna `{ success: false, message }` — sem alteração necessária

**Se a decisão for adicionar CPF:**
1. Adicionar `CPF string(14)?` na entidade `PacienteICT` (`ModelsNew/PacienteICT.cs`)
2. Criar migration: `Add-Migration AddCPFToPaciente`
3. Atualizar modal "Novo Paciente" em `PacientesAnalisados.cshtml` com campo CPF + máscara
4. Validar unicidade no serviço e atualizar o record `CadastroPacienteRequest` em `PacienteController.cs`

**Arquivos a modificar:**
- `Services/PacienteServices/Implementacao/PacientesServices.cs`
- `ModelsNew/PacienteICT.cs` (somente se CPF)
- `Controllers/PacienteController.cs` (somente se CPF)
- `Views/Paciente/PacientesAnalisados.cshtml` (somente se CPF)

---

## Ordem de Prioridade Sugerida

| Prioridade | Item | Justificativa |
|------------|------|---------------|
| 1 | Bug: label "Não aderiu" em VerPaciente | Correção trivial, 1 linha, exibição incorreta ao usuário |
| 2 | RNF01 — Confirmação ao Salvar Receita | Pequena adição em Form.cshtml, alta relevância para UX |
| 3 | RNF03 — PDF do Histórico | Reutiliza html2pdf.js e dados já disponíveis via DetalharReceita |
| 4 | RF11 — Opção "Outros" em Medicamentos | Requer decisão sobre FK nula em ReceitaMed |
| 5 | RNF06 — Deduplicação de Pacientes | Requer decisão arquitetural (nome vs. CPF) |
| 6 | RF13 — Relatórios Analíticos | Maior escopo, implementar por último |
