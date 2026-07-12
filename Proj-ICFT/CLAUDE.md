# Agente de Desenvolvimento — Proj-ICFT (TCC)

Você é um desenvolvedor experiente participando ativamente do projeto **Proj-ICFT** — um sistema acadêmico de cálculo do **Índice de Complexidade Terapêutica (ICT)** para TCC de graduação/pós.

Você não apenas responde perguntas. Você participa do projeto: questiona decisões, identifica riscos, aponta impactos, e executa implementações com contexto real. Antes de implementar qualquer coisa, entenda a direção da evolução do sistema.

---

## O que é o sistema

O **ICFT** é uma ferramenta usada por profissionais de saúde (médicos, farmacêuticos) para avaliar a **complexidade terapêutica** de um paciente com base nos medicamentos que ele usa. O sistema:

1. Autentica o profissional via **Firebase**
2. Apresenta um formulário onde o profissional seleciona medicamentos, com categorias, tipos, frequências e instruções adicionais
3. Cada combinação tem pesos cadastrados no banco
4. O sistema calcula o **score ICT** somando esses pesos
5. O score é associado a um paciente e salvo como uma **Receita** com o campo `ICT`
6. O sistema também permite associar diagnósticos **CID-11** à receita

O CID (Classificação Internacional de Doenças, versão 11) é uma hierarquia: **Capítulo → Bloco → Categoria**. A `Categoria_CID` é o nível folha e é o que é associado à receita.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 8.0 MVC |
| ORM | Entity Framework Core 9.0 |
| Banco | SQL Server (connection string em appsettings.json) |
| Autenticação | Firebase Authentication + JWT Bearer |
| Frontend | Razor Views + Bootstrap + JavaScript vanilla |
| DI Container | .NET built-in (Program.cs) |

---

## Arquitetura

```
Views (Razor) ←→ Controllers ←→ Services (Interface + Implementação) ←→ AppDbContextNew (EF Core) ←→ SQL Server
```

**Padrões adotados:**
- Services com interfaces (ex: `IFormularioServices` / `FormularioServices`)
- Injeção de dependência via construtor
- ViewModels para transferência de dados entre Controller e View
- `SessionFilter` como action filter para proteger rotas autenticadas
- `AppDbContextNew` é o **único DbContext ativo** (em `DataNew/`)

**Diretórios importantes:**
```
Controllers/          ← HomeController, AccountController, PacienteController, CIDController, MedicamentosController
ModelsNew/            ← Entidades EF Core (fonte de verdade do banco)
DataNew/              ← AppDbContextNew (DbContext ativo)
Services/             ← Lógica de negócio (FormularioServices, PacienteServices, CIDServices, MedicamentosServices, UsuarioService)
Models/               ← ViewModels, DTOs, modelos de filtros
Views/                ← Razor Views por controller
```

> ⚠️ A pasta `Data/` (antiga) foi **excluída da compilação** no `.csproj`. Ignore tudo que estiver lá — o sistema migrou para `DataNew/AppDbContextNew`.

---

## Banco de Dados — Estrutura Completa

Esta é a **principal fonte de verdade do domínio**. Mudanças no banco impactam diretamente serviços, endpoints, ViewModels e telas.

### Diagrama de Relacionamentos

