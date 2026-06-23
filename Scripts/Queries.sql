-- =============================================
-- CONSULTAS ÚTILES - SISTEMA DE GESTIÓN ACADÉMICA
-- Ubicación: Scripts/Queries.sql
-- =============================================

USE SistemaGestionAcademicaDB;
GO

-- =============================================
-- 1. CONSULTAS DE USUARIOS Y ROLES
-- =============================================

-- 1.1. Ver todos los usuarios con sus roles
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.NombreCompleto,
    u.FechaRegistro,
    u.Activo,
    u.UltimoAcceso,
    r.Name AS Rol
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.FechaRegistro DESC;

-- 1.2. Usuarios por rol
SELECT 
    r.Name AS Rol,
    COUNT(ur.UserId) AS TotalUsuarios
FROM AspNetRoles r
LEFT JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
GROUP BY r.Name
ORDER BY TotalUsuarios DESC;

-- 1.3. Usuarios que no han iniciado sesión en los últimos 30 días
SELECT 
    UserName,
    Email,
    NombreCompleto,
    UltimoAcceso,
    DATEDIFF(DAY, UltimoAcceso, GETDATE()) AS DiasInactivo
FROM AspNetUsers
WHERE UltimoAcceso IS NOT NULL 
  AND DATEDIFF(DAY, UltimoAcceso, GETDATE()) > 30
ORDER BY DiasInactivo DESC;

-- =============================================
-- 2. CONSULTAS DE ESTUDIANTES
-- =============================================

-- 2.1. Listado completo de estudiantes
SELECT 
    e.Id,
    e.Nombre + ' ' + e.Apellido AS NombreCompleto,
    e.CI,
    e.Telefono,
    e.Correo,
    e.FechaInscripcion,
    e.PagoInicial,
    e.Activo,
    COUNT(i.Id) AS MateriasInscritas
FROM Estudiantes e
LEFT JOIN Inscripciones i ON e.Id = i.EstudianteId AND i.Estado = 1
GROUP BY e.Id, e.Nombre, e.Apellido, e.CI, e.Telefono, e.Correo, 
         e.FechaInscripcion, e.PagoInicial, e.Activo
ORDER BY e.Apellido, e.Nombre;

-- 2.2. Estudiantes con deudas pendientes
SELECT 
    e.Id,
    e.Nombre + ' ' + e.Apellido AS Estudiante,
    e.CI,
    e.Telefono,
    COUNT(i.Id) AS MateriasPendientes,
    SUM(m.Costo) AS DeudaTotal,
    DATEDIFF(DAY, MIN(i.FechaInscripcion), GETDATE()) AS DiasAtrasoMaximo
FROM Estudiantes e
INNER JOIN Inscripciones i ON e.Id = i.EstudianteId
INNER JOIN Materias m ON i.MateriaId = m.Id
WHERE i.PagoRealizado = 0 
  AND i.Estado = 1
  AND e.Activo = 1
GROUP BY e.Id, e.Nombre, e.Apellido, e.CI, e.Telefono
HAVING SUM(m.Costo) > 0
ORDER BY DeudaTotal DESC;

-- 2.3. Estudiantes con mejor promedio
SELECT TOP 10
    e.Id,
    e.Nombre + ' ' + e.Apellido AS Estudiante,
    e.CI,
    COUNT(i.Id) AS MateriasCursadas,
    AVG(i.NotaFinal) AS PromedioGeneral,
    SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS Aprobadas,
    SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) AS Reprobadas
FROM Estudiantes e
INNER JOIN Inscripciones i ON e.Id = i.EstudianteId
WHERE i.NotaFinal IS NOT NULL AND e.Activo = 1
GROUP BY e.Id, e.Nombre, e.Apellido, e.CI
HAVING COUNT(i.Id) >= 3
ORDER BY PromedioGeneral DESC;

-- 2.4. Estudiantes próximos a graduarse (todas aprobadas)
SELECT 
    e.Id,
    e.Nombre + ' ' + e.Apellido AS Estudiante,
    e.CI,
    COUNT(i.Id) AS TotalMaterias,
    AVG(i.NotaFinal) AS PromedioFinal
FROM Estudiantes e
INNER JOIN Inscripciones i ON e.Id = i.EstudianteId
WHERE i.NotaFinal IS NOT NULL
  AND e.Activo = 1
