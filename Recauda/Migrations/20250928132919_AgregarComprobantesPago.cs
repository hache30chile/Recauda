using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recauda.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComprobantesPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComprobantesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pag_id = table.Column<int>(type: "int", nullable: false),
                    comp_ruta_archivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    comp_nombre_original = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    comp_extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    comp_tamaño_kb = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    comp_tipo_comprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    comp_descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    comp_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesPago_Pagos_pag_id",
                        column: x => x.pag_id,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_Activo",
                table: "ComprobantesPago",
                column: "comp_activo");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_Activo_FechaRegistro",
                table: "ComprobantesPago",
                columns: new[] { "comp_activo", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_FechaRegistro",
                table: "ComprobantesPago",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_Pago",
                table: "ComprobantesPago",
                column: "pag_id");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_Pago_Activo",
                table: "ComprobantesPago",
                columns: new[] { "pag_id", "comp_activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_Pago_TipoComprobante",
                table: "ComprobantesPago",
                columns: new[] { "pag_id", "comp_tipo_comprobante" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_TipoComprobante",
                table: "ComprobantesPago",
                column: "comp_tipo_comprobante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantesPago");
        }
    }
}
