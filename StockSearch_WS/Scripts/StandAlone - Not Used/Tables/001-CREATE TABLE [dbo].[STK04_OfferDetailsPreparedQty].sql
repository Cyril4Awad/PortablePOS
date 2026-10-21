CREATE TABLE [dbo].[STK04_OfferDetailsPreparedQty]
(
[ODPQ_RecID] [numeric] (18, 0) NOT NULL,
[ODPQ_Location] [varchar] (2) NOT NULL,
[ODPQ_HeaderRecID] [numeric] (18, 0) NOT NULL,
[ODPQ_Counter] [numeric] (5, 0) NOT NULL,
[ODPQ_ItemRecID] [numeric] (18, 0) NOT NULL,
[ODPQ_ExpiryDate] [varchar] (10) NULL,
[ODPQ_LotNumber] [varchar] (15) NULL,
[ODPQ_Qty] [numeric] (18, 3) NOT NULL,
[ODPQ_Machine] [nvarchar] (100) NOT NULL,
[ODPQ_UserStamp] [varchar] (4) NOT NULL,
[ODPQ_DateStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_OfferDetailsPreparedQty] ADD CONSTRAINT [PK_STK04_OfferDetailsPreparedQty] PRIMARY KEY CLUSTERED ([ODPQ_RecID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_OfferDetailsPreparedQty] ADD CONSTRAINT [FK_STK04_OfferDetailsPreparedQty_STK04_OfferDetails] FOREIGN KEY ([ODPQ_Counter], [ODPQ_HeaderRecID], [ODPQ_Location]) REFERENCES [dbo].[STK04_OfferDetails] ([DOFR_Counter], [DOFR_HeadRecId], [DOFR_LOC])
GO
ALTER TABLE [dbo].[STK04_OfferDetailsPreparedQty] ADD CONSTRAINT [FK_STK04_OfferDetailsPreparedQty_STK04_STKFIL] FOREIGN KEY ([ODPQ_ItemRecID]) REFERENCES [dbo].[STK04_STKFIL] ([STK_RECID])
GO
