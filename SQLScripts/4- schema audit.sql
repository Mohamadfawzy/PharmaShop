
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

/*========================================================
  Schema: audit
========================================================*/
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'audit')
    EXEC('CREATE SCHEMA audit');
GO



/*========================================================
  audit.ProductAuditLog
  - Field-level audit (row per changed field)
  - OperationId groups one update into one event
========================================================*/
CREATE TABLE audit.ProductAuditLog
(
    Id            BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ProductAuditLog PRIMARY KEY,

    PharmacyId    INT NOT NULL,                       -- Multi-tenant
    ProductId     INT NOT NULL,                       -- Target product

    OperationId   UNIQUEIDENTIFIER NOT NULL,          -- Groups one update operation
    ChangeType    NVARCHAR(50) NOT NULL,              -- Update | Create | SoftDelete | Restore

    FieldName     NVARCHAR(100) NULL,                 -- Which field changed
    OldValue      NVARCHAR(MAX) NULL,                 -- Old value (stringified)
    NewValue      NVARCHAR(MAX) NULL,                 -- New value (stringified)

    ChangedBy     NVARCHAR(100) NULL,                 -- UserId / Username
    ChangeDate    DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductAuditLog_ChangeDate DEFAULT (SYSUTCDATETIME()),

    Reason        NVARCHAR(500) NULL,                 -- Optional reason

    CONSTRAINT FK_ProductAuditLog_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_ProductAuditLog_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

/*========================================================
  Indexes
========================================================*/

-- Fast timeline per product
CREATE INDEX IX_ProductAuditLog_Product_ChangeDate
ON audit.ProductAuditLog(ProductId, ChangeDate DESC)
INCLUDE (OperationId, ChangeType, FieldName, OldValue, NewValue, ChangedBy, Reason, PharmacyId);
GO

-- Fast grouping by operation
CREATE INDEX IX_ProductAuditLog_Product_Operation
ON audit.ProductAuditLog(ProductId, OperationId)
INCLUDE (ChangeDate, ChangeType, FieldName, OldValue, NewValue, ChangedBy, Reason, PharmacyId);
GO

-- Filter by user (audit for admin/user)
CREATE INDEX IX_ProductAuditLog_Pharmacy_User_Date
ON audit.ProductAuditLog(PharmacyId, ChangedBy, ChangeDate DESC)
INCLUDE (ProductId, OperationId, ChangeType, FieldName, OldValue, NewValue, Reason);
GO




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