```
Usuario (1)
  ├─(1:N)→ PacienteICT
  └─(1:N)→ Receita

PacienteICT (1)
  ├─(N:1)→ Usuario          [UsuarioCriacaoID]
  └─(1:N)→ Receita

Receita (1)
  ├─(N:1)→ PacienteICT     [PacienteID]
  ├─(N:1)→ Usuario          [UsuarioCriacaoID]
  ├─(1:N)→ ReceitaMed
  └─(1:N)→ ReceitaCID

ReceitaMed (1)
  ├─(N:1)→ Receita          [ReceitaID]
  ├─(N:1)→ Medicamento      [MedicamentoID]
  ├─(N:1)→ Categoria        [CategoriaID]
  ├─(N:1)→ Tipo             [TipoID]
  ├─(N:1)→ Frequencia       [FrequenciaID]
  └─(1:N)→ InstrucoesMed

InstrucoesMed (M:N pivot)
  ├─(N:1)→ ReceitaMed       [Med_ReceitaID]
  └─(N:1)→ InstrucoesAdicionais [InstrucaoId]

ReceitaCID (M:N pivot)
  ├─(N:1)→ Receita          [ReceitaID]
  └─(N:1)→ Categorias_CID   [CategoriaCID_ID]

Capitulos_CID (1)
  ├─(1:N)→ Blocos_CID       [ChapterNo]
  └─(1:N)→ Categorias_CID   [ChapterNo]

Blocos_CID (1)
  └─(N:1)→ Capitulos_CID    [ChapterNo]

Categorias_CID (1)
  ├─(N:1)→ Capitulos_CID    [ChapterNo]
  └─(1:N)→ ReceitaCID
```

### Entidades detalhadas

#### `Usuarios` → classe `Usuario`
| Campo | Tipo | Notas |
|---|---|---|
| `id` | int PK | |
| `Usuario1` | string(100) UNIQUE | mapeado para coluna "Usuario" — é o email do Firebase |
| `DataCriacao` | datetime | |

#### `PacienteICT` → classe `PacienteICT`
| Campo | Tipo | Notas |
|---|---|---|
| `ID` | int PK | |
| `NomePaciente` | string(100) | |
| `DataCriacao` | datetime | |
| `UsuarioCriacaoID` | int FK→Usuarios | |
| `Sexo` | char(1) | "M" ou "F" |
| `Idade` | int | |

#### `Receita` → classe `Receitum`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `PacienteID` | int FK→PacienteICT | |
| `UsuarioCriacaoID` | int FK→Usuarios | |
| `DataCriacao` | datetime | |
| `Adesao` | bool | adesão ao tratamento |
| `ICT` | double | score calculado — soma dos pesos |

#### `Categoria` → classe `Categorium`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `Name` | string(50) | |
| navigation | `ICollection<Tipo>` | tipos filhos |

#### `Tipo` → classe `Tipo`
| Campo | Tipo | Notas |
|---|---|---|
| `id` | int PK | |
| `Name` | string(50) | |
| `CategoriaId` | int FK→Categoria | |
| `Peso` | int | **componente do cálculo ICT** |

#### `Frequencia` → classe `Frequencium`
| Campo | Tipo | Notas |
|---|---|---|
| `id` | int PK | |
| `Name` | string(50) | |
| `Peso` | double | **componente do cálculo ICT** |

#### `InstrucoesAdicionais` → classe `InstrucoesAdicionai`
| Campo | Tipo | Notas |
|---|---|---|
| `id` | int PK | |
| `Name` | string(100) | |
| `Peso` | int | **componente do cálculo ICT** |

#### `Medicamentos` → classe `Medicamento`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `NOME_PRODUTO` | string(255) | |
| `PRINCIPIO_ATIVO` | string? | |
| `CATEGORIA_REGULATORIA` | string(100)? | |
| `CLASSE_TERAPEUTICA` | string(500)? | |
| `NUMERO_REGISTRO_PRODUTO` | string(50)? | |
| `EMPRESA_DETENTORA_REGISTRO` | string(255)? | |
| `SITUACAO_REGISTRO` | string(100)? | filtrado por "ATIVO" ou "VÁLIDO" |

#### `ReceitaMed` (linha de prescrição)
| Campo | Tipo | Notas |
|---|---|---|
| `ID` | int PK | |
| `ReceitaID` | int FK→Receita | |
| `MedicamentoID` | int FK→Medicamentos | |
| `CategoriaID` | int FK→Categoria | |
| `TipoID` | int FK→Tipo | |
| `FrequenciaID` | int FK→Frequencia | |

#### `InstrucoesMed` (pivot: ReceitaMed ↔ InstrucoesAdicionais)
| Campo | Tipo |
|---|---|
| `id` | int PK |
| `Med_ReceitaID` | int FK→ReceitaMed |
| `InstrucaoId` | int FK→InstrucoesAdicionais |

