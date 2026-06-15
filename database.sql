CREATE TABLE [BilliardTables] (
    [Id] uniqueidentifier NOT NULL,
    [TableName] nvarchar(50) NOT NULL,
    [TableType] nvarchar(50) NOT NULL,
    [PricePerHour] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [Description] nvarchar(500) NULL,
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
    [Email] nvarchar(100) NULL,
    [ProfilePictureUrl] nvarchar(500) NULL,
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
    [Description] nvarchar(500) NULL,
    [StockQuantity] int NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsAvailable] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
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
    [DurationHours] int NOT NULL,
    [DurationMinutes] int NULL,
    [RemainingMinutes] int NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    [IsFinished] bit NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TableSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TableSessions_BilliardTables_TableId] FOREIGN KEY ([TableId]) REFERENCES [BilliardTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
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


CREATE UNIQUE INDEX [IX_BilliardTables_TableName] ON [BilliardTables] ([TableName]);
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


CREATE UNIQUE INDEX [IX_Products_ProductName] ON [Products] ([ProductName]);
GO


CREATE INDEX [IX_Shifts_UserId] ON [Shifts] ([UserId]);
GO


CREATE INDEX [IX_TableSessions_TableId] ON [TableSessions] ([TableId]);
GO


CREATE INDEX [IX_TableSessions_UserId] ON [TableSessions] ([UserId]);
GO


CREATE INDEX [IX_TableStatusHistories_ChangedById] ON [TableStatusHistories] ([ChangedById]);
GO


CREATE INDEX [IX_TableStatusHistories_TableId] ON [TableStatusHistories] ([TableId]);
GO


