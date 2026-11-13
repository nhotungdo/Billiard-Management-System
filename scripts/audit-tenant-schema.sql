-- Read-only assertions. Run against a disposable migrated database, before and after API tests.
SET NOCOUNT ON;
DECLARE @empty uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @tables TABLE (Name sysname);
INSERT @tables VALUES ('BilliardTables'), ('TableSessions'), ('TableStatusHistories'),
    ('Products'), ('Categories'), ('Combos'), ('ComboItems'), ('Customers'), ('Shifts');

IF OBJECT_ID('dbo.Stores') IS NULL OR OBJECT_ID('dbo.Users') IS NULL
    THROW 51000, 'Missing Stores or Users table', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20261113070108_AddMultiTenantSupport')
    THROW 51000, 'Multi-tenant migration is not applied', 1;
IF EXISTS (SELECT 1 FROM @tables t WHERE NOT EXISTS (
    SELECT 1 FROM sys.columns c WHERE c.object_id = OBJECT_ID('dbo.' + t.Name)
        AND c.name = 'StoreId' AND c.is_nullable = 0 AND TYPE_NAME(c.user_type_id) = 'uniqueidentifier'))
    THROW 51000, 'Missing required non-nullable StoreId', 1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'StoreId' AND is_nullable = 1)
    THROW 51000, 'Users.StoreId must allow NULL for SuperAdmin', 1;

DECLARE @table sysname, @sql nvarchar(max);
DECLARE tenant_tables CURSOR LOCAL FAST_FORWARD FOR SELECT Name FROM @tables;
OPEN tenant_tables;
FETCH NEXT FROM tenant_tables INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_key_columns f
        JOIN sys.columns c ON c.object_id = f.parent_object_id AND c.column_id = f.parent_column_id
        WHERE f.parent_object_id = OBJECT_ID('dbo.' + @table) AND c.name = 'StoreId'
            AND f.referenced_object_id = OBJECT_ID('dbo.Stores'))
        THROW 51000, 'Missing StoreId foreign key to Stores', 1;
    SET @sql = N'IF EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@table) + N' e LEFT JOIN dbo.Stores s ON s.Id = e.StoreId
        WHERE e.StoreId IS NULL OR e.StoreId = @empty OR s.Id IS NULL)
        THROW 51000, ''Orphan tenant data: ' + @table + N''', 1;';
    EXEC sp_executesql @sql, N'@empty uniqueidentifier', @empty;
    FETCH NEXT FROM tenant_tables INTO @table;
END;
CLOSE tenant_tables;
DEALLOCATE tenant_tables;

IF EXISTS (SELECT 1 FROM dbo.Users u LEFT JOIN dbo.Stores s ON s.Id = u.StoreId
    WHERE (u.Role = 0 AND u.StoreId IS NOT NULL) OR
        (u.Role <> 0 AND (u.StoreId IS NULL OR u.StoreId = @empty OR s.Id IS NULL)))
    THROW 51000, 'Invalid user role/store assignment', 1;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1)
    THROW 51000, 'Disabled or untrusted foreign key', 1;

DECLARE @unique TABLE (TableName sysname, FieldName sysname);
INSERT @unique VALUES ('BilliardTables', 'TableName'), ('Products', 'ProductName'),
    ('Customers', 'PhoneNumber'), ('Combos', 'ComboCode');
IF EXISTS (SELECT 1 FROM @unique u WHERE NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.index_columns k1 ON k1.object_id = i.object_id AND k1.index_id = i.index_id AND k1.key_ordinal = 1
    JOIN sys.columns c1 ON c1.object_id = k1.object_id AND c1.column_id = k1.column_id
    JOIN sys.index_columns k2 ON k2.object_id = i.object_id AND k2.index_id = i.index_id AND k2.key_ordinal = 2
    JOIN sys.columns c2 ON c2.object_id = k2.object_id AND c2.column_id = k2.column_id
    WHERE i.object_id = OBJECT_ID('dbo.' + u.TableName) AND i.is_unique = 1 AND i.is_disabled = 0
        AND c1.name = 'StoreId' AND c2.name = u.FieldName
        AND NOT EXISTS (SELECT 1 FROM sys.index_columns k3 WHERE k3.object_id = i.object_id AND k3.index_id = i.index_id AND k3.key_ordinal > 2)))
    THROW 51000, 'Missing per-store unique index', 1;
