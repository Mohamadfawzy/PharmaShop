USE [master]
GO
/****** Object:  Database [pharma_shope_db_temp]    Script Date: 24/01/2026 05:33 PM ******/
CREATE DATABASE [pharma_shope_db_temp]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'pharma_shope_db_temp', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\pharma_shope_db_temp.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'pharma_shope_db_temp_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\pharma_shope_db_temp_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [pharma_shope_db_temp] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [pharma_shope_db_temp].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [pharma_shope_db_temp] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ARITHABORT OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [pharma_shope_db_temp] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [pharma_shope_db_temp] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET  ENABLE_BROKER 
GO
ALTER DATABASE [pharma_shope_db_temp] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [pharma_shope_db_temp] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET RECOVERY FULL 
GO
ALTER DATABASE [pharma_shope_db_temp] SET  MULTI_USER 
GO
ALTER DATABASE [pharma_shope_db_temp] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [pharma_shope_db_temp] SET DB_CHAINING OFF 
GO
ALTER DATABASE [pharma_shope_db_temp] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [pharma_shope_db_temp] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [pharma_shope_db_temp] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [pharma_shope_db_temp] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'pharma_shope_db_temp', N'ON'
GO
ALTER DATABASE [pharma_shope_db_temp] SET QUERY_STORE = ON
GO
ALTER DATABASE [pharma_shope_db_temp] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [pharma_shope_db_temp]
GO
/****** Object:  Schema [audit]    Script Date: 24/01/2026 05:33 PM ******/
CREATE SCHEMA [audit]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Brands]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Brands](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[NameEn] [nvarchar](150) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Brands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentCategoryId] [int] NULL,
	[Name] [nvarchar](200) NOT NULL,
	[NameEn] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageId] [nvarchar](128) NULL,
	[ImageFormat] [tinyint] NULL,
	[ImageVersion] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[ImageUpdatedAt] [datetime] NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[PharmacyId] [int] NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[FullNameEn] [nvarchar](200) NOT NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[Gender] [nvarchar](10) NULL,
	[DateOfBirth] [date] NULL,
	[Email] [nvarchar](150) NULL,
	[NationalId] [nvarchar](20) NULL,
	[CustomerType] [nvarchar](50) NULL,
	[Points] [int] NOT NULL,
	[PointsExpiryDate] [datetime] NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoginAudits]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoginAudits](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedAtUtc] [datetime2](3) NOT NULL,
	[Outcome] [nvarchar](20) NOT NULL,
	[FailureReason] [nvarchar](50) NULL,
	[UserId] [int] NULL,
	[IdentifierMasked] [nvarchar](256) NULL,
	[IdentifierHash] [varbinary](32) NULL,
	[IpAddress] [nvarchar](45) NULL,
	[UserAgent] [nvarchar](512) NULL,
	[TraceId] [nvarchar](64) NULL,
	[PharmacyId] [int] NULL,
	[LatencyMs] [int] NULL,
 CONSTRAINT [PK_LoginAudits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItemPromotions]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItemPromotions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [bigint] NOT NULL,
	[PromotionId] [int] NOT NULL,
	[EffectId] [bigint] NULL,
	[AppliedQty] [int] NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[MetadataJson] [nvarchar](max) NULL,
	[AppliedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_OrderItemPromotions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItems]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderId] [bigint] NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[StoreId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductUnitId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[ListUnitPrice] [decimal](18, 2) NOT NULL,
	[OriginalLineTotal] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[FinalLineTotal] [decimal](18, 2) NOT NULL,
	[VatRate] [decimal](5, 2) NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderPromotions]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderPromotions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderId] [bigint] NOT NULL,
	[PromotionId] [int] NOT NULL,
	[EffectId] [bigint] NULL,
	[CouponId] [int] NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[MetadataJson] [nvarchar](max) NULL,
	[AppliedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_OrderPromotions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[StoreId] [int] NOT NULL,
	[CustomerId] [int] NULL,
	[OrderNumber] [nvarchar](30) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[Channel] [tinyint] NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[DiscountTotal] [decimal](18, 2) NOT NULL,
	[TaxTotal] [decimal](18, 2) NOT NULL,
	[ShippingFee] [decimal](18, 2) NOT NULL,
	[GrandTotal] [decimal](18, 2) NOT NULL,
	[PointsEarned] [int] NOT NULL,
	[PointsRedeemed] [int] NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pharmacies]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pharmacies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[NameEn] [nvarchar](200) NOT NULL,
	[OwnerName] [nvarchar](150) NULL,
	[LicenseNumber] [nvarchar](100) NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[Email] [nvarchar](150) NULL,
	[Address] [nvarchar](300) NULL,
	[Latitude] [decimal](10, 7) NULL,
	[Longitude] [decimal](10, 7) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Pharmacies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pharmacists]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pharmacists](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NULL,
	[FullNameEn] [nvarchar](200) NOT NULL,
	[Specialty] [nvarchar](100) NULL,
	[UserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PointSettings]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PointSettings](
	[PharmacyId] [int] NOT NULL,
	[EarnEnabled] [bit] NOT NULL,
	[EarnPerAmount] [decimal](18, 2) NOT NULL,
	[EarnPoints] [int] NOT NULL,
	[RedeemEnabled] [bit] NOT NULL,
	[PointValueEGP] [decimal](18, 4) NOT NULL,
	[MaxRedeemPercent] [decimal](5, 2) NOT NULL,
	[PointsExpireDays] [int] NULL,
	[UpdatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PointSettings] PRIMARY KEY CLUSTERED 
(
	[PharmacyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PointsTransactions]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PointsTransactions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[TxType] [tinyint] NOT NULL,
	[Points] [int] NOT NULL,
	[ReferenceType] [tinyint] NULL,
	[ReferenceId] [bigint] NULL,
	[ExpiresAt] [datetime2](0) NULL,
	[Note] [nvarchar](300) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[OrderId] [bigint] NULL,
	[PromotionId] [int] NULL,
	[PromotionEffectId] [bigint] NULL,
 CONSTRAINT [PK_PointsTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductBatches]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductBatches](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[StoreId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductUnitId] [int] NOT NULL,
	[BatchNumber] [nvarchar](80) NOT NULL,
	[ExpirationDate] [date] NULL,
	[ReceivedAt] [datetime2](0) NOT NULL,
	[QuantityReceived] [int] NOT NULL,
	[QuantityOnHand] [int] NOT NULL,
	[CostPrice] [decimal](18, 2) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ProductBatches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductImages]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductImages](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ImageUrl] [nvarchar](600) NOT NULL,
	[ThumbnailUrl] [nvarchar](600) NULL,
	[AltText] [nvarchar](200) NULL,
	[SortOrder] [int] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductInventory]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductInventory](
	[PharmacyId] [int] NOT NULL,
	[StoreId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductUnitId] [int] NOT NULL,
	[QuantityOnHand] [int] NOT NULL,
	[ReservedQty] [int] NOT NULL,
	[MinStockLevel] [int] NULL,
	[MaxStockLevel] [int] NULL,
	[LastStockUpdateAt] [datetime2](0) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ProductInventory] PRIMARY KEY CLUSTERED 
