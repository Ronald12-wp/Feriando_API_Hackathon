using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feriando.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDepartamentoDirectoAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartamentoID",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [u]
                SET [DepartamentoID] = [m].[DepartamentoID]
                FROM [Usuarios] AS [u]
                INNER JOIN [Municipios] AS [m] ON [m].[MunicipioID] = [u].[MunicipioID];
                """);

            migrationBuilder.AlterColumn<int>(
                name: "DepartamentoID",
                table: "Usuarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DepartamentoID",
                table: "Usuarios",
                column: "DepartamentoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Departamentos_DepartamentoID",
                table: "Usuarios",
                column: "DepartamentoID",
                principalTable: "Departamentos",
                principalColumn: "DepartamentoID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Departamentos_DepartamentoID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_DepartamentoID",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DepartamentoID",
                table: "Usuarios");
        }
    }
}
