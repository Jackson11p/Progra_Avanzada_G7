-- Crear base de datos
CREATE DATABASE HealthyLifeDB;
GO

USE HealthyLifeDB;
GO

-- Tabla: Roles de usuario
CREATE TABLE Roles (
    RolID INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

INSERT INTO Roles (NombreRol) VALUES ('Usuario');
GO
INSERT INTO Roles (NombreRol) VALUES ('Doctor');
GO
INSERT INTO Roles (NombreRol) VALUES ('Administrador');
GO
-- Tabla: Usuarios
CREATE TABLE Usuarios (
    UsuarioID INT PRIMARY KEY IDENTITY(1,1),
	Cedula VARCHAR(50) UNIQUE NOT NULL,
    NombreCompleto VARCHAR(100) NOT NULL,
    CorreoElectronico VARCHAR(100) UNIQUE NOT NULL,
    ContrasenaHash VARCHAR(255) NOT NULL,
    RolID INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RolID) REFERENCES Roles(RolID)
);

-- Tabla: Doctores
CREATE TABLE Doctores (
    DoctorID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    Especialidad VARCHAR(100) NOT NULL,
    CedulaProfesional VARCHAR(20) UNIQUE NOT NULL,
    FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);

-- Tabla: Pacientes
CREATE TABLE Pacientes (
    PacienteID INT PRIMARY KEY IDENTITY(1,1),
    NombreCompleto VARCHAR(100) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    Genero VARCHAR(10),
    Direccion VARCHAR(200),
    Telefono VARCHAR(20),
    CorreoElectronico VARCHAR(100)
);

-- Tabla: Citas m dicas
CREATE TABLE Citas (
    CitaID INT PRIMARY KEY IDENTITY(1,1),
    PacienteID INT NOT NULL,
    DoctorID INT NOT NULL,
    FechaHora DATETIME NOT NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    MotivoConsulta VARCHAR(255),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PacienteID) REFERENCES Pacientes(PacienteID),
    FOREIGN KEY (DoctorID) REFERENCES Doctores(DoctorID),
    CONSTRAINT UC_Cita_Unica UNIQUE (DoctorID, FechaHora)
);

-- Tabla: Diagn sticos m dicos
CREATE TABLE Diagnosticos (
    DiagnosticoID INT PRIMARY KEY IDENTITY(1,1),
    CitaID INT NOT NULL,
    Descripcion TEXT NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CitaID) REFERENCES Citas(CitaID)
);

-- Tabla: Tratamientos aplicados
CREATE TABLE Tratamientos (
    TratamientoID INT PRIMARY KEY IDENTITY(1,1),
    NombreTratamiento VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Costo DECIMAL(10,2)
);

-- Tabla intermedia: Cita-Tratamiento (N:M)
CREATE TABLE CitaTratamientos (
    CitaID INT NOT NULL,
    TratamientoID INT NOT NULL,
    PRIMARY KEY (CitaID, TratamientoID),
    FOREIGN KEY (CitaID) REFERENCES Citas(CitaID),
    FOREIGN KEY (TratamientoID) REFERENCES Tratamientos(TratamientoID)
);

--Tabla: Facturacion
CREATE TABLE Facturas (
    FacturaID INT PRIMARY KEY IDENTITY(1,1),
    PacienteID INT NOT NULL,
    FechaEmision DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    EstadoPago VARCHAR(20) DEFAULT 'Pendiente', -- Pagado, Pendiente, Cancelado
    FOREIGN KEY (PacienteID) REFERENCES Pacientes(PacienteID)
);
--Tabla:DetallesFactura
CREATE TABLE FacturaDetalles (
    DetalleID INT PRIMARY KEY IDENTITY(1,1),
    FacturaID INT NOT NULL,
    TratamientoID INT NOT NULL,
    CostoUnitario DECIMAL(10,2),
    Cantidad INT DEFAULT 1,
    Subtotal AS (CostoUnitario * Cantidad),
    FOREIGN KEY (FacturaID) REFERENCES Facturas(FacturaID),
    FOREIGN KEY (TratamientoID) REFERENCES Tratamientos(TratamientoID)
);

-- Tabla: Historial M dico
CREATE TABLE HistorialMedico (
    HistorialID INT PRIMARY KEY IDENTITY(1,1),
    PacienteID INT NOT NULL,
    DoctorID INT NOT NULL,
    DiagnosticoID INT NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (PacienteID) REFERENCES Pacientes(PacienteID),
    FOREIGN KEY (DoctorID) REFERENCES Doctores(DoctorID),
    FOREIGN KEY (DiagnosticoID) REFERENCES Diagnosticos(DiagnosticoID)
);

