CREATE TABLE [dbo].[STK04_ClientsVisits]
(
[CLV_RecID] [int] IDENTITY (1, 1) NOT NULL,
[CLV_CLRecID] [numeric] (18, 0) NOT NULL,
[CLV_UserCode] [varchar] (4) NOT NULL,
[CLV_DateTime] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_ClientsVisits] ADD CONSTRAINT [PK_STK04_ClientsVisits] PRIMARY KEY CLUSTERED ([CLV_RecID]) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_STK04_ClientsVisits_User_Date] ON [dbo].[STK04_ClientsVisits] ([CLV_UserCode], [CLV_DateTime]) INCLUDE ([CLV_CLRecID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_ClientsVisits] ADD CONSTRAINT [FK_STK04_ClientsVisits_Stk04_Clients] FOREIGN KEY ([CLV_CLRecID]) REFERENCES [dbo].[Stk04_Clients] ([CL_RecID])
GO
