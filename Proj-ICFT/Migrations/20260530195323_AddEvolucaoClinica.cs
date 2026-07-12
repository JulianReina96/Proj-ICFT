using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proj_ICFT.Migrations
{
    /// <inheritdoc />
    public partial class AddEvolucaoClinica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvolucaoClinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteID = table.Column<int>(type: "int", nullable: false),
                    UsuarioCriacaoID = table.Column<int>(type: "int", nullable: false),
                    ReceitaID = table.Column<int>(type: "int", nullable: true),
                    CategoriaCID_ID = table.Column<int>(type: "int", nullable: true),
                    DataConsulta = table.Column<DateTime>(type: "datetime", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime", nullable: false),
                    Subjetivo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Avaliacao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Plano = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ObservacaoObjetivo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaSistolica = table.Column<int>(type: "int", nullable: true),
                    PaDiastolica = table.Column<int>(type: "int", nullable: true),
                    FrequenciaCardiaca = table.Column<int>(type: "int", nullable: true),
                    Peso = table.Column<double>(type: "float", nullable: true),
                    Spo2 = table.Column<int>(type: "int", nullable: true),
                    Glicemia = table.Column<double>(type: "float", nullable: true),
                    Hba1c = table.Column<double>(type: "float", nullable: true),
                    Creatinina = table.Column<double>(type: "float", nullable: true),
                    EventosAdversos = table.Column<int>(type: "int", nullable: false),
                    Hospitalizacoes = table.Column<int>(type: "int", nullable: false),
                    IdasEmergencia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvolucaoClinica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvolucaoClinica_Categorias_CID",
                        column: x => x.CategoriaCID_ID,
                        principalTable: "Categorias_CID",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EvolucaoClinica_PacienteICT",
                        column: x => x.PacienteID,
                        principalTable: "PacienteICT",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvolucaoClinica_Receita",
                        column: x => x.ReceitaID,
                        principalTable: "Receita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EvolucaoClinica_Usuario",
                        column: x => x.UsuarioCriacaoID,
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvolucaoClinica_CategoriaCID_ID",
                table: "EvolucaoClinica",
                column: "CategoriaCID_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EvolucaoClinica_Paciente_Data",
                table: "EvolucaoClinica",
                columns: new[] { "PacienteID", "DataConsulta" });

            migrationBuilder.CreateIndex(
                name: "IX_EvolucaoClinica_PacienteID",
                table: "EvolucaoClinica",
                column: "PacienteID");

            migrationBuilder.CreateIndex(
                name: "IX_EvolucaoClinica_ReceitaID",
                table: "EvolucaoClinica",
                column: "ReceitaID");

            migrationBuilder.CreateIndex(
                name: "IX_EvolucaoClinica_UsuarioCriacaoID",
                table: "EvolucaoClinica",
                column: "UsuarioCriacaoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvolucaoClinica");
        }
    }
}
