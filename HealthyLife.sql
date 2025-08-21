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

select * from Usuarios
DELETE FROM doctores
where DoctorID = 11
-- Inserts Roles --
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

--Tabla: Facturacion
CREATE TABLE Facturas (
    FacturaID INT PRIMARY KEY IDENTITY(1,1),
    CitaID INT NOT NULL,
    FechaEmision DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    EstadoPago VARCHAR(20) DEFAULT 'Pendiente', -- Pagado, Pendiente, Cancelado
    FOREIGN KEY (CitaID) REFERENCES Citas(CitaID)
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

--Tabla Errores
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

-- Tabla CitasPublicas
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

--- ALTERS

GO
ALTER TABLE Pacientes ADD UsuarioID INT NULL;

ALTER TABLE Pacientes ALTER COLUMN UsuarioID INT NOT NULL;
ALTER TABLE Pacientes
  ADD CONSTRAINT FK_Pacientes_Usuarios
  FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID);

ALTER TABLE dbo.Pacientes ADD NombreCompleto VARCHAR(100) NULL;
ALTER TABLE dbo.Pacientes ADD Cedula VARCHAR(50) NULL;

-- Procedimeintos --

-- Registrar Usuario --
GO
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

-- Iniciar sesion--
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
CREATE OR ALTER PROCEDURE CitasPorUsuario
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.CitaID,
        c.PacienteID,
        c.DoctorID,
        c.FechaHora,
        c.Estado,
        c.MotivoConsulta,
        c.FechaCreacion
    FROM Citas c
    INNER JOIN Pacientes p ON p.PacienteID = c.PacienteID
    WHERE p.UsuarioID = @UsuarioID
    ORDER BY c.FechaHora DESC;
END



-- Validar Correo --
GO
CREATE OR ALTER PROCEDURE ValidarCorreo
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


-- ActualizarContrasena --
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


-- ActualizarUsuario --
GO
CREATE OR ALTER PROCEDURE [dbo].[ActualizarUsuario]
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

CREATE OR ALTER PROCEDURE ActualizarContrasenna
	@IdUsuario bigint,
	@ContrasenaHash varchar(255)
AS
BEGIN
	UPDATE	Usuarios
		SET	ContrasenaHash = @ContrasenaHash
		WHERE IdUsuario = @IdUsuario
	
END;
GO	

-- ConsultarUsuario --
GO
CREATE OR ALTER PROCEDURE ConsultarUsuario
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

EXEC ConsultarUsuario 22

-- ConsultarUsuario --
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


-- ActualizarUsuario --
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


-- Errores --

-- RegistrarError --
GO

CREATE OR ALTER PROCEDURE RegistrarError
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


-- ConsultarHistorialPorPaciente --
GO
CREATE OR ALTER PROCEDURE ConsultarHistorialPorPaciente
    @PacienteID INT
AS
BEGIN
    SELECT h.HistorialID, h.FechaRegistro, d.NombreCompleto AS NombreDoctor, diag.Descripcion
    FROM HistorialMedico h
    JOIN Doctores d ON d.DoctorID = h.DoctorID
    JOIN Diagnosticos diag ON diag.DiagnosticoID = h.DiagnosticoID
    WHERE h.PacienteID = @PacienteID
END

-- RegistrarHistorial --
GO
CREATE OR ALTER PROCEDURE RegistrarHistorial
    @PacienteID INT,
    @DoctorID INT,
    @DiagnosticoID INT
AS
BEGIN
    INSERT INTO HistorialMedico (PacienteID, DoctorID, DiagnosticoID, FechaRegistro)
    VALUES (@PacienteID, @DoctorID, @DiagnosticoID, GETDATE())
END

-- CrearFactura --
GO
CREATE OR ALTER PROCEDURE CrearFactura
    @CitaID INT,
    @Total DECIMAL(10,2),
    @EstadoPago VARCHAR(20)
AS
BEGIN
    INSERT INTO Facturas (CitaID, Total, EstadoPago, FechaEmision)
    VALUES (@CitaID, @Total, @EstadoPago, GETDATE())
