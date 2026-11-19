USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.OrderDeliveryAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDeliveryAssignments
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDeliveryAssignments PRIMARY KEY,
        OrderId INT NOT NULL,
        DeliveryAgentUserId INT NOT NULL,
        AssignedByUserId INT NULL,
        UnassignedByUserId INT NULL,
        AssignedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrderDeliveryAssignments_AssignedAt DEFAULT (SYSUTCDATETIME()),
        UnassignedAt DATETIME2(0) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_OrderDeliveryAssignments_IsActive DEFAULT (1),
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_OrderDeliveryAssignments_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_DeliveryAgents FOREIGN KEY (DeliveryAgentUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_AssignedByUsers FOREIGN KEY (AssignedByUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_UnassignedByUsers FOREIGN KEY (UnassignedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE UNIQUE INDEX UX_OrderDeliveryAssignments_ActiveOrder
        ON dbo.OrderDeliveryAssignments(OrderId) WHERE IsActive = 1;
    CREATE INDEX IX_OrderDeliveryAssignments_Agent_Active_AssignedAt
        ON dbo.OrderDeliveryAssignments(DeliveryAgentUserId, IsActive, AssignedAt DESC);
END;
GO

IF COL_LENGTH(N'dbo.OrderDeliveryAssignments', N'UnassignedByUserId') IS NULL
    ALTER TABLE dbo.OrderDeliveryAssignments ADD UnassignedByUserId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OrderDeliveryAssignments_UnassignedByUsers')
    ALTER TABLE dbo.OrderDeliveryAssignments WITH CHECK ADD CONSTRAINT FK_OrderDeliveryAssignments_UnassignedByUsers
        FOREIGN KEY (UnassignedByUserId) REFERENCES dbo.AspNetUsers(Id);
GO

IF OBJECT_ID(N'dbo.OrderStatusHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistories
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderStatusHistories PRIMARY KEY,
        OrderId INT NOT NULL,
        PreviousStatus TINYINT NULL,
        NewStatus TINYINT NOT NULL,
        ChangedByUserId INT NULL,
        ChangedByRole NVARCHAR(50) NOT NULL,
        Notes NVARCHAR(500) NULL,
        ChangedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrderStatusHistories_ChangedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_OrderStatusHistories_Status CHECK (NewStatus BETWEEN 1 AND 8 AND (PreviousStatus IS NULL OR PreviousStatus BETWEEN 1 AND 8)),
        CONSTRAINT FK_OrderStatusHistories_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderStatusHistories_Users FOREIGN KEY (ChangedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_OrderStatusHistories_Order_ChangedAt
        ON dbo.OrderStatusHistories(OrderId, ChangedAt, Id);
END;
GO

INSERT INTO dbo.OrderStatusHistories
    (OrderId, PreviousStatus, NewStatus, ChangedByUserId, ChangedByRole, Notes, ChangedAt)
SELECT o.Id, NULL, o.Status, NULL, N'System', N'History initialized from the current order status during Epic 16 rollout', o.StatusUpdatedAt
FROM dbo.Orders AS o
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrderStatusHistories AS h WHERE h.OrderId = o.Id);
GO
