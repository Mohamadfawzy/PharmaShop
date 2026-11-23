USE [master];
GO

-- 1) Stop execution if the target database does not exist
IF DB_ID(N'pharma_shope_db') IS NULL
BEGIN
    THROW 50001, 'Database pharma_shope_db does not exist.', 1;
END;
GO

USE [pharma_shope_db];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Sql nvarchar(max) = N'';

    -- 2) Disable system versioning for temporal tables if any exist
    SELECT @Sql = STRING_AGG(
        CAST(
            N'ALTER TABLE '
            + QUOTENAME(OBJECT_SCHEMA_NAME(t.object_id))
            + N'.'
            + QUOTENAME(t.name)
            + N' SET (SYSTEM_VERSIONING = OFF);'
            AS nvarchar(max)
        ),
        CHAR(13) + CHAR(10)
    )
    FROM sys.tables AS t
    WHERE t.temporal_type = 2
      AND t.is_ms_shipped = 0;

    IF NULLIF(@Sql, N'') IS NOT NULL
        EXEC sys.sp_executesql @Sql;

    SET @Sql = N'';

    -- 3) Drop all foreign key constraints first
    SELECT @Sql = STRING_AGG(
        CAST(
            N'ALTER TABLE '
            + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
            + N'.'
            + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
            + N' DROP CONSTRAINT '
            + QUOTENAME(fk.name)
            + N';'
            AS nvarchar(max)
        ),
        CHAR(13) + CHAR(10)
    )
    FROM sys.foreign_keys AS fk
    INNER JOIN sys.tables AS t
        ON t.object_id = fk.parent_object_id
    WHERE t.is_ms_shipped = 0;

    IF NULLIF(@Sql, N'') IS NOT NULL
        EXEC sys.sp_executesql @Sql;

    SET @Sql = N'';

    -- 4) Drop all user tables after removing relationships
    SELECT @Sql = STRING_AGG(
        CAST(
            N'DROP TABLE '
            + QUOTENAME(s.name)
            + N'.'
            + QUOTENAME(t.name)
            + N';'
            AS nvarchar(max)
        ),
        CHAR(13) + CHAR(10)
    )
    FROM sys.tables AS t
    INNER JOIN sys.schemas AS s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0
      AND t.temporal_type <> 1;

    IF NULLIF(@Sql, N'') IS NOT NULL
        EXEC sys.sp_executesql @Sql;

    -- 5) Commit only after every table is removed successfully
    COMMIT TRANSACTION;

    PRINT N'All user tables were deleted successfully from pharma_shope_db.';
END TRY
BEGIN CATCH
    -- 6) Roll back the whole operation if any deletion fails
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage nvarchar(2048) =
        CONCAT(
            N'Failed to delete all tables. Error ',
            ERROR_NUMBER(),
            N' at line ',
            ERROR_LINE(),
            N': ',
            ERROR_MESSAGE()
        );

    THROW 50002, @ErrorMessage, 1;
END CATCH;
GO

-- 7) Verify that no user tables remain
SELECT
    s.name AS SchemaName,
    t.name AS TableName
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;
GO