END
GO
CREATE OR ALTER PROCEDURE ConsultarFactura
    @FacturaID INT
AS
BEGIN
    SELECT f.FacturaID, f.Total, f.EstadoPago, f.FechaEmision, c.PacienteID, p.NombreCompleto AS Paciente
    FROM Facturas f
    JOIN Citas c ON c.CitaID = f.CitaID
    JOIN Pacientes p ON p.PacienteID = c.PacienteID
    WHERE f.FacturaID = @FacturaID
END

--Registra un doctor
GO
CREATE OR ALTER   PROCEDURE RegistrarDoctor
    @UsuarioID int,
    @Especialidad VARCHAR(100),
    @CedulaProfesional VARCHAR(20)    
AS
BEGIN
	INSERT INTO Doctores (UsuarioID, Especialidad, CedulaProfesional)
    VALUES (@UsuarioID, @Especialidad, @CedulaProfesional);
    
END

--Registra una cita--
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

--Registra un paciente--
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


-- RegistrarCitaPublica --
GO
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


-- Cita_Listar_Unificada --
GO
CREATE OR ALTER PROCEDURE Cita_Listar_Unificada
AS
BEGIN
  SET NOCOUNT ON;

  -- Citas (contacto desde Pacientes)
  SELECT
      'Cita'                         AS Tipo,
      c.CitaID,
      CAST(NULL AS bigint)          AS SolicitudID,
      c.FechaHora,
      c.FechaCreacion               AS FechaRegistro,
      p.PacienteID,
      p.NombreCompleto              AS PacienteNombre,
      d.DoctorID,
      u.NombreCompleto              AS DoctorNombre,
      c.Estado,
      c.MotivoConsulta              AS Motivo,
      p.CorreoElectronico           AS Email,
      p.Telefono                    AS Telefono
  FROM Citas c
  JOIN Pacientes p        ON p.PacienteID = c.PacienteID
  LEFT JOIN Doctores d    ON d.DoctorID   = c.DoctorID
  LEFT JOIN Usuarios u    ON u.UsuarioID  = d.UsuarioID

  UNION ALL

  -- Solicitudes públicas (contacto viene de la propia solicitud)
  SELECT
      'Solicitud'                    AS Tipo,
      CAST(NULL AS int)              AS CitaID,
      s.Id                           AS SolicitudID,
      s.FechaHoraPreferida           AS FechaHora,
      s.FechaSolicitud               AS FechaRegistro,
      CAST(NULL AS int)              AS PacienteID,
      s.Nombre                       AS PacienteNombre,
      CAST(NULL AS int)              AS DoctorID,
      s.DoctorNombre                 AS DoctorNombre,
      'Pendiente'                    AS Estado,
      s.Mensaje                      AS Motivo,
      s.Email                        AS Email,
      s.Telefono                     AS Telefono
  FROM CitasPublicas s

  ORDER BY FechaHora DESC;
END


-- Cita_Crear --
GO
CREATE OR ALTER PROCEDURE Cita_Crear
  @PacienteID      INT,
  @DoctorID        INT,
  @FechaHora       DATETIME2(0),
  @Estado          NVARCHAR(50),
  @MotivoConsulta  NVARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  IF NOT EXISTS (SELECT 1 FROM Pacientes WHERE PacienteID = @PacienteID)
  BEGIN
    RAISERROR('Paciente no existe', 16, 1);
    RETURN;
  END

  IF NOT EXISTS (SELECT 1 FROM Doctores WHERE DoctorID = @DoctorID)
  BEGIN
    RAISERROR('Doctor no existe', 16, 1);
    RETURN;
  END

  INSERT INTO Citas(PacienteID, DoctorID, FechaHora, Estado, MotivoConsulta, FechaCreacion)
  VALUES (@PacienteID, @DoctorID, @FechaHora, @Estado, @MotivoConsulta, SYSDATETIME());

  SELECT CAST(SCOPE_IDENTITY() AS INT) AS CitaID;
