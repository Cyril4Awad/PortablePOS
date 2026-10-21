CREATE TABLE [dbo].[tbl_CPP_MissingItems]
(
[MI_LocCode] [varchar] (4) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[MI_RecID] [numeric] (18, 0) NOT NULL,
[MI_Description] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[MI_Brand] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[MI_RequestedBy] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[MI_UserStamp] [varchar] (4) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[MI_DateStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_MissingItems] ADD CONSTRAINT [PK_tbl_CPP_MissingItems] PRIMARY KEY CLUSTERED ([MI_LocCode], [MI_RecID]) ON [PRIMARY]
GO
