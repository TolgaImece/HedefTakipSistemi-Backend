-- =============================================
-- AuditDB - Create Database & Tables
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AuditDB')
BEGIN
    CREATE DATABASE AuditDB;
END
GO

USE AuditDB;
GO

-- =============================================
-- 1. AuditLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        ServiceName NVARCHAR(50)        NOT NULL,
        -- IdentityService | ParameterService | GoalService
        Action      NVARCHAR(50)        NOT NULL,
        -- Create | Update | Delete | Login | Logout | StatusChange | ChangePassword
        EntityName  NVARCHAR(100)       NULL,
        EntityId    NVARCHAR(100)       NULL,
        -- GUID string — cross-DB referans
        UserId      NVARCHAR(100)       NULL,
        -- GUID string — kim yaptı
        Details     NVARCHAR(MAX)       NULL,
        -- JSON formatında ek bilgi
        IpAddress   NVARCHAR(50)        NULL,
        CreatedTime DATETIME2           NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
        CONSTRAINT CK_AuditLogs_ServiceName CHECK (ServiceName IN ('IdentityService', 'ParameterService', 'GoalService')),
        CONSTRAINT CK_AuditLogs_Action CHECK (Action IN ('Create', 'Update', 'Delete', 'Login', 'Logout', 'StatusChange', 'ChangePassword'))
    );

    CREATE INDEX IX_AuditLogs_ServiceName ON AuditLogs(ServiceName);
    CREATE INDEX IX_AuditLogs_UserId      ON AuditLogs(UserId);
    CREATE INDEX IX_AuditLogs_CreatedTime ON AuditLogs(CreatedTime);
END
GO

-- =============================================
-- Doğrulama
-- =============================================
SELECT 'AuditLogs' AS TableName, COUNT(*) AS [RowCount] FROM AuditLogs;
GO
