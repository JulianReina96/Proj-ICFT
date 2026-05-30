# Evolução Clínica — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar o registro e acompanhamento longitudinal de evolução clínica (RF16) no Proj-ICFT, permitindo correlação entre complexidade terapêutica (ICT), adesão e desfechos clínicos por paciente.

**Architecture:** Nova entidade `EvolucaoClinica` independente com FK obrigatória a `PacienteICT` e FKs opcionais a `Receitum` e `Categorias_CID`. Backend C# ASP.NET Core MVC seguindo padrão Service+Controller já adotado no projeto. Frontend Razor + Bootstrap nav-tabs em `VerPaciente.cshtml`, modal SOAP, Chart.js para visualização longitudinal. Adoção de EF Core Migrations a partir de uma `InitialBaseline` vazia.

**Tech Stack:** .NET 8.0, EF Core 9.0, SQL Server (local Windows Auth), Razor Views, Bootstrap 5, Chart.js 4.x via CDN, SweetAlert2 (já presente), Select2 (já presente).

**Spec de referência:** [docs/superpowers/specs/2026-05-30-evolucao-clinica-design.md](../specs/2026-05-30-evolucao-clinica-design.md)

**Pré-requisitos:**
- SQL Server local rodando com banco `ICT_TCC2` configurado em `appsettings.json`.
- .NET 8 SDK + ferramenta `dotnet-ef` global (`dotnet tool install --global dotnet-ef --version 9.0.0`).
- Visual Studio ou PowerShell na pasta raiz do projeto.

**Convenção de verificação:** Como o projeto não possui suite de testes automatizados, cada task termina com `dotnet build` (sanity check de compilação) e, quando aplicável, verificação manual descrita explicitamente.

---

## Fase 1 — Adoção de EF Core Migrations

### Task 1: Bootstrap baseline migration vazia

**Files:**
- Create: `Migrations/<timestamp>_InitialBaseline.cs`
- Create: `Migrations/<timestamp>_InitialBaseline.Designer.cs`
- Create: `Migrations/AppDbContextNewModelSnapshot.cs`

- [ ] **Step 1: Gerar a baseline**

```powershell
dotnet ef migrations add InitialBaseline
```

Expected: cria a pasta `Migrations/` com 3 arquivos. O `Up()` da baseline vem cheio de `CreateTable(...)` — isso será esvaziado no Step 2.

- [ ] **Step 2: Esvaziar Up() e Down() da baseline**

Abrir `Migrations/<timestamp>_InitialBaseline.cs` e substituir o corpo da classe pelos métodos vazios:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proj_ICFT.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intencionalmente vazio — captura o estado atual do banco como baseline.
            // O schema real já existe no SQL Server (criado via scaffolding DB-first).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não aplicável: baseline não é revertível.
        }
    }
}
```

NÃO altere o `Designer.cs` nem o `AppDbContextNewModelSnapshot.cs` — eles capturam o estado real do schema e são usados para fazer o diff das próximas migrations.

- [ ] **Step 3: Aplicar a baseline no banco**

```powershell
dotnet ef database update
```

Expected: cria a tabela `__EFMigrationsHistory` (se não existir) e insere a linha da baseline. Nenhuma outra alteração no schema.

- [ ] **Step 4: Verificar via SSMS (ou query)**

Rodar no SQL Server:

```sql
SELECT * FROM ICT_TCC2.dbo.__EFMigrationsHistory;
```

Expected: 1 linha com `MigrationId = '<timestamp>_InitialBaseline'`.

- [ ] **Step 5: Build sanity check**

```powershell
dotnet build
```

Expected: `Build succeeded. 0 Errors`.

- [ ] **Step 6: Commit**

```powershell
git add Migrations/
git commit -m "chore(ef): bootstrap InitialBaseline migration vazia para adotar EF migrations"
```

---

## Fase 2 — Domain Model

### Task 2: Criar enum `StatusEvolucao`

**Files:**
- Create: `ModelsNew/StatusEvolucao.cs`

- [ ] **Step 1: Criar o arquivo**

```csharp
namespace Proj_ICFT.ModelsNew;

public enum StatusEvolucao
{
    Inconclusivo = 0,
    Melhorou     = 1,
    Estavel      = 2,
    Piorou       = 3
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded. 0 Errors`.

- [ ] **Step 3: Commit**

```powershell
git add ModelsNew/StatusEvolucao.cs
git commit -m "feat(model): adicionar enum StatusEvolucao (4 estados)"
```

---

### Task 3: Criar entidade `EvolucaoClinica`

**Files:**
- Create: `ModelsNew/EvolucaoClinica.cs`

- [ ] **Step 1: Criar a entidade completa**

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.ModelsNew;

[Table("EvolucaoClinica")]
public partial class EvolucaoClinica
{
    [Key]
    public int Id { get; set; }

    // Relações estruturais
    public int PacienteID { get; set; }
    public int UsuarioCriacaoID { get; set; }
    public int? ReceitaID { get; set; }
    public int? CategoriaCID_ID { get; set; }

    // Temporal
    [Column(TypeName = "datetime")]
    public DateTime DataConsulta { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DataCriacao { get; set; }

    // SOAP — texto livre
    [StringLength(2000)] public string? Subjetivo { get; set; }
    [StringLength(2000)] public string? Avaliacao { get; set; }
    [StringLength(2000)] public string? Plano { get; set; }
    [StringLength(2000)] public string? ObservacaoObjetivo { get; set; }

    // Desfecho categórico
    public StatusEvolucao Status { get; set; }

    // OBJETIVO — Sinais vitais (opcionais)
    public int?    PaSistolica       { get; set; }
    public int?    PaDiastolica      { get; set; }
    public int?    FrequenciaCardiaca{ get; set; }
    public double? Peso              { get; set; }
    public int?    Spo2              { get; set; }

    // OBJETIVO — Laboratoriais (opcionais)
    public double? Glicemia   { get; set; }
    public double? Hba1c      { get; set; }
    public double? Creatinina { get; set; }

    // OBJETIVO — Contadores de impacto (default 0)
    public int EventosAdversos { get; set; }
    public int Hospitalizacoes { get; set; }
    public int IdasEmergencia  { get; set; }

    // Navegação
    [ForeignKey("PacienteID")]
    public virtual PacienteICT Paciente { get; set; } = null!;

    [ForeignKey("UsuarioCriacaoID")]
    public virtual Usuario UsuarioCriacao { get; set; } = null!;

    [ForeignKey("ReceitaID")]
    public virtual Receitum? Receita { get; set; }

    [ForeignKey("CategoriaCID_ID")]
    public virtual Categorias_CID? CategoriaCID { get; set; }
}
```

- [ ] **Step 2: Build (vai falhar — entidades existentes ainda não têm as navegações inversas)**

```powershell
dotnet build
```

Expected: pode buildar sem erro porque as navegações inversas serão adicionadas na Task 4. Mas se houver erro de referência cruzada, é esperado e será resolvido após Task 4.

- [ ] **Step 3: Commit**

```powershell
git add ModelsNew/EvolucaoClinica.cs
git commit -m "feat(model): adicionar entidade EvolucaoClinica"
```

---

### Task 4: Adicionar navegações inversas nas entidades existentes

**Files:**
- Modify: `ModelsNew/PacienteICT.cs`
- Modify: `ModelsNew/Receitum.cs`
- Modify: `ModelsNew/Categorias_CID.cs`
- Modify: `ModelsNew/Usuario.cs`

- [ ] **Step 1: Adicionar coleção em `PacienteICT.cs`**

No arquivo `ModelsNew/PacienteICT.cs`, dentro da classe `PacienteICT`, **logo após** a propriedade `Receita` (linha ~30), adicionar:

```csharp
    [InverseProperty("Paciente")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();
```

- [ ] **Step 2: Adicionar coleção em `Receitum.cs`**

No arquivo `ModelsNew/Receitum.cs`, dentro da classe `Receitum`, **logo após** a coleção `ReceitaMeds`, adicionar:

```csharp
    [InverseProperty("Receita")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();
```

- [ ] **Step 3: Adicionar coleção em `Categorias_CID.cs`**

No arquivo `ModelsNew/Categorias_CID.cs`, dentro da classe `Categorias_CID`, **no final da classe** (antes da última `}`), adicionar:

```csharp
    [InverseProperty("CategoriaCID")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();
```

- [ ] **Step 4: Adicionar coleção em `Usuario.cs`**

No arquivo `ModelsNew/Usuario.cs`, dentro da classe `Usuario`, **no final da classe** (antes da última `}`), adicionar:

```csharp
    [InverseProperty("UsuarioCriacao")]
    public virtual ICollection<EvolucaoClinica> EvolucaoClinicas { get; set; } = new List<EvolucaoClinica>();
```

- [ ] **Step 5: Build**

```powershell
dotnet build
```

Expected: `Build succeeded. 0 Errors`.

- [ ] **Step 6: Commit**

```powershell
git add ModelsNew/PacienteICT.cs ModelsNew/Receitum.cs ModelsNew/Categorias_CID.cs ModelsNew/Usuario.cs
git commit -m "feat(model): navegações inversas para EvolucaoClinica em PacienteICT, Receitum, Categorias_CID e Usuario"
```

---

### Task 5: Atualizar `AppDbContextNew` com DbSet e OnModelCreating

**Files:**
- Modify: `DataNew/AppDbContextNew.cs`

- [ ] **Step 1: Adicionar `DbSet<EvolucaoClinica>`**

No arquivo `DataNew/AppDbContextNew.cs`, **logo após** `public virtual DbSet<Usuario> Usuarios { get; set; }`, adicionar:

```csharp
    public virtual DbSet<EvolucaoClinica> EvolucaoClinicas { get; set; }
```

- [ ] **Step 2: Adicionar configuração no `OnModelCreating`**

No mesmo arquivo, dentro do método `OnModelCreating`, **logo antes** do `OnModelCreatingPartial(modelBuilder)` (se existir) ou no final dos demais `modelBuilder.Entity<...>`, adicionar:

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

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded. 0 Errors`.

- [ ] **Step 4: Commit**

```powershell
git add DataNew/AppDbContextNew.cs
git commit -m "feat(db): registrar EvolucaoClinica no AppDbContextNew com FKs e índices"
```

---

### Task 6: Gerar e aplicar migration `AddEvolucaoClinica`

**Files:**
- Create: `Migrations/<timestamp>_AddEvolucaoClinica.cs`
- Modify: `Migrations/AppDbContextNewModelSnapshot.cs` (auto)

- [ ] **Step 1: Gerar a migration**

```powershell
dotnet ef migrations add AddEvolucaoClinica
```

Expected: cria `<timestamp>_AddEvolucaoClinica.cs` + atualiza o snapshot. O `Up()` deve conter `CreateTable("EvolucaoClinica", ...)` apenas (não recriar tabelas existentes).

- [ ] **Step 2: Inspecionar o SQL gerado antes de aplicar**

```powershell
dotnet ef migrations script InitialBaseline AddEvolucaoClinica
```

Expected: SQL contendo `CREATE TABLE [EvolucaoClinica]` com 21 colunas + 4 FKs + 2 índices. Confirmar:
- `ON DELETE CASCADE` na FK `PacienteICT`
- `ON DELETE SET NULL` nas FKs `Receita` e `Categorias_CID`
- `ON DELETE NO ACTION` na FK `Usuario`

Se aparecer erro de "multiple cascade paths" ao tentar aplicar (Step 3 abaixo), voltar à Task 5 e trocar `OnDelete(Cascade)` por `OnDelete(Restrict)` na relação `PacienteICT`. Refazer `dotnet ef migrations remove` + `dotnet ef migrations add AddEvolucaoClinica`.

- [ ] **Step 3: Aplicar no banco**

```powershell
dotnet ef database update
```

Expected: tabela `EvolucaoClinica` criada com sucesso.

- [ ] **Step 4: Verificar no SQL Server**

```sql
SELECT TOP 5 * FROM ICT_TCC2.dbo.EvolucaoClinica;
SELECT * FROM ICT_TCC2.dbo.__EFMigrationsHistory;
```

Expected: tabela existe (vazia). Linha `AddEvolucaoClinica` em `__EFMigrationsHistory`.

- [ ] **Step 5: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 6: Commit**

```powershell
git add Migrations/
git commit -m "feat(db): adicionar tabela EvolucaoClinica via migration AddEvolucaoClinica"
```

---

## Fase 3 — DTOs e ViewModels

### Task 7: Criar DTO `SalvarEvolucaoRequest`

**Files:**
- Create: `Models/Request/EvolucaoRequest.cs`

- [ ] **Step 1: Criar o arquivo**

```csharp
using System;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models.Request;

public record SalvarEvolucaoRequest(
    int PacienteID,
    int? ReceitaID,
    int? CategoriaCID_ID,
    DateTime DataConsulta,
    StatusEvolucao Status,

    string? Subjetivo,
    string? Avaliacao,
    string? Plano,
    string? ObservacaoObjetivo,

    int? PaSistolica,
    int? PaDiastolica,
    int? FrequenciaCardiaca,
    double? Peso,
    int? Spo2,

    double? Glicemia,
    double? Hba1c,
    double? Creatinina,

    int EventosAdversos,
    int Hospitalizacoes,
    int IdasEmergencia
);
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Models/Request/EvolucaoRequest.cs
git commit -m "feat(dto): adicionar SalvarEvolucaoRequest"
```

---

### Task 8: Criar ViewModels da Evolução

**Files:**
- Create: `Models/ViewModels/EvolucaoViewModel.cs`

- [ ] **Step 1: Criar o arquivo com todos os ViewModels**

```csharp
using System;
using System.Collections.Generic;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models.ViewModels;

public class EvolucaoListagemViewModel
{
    public int Id { get; set; }
    public DateTime DataConsulta { get; set; }
    public StatusEvolucao Status { get; set; }
    public string StatusLabel { get; set; } = "";
    public int? ReceitaID { get; set; }
    public double? IctDaReceita { get; set; }
    public bool? ReceitaAdesao { get; set; }
    public string? CidCodigo { get; set; }
    public string? CidTitulo { get; set; }
}

public class EvolucaoDetalheViewModel : EvolucaoListagemViewModel
{
    public string? Subjetivo { get; set; }
    public string? Avaliacao { get; set; }
    public string? Plano { get; set; }
    public string? ObservacaoObjetivo { get; set; }
    public int? CategoriaCID_ID { get; set; }

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
}

public class CIDOpcaoViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
}

