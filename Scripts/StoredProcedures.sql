-- =============================================
-- PROCEDIMIENTOS ALMACENADOS
-- Sistema de Gestión Académica
-- Ubicación: Scripts/StoredProcedures.sql
-- =============================================

USE SistemaGestionAcademicaDB;
GO

-- =============================================
-- 1. CALCULAR NOTA FINAL DE UN ESTUDIANTE EN UNA MATERIA
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CalcularNotaFinal')
    DROP PROCEDURE sp_CalcularNotaFinal;
GO

CREATE PROCEDURE sp_CalcularNotaFinal
    @InscripcionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NotaFinal DECIMAL(5,2) = 0;
    
    -- Calcular nota final ponderada
    SELECT @NotaFinal = SUM(n.Calificacion * a.ValorPorcentual / 100)
    FROM Notas n
    INNER JOIN Actividades a ON n.ActividadId = a.Id
    WHERE n.InscripcionId = @InscripcionId
      AND a.Activo = 1;
    
    -- Actualizar la inscripción
    UPDATE Inscripciones
    SET NotaFinal = ISNULL(@NotaFinal, 0)
    WHERE Id = @InscripcionId;
    
    -- Retornar resultado
    SELECT 
        @InscripcionId AS InscripcionId,
        ISNULL(@NotaFinal, 0) AS NotaFinal,
        CASE 
            WHEN ISNULL(@NotaFinal, 0) >= 51 THEN 'APROBADO'
            ELSE 'REPROBADO'
        END AS Estado;
END;
GO

-- =============================================
-- 2. REGISTRAR PAGO DE ESTUDIANTE
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_RegistrarPago')
    DROP PROCEDURE sp_RegistrarPago;
GO

CREATE PROCEDURE sp_RegistrarPago
    @InscripcionId INT,
    @Monto DECIMAL(18,2),
    @Concepto NVARCHAR(200),
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar que la inscripción existe
        IF NOT EXISTS (SELECT 1 FROM Inscripciones WHERE Id = @InscripcionId)
        BEGIN
            RAISERROR('La inscripción no existe', 16, 1);
            RETURN;
        END
        
        -- Obtener estudiante de la inscripción
        DECLARE @EstudianteId INT;
        SELECT @EstudianteId = EstudianteId FROM Inscripciones WHERE Id = @InscripcionId;
        
        -- Registrar el pago
        INSERT INTO Pagos (
            InscripcionId,
            EstudianteId,
            Monto,
            FechaPago,
            Tipo,
            Concepto,
            RegistradoPorId,
            Estado
        ) VALUES (
            @InscripcionId,
            @EstudianteId,
            @Monto,
            GETDATE(),
            3, -- Tipo: Materia
            @Concepto,
            @UsuarioId,
            2  -- Estado: Completado
        );
        
        -- Verificar si el pago cubre el costo total
        DECLARE @CostoMateria DECIMAL(18,2);
        DECLARE @TotalPagado DECIMAL(18,2);
        
        SELECT @CostoMateria = m.Costo
        FROM Inscripciones i
        INNER JOIN Materias m ON i.MateriaId = m.Id
        WHERE i.Id = @InscripcionId;
        
        SELECT @TotalPagado = ISNULL(SUM(Monto), 0)
        FROM Pagos
        WHERE InscripcionId = @InscripcionId AND Estado = 2;
        
        -- Actualizar estado de pago si corresponde
        IF @TotalPagado >= @CostoMateria
        BEGIN
            UPDATE Inscripciones
            SET PagoRealizado = 1
            WHERE Id = @InscripcionId;
        END
        
        COMMIT TRANSACTION;
        
        SELECT 
            'Pago registrado exitosamente' AS Mensaje,
            @Monto AS MontoPagado,
            @TotalPagado AS TotalAcumulado,
            @CostoMateria AS CostoTotal;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- 3. OBTENER ESTADO ACADÉMICO DE UN ESTUDIANTE
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EstadoAcademicoEstudiante')
    DROP PROCEDURE sp_EstadoAcademicoEstudiante;
