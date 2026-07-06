-- =============================================
-- GoalDB - Create Database & Tables
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'GoalDB')
BEGIN
    CREATE DATABASE GoalDB;
END
GO

USE GoalDB;
GO

-- =============================================
-- 1. GoalCategories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoalCategories')
BEGIN
    CREATE TABLE GoalCategories (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Name        NVARCHAR(100)       NOT NULL,
        IsActive    BIT                 NOT NULL DEFAULT 1,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime DATETIME2           NULL,

        CONSTRAINT PK_GoalCategories PRIMARY KEY (Id),
        CONSTRAINT UQ_GoalCategories_Name UNIQUE (Name)
    );
END
GO

-- =============================================
-- 2. GoalTemplates
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoalTemplates')
BEGIN
    CREATE TABLE GoalTemplates (
        Id             UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        GoalCategoryId UNIQUEIDENTIFIER    NOT NULL,
        Title          NVARCHAR(200)       NOT NULL,
        Description    NVARCHAR(1000)      NULL,
        GoalType       NVARCHAR(20)        NOT NULL,
        -- Completion | Metric | Counter
        IsActive       BIT                 NOT NULL DEFAULT 1,
        CreatedTime    DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime    DATETIME2           NULL,

        CONSTRAINT PK_GoalTemplates PRIMARY KEY (Id),
        CONSTRAINT FK_GoalTemplates_GoalCategories FOREIGN KEY (GoalCategoryId)
            REFERENCES GoalCategories(Id),
        CONSTRAINT CK_GoalTemplates_GoalType CHECK (GoalType IN ('Completion', 'Metric', 'Counter'))
    );
END
GO

-- =============================================
-- 3. GoalAssignments
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoalAssignments')
BEGIN
    CREATE TABLE GoalAssignments (
        Id             UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        GoalTemplateId UNIQUEIDENTIFIER    NOT NULL,
        UserId         UNIQUEIDENTIFIER    NOT NULL,
        -- FK yok: cross-DB, uygulama seviyesinde yönetilir (IdentityDB)
        PeriodId       UNIQUEIDENTIFIER    NOT NULL,
        -- FK yok: cross-DB, uygulama seviyesinde yönetilir (ParameterDB)
        Status         NVARCHAR(20)        NOT NULL DEFAULT 'NotStarted',
        -- NotStarted | InProgress | Completed | Expired | Cancelled
        IsActive       BIT                 NOT NULL DEFAULT 1,
        CreatedTime    DATETIME2           NOT NULL DEFAULT GETDATE(),
        UpdatedTime    DATETIME2           NULL,

        CONSTRAINT PK_GoalAssignments PRIMARY KEY (Id),
        CONSTRAINT FK_GoalAssignments_GoalTemplates FOREIGN KEY (GoalTemplateId)
            REFERENCES GoalTemplates(Id),
        CONSTRAINT CK_GoalAssignments_Status CHECK (Status IN ('NotStarted', 'InProgress', 'Completed', 'Expired', 'Cancelled'))
    );
END
GO

-- =============================================
-- 4. GoalComments
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoalComments')
BEGIN
    CREATE TABLE GoalComments (
        Id               UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        GoalAssignmentId UNIQUEIDENTIFIER    NOT NULL,
        UserId           UNIQUEIDENTIFIER    NOT NULL,
        -- FK yok: cross-DB, uygulama seviyesinde yönetilir (IdentityDB)
        Content          NVARCHAR(2000)      NOT NULL,
        CreatedTime      DATETIME2           NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_GoalComments PRIMARY KEY (Id),
        CONSTRAINT FK_GoalComments_GoalAssignments FOREIGN KEY (GoalAssignmentId)
            REFERENCES GoalAssignments(Id)
    );
END
GO

-- =============================================
-- Seed: Varsayılan kategoriler
-- =============================================
IF NOT EXISTS (SELECT 1 FROM GoalCategories WHERE Name = 'Teknik')
BEGIN
    INSERT INTO GoalCategories (Id, Name, IsActive, CreatedTime)
    VALUES
        (NEWID(), N'Teknik',            1, GETDATE()),
        (NEWID(), N'Kişisel Gelişim',   1, GETDATE()),
        (NEWID(), N'Kalite',            1, GETDATE()),
        (NEWID(), N'Takım Çalışması',   1, GETDATE());
END
GO

-- =============================================
-- Doğrulama
-- =============================================
SELECT 'GoalCategories'  AS TableName, COUNT(*) AS [RowCount] FROM GoalCategories
UNION ALL
SELECT 'GoalTemplates',  COUNT(*) FROM GoalTemplates
UNION ALL
SELECT 'GoalAssignments', COUNT(*) FROM GoalAssignments
UNION ALL
SELECT 'GoalComments',   COUNT(*) FROM GoalComments;
GO
