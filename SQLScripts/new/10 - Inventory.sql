USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.ProductInventoryTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductInventoryTransactions
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductInventoryTransactions PRIMARY KEY,
        ProductId INT NOT NULL,
        EmployeeId INT NULL,
        OrderId INT NULL,
        OrderItemId INT NULL,
        ProductUnitId INT NULL,
        TransactionType TINYINT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        QuantityBefore DECIMAL(18,3) NOT NULL,
        QuantityAfter DECIMAL(18,3) NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        BatchNumber NVARCHAR(80) NULL,
        ExpirationDate DATETIME2(0) NULL,
        CostPrice DECIMAL(18,2) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ProductInventoryTransactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_ProductInventoryTransactions_Type CHECK (TransactionType BETWEEN 1 AND 6),
        CONSTRAINT CK_ProductInventoryTransactions_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_ProductInventoryTransactions_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_ProductInventoryTransactions_OrderItems FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Units FOREIGN KEY (ProductUnitId) REFERENCES dbo.Units(Id)
    );

    CREATE INDEX IX_ProductInventoryTransactions_Product_CreatedAt
        ON dbo.ProductInventoryTransactions(ProductId, CreatedAt DESC);
    CREATE INDEX IX_ProductInventoryTransactions_Order
        ON dbo.ProductInventoryTransactions(OrderId, TransactionType) WHERE OrderId IS NOT NULL;
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_OrderDeduction
        ON dbo.ProductInventoryTransactions(OrderId, OrderItemId)
        WHERE OrderId IS NOT NULL AND OrderItemId IS NOT NULL AND TransactionType = 4;
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_OrderRestock
        ON dbo.ProductInventoryTransactions(OrderId, OrderItemId)
        WHERE OrderId IS NOT NULL AND OrderItemId IS NOT NULL AND TransactionType = 5;
END;
GO
