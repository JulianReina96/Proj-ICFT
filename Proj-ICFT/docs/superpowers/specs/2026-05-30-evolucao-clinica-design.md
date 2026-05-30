# Design — Registro e Acompanhamento de Evolução Clínica

**Projeto:** Proj-ICFT (TCC)
**Data:** 2026-05-30
**Autor:** Julian Reina (em colaboração com Claude)
**Status:** Aprovado para implementação

---

## 1. Contexto e motivação

O objetivo central declarado do TCC é:

> *"Unificar, automatizar e padronizar o processo de análise da complexidade terapêutica, correlacionando-o com dados de adesão e evolução clínica dos pacientes."*

O sistema atual já atende aos verbos "unificar", "automatizar" e "padronizar" via os requisitos implementados:

- **Unificar** — RF01 (cadastro de paciente), RF05 (receita com ICT), RF06 (visualização), RF12 (associação CID), RF14 (adesão).
- **Automatizar** — RF05 (cálculo automático), RF14 (classificação binária de adesão), RF07/09/13 (exportações).
- **Padronizar** — pesos cadastrados, regras MRCI, hierarquia CID-11, enum de classificação.

O ponto não coberto pelos requisitos existentes é o **acompanhamento longitudinal da evolução clínica**. Sem ele, o sistema mede a *prescrição* (complexidade) e a *intenção* (adesão), mas não o *desfecho clínico* (o paciente melhorou, piorou ou se manteve estável após o regime terapêutico?).

Esta especificação define um novo requisito funcional **RF16 — Registro e Acompanhamento de Evolução Clínica**:

> *"O sistema deverá registrar e acompanhar indicadores de evolução clínica associados ao CID e ao esquema terapêutico do paciente, permitindo análise longitudinal da relação entre complexidade terapêutica, adesão e desfechos clínicos."*

---

## 2. Objetivos da feature

1. Permitir ao profissional registrar evoluções clínicas de cada paciente ao longo do tempo, usando estrutura SOAP híbrida (texto livre + indicadores estruturados).
2. Vincular cada evolução opcionalmente a uma receita e a um CID monitorado, viabilizando análise por doença e por regime terapêutico.
3. Apresentar a evolução temporal em gráfico que cruza ICT (complexidade) com Status de evolução (desfecho), oferecendo visualização detalhada por indicador sob demanda.
4. Manter o isolamento por usuário já adotado no resto do sistema (RNF05).
5. Não introduzir alterações em entidades existentes além de propriedades de navegação inversas.

---

## 3. Decisões consolidadas

| Decisão | Escolha |
|---|---|
| Unidade de registro | Tabela `EvolucaoClinica` independente, com FK obrigatória pra `PacienteICT` e FK opcional pra `Receitum` |
| Schema management | Adotar EF Core Migrations a partir desta feature, com `InitialBaseline` vazia |
| Estrutura SOAP | Híbrido — S/A/P como textareas livres + Objetivo estruturado + enum `StatusEvolucao` |
| Campos do Objetivo | 5 sinais vitais + 3 laboratoriais + 3 contadores de impacto + observação livre |
| Vínculo CID | FK opcional para 1 CID monitorado (`CategoriaCID_ID`) |
| Fluxo de criação | Botão dedicado em VerPaciente + prompt opcional após `SalvarReceita` |
| Local de listagem | Nova aba "Evoluções" dentro de `VerPaciente.cshtml` |
| Visualização | Progressiva: gráfico padrão ICT × StatusEvolucao; "+ Detalhes" expande gráficos por indicador |
| `StatusEvolucao` | 4 estados — Inconclusivo (0), Melhorou (1), Estável (2), Piorou (3) |
| CRUD | Livre. Paciente deletado → cascade. Receita deletada → `ReceitaID` vira NULL na evolução |
| Resolução de usuário | Inline via `_db.Usuarios.FirstOrDefaultAsync(...)` (mesmo padrão de `PacientesServices`) |
| `IUsuarioService.GetUsuarioByEmail` | Não usado pela feature; bug existente permanece (não é pré-requisito) |
| Biblioteca de gráficos | Chart.js 4.x via CDN no `_Layout.cshtml` |