IF EXISTS (SELECT 1 FROM @unique u JOIN sys.indexes i ON i.object_id = OBJECT_ID('dbo.' + u.TableName)
    JOIN sys.index_columns k ON k.object_id = i.object_id AND k.index_id = i.index_id AND k.key_ordinal = 1
    JOIN sys.columns c ON c.object_id = k.object_id AND c.column_id = k.column_id
    WHERE i.is_unique = 1 AND c.name = u.FieldName AND NOT EXISTS (
        SELECT 1 FROM sys.index_columns k2 WHERE k2.object_id = i.object_id AND k2.index_id = i.index_id AND k2.key_ordinal > 1))
    THROW 51000, 'Legacy global unique index still exists', 1;

-- Check tenant consistency of every direct FK between tenant-scoped tables.
DECLARE @parent sysname, @child sysname, @column sysname, @refColumn sysname;
DECLARE tenant_links CURSOR LOCAL FAST_FORWARD FOR
    SELECT OBJECT_NAME(f.parent_object_id), OBJECT_NAME(f.referenced_object_id), pc.name, rc.name
    FROM sys.foreign_key_columns f
    JOIN sys.columns pc ON pc.object_id = f.parent_object_id AND pc.column_id = f.parent_column_id
    JOIN sys.columns rc ON rc.object_id = f.referenced_object_id AND rc.column_id = f.referenced_column_id
    WHERE COL_LENGTH('dbo.' + OBJECT_NAME(f.parent_object_id), 'StoreId') IS NOT NULL
        AND COL_LENGTH('dbo.' + OBJECT_NAME(f.referenced_object_id), 'StoreId') IS NOT NULL;
OPEN tenant_links;
FETCH NEXT FROM tenant_links INTO @parent, @child, @column, @refColumn;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'IF EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@parent) + N' p JOIN dbo.' + QUOTENAME(@child) + N' c
        ON p.' + QUOTENAME(@column) + N' = c.' + QUOTENAME(@refColumn) + N' WHERE p.StoreId <> c.StoreId'
        + CASE WHEN @child = 'Users' THEN N' AND c.Role <> 0' ELSE N'' END
        + N') THROW 51000, ''Cross-tenant FK: ' + @parent + N'.' + @column + N''', 1;';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM tenant_links INTO @parent, @child, @column, @refColumn;
END;
CLOSE tenant_links;
DEALLOCATE tenant_links;

IF EXISTS (SELECT 1 FROM dbo.OrderItems oi JOIN dbo.Orders o ON o.Id = oi.OrderId
    JOIN dbo.TableSessions s ON s.Id = o.TableSessionId JOIN dbo.Products p ON p.Id = oi.ProductId WHERE s.StoreId <> p.StoreId)
    THROW 51000, 'Order item product belongs to another tenant', 1;
IF EXISTS (SELECT 1 FROM dbo.Orders o JOIN dbo.TableSessions s ON s.Id = o.TableSessionId
    JOIN dbo.Users u ON u.Id = o.OrderedBy WHERE u.Role <> 0 AND s.StoreId <> u.StoreId)
    THROW 51000, 'Order actor belongs to another tenant', 1;
IF EXISTS (SELECT 1 FROM dbo.SessionCombos sc JOIN dbo.TableSessions s ON s.Id = sc.TableSessionId
    JOIN dbo.Combos c ON c.Id = sc.ComboId WHERE s.StoreId <> c.StoreId)
    THROW 51000, 'Session combo belongs to another tenant', 1;
IF EXISTS (SELECT 1 FROM dbo.Invoices i JOIN dbo.TableSessions s ON s.Id = i.TableSessionId
    JOIN dbo.Customers c ON c.Id = i.CustomerId WHERE s.StoreId <> c.StoreId)
    THROW 51000, 'Invoice customer belongs to another tenant', 1;
IF EXISTS (SELECT 1 FROM dbo.Invoices i JOIN dbo.TableSessions s ON s.Id = i.TableSessionId
    JOIN dbo.Orders o ON o.Id = i.OrderId JOIN dbo.TableSessions os ON os.Id = o.TableSessionId WHERE s.StoreId <> os.StoreId)
    THROW 51000, 'Invoice order belongs to another tenant', 1;
SELECT 'PASS: tenant schema, migration, FK/index metadata, role/store assignments and data integrity' AS AuditResult;
