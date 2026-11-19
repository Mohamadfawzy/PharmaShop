USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.UserNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserNotifications
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserNotifications PRIMARY KEY,
        UserId INT NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        RelatedEntityType NVARCHAR(50) NULL,
        RelatedEntityId BIGINT NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_UserNotifications_IsRead DEFAULT (0),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserNotifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ReadAt DATETIME2(0) NULL,
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_UserNotifications_User_Read_CreatedAt
        ON dbo.UserNotifications(UserId, IsRead, CreatedAt DESC, Id DESC);
END;
GO

IF OBJECT_ID(N'dbo.CustomerNotificationPreferences', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerNotificationPreferences
    (
        UserId INT NOT NULL CONSTRAINT PK_CustomerNotificationPreferences PRIMARY KEY,
        OrderUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Order DEFAULT (1),
        PrescriptionUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Prescription DEFAULT (1),
        PointsUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Points DEFAULT (1),
        EmailEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Email DEFAULT (1),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_CustomerNotificationPreferences_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.PaymentAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentAttempts
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentAttempts PRIMARY KEY,
        OrderId INT NOT NULL,
        Provider NVARCHAR(50) NOT NULL,
        ExternalReference NVARCHAR(200) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_PaymentAttempts_Status DEFAULT (1),
        FailureReason NVARCHAR(500) NULL,
        CreatedByUserId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentAttempts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT CK_PaymentAttempts_Amount CHECK (Amount > 0),
        CONSTRAINT CK_PaymentAttempts_Status CHECK (Status BETWEEN 1 AND 4),
        CONSTRAINT FK_PaymentAttempts_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_PaymentAttempts_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_PaymentAttempts_Order_CreatedAt ON dbo.PaymentAttempts(OrderId, CreatedAt DESC);
    CREATE UNIQUE INDEX UX_PaymentAttempts_Provider_ExternalReference
        ON dbo.PaymentAttempts(Provider, ExternalReference) WHERE ExternalReference IS NOT NULL;
END;
GO

IF OBJECT_ID(N'dbo.PaymentRefunds', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentRefunds
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentRefunds PRIMARY KEY,
        PaymentAttemptId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_PaymentRefunds_Status DEFAULT (1),
        ExternalReference NVARCHAR(200) NULL,
        Reason NVARCHAR(500) NOT NULL,
        RequestedByUserId INT NULL,
        ProcessedByUserId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentRefunds_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ProcessedAt DATETIME2(0) NULL,
        CONSTRAINT CK_PaymentRefunds_Amount CHECK (Amount > 0),
        CONSTRAINT CK_PaymentRefunds_Status CHECK (Status BETWEEN 1 AND 4),
        CONSTRAINT FK_PaymentRefunds_PaymentAttempts FOREIGN KEY (PaymentAttemptId) REFERENCES dbo.PaymentAttempts(Id),
        CONSTRAINT FK_PaymentRefunds_RequestedBy FOREIGN KEY (RequestedByUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_PaymentRefunds_ProcessedBy FOREIGN KEY (ProcessedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_PaymentRefunds_Attempt_Status ON dbo.PaymentRefunds(PaymentAttemptId, Status);
    CREATE UNIQUE INDEX UX_PaymentRefunds_ExternalReference
        ON dbo.PaymentRefunds(ExternalReference) WHERE ExternalReference IS NOT NULL;
END;
GO

-- Provider-specific payment initiation and webhook verification are intentionally
-- not enabled until the selected payment provider and its credentials are configured.