--Tablo Errores
CREATE TABLE LogsErrores (
    ErrorID      BIGINT           PRIMARY KEY IDENTITY(1,1),
    UsuarioID    BIGINT           NULL,
    Origen       NVARCHAR(200)    NULL,
    TipoError    NVARCHAR(100)    NULL,
    Mensaje      NVARCHAR(MAX)    NULL,
    StackTrace   NVARCHAR(MAX)    NULL,
    RequestId    NVARCHAR(100)    NULL,
    IpCliente    NVARCHAR(45)     NULL,
    FechaError   DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET()
);


CREATE OR ALTER PROCEDURE RegistrarUsuario
    @Cedula VARCHAR(50),
    @NombreCompleto VARCHAR(100),
    @CorreoElectronico VARCHAR(100),
    @ContrasenaHash VARCHAR(255)
AS
BEGIN
	INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID)
    VALUES (@Cedula, @NombreCompleto, @CorreoElectronico, @ContrasenaHash, 1);
    
END
GO

CREATE OR ALTER PROCEDURE IniciarSesion
    @CorreoElectronico VARCHAR(100),
    @ContrasenaHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UsuarioID, Cedula, NombreCompleto, CorreoElectronico, RolID
    FROM Usuarios
    WHERE CorreoElectronico = @CorreoElectronico
      AND ContrasenaHash = @ContrasenaHash
      AND Activo = 1;
END
GO

CREATE PROCEDURE ValidarCorreo
	@CorreoElectronico varchar(100)
AS
BEGIN

	SELECT	UsuarioID,
			NombreCompleto,
			CorreoElectronico,
			Cedula,
			Activo
	  FROM	Usuarios
	WHERE	CorreoElectronico = @CorreoElectronico
		AND Activo = 1
	
END
GO

CREATE OR ALTER PROCEDURE ActualizarContrasenna
    @UsuarioID int,
    @ContrasenaHash VARCHAR(255)
AS
BEGIN
    UPDATE Usuarios
    SET ContrasenaHash = @ContrasenaHash
    WHERE UsuarioID = @UsuarioID
END
GO

CREATE PROCEDURE [dbo].[ActualizarUsuario]
	@Identificacion varchar(20),
	@Nombre varchar(255),
	@Correo varchar(100),
	@IdUsuario bigint
AS
BEGIN
	
	IF NOT EXISTS(SELECT 1 FROM dbo.TUsuario
				  WHERE Identificacion = @Identificacion
					AND Correo = @Correo
					AND IdUsuario != @IdUsuario)
	BEGIN

		UPDATE	TUsuario
		SET		Identificacion = @Identificacion,
				Nombre = @Nombre,
				Correo =  @Correo
		WHERE	IdUsuario = @IdUsuario

	END

END
GO

CREATE PROCEDURE ConsultarUsuario
	@IdUsuario BIGINT
AS
BEGIN

	SELECT	IdUsuario,
			Nombre,
			Correo,
			Identificacion,
			Estado,
			U.IdRol,
			R.NombreRol
	  FROM	dbo.TUsuario U
	  INNER JOIN dbo.TRol R ON U.IdRol = R.IdRol
	WHERE	IdUsuario = @IdUsuario
	
END
GO

CREATE OR ALTER PROCEDURE ConsultarUsuario
    @UsuarioID INT
AS
BEGIN
    SELECT 
        u.UsuarioID,
        u.Cedula,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.Activo,
        u.RolID,
        r.NombreRol
    FROM 
        Usuarios u
    INNER JOIN 
        Roles r ON u.RolID = r.RolID
    WHERE 
        u.UsuarioID = @UsuarioID
END
GO


CREATE OR ALTER PROCEDURE ActualizarUsuario
    @UsuarioID INT,
    @Cedula VARCHAR(50),
    @NombreCompleto VARCHAR(100),
    @CorreoElectronico VARCHAR(100)
AS
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM Usuarios 
        WHERE 
            (Cedula = @Cedula OR CorreoElectronico = @CorreoElectronico)
            AND UsuarioID != @UsuarioID
    )
    BEGIN
        UPDATE Usuarios
        SET 
            Cedula = @Cedula,
            NombreCompleto = @NombreCompleto,
            CorreoElectronico = @CorreoElectronico
        WHERE UsuarioID = @UsuarioID
    END
