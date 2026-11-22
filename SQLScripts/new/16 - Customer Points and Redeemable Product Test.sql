USE [pharma_shope_db];
GO

/*
    Test data for an EXISTING customer. This script does not create a user,
    customer, or product. It grants available points and marks one existing,
    eligible product as redeemable by points.

    Required: edit @CustomerId below (dbo.Customers.Id, not AspNetUsers.Id).
    Optional: set @RedeemableProductId to pick a specific eligible product;
              leave NULL to select the first eligible product automatically.
    Optional: change @PointsToAdd and @ReferenceIdBase if needed.

    Product.Points is left unchanged; it is used for points earned from orders.
    This script only changes Products.IsRedeemableByPoints.
    Run after the current schema, including SQLScripts/new/8- Points.sql.
    Safe to rerun: the manual adjustment reference prevents duplicate grants.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CustomerId int = 0;                 -- EDIT: dbo.Customers.Id
DECLARE @PointsToAdd int = 1000;             -- Available points to grant
DECLARE @RedeemableProductId int = NULL;     -- Optional existing product Id
DECLARE @ReferenceIdBase int = 1500000000;   -- Change if this range is already in use

IF @CustomerId <= 0
    THROW 51000, 'Set @CustomerId to an existing dbo.Customers.Id before running this script.', 1;

IF @CustomerId > 499999999
    THROW 51001, 'CustomerId is outside the supported range for the generated test reference IDs.', 1;

IF @PointsToAdd <= 0
    THROW 51002, '@PointsToAdd must be greater than zero.', 1;

IF @ReferenceIdBase < 1 OR @ReferenceIdBase > 1999999999 - @CustomerId
    THROW 51003, '@ReferenceIdBase is outside the valid range.', 1;

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
   OR OBJECT_ID(N'dbo.CustomerPointLots', N'U') IS NULL
   OR OBJECT_ID(N'dbo.CustomerPointTransactions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Products', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PromotionProducts', N'U') IS NULL
    THROW 51004, 'Required PharmaShop tables are missing. Apply the schema scripts first.', 1;

DECLARE @NowUtc datetime2(0) = SYSUTCDATETIME();
DECLARE @GrantReferenceId int = @ReferenceIdBase + @CustomerId;
DECLARE @LegacyReferenceId int = @ReferenceIdBase - 500000000 + @CustomerId;
DECLARE @GrantNotes nvarchar(500) = CONCAT(N'Test points seed grant for customer ', @CustomerId);
DECLARE @LegacyNotes nvarchar(500) = CONCAT(N'Legacy customer points moved to point lot by test seed for customer ', @CustomerId);

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CustomerPoints int;
    DECLARE @PointsExpiryDate datetime2(0);
    DECLARE @LotPoints int;
    DECLARE @LegacyPoints int;
    DECLARE @LegacyExpiresAt datetime2(0);
    DECLARE @GrantAlreadyExists bit = 0;

    SELECT @CustomerPoints = c.[Points], @PointsExpiryDate = c.[PointsExpiryDate]
    FROM [dbo].[Customers] AS c WITH (UPDLOCK, HOLDLOCK)
    WHERE c.[Id] = @CustomerId AND c.[IsActive] = 1;

    IF @CustomerPoints IS NULL
        THROW 51005, 'Active customer was not found. Check @CustomerId in dbo.Customers.', 1;

    /* Stop if the points expiry worker has not yet processed expired lots. */
    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[CustomerPointLots] AS l WITH (UPDLOCK, HOLDLOCK)
        WHERE l.[CustomerId] = @CustomerId AND l.[Status] = 2
          AND l.[RemainingPoints] > 0 AND l.[ExpiresAt] <= @NowUtc
    )
        THROW 51006, 'This customer has expired point lots. Let the points expiry job process them, then rerun this script.', 1;

    SELECT @LotPoints = ISNULL(SUM(l.[RemainingPoints]), 0)
    FROM [dbo].[CustomerPointLots] AS l WITH (UPDLOCK, HOLDLOCK)
    WHERE l.[CustomerId] = @CustomerId AND l.[Status] = 2
      AND l.[RemainingPoints] > 0 AND l.[ExpiresAt] > @NowUtc;

    IF @CustomerPoints < @LotPoints
        THROW 51007, 'Customer aggregate points are lower than available point lots. Reconcile the points ledger before running this script.', 1;

    SET @LegacyPoints = @CustomerPoints - @LotPoints;
    IF @LegacyPoints > 0 AND @PointsExpiryDate IS NOT NULL AND @PointsExpiryDate <= @NowUtc
        THROW 51008, 'Unmigrated legacy points are expired. Run SQLScripts/new/9 - Migrate legacy points.sql first.', 1;

    IF EXISTS
    (
        SELECT 1 FROM [dbo].[CustomerPointTransactions]
        WHERE [ReferenceId] IN (@GrantReferenceId, @LegacyReferenceId)
          AND NOT
          (
              [CustomerId] = @CustomerId AND [TransactionType] = 7 AND [ReferenceType] = 3
              AND
              (
                  ([ReferenceId] = @GrantReferenceId AND [Notes] = @GrantNotes AND [PointsDelta] = @PointsToAdd)
                  OR ([ReferenceId] = @LegacyReferenceId AND [Notes] = @LegacyNotes AND [PointsDelta] > 0)
              )
          )
    )
        THROW 51009, 'A generated manual-adjustment ReferenceId is already used. Change @ReferenceIdBase and rerun.', 1;

    IF EXISTS
    (
        SELECT 1 FROM [dbo].[CustomerPointTransactions]
        WHERE [CustomerId] = @CustomerId AND [TransactionType] = 7 AND [ReferenceType] = 3
          AND [ReferenceId] = @GrantReferenceId AND [Notes] = @GrantNotes
          AND [PointsDelta] = @PointsToAdd
    )
        SET @GrantAlreadyExists = 1;

    /* Select or validate a product that the API will actually show as redeemable. */
    IF @RedeemableProductId IS NULL
    BEGIN
        SELECT TOP (1) @RedeemableProductId = p.[Id]
        FROM [dbo].[Products] AS p
        WHERE p.[IsActive] = 1 AND p.[IsAvailable] = 1 AND p.[Quantity] > 0 AND p.[DeletedAt] IS NULL
          AND NOT
          (
              p.[HasPromotion] = 1
              AND p.[PromotionStartsAt] IS NOT NULL
              AND p.[PromotionEndsAt] IS NOT NULL
              AND p.[PromotionStartsAt] <= @NowUtc
              AND p.[PromotionEndsAt] >= @NowUtc
          )
          AND NOT EXISTS
          (
              SELECT 1
              FROM [dbo].[PromotionProducts] AS pp
              INNER JOIN [dbo].[Promotions] AS pr ON pr.[Id] = pp.[PromotionId]
              WHERE pp.[ProductId] = p.[Id] AND pp.[DeletedAt] IS NULL
                AND pr.[DeletedAt] IS NULL AND pr.[IsActive] = 1
                AND pr.[StartAt] <= @NowUtc AND pr.[EndAt] >= @NowUtc
                AND pr.[BasicAmount] IS NOT NULL AND pr.[OfferAmount] IS NOT NULL
                AND pr.[DiscountPercent] IS NOT NULL
          )
        ORDER BY CASE WHEN p.[IsRedeemableByPoints] = 1 THEN 0 ELSE 1 END, p.[Id];
    END;

    IF @RedeemableProductId IS NULL
        THROW 51010, 'No active, available, stocked, non-promoted product was found. Set @RedeemableProductId to an eligible product.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Products] AS p
        WHERE p.[Id] = @RedeemableProductId AND p.[IsActive] = 1
          AND p.[IsAvailable] = 1 AND p.[Quantity] > 0 AND p.[DeletedAt] IS NULL
          AND NOT
          (
              p.[HasPromotion] = 1
              AND p.[PromotionStartsAt] IS NOT NULL
              AND p.[PromotionEndsAt] IS NOT NULL
              AND p.[PromotionStartsAt] <= @NowUtc
              AND p.[PromotionEndsAt] >= @NowUtc
          )
          AND NOT EXISTS
          (
              SELECT 1
              FROM [dbo].[PromotionProducts] AS pp
              INNER JOIN [dbo].[Promotions] AS pr ON pr.[Id] = pp.[PromotionId]
              WHERE pp.[ProductId] = p.[Id] AND pp.[DeletedAt] IS NULL
                AND pr.[DeletedAt] IS NULL AND pr.[IsActive] = 1
                AND pr.[StartAt] <= @NowUtc AND pr.[EndAt] >= @NowUtc
                AND pr.[BasicAmount] IS NOT NULL AND pr.[OfferAmount] IS NOT NULL
                AND pr.[DiscountPercent] IS NOT NULL
          )
    )
        THROW 51011, 'The selected product is not currently eligible for points redemption.', 1;

    /* Convert any untracked legacy customer balance into an available lot once. */
    IF @LegacyPoints > 0
    BEGIN
        IF EXISTS
        (
            SELECT 1 FROM [dbo].[CustomerPointTransactions]
            WHERE [CustomerId] = @CustomerId AND [TransactionType] = 7 AND [ReferenceType] = 3
              AND [ReferenceId] = @LegacyReferenceId AND [Notes] = @LegacyNotes
        )
            THROW 51012, 'Legacy points reference exists but the ledger still differs from Customers.Points. Reconcile the existing lot before rerunning.', 1;

        SET @LegacyExpiresAt = CASE
            WHEN @PointsExpiryDate > @NowUtc THEN @PointsExpiryDate
            ELSE DATEADD(YEAR, 1, @NowUtc)
        END;

        DECLARE @LegacyLot TABLE ([Id] int NOT NULL);
        INSERT INTO [dbo].[CustomerPointLots]
        (
            [CustomerId], [PointsTotal], [RemainingPoints], [Status],
            [EarnedAt], [AvailableAt], [ExpiresAt], [CreatedAt]
        )
        OUTPUT inserted.[Id] INTO @LegacyLot ([Id])
        VALUES (@CustomerId, @LegacyPoints, @LegacyPoints, 2, @NowUtc, @NowUtc, @LegacyExpiresAt, @NowUtc);

        INSERT INTO [dbo].[CustomerPointTransactions]
        (
            [CustomerId], [LotId], [TransactionType], [PointsDelta], [BalanceAfter],
            [ReferenceType], [ReferenceId], [Notes], [CreatedAt]
        )
        SELECT @CustomerId, [Id], 7, @LegacyPoints, @CustomerPoints,
               3, @LegacyReferenceId, @LegacyNotes, @NowUtc
        FROM @LegacyLot;
    END;

    UPDATE [dbo].[Products]
    SET [IsRedeemableByPoints] = 1, [UpdatedAt] = @NowUtc
    WHERE [Id] = @RedeemableProductId AND [IsRedeemableByPoints] = 0;

    /* Add one available lot and ledger row; the stable reference makes reruns safe. */
    IF @GrantAlreadyExists = 0
    BEGIN
        IF CONVERT(bigint, @CustomerPoints) + @PointsToAdd > 2147483647
            THROW 51013, 'The resulting customer point balance would exceed the supported integer range.', 1;

        DECLARE @GrantLot TABLE ([Id] int NOT NULL);
        INSERT INTO [dbo].[CustomerPointLots]
        (
            [CustomerId], [PointsTotal], [RemainingPoints], [Status],
            [EarnedAt], [AvailableAt], [ExpiresAt], [CreatedAt]
        )
        OUTPUT inserted.[Id] INTO @GrantLot ([Id])
        VALUES (@CustomerId, @PointsToAdd, @PointsToAdd, 2, @NowUtc, @NowUtc, DATEADD(YEAR, 1, @NowUtc), @NowUtc);

        INSERT INTO [dbo].[CustomerPointTransactions]
        (
            [CustomerId], [LotId], [TransactionType], [PointsDelta], [BalanceAfter],
            [ReferenceType], [ReferenceId], [Notes], [CreatedAt]
        )
        SELECT @CustomerId, [Id], 7, @PointsToAdd, @CustomerPoints + @PointsToAdd,
               3, @GrantReferenceId, @GrantNotes, @NowUtc
        FROM @GrantLot;

        UPDATE [dbo].[Customers]
        SET [Points] = [Points] + @PointsToAdd, [UpdatedAt] = @NowUtc
        WHERE [Id] = @CustomerId;
    END;

    COMMIT TRANSACTION;

    SELECT
        c.[Id] AS [CustomerId],
        c.[Points] AS [CustomerPoints],
        @RedeemableProductId AS [RedeemableProductId],
        p.[NameAr] AS [RedeemableProductNameAr],
        p.[IsRedeemableByPoints],
        @PointsToAdd AS [ConfiguredGrantPoints],
        @GrantReferenceId AS [GrantReferenceId],
        @GrantAlreadyExists AS [GrantWasAlreadyApplied]
    FROM [dbo].[Customers] AS c
    INNER JOIN [dbo].[Products] AS p ON p.[Id] = @RedeemableProductId
    WHERE c.[Id] = @CustomerId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
