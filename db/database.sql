-- ============================================================
-- Billiard Management System - Full database setup script
-- Generated from all EF Core migrations, including the default store.
-- Safe to run on an empty database; API startup seeding creates accounts.
-- ============================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [BilliardTables] (
    [Id] uniqueidentifier NOT NULL,
    [TableName] nvarchar(50) NOT NULL,
    [TableType] nvarchar(50) NOT NULL,
    [PricePerHour] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_BilliardTables] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [CategoryName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Role] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(150) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsAvailable] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Shifts] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ShiftDate] datetime2 NOT NULL,
    [StartShift] datetime2 NOT NULL,
    [EndShift] datetime2 NULL,
    [TotalRevenue] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shifts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TableSessions] (
    [Id] uniqueidentifier NOT NULL,
    [TableId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NULL,
    [DurationMinutes] int NULL,
    [TotalPrice] decimal(18,2) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TableSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TableSessions_BilliardTables_TableId] FOREIGN KEY ([TableId]) REFERENCES [BilliardTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Orders] (
    [Id] uniqueidentifier NOT NULL,
    [TableSessionId] uniqueidentifier NOT NULL,
    [OrderedBy] uniqueidentifier NOT NULL,
    [OrderTime] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_TableSessions_TableSessionId] FOREIGN KEY ([TableSessionId]) REFERENCES [TableSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Orders_Users_OrderedBy] FOREIGN KEY ([OrderedBy]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Invoices] (
    [Id] uniqueidentifier NOT NULL,
    [TableSessionId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [FinalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] int NOT NULL,
    [PaidAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]),
    CONSTRAINT [FK_Invoices_TableSessions_TableSessionId] FOREIGN KEY ([TableSessionId]) REFERENCES [TableSessions] ([Id])
);
GO

CREATE TABLE [OrderItems] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Invoices_OrderId] ON [Invoices] ([OrderId]);
GO

CREATE INDEX [IX_Invoices_TableSessionId] ON [Invoices] ([TableSessionId]);
GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
GO

CREATE INDEX [IX_Orders_OrderedBy] ON [Orders] ([OrderedBy]);
GO

CREATE INDEX [IX_Orders_TableSessionId] ON [Orders] ([TableSessionId]);
GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO

CREATE INDEX [IX_Shifts_UserId] ON [Shifts] ([UserId]);
GO

CREATE INDEX [IX_TableSessions_TableId] ON [TableSessions] ([TableId]);
GO

CREATE INDEX [IX_TableSessions_UserId] ON [TableSessions] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260516140652_InitialCreateV2', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260520144234_FixColumnMappings', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521033148_UpdateTableStatusEnum', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TableSessions]') AND [c].[name] = N'TotalPrice');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TableSessions] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [TableSessions] SET [TotalPrice] = 0.0 WHERE [TotalPrice] IS NULL;
ALTER TABLE [TableSessions] ALTER COLUMN [TotalPrice] decimal(18,2) NOT NULL;
ALTER TABLE [TableSessions] ADD DEFAULT 0.0 FOR [TotalPrice];
GO

ALTER TABLE [TableSessions] ADD [DurationHours] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TableSessions] ADD [IsFinished] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [TableSessions] ADD [RemainingMinutes] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521044130_AddSessionDurationFields', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Products] ADD [Description] nvarchar(500) NULL;
GO

ALTER TABLE [BilliardTables] ADD [Description] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521054948_AddTableAndProductDescription', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE UNIQUE INDEX [IX_BilliardTables_TableName] ON [BilliardTables] ([TableName]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521062054_AddUniqueIndexToTableName', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Products] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521092619_AddProductIsDeleted', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                WITH CTE AS (
                    SELECT Id,
                           ROW_NUMBER() OVER(
                               PARTITION BY LTRIM(RTRIM(LOWER(ProductName)))
                               ORDER BY Id
                           ) as rn
                    FROM Products
                )
                DELETE FROM Products WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
GO

