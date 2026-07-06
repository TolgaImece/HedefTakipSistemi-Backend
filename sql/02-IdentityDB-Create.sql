-- =============================================
-- IdentityDB - Create Database & Tables
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IdentityDB')
BEGIN
    CREATE DATABASE IdentityDB;
END
GO

USE IdentityDB;
GO

-- =============================================
-- 1. Departments
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Name        NVARCHAR(100)       NOT NULL,
        IsActive    BIT                 NOT NULL DEFAULT 1,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime DATETIME2           NULL,

        CONSTRAINT PK_Departments PRIMARY KEY (Id),
        CONSTRAINT UQ_Departments_Name UNIQUE (Name)
    );
END
GO

-- =============================================
-- 2. Positions
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions')
BEGIN
    CREATE TABLE Positions (
        Id           UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        DepartmentId UNIQUEIDENTIFIER    NOT NULL,
        Name         NVARCHAR(100)       NOT NULL,
        IsActive     BIT                 NOT NULL DEFAULT 1,
        CreatedTime  DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime  DATETIME2           NULL,

        CONSTRAINT PK_Positions PRIMARY KEY (Id),
        CONSTRAINT FK_Positions_Departments FOREIGN KEY (DepartmentId)
            REFERENCES Departments(Id)
    );
END
GO

-- =============================================
-- 3. Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id           UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        FirstName    NVARCHAR(100)       NOT NULL,
        LastName     NVARCHAR(100)       NOT NULL,
        Email        NVARCHAR(256)       NOT NULL,
        PasswordHash NVARCHAR(MAX)       NOT NULL,
        Role         NVARCHAR(20)        NOT NULL DEFAULT 'User',
        -- Admin: tüm sistem | Manager: kendi departmanı | User: kendi hedefleri
        DepartmentId UNIQUEIDENTIFIER    NULL,
        -- FK yok: uygulama seviyesinde yönetilir (cross-entity bağımsızlık)
        PositionId   UNIQUEIDENTIFIER    NULL,
        -- FK yok: uygulama seviyesinde yönetilir
        IsActive     BIT                 NOT NULL DEFAULT 1,
        CreatedTime  DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime  DATETIME2           NULL,

        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Manager', 'User'))
    );
END
GO

-- =============================================
-- 4. RefreshTokens
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        Token       NVARCHAR(MAX)       NOT NULL,
        ExpiresAt   DATETIME2           NOT NULL,
        IsRevoked   BIT                 NOT NULL DEFAULT 0,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId)
            REFERENCES Users(Id)
    );
END
GO

-- =============================================
-- Seed: Admin kullanıcı
-- BCrypt hash → Admin123!
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@hedeftakip.com')
BEGIN
    INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, Role, IsActive, CreatedTime)
    VALUES (
        NEWID(),
        N'Super',
        N'Admin',
        'admin@hedeftakip.com',
        '$2a$11$3SGoTMKmjyjuE8kLxTpEVOEL8jiuI8XfI/H.ZUdqu/QOZFEuJ8DES', -- Admin123!
        'Admin',
        1,
        GETDATE()
    );
END
GO

-- =============================================
-- Doğrulama
-- =============================================
SELECT 'Departments'   AS TableName, COUNT(*) AS [RowCount] FROM Departments
UNION ALL
SELECT 'Positions',    COUNT(*) FROM Positions
UNION ALL
SELECT 'Users',        COUNT(*) FROM Users
UNION ALL
SELECT 'RefreshTokens', COUNT(*) FROM RefreshTokens;
GO
