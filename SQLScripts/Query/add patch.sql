BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PharmacyId    INT        = 1;
    DECLARE @StoreId       INT        = 1;
    DECLARE @ProductId     INT        = 10;
    DECLARE @ProductUnitId INT        = 5;

    DECLARE @BatchNumber   NVARCHAR(80) = N'BATCH-2026-0001';
    DECLARE @ExpirationDate DATE        = '2027-12-31';   -- ممكن NULL لو المنتج بدون صلاحية
    DECLARE @ReceivedQty   INT          = 100;
    DECLARE @CostPrice     DECIMAL(18,2)= 80.00;          -- تكلفة هذه الدفعة (اختياري)

    -------------------------------------------------------------------
    -- 1) تأكيد أن الـ ProductUnitId تابع لنفس ProductId (Composite FK موجود أصلاً)
    --    لكن نتحقق برسالة أوضح قبل ما يحصل خطأ FK.
    -------------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.ProductUnits pu
        WHERE pu.Id = @ProductUnitId
          AND pu.ProductId = @ProductId
          AND pu.PharmacyId = @PharmacyId
          AND pu.DeletedAt IS NULL
    )
    BEGIN
        THROW 50001, 'Invalid ProductUnitId for the given ProductId/PharmacyId.', 1;
    END;

    -------------------------------------------------------------------
    -- 2) إضافة الـ Batch
    -------------------------------------------------------------------
    INSERT INTO dbo.ProductBatches
    (
        PharmacyId, StoreId,
        ProductId, ProductUnitId,
        BatchNumber, ExpirationDate,
        ReceivedAt,
        QuantityReceived, QuantityOnHand,
        CostPrice, IsActive
    )
    VALUES
    (
        @PharmacyId, @StoreId,
        @ProductId, @ProductUnitId,
        @BatchNumber, @ExpirationDate,
        SYSUTCDATETIME(),
        @ReceivedQty, @ReceivedQty,
        @CostPrice, 1
    );

    -------------------------------------------------------------------
    -- 3) Upsert للـ Inventory: لو موجود نزود، لو مش موجود ننشئ صف جديد
    -------------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.ProductInventory i
        WHERE i.StoreId = @StoreId
          AND i.ProductUnitId = @ProductUnitId
    )
    BEGIN
        UPDATE dbo.ProductInventory
        SET
            QuantityOnHand    = QuantityOnHand + @ReceivedQty,
            LastStockUpdateAt = SYSUTCDATETIME()
        WHERE StoreId = @StoreId
          AND ProductUnitId = @ProductUnitId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.ProductInventory
        (
            PharmacyId, StoreId,
            ProductId, ProductUnitId,
            QuantityOnHand, ReservedQty,
            MinStockLevel, MaxStockLevel,
            LastStockUpdateAt
        )
        VALUES
        (
            @PharmacyId, @StoreId,
            @ProductId, @ProductUnitId,
            @ReceivedQty, 0,
            NULL, NULL,
            SYSUTCDATETIME()
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    -- إظهار سبب الخطأ
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    THROW 50002, @Err, 1;
END CATCH;
