USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.ReturnRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReturnRequests
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReturnRequests PRIMARY KEY,
        OrderId INT NOT NULL,
        CustomerId INT NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_ReturnRequests_Status DEFAULT (1),
        Reason NVARCHAR(500) NOT NULL,
        RequestedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
        ReviewedByEmployeeId INT NULL,
        ReviewedByUserId INT NULL,
        ReviewedAt DATETIME2(0) NULL,
        ReviewNotes NVARCHAR(500) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequests_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT CK_ReturnRequests_Status CHECK (Status IN (1,2,3)),
        CONSTRAINT FK_ReturnRequests_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_ReturnRequests_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
        CONSTRAINT FK_ReturnRequests_Employees FOREIGN KEY (ReviewedByEmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_ReturnRequests_Users FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );

    CREATE INDEX IX_ReturnRequests_Status_RequestedAt ON dbo.ReturnRequests(Status, RequestedAt DESC);
    CREATE INDEX IX_ReturnRequests_Order_Status ON dbo.ReturnRequests(OrderId, Status);

    CREATE TABLE dbo.ReturnRequestItems
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReturnRequestItems PRIMARY KEY,
        ReturnRequestId INT NOT NULL,
        OrderItemId INT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequestItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_ReturnRequestItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_ReturnRequestItems_ReturnRequests FOREIGN KEY (ReturnRequestId) REFERENCES dbo.ReturnRequests(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReturnRequestItems_OrderItems FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id)
    );

    CREATE UNIQUE INDEX UX_ReturnRequestItems_Return_OrderItem ON dbo.ReturnRequestItems(ReturnRequestId, OrderItemId);
    CREATE INDEX IX_ReturnRequestItems_OrderItem ON dbo.ReturnRequestItems(OrderItemId);
END;
GO

IF COL_LENGTH(N'dbo.ReturnRequests', N'ReviewedByUserId') IS NULL
    ALTER TABLE dbo.ReturnRequests ADD ReviewedByUserId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReturnRequests_Users')
    ALTER TABLE dbo.ReturnRequests WITH CHECK ADD CONSTRAINT FK_ReturnRequests_Users
        FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.AspNetUsers(Id);
GO

IF COL_LENGTH(N'dbo.ProductInventoryTransactions', N'ReturnRequestItemId') IS NULL
    ALTER TABLE dbo.ProductInventoryTransactions ADD ReturnRequestItemId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductInventoryTransactions_ReturnRequestItems')
    ALTER TABLE dbo.ProductInventoryTransactions WITH CHECK
        ADD CONSTRAINT FK_ProductInventoryTransactions_ReturnRequestItems
        FOREIGN KEY (ReturnRequestItemId) REFERENCES dbo.ReturnRequestItems(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ProductInventoryTransactions_ReturnRequestItem')
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_ReturnRequestItem
        ON dbo.ProductInventoryTransactions(ReturnRequestItemId)
        WHERE ReturnRequestItemId IS NOT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProductInventoryTransactions_Type')
    ALTER TABLE dbo.ProductInventoryTransactions DROP CONSTRAINT CK_ProductInventoryTransactions_Type;
ALTER TABLE dbo.ProductInventoryTransactions WITH CHECK
    ADD CONSTRAINT CK_ProductInventoryTransactions_Type CHECK (TransactionType BETWEEN 1 AND 6);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_Type')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_Type;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_PointsDelta')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_PointsDelta;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_EarnExpiresAt')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_EarnExpiresAt;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_ReferenceType')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_ReferenceType;

ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_Type CHECK (TransactionType IN (1,2,3,4,5,6,7,8));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_PointsDelta CHECK (
        (TransactionType IN (1,2,4,8) AND PointsDelta > 0)
        OR (TransactionType IN (3,5,6) AND PointsDelta < 0)
        OR (TransactionType = 7 AND PointsDelta <> 0));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_EarnExpiresAt CHECK (
        (TransactionType NOT IN (1,2,8) AND ExpiresAt IS NULL)
        OR (TransactionType IN (1,2,8) AND ExpiresAt IS NOT NULL));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_ReferenceType CHECK (ReferenceType IS NULL OR ReferenceType IN (1,2,3,4,5));
GO
