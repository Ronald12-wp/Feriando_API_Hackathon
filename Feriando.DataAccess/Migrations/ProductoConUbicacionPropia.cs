using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feriando.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ProductoConUbicacionPropia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionExacta",
                table: "Productos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MunicipioID",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_MunicipioID",
                table: "Productos",
                column: "MunicipioID");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Municipios_MunicipioID",
                table: "Productos",
                column: "MunicipioID",
                principalTable: "Municipios",
                principalColumn: "MunicipioID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                UPDATE p
                SET p.MunicipioID = u.MunicipioID,
                    p.DireccionExacta = u.DireccionExacta
                FROM Productos AS p
                INNER JOIN Usuarios AS u ON u.UsuarioID = p.UsuarioID
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Municipios_MunicipioID",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_MunicipioID",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DireccionExacta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MunicipioID",
                table: "Productos");
        }
    }
}
