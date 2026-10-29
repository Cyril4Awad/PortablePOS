-- The devices (portables) allowed on this database, DEV_Name = the device name (Settings of the device > About phone > Device name)
-- The script can be run again : the table is created when missing, then the device below is added
IF OBJECT_ID('dbo.tbl_CPP_Devices') IS NULL
CREATE TABLE [dbo].[tbl_CPP_Devices]
(
[DEV_Name] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1256_CI_AS NOT NULL,
[DEV_ID] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_UniqueID] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_LoggedIn] [bit] NOT NULL,
[DEV_LoggedInUser] [varchar] (4) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,
[DEV_DateStamp] [datetime] NOT NULL,
CONSTRAINT [PK_tbl_tbl_CPP_Devices] PRIMARY KEY CLUSTERED ([DEV_Name]) ON [PRIMARY]
) ON [PRIMARY]
GO

/* Adds a device for a client deployment : put the device name, run, then press Register Device on the login screen of that device
   (Register Device fills DEV_ID and DEV_UniqueID of this row, it never adds the row).
   Nothing is added while the name is empty, a device already in the table is left as it is */
DECLARE @DeviceName NVARCHAR(30) = N'';

IF LTRIM(RTRIM(@DeviceName)) <> ''
   AND NOT EXISTS ( SELECT TOP 1 1 FROM dbo.tbl_CPP_Devices WHERE DEV_Name = LTRIM(RTRIM(@DeviceName)) )
    INSERT INTO dbo.tbl_CPP_Devices ( DEV_Name, DEV_LoggedIn, DEV_DateStamp )
    VALUES ( LTRIM(RTRIM(@DeviceName)), 0, GETDATE() );
GO