#### `ReceitaCID` (pivot: Receita ↔ Categorias_CID)
| Campo | Tipo |
|---|---|
| `Id` | int PK |
| `ReceitaID` | int FK→Receita |
| `CategoriaCID_ID` | int FK→Categorias_CID |

#### `Capitulos_CID`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `ChapterNo` | string(10) UNIQUE | chave de negócio usada como FK |
| `Title` | string(500)? | em português |
| `TitleEN` | string(500)? | em inglês |
| `FoundationURI` | string(255)? | |
| `LinearizationURI` | string(255)? | |

#### `Blocos_CID`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `BlockId` | string(50)? UNIQUE | chave de negócio |
| `Title` | string(500)? | |
| `TitleEN` | string(500)? | |
| `ChapterNo` | string(10)? FK→Capitulos_CID | |
| `Grouping1` | string(100)? | |
| `Grouping2` | string(100)? | |

#### `Categorias_CID` (folhas — associadas às receitas)
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `Code` | string(50)? | código CID ex: "A00.0" |
| `Title` | string(500)? | |
| `TitleEN` | string(500)? | |
| `BlockId` | string(50)? | ref ao bloco (não FK mapeada) |
| `ChapterNo` | string(10) FK→Capitulos_CID | |
| `IsLeaf` | bool? | true = pode ser associado à receita |

---

## Cálculo do ICT

```
ICT = Σ (por medicamento na receita):
        Peso do Tipo (subcategoria)
      + Peso da Frequência
      + Σ Peso de cada InstruçãoAdicional
```

O campo `Receita.ICT` armazena o score total. `RemedioViewModel.PesoTotal` calcula parcialmente por medicamento. `ReceitaViewModel.PesoTotal` soma todos os medicamentos.

---

## Endpoints Disponíveis

### HomeController
| Rota | Método | Descrição |
|---|---|---|
| `/` | GET | Página inicial |
| `/Home` | GET | Home page |
| `/Form` | GET [SessionFilter] | Formulário principal — carrega FormularioViewModel |
| `/ExportarDados` | POST [SessionFilter] | Recebe ExportacaoModel, calcula ICT (⚠️ não persiste) |
| `/ObterSubcategorias?categoriaId=` | GET [SessionFilter] | Tipos por categoria (JSON) |

### AccountController
| Rota | Método | Descrição |
|---|---|---|
| `/Account/Login` | GET | Tela de login |
| `/Account/Register` | GET [SessionFilter] | Tela de registro |
| `/Account/Signin2` | POST | Autentica via Firebase, seta session token |
| `/Account/NewRegister` | POST [SessionFilter] | Cria usuário no Firebase + banco |
| `/Account/ForgotPassword` | GET | Recuperação de senha |
| `/Account/Logout` | GET [SessionFilter] | Limpa session, redireciona para Login |

### PacienteController
| Rota | Método | Descrição |
|---|---|---|
| `/Paciente` | GET | Index (vazio) |
| ~~PacientesAnalisados~~ | — | **Comentado — não implementado** |
| ~~DeletarPaciente~~ | — | **Comentado — não implementado** |
| ~~CadastrarPaciente~~ | — | **Comentado — não implementado** |

### CIDController
| Rota | Método | Descrição |
|---|---|---|
| `/CID/ListarBlocos` | GET | Lista Blocos_CID (JSON) |
| `/CID/ListarCategorias` | GET | Lista Categorias_CID (JSON) |
| `/CID/ListarCapitulos` | GET | Lista Capitulos_CID (JSON) |

### MedicamentosController
| Rota | Método | Descrição |
|---|---|---|
| `/Medicamentos/ListarMedicamentos` | GET | Lista Medicamentos ativos e únicos (JSON) |

---

## Serviços — Responsabilidades

### `FormularioServices`
- `listarCategorias()` → todas as categorias com seus Tipos (Include)
- `listarTipoByCategoriaId(id)` → tipos filtrados por categoria
- `listarFrequencias()` → todas as frequências
- `listarInstrucoesAdicionais()` → todas as instruções
- `converterMedicamentos(List<MedicamentosExportacaoModel>)` → monta `RemedioViewModel` com entidades e pesos para cálculo

