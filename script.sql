USE [BoookingHotels]
GO
/****** Object:  Table [dbo].[AdminLogs]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminLogs](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[AdminId] [int] NOT NULL,
	[Action] [nvarchar](100) NOT NULL,
	[Entity] [nvarchar](100) NOT NULL,
	[EntityId] [int] NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Amenities]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Amenities](
	[AmenityId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Icon] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[AmenityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bookings]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookings](
	[BookingId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[HotelId] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CheckIn] [date] NOT NULL,
	[CheckOut] [date] NOT NULL,
	[GuestName] [nvarchar](100) NOT NULL,
	[GuestPhone] [nvarchar](20) NOT NULL,
	[SubTotal] [decimal](18, 2) NOT NULL,
	[Discount] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[Currency] [nvarchar](10) NULL,
	[CreatedAt] [datetime] NULL,
	[RoomId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hotels]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hotels](
	[HotelId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Address] [nvarchar](255) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[Country] [nvarchar](100) NOT NULL,
	[Status] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
PRIMARY KEY CLUSTERED 
(
	[HotelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Photos]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Photos](
	[PhotoId] [int] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NULL,
	[RoomId] [int] NULL,
	[Url] [nvarchar](255) NOT NULL,
	[SortOrder] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[PhotoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReviewPhotos]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReviewPhotos](
	[PhotoId] [int] IDENTITY(1,1) NOT NULL,
	[ReviewId] [int] NOT NULL,
	[Url] [nvarchar](255) NOT NULL,
	[SortOrder] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[PhotoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[HotelId] [int] NULL,
	[RoomId] [int] NULL,
	[Rating] [int] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoomAmenities]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoomAmenities](
	[RoomId] [int] NOT NULL,
	[AmenityId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomId] ASC,
	[AmenityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rooms]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rooms](
	[RoomId] [int] IDENTITY(1,1) NOT NULL,
	[HotelId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Capacity] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[BedType] [nvarchar](50) NULL,
	[Size] [int] NULL,
	[Status] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRoles](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Phone] [nvarchar](20) NULL,
	[Password] [nvarchar](200) NOT NULL,
	[FullName] [nvarchar](150) NULL,
	[AvatarUrl] [nvarchar](255) NULL,
	[Status] [bit] NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vouchers]    Script Date: 28/09/2025 3:19:53 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vouchers](
	[VoucherId] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[DiscountType] [nvarchar](10) NOT NULL,
	[DiscountValue] [decimal](18, 2) NOT NULL,
	[MinOrderValue] [decimal](18, 2) NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[Quantity] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[VoucherId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[AdminLogs] ON 

INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (1, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:30:53.270' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (2, 4, N'Vouchers', N'Admin', NULL, N'Admin thực hiện action Vouchers trên Admin', CAST(N'2025-09-28T12:30:59.813' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (3, 4, N'CreateVoucher', N'Admin', NULL, N'Admin thực hiện action CreateVoucher trên Admin', CAST(N'2025-09-28T12:31:01.063' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (4, 4, N'CreateVoucher', N'Admin', NULL, N'Admin thực hiện action CreateVoucher trên Admin', CAST(N'2025-09-28T12:31:15.817' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (5, 4, N'Vouchers', N'Admin', NULL, N'Admin thực hiện action Vouchers trên Admin', CAST(N'2025-09-28T12:31:15.843' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (6, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:32:03.020' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (7, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:36:07.350' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (8, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:37:56.273' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (9, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:39:19.023' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (10, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:40:38.503' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (11, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:41:34.227' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (12, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:41:55.920' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (13, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:43:33.057' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (14, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:43:48.070' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (15, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:43:50.073' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (16, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T12:44:47.060' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (17, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:25:16.857' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (18, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:25:36.897' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (19, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:26:14.140' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (21, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:26:31.387' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (22, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:28:08.053' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (23, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:28:10.900' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (24, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:28:12.653' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (25, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:30:44.380' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (26, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:30:46.547' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (27, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:30:47.793' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (28, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:34:41.167' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (29, 4, N'Rooms', N'Admin', NULL, N'Admin thực hiện action Rooms trên Admin', CAST(N'2025-09-28T13:34:42.710' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (30, 4, N'EditRoom', N'Admin', NULL, N'Admin thực hiện action EditRoom trên Admin', CAST(N'2025-09-28T13:34:44.273' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (31, 4, N'Rooms', N'Admin', NULL, N'Admin thực hiện action Rooms trên Admin', CAST(N'2025-09-28T13:34:46.493' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (32, 4, N'EditRoom', N'Admin', NULL, N'Admin thực hiện action EditRoom trên Admin', CAST(N'2025-09-28T13:34:47.980' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (33, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:34:49.387' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (34, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:34:50.640' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (35, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:34:52.067' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (36, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:34:59.617' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (37, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:35:00.980' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (38, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:37:24.297' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (39, 4, N'Rooms', N'Admin', NULL, N'Admin thực hiện action Rooms trên Admin', CAST(N'2025-09-28T13:37:26.160' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (40, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:37:28.763' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (41, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:37:30.757' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (42, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:44:16.057' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (43, 4, N'Rooms', N'Admin', NULL, N'Admin thực hiện action Rooms trên Admin', CAST(N'2025-09-28T13:44:19.020' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (44, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:44:21.010' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (45, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:44:22.767' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (46, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:45:33.143' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (47, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:45:38.043' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (48, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:45:39.420' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (49, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:46:40.207' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (50, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:46:51.257' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (51, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:46:52.540' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (52, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:46:56.950' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (53, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:46:57.690' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (54, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:47:29.653' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (55, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:47:31.350' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (56, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:47:32.393' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (57, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:48:03.297' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (58, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:48:04.977' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (59, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:48:06.127' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (60, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:48:10.703' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (61, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:49:09.397' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (62, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:49:11.083' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (63, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:49:12.300' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (64, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:49:30.293' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (65, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:49:32.103' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (66, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:49:33.507' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (67, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:50:50.967' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (68, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:50:53.560' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (69, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:53:22.370' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (70, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:53:33.510' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (71, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:53:37.623' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (72, 4, N'DeleteHotel', N'Admin', NULL, N'Admin thực hiện action DeleteHotel trên Admin', CAST(N'2025-09-28T13:53:44.317' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (73, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:53:44.337' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (74, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:56:57.173' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (75, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:56:59.410' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (76, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:57:00.563' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (77, 4, N'CreateHotel', N'Admin', NULL, N'Admin thực hiện action CreateHotel trên Admin', CAST(N'2025-09-28T13:57:05.700' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (78, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T13:59:20.790' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (79, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T13:59:22.193' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (80, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T13:59:23.567' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (81, 4, N'CreateHotel', N'Admin', NULL, N'Admin thực hiện action CreateHotel trên Admin', CAST(N'2025-09-28T13:59:26.980' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (82, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T14:03:00.543' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (83, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:03:01.893' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (84, 4, N'CreateHotel', N'Admin', NULL, N'Admin thực hiện action CreateHotel trên Admin', CAST(N'2025-09-28T14:03:02.757' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (85, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:03:19.827' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (86, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:03:21.777' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (87, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T14:04:37.090' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (88, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:04:39.050' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (89, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:04:40.560' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (90, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:04:46.557' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (91, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:04:46.570' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (92, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:05:11.627' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (93, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:06:01.197' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (94, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:01.217' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (95, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:06:03.710' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (96, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:07.413' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (97, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:06:08.340' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (98, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:17.167' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (99, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:06:18.960' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (100, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:21.210' AS DateTime))
GO
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (101, 4, N'EditHotel', N'Admin', NULL, N'Admin thực hiện action EditHotel trên Admin', CAST(N'2025-09-28T14:06:22.533' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (102, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:24.387' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (103, 4, N'DeleteHotel', N'Admin', NULL, N'Admin thực hiện action DeleteHotel trên Admin', CAST(N'2025-09-28T14:06:33.163' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (104, 4, N'Hotels', N'Admin', NULL, N'Admin thực hiện action Hotels trên Admin', CAST(N'2025-09-28T14:06:33.177' AS DateTime))
INSERT [dbo].[AdminLogs] ([LogId], [AdminId], [Action], [Entity], [EntityId], [Description], [CreatedAt]) VALUES (105, 4, N'Index', N'Admin', NULL, N'Admin thực hiện action Index trên Admin', CAST(N'2025-09-28T14:06:35.180' AS DateTime))
SET IDENTITY_INSERT [dbo].[AdminLogs] OFF
GO
SET IDENTITY_INSERT [dbo].[Amenities] ON 

INSERT [dbo].[Amenities] ([AmenityId], [Name], [Icon]) VALUES (1, N'Wi-Fi', N'bi-wifi')
INSERT [dbo].[Amenities] ([AmenityId], [Name], [Icon]) VALUES (2, N'Pool', N'bi-water')
INSERT [dbo].[Amenities] ([AmenityId], [Name], [Icon]) VALUES (3, N'Breakfast', N'bi-cup-hot')
INSERT [dbo].[Amenities] ([AmenityId], [Name], [Icon]) VALUES (4, N'Parking', N'bi-car-front')
SET IDENTITY_INSERT [dbo].[Amenities] OFF
GO
SET IDENTITY_INSERT [dbo].[Bookings] ON 

INSERT [dbo].[Bookings] ([BookingId], [UserId], [HotelId], [Status], [CheckIn], [CheckOut], [GuestName], [GuestPhone], [SubTotal], [Discount], [Total], [Currency], [CreatedAt], [RoomId]) VALUES (5, 2, 1, N'Paid', CAST(N'2025-09-28' AS Date), CAST(N'2025-09-29' AS Date), N'123', N'123', CAST(10000.00 AS Decimal(18, 2)), NULL, CAST(10000.00 AS Decimal(18, 2)), N'VND', CAST(N'2025-09-28T03:11:28.853' AS DateTime), 9)
INSERT [dbo].[Bookings] ([BookingId], [UserId], [HotelId], [Status], [CheckIn], [CheckOut], [GuestName], [GuestPhone], [SubTotal], [Discount], [Total], [Currency], [CreatedAt], [RoomId]) VALUES (6, 2, 1, N'Canceled', CAST(N'2025-09-28' AS Date), CAST(N'2025-09-29' AS Date), N'123', N'123', CAST(1000000.00 AS Decimal(18, 2)), NULL, CAST(1000000.00 AS Decimal(18, 2)), N'VND', CAST(N'2025-09-28T09:41:34.663' AS DateTime), 10)
INSERT [dbo].[Bookings] ([BookingId], [UserId], [HotelId], [Status], [CheckIn], [CheckOut], [GuestName], [GuestPhone], [SubTotal], [Discount], [Total], [Currency], [CreatedAt], [RoomId]) VALUES (7, 2, 1, N'Canceled', CAST(N'2025-09-29' AS Date), CAST(N'2025-09-30' AS Date), N'123', N'123', CAST(1000000.00 AS Decimal(18, 2)), NULL, CAST(1000000.00 AS Decimal(18, 2)), N'VND', CAST(N'2025-09-28T09:42:30.567' AS DateTime), 10)
INSERT [dbo].[Bookings] ([BookingId], [UserId], [HotelId], [Status], [CheckIn], [CheckOut], [GuestName], [GuestPhone], [SubTotal], [Discount], [Total], [Currency], [CreatedAt], [RoomId]) VALUES (8, 2, 1, N'Paid', CAST(N'2025-09-28' AS Date), CAST(N'2025-09-30' AS Date), N'123', N'123', CAST(2000000.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'VND', CAST(N'2025-09-28T12:16:46.190' AS DateTime), 10)
SET IDENTITY_INSERT [dbo].[Bookings] OFF
GO
SET IDENTITY_INSERT [dbo].[Hotels] ON 

INSERT [dbo].[Hotels] ([HotelId], [Name], [Description], [Address], [City], [Country], [Status], [CreatedAt], [Latitude], [Longitude]) VALUES (1, N'Paradise Resort', N'Khu nghỉ dưỡng cao cấp gần biển', N'123 Đường Biển', N'Đà Nẵng', N'Việt Nam', 1, NULL, 10.750226, 106.566826)
INSERT [dbo].[Hotels] ([HotelId], [Name], [Description], [Address], [City], [Country], [Status], [CreatedAt], [Latitude], [Longitude]) VALUES (2, N'Sapa Highland', N'Khách sạn trên núi với view thung lũng', N'89 Fansipan', N'Lào Cai', N'Việt Nam', 1, NULL, NULL, NULL)
INSERT [dbo].[Hotels] ([HotelId], [Name], [Description], [Address], [City], [Country], [Status], [CreatedAt], [Latitude], [Longitude]) VALUES (4, N'123', N'123', N'1234', N'123', N'123', 0, CAST(N'2025-09-28T10:09:00.000' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[Hotels] OFF
GO
SET IDENTITY_INSERT [dbo].[Photos] ON 

INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (21, NULL, 9, N'/Image/1.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (22, NULL, 9, N'/Image/2.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (23, NULL, 9, N'/Image/3.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (24, 1, NULL, N'/Image/4.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (25, 2, NULL, N'/Image/5.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (26, NULL, 10, N'/Image/6.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (27, NULL, 10, N'/Image/7.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (28, NULL, 10, N'/Image/8.jpg', 0)
INSERT [dbo].[Photos] ([PhotoId], [HotelId], [RoomId], [Url], [SortOrder]) VALUES (29, 4, NULL, N'/Image/9.jpg', 0)
SET IDENTITY_INSERT [dbo].[Photos] OFF
GO
SET IDENTITY_INSERT [dbo].[ReviewPhotos] ON 

INSERT [dbo].[ReviewPhotos] ([PhotoId], [ReviewId], [Url], [SortOrder]) VALUES (2, 9, N'/Image/reviews/580666be-534e-4369-ae56-34509c0980a3.jpg', 0)
INSERT [dbo].[ReviewPhotos] ([PhotoId], [ReviewId], [Url], [SortOrder]) VALUES (3, 9, N'/Image/reviews/406cb6d8-d402-4a6d-8f27-bf246a91c32b.jpg', 0)
SET IDENTITY_INSERT [dbo].[ReviewPhotos] OFF
GO
SET IDENTITY_INSERT [dbo].[Reviews] ON 

INSERT [dbo].[Reviews] ([ReviewId], [UserId], [HotelId], [RoomId], [Rating], [Comment], [CreatedAt]) VALUES (9, 2, 1, 10, 5, N'12312312312', CAST(N'2025-09-28T11:16:17.363' AS DateTime))
SET IDENTITY_INSERT [dbo].[Reviews] OFF
GO
SET IDENTITY_INSERT [dbo].[Roles] ON 

INSERT [dbo].[Roles] ([RoleId], [RoleName]) VALUES (1, N'Admin')
SET IDENTITY_INSERT [dbo].[Roles] OFF
GO
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (9, 1)
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (9, 2)
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (9, 3)
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (10, 1)
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (10, 2)
INSERT [dbo].[RoomAmenities] ([RoomId], [AmenityId]) VALUES (10, 3)
GO
SET IDENTITY_INSERT [dbo].[Rooms] ON 

INSERT [dbo].[Rooms] ([RoomId], [HotelId], [Name], [Capacity], [Price], [BedType], [Size], [Status]) VALUES (9, 1, N'1', 2, CAST(10000.00 AS Decimal(18, 2)), N'Queen', 123, 1)
INSERT [dbo].[Rooms] ([RoomId], [HotelId], [Name], [Capacity], [Price], [BedType], [Size], [Status]) VALUES (10, 1, N'Hotel test', 2, CAST(1000000.00 AS Decimal(18, 2)), N'Queen', 30, 1)
SET IDENTITY_INSERT [dbo].[Rooms] OFF
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (4, 1)
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([UserId], [UserName], [Email], [Phone], [Password], [FullName], [AvatarUrl], [Status], [CreatedAt]) VALUES (1, N'kinggynstar@gmail.com', N'kinggynstar@gmail.com', N'0353803490', N'$2b$10$mJdjAm/PsTFNgG5Agg8wUeGfzjqmGHRFTDlQyc5DsXSDan4WBHw62', NULL, NULL, 0, CAST(N'2025-09-27T14:34:45.420' AS DateTime))
INSERT [dbo].[Users] ([UserId], [UserName], [Email], [Phone], [Password], [FullName], [AvatarUrl], [Status], [CreatedAt]) VALUES (2, N'hoahuit@gmail.com', N'hoahuit@gmail.com', N'123', N'$2b$10$PwzIoR34oa9zMWseq5JQiO1c1/OIQhhODuLqMb6TsMxXuUYnX7PFi', NULL, NULL, 1, CAST(N'2025-09-27T15:46:49.680' AS DateTime))
INSERT [dbo].[Users] ([UserId], [UserName], [Email], [Phone], [Password], [FullName], [AvatarUrl], [Status], [CreatedAt]) VALUES (3, N'nguyentuankiet2003py@gmail.com', N'nguyentuankiet2003py@gmail.com', N'12412', N'$2b$10$jEpihD9A61vPIs4WWsgTx.WTg1IyuWGW1ETa1LxP5fDcs6dVzWGAC', NULL, NULL, 0, CAST(N'2025-09-27T22:01:05.793' AS DateTime))
INSERT [dbo].[Users] ([UserId], [UserName], [Email], [Phone], [Password], [FullName], [AvatarUrl], [Status], [CreatedAt]) VALUES (4, N'admin', N'admin@boookinghotels.com', N'0123456789', N'$2b$10$PwzIoR34oa9zMWseq5JQiO1c1/OIQhhODuLqMb6TsMxXuUYnX7PFi', N'System Administrator', NULL, 1, CAST(N'2025-09-28T01:43:31.233' AS DateTime))
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[Vouchers] ON 

INSERT [dbo].[Vouchers] ([VoucherId], [Code], [Description], [DiscountType], [DiscountValue], [MinOrderValue], [ExpiryDate], [Quantity], [IsActive], [CreatedAt], [UserId]) VALUES (1, N'ADM-D665C3D1', N'Voucher bồi hoàn cho booking 6', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(N'2025-10-28T11:56:55.153' AS DateTime), 0, 0, CAST(N'2025-09-28T11:56:55.153' AS DateTime), NULL)
INSERT [dbo].[Vouchers] ([VoucherId], [Code], [Description], [DiscountType], [DiscountValue], [MinOrderValue], [ExpiryDate], [Quantity], [IsActive], [CreatedAt], [UserId]) VALUES (2, N'VOUCER001', N'VOUCER001', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(10000.00 AS Decimal(18, 2)), CAST(N'2025-09-29T00:00:00.000' AS DateTime), 2, 1, CAST(N'2025-09-28T12:31:15.793' AS DateTime), NULL)
SET IDENTITY_INSERT [dbo].[Vouchers] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Roles__8A2B61603004BE36]    Script Date: 28/09/2025 3:19:53 pm ******/
ALTER TABLE [dbo].[Roles] ADD UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__5C7E359EE91E0878]    Script Date: 28/09/2025 3:19:53 pm ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Phone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D1053466371E71]    Script Date: 28/09/2025 3:19:53 pm ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Vouchers__A25C5AA75A743EFD]    Script Date: 28/09/2025 3:19:53 pm ******/
ALTER TABLE [dbo].[Vouchers] ADD UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AdminLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT ((0)) FOR [Discount]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT ('VND') FOR [Currency]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Hotels] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Hotels] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Photos] ADD  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[ReviewPhotos] ADD  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Reviews] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Rooms] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Vouchers] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[Vouchers] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Vouchers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[AdminLogs]  WITH CHECK ADD  CONSTRAINT [FK_AdminLogs_Admin] FOREIGN KEY([AdminId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[AdminLogs] CHECK CONSTRAINT [FK_AdminLogs_Admin]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hotels] ([HotelId])
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Photos]  WITH CHECK ADD FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hotels] ([HotelId])
GO
ALTER TABLE [dbo].[Photos]  WITH CHECK ADD FOREIGN KEY([RoomId])
REFERENCES [dbo].[Rooms] ([RoomId])
GO
ALTER TABLE [dbo].[ReviewPhotos]  WITH CHECK ADD FOREIGN KEY([ReviewId])
REFERENCES [dbo].[Reviews] ([ReviewId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hotels] ([HotelId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD FOREIGN KEY([RoomId])
REFERENCES [dbo].[Rooms] ([RoomId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[RoomAmenities]  WITH CHECK ADD FOREIGN KEY([AmenityId])
REFERENCES [dbo].[Amenities] ([AmenityId])
GO
ALTER TABLE [dbo].[RoomAmenities]  WITH CHECK ADD FOREIGN KEY([RoomId])
REFERENCES [dbo].[Rooms] ([RoomId])
GO
ALTER TABLE [dbo].[Rooms]  WITH CHECK ADD FOREIGN KEY([HotelId])
REFERENCES [dbo].[Hotels] ([HotelId])
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Vouchers]  WITH CHECK ADD  CONSTRAINT [FK_Vouchers_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Vouchers] CHECK CONSTRAINT [FK_Vouchers_Users]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