public class ReceitaOpcaoViewModel
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public double Ict { get; set; }
}

public class SerieTemporalViewModel
{
    public List<DateTime> Datas { get; set; } = new();
    public List<double?> IctPorEvolucao { get; set; } = new();
    public List<int> StatusPorEvolucao { get; set; } = new();

    public List<int?> PaSistolica { get; set; } = new();
    public List<int?> PaDiastolica { get; set; } = new();
    public List<int?> FrequenciaCardiaca { get; set; } = new();
    public List<double?> Peso { get; set; } = new();
    public List<int?> Spo2 { get; set; } = new();

    public List<double?> Glicemia { get; set; } = new();
    public List<double?> Hba1c { get; set; } = new();
    public List<double?> Creatinina { get; set; } = new();

    public List<int> EventosAdversos { get; set; } = new();
    public List<int> Hospitalizacoes { get; set; } = new();
    public List<int> IdasEmergencia { get; set; } = new();
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Models/ViewModels/EvolucaoViewModel.cs
git commit -m "feat(vm): adicionar ViewModels para Evolução Clínica (Listagem, Detalhe, CIDOpcao, SerieTemporal)"
```

---

## Fase 4 — Service Layer

### Task 9: Criar interface `IEvolucaoClinicaServices`

**Files:**
- Create: `Services/EvolucaoClinicaServices/Interface/IEvolucaoClinicaServices.cs`

- [ ] **Step 1: Criar o arquivo**

```csharp
using Proj_ICFT.Models.Request;
using Proj_ICFT.Models.ViewModels;

namespace Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

public interface IEvolucaoClinicaServices
{
    Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string emailUsuario);
    Task<EvolucaoDetalheViewModel?> Detalhar(int id, string emailUsuario);
    Task<int> Salvar(SalvarEvolucaoRequest request, string emailUsuario);
    Task Atualizar(int id, SalvarEvolucaoRequest request, string emailUsuario);
    Task Deletar(int id, string emailUsuario);
    Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string emailUsuario);
    Task<List<ReceitaOpcaoViewModel>> ListarReceitasDoPaciente(int pacienteId, string emailUsuario);
    Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string emailUsuario);
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Services/EvolucaoClinicaServices/Interface/IEvolucaoClinicaServices.cs
git commit -m "feat(service): adicionar interface IEvolucaoClinicaServices"
```

---

### Task 10: Implementar `EvolucaoClinicaServices` — esqueleto + Salvar + Atualizar + Deletar

**Files:**
- Create: `Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs`

- [ ] **Step 1: Criar o arquivo com guard e mutações**

```csharp
using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models.Request;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

namespace Proj_ICFT.Services.EvolucaoClinicaServices.Implementacao;

public class EvolucaoClinicaServices : IEvolucaoClinicaServices
{
    private readonly AppDbContextNew _db;

    public EvolucaoClinicaServices(AppDbContextNew db)
    {
        _db = db;
    }

    private async Task<(Usuario usuario, PacienteICT paciente)> ResolverContexto(int pacienteId, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");
        var paciente = await _db.PacienteICTs
            .FirstOrDefaultAsync(p => p.ID == pacienteId && p.UsuarioCriacaoID == usuario.id)
            ?? throw new InvalidOperationException("Paciente não encontrado ou sem permissão.");
        return (usuario, paciente);
    }

    private static string StatusLabel(StatusEvolucao s) => s switch
    {
        StatusEvolucao.Melhorou     => "Melhorou",
        StatusEvolucao.Estavel      => "Estável",
        StatusEvolucao.Piorou       => "Piorou",
        _                            => "Inconclusivo"
    };

    public async Task<int> Salvar(SalvarEvolucaoRequest r, string email)
    {
        var (usuario, paciente) = await ResolverContexto(r.PacienteID, email);

        if (r.ReceitaID is int rid)
        {
            var ok = await _db.Receita.AnyAsync(x => x.Id == rid && x.PacienteID == paciente.ID);
            if (!ok) throw new InvalidOperationException("Receita inválida para este paciente.");
        }

        if (r.CategoriaCID_ID is int cid && !await _db.Categorias_CIDs.AnyAsync(c => c.Id == cid))
            throw new InvalidOperationException("CID inválido.");

        if (r.DataConsulta > DateTime.Now.AddDays(1))
            throw new InvalidOperationException("Data da consulta não pode ser futura.");

        var ev = new EvolucaoClinica
        {
            PacienteID         = paciente.ID,
            UsuarioCriacaoID   = usuario.id,
            ReceitaID          = r.ReceitaID,
            CategoriaCID_ID    = r.CategoriaCID_ID,
            DataConsulta       = r.DataConsulta,
            DataCriacao        = DateTime.Now,
            Status             = r.Status,
            Subjetivo          = r.Subjetivo,
            Avaliacao          = r.Avaliacao,
            Plano              = r.Plano,
            ObservacaoObjetivo = r.ObservacaoObjetivo,
            PaSistolica        = r.PaSistolica,
            PaDiastolica       = r.PaDiastolica,
            FrequenciaCardiaca = r.FrequenciaCardiaca,
            Peso               = r.Peso,
            Spo2               = r.Spo2,
            Glicemia           = r.Glicemia,
            Hba1c              = r.Hba1c,
            Creatinina         = r.Creatinina,
            EventosAdversos    = r.EventosAdversos,
            Hospitalizacoes    = r.Hospitalizacoes,
            IdasEmergencia     = r.IdasEmergencia
        };
        _db.EvolucaoClinicas.Add(ev);
        await _db.SaveChangesAsync();
        return ev.Id;
    }