(
	[StoreId] ASC,
	[ProductUnitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[BrandId] [int] NULL,
	[Barcode] [varchar](50) NULL,
	[InternationalCode] [varchar](50) NULL,
	[StockProductCode] [varchar](50) NULL,
	[Name] [nvarchar](250) NOT NULL,
	[NameEn] [nvarchar](250) NOT NULL,
	[Slug] [nvarchar](300) NULL,
	[Description] [nvarchar](max) NULL,
	[DescriptionEn] [nvarchar](max) NULL,
	[SearchKeywords] [nvarchar](500) NULL,
	[NormalizedName] [nvarchar](250) NULL,
	[NormalizedNameEn] [nvarchar](250) NULL,
	[DosageForm] [nvarchar](50) NULL,
	[Strength] [nvarchar](50) NULL,
	[PackSize] [nvarchar](80) NULL,
	[Unit] [nvarchar](30) NULL,
	[RequiresPrescription] [bit] NOT NULL,
	[EarnPoints] [bit] NOT NULL,
	[HasExpiry] [bit] NOT NULL,
	[AgeRestricted] [bit] NOT NULL,
	[MinAge] [int] NULL,
	[RequiresColdChain] [bit] NOT NULL,
	[ControlledSubstance] [bit] NOT NULL,
	[StorageConditions] [nvarchar](200) NULL,
	[IsTaxable] [bit] NOT NULL,
	[VatRate] [decimal](5, 2) NOT NULL,
	[TaxCategoryCode] [nvarchar](30) NULL,
	[MinOrderQty] [int] NOT NULL,
	[MaxOrderQty] [int] NULL,
	[MaxPerDayQty] [int] NULL,
	[IsReturnable] [bit] NOT NULL,
	[ReturnWindowDays] [int] NULL,
	[AllowSplitSale] [bit] NOT NULL,
	[SplitLevel] [tinyint] NULL,
	[WeightGrams] [int] NULL,
	[LengthMm] [int] NULL,
	[WidthMm] [int] NULL,
	[HeightMm] [int] NULL,
	[TrackInventory] [bit] NOT NULL,
	[IsFeatured] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsIntegrated] [bit] NOT NULL,
	[IntegratedAt] [datetime2](0) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[UpdatedBy] [nvarchar](100) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductTags]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTags](
	[ProductId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ProductTags] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductUnits]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductUnits](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[UnitCode] [varchar](50) NULL,
	[SKU] [varchar](50) NULL,
	[ParentProductUnitId] [int] NULL,
	[UnitsPerParent] [decimal](18, 3) NULL,
	[BaseUnitId] [int] NULL,
	[BaseQuantity] [decimal](18, 3) NULL,
	[CurrencyCode] [char](3) NOT NULL,
	[CostPrice] [decimal](18, 2) NULL,
	[ListPrice] [decimal](18, 2) NOT NULL,
	[PriceUpdatedAt] [datetime2](0) NULL,
	[IsPrimary] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ProductUnits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ProductUnits_Id_ProductId] UNIQUE NONCLUSTERED 
(
	[Id] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionBuyXGetYRules]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionBuyXGetYRules](
	[EffectId] [bigint] NOT NULL,
	[BuyProductUnitId] [int] NULL,
	[BuyProductId] [int] NULL,
	[BuyCategoryId] [int] NULL,
	[BuyTagId] [int] NULL,
	[BuyQty] [int] NOT NULL,
	[GetProductUnitId] [int] NULL,
	[GetProductId] [int] NULL,
	[GetCategoryId] [int] NULL,
	[GetTagId] [int] NULL,
	[GetQty] [int] NOT NULL,
	[RewardType] [tinyint] NOT NULL,
	[RewardPercent] [decimal](9, 4) NULL,
	[RewardAmount] [decimal](18, 2) NULL,
	[MaxSetsPerOrder] [int] NULL,
 CONSTRAINT [PK_PromotionBuyXGetYRules] PRIMARY KEY CLUSTERED 
(
	[EffectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionConditions]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionConditions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PromotionId] [int] NOT NULL,
	[ConditionType] [tinyint] NOT NULL,
	[IntValue] [int] NULL,
	[DecimalValue] [decimal](18, 2) NULL,
	[StringValue] [nvarchar](200) NULL,
	[BitValue] [bit] NULL,
	[JsonValue] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PromotionConditions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionCoupons]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionCoupons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PromotionId] [int] NOT NULL,
	[Code] [nvarchar](40) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[MaxUsesTotal] [int] NULL,
	[MaxUsesPerCustomer] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PromotionCoupons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionEffects]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionEffects](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PromotionId] [int] NOT NULL,
	[EffectType] [tinyint] NOT NULL,
	[PercentValue] [decimal](9, 4) NULL,
	[AmountValue] [decimal](18, 2) NULL,
	[PointsValue] [int] NULL,
	[Multiplier] [decimal](9, 4) NULL,
	[MaxDiscountAmount] [decimal](18, 2) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PromotionEffects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Promotions]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Promotions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[StartAt] [datetime2](0) NOT NULL,
	[EndAt] [datetime2](0) NOT NULL,
	[Priority] [int] NOT NULL,
	[StackPolicy] [tinyint] NOT NULL,
	[StackGroupKey] [nvarchar](80) NULL,
	[AppliesToAllProducts] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionSchedules]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionSchedules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PromotionId] [int] NOT NULL,
	[DaysMask] [int] NOT NULL,
	[StartTime] [time](0) NULL,
	[EndTime] [time](0) NULL,
 CONSTRAINT [PK_PromotionSchedules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionTargets]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionTargets](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PromotionId] [int] NOT NULL,
	[ProductId] [int] NULL,
	[ProductUnitId] [int] NULL,
	[CategoryId] [int] NULL,
	[TagId] [int] NULL,
	[ProductIdForUnit] [int] NULL,
	[IncludeSubcategories] [bit] NOT NULL,
	[MinQty] [int] NULL,
	[MaxQty] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PromotionTargets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionUsageLimits]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionUsageLimits](
	[PromotionId] [int] NOT NULL,
	[MaxRedemptionsTotal] [int] NULL,
	[MaxRedemptionsPerCustomer] [int] NULL,
	[MaxRedemptionsPerOrder] [int] NULL,
 CONSTRAINT [PK_PromotionUsageLimits] PRIMARY KEY CLUSTERED 
(
	[PromotionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stores]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stores](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Code] [varchar](30) NULL,
	[Address] [nvarchar](300) NULL,
	[IsDefault] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[DeletedAt] [datetime2](0) NULL,
	[DeletedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Stores] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PharmacyId] [int] NOT NULL,
	[Name] [nvarchar](80) NOT NULL,
	[NameEn] [nvarchar](80) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Units]    Script Date: 24/01/2026 05:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Units](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[NameAr] [nvarchar](60) NOT NULL,
	[NameEn] [nvarchar](60) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Units] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [PhoneNumberIndex]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [PhoneNumberIndex] ON [dbo].[AspNetUsers]
