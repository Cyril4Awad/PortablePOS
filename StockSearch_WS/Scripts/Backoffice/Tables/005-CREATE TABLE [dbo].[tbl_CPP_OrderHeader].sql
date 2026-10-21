CREATE TABLE [dbo].[tbl_CPP_OrderHeader]
(
[ORDH_LocCode] [varchar] (2) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[ORDH_Number] [numeric] (18, 0) NOT NULL,
[ORDH_Date] [date] NOT NULL,
[ORDH_ClientRecID] [numeric] (18, 0) NOT NULL,
[ORDH_VatType] [varchar] (1) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[ORDH_HeaderRemark] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[ORDH_FooterRemark] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[ORDH_Currency] [varchar] (3) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[ORDH_Total] [numeric] (18, 3) NOT NULL,
[ORDH_TotalVAT] [numeric] (18, 3) NOT NULL,
[ORDH_OfferRecID] [numeric] (18, 0) NULL,
[ORDH_Discrepancy] [bit] NOT NULL CONSTRAINT [DF_tbl_CPP_OrderHeader_ORDH_Discrepancy] DEFAULT ((0)),
[ORDH_UserStamp] [varchar] (4) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[ORDH_DateStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_OrderHeader] ADD CONSTRAINT [PK_tbl_CPP_OrderHeader] PRIMARY KEY CLUSTERED ([ORDH_LocCode], [ORDH_Number]) ON [PRIMARY]
GO
