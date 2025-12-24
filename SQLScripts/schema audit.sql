
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
