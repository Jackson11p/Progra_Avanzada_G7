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