GO

CREATE PROCEDURE sp_EstadoAcademicoEstudiante
    @EstudianteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Datos del estudiante
    SELECT 
        e.Id,
        e.Nombre + ' ' + e.Apellido AS NombreCompleto,
        e.CI,
        e.FechaInscripcion,
        COUNT(i.Id) AS TotalMaterias,
        SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS Aprobadas,
        SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) AS Reprobadas,
        AVG(i.NotaFinal) AS PromedioGeneral,
        SUM(CASE WHEN i.PagoRealizado = 0 AND i.Estado = 1 THEN m.Costo ELSE 0 END) AS DeudaTotal
    FROM Estudiantes e
    LEFT JOIN Inscripciones i ON e.Id = i.EstudianteId
    LEFT JOIN Materias m ON i.MateriaId = m.Id
    WHERE e.Id = @EstudianteId
    GROUP BY e.Id, e.Nombre, e.Apellido, e.CI, e.FechaInscripcion;
    
    -- Detalle por materia
    SELECT 
        m.Nombre AS Materia,
        p.Nombre + ' ' + p.Apellido AS Profesor,
        i.FechaInscripcion,
        i.NotaFinal,
        i.PagoRealizado,
        CASE 
            WHEN i.NotaFinal >= 51 THEN 'Aprobado'
            WHEN i.NotaFinal IS NULL THEN 'Sin calificar'
            ELSE 'Reprobado'
        END AS Estado,
        CASE i.Estado
            WHEN 1 THEN 'Activa'
            WHEN 2 THEN 'Baja Temporal'
            WHEN 3 THEN 'Baja Definitiva'
            WHEN 4 THEN 'Completada'
        END AS EstadoInscripcion
    FROM Inscripciones i
    INNER JOIN Materias m ON i.MateriaId = m.Id
    LEFT JOIN Profesores p ON m.ProfesorId = p.Id
    WHERE i.EstudianteId = @EstudianteId
    ORDER BY i.FechaInscripcion DESC;
END;
GO

-- =============================================
-- 4. REPORTE DE PAGOS POR PERIODO
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ReportePagosPeriodo')
    DROP PROCEDURE sp_ReportePagosPeriodo;
GO

CREATE PROCEDURE sp_ReportePagosPeriodo
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Resumen
    SELECT 
        @FechaInicio AS FechaInicio,
        @FechaFin AS FechaFin,
        COUNT(p.Id) AS TotalPagos,
        SUM(p.Monto) AS MontoTotal,
        AVG(p.Monto) AS MontoPromedio,
        COUNT(DISTINCT p.EstudianteId) AS EstudiantesUnicos
    FROM Pagos p
    WHERE CAST(p.FechaPago AS DATE) BETWEEN @FechaInicio AND @FechaFin
      AND p.Estado = 2;
    
    -- Detalle
    SELECT 
        p.Id,
        p.FechaPago,
        e.Nombre + ' ' + e.Apellido AS Estudiante,
        e.CI,
        m.Nombre AS Materia,
        p.Monto,
        p.Concepto,
        CASE p.Tipo
            WHEN 1 THEN 'Inscripción Inicial'
            WHEN 2 THEN 'Mensualidad'
            WHEN 3 THEN 'Materia'
            WHEN 4 THEN 'Penalización'
            WHEN 5 THEN 'Otro'
        END AS TipoPago
    FROM Pagos p
    LEFT JOIN Estudiantes e ON p.EstudianteId = e.Id
    LEFT JOIN Inscripciones i ON p.InscripcionId = i.Id
    LEFT JOIN Materias m ON i.MateriaId = m.Id
    WHERE CAST(p.FechaPago AS DATE) BETWEEN @FechaInicio AND @FechaFin
      AND p.Estado = 2
    ORDER BY p.FechaPago DESC;
END;
GO

