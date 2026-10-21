CREATE TABLE [dbo].[tbl_CPP_OrderDetails]
(
[ORDD_RecID] [numeric] (18, 0) NOT NULL,
[ORDD_LocCode] [varchar] (2) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[ORDD_Number] [numeric] (18, 0) NOT NULL,
[ORDD_ItemRecID] [numeric] (18, 0) NOT NULL,
[ORDD_ItemQty] [numeric] (18, 3) NOT NULL,
[ORDD_ItemPrice] [numeric] (18, 3) NOT NULL,
[ORDD_ItemDiscount] [numeric] (5, 2) NOT NULL,
[ORDD_ItemTotal] [numeric] (18, 3) NOT NULL,
[ORDD_ItemVatValue] [numeric] (18, 3) NOT NULL,
[ORDD_ItemRemark] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_OrderDetails] ADD CONSTRAINT [PK_tbl_CPP_OrderDetails] PRIMARY KEY CLUSTERED ([ORDD_RecID], [ORDD_LocCode], [ORDD_Number]) ON [PRIMARY]
GO