    public async Task Atualizar(int id, SalvarEvolucaoRequest r, string email)
    {
        var (_, paciente) = await ResolverContexto(r.PacienteID, email);

        var ev = await _db.EvolucaoClinicas
            .FirstOrDefaultAsync(e => e.Id == id && e.PacienteID == paciente.ID)
            ?? throw new InvalidOperationException("Evolução não encontrada ou sem permissão.");

        if (r.ReceitaID is int rid)
        {
            var ok = await _db.Receita.AnyAsync(x => x.Id == rid && x.PacienteID == paciente.ID);
            if (!ok) throw new InvalidOperationException("Receita inválida para este paciente.");
        }
        if (r.CategoriaCID_ID is int cid && !await _db.Categorias_CIDs.AnyAsync(c => c.Id == cid))
            throw new InvalidOperationException("CID inválido.");
        if (r.DataConsulta > DateTime.Now.AddDays(1))
            throw new InvalidOperationException("Data da consulta não pode ser futura.");

        ev.ReceitaID          = r.ReceitaID;
        ev.CategoriaCID_ID    = r.CategoriaCID_ID;
        ev.DataConsulta       = r.DataConsulta;
        ev.Status             = r.Status;
        ev.Subjetivo          = r.Subjetivo;
        ev.Avaliacao          = r.Avaliacao;
        ev.Plano              = r.Plano;
        ev.ObservacaoObjetivo = r.ObservacaoObjetivo;
        ev.PaSistolica        = r.PaSistolica;
        ev.PaDiastolica       = r.PaDiastolica;
        ev.FrequenciaCardiaca = r.FrequenciaCardiaca;
        ev.Peso               = r.Peso;
        ev.Spo2               = r.Spo2;
        ev.Glicemia           = r.Glicemia;
        ev.Hba1c              = r.Hba1c;
        ev.Creatinina         = r.Creatinina;
        ev.EventosAdversos    = r.EventosAdversos;
        ev.Hospitalizacoes    = r.Hospitalizacoes;
        ev.IdasEmergencia     = r.IdasEmergencia;

        await _db.SaveChangesAsync();
    }

    public async Task Deletar(int id, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var ev = await _db.EvolucaoClinicas
            .Include(e => e.Paciente)
            .FirstOrDefaultAsync(e => e.Id == id && e.Paciente.UsuarioCriacaoID == usuario.id)
            ?? throw new InvalidOperationException("Evolução não encontrada ou sem permissão.");

        _db.EvolucaoClinicas.Remove(ev);
        await _db.SaveChangesAsync();
    }

    // Os 4 métodos abaixo serão implementados nas Tasks 11 e 12.
    public Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string emailUsuario)
        => throw new NotImplementedException();

    public Task<EvolucaoDetalheViewModel?> Detalhar(int id, string emailUsuario)
        => throw new NotImplementedException();

    public Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string emailUsuario)
        => throw new NotImplementedException();

    public Task<List<ReceitaOpcaoViewModel>> ListarReceitasDoPaciente(int pacienteId, string emailUsuario)
        => throw new NotImplementedException();

    public Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string emailUsuario)
        => throw new NotImplementedException();
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs
git commit -m "feat(service): EvolucaoClinicaServices com Salvar, Atualizar, Deletar e ResolverContexto"
```

---

### Task 11: Implementar `ListarPorPaciente` + `Detalhar`

**Files:**
- Modify: `Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs`

- [ ] **Step 1: Substituir `ListarPorPaciente` (que está com NotImplemented)**

```csharp
    public async Task<List<EvolucaoListagemViewModel>> ListarPorPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        var evolucoes = await _db.EvolucaoClinicas
            .Include(e => e.Receita)
            .Include(e => e.CategoriaCID)
            .Where(e => e.PacienteID == paciente.ID)
            .OrderByDescending(e => e.DataConsulta)
            .ToListAsync();

        return evolucoes.Select(e => new EvolucaoListagemViewModel
        {
            Id             = e.Id,
            DataConsulta   = e.DataConsulta,
            Status         = e.Status,
            StatusLabel    = StatusLabel(e.Status),
            ReceitaID      = e.ReceitaID,
            IctDaReceita   = e.Receita?.ICT,
            ReceitaAdesao  = e.Receita?.Adesao,
            CidCodigo      = e.CategoriaCID?.Code,
            CidTitulo      = e.CategoriaCID?.Title
        }).ToList();
    }
```

- [ ] **Step 2: Substituir `Detalhar` (que está com NotImplemented)**

```csharp
    public async Task<EvolucaoDetalheViewModel?> Detalhar(int id, string email)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var ev = await _db.EvolucaoClinicas
            .Include(e => e.Paciente)
            .Include(e => e.Receita)
            .Include(e => e.CategoriaCID)
            .FirstOrDefaultAsync(e => e.Id == id && e.Paciente.UsuarioCriacaoID == usuario.id);

        if (ev == null) return null;

        return new EvolucaoDetalheViewModel
        {
            Id                 = ev.Id,
            DataConsulta       = ev.DataConsulta,
            Status             = ev.Status,
            StatusLabel        = StatusLabel(ev.Status),
            ReceitaID          = ev.ReceitaID,
            IctDaReceita       = ev.Receita?.ICT,
            ReceitaAdesao      = ev.Receita?.Adesao,
            CategoriaCID_ID    = ev.CategoriaCID_ID,
            CidCodigo          = ev.CategoriaCID?.Code,
            CidTitulo          = ev.CategoriaCID?.Title,
            Subjetivo          = ev.Subjetivo,
            Avaliacao          = ev.Avaliacao,
            Plano              = ev.Plano,
            ObservacaoObjetivo = ev.ObservacaoObjetivo,
            PaSistolica        = ev.PaSistolica,
            PaDiastolica       = ev.PaDiastolica,
            FrequenciaCardiaca = ev.FrequenciaCardiaca,
            Peso               = ev.Peso,
            Spo2               = ev.Spo2,
            Glicemia           = ev.Glicemia,
            Hba1c              = ev.Hba1c,
            Creatinina         = ev.Creatinina,
            EventosAdversos    = ev.EventosAdversos,
            Hospitalizacoes    = ev.Hospitalizacoes,
            IdasEmergencia     = ev.IdasEmergencia
        };
    }
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```powershell
git add Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs
git commit -m "feat(service): implementar ListarPorPaciente e Detalhar"
```

---

### Task 12: Implementar `ListarCIDsDoPaciente` + `ListarReceitasDoPaciente`

**Files:**
- Modify: `Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs`

- [ ] **Step 1: Substituir `ListarCIDsDoPaciente`**

```csharp
    public async Task<List<CIDOpcaoViewModel>> ListarCIDsDoPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        // CIDs vinculados às receitas do paciente (sem duplicar)
        var cids = await _db.ReceitaCIDs
            .Include(rc => rc.CategoriaCID)
            .Where(rc => rc.Receita.PacienteID == paciente.ID)
            .Select(rc => new CIDOpcaoViewModel
            {
                Id     = rc.CategoriaCID.Id,
                Codigo = rc.CategoriaCID.Code ?? "",
                Titulo = rc.CategoriaCID.Title ?? ""
            })
            .Distinct()
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        return cids;
    }
```

> **Nota:** confira em `ModelsNew/ReceitaCID.cs` qual é o nome da navegação para `Categorias_CID`. Se for diferente de `CategoriaCID`, ajustar o `.Include(rc => rc.CategoriaCID)` e o `rc.CategoriaCID.Id` no `Select`.

- [ ] **Step 2: Substituir `ListarReceitasDoPaciente`**

```csharp
    public async Task<List<ReceitaOpcaoViewModel>> ListarReceitasDoPaciente(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        return await _db.Receita
            .Where(r => r.PacienteID == paciente.ID)
            .OrderByDescending(r => r.DataCriacao)
            .Select(r => new ReceitaOpcaoViewModel
            {
                Id          = r.Id,
                DataCriacao = r.DataCriacao,
                Ict         = r.ICT
            })
            .ToListAsync();
    }
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`. Se falhar por nome de navegação errado em `ReceitaCID`, ajustar conforme nota acima.

- [ ] **Step 4: Commit**

```powershell
git add Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs
git commit -m "feat(service): implementar ListarCIDsDoPaciente e ListarReceitasDoPaciente"
```

---

### Task 13: Implementar `ObterSerieTemporal`

**Files:**
- Modify: `Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs`

- [ ] **Step 1: Substituir `ObterSerieTemporal`**

```csharp
    public async Task<SerieTemporalViewModel> ObterSerieTemporal(int pacienteId, string email)
    {
        var (_, paciente) = await ResolverContexto(pacienteId, email);

        var evolucoes = await _db.EvolucaoClinicas
            .Include(e => e.Receita)
            .Where(e => e.PacienteID == paciente.ID)
            .OrderBy(e => e.DataConsulta)
            .ToListAsync();

        var serie = new SerieTemporalViewModel();
        foreach (var e in evolucoes)
        {
            serie.Datas.Add(e.DataConsulta);
            serie.IctPorEvolucao.Add(e.Receita?.ICT);
            serie.StatusPorEvolucao.Add((int)e.Status);

            serie.PaSistolica.Add(e.PaSistolica);
            serie.PaDiastolica.Add(e.PaDiastolica);
            serie.FrequenciaCardiaca.Add(e.FrequenciaCardiaca);
            serie.Peso.Add(e.Peso);
            serie.Spo2.Add(e.Spo2);

            serie.Glicemia.Add(e.Glicemia);
            serie.Hba1c.Add(e.Hba1c);
            serie.Creatinina.Add(e.Creatinina);

            serie.EventosAdversos.Add(e.EventosAdversos);
            serie.Hospitalizacoes.Add(e.Hospitalizacoes);
            serie.IdasEmergencia.Add(e.IdasEmergencia);
        }
        return serie;
    }
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Services/EvolucaoClinicaServices/Implementacao/EvolucaoClinicaServices.cs
git commit -m "feat(service): implementar ObterSerieTemporal para gráficos longitudinais"
```

---

## Fase 5 — Controller e DI

### Task 14: Criar `EvolucaoClinicaController`