CREATE UNIQUE INDEX [IX_Products_ProductName] ON [Products] ([ProductName]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260522164554_AddProductUniqueName', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [Email] nvarchar(100) NULL;
GO

ALTER TABLE [Users] ADD [ProfilePictureUrl] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260526035322_AddUserProfileFields', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [TableStatusHistories] (
    [Id] uniqueidentifier NOT NULL,
    [TableId] uniqueidentifier NOT NULL,
    [OldStatus] int NOT NULL,
    [NewStatus] int NOT NULL,
    [ChangedById] uniqueidentifier NULL,
    [ChangedAt] datetime2 NOT NULL,
    [Reason] nvarchar(500) NULL,
    CONSTRAINT [PK_TableStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TableStatusHistories_BilliardTables_TableId] FOREIGN KEY ([TableId]) REFERENCES [BilliardTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableStatusHistories_Users_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_TableStatusHistories_ChangedById] ON [TableStatusHistories] ([ChangedById]);
GO

CREATE INDEX [IX_TableStatusHistories_TableId] ON [TableStatusHistories] ([TableId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260527140850_AddTableStatusHistory', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TableSessions] ADD [CustomerId] uniqueidentifier NULL;
GO

ALTER TABLE [Invoices] ADD [CustomerId] uniqueidentifier NULL;
GO

CREATE TABLE [Customers] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(255) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [TotalVisits] int NOT NULL,
    [TotalPlayHours] decimal(18,2) NOT NULL,
    [TotalSpent] decimal(18,2) NOT NULL,
    [FirstVisitDate] datetime2 NULL,
    [LastVisitDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_TableSessions_CustomerId] ON [TableSessions] ([CustomerId]);
GO

CREATE INDEX [IX_Invoices_CustomerId] ON [Invoices] ([CustomerId]);
GO

CREATE UNIQUE INDEX [IX_Customers_PhoneNumber] ON [Customers] ([PhoneNumber]);
GO

ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [TableSessions] ADD CONSTRAINT [FK_TableSessions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617070604_CustomerManagement', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Invoices]') AND name = N'DiscountAmount') ALTER TABLE [Invoices] DROP COLUMN [DiscountAmount];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260724101328_RemoveDiscountAmount', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Combos] (
    [Id] uniqueidentifier NOT NULL,
    [ComboCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [PlayingHours] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Combos] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ComboItems] (
    [Id] uniqueidentifier NOT NULL,
    [ComboId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_ComboItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComboItems_Combos_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [Combos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ComboItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_ComboItems_ComboId] ON [ComboItems] ([ComboId]);
GO

CREATE INDEX [IX_ComboItems_ProductId] ON [ComboItems] ([ProductId]);
GO

CREATE UNIQUE INDEX [IX_Combos_ComboCode] ON [Combos] ([ComboCode]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260724113201_AddCombosTable', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TableSessions] ADD [ComboHours] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TableSessions] ADD [ComboId] uniqueidentifier NULL;
GO

ALTER TABLE [Combos] ADD [IsVip] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE INDEX [IX_TableSessions_ComboId] ON [TableSessions] ([ComboId]);
GO

ALTER TABLE [TableSessions] ADD CONSTRAINT [FK_TableSessions_Combos_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [Combos] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260724195051_AddComboToTableSession', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TableSessions] ADD [ComboDurationMinutes] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TableSessions] ADD [ComboEndTime] datetime2 NULL;
GO

ALTER TABLE [TableSessions] ADD [ComboPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Orders] ADD [IsComboOrder] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [SessionCombos] (
    [Id] uniqueidentifier NOT NULL,
    [TableSessionId] uniqueidentifier NOT NULL,
    [ComboId] uniqueidentifier NULL,
    [ComboName] nvarchar(150) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [DurationMinutes] int NOT NULL,
    [AppliedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SessionCombos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionCombos_Combos_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [Combos] ([Id]),
    CONSTRAINT [FK_SessionCombos_TableSessions_TableSessionId] FOREIGN KEY ([TableSessionId]) REFERENCES [TableSessions] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SessionCombos_ComboId] ON [SessionCombos] ([ComboId]);
GO

CREATE INDEX [IX_SessionCombos_TableSessionId] ON [SessionCombos] ([TableSessionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260724203844_AddIsComboOrderToOrder', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Products_ProductName] ON [Products];
GO

DROP INDEX [IX_Customers_PhoneNumber] ON [Customers];
GO

DROP INDEX [IX_Combos_ComboCode] ON [Combos];
GO

DROP INDEX [IX_BilliardTables_TableName] ON [BilliardTables];
GO

ALTER TABLE [Users] ADD [StoreId] uniqueidentifier NULL;
GO

ALTER TABLE [TableStatusHistories] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [TableSessions] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [Shifts] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [Products] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [Customers] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [Combos] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [ComboItems] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [Categories] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

ALTER TABLE [BilliardTables] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';
GO

CREATE TABLE [Stores] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Address] nvarchar(500) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Email] nvarchar(100) NULL,
    [LogoUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Stores] PRIMARY KEY ([Id])
);
GO

INSERT INTO [Stores] ([Id], [Name], [Address], [PhoneNumber], [Email], [LogoUrl], [IsActive], [CreatedAt]) VALUES ('00000000-0000-0000-0000-000000000001', N'Cửa hàng mặc định', NULL, NULL, NULL, NULL, 1, GETUTCDATE());
GO

UPDATE [Users] SET [StoreId] = '00000000-0000-0000-0000-000000000001' WHERE [StoreId] IS NULL AND [Role] <> 0;
GO

CREATE INDEX [IX_Users_StoreId] ON [Users] ([StoreId]);
GO

CREATE INDEX [IX_TableStatusHistories_StoreId] ON [TableStatusHistories] ([StoreId]);
GO

CREATE INDEX [IX_TableSessions_StoreId] ON [TableSessions] ([StoreId]);
GO

CREATE INDEX [IX_Shifts_StoreId] ON [Shifts] ([StoreId]);
GO

CREATE INDEX [IX_Products_StoreId] ON [Products] ([StoreId]);
GO

CREATE UNIQUE INDEX [IX_Products_StoreId_ProductName] ON [Products] ([StoreId], [ProductName]);
GO

CREATE INDEX [IX_Customers_StoreId] ON [Customers] ([StoreId]);
GO

CREATE UNIQUE INDEX [IX_Customers_StoreId_PhoneNumber] ON [Customers] ([StoreId], [PhoneNumber]);
GO

CREATE INDEX [IX_Combos_StoreId] ON [Combos] ([StoreId]);
GO

CREATE UNIQUE INDEX [IX_Combos_StoreId_ComboCode] ON [Combos] ([StoreId], [ComboCode]);
GO

CREATE INDEX [IX_ComboItems_StoreId] ON [ComboItems] ([StoreId]);
GO

CREATE INDEX [IX_Categories_StoreId] ON [Categories] ([StoreId]);
GO

CREATE INDEX [IX_BilliardTables_StoreId] ON [BilliardTables] ([StoreId]);
GO

CREATE UNIQUE INDEX [IX_BilliardTables_StoreId_TableName] ON [BilliardTables] ([StoreId], [TableName]);
GO

CREATE INDEX [IX_Stores_Name] ON [Stores] ([Name]);
GO

ALTER TABLE [BilliardTables] ADD CONSTRAINT [FK_BilliardTables_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Categories] ADD CONSTRAINT [FK_Categories_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ComboItems] ADD CONSTRAINT [FK_ComboItems_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Combos] ADD CONSTRAINT [FK_Combos_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Shifts] ADD CONSTRAINT [FK_Shifts_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [TableSessions] ADD CONSTRAINT [FK_TableSessions_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [TableStatusHistories] ADD CONSTRAINT [FK_TableStatusHistories_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261113070108_AddMultiTenantSupport', N'8.0.27');
GO

COMMIT;
GO