END


-- Cita_Actualizar --
GO
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


-- Cita_Eliminar --
GO
CREATE OR ALTER PROCEDURE Cita_Eliminar
  @CitaID INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM Citas WHERE CitaID=@CitaID;
  SELECT @@ROWCOUNT AS Afectados;
END


-- Cita_Obtener --
GO
CREATE OR ALTER PROCEDURE Cita_Obtener
  @CitaID INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT CitaID, PacienteID, DoctorID, FechaHora, Estado, MotivoConsulta, FechaCreacion
  FROM Citas WHERE CitaID=@CitaID;
END


--ConsultarDoctores--
GO
CREATE OR ALTER PROCEDURE ConsultarDoctores
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


--carga los nombres de los usuarios para popular dropdowns:
GO
CREATE OR ALTER PROCEDURE dbo.ConsultarUsuariosDropdown
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UsuarioID, NombreCompleto
    FROM Usuarios
    ORDER BY NombreCompleto;
END;

-- ATENDER: crea cita y elimina la solicitud --
GO
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


--Actualizar doctor
GO
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




-- DESCARTAR solicitud --
GO
CREATE OR ALTER PROCEDURE CitaPublica_Eliminar
  @SolicitudID BIGINT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM CitasPublicas WHERE Id=@SolicitudID;
  SELECT @@ROWCOUNT AS Afectados;
END


-- Lista simple de pacientes (Usuarios con rol 'Usuario')
DROP PROCEDURE IF EXISTS Paciente_ListaSimple;

-- Paciente_ListaSimple --
GO
CREATE OR ALTER PROCEDURE Paciente_ListaSimple
AS
BEGIN
  SET NOCOUNT ON;

  SELECT 
    p.PacienteID,
    u.Cedula,
    u.NombreCompleto,
    u.CorreoElectronico
  FROM Pacientes p
  JOIN Usuarios  u ON u.UsuarioID = p.UsuarioID
  WHERE u.Activo = 1
  ORDER BY u.NombreCompleto;
END
GO



-- Buscar paciente por email y/o cédula
DROP PROCEDURE IF EXISTS Paciente_Buscar;
GO
CREATE OR ALTER PROCEDURE Paciente_Buscar
  @Email  VARCHAR(100) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  IF (@Email IS NULL)
  BEGIN
    RAISERROR('Debe enviar @Email', 16, 1);
    RETURN;
  END

  SELECT TOP 1
         PacienteID,
         NombreCompleto,
         CorreoElectronico,
         Telefono,
         '' AS Cedula
  FROM Pacientes
  WHERE CorreoElectronico = @Email
  ORDER BY PacienteID;
END

-- Paciente_Crear --
GO
CREATE OR ALTER PROCEDURE Paciente_Crear
  @Cedula           VARCHAR(50),
  @NombreCompleto   VARCHAR(100),
  @CorreoElectronico VARCHAR(100),
  @ContrasenaHash   VARCHAR(255) = NULL,   -- si no mandan, pones algo temporal
  @Telefono         VARCHAR(20)  = NULL,
  @FechaNacimiento  DATE         = NULL,
  @Genero           VARCHAR(10)  = NULL,
  @Direccion        VARCHAR(200) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  IF @FechaNacimiento IS NULL SET @FechaNacimiento = '1990-01-01';
  IF @ContrasenaHash IS NULL SET @ContrasenaHash = 'temporal';

  DECLARE @RolUsuario INT = (SELECT RolID FROM Roles WHERE NombreRol='Usuario');

  BEGIN TRAN;

    DECLARE @UsuarioID INT;

    INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
    VALUES (@Cedula, @NombreCompleto, @CorreoElectronico, @ContrasenaHash, @RolUsuario, 1);

    SET @UsuarioID = SCOPE_IDENTITY();

    INSERT INTO Pacientes (UsuarioID, FechaNacimiento, Genero, Direccion, Telefono, CorreoElectronico, NombreCompleto)
    VALUES (@UsuarioID, @FechaNacimiento, @Genero, @Direccion, @Telefono, @CorreoElectronico, @NombreCompleto);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS PacienteID;

  COMMIT;
