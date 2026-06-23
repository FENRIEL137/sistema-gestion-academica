-- =============================================
-- Script de Datos de Prueba
-- Sistema de Gestión Académica
-- =============================================

USE SistemaGestionAcademicaDB;
GO

-- Insertar usuarios de prueba
-- Nota: Las contraseñas deben ser hasheadas usando el PasswordHasher de Identity

-- Insertar materias de prueba (ejemplo)
INSERT INTO Materias (Nombre, Descripcion, Costo, Activo)
VALUES 
('Matemáticas I', 'Curso introductorio de matemáticas', 350.00, 1),
('Programación I', 'Fundamentos de programación', 400.00, 1),
('Base de Datos', 'Diseño y administración de bases de datos', 350.00, 1),
('Redes de Computadoras', 'Fundamentos de redes', 380.00, 1),
('Inglés Técnico', 'Inglés orientado a tecnología', 300.00, 1);

-- Consultar datos insertados
SELECT * FROM Materias;
SELECT * FROM Aulas;
SELECT * FROM Horarios;
SELECT * FROM ConfiguracionesInstitucionales;

PRINT 'Datos de prueba insertados exitosamente';
GO