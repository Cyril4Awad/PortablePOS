CREATE TABLE [dbo].[STK04_PreparedOffers]
(
[PROF_Location] [varchar] (2) NOT NULL,
[PROF_HeaderRecID] [numeric] (18, 0) NOT NULL,
[PROF_Machine] [nvarchar] (100) NOT NULL,
[PROF_UserStamp] [varchar] (4) NOT NULL,
[PROF_DateStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_PreparedOffers] ADD CONSTRAINT [PK_STK04_PreparedOffers] PRIMARY KEY CLUSTERED ([PROF_Location], [PROF_HeaderRecID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_PreparedOffers] ADD CONSTRAINT [FK_STK04_PreparedOffers_STK04_OfferHeader] FOREIGN KEY ([PROF_Location], [PROF_HeaderRecID]) REFERENCES [dbo].[STK04_OfferHeader] ([HOFR_LOC], [HOFR_HeadRecId])
GO