---

## 4. Modelo de dados

### 4.1 Nova entidade `EvolucaoClinica` (tabela `EvolucaoClinica`)

```csharp
namespace Proj_ICFT.ModelsNew;

[Table("EvolucaoClinica")]
public partial class EvolucaoClinica
{
    [Key] public int Id { get; set; }

    public int PacienteID { get; set; }
    public int UsuarioCriacaoID { get; set; }
    public int? ReceitaID { get; set; }
    public int? CategoriaCID_ID { get; set; }

    [Column(TypeName = "datetime")] public DateTime DataConsulta { get; set; }
    [Column(TypeName = "datetime")] public DateTime DataCriacao { get; set; }

    [StringLength(2000)] public string? Subjetivo { get; set; }
    [StringLength(2000)] public string? Avaliacao { get; set; }
    [StringLength(2000)] public string? Plano { get; set; }
    [StringLength(2000)] public string? ObservacaoObjetivo { get; set; }

    public StatusEvolucao Status { get; set; }

    public int? PaSistolica { get; set; }
    public int? PaDiastolica { get; set; }
    public int? FrequenciaCardiaca { get; set; }
    public double? Peso { get; set; }
    public int? Spo2 { get; set; }

    public double? Glicemia { get; set; }
    public double? Hba1c { get; set; }
    public double? Creatinina { get; set; }

    public int EventosAdversos { get; set; }
    public int Hospitalizacoes { get; set; }
    public int IdasEmergencia { get; set; }

    [ForeignKey("PacienteID")]       public virtual PacienteICT Paciente { get; set; } = null!;
    [ForeignKey("UsuarioCriacaoID")] public virtual Usuario UsuarioCriacao { get; set; } = null!;
    [ForeignKey("ReceitaID")]        public virtual Receitum? Receita { get; set; }
    [ForeignKey("CategoriaCID_ID")]  public virtual Categorias_CID? CategoriaCID { get; set; }
}
```

### 4.2 Novo enum `StatusEvolucao`

```csharp
public enum StatusEvolucao
{
    Inconclusivo = 0,
    Melhorou     = 1,
    Estavel      = 2,
    Piorou       = 3
}
```

Inconclusivo é o valor 0 para que seja o default seguro do EF Core em casos de primeira consulta sem desfecho claro.

### 4.3 Diagrama de relacionamentos (delta)

```
PacienteICT (1) ─────────── (N) EvolucaoClinica
                                  ├─(N:1 opcional)→ Receitum         [ON DELETE SET NULL]
                                  ├─(N:1 opcional)→ Categorias_CID   [ON DELETE SET NULL]
                                  └─(N:1 obrigatório)→ Usuario        [ON DELETE NO ACTION]
```

### 4.4 Propriedades de navegação inversas a adicionar

| Entidade existente | Adicionar |
|---|---|
| `PacienteICT` | `ICollection<EvolucaoClinica> EvolucaoClinicas` |
| `Receitum` | `ICollection<EvolucaoClinica> EvolucaoClinicas` |
| `Categorias_CID` | `ICollection<EvolucaoClinica> EvolucaoClinicas` |
| `Usuario` | `ICollection<EvolucaoClinica> EvolucaoClinicas` |

### 4.5 Notas de design

- `DataConsulta` é distinta de `DataCriacao` para permitir registro retroativo de consulta passada.
- Indicadores objetivos numéricos são `nullable` (ausência de medida ≠ medida zero); contadores de impacto são `int` não-nullable com default 0 (zero hospitalizações é informação).
- Status é persistido como `int` no banco para permitir filtros eficientes.
- Comprimento de 2000 chars nos campos textuais (nvarchar(2000)) suporta anotações clínicas sem custo de `nvarchar(MAX)`.

---

## 5. Backend

### 5.1 Estrutura de pastas