END

-- Listar roles
GO
CREATE OR ALTER PROCEDURE dbo.Roles_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolID, NombreRol
    FROM Roles
    ORDER BY RolID;
END


-- Listar Usuarios
GO
CREATE OR ALTER PROCEDURE dbo.Usuario_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.UsuarioID,
        u.Cedula,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.RolID,
        r.NombreRol,
        u.Activo,
        u.FechaRegistro
    FROM dbo.Usuarios u WITH (NOLOCK)
    INNER JOIN dbo.Roles r WITH (NOLOCK) ON r.RolID = u.RolID
    ORDER BY u.UsuarioID DESC;
END


--Obtener usuarios
GO
CREATE OR ALTER PROCEDURE dbo.Usuario_Obtener
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        u.UsuarioID,
        u.Cedula,
        u.NombreCompleto,
        u.CorreoElectronico,
        u.RolID,
        r.NombreRol,
        u.Activo,
        u.FechaRegistro
    FROM Usuarios u
    INNER JOIN Roles r ON r.RolID = u.RolID
    WHERE u.UsuarioID = @UsuarioID;
END


--Crear usuario
GO
CREATE OR ALTER PROCEDURE dbo.Usuario_Crear
    @Cedula           VARCHAR(50),
    @NombreCompleto   VARCHAR(100),
    @CorreoElectronico VARCHAR(100),
    @ContrasenaHash   VARCHAR(255),
    @RolID            INT,
    @Activo           BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS (SELECT 1 FROM Usuarios WHERE Cedula = @Cedula)
            RAISERROR('La cédula ya existe.', 16, 1);

        IF EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico)
            RAISERROR('El correo electrónico ya existe.', 16, 1);

        INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
        VALUES (@Cedula, @NombreCompleto, @CorreoElectronico, @ContrasenaHash, @RolID, @Activo);

        DECLARE @NewID INT = SCOPE_IDENTITY();

        COMMIT;
        SELECT @NewID AS UsuarioID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;

        IF ERROR_NUMBER() IN (2601, 2627)
            RAISERROR('La cédula o el correo ya existe.', 16, 1);
        ELSE
            THROW;
    END CATCH
END



-- Actualizar usuario
GO
CREATE OR ALTER PROCEDURE dbo.Usuario_Actualizar
    @UsuarioID        INT,
    @Cedula           VARCHAR(50),
    @NombreCompleto   VARCHAR(100),
    @CorreoElectronico VARCHAR(100),
    @RolID            INT,
    @Activo           BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UsuarioID = @UsuarioID)
            RAISERROR('El usuario no existe.', 16, 1);

        IF EXISTS (SELECT 1 FROM Usuarios WHERE Cedula = @Cedula AND UsuarioID <> @UsuarioID)
            RAISERROR('La cédula ya existe.', 16, 1);

        IF EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico AND UsuarioID <> @UsuarioID)
            RAISERROR('El correo electrónico ya existe.', 16, 1);

        UPDATE Usuarios
        SET Cedula = @Cedula,
            NombreCompleto = @NombreCompleto,
            CorreoElectronico = @CorreoElectronico,
            RolID = @RolID,
            Activo = @Activo
        WHERE UsuarioID = @UsuarioID;

        COMMIT;
        SELECT 1 AS Ok;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;

        IF ERROR_NUMBER() IN (2601, 2627)
            RAISERROR('La cédula o el correo ya existe.', 16, 1);
        ELSE
            THROW;
    END CATCH
END


-- Password reset
GO
CREATE OR ALTER PROCEDURE dbo.Usuario_ResetPassword
    @UsuarioID        INT,
    @ContrasenaHash   VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UsuarioID = @UsuarioID)
        RAISERROR('El usuario no existe.', 16, 1);

    UPDATE Usuarios
    SET ContrasenaHash = @ContrasenaHash
    WHERE UsuarioID = @UsuarioID;

    SELECT 1 AS Ok;
