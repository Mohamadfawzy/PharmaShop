USE [pharma_shope_db];
GO

/* Run once after Customers and CustomerPointLots/Transactions have been created and customer seed data inserted.
   This converts legacy Customers.Points balances into the point-lot ledger. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @NowUtc datetime2(0) = SYSUTCDATETIME();
DECLARE @ExpiredLegacy TABLE (CustomerId int NOT NULL, ExpiredPoints int NOT NULL, BalanceAfter int NOT NULL);
DECLARE @MigratedLots TABLE (LotId int NOT NULL, CustomerId int NOT NULL, Points int NOT NULL);

/* Expire legacy balances whose old aggregate expiry date has passed. */
INSERT INTO @ExpiredLegacy (CustomerId, ExpiredPoints, BalanceAfter)
SELECT c.[Id], c.[Points] - ISNULL(activeLots.[RemainingPoints], 0), ISNULL(activeLots.[RemainingPoints], 0)
FROM [dbo].[Customers] AS c
OUTER APPLY
(
    SELECT SUM(l.[RemainingPoints]) AS [RemainingPoints]
    FROM [dbo].[CustomerPointLots] AS l
    WHERE l.[CustomerId] = c.[Id] AND l.[Status] = 2
      AND l.[RemainingPoints] > 0 AND l.[ExpiresAt] > @NowUtc
) AS activeLots
WHERE c.[Points] > ISNULL(activeLots.[RemainingPoints], 0)
  AND c.[PointsExpiryDate] IS NOT NULL
  AND c.[PointsExpiryDate] <= @NowUtc;

INSERT INTO [dbo].[CustomerPointTransactions]
(
    [CustomerId], [TransactionType], [PointsDelta], [BalanceAfter],
    [ReferenceType], [ReferenceId], [Notes], [CreatedAt]
)
SELECT [CustomerId], 5, -[ExpiredPoints], [BalanceAfter], 4, -[CustomerId],
       N'Expired legacy points balance during points-lot migration', @NowUtc
FROM @ExpiredLegacy;

UPDATE c SET c.[Points] = expired.[BalanceAfter], c.[UpdatedAt] = @NowUtc
FROM [dbo].[Customers] AS c
INNER JOIN @ExpiredLegacy AS expired ON expired.[CustomerId] = c.[Id];

/* Migrate remaining valid legacy points into one available lot per customer. */
INSERT INTO [dbo].[CustomerPointLots]
(
    [CustomerId], [OrderId], [OrderItemId], [PointsTotal], [RemainingPoints],
    [Status], [EarnedAt], [AvailableAt], [ExpiresAt], [CreatedAt]
)
OUTPUT inserted.[Id], inserted.[CustomerId], inserted.[PointsTotal]
INTO @MigratedLots ([LotId], [CustomerId], [Points])
SELECT
    c.[Id], NULL, NULL, c.[Points] - ISNULL(activeLots.[RemainingPoints], 0),
    c.[Points] - ISNULL(activeLots.[RemainingPoints], 0),
    2, @NowUtc, @NowUtc,
    CASE WHEN c.[PointsExpiryDate] > @NowUtc THEN c.[PointsExpiryDate] ELSE DATEADD(YEAR, 1, @NowUtc) END,
    @NowUtc
FROM [dbo].[Customers] AS c
OUTER APPLY
(
    SELECT SUM(l.[RemainingPoints]) AS [RemainingPoints]
    FROM [dbo].[CustomerPointLots] AS l
    WHERE l.[CustomerId] = c.[Id] AND l.[Status] = 2
      AND l.[RemainingPoints] > 0 AND l.[ExpiresAt] > @NowUtc
) AS activeLots
WHERE c.[Points] > ISNULL(activeLots.[RemainingPoints], 0);

INSERT INTO [dbo].[CustomerPointTransactions]
(
    [CustomerId], [LotId], [TransactionType], [PointsDelta], [BalanceAfter],
    [ExpiresAt], [ReferenceType], [ReferenceId], [Notes], [CreatedAt]
)
SELECT migrated.[CustomerId], migrated.[LotId], 7, migrated.[Points], c.[Points],
       NULL, 3, -migrated.[LotId], N'Legacy points balance migrated to an available lot', @NowUtc
FROM @MigratedLots AS migrated
INNER JOIN [dbo].[Customers] AS c ON c.[Id] = migrated.[CustomerId];

COMMIT TRANSACTION;
GO