(
	[PhoneNumber] ASC
)
WHERE ([PhoneNumber] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Brands_Pharmacy_Name]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Brands_Pharmacy_Name] ON [dbo].[Brands]
(
	[PharmacyId] ASC,
	[Name] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Categories_ImageId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Categories_ImageId] ON [dbo].[Categories]
(
	[ImageId] ASC
)
WHERE ([ImageId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Categories_ParentCategoryId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryId] ON [dbo].[Categories]
(
	[ParentCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LoginAudits_CreatedAtUtc]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoginAudits_CreatedAtUtc] ON [dbo].[LoginAudits]
(
	[CreatedAtUtc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_LoginAudits_Ip_CreatedAtUtc]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoginAudits_Ip_CreatedAtUtc] ON [dbo].[LoginAudits]
(
	[IpAddress] ASC,
	[CreatedAtUtc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LoginAudits_UserId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoginAudits_UserId] ON [dbo].[LoginAudits]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItemPromotions_OrderItemId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItemPromotions_OrderItemId] ON [dbo].[OrderItemPromotions]
(
	[OrderItemId] ASC
)
INCLUDE([PromotionId],[DiscountAmount],[AppliedQty]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItems_OrderId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [dbo].[OrderItems]
(
	[OrderId] ASC
)
INCLUDE([ProductId],[ProductUnitId],[Quantity],[FinalLineTotal],[DiscountAmount]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderPromotions_OrderId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderPromotions_OrderId] ON [dbo].[OrderPromotions]
(
	[OrderId] ASC
)
INCLUDE([PromotionId],[DiscountAmount]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_Pharmacy_CreatedAt]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_Pharmacy_CreatedAt] ON [dbo].[Orders]
(
	[PharmacyId] ASC,
	[CreatedAt] DESC
)
INCLUDE([Status],[GrandTotal],[CustomerId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Orders_Pharmacy_OrderNumber]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Orders_Pharmacy_OrderNumber] ON [dbo].[Orders]
(
	[PharmacyId] ASC,
	[OrderNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PointsTransactions_Customer_CreatedAt]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PointsTransactions_Customer_CreatedAt] ON [dbo].[PointsTransactions]
(
	[CustomerId] ASC,
	[CreatedAt] DESC
)
INCLUDE([TxType],[Points],[ReferenceType],[ReferenceId],[ExpiresAt]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductBatches_Product]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductBatches_Product] ON [dbo].[ProductBatches]
(
	[ProductId] ASC,
	[StoreId] ASC,
	[ExpirationDate] ASC
)
INCLUDE([ProductUnitId],[QuantityOnHand],[BatchNumber]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductBatches_Unit_Expiry]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductBatches_Unit_Expiry] ON [dbo].[ProductBatches]
(
	[ProductUnitId] ASC,
	[ExpirationDate] ASC,
	[Id] ASC
)
INCLUDE([StoreId],[QuantityOnHand],[BatchNumber],[CostPrice]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ProductBatches_Store_Unit_Batch]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductBatches_Store_Unit_Batch] ON [dbo].[ProductBatches]
(
	[StoreId] ASC,
	[ProductUnitId] ASC,
	[BatchNumber] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductImages_Pharmacy_Product]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductImages_Pharmacy_Product] ON [dbo].[ProductImages]
(
	[PharmacyId] ASC,
	[ProductId] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductImages_Product_Sort]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductImages_Product_Sort] ON [dbo].[ProductImages]
(
	[ProductId] ASC,
	[IsPrimary] DESC,
	[SortOrder] ASC,
	[Id] ASC
)
INCLUDE([ImageUrl],[ThumbnailUrl],[AltText]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductImages_ProductId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages]
(
	[ProductId] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ProductImages_Product_Primary]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductImages_Product_Primary] ON [dbo].[ProductImages]
(
	[ProductId] ASC,
	[IsPrimary] ASC
)
WHERE ([IsPrimary]=(1) AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductInventory_Pharmacy_Store]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductInventory_Pharmacy_Store] ON [dbo].[ProductInventory]
(
	[PharmacyId] ASC,
	[StoreId] ASC
)
INCLUDE([ProductId],[ProductUnitId],[QuantityOnHand],[ReservedQty]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductInventory_Product]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductInventory_Product] ON [dbo].[ProductInventory]
(
	[ProductId] ASC
)
INCLUDE([StoreId],[ProductUnitId],[QuantityOnHand],[ReservedQty]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_Pharmacy_Category_Active]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Products_Pharmacy_Category_Active] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[CategoryId] ASC,
	[IsActive] ASC
)
INCLUDE([Name],[NameEn],[TrackInventory],[RequiresPrescription],[IsFeatured]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_Pharmacy_Name]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Products_Pharmacy_Name] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[Name] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_Pharmacy_NameEn]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Products_Pharmacy_NameEn] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[NameEn] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_Pharmacy_Search]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Products_Pharmacy_Search] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[NormalizedName] ASC,
	[NormalizedNameEn] ASC
)
INCLUDE([Name],[NameEn]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Products_Pharmacy_Barcode]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Products_Pharmacy_Barcode] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[Barcode] ASC
)
WHERE ([Barcode] IS NOT NULL AND [Barcode]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Products_Pharmacy_InternationalCode]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Products_Pharmacy_InternationalCode] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[InternationalCode] ASC
)
WHERE ([InternationalCode] IS NOT NULL AND [InternationalCode]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Products_Pharmacy_StockProductCode]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Products_Pharmacy_StockProductCode] ON [dbo].[Products]
(
	[PharmacyId] ASC,
	[StockProductCode] ASC
)
WHERE ([StockProductCode] IS NOT NULL AND [StockProductCode]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductTags_TagId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductTags_TagId] ON [dbo].[ProductTags]
(
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductUnits_Product_List]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductUnits_Product_List] ON [dbo].[ProductUnits]
(
	[ProductId] ASC,
	[IsActive] ASC,
	[IsPrimary] ASC,
	[SortOrder] ASC
)
INCLUDE([UnitId],[ListPrice],[CurrencyCode],[UnitsPerParent],[BaseUnitId],[BaseQuantity]) 
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ProductUnits_Pharmacy_Product_Unit]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductUnits_Pharmacy_Product_Unit] ON [dbo].[ProductUnits]
(
	[PharmacyId] ASC,
	[ProductId] ASC,
	[UnitId] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ProductUnits_Pharmacy_SKU]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductUnits_Pharmacy_SKU] ON [dbo].[ProductUnits]
