USE [ElTruequeDB];
GO

-- 1. Base actual e historial de migraciones registradas.
SELECT DB_NAME() AS BaseDeDatosActual;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
BEGIN
    SELECT MigrationId, ProductVersion
    FROM dbo.__EFMigrationsHistory
    ORDER BY MigrationId;
END
ELSE
BEGIN
    SELECT N'No existe dbo.__EFMigrationsHistory' AS EstadoHistorial;
END;
GO

-- 2. Columnas, tipos, nulabilidad, valores predeterminados e identidad.
SELECT
    esquema.name AS Esquema,
    tabla.name AS Tabla,
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS TipoDato,
    CASE
        WHEN columna.max_length = -1 THEN N'MAX'
        WHEN tipo.name IN (N'nchar', N'nvarchar') THEN CONVERT(nvarchar(20), columna.max_length / 2)
        WHEN tipo.name IN (N'char', N'varchar', N'binary', N'varbinary') THEN CONVERT(nvarchar(20), columna.max_length)
        ELSE NULL
    END AS Longitud,
    columna.precision AS Precision,
    columna.scale AS Escala,
    columna.is_nullable AS AceptaNull,
    columna.is_identity AS EsIdentity,
    dc.definition AS ValorPredeterminado
FROM sys.tables AS tabla
JOIN sys.schemas AS esquema ON esquema.schema_id = tabla.schema_id
JOIN sys.columns AS columna ON columna.object_id = tabla.object_id
JOIN sys.types AS tipo ON tipo.user_type_id = columna.user_type_id
LEFT JOIN sys.default_constraints AS dc
    ON dc.object_id = columna.default_object_id
WHERE tabla.is_ms_shipped = 0
ORDER BY esquema.name, tabla.name, columna.column_id;
GO

-- 3. Claves foráneas y reglas de borrado.
SELECT
    fk.name AS ClaveForanea,
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS EsquemaTabla,
    OBJECT_NAME(fk.parent_object_id) AS Tabla,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Columna,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS EsquemaReferenciada,
    OBJECT_NAME(fk.referenced_object_id) AS TablaReferenciada,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ColumnaReferenciada,
    fk.delete_referential_action_desc AS AccionAlBorrar
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
ORDER BY EsquemaTabla, Tabla, ClaveForanea, fkc.constraint_column_id;
GO

-- 4. Índices y columnas incluidas en cada índice.
SELECT
    esquema.name AS Esquema,
    tabla.name AS Tabla,
    indice.name AS Indice,
    indice.is_primary_key AS EsClavePrimaria,
    indice.is_unique AS EsUnico,
    indice.is_unique_constraint AS EsRestriccionUnica,
    columna.name AS Columna,
    indice_columna.key_ordinal AS OrdenClave,
    indice_columna.is_included_column AS EsColumnaIncluida,
    indice.has_filter AS TieneFiltro,
    indice.filter_definition AS Filtro
FROM sys.indexes AS indice
JOIN sys.tables AS tabla ON tabla.object_id = indice.object_id
JOIN sys.schemas AS esquema ON esquema.schema_id = tabla.schema_id
JOIN sys.index_columns AS indice_columna
    ON indice_columna.object_id = indice.object_id
    AND indice_columna.index_id = indice.index_id
JOIN sys.columns AS columna
    ON columna.object_id = indice_columna.object_id
    AND columna.column_id = indice_columna.column_id
WHERE indice.index_id > 0
  AND indice.is_hypothetical = 0
ORDER BY esquema.name, tabla.name, indice.name, indice_columna.key_ordinal, columna.column_id;
GO