```
Services/EvolucaoClinicaServices/
  ├─ Interface/IEvolucaoClinicaServices.cs
  └─ Implementacao/EvolucaoClinicaServices.cs

Controllers/EvolucaoClinicaController.cs
Models/Request/EvolucaoRequest.cs
Models/ViewModels/EvolucaoViewModel.cs
```

### 5.2 Interface `IEvolucaoClinicaServices`

```csharp
public interface IEvolucaoClinicaServices
{
    Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string emailUsuario);
    Task<EvolucaoDetalheViewModel?> Detalhar(int id, string emailUsuario);
    Task<int> Salvar(SalvarEvolucaoRequest request, string emailUsuario);
    Task Atualizar(int id, SalvarEvolucaoRequest request, string emailUsuario);
    Task Deletar(int id, string emailUsuario);
    Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string emailUsuario);
    Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string emailUsuario);
}
```

### 5.3 DTO de entrada

```csharp
public record SalvarEvolucaoRequest(
    int PacienteID,
    int? ReceitaID,
    int? CategoriaCID_ID,
    DateTime DataConsulta,
    StatusEvolucao Status,
    string? Subjetivo, string? Avaliacao, string? Plano, string? ObservacaoObjetivo,
    int? PaSistolica, int? PaDiastolica, int? FrequenciaCardiaca, double? Peso, int? Spo2,
    double? Glicemia, double? Hba1c, double? Creatinina,
    int EventosAdversos, int Hospitalizacoes, int IdasEmergencia
);
```

### 5.4 ViewModels de saída

- `EvolucaoListagemViewModel` — linhas da tabela na aba: Id, DataConsulta, Status, StatusLabel, ReceitaID, IctDaReceita, ReceitaAdesao, CidCodigo, CidTitulo.
- `EvolucaoDetalheViewModel` — herda da listagem + todos os campos SOAP e indicadores.
- `CIDOpcaoViewModel` — Id, Codigo, Titulo (para popular o select de CID).
- `SerieTemporalViewModel` — listas paralelas alinhadas pelo índice da data:
  - `List<DateTime> Datas`
  - `List<double?> IctPorEvolucao`, `List<int> StatusPorEvolucao`
  - Uma `List<T?>` para cada um dos 8 indicadores numéricos
  - 3 listas para os contadores

### 5.5 Implementação `EvolucaoClinicaServices` — guard reutilizado

```csharp
private async Task<(Usuario, PacienteICT)> ResolverContexto(int pacienteId, string email)
{
    var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
        ?? throw new InvalidOperationException("Usuário não encontrado.");
    var paciente = await _db.PacienteICTs
        .FirstOrDefaultAsync(p => p.ID == pacienteId && p.UsuarioCriacaoID == usuario.id)
        ?? throw new InvalidOperationException("Paciente não encontrado ou sem permissão.");
    return (usuario, paciente);
}
```

Toda mutação invoca `ResolverContexto` para garantir posse do paciente. O service também valida que `ReceitaID` (quando enviado) pertence ao mesmo paciente e que `CategoriaCID_ID` (quando enviado) existe.

### 5.6 Controller `EvolucaoClinicaController`

| Rota | Método | Função |
|---|---|---|
| `/EvolucaoClinica/ListarPorPaciente?pacienteId=X` | GET | Lista da aba Evoluções |
| `/EvolucaoClinica/Detalhar?id=X` | GET | Detalhe completo (modal) |
| `/EvolucaoClinica/Salvar` | POST | Cria nova evolução |
| `/EvolucaoClinica/Atualizar?id=X` | POST | Edita |
| `/EvolucaoClinica/Deletar?id=X` | POST | Remove |
| `/EvolucaoClinica/ListarCIDsDoPaciente?pacienteId=X` | GET | CIDs já vinculados às receitas do paciente |
| `/EvolucaoClinica/SerieTemporal?pacienteId=X` | GET | Payload completo para os gráficos Chart.js |