END



-- Doctor_ListaSimple --
GO
CREATE OR ALTER PROCEDURE Doctor_ListaSimple
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.DoctorID,
        u.NombreCompleto AS NombreCompleto, 
        d.Especialidad
    FROM Doctores d
    INNER JOIN Usuarios u ON u.UsuarioID = d.UsuarioID
    WHERE u.Activo = 1
    ORDER BY u.NombreCompleto;
END

-- Paciente_ToggleEstado --
GO
CREATE OR ALTER PROCEDURE dbo.Paciente_ToggleEstado
  @PacienteID INT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @UsuarioID INT = (SELECT UsuarioID FROM Pacientes WHERE PacienteID=@PacienteID);
  IF @UsuarioID IS NULL
  BEGIN
    RAISERROR('Paciente no existe.',16,1);
    RETURN;
  END

  UPDATE Usuarios
     SET Activo = CASE WHEN Activo=1 THEN 0 ELSE 1 END
   WHERE UsuarioID=@UsuarioID;

  SELECT Activo FROM Usuarios WHERE UsuarioID=@UsuarioID;
END

-- Paciente_Listar --
GO
CREATE OR ALTER PROCEDURE dbo.Paciente_Listar
  @q VARCHAR(100) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SELECT 
    p.PacienteID,
    p.UsuarioID,
    COALESCE(u.Cedula, p.Cedula)           AS Cedula,
    COALESCE(u.NombreCompleto, p.NombreCompleto) AS NombreCompleto,
    COALESCE(u.CorreoElectronico, p.CorreoElectronico) AS CorreoElectronico,
    p.Telefono,
    p.FechaNacimiento,
    p.Genero,
    p.Direccion,
    u.Activo
  FROM Pacientes p
  JOIN Usuarios  u ON u.UsuarioID = p.UsuarioID
  WHERE @q IS NULL OR
        u.Cedula           LIKE '%'+@q+'%' OR
        u.NombreCompleto   LIKE '%'+@q+'%' OR
        u.CorreoElectronico LIKE '%'+@q+'%'
  ORDER BY u.NombreCompleto;
END


-- Paciente_Obtener --
GO
CREATE OR ALTER PROCEDURE dbo.Paciente_Obtener
  @PacienteID INT
AS
BEGIN
  SET NOCOUNT ON;

  SELECT TOP 1
    p.PacienteID,
    p.UsuarioID,
    COALESCE(u.Cedula, p.Cedula)           AS Cedula,
    COALESCE(u.NombreCompleto, p.NombreCompleto) AS NombreCompleto,
    COALESCE(u.CorreoElectronico, p.CorreoElectronico) AS CorreoElectronico,
    p.Telefono,
    p.FechaNacimiento,
    p.Genero,
    p.Direccion,
    u.Activo
  FROM Pacientes p
  JOIN Usuarios  u ON u.UsuarioID = p.UsuarioID
  WHERE p.PacienteID = @PacienteID;
END

--Agarra el id de usuario > lo busca en paciente > con paciente obtiene las citas
GO
CREATE OR ALTER PROCEDURE CitasPorUsuarioUnificada
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.CitaID,
        c.FechaHora,
        c.Estado,
        c.MotivoConsulta,
        c.FechaCreacion,
        p.PacienteID,
        p.NombreCompleto AS NombrePaciente,
        d.DoctorID,
        u.NombreCompleto AS NombreDoctor
    FROM Citas c
    INNER JOIN Pacientes p ON p.PacienteID = c.PacienteID
    LEFT JOIN Doctores d ON d.DoctorID = c.DoctorID
    LEFT JOIN Usuarios u ON u.UsuarioID = d.UsuarioID
    WHERE p.UsuarioID = @UsuarioID
    ORDER BY c.FechaHora DESC;
END

EXEC CitasPorUsuarioUnificada @UsuarioID = 4;

