/****** Object:  Table [dbo].[_TRANSFER_tbl_upload_details]    Script Date: 22/10/2026 11:28:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The transfers saved by the portable (Save Transfer), one row per item lot and destination, uploaded later to the ERP
-- _id : one id per transfer saved (device save id + destination), Uploaded / UploadError : N until the ERP takes the row
IF OBJECT_ID('dbo._TRANSFER_tbl_upload_details') IS NULL
CREATE TABLE [dbo].[_TRANSFER_tbl_upload_details](
	[RecID] [int] IDENTITY(1,1) NOT NULL,
	[_id] [nvarchar](50) NULL,
	[RowNumber] [nvarchar](50) NULL,
	[STK_RECID] [nvarchar](50) NULL,
	[STK_REF] [nvarchar](50) NULL,
	[STK_DESC] [nvarchar](250) NULL,
	[STK_SingleRef1] [nvarchar](50) NULL,
	[STK_SingleRef2] [nvarchar](50) NULL,
	[dbar_barcode] [nvarchar](50) NULL,
	[br_Name] [nvarchar](50) NULL,
	[IT_DESC] [nvarchar](50) NULL,
	[Size] [nvarchar](50) NULL,
	[Color] [nvarchar](50) NULL,
	[Expiry_Dt] [nvarchar](50) NULL,
	[Lot_Nbr] [nvarchar](50) NULL,
	[Available_Qty] [nvarchar](50) NULL,
	[TransferQty] [nvarchar](50) NULL,
	[STK_ItmClass] [nvarchar](50) NULL,
	[Desc2] [nvarchar](50) NULL,
	[Desc2Header] [nvarchar](50) NULL,
	[DisplayPrice] [nvarchar](50) NULL,
	[PriceCurr] [nvarchar](50) NULL,
	[QRCode] [nchar](100) NULL,
	[FromLoc] [nvarchar](50) NULL,
	[ToLoc] [nvarchar](50) NULL,
	[Machine] [nvarchar](50) NULL,
	[ParseError] [char](1) NULL,
	[Uploaded] [char](1) NULL,
	[UploadError] [char](1) NULL,
	[dtstmp] [datetime] NULL,
 CONSTRAINT [PK__TRANSFER_tbl_upload_details_details] PRIMARY KEY CLUSTERED 
(
	[RecID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

IF OBJECT_ID('dbo.DF__TRANSFER_tbl_upload_details_Uploaded') IS NULL
ALTER TABLE [dbo].[_TRANSFER_tbl_upload_details] ADD  CONSTRAINT [DF__TRANSFER_tbl_upload_details_Uploaded]  DEFAULT ('N') FOR [Uploaded]
GO

IF OBJECT_ID('dbo.DF__TRANSFER_tbl_upload_details_UploadError') IS NULL
ALTER TABLE [dbo].[_TRANSFER_tbl_upload_details] ADD  CONSTRAINT [DF__TRANSFER_tbl_upload_details_UploadError]  DEFAULT ('N') FOR [UploadError]
GO

IF OBJECT_ID('dbo.DF__TRANSFER_tbl_upload_details_dtstmp') IS NULL
ALTER TABLE [dbo].[_TRANSFER_tbl_upload_details] ADD  CONSTRAINT [DF__TRANSFER_tbl_upload_details_dtstmp]  DEFAULT (getdate()) FOR [dtstmp]
GO


