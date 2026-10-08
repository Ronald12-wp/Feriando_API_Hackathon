-- Esquema generado desde las migraciones de Entity Framework Core.
-- Crear la base si aún no existe; las migraciones controlan el esquema y su historial.
USE [master];
GO
IF DB_ID(N'ElTruequeDB') IS NULL
BEGIN
    CREATE DATABASE [ElTruequeDB];
END
GO
USE [ElTruequeDB];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Categorias] (
        [CategoriaID] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [Icono] nvarchar(200) NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([CategoriaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Departamentos] (
        [DepartamentoID] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Departamentos] PRIMARY KEY ([DepartamentoID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Idiomas] (
        [IdiomaID] int NOT NULL IDENTITY,
        [Codigo] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(60) NOT NULL,
        CONSTRAINT [PK_Idiomas] PRIMARY KEY ([IdiomaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [UnidadesMedida] (
        [UnidadMedidaID] int NOT NULL IDENTITY,
        [Nombre] nvarchar(30) NOT NULL,
        [Abreviatura] nvarchar(10) NULL,
        CONSTRAINT [PK_UnidadesMedida] PRIMARY KEY ([UnidadMedidaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Municipios] (
        [MunicipioID] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [DepartamentoID] int NOT NULL,
        CONSTRAINT [PK_Municipios] PRIMARY KEY ([MunicipioID]),
        CONSTRAINT [FK_Municipios_Departamentos_DepartamentoID] FOREIGN KEY ([DepartamentoID]) REFERENCES [Departamentos] ([DepartamentoID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [UsuarioID] int NOT NULL IDENTITY,
        [Nombres] nvarchar(100) NOT NULL,
        [Apellidos] nvarchar(100) NOT NULL,
        [Telefono] nvarchar(20) NOT NULL,
        [Correo] nvarchar(150) NULL,
        [PasswordHash] varbinary(max) NOT NULL,
        [Genero] nvarchar(1) NULL,
        [FechaNacimiento] datetime2 NULL,
        [MunicipioID] int NOT NULL,
        [DireccionExacta] nvarchar(300) NOT NULL,
        [IdiomaPreferidoID] int NULL,
        [EsProductora] bit NOT NULL,
        [FotoPerfil] nvarchar(300) NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [EstadoActivo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([UsuarioID]),
        CONSTRAINT [FK_Usuarios_Idiomas_IdiomaPreferidoID] FOREIGN KEY ([IdiomaPreferidoID]) REFERENCES [Idiomas] ([IdiomaID]),
        CONSTRAINT [FK_Usuarios_Municipios_MunicipioID] FOREIGN KEY ([MunicipioID]) REFERENCES [Municipios] ([MunicipioID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Productos] (
        [ProductoID] int NOT NULL IDENTITY,
        [UsuarioID] int NOT NULL,
        [CategoriaID] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [Cantidad] decimal(10,2) NOT NULL,
        [UnidadMedidaID] int NOT NULL,
        [TipoOferta] nvarchar(20) NOT NULL,
        [PrecioReferencial] decimal(10,2) NULL,
        [Estado] nvarchar(20) NOT NULL DEFAULT N'Disponible',
        [FechaPublicacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([ProductoID]),
        CONSTRAINT [FK_Productos_Categorias_CategoriaID] FOREIGN KEY ([CategoriaID]) REFERENCES [Categorias] ([CategoriaID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Productos_UnidadesMedida_UnidadMedidaID] FOREIGN KEY ([UnidadMedidaID]) REFERENCES [UnidadesMedida] ([UnidadMedidaID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Productos_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [ImagenesProducto] (
        [ImagenID] int NOT NULL IDENTITY,
        [ProductoID] int NOT NULL,
        [UrlImagen] nvarchar(300) NOT NULL,
        [Orden] tinyint NOT NULL,
        CONSTRAINT [PK_ImagenesProducto] PRIMARY KEY ([ImagenID]),
        CONSTRAINT [FK_ImagenesProducto_Productos_ProductoID] FOREIGN KEY ([ProductoID]) REFERENCES [Productos] ([ProductoID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Trueques] (
        [TruequeID] int NOT NULL IDENTITY,
        [ProductoOfertadoID] int NOT NULL,
        [ProductoSolicitadoID] int NULL,
        [UsuarioSolicitanteID] int NOT NULL,
        [UsuarioReceptorID] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL DEFAULT N'Pendiente',
        [MontoAdicional] decimal(10,2) NULL,
        [FechaSolicitud] datetime2 NOT NULL,
        [FechaRespuesta] datetime2 NULL,
        [FechaCompletado] datetime2 NULL,
        [LugarEncuentro] nvarchar(200) NULL,
        CONSTRAINT [PK_Trueques] PRIMARY KEY ([TruequeID]),
        CONSTRAINT [FK_Trueques_Productos_ProductoOfertadoID] FOREIGN KEY ([ProductoOfertadoID]) REFERENCES [Productos] ([ProductoID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Trueques_Productos_ProductoSolicitadoID] FOREIGN KEY ([ProductoSolicitadoID]) REFERENCES [Productos] ([ProductoID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Trueques_Usuarios_UsuarioReceptorID] FOREIGN KEY ([UsuarioReceptorID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Trueques_Usuarios_UsuarioSolicitanteID] FOREIGN KEY ([UsuarioSolicitanteID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Notificaciones] (
        [NotificacionID] int NOT NULL IDENTITY,
        [UsuarioID] int NOT NULL,
        [Titulo] nvarchar(100) NOT NULL,
        [Mensaje] nvarchar(300) NOT NULL,
        [TruequeID] int NULL,
        [Leido] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([NotificacionID]),
        CONSTRAINT [FK_Notificaciones_Trueques_TruequeID] FOREIGN KEY ([TruequeID]) REFERENCES [Trueques] ([TruequeID]),
        CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE TABLE [Valoraciones] (
        [ValoracionID] int NOT NULL IDENTITY,
        [TruequeID] int NOT NULL,
        [UsuarioEvaluadorID] int NOT NULL,
        [UsuarioEvaluadoID] int NOT NULL,
        [Puntuacion] tinyint NOT NULL,
        [Comentario] nvarchar(300) NULL,
        [FechaValoracion] datetime2 NOT NULL,
        CONSTRAINT [PK_Valoraciones] PRIMARY KEY ([ValoracionID]),
        CONSTRAINT [FK_Valoraciones_Trueques_TruequeID] FOREIGN KEY ([TruequeID]) REFERENCES [Trueques] ([TruequeID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Valoraciones_Usuarios_UsuarioEvaluadoID] FOREIGN KEY ([UsuarioEvaluadoID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Valoraciones_Usuarios_UsuarioEvaluadorID] FOREIGN KEY ([UsuarioEvaluadorID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_ImagenesProducto_ProductoID] ON [ImagenesProducto] ([ProductoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Municipios_DepartamentoID] ON [Municipios] ([DepartamentoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_TruequeID] ON [Notificaciones] ([TruequeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_UsuarioID] ON [Notificaciones] ([UsuarioID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaID] ON [Productos] ([CategoriaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Productos_UnidadMedidaID] ON [Productos] ([UnidadMedidaID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Productos_UsuarioID] ON [Productos] ([UsuarioID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Trueques_ProductoOfertadoID] ON [Trueques] ([ProductoOfertadoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Trueques_ProductoSolicitadoID] ON [Trueques] ([ProductoSolicitadoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Trueques_UsuarioReceptorID] ON [Trueques] ([UsuarioReceptorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Trueques_UsuarioSolicitanteID] ON [Trueques] ([UsuarioSolicitanteID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]) WHERE [Correo] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Usuarios_IdiomaPreferidoID] ON [Usuarios] ([IdiomaPreferidoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Usuarios_MunicipioID] ON [Usuarios] ([MunicipioID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Telefono] ON [Usuarios] ([Telefono]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Valoraciones_TruequeID] ON [Valoraciones] ([TruequeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Valoraciones_UsuarioEvaluadoID] ON [Valoraciones] ([UsuarioEvaluadoID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    CREATE INDEX [IX_Valoraciones_UsuarioEvaluadorID] ON [Valoraciones] ([UsuarioEvaluadorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929163658_AgregarTablaMensajesChat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929163658_AgregarTablaMensajesChat', N'10.0.11');
END;

COMMIT;
GO


BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003074549_AgregarMensajesChat'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003074549_AgregarMensajesChat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003074549_AgregarMensajesChat', N'10.0.11');
END;

COMMIT;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007055347_GestionarEstadosProductoYTrueque'
)
BEGIN
    UPDATE [t]
    SET [Estado] = N'Aceptado', [FechaCompletado] = NULL
    FROM [Trueques] AS [t]
    WHERE [t].[Estado] = N'Completado'
      AND (SELECT COUNT(*) FROM [Valoraciones] AS [v] WHERE [v].[TruequeID] = [t].[TruequeID]) < 2;

    UPDATE [t]
    SET [Estado] = N'Completado', [FechaCompletado] = COALESCE([t].[FechaCompletado], SYSUTCDATETIME())
    FROM [Trueques] AS [t]
    WHERE [t].[Estado] = N'Aceptado'
      AND (SELECT COUNT(*) FROM [Valoraciones] AS [v] WHERE [v].[TruequeID] = [t].[TruequeID]) >= 2;

    UPDATE [p]
    SET [Estado] = N'Intercambiado'
    FROM [Productos] AS [p]
    WHERE EXISTS (
        SELECT 1 FROM [Trueques] AS [t]
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

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Valoraciones') AND name = N'IX_Valoraciones_TruequeID')
        DROP INDEX [IX_Valoraciones_TruequeID] ON [Valoraciones];
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Valoraciones') AND name = N'IX_Valoraciones_TruequeID_UsuarioEvaluadorID')
        CREATE UNIQUE INDEX [IX_Valoraciones_TruequeID_UsuarioEvaluadorID]
            ON [Valoraciones] ([TruequeID], [UsuarioEvaluadorID]);

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Productos_Estado' AND parent_object_id = OBJECT_ID(N'dbo.Productos'))
        ALTER TABLE [Productos] ADD CONSTRAINT [CK_Productos_Estado]
            CHECK ([Estado] IN (N'Disponible', N'Reservado', N'Intercambiado', N'Inactivo'));
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Trueques_Estado' AND parent_object_id = OBJECT_ID(N'dbo.Trueques'))
        ALTER TABLE [Trueques] ADD CONSTRAINT [CK_Trueques_Estado]
            CHECK ([Estado] IN (N'Pendiente', N'Aceptado', N'Rechazado', N'Completado', N'Cancelado'));

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007055347_GestionarEstadosProductoYTrueque', N'10.0.11');
END;
COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007064438_OcultarConversacionesPorUsuario'
)
BEGIN
    IF OBJECT_ID(N'dbo.ConversacionesOcultas', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[ConversacionesOcultas]
        (
            [UsuarioID] int NOT NULL,
            [ChatId] nvarchar(100) NOT NULL,
            [FechaOcultacion] datetime2 NOT NULL,
            CONSTRAINT [PK_ConversacionesOcultas] PRIMARY KEY ([UsuarioID], [ChatId]),
            CONSTRAINT [FK_ConversacionesOcultas_Usuarios_UsuarioID]
                FOREIGN KEY ([UsuarioID]) REFERENCES [dbo].[Usuarios] ([UsuarioID]) ON DELETE CASCADE
        );
    END
    ELSE IF
        COL_LENGTH(N'dbo.ConversacionesOcultas', N'UsuarioID') IS NULL OR
        COL_LENGTH(N'dbo.ConversacionesOcultas', N'ChatId') IS NULL OR
        COL_LENGTH(N'dbo.ConversacionesOcultas', N'FechaOcultacion') IS NULL
    BEGIN
        ;THROW 51004, N'La tabla dbo.ConversacionesOcultas existe, pero su esquema no coincide con la migración.', 1;
    END;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007064438_OcultarConversacionesPorUsuario', N'10.0.11');
END;
COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007074413_ReemplazarFechaNacimientoPorCedula'
)
BEGIN
    IF COL_LENGTH(N'dbo.Usuarios', N'FechaNacimiento') IS NOT NULL
        ALTER TABLE [dbo].[Usuarios] DROP COLUMN [FechaNacimiento];

    IF COL_LENGTH(N'dbo.Usuarios', N'Cedula') IS NULL
        ALTER TABLE [dbo].[Usuarios] ADD [Cedula] nvarchar(20) NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Usuarios')
          AND name = N'IX_Usuarios_Cedula'
    )
        EXEC(N'CREATE UNIQUE INDEX [IX_Usuarios_Cedula]
            ON [dbo].[Usuarios] ([Cedula])
            WHERE [Cedula] IS NOT NULL;');

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007074413_ReemplazarFechaNacimientoPorCedula', N'10.0.11');
END;
COMMIT;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007075108_AgregarDepartamentoDirectoAUsuario'
)
BEGIN
    IF COL_LENGTH(N'dbo.Usuarios', N'DepartamentoID') IS NULL
        ALTER TABLE [dbo].[Usuarios] ADD [DepartamentoID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007075108_AgregarDepartamentoDirectoAUsuario'
)
BEGIN
    EXEC(N'UPDATE [u]
        SET [DepartamentoID] = [m].[DepartamentoID]
        FROM [dbo].[Usuarios] AS [u]
        INNER JOIN [dbo].[Municipios] AS [m] ON [m].[MunicipioID] = [u].[MunicipioID];');

    IF EXISTS (SELECT 1 FROM [dbo].[Usuarios] WHERE [DepartamentoID] IS NULL)
        THROW 51005, N'No se pudo determinar el departamento de todos los usuarios desde su municipio.', 1;

    ALTER TABLE [dbo].[Usuarios] ALTER COLUMN [DepartamentoID] int NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Usuarios')
          AND name = N'IX_Usuarios_DepartamentoID'
    )
        CREATE INDEX [IX_Usuarios_DepartamentoID] ON [dbo].[Usuarios] ([DepartamentoID]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.Usuarios')
          AND name = N'FK_Usuarios_Departamentos_DepartamentoID'
    )
        ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [FK_Usuarios_Departamentos_DepartamentoID]
            FOREIGN KEY ([DepartamentoID]) REFERENCES [dbo].[Departamentos] ([DepartamentoID]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007075108_AgregarDepartamentoDirectoAUsuario', N'10.0.11');
END;
COMMIT;
GO

