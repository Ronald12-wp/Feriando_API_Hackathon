using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feriando.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class GestionarEstadosProductoYTrueque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- Registros completados por el comportamiento anterior solo con una valoración
                -- vuelven a Aceptado; su publicación permanece Reservada hasta la segunda valoración.
                UPDATE [t]
                SET [Estado] = N'Aceptado', [FechaCompletado] = NULL
                FROM [Trueques] AS [t]
                WHERE [t].[Estado] = N'Completado'
                  AND (SELECT COUNT(*) FROM [Valoraciones] AS [v] WHERE [v].[TruequeID] = [t].[TruequeID]) < 2;

                -- Completa los trueques aceptados que ya tienen las dos valoraciones.
                UPDATE [t]
                SET [Estado] = N'Completado', [FechaCompletado] = COALESCE([t].[FechaCompletado], SYSUTCDATETIME())
                FROM [Trueques] AS [t]
                WHERE [t].[Estado] = N'Aceptado'
                  AND (SELECT COUNT(*) FROM [Valoraciones] AS [v] WHERE [v].[TruequeID] = [t].[TruequeID]) >= 2;

                UPDATE [p]
                SET [Estado] = N'Intercambiado'
                FROM [Productos] AS [p]
                WHERE EXISTS (
                    SELECT 1
                    FROM [Trueques] AS [t]
                    WHERE [t].[Estado] = N'Completado'
                      AND ([t].[ProductoOfertadoID] = [p].[ProductoID]
                           OR [t].[ProductoSolicitadoID] = [p].[ProductoID])
                );

                IF EXISTS (SELECT 1 FROM [Productos] WHERE [Estado] NOT IN (N'Disponible', N'Reservado', N'Intercambiado', N'Inactivo'))
                    THROW 51001, N'Hay estados de producto no reconocidos. Corrige esos datos antes de aplicar la migración.', 1;

                IF EXISTS (SELECT 1 FROM [Trueques] WHERE [Estado] NOT IN (N'Pendiente', N'Aceptado', N'Rechazado', N'Completado', N'Cancelado'))
                    THROW 51002, N'Hay estados de trueque no reconocidos. Corrige esos datos antes de aplicar la migración.', 1;

                IF EXISTS (
                    SELECT 1 FROM [Valoraciones]
                    GROUP BY [TruequeID], [UsuarioEvaluadorID]
                    HAVING COUNT(*) > 1
                )
                    THROW 51003, N'Hay valoraciones duplicadas por participante. Corrige esos datos antes de aplicar la migración.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Valoraciones_TruequeID",
                table: "Valoraciones");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_TruequeID_UsuarioEvaluadorID",
                table: "Valoraciones",
                columns: new[] { "TruequeID", "UsuarioEvaluadorID" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trueques_Estado",
                table: "Trueques",
                sql: "[Estado] IN (N'Pendiente', N'Aceptado', N'Rechazado', N'Completado', N'Cancelado')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Estado",
                table: "Productos",
                sql: "[Estado] IN (N'Disponible', N'Reservado', N'Intercambiado', N'Inactivo')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Valoraciones_TruequeID_UsuarioEvaluadorID",
                table: "Valoraciones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Trueques_Estado",
                table: "Trueques");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Estado",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_TruequeID",
                table: "Valoraciones",
                column: "TruequeID");
        }
    }
}