END
GO

CREATE PROCEDURE RegistrarError
    @UsuarioID    BIGINT         = NULL,
    @Origen       NVARCHAR(200),
    @TipoError    NVARCHAR(100),
    @Mensaje      NVARCHAR(MAX),
    @StackTrace   NVARCHAR(MAX),
    @RequestId    NVARCHAR(100),   -- ahora NVARCHAR
    @IpCliente    NVARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO LogsErrores
      (UsuarioID, Origen, TipoError, Mensaje, StackTrace, RequestId, IpCliente)
    VALUES
      (@UsuarioID, @Origen, @TipoError, @Mensaje, @StackTrace, @RequestId, @IpCliente);
END
GO

CREATE PROCEDURE ConsultarHistorialPorPaciente
    @PacienteID INT
AS
BEGIN
    SELECT h.HistorialID, h.FechaRegistro, d.NombreCompleto AS NombreDoctor, diag.Descripcion
    FROM HistorialMedico h
    JOIN Doctores d ON d.DoctorID = h.DoctorID
    JOIN Diagnosticos diag ON diag.DiagnosticoID = h.DiagnosticoID
    WHERE h.PacienteID = @PacienteID
END

CREATE PROCEDURE RegistrarHistorial
    @PacienteID INT,
    @DoctorID INT,
    @DiagnosticoID INT
AS
BEGIN
    INSERT INTO HistorialMedico (PacienteID, DoctorID, DiagnosticoID, FechaRegistro)
    VALUES (@PacienteID, @DoctorID, @DiagnosticoID, GETDATE())
END

CREATE PROCEDURE CrearFactura
    @CitaID INT,
    @Total DECIMAL(10,2),
    @EstadoPago VARCHAR(20)
AS
BEGIN
    INSERT INTO Facturas (CitaID, Total, EstadoPago, FechaEmision)
    VALUES (@CitaID, @Total, @EstadoPago, GETDATE())
END

CREATE PROCEDURE ConsultarFactura
    @FacturaID INT
AS
BEGIN
    SELECT f.FacturaID, f.Total, f.EstadoPago, f.FechaEmision, c.PacienteID, p.NombreCompleto AS Paciente
    FROM Facturas f
    JOIN Citas c ON c.CitaID = f.CitaID
    JOIN Pacientes p ON p.PacienteID = c.PacienteID
    WHERE f.FacturaID = @FacturaID
END


--Metodos Erick

--Registra un doctor
GO
CREATE OR ALTER   PROCEDURE [dbo].[RegistrarDcotor]
    @UsuarioID int,
    @Especialidad VARCHAR(100),
    @CedulaProfesional VARCHAR(20)    
AS
BEGIN
	INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
    VALUES (@UsuarioID, @Especialidad, @CedulaProfesional);
    
END

--Registra una cita
GO
CREATE OR ALTER PROCEDURE [dbo].[RegistrarCita]    
    @PacienteID INT,
    @DoctorID INT,
    @FechaHora DATETIME,
    @Estado VARCHAR(30),
    @MotivoConsulta VARCHAR(255),
    @FechaCreacion DATETIME
AS
BEGIN
    -- Ver si una cita existe
    IF NOT EXISTS (
        SELECT 1 
        FROM Citas 
        WHERE DoctorID = @DoctorID AND FechaHora = @FechaHora
    )
    BEGIN
        -- Si no, guard la cita
        INSERT INTO Citas (PacienteID, DoctorID, FechaHora, Estado, MotivoConsulta, FechaCreacion)
        VALUES (@PacienteID, @DoctorID, @FechaHora, @Estado, @MotivoConsulta, @FechaCreacion);
    END
END


--Registra un paciente
GO
CREATE OR ALTER PROCEDURE [dbo].[RegistrarPaciente]    
    @NombreCompleto VARCHAR(100),
    @FechaNacimiento date,
    @Genero VARCHAR(10),
    @Direccion VARCHAR(200),
    @Telefono VARCHAR(20),
    @CorreoElectronico VARCHAR(100)
AS
BEGIN
	INSERT INTO Pacientes (NombreCompleto, FechaNacimiento, Genero, Direccion, Telefono, CorreoElectronico)
    VALUES (@NombreCompleto, @FechaNacimiento, @Genero, @Direccion, @Telefono, @CorreoElectronico);    
END