-- Paciente_Actualizar --
GO
CREATE OR ALTER PROCEDURE dbo.Paciente_Actualizar
  @PacienteID       INT,
  @Cedula           VARCHAR(50),
  @NombreCompleto   VARCHAR(100),
  @CorreoElectronico VARCHAR(100),
  @Telefono         VARCHAR(20)  = NULL,
  @FechaNacimiento  DATE         = NULL,
  @Genero           VARCHAR(10)  = NULL,
  @Direccion        VARCHAR(200) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  BEGIN TRAN;

    DECLARE @UsuarioID INT = (SELECT UsuarioID FROM Pacientes WHERE PacienteID = @PacienteID);
    IF @UsuarioID IS NULL
    BEGIN
      RAISERROR('Paciente no existe.',16,1);
      ROLLBACK TRAN;
      RETURN;
    END

    UPDATE Usuarios
      SET Cedula=@Cedula, NombreCompleto=@NombreCompleto, CorreoElectronico=@CorreoElectronico
    WHERE UsuarioID=@UsuarioID;

    UPDATE Pacientes
      SET Telefono=@Telefono,
          FechaNacimiento=@FechaNacimiento,
          Genero=@Genero,
          Direccion=@Direccion,
          Cedula=@Cedula,
          NombreCompleto=@NombreCompleto,
          CorreoElectronico=@CorreoElectronico
    WHERE PacienteID=@PacienteID;

  COMMIT;
END

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

-- Pacientes de prueba (rol Usuario)
INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('1-1012-0345', 'María Fernández', 'maria.fernandez@healthylife.test', 'temp', (SELECT RolID FROM Roles WHERE NombreRol='Usuario'), 1);

INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('2-3456-7890', 'José Ramírez', 'jose.ramirez@healthylife.test', 'temp', (SELECT RolID FROM Roles WHERE NombreRol='Usuario'), 1);

INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('4-1122-3344', 'Daniela Quesada', 'daniela.quesada@healthylife.test', 'temp', (SELECT RolID FROM Roles WHERE NombreRol='Usuario'), 1);

INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('5-5566-7788', 'Luis Álvarez', 'luis.alvarez@healthylife.test', 'temp', (SELECT RolID FROM Roles WHERE NombreRol='Usuario'), 1);

INSERT INTO Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
VALUES ('7-0199-3456', 'Carmen Rojas', 'carmen.rojas@healthylife.test', 'temp', (SELECT RolID FROM Roles WHERE NombreRol='Usuario'), 1);


------- Fixes en pacientes y usuarios ------- 

ALTER TABLE dbo.Pacientes
ADD CONSTRAINT DF_Pacientes_FechaNacimiento
DEFAULT ('1990-01-01') FOR FechaNacimiento;


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Pacientes_UsuarioID' AND object_id = OBJECT_ID('dbo.Pacientes'))
    CREATE UNIQUE INDEX UX_Pacientes_UsuarioID ON dbo.Pacientes(UsuarioID);
GO

CREATE OR ALTER TRIGGER dbo.trg_Usuarios_AfterInsert_AutoPaciente
ON dbo.Usuarios
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RolUsuario INT = (SELECT RolID FROM Roles WHERE NombreRol = 'Usuario');

    INSERT INTO dbo.Pacientes (UsuarioID, NombreCompleto, CorreoElectronico, Telefono, Direccion, Genero)
    SELECT  i.UsuarioID,
            i.NombreCompleto,
            i.CorreoElectronico,
            NULL, NULL, NULL
    FROM inserted i
    LEFT JOIN dbo.Pacientes p ON p.UsuarioID = i.UsuarioID
    WHERE i.RolID = @RolUsuario
      AND p.PacienteID IS NULL;
END
GO


