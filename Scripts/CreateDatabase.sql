-- =============================================
-- Script de Creación de Base de Datos
-- Sistema de Gestión Académica
-- =============================================

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SistemaGestionAcademicaDB')
BEGIN
    CREATE DATABASE SistemaGestionAcademicaDB;
END
GO

USE SistemaGestionAcademicaDB;
GO

-- Verificar que la base de datos se creó correctamente
IF DB_NAME() = 'SistemaGestionAcademicaDB'
BEGIN
    PRINT 'Base de datos creada exitosamente: SistemaGestionAcademicaDB'
    
    -- Mostrar todas las tablas después de aplicar las migraciones
    SELECT 
        TABLE_SCHEMA,
        TABLE_NAME,
        TABLE_TYPE
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME;
END
ELSE
BEGIN
    PRINT 'Error: No se pudo crear la base de datos'
END
GO