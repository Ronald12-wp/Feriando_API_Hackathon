using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feriando.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMensajesChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.MensajesChat', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MensajesChat]
                    (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [ChatId] nvarchar(100) NOT NULL,
                        [EmisorId] int NOT NULL,
                        [Mensaje] nvarchar(max) NOT NULL,
                        [FechaEnvio] datetime2 NOT NULL,
                        [Leido] bit NOT NULL,
                        CONSTRAINT [PK_MensajesChat] PRIMARY KEY ([Id])
                    );
                END
                ELSE IF
                    NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'Id' AND system_type_id = 56
                          AND is_nullable = 0 AND is_identity = 1
                    )
                    OR NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'ChatId' AND system_type_id = 231
                          AND max_length = 200 AND is_nullable = 0
                    )
                    OR NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'EmisorId' AND system_type_id = 56
                          AND is_nullable = 0
                    )
                    OR NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'Mensaje' AND system_type_id = 231
                          AND max_length = -1 AND is_nullable = 0
                    )
                    OR NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'FechaEnvio' AND system_type_id = 42
                          AND is_nullable = 0
                    )
                    OR NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.MensajesChat')
                          AND name = N'Leido' AND system_type_id = 104
                          AND is_nullable = 0
                    )
                BEGIN
                    ;THROW 51000, N'La tabla dbo.MensajesChat existe, pero su esquema no coincide con la migración. No se modificó la tabla.', 1;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La tabla puede contener mensajes previos a la migración; conservarlos al revertir el historial.
        }
    }
}
