-- =============================================
-- ParameterDB - Create Database & Tables
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ParameterDB')
BEGIN
    CREATE DATABASE ParameterDB;
END
GO

USE ParameterDB;
GO

-- =============================================
-- 1. Periods (Dönemler)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Periods')
BEGIN
    CREATE TABLE Periods (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Name        NVARCHAR(100)       NOT NULL,
        Type        NVARCHAR(20)        NOT NULL,
        -- Monthly | Quarterly | HalfYearly | Yearly
        StartDate   DATE                NOT NULL,
        EndDate     DATE                NOT NULL,
        IsClosed    BIT                 NOT NULL DEFAULT 0,
        IsActive    BIT                 NOT NULL DEFAULT 1,
        IsEnabled   BIT                 NOT NULL DEFAULT 1,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime DATETIME2           NULL,

        CONSTRAINT PK_Periods PRIMARY KEY (Id),
        CONSTRAINT UQ_Periods_Name UNIQUE (Name),
        CONSTRAINT CK_Periods_Type CHECK (Type IN ('Monthly', 'Quarterly', 'HalfYearly', 'Yearly')),
        CONSTRAINT CK_Periods_Dates CHECK (EndDate > StartDate)
    );
END
GO

-- =============================================
-- 2. Parameters (Sistem Parametreleri)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Parameters')
BEGIN
    CREATE TABLE Parameters (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Category    NVARCHAR(50)        NOT NULL DEFAULT 'System',
        -- System | Goal | Notification | UI
        [Key]       NVARCHAR(100)       NOT NULL,
        Value       NVARCHAR(500)       NOT NULL,
        Description NVARCHAR(250)       NULL,
        IsActive    BIT                 NOT NULL DEFAULT 1,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime DATETIME2           NULL,

        CONSTRAINT PK_Parameters PRIMARY KEY (Id),
        CONSTRAINT UQ_Parameters_Key UNIQUE ([Key]),
        CONSTRAINT CK_Parameters_Category CHECK (Category IN ('System', 'Goal', 'Notification', 'UI'))
    );
END
GO

-- =============================================
-- Seed: Varsayılan sistem parametreleri
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Parameters WHERE [Key] = 'DEFAULT_PERIOD_TYPE')
BEGIN
    INSERT INTO Parameters (Id, Category, [Key], Value, Description, IsActive, CreatedTime)
    VALUES
        (NEWID(), 'System', 'DEFAULT_PERIOD_TYPE',    'Quarterly', 'Varsayılan dönem tipi',                      1, GETDATE()),
        (NEWID(), 'Goal',   'MAX_GOALS_PER_USER',     '10',        'Kullanıcı başına maksimum hedef sayısı',      1, GETDATE()),
        (NEWID(), 'UI',     'APP_NAME',               'Hedef Takip Sistemi', 'Uygulama adı',                     1, GETDATE());
END
GO

-- =============================================
-- Doğrulama
-- =============================================
SELECT 'Periods'    AS TableName, COUNT(*) AS [RowCount] FROM Periods
UNION ALL
SELECT 'Parameters', COUNT(*) FROM Parameters;
GO