Todos os endpoints exigem `[SessionFilter]`, leem `EmailSessao` (helper igual ao `PacienteController`) e devolvem `{ success, message, data }`.

### 5.7 Integração com `SalvarReceita`

A integração é **client-side**. Após o response de sucesso de `/Home/SalvarReceita`, o JS de `Form.cshtml` exibe um SweetAlert "Registrar evolução clínica agora?". Se confirmado, redireciona para `VerPaciente/{id}?abrirEvolucao=1&receitaId={novaReceitaId}`, e o JS de `VerPaciente.cshtml` lê a query string para abrir a aba e o modal pré-preenchido.

Motivação: evita acoplar `FormularioServices` ao `EvolucaoClinicaServices` e mantém a decisão ("quer registrar agora?") no front.

### 5.8 Registro DI em `Program.cs`

```csharp
builder.Services.AddScoped<IEvolucaoClinicaServices, EvolucaoClinicaServices>();
```

Posicionado junto aos demais `AddScoped` de serviços.

---

## 6. Frontend

### 6.1 Reestruturação de `Views/Paciente/VerPaciente.cshtml`

A view atual ganha estrutura de **Bootstrap nav-tabs**:

```html
<ul class="nav nav-tabs" id="pacienteTabs">
  <li class="nav-item">
    <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#tab-receitas">
      Receitas
    </button>
  </li>
  <li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-evolucoes">
      Evoluções Clínicas
    </button>
  </li>
</ul>

<div class="tab-content">
  <div class="tab-pane fade show active" id="tab-receitas">@* conteúdo atual *@</div>
  <div class="tab-pane fade" id="tab-evolucoes">
    <partial name="_EvolucoesTab" model="Model" />
  </div>
</div>
```

A aba Evoluções faz **lazy load**: o JS escuta `shown.bs.tab` e carrega dados via AJAX na primeira abertura.

### 6.2 Nova partial `Views/Paciente/_EvolucoesTab.cshtml`

Estrutura visível:

- Topo: botão `+ Nova Evolução` (lado esquerdo) e `Atualizar` (lado direito).
- Gráfico principal: `<canvas id="chartIctStatus">` (linha do ICT com pontos coloridos por Status).
- Toggle collapse "Ver gráficos detalhados de indicadores".
- Quando expandido: row Bootstrap com 4 canvas — PA+FC, Glicemia+HbA1c, Peso, Contadores de impacto.
- Tabela cronológica (`DataConsulta DESC`) com colunas: Data, Status, CID, Receita/ICT, Ações (👁/✏/🗑).
- Empty-state amigável quando não há evoluções.

### 6.3 Nova partial `Views/Paciente/_ModalEvolucao.cshtml`

Modal único Bootstrap que serve três modos via parâmetro JS: **criar**, **editar**, **ler**. Estrutura:

- Header: título dinâmico ("Nova Evolução" / "Editar Evolução" / "Detalhes da Evolução").
- Campos top-level:
  - `DataConsulta` (date picker, default = hoje).
  - Select de Receita (dropdown com receitas do paciente, formato "DD/MM/AAAA — ICT X.X", + opção "Nenhuma").
  - Select de CID (lista de CIDs já vinculados ao paciente via `ListarCIDsDoPaciente` + botão "+ Outro CID" que abre Select2 com busca completa).
  - Radio group `StatusEvolucao` (4 botões).
- Seção SOAP:
  - **Subjetivo** — textarea livre.
  - **Objetivo** — agrupado em 3 sub-seções:
    - Sinais Vitais: PA sistólica/diastólica, FC, Peso, SpO2.
    - Laboratoriais: Glicemia, HbA1c, Creatinina.
    - Desde a última consulta: Eventos adversos, Hospitalizações, Idas à emergência.
    - Observação livre.
  - **Avaliação** — textarea livre.
  - **Plano** — textarea livre.
- Footer: `Cancelar` + `Salvar Evolução` (em modo leitura: apenas `Fechar`).
- Validação client-side em campos numéricos (PA sistólica 50-300, FC 30-220, etc.).