-- =============================================
-- 5. APLICAR PENALIZACIÓN POR MORA
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AplicarPenalizacionMora')
    DROP PROCEDURE sp_AplicarPenalizacionMora;
GO

CREATE PROCEDURE sp_AplicarPenalizacionMora
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PorcentajeMora DECIMAL(5,2);
    DECLARE @Hoy DATE = GETDATE();
    DECLARE @RegistrosAfectados INT = 0;
    
    -- Obtener porcentaje de mora de la configuración
    SELECT @PorcentajeMora = PorcentajePenalizacionMora
    FROM ConfiguracionesInstitucionales
    WHERE Activo = 1;
    
    -- Crear tabla temporal para procesar
    DECLARE @Pendientes TABLE (
        InscripcionId INT,
        EstudianteId INT,
        CostoMateria DECIMAL(18,2),
        DiasAtraso INT,
        MontoMora DECIMAL(18,2)
    );
    
    -- Identificar inscripciones con más de 30 días de atraso
    INSERT INTO @Pendientes
    SELECT 
        i.Id,
        i.EstudianteId,
        m.Costo,
        DATEDIFF(DAY, i.FechaInscripcion, @Hoy),
        m.Costo * (@PorcentajeMora / 100)
    FROM Inscripciones i
    INNER JOIN Materias m ON i.MateriaId = m.Id
    WHERE i.PagoRealizado = 0
      AND i.Estado = 1
      AND DATEDIFF(DAY, i.FechaInscripcion, @Hoy) > 30;
    
    -- Insertar pagos de penalización
    INSERT INTO Pagos (InscripcionId, EstudianteId, Monto, FechaPago, Tipo, Concepto, Estado)
    SELECT 
        InscripcionId,
        EstudianteId,
        MontoMora,
        @Hoy,
        4, -- Tipo: Penalización
        CONCAT('Penalización por mora - ', DiasAtraso, ' días de atraso'),
        1  -- Estado: Pendiente
    FROM @Pendientes;
    
    SET @RegistrosAfectados = @@ROWCOUNT;
    
    SELECT 
        @RegistrosAfectados AS PenalizacionesAplicadas,
        @PorcentajeMora AS PorcentajeMora,
        (SELECT SUM(MontoMora) FROM @Pendientes) AS TotalPenalizaciones,
        (SELECT COUNT(DISTINCT EstudianteId) FROM @Pendientes) AS EstudiantesAfectados;
END;
GO

-- =============================================
-- 6. REPORTE DE MATERIAS POR PROFESOR
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ReporteMateriasProfesor')
    DROP PROCEDURE sp_ReporteMateriasProfesor;
GO

CREATE PROCEDURE sp_ReporteMateriasProfesor
    @ProfesorId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Datos del profesor
    SELECT 
        p.Nombre + ' ' + p.Apellido AS Profesor,
        p.Especialidad,
        p.Correo,
        COUNT(m.Id) AS TotalMaterias,
        COUNT(DISTINCT i.EstudianteId) AS TotalEstudiantes
    FROM Profesores p
    LEFT JOIN Materias m ON p.Id = m.ProfesorId
    LEFT JOIN Inscripciones i ON m.Id = i.MateriaId AND i.Estado = 1
    WHERE p.Id = @ProfesorId
    GROUP BY p.Nombre, p.Apellido, p.Especialidad, p.Correo;
    
    -- Detalle de materias
    SELECT 
        m.Nombre AS Materia,
        a.Nombre AS Aula,
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
        m.Costo,
        COUNT(i.Id) AS EstudiantesInscritos,
        AVG(i.NotaFinal) AS PromedioNotas,
        SUM(CASE WHEN i.NotaFinal >= 51 THEN 1 ELSE 0 END) AS Aprobados,
        SUM(CASE WHEN i.NotaFinal < 51 THEN 1 ELSE 0 END) AS Reprobados
    FROM Materias m
    LEFT JOIN Aulas a ON m.AulaId = a.Id
    LEFT JOIN Horarios h ON m.HorarioId = h.Id
    LEFT JOIN Inscripciones i ON m.Id = i.MateriaId
    WHERE m.ProfesorId = @ProfesorId
    GROUP BY m.Nombre, a.Nombre, h.Dia, h.HoraInicio, h.HoraFin, m.Costo
    ORDER BY m.Nombre;