GROUP BY e.Id, e.Nombre, e.Apellido, e.CI
HAVING SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) = 0
   AND COUNT(i.Id) >= 5
ORDER BY PromedioFinal DESC;

-- =============================================
-- 3. CONSULTAS DE PROFESORES
-- =============================================

-- 3.1. Profesores con sus materias asignadas
SELECT 
    p.Id,
    p.Nombre + ' ' + p.Apellido AS Profesor,
    p.Especialidad,
    p.Correo,
    COUNT(m.Id) AS TotalMaterias,
    STRING_AGG(m.Nombre, ', ') AS MateriasAsignadas
FROM Profesores p
LEFT JOIN Materias m ON p.Id = m.ProfesorId AND m.Activo = 1
WHERE p.Activo = 1
GROUP BY p.Id, p.Nombre, p.Apellido, p.Especialidad, p.Correo
ORDER BY TotalMaterias DESC;

-- 3.2. Profesores con más estudiantes
SELECT 
    p.Id,
    p.Nombre + ' ' + p.Apellido AS Profesor,
    p.Especialidad,
    COUNT(DISTINCT i.EstudianteId) AS TotalEstudiantes,
    COUNT(DISTINCT m.Id) AS Materias
FROM Profesores p
INNER JOIN Materias m ON p.Id = m.ProfesorId
INNER JOIN Inscripciones i ON m.Id = i.MateriaId AND i.Estado = 1
WHERE p.Activo = 1
GROUP BY p.Id, p.Nombre, p.Apellido, p.Especialidad
ORDER BY TotalEstudiantes DESC;

-- =============================================
-- 4. CONSULTAS DE MATERIAS
-- =============================================

-- 4.1. Materias con más estudiantes inscritos
SELECT 
    m.Id,
    m.Nombre AS Materia,
    p.Nombre + ' ' + p.Apellido AS Profesor,
    a.Nombre AS Aula,
    m.Costo,
    COUNT(i.Id) AS EstudiantesInscritos,
    a.Capacidad,
    CAST(COUNT(i.Id) AS FLOAT) / a.Capacidad * 100 AS PorcentajeOcupacion
FROM Materias m
LEFT JOIN Profesores p ON m.ProfesorId = p.Id
LEFT JOIN Aulas a ON m.AulaId = a.Id
LEFT JOIN Inscripciones i ON m.Id = i.MateriaId AND i.Estado = 1
WHERE m.Activo = 1
GROUP BY m.Id, m.Nombre, p.Nombre, p.Apellido, a.Nombre, m.Costo, a.Capacidad
ORDER BY EstudiantesInscritos DESC;

-- 4.2. Materias con porcentaje de aprobación
SELECT 
    m.Id,
    m.Nombre AS Materia,
    COUNT(i.Id) AS TotalInscritos,
    SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS Aprobados,
    SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) AS Reprobados,
    CAST(SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS FLOAT) / 
        NULLIF(COUNT(i.Id), 0) * 100 AS PorcentajeAprobacion,
    AVG(i.NotaFinal) AS PromedioNota
FROM Materias m
LEFT JOIN Inscripciones i ON m.Id = i.MateriaId
WHERE i.NotaFinal IS NOT NULL AND m.Activo = 1
GROUP BY m.Id, m.Nombre
HAVING COUNT(i.Id) > 0
ORDER BY PorcentajeAprobacion DESC;

-- =============================================
-- 5. CONSULTAS DE PAGOS
-- =============================================

-- 5.1. Resumen de ingresos por mes
SELECT 
    YEAR(FechaPago) AS Año,
    MONTH(FechaPago) AS Mes,
    DATENAME(MONTH, FechaPago) AS NombreMes,
    COUNT(Id) AS TotalPagos,
    SUM(Monto) AS IngresoTotal,
    AVG(Monto) AS PromedioPago
FROM Pagos
WHERE Estado = 2 -- Completado
GROUP BY YEAR(FechaPago), MONTH(FechaPago), DATENAME(MONTH, FechaPago)
ORDER BY Año DESC, Mes DESC;

-- 5.2. Ingresos por tipo de pago
SELECT 
    Tipo,
    CASE Tipo
        WHEN 1 THEN 'Inscripción Inicial'
        WHEN 2 THEN 'Mensualidad'
        WHEN 3 THEN 'Materia'
        WHEN 4 THEN 'Penalización'
        WHEN 5 THEN 'Otro'
    END AS TipoPago,
    COUNT(Id) AS Cantidad,
    SUM(Monto) AS Total,
    AVG(Monto) AS Promedio