CREATE OR ALTER PROCEDURE dbo.Paciente_Crear
  @Cedula            VARCHAR(50),
  @NombreCompleto    VARCHAR(100),
  @CorreoElectronico VARCHAR(100),
  @ContrasenaHash    VARCHAR(255) = NULL,
  @Telefono          VARCHAR(20)  = NULL,
  @FechaNacimiento   DATE         = NULL,
  @Genero            VARCHAR(10)  = NULL,
  @Direccion         VARCHAR(200) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  IF @FechaNacimiento IS NULL SET @FechaNacimiento = '1990-01-01';
  IF @ContrasenaHash IS NULL SET @ContrasenaHash = 'temporal';

  DECLARE @RolUsuario INT = (SELECT RolID FROM Roles WHERE NombreRol='Usuario');
  DECLARE @UsuarioID  INT;
  DECLARE @PacienteID INT;

  BEGIN TRAN;


    SELECT @UsuarioID = UsuarioID
    FROM dbo.Usuarios
    WHERE CorreoElectronico = @CorreoElectronico
       OR Cedula = @Cedula;

 
    IF @UsuarioID IS NULL
    BEGIN
      INSERT INTO dbo.Usuarios (Cedula, NombreCompleto, CorreoElectronico, ContrasenaHash, RolID, Activo)
      VALUES (@Cedula, @NombreCompleto, @CorreoElectronico, @ContrasenaHash, @RolUsuario, 1);

      SET @UsuarioID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
      -- opcional: mantener datos alineados
      UPDATE dbo.Usuarios
         SET Cedula = @Cedula,
             NombreCompleto = @NombreCompleto,
             CorreoElectronico = @CorreoElectronico
       WHERE UsuarioID = @UsuarioID;
    END

    -- 3) Asegurar Paciente
    IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE UsuarioID = @UsuarioID)
    BEGIN
      INSERT INTO dbo.Pacientes (UsuarioID, NombreCompleto, CorreoElectronico, Telefono, Direccion, Genero, FechaNacimiento)
      VALUES (@UsuarioID, @NombreCompleto, @CorreoElectronico, @Telefono, @Direccion, @Genero, @FechaNacimiento);
    END
    ELSE
    BEGIN
      UPDATE p
         SET p.NombreCompleto    = @NombreCompleto,
             p.CorreoElectronico = @CorreoElectronico,
             p.Telefono          = @Telefono,
             p.Direccion         = @Direccion,
             p.Genero            = @Genero,
             p.FechaNacimiento   = @FechaNacimiento
       FROM dbo.Pacientes p
       WHERE p.UsuarioID = @UsuarioID;
    END

    SELECT @PacienteID = PacienteID
    FROM dbo.Pacientes
    WHERE UsuarioID = @UsuarioID;

  COMMIT;

  SELECT @PacienteID AS PacienteID;
END

--Consultar Facturas
GO
CREATE OR ALTER PROCEDURE [dbo].[ConsultarFacturas]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        FacturaID,
        CitaID,
        FechaEmision,
        Total,
        EstadoPago
    FROM Facturas;
END;
GO

--Triggers:

-- Trigger para generar factura cuando una cita se completa
GO
CREATE OR ALTER TRIGGER Cita_Completada
ON Citas
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Facturas (CitaID, Total, EstadoPago)
    SELECT i.CitaID, 60000, 'Cancelado'
    FROM inserted i
    INNER JOIN deleted d ON i.CitaID = d.CitaID
    WHERE i.Estado = 'Completada' AND d.Estado <> 'Completada'
          AND NOT EXISTS (SELECT 1 FROM Facturas f WHERE f.CitaID = i.CitaID);
   
    UPDATE f
    SET f.Total = 60000,
        f.EstadoPago = 'Cancelado'
    FROM Facturas f
    INNER JOIN inserted i ON f.CitaID = i.CitaID
    INNER JOIN deleted d ON i.CitaID = d.CitaID
    WHERE i.Estado = 'Completada' AND d.Estado <> 'Completada';
END;
GO

CREATE PROCEDURE CargarUsuarios
AS
BEGIN
    SELECT 
        UsuarioID,
        NombreCompleto,
        Cedula,
        CorreoElectronico
    FROM Usuarios
	WHERE RolID = 1
    ORDER BY NombreCompleto;
END
GO