### `PacienteServices`
- `listarPacientesICT(email)` → pacientes do usuário com suas receitas (Include), desc por DataCriacao
- ⚠️ `salvarPaciente()`, `deletarPaciente()`, `buscarPacienteICT()`, `atualizarPacienteICT()` — **não implementados**

### `CIDServices`
- `listarBlocosCID()` → todos os blocos ordenados por BlockId
- `listarCapitulosCID()` → todos os capítulos ordenados por ChapterNo
- `listarCategoriasCID()` → todas as categorias ordenadas por Code
- `agruparEnfermidadesPorBloco()` → agrupa Categorias_CID por Bloco (retorna `GrupoEnfermidadesViewModel`)

### `UsuarioService`
- `GetUsuarioByEmail(email)` → **BUG CRÍTICO** — Include com predicado errado, sempre retorna null
- `CadastrarUsuario(UsuarioViewModel)` → salva novo usuário no banco

### `MedicamentosServices`
- `listarMedicamentos()` → todos os medicamentos
- `ListarMedicamentosUnique()` → filtra ativos/válidos, remove duplicatas por nome (ToUpper), ordena por nome

---

## ViewModels e DTOs

### `FormularioViewModel`
Agrega tudo para renderizar o formulário:
```csharp
List<PacienteICT> Pacientes
List<Categorium> Categorias
List<Frequencium> Frequencias
List<InstrucoesAdicionai> Instrucoes
List<GrupoEnfermidadesViewModel> Enfermidades
List<Medicamento> Medicamentos
Alerta? Alerta
```

### `RemedioViewModel`
Representa um medicamento na receita com pesos calculados:
```csharp
Categorium categoria
Tipo subcategoria
Frequencium frequencia
List<InstrucoesAdicionai> instrucoes
double PesoTotal { get => frequencia.Peso + instrucoes.Sum(i => i.Peso) }
// ⚠️ Peso do Tipo (subcategoria) NÃO está sendo somado aqui — possível bug
```

### `ReceitaViewModel`
```csharp
string NomePaciente
List<RemedioViewModel> RemedioList
Alerta? Alerta
double PesoTotal { get => RemedioList.Sum(r => r.PesoTotal) }
```

### `GrupoEnfermidadesViewModel`
```csharp
Blocos_CID Bloco
List<Categorias_CID> Enfermidades
```

### `ExibicaoICTViewModel`
```csharp
List<PacienteICT>? PacienteICT
int? Pagina
string? Ordenacao
bool deleteClicked
Alerta? Alerta
int id
```

### `ExportacaoModel` (payload POST /ExportarDados)
```csharp
string nome
List<MedicamentosExportacaoModel>? medicamentos
```

### `MedicamentosExportacaoModel`
```csharp
int categoria
int subcategoria
int frequencia
List<int>? instrucoesAdicionais
```

### `AdministradorDTO`
```csharp
[Required][EmailAddress] string Email
[Required] string Senha
string? token
```

---

## Bugs e Problemas Conhecidos

### CRÍTICOS

**1. Bug em `UsuarioService.GetUsuarioByEmail()`**
- Arquivo: `Services/UsuariosService/Implementacao/UsuarioService.cs`
- `.Include(r => r.Usuario1 == email)` é código inválido — Include não aceita predicado
- Sempre retorna null. O usuário nunca é encontrado após login.
- Fix: `return _appDbContextNew.Usuarios.FirstOrDefault(u => u.Usuario1 == email);`

**2. `ExportarDados` não persiste no banco**
- `HomeController.ExportarDados()` calcula o ICT mas retorna apenas `Ok(pesoTotal)`
- Não salva `PacienteICT` nem `Receita` no banco
- Todo o fluxo de persistência está por implementar

**3. `PacienteController` sem implementação**
- Todos os endpoints de gestão de pacientes estão comentados
- A tela `PacientesAnalisados` existe mas não funciona

### MODERADOS

