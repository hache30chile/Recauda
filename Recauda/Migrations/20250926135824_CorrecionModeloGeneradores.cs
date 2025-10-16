using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recauda.Migrations
{
    /// <inheritdoc />
    public partial class CorrecionModeloGeneradores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Generadores_Usuario_Compania",
                table: "Generadores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Usuario_Compania",
                table: "Generadores",
                columns: new[] { "usu_id", "com_id" },
                unique: true);
        }
    }
}
