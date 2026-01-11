
use pharma_shope_db;

CREATE SCHEMA audit;
GO

-- CategoryAuditLog 
CREATE TABLE audit.CategoryAuditLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    ChangeType NVARCHAR(50) NOT NULL,
    FieldName NVARCHAR(100) NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    ChangedBy NVARCHAR(100) NULL,
    ChangeDate DATETIME NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(500) NULL,
    CONSTRAINT FK_CategoryAuditLog_Category
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);



USE pharma_shope_db;
GO

IF OBJECT_ID(N'dbo.LoginAudits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAudits
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginAudits PRIMARY KEY,
        CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_LoginAudits_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        Outcome         NVARCHAR(20) NOT NULL,       -- Success / Failure
        FailureReason   NVARCHAR(50) NULL,           -- InvalidCredentials / LockedOut / EmailNotConfirmed / UserDisabled / ...

        UserId          INT NULL,                    -- لو عرفنا المستخدم
        IdentifierMasked NVARCHAR(256) NULL,         -- masked email/username
        IdentifierHash  VARBINARY(32) NULL,          -- SHA-256(identifierLower)

        IpAddress       NVARCHAR(45) NULL,           -- IPv4/IPv6
        UserAgent       NVARCHAR(512) NULL,

        TraceId         NVARCHAR(64) NULL,
        PharmacyId      INT NULL,

        LatencyMs       INT NULL
    );
END
GO

-- Indexes (Idempotent)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_UserId' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_UserId ON dbo.LoginAudits(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_CreatedAtUtc' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_CreatedAtUtc ON dbo.LoginAudits(CreatedAtUtc);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_Ip_CreatedAtUtc' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_Ip_CreatedAtUtc ON dbo.LoginAudits(IpAddress, CreatedAtUtc);
GO