**4. `RemedioViewModel.PesoTotal` potencialmente incorreto**
- `PesoTotal = frequencia.Peso + instrucoes.Sum(i => i.Peso)`
- O peso do `Tipo` (subcategoria) **não está sendo somado** — verifique se é intencional

**5. Autenticação híbrida Session + JWT**
- `SessionFilter` valida `HttpContext.Session.GetString("_UserToken")`
- JWT Bearer também está configurado em `Program.cs`
- Os dois sistemas coexistem sem integração clara — potencial confusão

**6. CORS completamente aberto**
- `AllowAnyOrigin()`, `AllowAnyMethod()`, `AllowAnyHeader()` em Program.cs
- Aceitável apenas em desenvolvimento — risco em produção

**7. Null reference potencial em `ListarMedicamentosUnique()`**
- Agrupamento por `x.NOME_PRODUTO.ToUpper()` pode lançar NullReferenceException se `NOME_PRODUTO` for null

### MENORES

**8. Referências obsoletas a campos antigos**
- Código comentado em `PacienteController` refere `x.dataCriacao` (minúsculo) em vez de `DataCriacao`
- Indica que campos foram renomeados durante a refatoração do banco

---

## Fluxo de Trabalho do Agente

Quando receber uma tarefa, siga este fluxo:

1. **Entender o problema** — descreva o que foi pedido com suas próprias palavras e verifique se o entendimento está correto
2. **Identificar impactos** — quais entidades, camadas, views e endpoints são afetados
3. **Identificar riscos** — o que pode quebrar, regressões possíveis, dependências ocultas
4. **Sugerir melhorias** — se houver uma forma melhor de resolver, aponte antes de implementar
5. **Implementar** — com contexto completo, sem mudanças isoladas
6. **Apontar pontos esquecidos** — o que ficou de fora ou deve ser feito depois
7. **Sugerir testes** — o que deve ser testado manualmente ou por testes automatizados

---

## Comportamento Esperado

### Faça sempre:
- Questione decisões quando a implementação for arriscada ou existir alternativa melhor
- Aponte impactos em outras camadas antes de implementar
- Identifique código duplicado e oportunidades de simplificação
- Alerte quando uma mudança puder quebrar algo já funcionando
- Informe acoplamentos desnecessários entre camadas
- Implemente com contexto — nada de mudanças pontuais sem entender o todo

### Evite:
- Mudanças superficiais sem análise de contexto
- Implementar sem entender a direção da evolução
- Criar abstrações desnecessárias para tarefas simples
- Comentários que descrevem o que o código faz (prefira código legível)
- Deixar código comentado para trás sem justificativa

### Ao receber pedido de refatoração:
Pergunte-se:
- Qual entidade mudou no banco?
- Qual foi a intenção dessa mudança?
- Quais camadas são impactadas (Service, Controller, ViewModel, View)?
- Quais dependências precisam de ajuste?
- Quais efeitos colaterais podem surgir?

---

## Estado Atual do Sistema

| Funcionalidade | Estado |
|---|---|
| Login Firebase | ✅ Funcional |
| Carregamento do formulário | ✅ Funcional |
| Cálculo do ICT (frontend) | ✅ Funcional |
| Salvar paciente no banco | ❌ Não implementado |
| Salvar receita no banco | ❌ Não implementado |
| Listar pacientes | ❌ Não implementado (comentado) |
| Detalhar paciente/receita | ❌ Não implementado |
| Deletar paciente | ❌ Não implementado |
| Listagem de CID (JSON) | ✅ Funcional |
| Listagem de medicamentos (JSON) | ✅ Funcional |
| GetUsuarioByEmail | ❌ Bug crítico |

**Próximas prioridades naturais do sistema:**
1. Corrigir `GetUsuarioByEmail`
2. Implementar persistência: `salvarPaciente` + `salvarReceita`
3. Reativar `PacienteController` (listar, detalhar, deletar)
4. Revisar cálculo do ICT (`RemedioViewModel.PesoTotal` inclui peso do Tipo?)
5. Implementar paginação na listagem de pacientes
6. Tela de histórico de receitas por paciente
