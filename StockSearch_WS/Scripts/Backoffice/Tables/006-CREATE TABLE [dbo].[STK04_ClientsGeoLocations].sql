CREATE TABLE [dbo].[STK04_ClientsGeoLocations]
(
[CGL_CLRecID] [numeric] (18, 0) NOT NULL,
[CGL_Coordinates] [sys].[geography] NOT NULL,
[CGL_Accuracy] [numeric] (18, 0) NOT NULL,
[CGL_RadiusInMeters] [numeric] (18, 0) NULL,
[CGL_CrUStamp] [varchar] (4) NOT NULL,
[CGL_CrDStamp] [datetime] NOT NULL,
[CGL_UStamp] [varchar] (4) NOT NULL,
[CGL_DStamp] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_ClientsGeoLocations] ADD CONSTRAINT [PK_STK04_ClientsGeoLocations] PRIMARY KEY CLUSTERED ([CGL_CLRecID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STK04_ClientsGeoLocations] ADD CONSTRAINT [FK_STK04_ClientsGeoLocations_Stk04_Clients] FOREIGN KEY ([CGL_CLRecID]) REFERENCES [dbo].[Stk04_Clients] ([CL_RecID])
GO
