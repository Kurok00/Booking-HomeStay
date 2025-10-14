CREATE TABLE [Amenities] (
    [AmenityId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Icon] nvarchar(max) NULL,
    CONSTRAINT [PK_Amenities] PRIMARY KEY ([AmenityId])
);
GO


CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);
GO


CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [UserName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [Password] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NULL,
    [AvatarUrl] nvarchar(max) NULL,
    [Status] bit NOT NULL,
    [CreatedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO


CREATE TABLE [AdminLogs] (
    [LogId] int NOT NULL IDENTITY,
    [AdminId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Entity] nvarchar(max) NOT NULL,
    [EntityId] int NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AdminLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_AdminLogs_Users_AdminId] FOREIGN KEY ([AdminId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Blogs] (
    [BlogId] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [ShortDescription] nvarchar(500) NULL,
    [Content] nvarchar(max) NOT NULL,
    [Thumbnail] nvarchar(500) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [Author] nvarchar(255) NOT NULL,
    [ReviewerId] int NOT NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([BlogId]),
    CONSTRAINT [FK_Blogs_Users_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Hotels] (
    [HotelId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Address] nvarchar(255) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Country] nvarchar(100) NOT NULL,
    [Status] bit NULL,
    [CreatedAt] datetime2 NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [IsUserHostCreated] bit NULL,
    [IsUserHostCreatedDate] datetime2 NULL,
    [IsApproved] bit NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_Hotels] PRIMARY KEY ([HotelId]),
    CONSTRAINT [FK_Hotels_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [UserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Vouchers] (
    [VoucherId] int NOT NULL IDENTITY,
    [Code] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DiscountType] nvarchar(max) NOT NULL,
    [DiscountValue] decimal(18,2) NOT NULL,
    [MinOrderValue] decimal(18,2) NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [Quantity] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserId] int NULL,
    CONSTRAINT [PK_Vouchers] PRIMARY KEY ([VoucherId]),
    CONSTRAINT [FK_Vouchers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Rooms] (
    [RoomId] int NOT NULL IDENTITY,
    [HotelId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Capacity] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [BedType] nvarchar(max) NULL,
    [Size] int NULL,
    [Status] bit NOT NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomId]),
    CONSTRAINT [FK_Rooms_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([HotelId]) ON DELETE CASCADE
);
GO


CREATE TABLE [UserVouchers] (
    [UserVoucherId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [VoucherId] int NOT NULL,
    CONSTRAINT [PK_UserVouchers] PRIMARY KEY ([UserVoucherId]),
    CONSTRAINT [FK_UserVouchers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserVouchers_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([VoucherId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Bookings] (
    [BookingId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [HotelId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [RoomId] int NOT NULL,
    [CheckIn] datetime2 NOT NULL,
    [CheckOut] datetime2 NOT NULL,
    [GuestName] nvarchar(max) NOT NULL,
    [GuestPhone] nvarchar(max) NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NULL,
    [Total] decimal(18,2) NOT NULL,
    [Currency] nvarchar(max) NULL,
    [CreatedAt] datetime2 NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
    CONSTRAINT [FK_Bookings_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([HotelId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookings_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Photos] (
    [PhotoId] int NOT NULL IDENTITY,
    [HotelId] int NULL,
    [RoomId] int NULL,
    [Url] nvarchar(max) NOT NULL,
    [SortOrder] int NULL,
    CONSTRAINT [PK_Photos] PRIMARY KEY ([PhotoId]),
    CONSTRAINT [FK_Photos_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([HotelId]),
    CONSTRAINT [FK_Photos_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId])
);
GO


CREATE TABLE [Reviews] (
    [ReviewId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [HotelId] int NULL,
    [RoomId] int NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsVisible] bit NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
    CONSTRAINT [FK_Reviews_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([HotelId]),
    CONSTRAINT [FK_Reviews_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId]),
    CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [RoomAmenities] (
    [RoomId] int NOT NULL,
    [AmenityId] int NOT NULL,
    CONSTRAINT [PK_RoomAmenities] PRIMARY KEY ([RoomId], [AmenityId]),
    CONSTRAINT [FK_RoomAmenities_Amenities_AmenityId] FOREIGN KEY ([AmenityId]) REFERENCES [Amenities] ([AmenityId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RoomAmenities_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ReviewPhotos] (
    [PhotoId] int NOT NULL IDENTITY,
    [ReviewId] int NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_ReviewPhotos] PRIMARY KEY ([PhotoId]),
    CONSTRAINT [FK_ReviewPhotos_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([ReviewId]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AdminLogs_AdminId] ON [AdminLogs] ([AdminId]);
GO


CREATE INDEX [IX_Blogs_ReviewerId] ON [Blogs] ([ReviewerId]);
GO


CREATE INDEX [IX_Bookings_HotelId] ON [Bookings] ([HotelId]);
GO


CREATE INDEX [IX_Bookings_RoomId] ON [Bookings] ([RoomId]);
GO


CREATE INDEX [IX_Bookings_UserId] ON [Bookings] ([UserId]);
GO


CREATE INDEX [IX_Hotels_CreatedBy] ON [Hotels] ([CreatedBy]);
GO


CREATE INDEX [IX_Photos_HotelId] ON [Photos] ([HotelId]);
GO


CREATE INDEX [IX_Photos_RoomId] ON [Photos] ([RoomId]);
GO


CREATE INDEX [IX_ReviewPhotos_ReviewId] ON [ReviewPhotos] ([ReviewId]);
GO


CREATE INDEX [IX_Reviews_HotelId] ON [Reviews] ([HotelId]);
GO


CREATE INDEX [IX_Reviews_RoomId] ON [Reviews] ([RoomId]);
GO


CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);
GO


CREATE INDEX [IX_RoomAmenities_AmenityId] ON [RoomAmenities] ([AmenityId]);
GO


CREATE INDEX [IX_Rooms_HotelId] ON [Rooms] ([HotelId]);
GO


CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO


CREATE INDEX [IX_UserVouchers_UserId] ON [UserVouchers] ([UserId]);
GO


CREATE INDEX [IX_UserVouchers_VoucherId] ON [UserVouchers] ([VoucherId]);
GO


CREATE INDEX [IX_Vouchers_UserId] ON [Vouchers] ([UserId]);
GO