### 6.4 Novo `wwwroot/js/evolucao-clinica.js`

Responsabilidades:

- `carregarEvolucoes(pacienteId)` — `Promise.all` para `ListarPorPaciente` e `SerieTemporal`; renderiza tabela e gráficos.
- `renderizarGraficoPrincipal(serie)` — Chart.js `type: 'line'` com ICT no eixo Y e pontos com `pointBackgroundColor` arrayificado por Status. `spanGaps: false` para honrar buracos quando `IctPorEvolucao[i] == null`.
- `renderizarGraficosDetalhados(serie)` — lazy render no evento `show.bs.collapse`. Quatro mini-charts: PA+FC, Glicemia+HbA1c, Peso, Contadores (barras agrupadas).
- `abrirModalEvolucao({ modo, id?, receitaIdPreSel?, cidPreSel? })` — três modos suportados; em editar/ler busca `Detalhar` antes.
- Handlers para criar, editar, deletar com SweetAlert2 de confirmação no delete (padrão já usado em `PacientesAnalisados.cshtml`).
- Leitura de query string `abrirEvolucao=1&receitaId=X` ao carregar a página para abrir modal pré-preenchido vindo de `Form.cshtml`.

### 6.5 Novo `wwwroot/css/evolucao-clinica.css`

Estilos específicos: cores do Status (verde Melhorou, amarelo Estável, vermelho Piorou, cinza Inconclusivo), espaçamento dos canvas, badge das linhas da tabela.

### 6.6 Mudança em `Views/Home/Form.cshtml`

Após o response de sucesso de `/Home/SalvarReceita`:

```js
const wantEvol = await Swal.fire({
  icon: 'question',
  title: 'Registrar evolução clínica?',
  text: 'Você pode registrar agora ou depois pela tela do paciente.',
  showCancelButton: true,
  confirmButtonText: 'Registrar agora',
  cancelButtonText: 'Depois'
});
window.location.href = wantEvol.isConfirmed
  ? `/Paciente/VerPaciente/${pacienteId}?abrirEvolucao=1&receitaId=${r.receitaId}`
  : `/Paciente/VerPaciente/${pacienteId}`;
```

### 6.7 Mudança em `Views/Shared/_Layout.cshtml`

Adicionar Chart.js 4.x via CDN, junto às libs já presentes globalmente:

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
```

---

## 7. Análise longitudinal e correlação ICT × Evolução

### 7.1 Leitura do gráfico principal (ICT × Status)

- **Eixo X**: datas das consultas (cronológico).
- **Eixo Y**: ICT (linha contínua).
- **Cor dos pontos**: dada pelo Status (verde/amarelo/vermelho/cinza).
- **Tooltip**: "DD/MM/AAAA — ICT 32.5 — Estável".

Padrões interpretáveis em defesa de TCC:

- ICT crescente + repetidos "Piorou" → polifarmácia sem ganho.
- ICT crescente + "Melhorou" → ajuste bem-sucedido.
- ICT decrescente + "Estável/Melhorou" → desprescrição bem-sucedida.
- ICT alto + "Inconclusivo" + alta `Hospitalizacoes` → bandeira de revisão.

### 7.2 Tratamento de pontos parciais

| Situação | Comportamento gráfico |
|---|---|
| Evolução sem `ReceitaID` | Ponto colorido pelo Status, mas linha do ICT quebra (gap honesto) |
| Indicador `null` em evolução | Linha daquele indicador quebra; demais continuam |
| Paciente com 1 evolução | Ponto isolado + aviso "Histórico curto — registre mais evoluções" |
| Paciente com 0 evoluções | Empty-state com CTA "Registrar primeira evolução" |

### 7.3 Construção da série temporal

`ObterSerieTemporal` ordena evoluções por `DataConsulta ASC`, carrega `Receita` via `Include`, e preenche listas paralelas indexadas pelo mesmo i. `IctPorEvolucao[i]` é `null` quando `e.Receita == null`.

### 7.4 Correlação CID × ICT × Evolução (insumo para RF13)

A query analítica que o RF13 (Relatórios — pendente) executará usa `EvolucaoClinica` como tabela-fato:

```sql
SELECT cid.Code, cid.Title,
       AVG(r.ICT) AS IctMedio,
       SUM(CASE WHEN e.Status = 1 THEN 1 ELSE 0 END) AS QtdMelhorou,
       SUM(CASE WHEN e.Status = 3 THEN 1 ELSE 0 END) AS QtdPiorou,
       SUM(e.Hospitalizacoes) AS TotalHospitalizacoes,
       SUM(e.EventosAdversos) AS TotalEventosAdversos