**Files:**
- Create: `Controllers/EvolucaoClinicaController.cs`

- [ ] **Step 1: Criar o controller**

```csharp
using Microsoft.AspNetCore.Mvc;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Models.Request;
using Proj_ICFT.Services.EvolucaoClinicaServices.Interface;

namespace Proj_ICFT.Controllers;

public class EvolucaoClinicaController : Controller
{
    private readonly IEvolucaoClinicaServices _ev;

    public EvolucaoClinicaController(IEvolucaoClinicaServices ev)
    {
        _ev = ev;
    }

    private string? EmailSessao => HttpContext.Session.GetString("_UserEmail");

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarPorPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarPorPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar evoluções." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> Detalhar(int id)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.Detalhar(id, email);
            if (data == null)
                return Json(new { success = false, message = "Evolução não encontrada." });

            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao carregar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Salvar([FromBody] SalvarEvolucaoRequest dto)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var id = await _ev.Salvar(dto, email);
            return Json(new { success = true, id });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao salvar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Atualizar(int id, [FromBody] SalvarEvolucaoRequest dto)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            await _ev.Atualizar(id, dto, email);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao atualizar evolução." });
        }
    }

    [HttpPost, SessionFilter]
    public async Task<JsonResult> Deletar(int id)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            await _ev.Deletar(id, email);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao deletar evolução." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarCIDsDoPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarCIDsDoPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar CIDs." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> ListarReceitasDoPaciente(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ListarReceitasDoPaciente(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao listar receitas." });
        }
    }

    [HttpGet, SessionFilter]
    public async Task<JsonResult> SerieTemporal(int pacienteId)
    {
        try
        {
            var email = EmailSessao;
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Sessão expirada." });

            var data = await _ev.ObterSerieTemporal(pacienteId, email);
            return Json(new { success = true, data });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "Erro ao gerar série temporal." });
        }
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add Controllers/EvolucaoClinicaController.cs
git commit -m "feat(controller): adicionar EvolucaoClinicaController com 8 endpoints"
```

---

### Task 15: Registrar service em `Program.cs`

**Files:**
- Modify: `Program.cs`

- [ ] **Step 1: Adicionar using**

No topo de `Program.cs`, junto aos outros `using Proj_ICFT.Services.*`, adicionar:

```csharp
using Proj_ICFT.Services.EvolucaoClinicaServices.Implementacao;
using Proj_ICFT.Services.EvolucaoClinicaServices.Interface;
```

- [ ] **Step 2: Registrar o AddScoped**

Logo após `builder.Services.AddScoped<IMedicamentoService, MedicamentosServices>();` (linha 32), adicionar:

```csharp
builder.Services.AddScoped<IEvolucaoClinicaServices, EvolucaoClinicaServices>();
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Verificação manual rápida — subir o app**

```powershell
dotnet run
```

Abrir o browser em `https://localhost:<porta>` e fazer login. Em outra aba, testar o endpoint protegido com curl (vai retornar 302 redirect para Login se a sessão estiver válida — mas não vai dar 500):

```powershell
curl -k "https://localhost:<porta>/EvolucaoClinica/ListarPorPaciente?pacienteId=1" -v
```

Expected: resposta JSON `{"success":false,"message":"..."}` ou redirect (não 500).

Parar o app (`Ctrl+C`).

- [ ] **Step 5: Commit**

```powershell
git add Program.cs
git commit -m "chore(di): registrar IEvolucaoClinicaServices em Program.cs"
```

---

## Fase 6 — Frontend Foundation

### Task 16: Adicionar Chart.js ao `_Layout.cshtml`

**Files:**
- Modify: `Views/Shared/_Layout.cshtml`

- [ ] **Step 1: Adicionar a tag `<script>` do Chart.js**

Localizar a seção onde estão os scripts globais (provavelmente perto de `</body>` ou no `<head>`, próximo às tags de `xlsx.full.min.js` e `html2pdf.js`). Adicionar:

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.4/dist/chart.umd.min.js"></script>
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Verificação manual**

```powershell
dotnet run
```

Abrir qualquer página, abrir DevTools → Console e digitar:

```js
typeof Chart
```

Expected: `"function"` (Chart.js carregou). Parar o app.

- [ ] **Step 4: Commit**

```powershell
git add Views/Shared/_Layout.cshtml
git commit -m "chore(layout): adicionar Chart.js 4.x via CDN para visualização de evoluções"
```

---

### Task 17: Criar partial `_EvolucoesTab.cshtml`

**Files:**
- Create: `Views/Paciente/_EvolucoesTab.cshtml`

- [ ] **Step 1: Criar a partial com estrutura HTML**

```cshtml
@model Proj_ICFT.ModelsNew.PacienteICT

<div class="evol-toolbar d-flex justify-content-between align-items-center mb-3">
    <button class="btn btn-primary" id="btnNovaEvolucao" data-paciente-id="@Model.ID">
        <i class="bi bi-plus-circle"></i> Nova Evolução
    </button>
    <button class="btn btn-outline-secondary btn-sm" id="btnRecarregarEvol">
        <i class="bi bi-arrow-clockwise"></i> Atualizar
    </button>
</div>

<div id="evolEmptyState" class="vp-empty" style="display:none">
    <i class="bi bi-graph-up-arrow"></i>
    <p>Nenhuma evolução clínica registrada para este paciente.</p>
    <button class="btn btn-primary" onclick="document.getElementById('btnNovaEvolucao').click()">
        <i class="bi bi-plus-lg"></i> Registrar primeira evolução
    </button>
</div>

<div id="evolConteudo" style="display:none">
    <div class="card mb-3">
        <div class="card-body">
            <h6 class="mb-3"><i class="bi bi-activity me-1"></i> ICT × Status de Evolução</h6>
            <canvas id="chartIctStatus" height="120"></canvas>
        </div>
    </div>

    <button class="btn btn-link btn-sm mb-2" type="button"
            data-bs-toggle="collapse" data-bs-target="#graficosDetalhados"
            id="btnToggleDetalhes">
        <i class="bi bi-chevron-down"></i> Ver gráficos detalhados de indicadores
    </button>

    <div class="collapse mb-3" id="graficosDetalhados">
        <div class="row g-2">
            <div class="col-md-6"><div class="card"><div class="card-body"><h6>Pressão Arterial + FC</h6><canvas id="chartPA" height="180"></canvas></div></div></div>
            <div class="col-md-6"><div class="card"><div class="card-body"><h6>Glicemia + HbA1c</h6><canvas id="chartGlicemia" height="180"></canvas></div></div></div>
            <div class="col-md-6"><div class="card"><div class="card-body"><h6>Peso</h6><canvas id="chartPeso" height="180"></canvas></div></div></div>
            <div class="col-md-6"><div class="card"><div class="card-body"><h6>Eventos / Internações</h6><canvas id="chartContadores" height="180"></canvas></div></div></div>
        </div>
    </div>

    <table class="table table-hover" id="tabelaEvolucoes">
        <thead>
            <tr>
                <th>Data Consulta</th>
                <th>Status</th>
                <th>CID</th>
                <th>Receita / ICT</th>
                <th class="text-end">Ações</th>
            </tr>
        </thead>
        <tbody></tbody>
    </table>
</div>

<partial name="_ModalEvolucao" model="Model" />
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded` (warning sobre `_ModalEvolucao` não existente é tolerável — será criado em Task 19).

- [ ] **Step 3: Commit**

```powershell
git add Views/Paciente/_EvolucoesTab.cshtml
git commit -m "feat(view): criar partial _EvolucoesTab.cshtml com estrutura de gráficos e tabela"
```

---

### Task 18: Refatorar `VerPaciente.cshtml` para nav-tabs

**Files:**
- Modify: `Views/Paciente/VerPaciente.cshtml`

- [ ] **Step 1: Adicionar a estrutura de tabs envolvendo o título "Receitas" e a lista**

Localizar no arquivo (linha ~106-164) o trecho que começa com:

```cshtml
        @* ── Lista de receitas ── *@
        <div class="vp-section-title">
            <i class="bi bi-file-earmark-medical me-2 text-primary"></i>Receitas
        </div>

        @if (!receitas.Any())
        {
            ...
        }
        else
        {
            <div class="vp-receitas-list">
                ...
            </div>
        }
```

Substituir esse bloco inteiro por:

```cshtml
        @* ── Tabs: Receitas / Evoluções ── *@
        <ul class="nav nav-tabs mb-3" id="pacienteTabs" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link active" id="tab-receitas-btn" data-bs-toggle="tab"
                        data-bs-target="#tab-receitas" type="button" role="tab">
                    <i class="bi bi-file-earmark-medical me-1"></i> Receitas
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="tab-evolucoes-btn" data-bs-toggle="tab"
                        data-bs-target="#tab-evolucoes" type="button" role="tab">
                    <i class="bi bi-graph-up-arrow me-1"></i> Evoluções Clínicas
                </button>
            </li>
        </ul>

        <div class="tab-content">
            <div class="tab-pane fade show active" id="tab-receitas" role="tabpanel">
                @if (!receitas.Any())
                {
                    <div class="vp-empty">
                        <i class="bi bi-file-earmark-x"></i>
                        <p>Nenhuma receita cadastrada para este paciente.</p>
                        @if (!isAnonimo)
                        {
                            <a href="/Home/Form?pacienteId=@Model.ID" class="btn-novo-paciente">
                                <i class="bi bi-plus-lg"></i> Criar primeira receita
                            </a>
                        }
                    </div>
                }
                else
                {
                    <div class="vp-receitas-list">
                        @foreach (var (r, idx) in receitas.Select((r, i) => (r, i + 1)))
                        {
                            <div class="vp-receita-card">
                                <div class="vp-receita-num">#@(receitas.Count - idx + 1)</div>
                                <div class="vp-receita-info">
                                    <span class="vp-receita-data">
                                        <i class="bi bi-calendar3"></i> @r.DataCriacao.ToString("dd/MM/yyyy")
                                    </span>
                                    <span class="vp-receita-meds-count">
                                        <i class="bi bi-capsule"></i> @r.ReceitaMeds.Count @(r.ReceitaMeds.Count == 1 ? "medicamento" : "medicamentos")
                                    </span>
                                </div>
                                <div class="vp-receita-ict">
                                    <span class="vp-ict-label">ICT</span>
                                    <span class="vp-ict-value">@r.ICT.ToString("F2")</span>
                                </div>
                                <div class="vp-receita-adesao @(r.Adesao ? "aderiu" : "nao-aderiu")">
                                    @if (r.Adesao)
                                    {
                                        <i class="bi bi-check-circle-fill"></i> <span>Aderiu</span>
                                    }
                                    else
                                    {
                                        <i class="bi bi-x-circle-fill"></i> <span>Aderiu</span>
                                    }
                                </div>
                                <button class="btn-ver-receita" onclick="abrirReceita(@r.Id)">
                                    <i class="bi bi-eye"></i> Ver receita
                                </button>
                            </div>
                        }
                    </div>
                }
            </div>

            <div class="tab-pane fade" id="tab-evolucoes" role="tabpanel">
                <partial name="_EvolucoesTab" model="Model" />
            </div>
        </div>
```

