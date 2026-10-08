using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feriando.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OcultarConversacionesPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversacionesOcultas",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaOcultacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversacionesOcultas", x => new { x.UsuarioID, x.ChatId });
                    table.ForeignKey(
                        name: "FK_ConversacionesOcultas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversacionesOcultas");
        }
    }
}