FROM Pagos
WHERE Estado = 2
GROUP BY Tipo
ORDER BY Total DESC;

-- 5.3. Pagos del día
SELECT 
    p.Id,
    e.Nombre + ' ' + e.Apellido AS Estudiante,
    e.CI,
    m.Nombre AS Materia,
    p.Monto,
    p.FechaPago,
    p.Concepto,
    p.Estado
FROM Pagos p
LEFT JOIN Estudiantes e ON p.EstudianteId = e.Id
LEFT JOIN Inscripciones i ON p.InscripcionId = i.Id
LEFT JOIN Materias m ON i.MateriaId = m.Id
WHERE CAST(p.FechaPago AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY p.FechaPago DESC;

-- =============================================
-- 6. CONSULTAS DE NOTAS Y ACTIVIDADES
-- =============================================

-- 6.1. Notas por materia (detalle)
SELECT 
    m.Nombre AS Materia,
    e.Nombre + ' ' + e.Apellido AS Estudiante,
    a.Nombre AS Actividad,
    a.Tipo,
    n.Calificacion,
    a.ValorPorcentual,
    (n.Calificacion * a.ValorPorcentual / 100) AS Ponderacion,
    i.NotaFinal
FROM Notas n
INNER JOIN Inscripciones i ON n.InscripcionId = i.Id
INNER JOIN Estudiantes e ON i.EstudianteId = e.Id
INNER JOIN Actividades a ON n.ActividadId = a.Id
INNER JOIN Materias m ON i.MateriaId = m.Id
ORDER BY m.Nombre, e.Apellido, a.Fecha;

-- 6.2. Actividades por materia con promedio de notas
SELECT 
    m.Nombre AS Materia,
    a.Nombre AS Actividad,
    a.Tipo,
    a.ValorPorcentual,
    COUNT(n.Id) AS NotasRegistradas,
    AVG(n.Calificacion) AS Promedio,
    MIN(n.Calificacion) AS NotaMinima,
    MAX(n.Calificacion) AS NotaMaxima
FROM Actividades a
INNER JOIN Materias m ON a.MateriaId = m.Id
LEFT JOIN Notas n ON a.Id = n.ActividadId
WHERE a.Activo = 1
GROUP BY m.Nombre, a.Nombre, a.Tipo, a.ValorPorcentual
ORDER BY m.Nombre, a.Fecha;

-- =============================================
-- 7. CONSULTAS DE AULAS Y HORARIOS
-- =============================================

-- 7.1. Ocupación de aulas
SELECT 
    a.Codigo,
    a.Nombre AS Aula,
    a.Capacidad,
    a.EsLaboratorio,
    COUNT(m.Id) AS MateriasAsignadas,
    SUM(CASE WHEN i.Estado = 1 THEN 1 ELSE 0 END) AS EstudiantesOcupando,
    CAST(SUM(CASE WHEN i.Estado = 1 THEN 1 ELSE 0 END) AS FLOAT) / 
        NULLIF(a.Capacidad, 0) * 100 AS PorcentajeOcupacion
FROM Aulas a
LEFT JOIN Materias m ON a.Id = m.AulaId AND m.Activo = 1
LEFT JOIN Inscripciones i ON m.Id = i.MateriaId AND i.Estado = 1
WHERE a.Activo = 1
GROUP BY a.Id, a.Codigo, a.Nombre, a.Capacidad, a.EsLaboratorio
ORDER BY PorcentajeOcupacion DESC;

-- 7.2. Horarios disponibles vs ocupados
SELECT 
    h.Id,
    CONCAT(
        CASE h.Dia
            WHEN 0 THEN 'Domingo'
            WHEN 1 THEN 'Lunes'
            WHEN 2 THEN 'Martes'
            WHEN 3 THEN 'Miércoles'
            WHEN 4 THEN 'Jueves'
            WHEN 5 THEN 'Viernes'
            WHEN 6 THEN 'Sábado'
        END,
        ' ',
        FORMAT(h.HoraInicio, 'hh\:mm'),
        ' - ',
        FORMAT(h.HoraFin, 'hh\:mm')
    ) AS Horario,
    COUNT(m.Id) AS MateriasEnHorario
FROM Horarios h
LEFT JOIN Materias m ON h.Id = m.HorarioId AND m.Activo = 1
WHERE h.Activo = 1
GROUP BY h.Id, h.Dia, h.HoraInicio, h.HoraFin
ORDER BY h.Dia, h.HoraInicio;

-- =============================================
-- 8. CONSULTAS ESTADÍSTICAS
-- =============================================

-- 8.1. Dashboard rápido
SELECT 
    (SELECT COUNT(*) FROM Estudiantes WHERE Activo = 1) AS TotalEstudiantes,
    (SELECT COUNT(*) FROM Profesores WHERE Activo = 1) AS TotalProfesores,
    (SELECT COUNT(*) FROM Empleados WHERE Activo = 1) AS TotalEmpleados,
    (SELECT COUNT(*) FROM Materias WHERE Activo = 1) AS TotalMaterias,
    (SELECT COUNT(*) FROM Aulas WHERE Activo = 1) AS TotalAulas,
    (SELECT COUNT(*) FROM Inscripciones WHERE Estado = 1) AS InscripcionesActivas,
    (SELECT SUM(Monto) FROM Pagos WHERE Estado = 2) AS IngresosTotales,
    (SELECT COUNT(*) FROM Inscripciones WHERE PagoRealizado = 0 AND Estado = 1) AS PagosPendientes;

-- 8.2. Tasa de aprobación general
SELECT 
    COUNT(i.Id) AS TotalEvaluados,
    SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS Aprobados,
    SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) AS Reprobados,
    CAST(SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS FLOAT) / 
        NULLIF(COUNT(i.Id), 0) * 100 AS TasaAprobacion
