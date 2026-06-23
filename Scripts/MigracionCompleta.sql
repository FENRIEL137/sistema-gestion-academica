-- =============================================
-- SCRIPT COMPLETO DE MIGRACIÓN
-- Sistema de Gestión Académica
-- =============================================

-- Crear base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SistemaGestionAcademicaDB')
BEGIN
    CREATE DATABASE SistemaGestionAcademicaDB;
    PRINT 'Base de datos creada: SistemaGestionAcademicaDB';
END
GO

USE SistemaGestionAcademicaDB;
GO

-- Tabla de Migraciones de EF Core
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL PRIMARY KEY,
        [ProductVersion] NVARCHAR(32) NOT NULL
    );
    PRINT 'Tabla de historial de migraciones creada';
END
GO

-- Verificar integridad
PRINT 'Verificando integridad de la base de datos...';

SELECT 
    t.name AS Tabla,
    COUNT(c.name) AS Columnas,
    SUM(CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END) AS PrimaryKeys,
    SUM(CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END) AS ForeignKeys
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
LEFT JOIN (
    SELECT ku.TABLE_NAME, ku.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON t.name = pk.TABLE_NAME AND c.name = pk.COLUMN_NAME
LEFT JOIN (
    SELECT TABLE_NAME, COLUMN_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE CONSTRAINT_NAME LIKE 'FK_%'
) fk ON t.name = fk.TABLE_NAME AND c.name = fk.COLUMN_NAME
GROUP BY t.name
ORDER BY t.name;

PRINT 'Script de migración completado exitosamente';
GO