END;
GO

-- =============================================
-- 7. DAR DE BAJA ESTUDIANTE Y SUS INSCRIPCIONES
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DarBajaEstudiante')
    DROP PROCEDURE sp_DarBajaEstudiante;
GO

CREATE PROCEDURE sp_DarBajaEstudiante
    @EstudianteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar que el estudiante existe
        IF NOT EXISTS (SELECT 1 FROM Estudiantes WHERE Id = @EstudianteId)
        BEGIN
            RAISERROR('El estudiante no existe', 16, 1);
            RETURN;
        END
        
        -- Dar de baja al estudiante
        UPDATE Estudiantes
        SET Activo = 0,
            FechaBaja = GETDATE()
        WHERE Id = @EstudianteId;
        
        -- Dar de baja inscripciones activas
        UPDATE Inscripciones
        SET Estado = 3, -- Baja Definitiva
            FechaBaja = GETDATE()
        WHERE EstudianteId = @EstudianteId
          AND Estado = 1; -- Solo las activas
        
        DECLARE @InscripcionesAfectadas INT = @@ROWCOUNT;
        
        -- Desactivar usuario si existe
        UPDATE AspNetUsers
        SET Activo = 0
        WHERE Id = (SELECT UserId FROM Estudiantes WHERE Id = @EstudianteId);
        
        COMMIT TRANSACTION;
        
        SELECT 
            'Estudiante dado de baja exitosamente' AS Mensaje,
            @EstudianteId AS EstudianteId,
            @InscripcionesAfectadas AS InscripcionesDadasDeBaja;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- 8. INSCRIBIR ESTUDIANTE EN MATERIA
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InscribirEstudiante')
    DROP PROCEDURE sp_InscribirEstudiante;
GO

CREATE PROCEDURE sp_InscribirEstudiante
    @EstudianteId INT,
    @MateriaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar pago inicial
        IF NOT EXISTS (SELECT 1 FROM Estudiantes WHERE Id = @EstudianteId AND PagoInicial > 0)
        BEGIN
            RAISERROR('El estudiante no ha realizado el pago inicial', 16, 1);
            RETURN;
        END
        
        -- Verificar que no esté inscrito ya
        IF EXISTS (
            SELECT 1 FROM Inscripciones 
            WHERE EstudianteId = @EstudianteId 
              AND MateriaId = @MateriaId 
              AND Estado = 1
        )
        BEGIN
            RAISERROR('El estudiante ya está inscrito en esta materia', 16, 1);
            RETURN;
        END
        
        -- Verificar cupos disponibles
        DECLARE @Capacidad INT, @Inscritos INT;
        SELECT @Capacidad = a.Capacidad
        FROM Materias m
        INNER JOIN Aulas a ON m.AulaId = a.Id
        WHERE m.Id = @MateriaId;
        
        SELECT @Inscritos = COUNT(*)
        FROM Inscripciones
        WHERE MateriaId = @MateriaId AND Estado = 1;
        
        IF @Inscritos >= @Capacidad
        BEGIN
            RAISERROR('No hay cupos disponibles en esta materia', 16, 1);
            RETURN;
        END
        
        -- Crear inscripción
        INSERT INTO Inscripciones (EstudianteId, MateriaId, FechaInscripcion, Estado, PagoRealizado)
        VALUES (@EstudianteId, @MateriaId, GETDATE(), 1, 0);
        
        DECLARE @InscripcionId INT = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        SELECT 
            'Inscripción realizada exitosamente' AS Mensaje,
            @InscripcionId AS InscripcionId,
            @EstudianteId AS EstudianteId,
            @MateriaId AS MateriaId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- 9. REPORTE FINANCIERO MENSUAL
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ReporteFinancieroMensual')
    DROP PROCEDURE sp_ReporteFinancieroMensual;