- [ ] **Step 2: Adicionar `<link>` e `<script>` no final do arquivo**

No final do `@section Scripts { ... }`, **antes** do `}` de fechamento da seção, adicionar:

```cshtml
<link rel="stylesheet" href="~/css/evolucao-clinica.css" asp-append-version="true" />
<script src="~/js/evolucao-clinica.js" asp-append-version="true"></script>
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Verificação manual**

```powershell
dotnet run
```

Logar, ir em PacientesAnalisados → clicar em um paciente. Deve ver as duas tabs ("Receitas" ativa por padrão, "Evoluções Clínicas" inativa). Clicar na tab "Evoluções Clínicas" não dá erro (mas mostra empty state ou nada — JS é da próxima task). Parar o app.

- [ ] **Step 5: Commit**

```powershell
git add Views/Paciente/VerPaciente.cshtml
git commit -m "feat(view): refatorar VerPaciente.cshtml para usar nav-tabs com aba Evoluções"
```

---

## Fase 7 — Modal SOAP

### Task 19: Criar partial `_ModalEvolucao.cshtml`

**Files:**
- Create: `Views/Paciente/_ModalEvolucao.cshtml`

- [ ] **Step 1: Criar a partial completa**

```cshtml
@model Proj_ICFT.ModelsNew.PacienteICT

<div class="modal fade" id="modalEvolucao" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-xl modal-dialog-scrollable">
        <div class="modal-content">

            <div class="modal-header">
                <h5 class="modal-title" id="modalEvolucaoTitulo">
                    <i class="bi bi-clipboard2-pulse me-2 text-primary"></i>
                    <span id="modalEvolucaoTituloTxt">Nova Evolução Clínica</span>
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
            </div>

            <form id="formEvolucao" autocomplete="off">
                <input type="hidden" id="evolucaoId" />
                <input type="hidden" id="evolPacienteId" value="@Model.ID" />

                <div class="modal-body">

                    <div class="row g-3 mb-3">
                        <div class="col-md-4">
                            <label class="form-label">Data da consulta</label>
                            <input type="date" class="form-control" id="evolDataConsulta" required />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Receita vinculada (opcional)</label>
                            <select class="form-select" id="evolReceitaId">
                                <option value="">Nenhuma</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">CID monitorado (opcional)</label>
                            <select class="form-select" id="evolCidId">
                                <option value="">Nenhum</option>
                            </select>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label d-block">Status da evolução</label>
                        <div class="btn-group" role="group" id="evolStatusGroup">
                            <input type="radio" class="btn-check" name="evolStatus" id="statusInconclusivo" value="0" checked />
                            <label class="btn btn-outline-secondary" for="statusInconclusivo">Inconclusivo</label>

                            <input type="radio" class="btn-check" name="evolStatus" id="statusMelhorou" value="1" />
                            <label class="btn btn-outline-success" for="statusMelhorou">Melhorou</label>

                            <input type="radio" class="btn-check" name="evolStatus" id="statusEstavel" value="2" />
                            <label class="btn btn-outline-warning" for="statusEstavel">Estável</label>

                            <input type="radio" class="btn-check" name="evolStatus" id="statusPiorou" value="3" />
                            <label class="btn btn-outline-danger" for="statusPiorou">Piorou</label>
                        </div>
                    </div>

                    <hr/>

                    <h6 class="text-primary"><i class="bi bi-chat-left-text me-1"></i> Subjetivo</h6>
                    <textarea class="form-control mb-3" id="evolSubjetivo" rows="2" maxlength="2000"
                              placeholder="Queixa do paciente, percepção, sintomas relatados..."></textarea>

                    <h6 class="text-primary"><i class="bi bi-clipboard-data me-1"></i> Objetivo</h6>

                    <div class="card mb-2">
                        <div class="card-body py-2">
                            <small class="text-muted">Sinais Vitais</small>
                            <div class="row g-2 mt-1">
                                <div class="col-md-2"><label class="form-label small">PA Sist. (mmHg)</label><input type="number" class="form-control" id="evolPaSist" min="50" max="300" /></div>
                                <div class="col-md-2"><label class="form-label small">PA Diast. (mmHg)</label><input type="number" class="form-control" id="evolPaDiast" min="30" max="200" /></div>
                                <div class="col-md-2"><label class="form-label small">FC (bpm)</label><input type="number" class="form-control" id="evolFc" min="30" max="220" /></div>
                                <div class="col-md-3"><label class="form-label small">Peso (kg)</label><input type="number" class="form-control" id="evolPeso" min="1" max="400" step="0.1" /></div>
                                <div class="col-md-3"><label class="form-label small">SpO2 (%)</label><input type="number" class="form-control" id="evolSpo2" min="50" max="100" /></div>
                            </div>
                        </div>
                    </div>

                    <div class="card mb-2">
                        <div class="card-body py-2">
                            <small class="text-muted">Laboratoriais</small>
                            <div class="row g-2 mt-1">
                                <div class="col-md-4"><label class="form-label small">Glicemia (mg/dL)</label><input type="number" class="form-control" id="evolGlicemia" min="20" max="800" step="0.1" /></div>
                                <div class="col-md-4"><label class="form-label small">HbA1c (%)</label><input type="number" class="form-control" id="evolHba1c" min="3" max="20" step="0.1" /></div>
                                <div class="col-md-4"><label class="form-label small">Creatinina (mg/dL)</label><input type="number" class="form-control" id="evolCreatinina" min="0.1" max="20" step="0.01" /></div>
                            </div>
                        </div>
                    </div>

                    <div class="card mb-3">
                        <div class="card-body py-2">
                            <small class="text-muted">Desde a última consulta</small>
                            <div class="row g-2 mt-1">
                                <div class="col-md-4"><label class="form-label small">Eventos adversos</label><input type="number" class="form-control" id="evolEventos" min="0" value="0" /></div>
                                <div class="col-md-4"><label class="form-label small">Hospitalizações</label><input type="number" class="form-control" id="evolHosp" min="0" value="0" /></div>
                                <div class="col-md-4"><label class="form-label small">Idas a emergência</label><input type="number" class="form-control" id="evolEmerg" min="0" value="0" /></div>
                            </div>
                        </div>
                    </div>

                    <label class="form-label small">Observação livre do Objetivo</label>
                    <textarea class="form-control mb-3" id="evolObsObj" rows="2" maxlength="2000"
                              placeholder="Outras observações objetivas (exame físico, escalas, etc.)"></textarea>

                    <h6 class="text-primary"><i class="bi bi-bullseye me-1"></i> Avaliação</h6>
                    <textarea class="form-control mb-3" id="evolAvaliacao" rows="2" maxlength="2000"
                              placeholder="Análise clínica do profissional..."></textarea>

                    <h6 class="text-primary"><i class="bi bi-list-check me-1"></i> Plano</h6>
                    <textarea class="form-control" id="evolPlano" rows="2" maxlength="2000"
                              placeholder="Conduta a seguir, ajustes, próximos passos..."></textarea>

                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="submit" class="btn btn-primary" id="btnSalvarEvolucao">
                        <i class="bi bi-check2-circle"></i> Salvar Evolução
                    </button>
                </div>
            </form>

        </div>
    </div>
</div>
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Verificação manual**

```powershell
dotnet run
```

Navegar até VerPaciente. O modal não deve aparecer ainda (sem JS). Inspecionar via DevTools que existe `<div id="modalEvolucao">` no DOM. Parar o app.

- [ ] **Step 4: Commit**

```powershell
git add Views/Paciente/_ModalEvolucao.cshtml
git commit -m "feat(view): criar partial _ModalEvolucao.cshtml com formulário SOAP completo"
```

---

## Fase 8 — JavaScript

### Task 20: Criar `evolucao-clinica.js` — base + carregamento

**Files:**
- Create: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Criar o arquivo com bootstrap e funções principais**

