CREATE TABLE [dbo].[tbl_CPP_Devices]
(
[DEV_Name] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[DEV_ID] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_UniqueID] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_LoggedIn] [bit] NOT NULL,
[DEV_LoggedInUser] [varchar] (4) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_DateStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_Devices] ADD CONSTRAINT [PK_tbl_tbl_CPP_Devices] PRIMARY KEY CLUSTERED ([DEV_Name]) ON [PRIMARY]
GO
