using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Energy_Monitoring_System.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leituras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedidorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tensao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Corrente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PotenciaAtiva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PotenciaReativa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnergiaAtivaDireta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnergiaAtivaReversa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FatorPotencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leituras", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_MedidorId_Timestamp",
                table: "Leituras",
                columns: new[] { "MedidorId", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leituras");
        }
    }
}