```javascript
(function () {
    'use strict';

    const pacienteId = parseInt(document.getElementById('evolPacienteId')?.value || '0', 10);
    if (!pacienteId) return;

    let chartIctStatus = null;
    let chartPA = null, chartGlicemia = null, chartPeso = null, chartContadores = null;
    let detalhesRenderizados = false;
    let evolucoesCache = [];
    let serieCache = null;

    const STATUS_CORES = {
        0: '#6c757d', // Inconclusivo - cinza
        1: '#198754', // Melhorou - verde
        2: '#ffc107', // Estável - amarelo
        3: '#dc3545'  // Piorou - vermelho
    };
    const STATUS_LABELS = {
        0: 'Inconclusivo', 1: 'Melhorou', 2: 'Estável', 3: 'Piorou'
    };

    const modalEvol = new bootstrap.Modal(document.getElementById('modalEvolucao'));

    // ============ Carregamento da aba (lazy) ============
    document.getElementById('tab-evolucoes-btn')?.addEventListener('shown.bs.tab', carregarEvolucoes);
    document.getElementById('btnRecarregarEvol')?.addEventListener('click', carregarEvolucoes);

    let jaCarregou = false;
    async function carregarEvolucoes() {
        try {
            const [listaResp, serieResp] = await Promise.all([
                fetch(`/EvolucaoClinica/ListarPorPaciente?pacienteId=${pacienteId}`).then(r => r.json()),
                fetch(`/EvolucaoClinica/SerieTemporal?pacienteId=${pacienteId}`).then(r => r.json())
            ]);

            if (!listaResp.success) {
                Swal.fire('Erro', listaResp.message || 'Falha ao carregar evoluções.', 'error');
                return;
            }
            evolucoesCache = listaResp.data || [];
            serieCache = serieResp.success ? serieResp.data : null;

            if (evolucoesCache.length === 0) {
                document.getElementById('evolEmptyState').style.display = '';
                document.getElementById('evolConteudo').style.display = 'none';
                return;
            }

            document.getElementById('evolEmptyState').style.display = 'none';
            document.getElementById('evolConteudo').style.display = '';
            renderizarTabela(evolucoesCache);
            renderizarGraficoPrincipal(serieCache);
            detalhesRenderizados = false;

            jaCarregou = true;
        } catch (e) {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    }

    // Stubs — implementados em tasks 21+
    function renderizarTabela(lista) { /* Task 21 */ }
    function renderizarGraficoPrincipal(serie) { /* Task 25 */ }
    function renderizarGraficosDetalhados(serie) { /* Task 25 */ }

    // ============ Query string: abrir modal direto vindo de Form.cshtml ============
    document.addEventListener('DOMContentLoaded', () => {
        const params = new URLSearchParams(window.location.search);
        if (params.get('abrirEvolucao') === '1') {
            const tabBtn = document.getElementById('tab-evolucoes-btn');
            if (tabBtn) bootstrap.Tab.getOrCreateInstance(tabBtn).show();
            setTimeout(() => {
                const receitaId = parseInt(params.get('receitaId') || '0', 10) || null;
                abrirModalEvolucao({ modo: 'criar', receitaIdPreSel: receitaId });
            }, 400);
        }
    });

    // Stub — implementado em Task 22
    async function abrirModalEvolucao(opts) { /* Task 22 */ }

    // Expor handlers globalmente para uso inline (botões)
    window.__evol = { carregarEvolucoes, abrirModalEvolucao };

    // Botão "Nova Evolução"
    document.getElementById('btnNovaEvolucao')?.addEventListener('click', () => {
        abrirModalEvolucao({ modo: 'criar' });
    });

})();
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): scaffold de evolucao-clinica.js com lazy load e query string handler"
```

---

### Task 21: JS — renderização da tabela

**Files:**
- Modify: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Substituir a função `renderizarTabela`**

Localizar `function renderizarTabela(lista) { /* Task 21 */ }` e substituir por:

```javascript
    function renderizarTabela(lista) {
        const tbody = document.querySelector('#tabelaEvolucoes tbody');
        tbody.innerHTML = lista.map(e => {
            const data = new Date(e.dataConsulta).toLocaleDateString('pt-BR');
            const cor = STATUS_CORES[e.status] || '#6c757d';
            const cid = e.cidCodigo ? `${e.cidCodigo} — ${e.cidTitulo}` : '<span class="text-muted">—</span>';
            const receita = e.receitaID
                ? `Receita #${e.receitaID} — ICT ${e.ictDaReceita?.toFixed(2) ?? '—'}`
                : '<span class="text-muted">Sem receita</span>';
            return `
                <tr>
                    <td>${data}</td>
                    <td><span class="badge" style="background:${cor}">${e.statusLabel}</span></td>
                    <td>${cid}</td>
                    <td>${receita}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-secondary btn-ver-evol" data-id="${e.id}" title="Ver">
                            <i class="bi bi-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-primary btn-editar-evol" data-id="${e.id}" title="Editar">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger btn-deletar-evol" data-id="${e.id}" title="Deletar">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>`;
        }).join('');
    }