-- Tabla
CREATE TABLE CitasPublicas (
    Id                 BIGINT IDENTITY(1,1) PRIMARY KEY,
    Nombre             NVARCHAR(150) NOT NULL,
    Email              NVARCHAR(150) NOT NULL,
    Telefono           NVARCHAR(50)  NOT NULL,
    FechaHoraPreferida DATETIME2     NOT NULL,
    Especialidad       NVARCHAR(100) NOT NULL,
    DoctorNombre       NVARCHAR(150) NULL,
    Mensaje            NVARCHAR(MAX) NULL,
    FechaSolicitud     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

-- Procedimiento
CREATE OR ALTER PROCEDURE RegistrarCitaPublica
    @Nombre             NVARCHAR(150),
    @Email              NVARCHAR(150),
    @Telefono           NVARCHAR(50),
    @FechaHoraPreferida DATETIME2,
    @Especialidad       NVARCHAR(100),
    @DoctorNombre       NVARCHAR(150) = NULL,
    @Mensaje            NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO CitasPublicas
      (Nombre, Email, Telefono, FechaHoraPreferida, Especialidad, DoctorNombre, Mensaje)
    VALUES
      (@Nombre, @Email, @Telefono, @FechaHoraPreferida, @Especialidad, @DoctorNombre, @Mensaje);
END
GO

CREATE OR ALTER PROCEDURE Cita_Listar_Unificada
AS
BEGIN
  SET NOCOUNT ON;

  SELECT
      CAST(N'Cita' AS NVARCHAR(10))          AS Tipo,
      c.CitaID                                AS CitaID,
      CAST(NULL AS BIGINT)                    AS SolicitudID,      
      c.PacienteID                            AS PacienteID,
      CAST(NULL AS NVARCHAR(150))             AS PacienteNombre,   
      c.DoctorID                              AS DoctorID,
      CAST(NULL AS NVARCHAR(150))             AS DoctorNombre,     
      CAST(c.FechaHora AS DATETIME2)          AS FechaHora,
      c.Estado                                AS Estado,
      c.MotivoConsulta                        AS Motivo,
      CAST(NULL AS NVARCHAR(150))             AS Email,
      CAST(NULL AS NVARCHAR(50))              AS Telefono,
      CAST(c.FechaCreacion AS DATETIME2)      AS FechaRegistro
  FROM Citas c

  UNION ALL

  SELECT
      CAST(N'Solicitud' AS NVARCHAR(10))      AS Tipo,
      CAST(NULL AS INT)                       AS CitaID,           
      cp.Id                                   AS SolicitudID,
      CAST(NULL AS INT)                       AS PacienteID,
      cp.Nombre                               AS PacienteNombre,
      CAST(NULL AS INT)                       AS DoctorID,
      cp.DoctorNombre                         AS DoctorNombre,
      CAST(cp.FechaHoraPreferida AS DATETIME2)AS FechaHora,
      CAST(N'Pendiente' AS NVARCHAR(30))      AS Estado,           
      cp.Mensaje                               AS Motivo,
      cp.Email                                 AS Email,
      cp.Telefono                              AS Telefono,
      CAST(cp.FechaSolicitud AS DATETIME2)     AS FechaRegistro
  FROM CitasPublicas cp

  ORDER BY FechaHora DESC;
END
GO

-- CREAR
CREATE OR ALTER PROCEDURE Cita_Crear
  @PacienteID INT,
  @DoctorID INT,
  @FechaHora DATETIME2,
  @Estado NVARCHAR(20),
  @MotivoConsulta NVARCHAR(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO Citas(PacienteID,DoctorID,FechaHora,Estado,MotivoConsulta,FechaCreacion)
  VALUES(@PacienteID,@DoctorID,@FechaHora,@Estado,@MotivoConsulta,SYSUTCDATETIME());
  SELECT CAST(SCOPE_IDENTITY() AS INT) AS CitaID;
END
GO

-- ACTUALIZAR
CREATE OR ALTER PROCEDURE Cita_Actualizar
  @CitaID INT,
  @PacienteID INT,
  @DoctorID INT,
  @FechaHora DATETIME2,
  @Estado NVARCHAR(20),
  @MotivoConsulta NVARCHAR(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE Citas
    SET PacienteID=@PacienteID,
        DoctorID=@DoctorID,
        FechaHora=@FechaHora,
        Estado=@Estado,
        MotivoConsulta=@MotivoConsulta
  WHERE CitaID=@CitaID;
  SELECT @@ROWCOUNT AS Afectados;
END
GO

-- ELIMINAR
CREATE OR ALTER PROCEDURE Cita_Eliminar
  @CitaID INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM Citas WHERE CitaID=@CitaID;
  SELECT @@ROWCOUNT AS Afectados;
END
GO

-- OBTENER
CREATE OR ALTER PROCEDURE Cita_Obtener
  @CitaID INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT CitaID, PacienteID, DoctorID, FechaHora, Estado, MotivoConsulta, FechaCreacion
  FROM Citas WHERE CitaID=@CitaID;
END
GO

--comsultar doctores:
CREATE PROCEDURE ConsultarDoctores
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.DoctorID,
        d.UsuarioID,
        u.NombreCompleto,
        d.Especialidad,
        d.CedulaProfesional
    FROM Doctores d
    INNER JOIN Usuarios u ON d.UsuarioID = u.UsuarioID;
END;
GO

--carga los nombres de los usuarios para popular dropdowns:
CREATE PROCEDURE dbo.ConsultarUsuariosDropdown
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UsuarioID, NombreCompleto
    FROM Usuarios
    ORDER BY NombreCompleto;
END;

-- ATENDER: crea cita y elimina la solicitud
CREATE OR ALTER PROCEDURE CitaPublica_Atender
  @SolicitudID BIGINT,
  @PacienteID INT,
  @DoctorID INT,
  @FechaHora DATETIME2,
  @Estado NVARCHAR(20) = 'Pendiente',
  @MotivoConsulta NVARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  BEGIN TRY
    BEGIN TRAN;

    DECLARE @Motivo NVARCHAR(MAX);
    SELECT @Motivo = ISNULL(@MotivoConsulta, Mensaje)
    FROM CitasPublicas WHERE Id = @SolicitudID;

    INSERT INTO Citas(PacienteID,DoctorID,FechaHora,Estado,MotivoConsulta,FechaCreacion)
    VALUES(@PacienteID,@DoctorID,@FechaHora,@Estado,@Motivo,SYSUTCDATETIME());

    DECLARE @NewCitaID INT = CAST(SCOPE_IDENTITY() AS INT);

    DELETE FROM CitasPublicas WHERE Id = @SolicitudID;

    COMMIT;
    SELECT @NewCitaID AS CitaID;
  END TRY
  BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    THROW;
  END CATCH
END
GO

--Actualiar doctor
CREATE OR ALTER PROCEDURE [dbo].[ActualizarDoctor]
    @DoctorID INT,
    @Especialidad VARCHAR(100),
    @CedulaProfesional VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Doctores
    SET         
        Especialidad = @Especialidad,
        CedulaProfesional = @CedulaProfesional
    WHERE DoctorID = @DoctorID;
END

UPDATE Doctores
SET Especialidad = 'Prueba',
    CedulaProfesional = '123456777'
WHERE DoctorID = 1;




-- DESCARTAR solicitud
CREATE OR ALTER PROCEDURE CitaPublica_Eliminar
  @SolicitudID BIGINT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM CitasPublicas WHERE Id=@SolicitudID;
  SELECT @@ROWCOUNT AS Afectados;
END
GO

-- Rol Doctor
DECLARE @RolDoctor INT = (SELECT RolID FROM Roles WHERE NombreRol = 'Doctor');

-- 1) Dra. Laura Gómez — Medicina General
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1001', 'Laura Gómez', 'laura.gomez@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U1 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U1, 'Medicina General', 'MG-001');

-- 2) Dr. Ricardo Solís — Cardiología
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1002', 'Ricardo Solís', 'ricardo.solis@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U2 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U2, 'Cardiología', 'CAR-002');

-- 3) Dra. Sofía Herrera — Pediatría
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1003', 'Sofía Herrera', 'sofia.herrera@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U3 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U3, 'Pediatría', 'PED-003');

-- 4) Dr. Andrés Mora — Ortopedia
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1004', 'Andrés Mora', 'andres.mora@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U4 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U4, 'Ortopedia', 'ORT-004');

-- 5) Dra. Paula Rojas — Ginecología
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1005', 'Paula Rojas', 'paula.rojas@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U5 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U5, 'Ginecología', 'GIN-005');

-- 6) Dr. Diego Campos — Dermatología
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('DOC-1006', 'Diego Campos', 'diego.campos@healthylife.test', 'temp', @RolDoctor, 1);
DECLARE @U6 INT = SCOPE_IDENTITY();
INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
VALUES (@U6, 'Dermatología', 'DER-006');