GO

CREATE PROCEDURE sp_ReporteFinancieroMensual
    @Año INT = NULL,
    @Mes INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Año IS NULL SET @Año = YEAR(GETDATE());
    IF @Mes IS NULL SET @Mes = MONTH(GETDATE());
    
    -- Ingresos del mes
    SELECT 
        'Ingresos' AS Concepto,
        ISNULL(SUM(CASE WHEN Tipo IN (1, 3) THEN Monto ELSE 0 END), 0) AS IngresosInscripciones,
        ISNULL(SUM(CASE WHEN Tipo = 2 THEN Monto ELSE 0 END), 0) AS IngresosMensualidades,
        ISNULL(SUM(CASE WHEN Tipo = 4 THEN Monto ELSE 0 END), 0) AS IngresosPenalizaciones,
        ISNULL(SUM(CASE WHEN Tipo = 5 THEN Monto ELSE 0 END), 0) AS OtrosIngresos,
        ISNULL(SUM(Monto), 0) AS TotalIngresos
    FROM Pagos
    WHERE YEAR(FechaPago) = @Año
      AND MONTH(FechaPago) = @Mes
      AND Estado = 2;
    
    -- Deudas pendientes
    SELECT 
        'Deudas Pendientes' AS Concepto,
        COUNT(DISTINCT i.EstudianteId) AS EstudiantesConDeuda,
        COUNT(i.Id) AS MateriasPendientes,
        ISNULL(SUM(m.Costo), 0) AS MontoTotalPendiente
    FROM Inscripciones i
    INNER JOIN Materias m ON i.MateriaId = m.Id
    WHERE i.PagoRealizado = 0 AND i.Estado = 1;
    
    -- Resumen de estudiantes
    SELECT 
        'Estudiantes' AS Concepto,
        COUNT(CASE WHEN Activo = 1 THEN 1 END) AS Activos,
        COUNT(CASE WHEN Activo = 0 THEN 1 END) AS Inactivos,
        COUNT(*) AS Total,
        COUNT(CASE WHEN YEAR(FechaInscripcion) = @Año AND MONTH(FechaInscripcion) = @Mes THEN 1 END) AS NuevosEsteMes
    FROM Estudiantes;
END;
GO

-- =============================================
-- 10. LIMPIEZA DE DATOS (ADMINISTRATIVO)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_LimpiezaDatos')
    DROP PROCEDURE sp_LimpiezaDatos;
GO

CREATE PROCEDURE sp_LimpiezaDatos
    @EliminarInscripcionesCanceladas BIT = 0,
    @EliminarPagosAnulados BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Resultados TABLE (
        Accion NVARCHAR(200),
        RegistrosAfectados INT
    );
    
    IF @EliminarInscripcionesCanceladas = 1
    BEGIN
        DELETE FROM Notas WHERE InscripcionId IN (SELECT Id FROM Inscripciones WHERE Estado = 3);
        DELETE FROM Pagos WHERE InscripcionId IN (SELECT Id FROM Inscripciones WHERE Estado = 3);
        DELETE FROM Inscripciones WHERE Estado = 3;
        
        INSERT INTO @Resultados VALUES ('Inscripciones canceladas eliminadas', @@ROWCOUNT);
    END
    
    IF @EliminarPagosAnulados = 1
    BEGIN
        DELETE FROM Pagos WHERE Estado = 3;
        INSERT INTO @Resultados VALUES ('Pagos anulados eliminados', @@ROWCOUNT);
    END
    
    SELECT * FROM @Resultados;
    
    IF NOT EXISTS (SELECT 1 FROM @Resultados)
        PRINT 'No se realizaron cambios. Especifique al menos una opción.';
END;
GO

PRINT 'Procedimientos almacenados creados exitosamente';
GO