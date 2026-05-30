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