(
	[PharmacyId] ASC,
	[SKU] ASC
)
WHERE ([SKU] IS NOT NULL AND [SKU]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ProductUnits_Pharmacy_UnitCode]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductUnits_Pharmacy_UnitCode] ON [dbo].[ProductUnits]
(
	[PharmacyId] ASC,
	[UnitCode] ASC
)
WHERE ([UnitCode] IS NOT NULL AND [UnitCode]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ProductUnits_Product_Primary]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProductUnits_Product_Primary] ON [dbo].[ProductUnits]
(
	[ProductId] ASC
)
WHERE ([IsPrimary]=(1) AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionConditions_PromotionId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionConditions_PromotionId] ON [dbo].[PromotionConditions]
(
	[PromotionId] ASC,
	[ConditionType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionCoupons_PromotionId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionCoupons_PromotionId] ON [dbo].[PromotionCoupons]
(
	[PromotionId] ASC,
	[IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_PromotionCoupons_Code]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_PromotionCoupons_Code] ON [dbo].[PromotionCoupons]
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionEffects_PromotionId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionEffects_PromotionId] ON [dbo].[PromotionEffects]
(
	[PromotionId] ASC,
	[EffectType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Promotions_Pharmacy_Active_Dates]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Promotions_Pharmacy_Active_Dates] ON [dbo].[Promotions]