```

- [ ] **Step 2: Adicionar event delegation para os botões da tabela**

No final do IIFE (antes do `})();`), antes da última linha, adicionar:

```javascript
    // Event delegation para botões da tabela
    document.querySelector('#tabelaEvolucoes tbody').addEventListener('click', (e) => {
        const btnVer = e.target.closest('.btn-ver-evol');
        const btnEdit = e.target.closest('.btn-editar-evol');
        const btnDel = e.target.closest('.btn-deletar-evol');
        if (btnVer) abrirModalEvolucao({ modo: 'ler', id: parseInt(btnVer.dataset.id, 10) });
        if (btnEdit) abrirModalEvolucao({ modo: 'editar', id: parseInt(btnEdit.dataset.id, 10) });
        if (btnDel) confirmarDeletar(parseInt(btnDel.dataset.id, 10));
    });

    function confirmarDeletar(id) { /* Task 24 */ }
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): renderizar tabela de evoluções com badge de status e botões de ação"
```

---

### Task 22: JS — abrir modal (criar, editar, ler)

**Files:**
- Modify: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Substituir a função `abrirModalEvolucao` stub**

Localizar `async function abrirModalEvolucao(opts) { /* Task 22 */ }` e substituir por:

```javascript
    async function abrirModalEvolucao({ modo, id, receitaIdPreSel, cidPreSel } = {}) {
        // Resetar form
        document.getElementById('formEvolucao').reset();
        document.getElementById('evolucaoId').value = '';

        const tituloEl = document.getElementById('modalEvolucaoTituloTxt');
        const btnSalvar = document.getElementById('btnSalvarEvolucao');

        // Carregar selects (receitas + CIDs do paciente) sempre que abrir
        await carregarSelectsModal();

        if (modo === 'criar') {
            tituloEl.textContent = 'Nova Evolução Clínica';
            btnSalvar.style.display = '';
            habilitarCampos(true);
            document.getElementById('evolDataConsulta').value = new Date().toISOString().substring(0, 10);
            if (receitaIdPreSel) document.getElementById('evolReceitaId').value = receitaIdPreSel;
            if (cidPreSel) document.getElementById('evolCidId').value = cidPreSel;
            modalEvol.show();
            return;
        }

        // ler ou editar — buscar dados
        const resp = await fetch(`/EvolucaoClinica/Detalhar?id=${id}`).then(r => r.json());
        if (!resp.success) {
            Swal.fire('Erro', resp.message || 'Erro ao carregar evolução.', 'error');
            return;
        }
        preencherForm(resp.data);
        document.getElementById('evolucaoId').value = id;

        if (modo === 'ler') {
            tituloEl.textContent = 'Detalhes da Evolução';
            btnSalvar.style.display = 'none';
            habilitarCampos(false);
        } else {
            tituloEl.textContent = 'Editar Evolução';
            btnSalvar.style.display = '';
            habilitarCampos(true);
        }
        modalEvol.show();
    }

    async function carregarSelectsModal() {
        const [rRec, rCid] = await Promise.all([
            fetch(`/EvolucaoClinica/ListarReceitasDoPaciente?pacienteId=${pacienteId}`).then(r => r.json()),
            fetch(`/EvolucaoClinica/ListarCIDsDoPaciente?pacienteId=${pacienteId}`).then(r => r.json())
        ]);
        const selRec = document.getElementById('evolReceitaId');
        selRec.innerHTML = '<option value="">Nenhuma</option>' +
            (rRec.success ? rRec.data.map(x => {
                const d = new Date(x.dataCriacao).toLocaleDateString('pt-BR');
                return `<option value="${x.id}">${d} — ICT ${x.ict.toFixed(2)}</option>`;
            }).join('') : '');

        const selCid = document.getElementById('evolCidId');
        selCid.innerHTML = '<option value="">Nenhum</option>' +
            (rCid.success ? rCid.data.map(x => `<option value="${x.id}">${x.codigo} — ${x.titulo}</option>`).join('') : '');
    }

    function preencherForm(d) {
        document.getElementById('evolDataConsulta').value = (d.dataConsulta || '').substring(0, 10);
        document.getElementById('evolReceitaId').value = d.receitaID ?? '';
        document.getElementById('evolCidId').value = d.categoriaCID_ID ?? '';
        document.querySelector(`input[name="evolStatus"][value="${d.status}"]`).checked = true;

        document.getElementById('evolSubjetivo').value = d.subjetivo ?? '';
        document.getElementById('evolAvaliacao').value = d.avaliacao ?? '';
        document.getElementById('evolPlano').value = d.plano ?? '';
        document.getElementById('evolObsObj').value = d.observacaoObjetivo ?? '';

        document.getElementById('evolPaSist').value = d.paSistolica ?? '';
        document.getElementById('evolPaDiast').value = d.paDiastolica ?? '';
        document.getElementById('evolFc').value = d.frequenciaCardiaca ?? '';
        document.getElementById('evolPeso').value = d.peso ?? '';
        document.getElementById('evolSpo2').value = d.spo2 ?? '';

        document.getElementById('evolGlicemia').value = d.glicemia ?? '';
        document.getElementById('evolHba1c').value = d.hba1c ?? '';
        document.getElementById('evolCreatinina').value = d.creatinina ?? '';

        document.getElementById('evolEventos').value = d.eventosAdversos ?? 0;
        document.getElementById('evolHosp').value = d.hospitalizacoes ?? 0;
        document.getElementById('evolEmerg').value = d.idasEmergencia ?? 0;
    }

    function habilitarCampos(enabled) {
        const form = document.getElementById('formEvolucao');
        form.querySelectorAll('input, select, textarea').forEach(el => {
            if (el.type !== 'hidden') el.disabled = !enabled;
        });
    }
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): implementar abrirModalEvolucao para criar, editar e ler"
```

---

### Task 23: JS — submissão do formulário (criar/editar)

**Files:**
- Modify: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Adicionar handler de submit**

No final do IIFE (antes do `})();` final), adicionar:

```javascript
    document.getElementById('formEvolucao').addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = parseInt(document.getElementById('evolucaoId').value || '0', 10);
        const payload = montarPayload();

        try {
            const url = id > 0
                ? `/EvolucaoClinica/Atualizar?id=${id}`
                : `/EvolucaoClinica/Salvar`;
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            }).then(r => r.json());

            if (!resp.success) {
                Swal.fire('Erro', resp.message || 'Erro ao salvar.', 'error');
                return;
            }
            modalEvol.hide();
            Swal.fire({ icon: 'success', title: 'Evolução salva!', timer: 1500, showConfirmButton: false });
            await carregarEvolucoes();
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    });

    function montarPayload() {
        const num  = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseFloat(v); };
        const int_ = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseInt(v, 10); };
        const intN = (id) => { const v = document.getElementById(id).value; return v === '' ? 0    : parseInt(v, 10); };
        const txt  = (id) => { const v = document.getElementById(id).value.trim(); return v === '' ? null : v; };
        const sel  = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseInt(v, 10); };

        return {
            PacienteID: pacienteId,
            ReceitaID: sel('evolReceitaId'),
            CategoriaCID_ID: sel('evolCidId'),
            DataConsulta: document.getElementById('evolDataConsulta').value,
            Status: parseInt(document.querySelector('input[name="evolStatus"]:checked').value, 10),

            Subjetivo: txt('evolSubjetivo'),
            Avaliacao: txt('evolAvaliacao'),
            Plano: txt('evolPlano'),
            ObservacaoObjetivo: txt('evolObsObj'),

            PaSistolica: int_('evolPaSist'),
            PaDiastolica: int_('evolPaDiast'),
            FrequenciaCardiaca: int_('evolFc'),
            Peso: num('evolPeso'),
            Spo2: int_('evolSpo2'),

            Glicemia: num('evolGlicemia'),
            Hba1c: num('evolHba1c'),
            Creatinina: num('evolCreatinina'),

            EventosAdversos: intN('evolEventos'),
            Hospitalizacoes: intN('evolHosp'),
            IdasEmergencia: intN('evolEmerg')
        };
    }
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): implementar submit do formulário de evolução (criar/editar)"
```

---

### Task 24: JS — deletar com SweetAlert

**Files:**
- Modify: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Substituir o stub `confirmarDeletar`**

Localizar `function confirmarDeletar(id) { /* Task 24 */ }` e substituir por:

```javascript
    async function confirmarDeletar(id) {
        const result = await Swal.fire({
            icon: 'warning',
            title: 'Excluir evolução?',
            text: 'Esta ação não poderá ser desfeita.',
            showCancelButton: true,
            confirmButtonText: 'Excluir',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc3545'
        });
        if (!result.isConfirmed) return;

        try {
            const resp = await fetch(`/EvolucaoClinica/Deletar?id=${id}`, { method: 'POST' }).then(r => r.json());
            if (!resp.success) {
                Swal.fire('Erro', resp.message || 'Falha ao excluir.', 'error');
                return;
            }
            Swal.fire({ icon: 'success', title: 'Excluído!', timer: 1200, showConfirmButton: false });
            await carregarEvolucoes();
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    }
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): implementar confirmação e exclusão de evolução"
```

---

### Task 25: JS — Chart.js (gráfico principal + detalhados lazy)

**Files:**
- Modify: `wwwroot/js/evolucao-clinica.js`

- [ ] **Step 1: Substituir os stubs `renderizarGraficoPrincipal` e `renderizarGraficosDetalhados`**

Localizar e substituir:

```javascript
    function renderizarGraficoPrincipal(serie) {
        if (!serie || serie.datas.length === 0) return;
        const ctx = document.getElementById('chartIctStatus').getContext('2d');
        const labels = serie.datas.map(d => new Date(d).toLocaleDateString('pt-BR'));
        const cores = serie.statusPorEvolucao.map(s => STATUS_CORES[s] || '#6c757d');
        const statusLabels = serie.statusPorEvolucao.map(s => STATUS_LABELS[s] || '—');

        if (chartIctStatus) chartIctStatus.destroy();
        chartIctStatus = new Chart(ctx, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'ICT',
                    data: serie.ictPorEvolucao,
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13,110,253,0.1)',
                    pointBackgroundColor: cores,
                    pointBorderColor: cores,
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    spanGaps: false,
                    tension: 0.2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const ict = ctx.parsed.y;
                                const st = statusLabels[ctx.dataIndex];
                                return `ICT: ${ict ?? '—'} — Status: ${st}`;
                            }
                        }
                    },
                    legend: { display: false }
                },
                scales: {
                    y: { title: { display: true, text: 'ICT (complexidade)' } }
                }
            }
        });
    }

    function renderizarGraficosDetalhados(serie) {
        if (!serie || serie.datas.length === 0) return;
        const labels = serie.datas.map(d => new Date(d).toLocaleDateString('pt-BR'));

        // 1. PA + FC
        if (chartPA) chartPA.destroy();
        chartPA = new Chart(document.getElementById('chartPA').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [
                    { label: 'PA Sistólica', data: serie.paSistolica, borderColor: '#dc3545', spanGaps: false, tension: 0.2 },
                    { label: 'PA Diastólica', data: serie.paDiastolica, borderColor: '#fd7e14', spanGaps: false, tension: 0.2 },
                    { label: 'FC', data: serie.frequenciaCardiaca, borderColor: '#0dcaf0', spanGaps: false, tension: 0.2 }
                ]
            },
            options: { responsive: true }
        });

        // 2. Glicemia + HbA1c
        if (chartGlicemia) chartGlicemia.destroy();
        chartGlicemia = new Chart(document.getElementById('chartGlicemia').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [
                    { label: 'Glicemia', data: serie.glicemia, borderColor: '#6610f2', spanGaps: false, tension: 0.2, yAxisID: 'y' },
                    { label: 'HbA1c (%)', data: serie.hba1c, borderColor: '#198754', spanGaps: false, tension: 0.2, yAxisID: 'y1' }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    y: { type: 'linear', position: 'left', title: { display: true, text: 'Glicemia mg/dL' } },
                    y1: { type: 'linear', position: 'right', title: { display: true, text: 'HbA1c %' }, grid: { drawOnChartArea: false } }
                }
            }
        });

        // 3. Peso
        if (chartPeso) chartPeso.destroy();
        chartPeso = new Chart(document.getElementById('chartPeso').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [{ label: 'Peso (kg)', data: serie.peso, borderColor: '#6f42c1', spanGaps: false, tension: 0.2 }]
            },
            options: { responsive: true }
        });

        // 4. Contadores
        if (chartContadores) chartContadores.destroy();
        chartContadores = new Chart(document.getElementById('chartContadores').getContext('2d'), {
            type: 'bar',
            data: {
                labels,
                datasets: [
                    { label: 'Eventos Adversos', data: serie.eventosAdversos, backgroundColor: '#ffc107' },
                    { label: 'Hospitalizações', data: serie.hospitalizacoes, backgroundColor: '#dc3545' },
                    { label: 'Emergências', data: serie.idasEmergencia, backgroundColor: '#fd7e14' }
                ]
            },
            options: { responsive: true, scales: { y: { beginAtZero: true } } }
        });
    }
```

- [ ] **Step 2: Adicionar trigger de lazy render no toggle de collapse**

Antes do `})();` final, adicionar:

```javascript
    document.getElementById('graficosDetalhados')?.addEventListener('shown.bs.collapse', () => {
        if (!detalhesRenderizados && serieCache) {
            renderizarGraficosDetalhados(serieCache);
            detalhesRenderizados = true;
        }
    });
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Verificação manual end-to-end**

```powershell
dotnet run
```

1. Logar e ir em VerPaciente de algum paciente que já tem receitas.
2. Clicar na tab "Evoluções Clínicas" → ver empty state.
3. Clicar em "Registrar primeira evolução" → modal abre.
4. Preencher: data hoje, status "Estável", PA 130/80, Peso 75. Salvar.
5. Confirmar que aparece na tabela com badge amarelo "Estável".
6. Confirmar que o gráfico principal renderiza com 1 ponto azul/amarelo.
7. Clicar em "+ Detalhes" → confirmar que os 4 gráficos detalhados renderizam.
8. Editar a evolução → mudar peso pra 73 → salvar → ver atualização.
9. Deletar a evolução → confirmar Swal → ver empty state retornar.

Parar o app.

- [ ] **Step 5: Commit**

```powershell
git add wwwroot/js/evolucao-clinica.js
git commit -m "feat(js): renderizar gráficos Chart.js (ICT × Status + detalhados lazy)"
```

---

## Fase 9 — Estilos + Integração `Form.cshtml`

### Task 26: Criar `evolucao-clinica.css`

**Files:**
- Create: `wwwroot/css/evolucao-clinica.css`

- [ ] **Step 1: Criar o arquivo**

```css
.evol-toolbar {
    border-bottom: 1px solid #e9ecef;
    padding-bottom: 0.75rem;
}

#tabelaEvolucoes td,
#tabelaEvolucoes th {
    vertical-align: middle;
}

#tabelaEvolucoes .badge {
    color: #fff;
    font-weight: 500;
    padding: 0.4em 0.7em;
}

#modalEvolucao .form-label.small {
    margin-bottom: 0.15rem;
    color: #6c757d;
}

#modalEvolucao .card {
    border-color: #e9ecef;
    background-color: #f8f9fa;
}

#modalEvolucao h6.text-primary {
    margin-top: 0.5rem;
    margin-bottom: 0.5rem;
    font-weight: 600;
}

#btnToggleDetalhes {
    text-decoration: none;
}

#graficosDetalhados .card {
    border: 1px solid #e9ecef;
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add wwwroot/css/evolucao-clinica.css
git commit -m "feat(css): adicionar estilos da aba de evoluções clínicas"
```

---

### Task 27: Adicionar prompt opcional após `SalvarReceita` em `Form.cshtml`

**Files:**
- Modify: `Views/Home/Form.cshtml`

- [ ] **Step 1: Localizar o handler de sucesso após `/Home/SalvarReceita`**

No arquivo `Views/Home/Form.cshtml`, encontrar o trecho JavaScript onde a resposta de `/Home/SalvarReceita` é tratada (provavelmente algo como `if (data.success) { ... }` ou similar dentro de um `.then(...)` ou `await fetch(...)`).

