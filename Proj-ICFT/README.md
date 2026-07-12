# Proj-ICFT (Simply Pharm) — Índice de Complexidade Farmacoterapêutica

Sistema web para cálculo do **Índice de Complexidade Terapêutica (ICT)** de pacientes a partir dos medicamentos que utilizam. Voltado a profissionais de saúde (médicos, farmacêuticos), permite montar prescrições, calcular o escore de complexidade, associar diagnósticos **CID-11**, registrar a **evolução clínica** do paciente e acompanhar indicadores por meio de **relatórios**.

Projeto desenvolvido como Trabalho de Conclusão de Curso (TCC).

---

## Sumário

- [Visão geral](#visão-geral)
- [Como o ICT é calculado](#como-o-ict-é-calculado)
- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Modelo de dados](#modelo-de-dados)
- [Funcionalidades](#funcionalidades)
- [Feature flags](#feature-flags)
- [Como executar](#como-executar)
- [Migrations](#migrations)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Documentação](#documentação)

---

## Visão geral

O fluxo central do sistema é:

1. O profissional autentica-se via **Firebase Authentication**.
2. Preenche um **formulário de prescrição**, escolhendo medicamentos com sua categoria, tipo (subcategoria), frequência e instruções adicionais.
3. Cada componente possui um **peso** cadastrado no banco.
4. O sistema calcula o **escore ICT** somando esses pesos.
5. A prescrição é associada a um paciente e persistida junto do escore calculado.
6. Diagnósticos **CID-11** (Capítulo → Bloco → Categoria) podem ser vinculados à prescrição.
7. A **evolução clínica** do paciente é registrada no formato SOAP, com sinais vitais, laboratoriais e indicadores de impacto, permitindo acompanhamento temporal.
8. **Relatórios** consolidam os dados por período para análise.

---

## Como o ICT é calculado

```
ICT = Σ (por medicamento na prescrição):
        Peso do Tipo (subcategoria)
      + Peso da Frequência
      + Σ Peso de cada Instrução Adicional
```

O escore total é armazenado no campo `ICT` da prescrição.

---

## Stack

| Camada       | Tecnologia                                   |
| ------------ | -------------------------------------------- |
| Framework    | ASP.NET Core 8.0 MVC                         |
| ORM          | Entity Framework Core 9.0                    |
| Banco        | SQL Server                                   |
| Autenticação | Firebase Authentication + JWT Bearer         |
| Frontend     | Razor Views + Bootstrap + JavaScript vanilla |
| Paginação    | X.PagedList                                  |
| DI           | Container nativo do .NET                     |

**Pacotes principais** (`Proj-ICFT.csproj`):
`Firebase.Auth`, `FirebaseAdmin`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.EntityFrameworkCore.SqlServer`, `X.PagedList.Mvc.Core`.

---

## Arquitetura

```
Views (Razor) ←→ Controllers ←→ Services (Interface + Implementação) ←→ AppDbContextNew (EF Core) ←→ SQL Server
```

**Padrões adotados:**

- Services com interface + implementação, registrados por injeção de dependência (`Program.cs`).
- ViewModels para transferência de dados entre Controller e View.
- `SessionFilter` (action filter) protegendo rotas autenticadas via sessão.
- `AppDbContextNew` (em `DataNew/`) é o **único DbContext ativo**.
- Autenticação híbrida: sessão (`SessionFilter`) + JWT Bearer validado contra o Firebase.

---

## Modelo de dados

Entidades principais (fonte de verdade em `ModelsNew/`):

- **Usuario** — profissional autenticado (e-mail do Firebase).
- **PacienteICT** — paciente avaliado (nome, sexo, idade), vinculado ao usuário.
- **Receita/Prescrição** — prescrição com o escore `ICT` calculado.
- **ReceitaMed** — linha de prescrição: medicamento + categoria + tipo + frequência.
- **InstrucoesMed** — pivot entre linha de prescrição e instruções adicionais.
- **Medicamento** — catálogo de medicamentos (filtrados por registro ativo/válido).
- **Categoria / Tipo / Frequencia / InstrucoesAdicionais** — componentes de peso do ICT.
- **EvolucaoClinica** — registro SOAP + sinais vitais, laboratoriais e indicadores de impacto.
- **CID-11** — `Capitulos_CID` → `Blocos_CID` → `Categorias_CID`, associados à prescrição via `ReceitaCID`.

---

## Funcionalidades

| Módulo                   | Descrição                                                                 |
| ------------------------ | ------------------------------------------------------------------------- |
| Autenticação             | Login, registro e logout via Firebase (`AccountController`)               |
| Formulário de prescrição | Seleção de medicamentos e cálculo do ICT (`HomeController`)               |
| Catálogo de medicamentos | Listagem de medicamentos ativos e únicos (`MedicamentosController`)       |
| Classificação CID-11     | Listagem de capítulos, blocos e categorias (`CIDController`)              |
| Pacientes                | Gestão de pacientes ICT (`PacienteController`)                            |
| Evolução clínica         | Registro SOAP, série temporal e indicadores (`EvolucaoClinicaController`) |
| Relatórios               | Consolidação de dados por período (`RelatorioController`)                 |

### Principais endpoints

- `GET /Form` — formulário principal de prescrição.
- `POST /ExportarDados` — recebe a prescrição e calcula o ICT.
- `GET /Medicamentos/ListarMedicamentos` — catálogo de medicamentos (JSON).
- `GET /CID/ListarCapitulos` · `/ListarBlocos` · `/ListarCategorias` — dados do CID-11 (JSON).
- `GET /EvolucaoClinica/ListarPorPaciente` · `/Salvar` · `/SerieTemporal` — evolução clínica.
- `GET /Relatorio/Dados?de=&ate=` — dados consolidados para relatórios (JSON).

A lista completa de rotas e serviços está em [`CLAUDE.md`](CLAUDE.md).

---

## Feature flags

Configuráveis na seção `Features` do `appsettings.json` (`Models/FeatureFlags.cs`):

| Flag              | Efeito                                                 |
| ----------------- | ------------------------------------------------------ |
| `HomeComoLogin`   | Usa a Home como tela de login                          |
| `UsarWordmark`    | Exibe a logomarca em texto (wordmark) no lugar do logo |
| `CadastroPublico` | Habilita o cadastro público de novos usuários          |

---

## Como executar

### Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- SQL Server (local ou remoto)
- Projeto do Firebase configurado (Authentication)

### Passos

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Configurar a connection string em appsettings.json (ConnectionStrings:DefaultConnection)
#    e as credenciais do Firebase

# 3. Aplicar as migrations ao banco
dotnet ef database update

# 4. Executar a aplicação
dotnet run
```

Por padrão a aplicação inicia na rota `Home/Home`.

> **Configuração sensível** (connection string, credenciais Firebase) deve ficar em _user secrets_ ou variáveis de ambiente, e não versionada. O projeto já possui `UserSecretsId` configurado no `.csproj`.

---

## Migrations

O schema é gerenciado por **EF Core Migrations** (a partir de 2026-05-30):

- `InitialBaseline` — captura o estado herdado do scaffolding.
- `AddEvolucaoClinica` — adiciona a entidade de evolução clínica.
- `RenomearReceitaParaPrescricao` — renomeia Receita → Prescrição.

Após cada `git pull`, rode:

```bash
dotnet ef database update
```

---

## Estrutura de pastas

```
Controllers/          Controllers MVC (Home, Account, Paciente, CID, Medicamentos, EvolucaoClinica, Relatorio)
ModelsNew/            Entidades EF Core (fonte de verdade do banco)
DataNew/              AppDbContextNew (DbContext ativo)
Services/             Lógica de negócio (interface + implementação por domínio)
Views/                Razor Views por controller
Migrations/           Migrations do EF Core
docs/                 Diagramas C4, casos de uso, classes e implantação
```

---
