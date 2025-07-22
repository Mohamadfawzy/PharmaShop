CREATE PROCEDURE AddCustomerPoints
    @CustomerId INT,
    @OrderId INT,
    @OrderTotal DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PointsEarned INT = FLOOR(@OrderTotal / 10); -- سياسة: كل 10 جنيه = 1 نقطة

    -- إضافة إلى السجل
    INSERT INTO CustomerPointsHistory (
        CustomerId, Points, Source, RelatedOrderId, Note,ExpiresAt , CreatedAt
    )
    VALUES (
        @CustomerId, @PointsEarned, 'Purchase', @OrderId,
        CONCAT('Points earned from order #', @OrderId),
		DATEADD(MONTH, 6, GETDATE()),
        GETDATE()
    );

    -- تحديث الرصيد
    MERGE CustomerPointsBalance AS target
    USING (SELECT @CustomerId AS CustomerId) AS source
    ON target.CustomerId = source.CustomerId
    WHEN MATCHED THEN 
        UPDATE SET TotalPoints = TotalPoints + @PointsEarned, LastUpdated = GETDATE()
    WHEN NOT MATCHED THEN 
        INSERT (CustomerId, TotalPoints, LastUpdated)
        VALUES (@CustomerId, @PointsEarned, GETDATE());
END;

--====================================================

CREATE PROCEDURE UseCustomerPointsAdvanced
    @CustomerId INT,
    @PointsToUse INT,
    @OrderId INT,
    @OrderTotal DECIMAL(18,2),
    @UseDate DATETIME = NULL -- وقت الشراء الفعلي
AS
BEGIN
    SET NOCOUNT ON;

    IF @UseDate IS NULL
        SET @UseDate = GETDATE();

    -- الحد الأدنى
    IF @PointsToUse < 50
    BEGIN
        RAISERROR('الحد الأدنى لاستخدام النقاط هو 50 نقطة.', 16, 1);
        RETURN;
    END

    -- الحد الأقصى (50% من قيمة الفاتورة)
    DECLARE @MaxAllowedPoints INT = FLOOR(@OrderTotal * 0.5 * 1.0); -- الحد الأعلى كنقاط

    IF @PointsToUse > @MaxAllowedPoints
    BEGIN
        RAISERROR('لا يمكن استخدام أكثر من 50%% من قيمة الفاتورة كنقاط.', 16, 1);
        RETURN;
    END

    -- اجمع فقط النقاط غير منتهية الصلاحية
    DECLARE @AvailablePoints INT;

    SELECT @AvailablePoints = ISNULL(SUM(Points), 0)
    FROM CustomerPointsHistory
    WHERE CustomerId = @CustomerId
      AND Points > 0
      AND (ExpiresAt IS NULL OR ExpiresAt >= @UseDate);

    IF @AvailablePoints < @PointsToUse
    BEGIN
        RAISERROR('رصيد النقاط المتاحة لا يكفي.', 16, 1);
        RETURN;
    END

    -- خصم النقاط بالتدرج (FIFO)
    DECLARE @RemainingToUse INT = @PointsToUse;

    DECLARE PointsCursor CURSOR FOR
    SELECT Id, Points
    FROM CustomerPointsHistory
    WHERE CustomerId = @CustomerId
      AND Points > 0
      AND (ExpiresAt IS NULL OR ExpiresAt >= @UseDate)
    ORDER BY CreatedAt;

    DECLARE @HistoryId INT;
    DECLARE @PointsAvailable INT;

    OPEN PointsCursor;
    FETCH NEXT FROM PointsCursor INTO @HistoryId, @PointsAvailable;

    WHILE @@FETCH_STATUS = 0 AND @RemainingToUse > 0
    BEGIN
        DECLARE @ToUse INT = CASE 
            WHEN @RemainingToUse >= @PointsAvailable THEN @PointsAvailable
            ELSE @RemainingToUse END;

        SET @RemainingToUse -= @ToUse;

        -- أضف سجل سالب (الاستخدام)
        INSERT INTO CustomerPointsHistory (
            CustomerId, Points, Source, RelatedOrderId, Note, CreatedAt
        )
        VALUES (
            @CustomerId, -@ToUse, 'Redemption', @OrderId,
            CONCAT('استخدام ', @ToUse, ' نقطة من سجل #', @HistoryId),
            @UseDate
        );

        FETCH NEXT FROM PointsCursor INTO @HistoryId, @PointsAvailable;
    END

    CLOSE PointsCursor;
    DEALLOCATE PointsCursor;

    -- تحديث الرصيد العام
    UPDATE CustomerPointsBalance
    SET TotalPoints = TotalPoints - @PointsToUse,
        LastUpdated = @UseDate
    WHERE CustomerId = @CustomerId;
END;


--====================================================


CREATE PROCEDURE GetCustomerPoints
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TotalPoints, LastUpdated
    FROM CustomerPointsBalance
    WHERE CustomerId = @CustomerId;
END;


--====================================================

CREATE PROCEDURE GetCustomerPointsHistory
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Points,
        Source,
        RelatedOrderId,
        Note,
        CreatedAt
    FROM CustomerPointsHistory
    WHERE CustomerId = @CustomerId
    ORDER BY CreatedAt DESC;
END;


--====================================================
--====================================================
--====================================================