FROM EvolucaoClinica e
JOIN Categorias_CID cid ON e.CategoriaCID_ID = cid.Id
LEFT JOIN Receita r     ON e.ReceitaID = r.Id
WHERE e.PacienteID IN (pacientes do usuário logado)
GROUP BY cid.Code, cid.Title
ORDER BY IctMedio DESC;
```

Sem `EvolucaoClinica`, o RF13 só consegue agregar dados de receita — não consegue medir desfecho.

### 7.5 Critérios mínimos defensáveis para a dissertação

Não viram regras do sistema, mas devem ser anotados na seção "Limitações" do TCC:

- Pacientes acompanhados: ≥ 10.
- Evoluções por paciente: ≥ 3.
- Período de acompanhamento: ≥ 3 meses.
- Distribuição de Status: não 100% "Inconclusivo".
- Pelo menos 1 indicador objetivo preenchido em ≥ 50% das evoluções.

### 7.6 Resposta ao objetivo do TCC

| Verbo | Como o sistema responde |
|---|---|
| Unificar | Paciente, CID, Receita (ICT), Adesão e Evolução em estrutura única conectada por FKs |
| Automatizar | Cálculo ICT (MRCI) automatizado; gráficos gerados sem Excel; relatórios consolidam |
| Padronizar | Pesos fixos no banco; Status em enum; CID-11; unidades únicas |
| Correlacionar adesão × evolução | JOIN trivial entre `EvolucaoClinica` e `Receitum` |
| Acompanhar evolução clínica | `EvolucaoClinica` é a entidade que completa o requisito |

---

## 8. Migrations

### 8.1 Adoção de migrations num projeto DB-first

O projeto nunca usou migrations (confirmado pela ausência da pasta `Migrations/` e da tabela `__EFMigrationsHistory`). O processo de adoção segue três passos:

**Passo 1 — Baseline vazia:**

```powershell
dotnet ef migrations add InitialBaseline
```

Gera `InitialBaseline.cs` cheio de `CreateTable`. Esvaziar manualmente:

```csharp
public partial class InitialBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) { }
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
```

**Passo 2 — Marcar baseline como aplicada:**

```powershell
dotnet ef database update
```

Cria `__EFMigrationsHistory` (se não existir) com a linha de `InitialBaseline`. Nenhuma alteração de schema é feita.

**Passo 3 — Migration de fato:**

```powershell
dotnet ef migrations add AddEvolucaoClinica
dotnet ef migrations script    # revisar SQL antes
dotnet ef database update
```

### 8.2 Configuração em `OnModelCreating`

```csharp
modelBuilder.Entity<EvolucaoClinica>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.HasOne(e => e.Paciente).WithMany(p => p.EvolucaoClinicas)
        .HasForeignKey(e => e.PacienteID)
        .OnDelete(DeleteBehavior.Cascade)
        .HasConstraintName("FK_EvolucaoClinica_PacienteICT");

    entity.HasOne(e => e.Receita).WithMany(r => r.EvolucaoClinicas)
        .HasForeignKey(e => e.ReceitaID)
        .OnDelete(DeleteBehavior.SetNull)
        .HasConstraintName("FK_EvolucaoClinica_Receita");

    entity.HasOne(e => e.CategoriaCID).WithMany(c => c.EvolucaoClinicas)
        .HasForeignKey(e => e.CategoriaCID_ID)
        .OnDelete(DeleteBehavior.SetNull)
        .HasConstraintName("FK_EvolucaoClinica_Categorias_CID");

    entity.HasOne(e => e.UsuarioCriacao).WithMany(u => u.EvolucaoClinicas)
        .HasForeignKey(e => e.UsuarioCriacaoID)
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("FK_EvolucaoClinica_Usuario");

    entity.HasIndex(e => e.PacienteID).HasDatabaseName("IX_EvolucaoClinica_PacienteID");
    entity.HasIndex(e => new { e.PacienteID, e.DataConsulta })
          .HasDatabaseName("IX_EvolucaoClinica_Paciente_Data");

    entity.Property(e => e.Status).HasConversion<int>();
});
```

E adicionar `DbSet<EvolucaoClinica> EvolucaoClinicas` ao DbContext.

### 8.3 Risco "multiple cascade paths"

`PacienteICT` → `Receitum` → `EvolucaoClinica` (cascade) + `PacienteICT` → `EvolucaoClinica` (cascade direto) pode gerar erro de "multiple cascade paths" no SQL Server. Se acontecer ao rodar `database update`:

- Trocar `OnDelete(Cascade)` para `Restrict` na relação `PacienteICT → EvolucaoClinica`.
- Implementar deleção em cascata via aplicação no método de deleção de paciente (padrão já usado em outros pontos do projeto para Receita).

### 8.4 Rollback

```powershell
# Reverte só a feature
dotnet ef database update InitialBaseline
Remove-Item Migrations/*_AddEvolucaoClinica*

# Reverte adoção de migrations (cenário extremo)
# - DELETE FROM __EFMigrationsHistory via SSMS
# - Remove-Item Migrations -Recurse
```

---

## 9. Mapa completo de arquivos

| Categoria | Arquivo | Ação |
|---|---|---|
| Modelo | `ModelsNew/EvolucaoClinica.cs` | Criar |
| Modelo | `ModelsNew/StatusEvolucao.cs` | Criar |
| Modelo | `ModelsNew/PacienteICT.cs` | Modificar — `ICollection<EvolucaoClinica>` |
| Modelo | `ModelsNew/Receitum.cs` | Modificar — `ICollection<EvolucaoClinica>` |
| Modelo | `ModelsNew/Categorias_CID.cs` | Modificar — `ICollection<EvolucaoClinica>` |
| Modelo | `ModelsNew/Usuario.cs` | Modificar — `ICollection<EvolucaoClinica>` |
| DbContext | `DataNew/AppDbContextNew.cs` | Modificar — DbSet + OnModelCreating |
| Migration | `Migrations/<ts>_InitialBaseline.cs` | Criar (esvaziar Up/Down) |
| Migration | `Migrations/<ts>_AddEvolucaoClinica.cs` | Criar (gerado pelo EF) |
| Migration | `Migrations/AppDbContextNewModelSnapshot.cs` | Criar (gerado pelo EF) |
| Service | `Services/EvolucaoClinicaServices/Interface/IEvolucaoClinicaServices.cs` | Criar |
| Service | `Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs` | Criar |
| DTO/VM | `Models/Request/EvolucaoRequest.cs` | Criar |
| DTO/VM | `Models/ViewModels/EvolucaoViewModel.cs` | Criar |
| Controller | `Controllers/EvolucaoClinicaController.cs` | Criar |
| DI | `Program.cs` | Modificar — AddScoped |
| View | `Views/Paciente/VerPaciente.cshtml` | Modificar — nav-tabs |
| View | `Views/Paciente/_EvolucoesTab.cshtml` | Criar |
| View | `Views/Paciente/_ModalEvolucao.cshtml` | Criar |
| View | `Views/Home/Form.cshtml` | Modificar — Swal pós-SalvarReceita |
| Layout | `Views/Shared/_Layout.cshtml` | Modificar — Chart.js CDN |
| JS | `wwwroot/js/evolucao-clinica.js` | Criar |
| CSS | `wwwroot/css/evolucao-clinica.css` | Criar |
| Docs | `CLAUDE.md` | Atualizar (entidade, enum, endpoints, requisito) |
| Docs | `PROXIMOS_PASSOS.md` | Atualizar (marcar RF16 como implementado, reordenar prioridades) |

**Total: 25 arquivos** (14 novos + 11 modificados).

---

## 10. Riscos e mitigações

| Risco | Probabilidade | Mitigação |
|---|---|---|
| Baseline esvaziada errado → EF tenta recriar tabelas existentes | Média | Conferir `Up()/Down()` vazios antes de `database update`. Rollback com `database update 0`. |
| Snapshot do EF divergir do schema real | Média | Comparar snapshot com SSMS antes de gerar `AddEvolucaoClinica`; ajustar `OnModelCreating` se necessário. |
| Erro "multiple cascade paths" no SQL Server | Média-Alta | Trocar Paciente→Evolução para `Restrict` e cascatear via aplicação. |
| Dev sem permissão de DDL | Baixa | Gerar `migrations script` e aplicar manualmente via DBA. |
| Esquecer `dotnet ef database update` em outra máquina de dev | Alta | Documentar no README pós-pull. |
| Bug do `GetUsuarioByEmail` ressurgir em código futuro | Baixa | Continua não usado por esta feature; permanece como tech debt rastreado no `CLAUDE.md`. |
| Evolução vinculada a receita deletada via cascade do Paciente | Baixa | Comportamento esperado: deletar paciente apaga tudo. Aviso já existe na deleção de paciente. |
| Performance da SerieTemporal com muitas evoluções | Baixa (TCC) | Adequado para TCC. Em produção, paginar ou agregar por mês. |

---

## 11. Fora de escopo (delimitação explícita)

Os itens abaixo **não** fazem parte desta feature:

- Análise estatística inferencial (p-valor, Pearson) — pertence ao RF13 ou ao TCC (planilha externa).
- Gráficos comparativos entre pacientes — pertence ao RF13.
- Alertas automáticos por threshold (ex.: PA > 180) — risco regulatório.
- Importação de exames laboratoriais (PDF/HL7) — fora do escopo de TCC.
- Audit trail / versionamento de edição — CRUD livre foi adotado.
- Notificações de "tempo desde última consulta" — futuro.
- Formulário condicional por CID (campos específicos por doença) — escopo inviável.
- Correção do bug `IUsuarioService.GetUsuarioByEmail` — não é pré-requisito desta feature.

---

## 12. Critérios de aceitação

A feature está completa quando:

1. Profissional consegue criar, listar, editar e deletar evoluções de seus pacientes.
2. Modal SOAP aceita preenchimento parcial (campos opcionais funcionam sem erro).
3. Tentativa de acessar evolução de paciente que não é do usuário logado retorna erro.
4. Aba "Evoluções" em VerPaciente é carregada via lazy load (não impacta tempo de carga inicial da página).
5. Gráfico principal (ICT × Status) renderiza corretamente com pontos coloridos pelo Status e gaps onde `ReceitaID == null`.
6. Gráficos detalhados são renderizados apenas quando o collapse é aberto pela primeira vez.
7. Prompt opcional aparece após `SalvarReceita` em `Form.cshtml`; redirecionamento e pré-preenchimento funcionam.
8. Deletar paciente cascateia evoluções; deletar receita preserva evolução com `ReceitaID = NULL`.
9. Migrations `InitialBaseline` e `AddEvolucaoClinica` aplicam sem erros e geram exatamente as colunas/índices especificados na seção 4 e 8.
10. `CLAUDE.md` e `PROXIMOS_PASSOS.md` refletem o novo estado.

---

## 13. Próximo passo

Esta especificação serve de entrada para a skill `superpowers:writing-plans`, que produzirá o plano de implementação passo-a-passo com tarefas verificáveis.