(
	[PharmacyId] ASC,
	[IsActive] ASC,
	[StartAt] ASC,
	[EndAt] ASC
)
INCLUDE([Priority],[StackPolicy],[StackGroupKey],[AppliesToAllProducts]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionSchedules_PromotionId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionSchedules_PromotionId] ON [dbo].[PromotionSchedules]
(
	[PromotionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionTargets_CategoryId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionTargets_CategoryId] ON [dbo].[PromotionTargets]
(
	[CategoryId] ASC
)
INCLUDE([PromotionId],[IncludeSubcategories]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionTargets_ProductId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionTargets_ProductId] ON [dbo].[PromotionTargets]
(
	[ProductId] ASC
)
INCLUDE([PromotionId],[MinQty],[MaxQty]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionTargets_ProductUnitId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionTargets_ProductUnitId] ON [dbo].[PromotionTargets]
(
	[ProductUnitId] ASC
)
INCLUDE([PromotionId],[ProductIdForUnit],[MinQty],[MaxQty]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionTargets_PromotionId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionTargets_PromotionId] ON [dbo].[PromotionTargets]
(
	[PromotionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PromotionTargets_TagId]    Script Date: 24/01/2026 05:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_PromotionTargets_TagId] ON [dbo].[PromotionTargets]
(
	[TagId] ASC
)
INCLUDE([PromotionId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_Stores_OneDefaultPerPharmacy]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Stores_OneDefaultPerPharmacy] ON [dbo].[Stores]
(
	[PharmacyId] ASC
)
WHERE ([IsDefault]=(1) AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Stores_Pharmacy_Code]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Stores_Pharmacy_Code] ON [dbo].[Stores]
(
	[PharmacyId] ASC,
	[Code] ASC
)
WHERE ([Code] IS NOT NULL AND [Code]<>'' AND [DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Stores_Pharmacy_Name]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Stores_Pharmacy_Name] ON [dbo].[Stores]
(
	[PharmacyId] ASC,
	[Name] ASC
)
WHERE ([DeletedAt] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Tags_Pharmacy_Name]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Tags_Pharmacy_Name] ON [dbo].[Tags]
(
	[PharmacyId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Units_Code]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Units_Code] ON [dbo].[Units]
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Units_NameEn]    Script Date: 24/01/2026 05:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Units_NameEn] ON [dbo].[Units]
(
	[NameEn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Brands] ADD  CONSTRAINT [DF_Brands_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Brands] ADD  CONSTRAINT [DF_Brands_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_ImageVersion]  DEFAULT ((0)) FOR [ImageVersion]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT ('Regular') FOR [CustomerType]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT ((0)) FOR [Points]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[LoginAudits] ADD  CONSTRAINT [DF_LoginAudits_CreatedAtUtc]  DEFAULT (sysutcdatetime()) FOR [CreatedAtUtc]
GO
ALTER TABLE [dbo].[OrderItemPromotions] ADD  CONSTRAINT [DF_OrderItemPromotions_AppliedAt]  DEFAULT (sysutcdatetime()) FOR [AppliedAt]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_DiscountAmount]  DEFAULT ((0)) FOR [DiscountAmount]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_TaxAmount]  DEFAULT ((0)) FOR [TaxAmount]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_VatRate]  DEFAULT ((0)) FOR [VatRate]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[OrderPromotions] ADD  CONSTRAINT [DF_OrderPromotions_AppliedAt]  DEFAULT (sysutcdatetime()) FOR [AppliedAt]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Status]  DEFAULT ((2)) FOR [Status]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Channel]  DEFAULT ((1)) FOR [Channel]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Subtotal]  DEFAULT ((0)) FOR [Subtotal]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_DiscountTotal]  DEFAULT ((0)) FOR [DiscountTotal]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_TaxTotal]  DEFAULT ((0)) FOR [TaxTotal]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ShippingFee]  DEFAULT ((0)) FOR [ShippingFee]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_GrandTotal]  DEFAULT ((0)) FOR [GrandTotal]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_PointsEarned]  DEFAULT ((0)) FOR [PointsEarned]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_PointsRedeemed]  DEFAULT ((0)) FOR [PointsRedeemed]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Pharmacies] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Pharmacies] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_EarnEnabled]  DEFAULT ((1)) FOR [EarnEnabled]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_EarnPerAmount]  DEFAULT ((10.00)) FOR [EarnPerAmount]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_EarnPoints]  DEFAULT ((1)) FOR [EarnPoints]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_RedeemEnabled]  DEFAULT ((1)) FOR [RedeemEnabled]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_PointValue]  DEFAULT ((0.10)) FOR [PointValueEGP]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_MaxRedeem]  DEFAULT ((50.00)) FOR [MaxRedeemPercent]
GO
ALTER TABLE [dbo].[PointSettings] ADD  CONSTRAINT [DF_PointSettings_UpdatedAt]  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[PointsTransactions] ADD  CONSTRAINT [DF_PointsTransactions_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProductBatches] ADD  CONSTRAINT [DF_ProductBatches_ReceivedAt]  DEFAULT (sysutcdatetime()) FOR [ReceivedAt]
GO
ALTER TABLE [dbo].[ProductBatches] ADD  CONSTRAINT [DF_ProductBatches_QtyReceived]  DEFAULT ((0)) FOR [QuantityReceived]
GO
ALTER TABLE [dbo].[ProductBatches] ADD  CONSTRAINT [DF_ProductBatches_QtyOnHand]  DEFAULT ((0)) FOR [QuantityOnHand]
GO
ALTER TABLE [dbo].[ProductBatches] ADD  CONSTRAINT [DF_ProductBatches_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductBatches] ADD  CONSTRAINT [DF_ProductBatches_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProductImages] ADD  CONSTRAINT [DF_ProductImages_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[ProductImages] ADD  CONSTRAINT [DF_ProductImages_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[ProductImages] ADD  CONSTRAINT [DF_ProductImages_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  CONSTRAINT [DF_ProductInventory_QtyOnHand]  DEFAULT ((0)) FOR [QuantityOnHand]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  CONSTRAINT [DF_ProductInventory_Reserved]  DEFAULT ((0)) FOR [ReservedQty]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_RequiresPrescription]  DEFAULT ((0)) FOR [RequiresPrescription]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_EarnPoints]  DEFAULT ((1)) FOR [EarnPoints]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_HasExpiry]  DEFAULT ((1)) FOR [HasExpiry]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_AgeRestricted]  DEFAULT ((0)) FOR [AgeRestricted]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_RequiresColdChain]  DEFAULT ((0)) FOR [RequiresColdChain]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ControlledSubstance]  DEFAULT ((0)) FOR [ControlledSubstance]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsTaxable]  DEFAULT ((1)) FOR [IsTaxable]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_VatRate]  DEFAULT ((0.00)) FOR [VatRate]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_MinOrderQty]  DEFAULT ((1)) FOR [MinOrderQty]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsReturnable]  DEFAULT ((1)) FOR [IsReturnable]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_AllowSplitSale]  DEFAULT ((0)) FOR [AllowSplitSale]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_TrackInventory]  DEFAULT ((1)) FOR [TrackInventory]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsFeatured]  DEFAULT ((0)) FOR [IsFeatured]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsIntegrated]  DEFAULT ((0)) FOR [IsIntegrated]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProductTags] ADD  CONSTRAINT [DF_ProductTags_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_CurrencyCode]  DEFAULT ('EGP') FOR [CurrencyCode]
GO
ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] ADD  CONSTRAINT [DF_BXGY_RewardType]  DEFAULT ((1)) FOR [RewardType]
GO
ALTER TABLE [dbo].[PromotionConditions] ADD  CONSTRAINT [DF_PromotionConditions_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PromotionCoupons] ADD  CONSTRAINT [DF_PromotionCoupons_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PromotionCoupons] ADD  CONSTRAINT [DF_PromotionCoupons_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PromotionEffects] ADD  CONSTRAINT [DF_PromotionEffects_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_Priority]  DEFAULT ((0)) FOR [Priority]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_StackPolicy]  DEFAULT ((0)) FOR [StackPolicy]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_AllProducts]  DEFAULT ((0)) FOR [AppliesToAllProducts]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PromotionSchedules] ADD  CONSTRAINT [DF_PromotionSchedules_DaysMask]  DEFAULT ((127)) FOR [DaysMask]
GO
ALTER TABLE [dbo].[PromotionTargets] ADD  CONSTRAINT [DF_PromotionTargets_IncludeSub]  DEFAULT ((1)) FOR [IncludeSubcategories]
GO
ALTER TABLE [dbo].[PromotionTargets] ADD  CONSTRAINT [DF_PromotionTargets_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Stores] ADD  CONSTRAINT [DF_Stores_IsDefault]  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[Stores] ADD  CONSTRAINT [DF_Stores_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Stores] ADD  CONSTRAINT [DF_Stores_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Tags] ADD  CONSTRAINT [DF_Tags_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Tags] ADD  CONSTRAINT [DF_Tags_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Units] ADD  CONSTRAINT [DF_Units_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Units] ADD  CONSTRAINT [DF_Units_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Brands]  WITH CHECK ADD  CONSTRAINT [FK_Brands_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Brands] CHECK CONSTRAINT [FK_Brands_Pharmacies]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [FK_Categories_ParentCategory] FOREIGN KEY([ParentCategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_ParentCategory]
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Pharmacies]
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Users]
GO
ALTER TABLE [dbo].[OrderItemPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderItemPromotions_Effects] FOREIGN KEY([EffectId])
REFERENCES [dbo].[PromotionEffects] ([Id])
GO
ALTER TABLE [dbo].[OrderItemPromotions] CHECK CONSTRAINT [FK_OrderItemPromotions_Effects]
GO
ALTER TABLE [dbo].[OrderItemPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderItemPromotions_OrderItems] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[OrderItems] ([Id])
GO
ALTER TABLE [dbo].[OrderItemPromotions] CHECK CONSTRAINT [FK_OrderItemPromotions_OrderItems]
GO
ALTER TABLE [dbo].[OrderItemPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderItemPromotions_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[OrderItemPromotions] CHECK CONSTRAINT [FK_OrderItemPromotions_Promotions]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Pharmacies]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Products]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_ProductUnits_Composite] FOREIGN KEY([ProductUnitId], [ProductId])
REFERENCES [dbo].[ProductUnits] ([Id], [ProductId])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_ProductUnits_Composite]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Stores] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Stores]
GO
ALTER TABLE [dbo].[OrderPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderPromotions_Coupons] FOREIGN KEY([CouponId])
REFERENCES [dbo].[PromotionCoupons] ([Id])
GO
ALTER TABLE [dbo].[OrderPromotions] CHECK CONSTRAINT [FK_OrderPromotions_Coupons]
GO
ALTER TABLE [dbo].[OrderPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderPromotions_Effects] FOREIGN KEY([EffectId])
REFERENCES [dbo].[PromotionEffects] ([Id])
GO
ALTER TABLE [dbo].[OrderPromotions] CHECK CONSTRAINT [FK_OrderPromotions_Effects]
GO
ALTER TABLE [dbo].[OrderPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderPromotions_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO
ALTER TABLE [dbo].[OrderPromotions] CHECK CONSTRAINT [FK_OrderPromotions_Orders]
GO
ALTER TABLE [dbo].[OrderPromotions]  WITH CHECK ADD  CONSTRAINT [FK_OrderPromotions_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[OrderPromotions] CHECK CONSTRAINT [FK_OrderPromotions_Promotions]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Customers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Customers]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Pharmacies]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Stores] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Stores]
GO
ALTER TABLE [dbo].[Pharmacists]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[PointSettings]  WITH CHECK ADD  CONSTRAINT [FK_PointSettings_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[PointSettings] CHECK CONSTRAINT [FK_PointSettings_Pharmacies]
GO
ALTER TABLE [dbo].[PointsTransactions]  WITH CHECK ADD  CONSTRAINT [FK_PointsTransactions_Customers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO
ALTER TABLE [dbo].[PointsTransactions] CHECK CONSTRAINT [FK_PointsTransactions_Customers]
GO
ALTER TABLE [dbo].[PointsTransactions]  WITH CHECK ADD  CONSTRAINT [FK_PointsTransactions_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[PointsTransactions] CHECK CONSTRAINT [FK_PointsTransactions_Pharmacies]
GO
ALTER TABLE [dbo].[ProductBatches]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatches_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[ProductBatches] CHECK CONSTRAINT [FK_ProductBatches_Pharmacies]
GO
ALTER TABLE [dbo].[ProductBatches]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatches_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductBatches] CHECK CONSTRAINT [FK_ProductBatches_Products]
GO
ALTER TABLE [dbo].[ProductBatches]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatches_ProductUnits_Composite] FOREIGN KEY([ProductUnitId], [ProductId])
REFERENCES [dbo].[ProductUnits] ([Id], [ProductId])
GO
ALTER TABLE [dbo].[ProductBatches] CHECK CONSTRAINT [FK_ProductBatches_ProductUnits_Composite]
GO
ALTER TABLE [dbo].[ProductBatches]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatches_Stores] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
GO
ALTER TABLE [dbo].[ProductBatches] CHECK CONSTRAINT [FK_ProductBatches_Stores]
GO
ALTER TABLE [dbo].[ProductImages]  WITH CHECK ADD  CONSTRAINT [FK_ProductImages_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[ProductImages] CHECK CONSTRAINT [FK_ProductImages_Pharmacies]
GO
ALTER TABLE [dbo].[ProductImages]  WITH CHECK ADD  CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductImages] CHECK CONSTRAINT [FK_ProductImages_Products]
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [FK_ProductInventory_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [FK_ProductInventory_Pharmacies]
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [FK_ProductInventory_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [FK_ProductInventory_Products]
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [FK_ProductInventory_ProductUnits_Composite] FOREIGN KEY([ProductUnitId], [ProductId])
REFERENCES [dbo].[ProductUnits] ([Id], [ProductId])
GO
ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [FK_ProductInventory_ProductUnits_Composite]
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [FK_ProductInventory_Stores] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
GO
ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [FK_ProductInventory_Stores]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Pharmacies]
GO
ALTER TABLE [dbo].[ProductTags]  WITH CHECK ADD  CONSTRAINT [FK_ProductTags_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductTags] CHECK CONSTRAINT [FK_ProductTags_Products]
GO
ALTER TABLE [dbo].[ProductTags]  WITH CHECK ADD  CONSTRAINT [FK_ProductTags_Tags] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tags] ([Id])
GO
ALTER TABLE [dbo].[ProductTags] CHECK CONSTRAINT [FK_ProductTags_Tags]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnits_BaseUnit] FOREIGN KEY([BaseUnitId])
REFERENCES [dbo].[Units] ([Id])
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnits_BaseUnit]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnits_ParentProductUnit] FOREIGN KEY([ParentProductUnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnits_ParentProductUnit]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnits_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnits_Pharmacies]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnits_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnits_Products]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnits_Unit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[Units] ([Id])
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnits_Unit]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_BuyCategory] FOREIGN KEY([BuyCategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_BuyCategory]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_BuyProduct] FOREIGN KEY([BuyProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_BuyProduct]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_BuyProductUnit] FOREIGN KEY([BuyProductUnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_BuyProductUnit]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_BuyTag] FOREIGN KEY([BuyTagId])
REFERENCES [dbo].[Tags] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_BuyTag]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_Effect] FOREIGN KEY([EffectId])
REFERENCES [dbo].[PromotionEffects] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_Effect]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_GetCategory] FOREIGN KEY([GetCategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_GetCategory]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_GetProduct] FOREIGN KEY([GetProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_GetProduct]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_GetProductUnit] FOREIGN KEY([GetProductUnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_GetProductUnit]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [FK_BXGY_GetTag] FOREIGN KEY([GetTagId])
REFERENCES [dbo].[Tags] ([Id])
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [FK_BXGY_GetTag]
GO
ALTER TABLE [dbo].[PromotionConditions]  WITH CHECK ADD  CONSTRAINT [FK_PromotionConditions_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionConditions] CHECK CONSTRAINT [FK_PromotionConditions_Promotions]
GO
ALTER TABLE [dbo].[PromotionCoupons]  WITH CHECK ADD  CONSTRAINT [FK_PromotionCoupons_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionCoupons] CHECK CONSTRAINT [FK_PromotionCoupons_Promotions]
GO
ALTER TABLE [dbo].[PromotionEffects]  WITH CHECK ADD  CONSTRAINT [FK_PromotionEffects_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionEffects] CHECK CONSTRAINT [FK_PromotionEffects_Promotions]
GO
ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [FK_Promotions_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [FK_Promotions_Pharmacies]
GO
ALTER TABLE [dbo].[PromotionSchedules]  WITH CHECK ADD  CONSTRAINT [FK_PromotionSchedules_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionSchedules] CHECK CONSTRAINT [FK_PromotionSchedules_Promotions]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_Categories]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_Products]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_ProductUnits] FOREIGN KEY([ProductUnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_ProductUnits]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_ProductUnits_Composite] FOREIGN KEY([ProductUnitId], [ProductIdForUnit])
REFERENCES [dbo].[ProductUnits] ([Id], [ProductId])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_ProductUnits_Composite]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_Promotions]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [FK_PromotionTargets_Tags] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tags] ([Id])
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [FK_PromotionTargets_Tags]
GO
ALTER TABLE [dbo].[PromotionUsageLimits]  WITH CHECK ADD  CONSTRAINT [FK_PromotionUsageLimits_Promotions] FOREIGN KEY([PromotionId])
REFERENCES [dbo].[Promotions] ([Id])
GO
ALTER TABLE [dbo].[PromotionUsageLimits] CHECK CONSTRAINT [FK_PromotionUsageLimits_Promotions]
GO
ALTER TABLE [dbo].[Stores]  WITH CHECK ADD  CONSTRAINT [FK_Stores_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Stores] CHECK CONSTRAINT [FK_Stores_Pharmacies]
GO
ALTER TABLE [dbo].[Tags]  WITH CHECK ADD  CONSTRAINT [FK_Tags_Pharmacies] FOREIGN KEY([PharmacyId])
REFERENCES [dbo].[Pharmacies] ([Id])
GO
ALTER TABLE [dbo].[Tags] CHECK CONSTRAINT [FK_Tags_Pharmacies]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [CK_Categories_ImageFormat] CHECK  (([ImageFormat] IS NULL OR ([ImageFormat]=(3) OR [ImageFormat]=(2) OR [ImageFormat]=(1))))
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [CK_Categories_ImageFormat]
GO
ALTER TABLE [dbo].[OrderItemPromotions]  WITH CHECK ADD  CONSTRAINT [CK_OrderItemPromotions_Discount] CHECK  (([DiscountAmount]>=(0)))
GO
ALTER TABLE [dbo].[OrderItemPromotions] CHECK CONSTRAINT [CK_OrderItemPromotions_Discount]
GO
ALTER TABLE [dbo].[OrderItemPromotions]  WITH CHECK ADD  CONSTRAINT [CK_OrderItemPromotions_Qty] CHECK  (([AppliedQty] IS NULL OR [AppliedQty]>(0)))
GO
ALTER TABLE [dbo].[OrderItemPromotions] CHECK CONSTRAINT [CK_OrderItemPromotions_Qty]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [CK_OrderItems_Prices] CHECK  (([ListUnitPrice]>=(0) AND [OriginalLineTotal]>=(0) AND [DiscountAmount]>=(0) AND [TaxAmount]>=(0) AND [FinalLineTotal]>=(0)))
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [CK_OrderItems_Prices]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [CK_OrderItems_Qty] CHECK  (([Quantity]>(0)))
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [CK_OrderItems_Qty]
GO
ALTER TABLE [dbo].[OrderPromotions]  WITH CHECK ADD  CONSTRAINT [CK_OrderPromotions_Discount] CHECK  (([DiscountAmount]>=(0)))
GO
ALTER TABLE [dbo].[OrderPromotions] CHECK CONSTRAINT [CK_OrderPromotions_Discount]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_Channel] CHECK  (([Channel]=(3) OR [Channel]=(2) OR [Channel]=(1)))
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_Channel]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_Status] CHECK  (([Status]=(5) OR [Status]=(4) OR [Status]=(3) OR [Status]=(2) OR [Status]=(1)))
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_Status]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_Totals] CHECK  (([Subtotal]>=(0) AND [DiscountTotal]>=(0) AND [TaxTotal]>=(0) AND [ShippingFee]>=(0) AND [GrandTotal]>=(0)))
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_Totals]
GO
ALTER TABLE [dbo].[PointSettings]  WITH CHECK ADD  CONSTRAINT [CK_PointSettings_Rules] CHECK  (([EarnPerAmount]>(0) AND [EarnPoints]>(0) AND [PointValueEGP]>=(0) AND [MaxRedeemPercent]>=(0) AND [MaxRedeemPercent]<=(100) AND ([PointsExpireDays] IS NULL OR [PointsExpireDays]>(0))))
GO
ALTER TABLE [dbo].[PointSettings] CHECK CONSTRAINT [CK_PointSettings_Rules]
GO
ALTER TABLE [dbo].[PointsTransactions]  WITH CHECK ADD  CONSTRAINT [CK_PointsTransactions_Type] CHECK  (([TxType]=(4) OR [TxType]=(3) OR [TxType]=(2) OR [TxType]=(1)))
GO
ALTER TABLE [dbo].[PointsTransactions] CHECK CONSTRAINT [CK_PointsTransactions_Type]
GO
ALTER TABLE [dbo].[ProductBatches]  WITH CHECK ADD  CONSTRAINT [CK_ProductBatches_Qty] CHECK  (([QuantityReceived]>=(0) AND [QuantityOnHand]>=(0) AND [QuantityOnHand]<=[QuantityReceived] AND ([CostPrice] IS NULL OR [CostPrice]>=(0))))
GO
ALTER TABLE [dbo].[ProductBatches] CHECK CONSTRAINT [CK_ProductBatches_Qty]
GO
ALTER TABLE [dbo].[ProductImages]  WITH CHECK ADD  CONSTRAINT [CK_ProductImages_SortOrder] CHECK  (([SortOrder]>=(0)))
GO
ALTER TABLE [dbo].[ProductImages] CHECK CONSTRAINT [CK_ProductImages_SortOrder]
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [CK_ProductInventory_Qty] CHECK  (([QuantityOnHand]>=(0) AND [ReservedQty]>=(0) AND [ReservedQty]<=[QuantityOnHand] AND ([MinStockLevel] IS NULL OR [MinStockLevel]>=(0)) AND ([MaxStockLevel] IS NULL OR [MaxStockLevel]>=(0))))
GO
ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [CK_ProductInventory_Qty]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_Age] CHECK  (([AgeRestricted]=(0) AND [MinAge] IS NULL OR [AgeRestricted]=(1) AND [MinAge] IS NOT NULL AND [MinAge]>=(0)))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_Age]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_Dimensions] CHECK  ((([WeightGrams] IS NULL OR [WeightGrams]>=(0)) AND ([LengthMm] IS NULL OR [LengthMm]>=(0)) AND ([WidthMm] IS NULL OR [WidthMm]>=(0)) AND ([HeightMm] IS NULL OR [HeightMm]>=(0))))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_Dimensions]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_OrderLimits] CHECK  (([MinOrderQty]>(0) AND ([MaxOrderQty] IS NULL OR [MaxOrderQty]>=[MinOrderQty]) AND ([MaxPerDayQty] IS NULL OR [MaxPerDayQty]>(0))))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_OrderLimits]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_ReturnWindow] CHECK  (([IsReturnable]=(0) AND [ReturnWindowDays] IS NULL OR [IsReturnable]=(1) AND ([ReturnWindowDays] IS NULL OR [ReturnWindowDays]>(0))))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_ReturnWindow]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_SplitRules] CHECK  (([AllowSplitSale]=(0) AND [SplitLevel] IS NULL OR [AllowSplitSale]=(1) AND ([SplitLevel]=(2) OR [SplitLevel]=(1))))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_SplitRules]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_Tax] CHECK  (([VatRate]>=(0) AND [VatRate]<=(100)))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_Tax]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [CK_ProductUnits_BaseQuantity] CHECK  (([BaseQuantity] IS NULL OR [BaseQuantity]>(0)))
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [CK_ProductUnits_BaseQuantity]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [CK_ProductUnits_Prices] CHECK  (([ListPrice]>=(0) AND ([CostPrice] IS NULL OR [CostPrice]>=(0))))
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [CK_ProductUnits_Prices]
GO
ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [CK_ProductUnits_UnitsPerParent] CHECK  (([UnitsPerParent] IS NULL OR [UnitsPerParent]>(0)))
GO
ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [CK_ProductUnits_UnitsPerParent]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [CK_BXGY_Qty] CHECK  (([BuyQty]>(0) AND [GetQty]>(0)))
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [CK_BXGY_Qty]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [CK_BXGY_RewardType] CHECK  (([RewardType]=(3) OR [RewardType]=(2) OR [RewardType]=(1)))
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [CK_BXGY_RewardType]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [CK_BXGY_RewardValues] CHECK  (([RewardType]=(1) AND [RewardPercent] IS NULL AND [RewardAmount] IS NULL OR [RewardType]=(2) AND [RewardPercent] IS NOT NULL AND [RewardPercent]>=(0) OR [RewardType]=(3) AND [RewardAmount] IS NOT NULL AND [RewardAmount]>=(0)))
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [CK_BXGY_RewardValues]
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules]  WITH CHECK ADD  CONSTRAINT [CK_BXGY_Targets] CHECK  ((([BuyProductUnitId] IS NOT NULL OR [BuyProductId] IS NOT NULL OR [BuyCategoryId] IS NOT NULL OR [BuyTagId] IS NOT NULL) AND ([GetProductUnitId] IS NOT NULL OR [GetProductId] IS NOT NULL OR [GetCategoryId] IS NOT NULL OR [GetTagId] IS NOT NULL)))
GO
ALTER TABLE [dbo].[PromotionBuyXGetYRules] CHECK CONSTRAINT [CK_BXGY_Targets]
GO
ALTER TABLE [dbo].[PromotionConditions]  WITH CHECK ADD  CONSTRAINT [CK_PromotionConditions_Type] CHECK  (([ConditionType]=(7) OR [ConditionType]=(6) OR [ConditionType]=(5) OR [ConditionType]=(4) OR [ConditionType]=(3) OR [ConditionType]=(2) OR [ConditionType]=(1)))
GO
ALTER TABLE [dbo].[PromotionConditions] CHECK CONSTRAINT [CK_PromotionConditions_Type]
GO
ALTER TABLE [dbo].[PromotionCoupons]  WITH CHECK ADD  CONSTRAINT [CK_PromotionCoupons_Uses] CHECK  ((([MaxUsesTotal] IS NULL OR [MaxUsesTotal]>(0)) AND ([MaxUsesPerCustomer] IS NULL OR [MaxUsesPerCustomer]>(0))))
GO
ALTER TABLE [dbo].[PromotionCoupons] CHECK CONSTRAINT [CK_PromotionCoupons_Uses]
GO
ALTER TABLE [dbo].[PromotionEffects]  WITH CHECK ADD  CONSTRAINT [CK_PromotionEffects_Type] CHECK  (([EffectType]=(6) OR [EffectType]=(5) OR [EffectType]=(4) OR [EffectType]=(3) OR [EffectType]=(2) OR [EffectType]=(1)))
GO
ALTER TABLE [dbo].[PromotionEffects] CHECK CONSTRAINT [CK_PromotionEffects_Type]
GO
ALTER TABLE [dbo].[PromotionEffects]  WITH CHECK ADD  CONSTRAINT [CK_PromotionEffects_Values] CHECK  (([EffectType]=(1) AND [PercentValue] IS NOT NULL AND [PercentValue]>=(0) OR [EffectType]=(2) AND [AmountValue] IS NOT NULL AND [AmountValue]>=(0) OR [EffectType]=(3) OR [EffectType]=(4) OR [EffectType]=(5) AND [PointsValue] IS NOT NULL AND [PointsValue]>=(0) OR [EffectType]=(6) AND [Multiplier] IS NOT NULL AND [Multiplier]>=(0)))
GO
ALTER TABLE [dbo].[PromotionEffects] CHECK CONSTRAINT [CK_PromotionEffects_Values]
GO
ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_Dates] CHECK  (([EndAt]>[StartAt]))
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_Dates]
GO
ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_StackPolicy] CHECK  (([StackPolicy]=(3) OR [StackPolicy]=(2) OR [StackPolicy]=(1) OR [StackPolicy]=(0)))
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_StackPolicy]
GO
ALTER TABLE [dbo].[PromotionSchedules]  WITH CHECK ADD  CONSTRAINT [CK_PromotionSchedules_DaysMask] CHECK  (([DaysMask]>=(1) AND [DaysMask]<=(127)))
GO
ALTER TABLE [dbo].[PromotionSchedules] CHECK CONSTRAINT [CK_PromotionSchedules_DaysMask]
GO
ALTER TABLE [dbo].[PromotionSchedules]  WITH CHECK ADD  CONSTRAINT [CK_PromotionSchedules_TimeWindow] CHECK  (([StartTime] IS NULL AND [EndTime] IS NULL OR [StartTime] IS NOT NULL AND [EndTime] IS NOT NULL AND [EndTime]>[StartTime]))
GO
ALTER TABLE [dbo].[PromotionSchedules] CHECK CONSTRAINT [CK_PromotionSchedules_TimeWindow]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [CK_PromotionTargets_ExactlyOneTarget] CHECK  (((((case when [ProductId] IS NULL then (0) else (1) end+case when [ProductUnitId] IS NULL then (0) else (1) end)+case when [CategoryId] IS NULL then (0) else (1) end)+case when [TagId] IS NULL then (0) else (1) end)=(1)))
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [CK_PromotionTargets_ExactlyOneTarget]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [CK_PromotionTargets_Qty] CHECK  ((([MinQty] IS NULL OR [MinQty]>(0)) AND ([MaxQty] IS NULL OR [MaxQty]>(0)) AND ([MinQty] IS NULL OR [MaxQty] IS NULL OR [MaxQty]>=[MinQty])))
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [CK_PromotionTargets_Qty]
GO
ALTER TABLE [dbo].[PromotionTargets]  WITH CHECK ADD  CONSTRAINT [CK_PromotionTargets_UnitProductPair] CHECK  (([ProductUnitId] IS NULL AND [ProductIdForUnit] IS NULL OR [ProductUnitId] IS NOT NULL AND [ProductIdForUnit] IS NOT NULL))
GO
ALTER TABLE [dbo].[PromotionTargets] CHECK CONSTRAINT [CK_PromotionTargets_UnitProductPair]
GO
ALTER TABLE [dbo].[PromotionUsageLimits]  WITH CHECK ADD  CONSTRAINT [CK_PromotionUsageLimits] CHECK  ((([MaxRedemptionsTotal] IS NULL OR [MaxRedemptionsTotal]>(0)) AND ([MaxRedemptionsPerCustomer] IS NULL OR [MaxRedemptionsPerCustomer]>(0)) AND ([MaxRedemptionsPerOrder] IS NULL OR [MaxRedemptionsPerOrder]>(0))))
GO
ALTER TABLE [dbo].[PromotionUsageLimits] CHECK CONSTRAINT [CK_PromotionUsageLimits]
GO
USE [master]
GO
ALTER DATABASE [pharma_shope_db_temp] SET  READ_WRITE 
GO
