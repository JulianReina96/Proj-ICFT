using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proj_ICFT.Migrations
{
    /// <inheritdoc />
    public partial class RenomearReceitaParaPrescricao : Migration
    {
        // Renomeação de nomenclatura "Receita" → "Prescrição".
        // Usa sp_rename (via Rename*) para PRESERVAR os dados — a geração automática
        // do EF produziria Drop/Create (perda de dados) por conta da troca de tipos.
        //
        // Os renomes de índice e da PK são GUARDADOS com IF EXISTS: o banco herdado
        // do scaffolding não possui os índices de FK que o snapshot do EF presume
        // (apenas PKs e alguns índices de EvolucaoClinica). Os guards evitam o erro
        // 15248 ("@objname ambíguo / @objtype errado") e mantêm a migration aplicável
        // em qualquer ambiente, independentemente de quais índices existam.

        private static string RenameIndexIfExists(string table, string oldName, string newName) =>
            $@"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{oldName}' AND object_id = OBJECT_ID(N'{table}'))
    EXEC sp_rename N'{table}.{oldName}', N'{newName}', N'INDEX';";

        private static string RenamePkIfExists(string oldName, string newName) =>
            $@"IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'{oldName}')
    EXEC sp_rename N'{oldName}', N'{newName}';";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Tabelas ───────────────────────────────────────────────────────────
            migrationBuilder.RenameTable(name: "Receita",    newName: "Prescricao");
            migrationBuilder.RenameTable(name: "ReceitaMed", newName: "PrescricaoMed");
            migrationBuilder.RenameTable(name: "ReceitaCID", newName: "PrescricaoCID");

            // ── Colunas FK que referenciavam Receita ──────────────────────────────
            migrationBuilder.RenameColumn(name: "ReceitaID",     table: "PrescricaoMed",   newName: "PrescricaoID");
            migrationBuilder.RenameColumn(name: "ReceitaID",     table: "PrescricaoCID",   newName: "PrescricaoID");
            migrationBuilder.RenameColumn(name: "ReceitaID",     table: "EvolucaoClinica", newName: "PrescricaoID");
            migrationBuilder.RenameColumn(name: "Med_ReceitaID", table: "InstrucoesMed",   newName: "Med_PrescricaoID");

            // ── PK convencional da antiga tabela Receita (guardada) ───────────────
            migrationBuilder.Sql(RenamePkIfExists("PK_Receita", "PK_Prescricao"));

            // ── Índices (guardados — alinham com a convenção do snapshot) ─────────
            migrationBuilder.Sql(RenameIndexIfExists("Prescricao",      "IX_Receita_PacienteID",          "IX_Prescricao_PacienteID"));
            migrationBuilder.Sql(RenameIndexIfExists("Prescricao",      "IX_Receita_UsuarioCriacaoID",    "IX_Prescricao_UsuarioCriacaoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_ReceitaMed_CategoriaID",      "IX_PrescricaoMed_CategoriaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_ReceitaMed_FrequenciaID",     "IX_PrescricaoMed_FrequenciaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_ReceitaMed_MedicamentoID",    "IX_PrescricaoMed_MedicamentoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_ReceitaMed_ReceitaID",        "IX_PrescricaoMed_PrescricaoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_ReceitaMed_TipoID",           "IX_PrescricaoMed_TipoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoCID",   "IX_ReceitaCID_CategoriaCID_ID",  "IX_PrescricaoCID_CategoriaCID_ID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoCID",   "IX_ReceitaCID_ReceitaID",        "IX_PrescricaoCID_PrescricaoID"));
            migrationBuilder.Sql(RenameIndexIfExists("EvolucaoClinica", "IX_EvolucaoClinica_ReceitaID",   "IX_EvolucaoClinica_PrescricaoID"));
            migrationBuilder.Sql(RenameIndexIfExists("InstrucoesMed",   "IX_InstrucoesMed_Med_ReceitaID", "IX_InstrucoesMed_Med_PrescricaoID"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── Índices (guardados) ───────────────────────────────────────────────
            migrationBuilder.Sql(RenameIndexIfExists("InstrucoesMed",   "IX_InstrucoesMed_Med_PrescricaoID", "IX_InstrucoesMed_Med_ReceitaID"));
            migrationBuilder.Sql(RenameIndexIfExists("EvolucaoClinica", "IX_EvolucaoClinica_PrescricaoID",   "IX_EvolucaoClinica_ReceitaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoCID",   "IX_PrescricaoCID_PrescricaoID",     "IX_ReceitaCID_ReceitaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoCID",   "IX_PrescricaoCID_CategoriaCID_ID",  "IX_ReceitaCID_CategoriaCID_ID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_PrescricaoMed_TipoID",           "IX_ReceitaMed_TipoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_PrescricaoMed_PrescricaoID",     "IX_ReceitaMed_ReceitaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_PrescricaoMed_MedicamentoID",    "IX_ReceitaMed_MedicamentoID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_PrescricaoMed_FrequenciaID",     "IX_ReceitaMed_FrequenciaID"));
            migrationBuilder.Sql(RenameIndexIfExists("PrescricaoMed",   "IX_PrescricaoMed_CategoriaID",      "IX_ReceitaMed_CategoriaID"));
            migrationBuilder.Sql(RenameIndexIfExists("Prescricao",      "IX_Prescricao_UsuarioCriacaoID",    "IX_Receita_UsuarioCriacaoID"));
            migrationBuilder.Sql(RenameIndexIfExists("Prescricao",      "IX_Prescricao_PacienteID",          "IX_Receita_PacienteID"));

            // ── PK convencional (guardada) ────────────────────────────────────────
            migrationBuilder.Sql(RenamePkIfExists("PK_Prescricao", "PK_Receita"));

            // ── Colunas FK ────────────────────────────────────────────────────────
            migrationBuilder.RenameColumn(name: "Med_PrescricaoID", table: "InstrucoesMed",   newName: "Med_ReceitaID");
            migrationBuilder.RenameColumn(name: "PrescricaoID",     table: "EvolucaoClinica", newName: "ReceitaID");
            migrationBuilder.RenameColumn(name: "PrescricaoID",     table: "PrescricaoCID",   newName: "ReceitaID");
            migrationBuilder.RenameColumn(name: "PrescricaoID",     table: "PrescricaoMed",   newName: "ReceitaID");

            // ── Tabelas ───────────────────────────────────────────────────────────
            migrationBuilder.RenameTable(name: "PrescricaoCID", newName: "ReceitaCID");
            migrationBuilder.RenameTable(name: "PrescricaoMed", newName: "ReceitaMed");
            migrationBuilder.RenameTable(name: "Prescricao",    newName: "Receita");
        }
    }
}