- [ ] **Step 2: Adicionar o prompt depois do sucesso**

Substituir o redirecionamento simples (ex.: `window.location.href = '/Paciente/VerPaciente/' + pacienteId;`) pelo seguinte bloco:

```javascript
// Após confirmar success da SalvarReceita:
const pacienteRedirId = pacienteIdParaRedirect; // já definido no contexto do handler
const novaReceitaId = data.receitaId || data.id; // ajustar conforme a chave que o backend retorna

const wantEvol = await Swal.fire({
    icon: 'question',
    title: 'Registrar evolução clínica?',
    text: 'Você pode registrar a evolução agora ou depois, pela tela do paciente.',
    showCancelButton: true,
    confirmButtonText: 'Registrar agora',
    cancelButtonText: 'Depois',
    confirmButtonColor: '#0d6efd'
});

if (wantEvol.isConfirmed && novaReceitaId) {
    window.location.href = `/Paciente/VerPaciente/${pacienteRedirId}?abrirEvolucao=1&receitaId=${novaReceitaId}`;
} else {
    window.location.href = `/Paciente/VerPaciente/${pacienteRedirId}`;
}
```

> **Nota:** ajustar `pacienteIdParaRedirect` e `novaReceitaId` aos nomes reais das variáveis no contexto do JS de `Form.cshtml`. Se a resposta do backend não inclui `receitaId`, pode ser necessário ajustar `HomeController.SalvarReceita` para retornar o id da nova receita criada. Verificar e ajustar pontualmente.

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 4: Verificação manual**

```powershell
dotnet run
```

1. Logar, criar uma receita normal em Form.
2. Após `SalvarReceita`, o Swal "Registrar evolução clínica?" deve aparecer.
3. Clicar "Depois" → redireciona para VerPaciente normalmente.
4. Fazer outra receita, clicar "Registrar agora" → deve abrir VerPaciente já na aba Evoluções com o modal aberto e receita pré-selecionada.

Parar o app.

- [ ] **Step 5: Commit**

```powershell
git add Views/Home/Form.cshtml
git commit -m "feat(form): prompt opcional pós-SalvarReceita para registrar evolução clínica"
```

---

## Fase 10 — Documentação

### Task 28: Atualizar `CLAUDE.md`

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 1: Atualizar a tabela "Estado Atual do Sistema"**

Encontrar a tabela no final do arquivo `CLAUDE.md` (seção "Estado Atual do Sistema") e adicionar uma linha:

```markdown
| Registro de Evolução Clínica | ✅ Implementado |
```

- [ ] **Step 2: Adicionar entidade `EvolucaoClinica` na seção "Banco de Dados — Estrutura Completa"**

Após a seção `ReceitaCID`, adicionar:

```markdown
#### `EvolucaoClinica` → classe `EvolucaoClinica`
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | int PK | |
| `PacienteID` | int FK→PacienteICT | obrigatória, ON DELETE CASCADE |
| `UsuarioCriacaoID` | int FK→Usuarios | obrigatória |
| `ReceitaID` | int? FK→Receita | opcional, ON DELETE SET NULL |
| `CategoriaCID_ID` | int? FK→Categorias_CID | opcional, ON DELETE SET NULL |
| `DataConsulta` | datetime | data clínica do registro |
| `DataCriacao` | datetime | timestamp de gravação |
| `Subjetivo` | nvarchar(2000)? | SOAP — texto livre |
| `Avaliacao` | nvarchar(2000)? | SOAP — texto livre |
| `Plano` | nvarchar(2000)? | SOAP — texto livre |
| `ObservacaoObjetivo` | nvarchar(2000)? | extras do Objetivo |
| `Status` | int | StatusEvolucao enum (0-3) |
| `PaSistolica`, `PaDiastolica`, `FrequenciaCardiaca`, `Peso`, `Spo2` | numéricos? | sinais vitais |
| `Glicemia`, `Hba1c`, `Creatinina` | double? | laboratoriais |
| `EventosAdversos`, `Hospitalizacoes`, `IdasEmergencia` | int (default 0) | contadores de impacto |

Enum `StatusEvolucao`: `Inconclusivo = 0`, `Melhorou = 1`, `Estavel = 2`, `Piorou = 3`.
```

- [ ] **Step 3: Adicionar endpoints na seção "Endpoints Disponíveis"**

Adicionar nova subseção:

```markdown
### EvolucaoClinicaController
| Rota | Método | Descrição |
|---|---|---|
| `/EvolucaoClinica/ListarPorPaciente?pacienteId=` | GET [SessionFilter] | Lista evoluções do paciente |
| `/EvolucaoClinica/Detalhar?id=` | GET [SessionFilter] | Detalhes completos |
| `/EvolucaoClinica/Salvar` | POST [SessionFilter] | Cria nova evolução |
| `/EvolucaoClinica/Atualizar?id=` | POST [SessionFilter] | Edita evolução |
| `/EvolucaoClinica/Deletar?id=` | POST [SessionFilter] | Remove evolução |
| `/EvolucaoClinica/ListarCIDsDoPaciente?pacienteId=` | GET [SessionFilter] | CIDs vinculados às receitas |
| `/EvolucaoClinica/ListarReceitasDoPaciente?pacienteId=` | GET [SessionFilter] | Receitas do paciente para vínculo |
| `/EvolucaoClinica/SerieTemporal?pacienteId=` | GET [SessionFilter] | Dados para gráficos Chart.js |
```

- [ ] **Step 4: Adicionar nota sobre EF Migrations**

Após a seção "Arquitetura", adicionar nota:

```markdown
> 📌 **Schema management:** A partir de 2026-05-30 o projeto usa **EF Core Migrations**, iniciando com `InitialBaseline` (vazia, captura o estado herdado do scaffolding) e seguindo com migrations incrementais. Após `git pull`, sempre rodar `dotnet ef database update`.
```

- [ ] **Step 5: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 6: Commit**

```powershell
git add CLAUDE.md
git commit -m "docs(claude): atualizar CLAUDE.md com EvolucaoClinica, endpoints e nota sobre migrations"
```

---

### Task 29: Atualizar `PROXIMOS_PASSOS.md`

**Files:**
- Modify: `PROXIMOS_PASSOS.md`

- [ ] **Step 1: Adicionar RF16 na tabela "Visão Geral"**

Localizar a tabela inicial em `PROXIMOS_PASSOS.md` e adicionar a linha após RF15:

```markdown
| RF16 | **Registro e Acompanhamento de Evolução Clínica** | ✅ Implementado |
```

- [ ] **Step 2: Atualizar "Última revisão"**

Trocar `**Última revisão:** 24/05/2026` por `**Última revisão:** 2026-05-30`.

- [ ] **Step 3: Reordenar a tabela "Ordem de Prioridade Sugerida"**

Localizar a tabela final "Ordem de Prioridade Sugerida" e atualizar a justificativa do RF13 para refletir que agora há dados ricos de desfecho clínico:

```markdown
| 6 | RF13 — Relatórios Analíticos | Agora com `EvolucaoClinica` é possível incluir correlações ICT × Status, hospitalizações por CID, eventos adversos por faixa de complexidade |
```

- [ ] **Step 4: Adicionar bloco de descrição do RF16 implementado**

Logo após a tabela de visão geral (antes do bloco "Bug identificado"), adicionar:

```markdown
---

## RF16 — Registro e Acompanhamento de Evolução Clínica (✅ Implementado em 2026-05-30)

**Origem:** decorrência direta do objetivo do TCC — *"correlacionar complexidade com adesão e evolução clínica"*. Os RFs anteriores cobriam complexidade (RF05) e adesão (RF14), mas faltava o desfecho clínico.

**Implementação:**
- Nova entidade `EvolucaoClinica` com estrutura SOAP híbrida (texto livre + indicadores objetivos estruturados + StatusEvolucao).
- Aba "Evoluções Clínicas" em `VerPaciente.cshtml` com tabela cronológica + gráfico Chart.js de ICT × Status + gráficos detalhados por indicador (lazy load).
- Fluxo de criação: botão dedicado em VerPaciente + prompt opcional após `SalvarReceita` em `Form.cshtml`.
- CRUD livre. Isolamento por usuário garantido via `ResolverContexto` no service.

**Spec completa:** [docs/superpowers/specs/2026-05-30-evolucao-clinica-design.md](docs/superpowers/specs/2026-05-30-evolucao-clinica-design.md)
**Plano de implementação:** [docs/superpowers/plans/2026-05-30-evolucao-clinica.md](docs/superpowers/plans/2026-05-30-evolucao-clinica.md)
```

- [ ] **Step 5: Build**

```powershell
dotnet build
```

Expected: `Build succeeded`.

- [ ] **Step 6: Commit final**

```powershell
git add PROXIMOS_PASSOS.md
git commit -m "docs(roadmap): RF16 implementado — Evolução Clínica"
```

---

## Critérios de aceitação (verificação final)

Após executar todas as tasks, validar manualmente os 10 critérios definidos na seção 12 do spec:

1. ✅ Criar, listar, editar e deletar evoluções funciona para o usuário logado.
2. ✅ Modal SOAP aceita preenchimento parcial.
3. ✅ Tentar acessar `/EvolucaoClinica/Detalhar?id=X` onde X pertence a outro usuário retorna `{ success: false, message: "..." }`.
4. ✅ Lazy load da aba — gráficos só aparecem ao clicar.
5. ✅ Gráfico principal mostra pontos coloridos por Status; linha quebra onde `ReceitaID == null`.
6. ✅ Gráficos detalhados só renderizam ao expandir o collapse.
7. ✅ Prompt aparece após `SalvarReceita`; redirecionamento + pré-preenchimento funciona.
8. ✅ Deletar paciente → cascade nas evoluções; deletar receita → `ReceitaID = NULL` na evolução.
9. ✅ Tabela `EvolucaoClinica` criada com schema correto via migration.
10. ✅ `CLAUDE.md` e `PROXIMOS_PASSOS.md` refletem o novo estado.