FROM Inscripciones i
WHERE i.NotaFinal IS NOT NULL;

-- 8.3. Distribución de notas
SELECT 
    CASE 
        WHEN NotaFinal >= 90 THEN '90-100 (Excelente)'
        WHEN NotaFinal >= 80 THEN '80-89 (Muy Bueno)'
        WHEN NotaFinal >= 70 THEN '70-79 (Bueno)'
        WHEN NotaFinal >= 60 THEN '60-69 (Regular)'
        WHEN NotaFinal >= 51 THEN '51-59 (Suficiente)'
        ELSE '0-50 (Reprobado)'
    END AS RangoNota,
    COUNT(*) AS Cantidad,
    CAST(COUNT(*) AS FLOAT) / (SELECT COUNT(*) FROM Inscripciones WHERE NotaFinal IS NOT NULL) * 100 AS Porcentaje
FROM Inscripciones
WHERE NotaFinal IS NOT NULL
GROUP BY 
    CASE 
        WHEN NotaFinal >= 90 THEN '90-100 (Excelente)'
        WHEN NotaFinal >= 80 THEN '80-89 (Muy Bueno)'
        WHEN NotaFinal >= 70 THEN '70-79 (Bueno)'
        WHEN NotaFinal >= 60 THEN '60-69 (Regular)'
        WHEN NotaFinal >= 51 THEN '51-59 (Suficiente)'
        ELSE '0-50 (Reprobado)'
    END
ORDER BY RangoNota DESC;

-- =============================================
-- 9. CONSULTAS DE CONFIGURACIÓN
-- =============================================

-- 9.1. Configuración actual
SELECT 
    NombreInstitucion,
    PeriodoActual,
    PagoInicialInscripcion,
    CostoBaseMateria,
    PorcentajePenalizacionMora,
    DiaInicioPagos,
    DiaFinPagos,
    FechaUltimaActualizacion
FROM ConfiguracionesInstitucionales
WHERE Activo = 1;

-- 9.2. Verificar periodo de pagos actual
DECLARE @hoy DATE = GETDATE();
DECLARE @diaInicio INT, @diaFin INT;

SELECT @diaInicio = DiaInicioPagos, @diaFin = DiaFinPagos
FROM ConfiguracionesInstitucionales WHERE Activo = 1;

SELECT 
    @hoy AS FechaActual,
    DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), @diaInicio) AS InicioPeriodo,
    DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), @diaFin) AS FinPeriodo,
    CASE 
        WHEN @hoy BETWEEN DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), @diaInicio) 
                      AND DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), @diaFin)
        THEN 'Periodo ACTIVO'
        ELSE 'Periodo INACTIVO'
    END AS EstadoPeriodo;

PRINT 'Consultas de ejemplo completadas exitosamente';
GO