SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Every counting step of the portable Physical Count (one row per scan, set total or undo), saved on the device
-- offline and sent with the upload : what changed on a count line and how
-- PCL_Operator : the machine name (device) that counted, PCL_UStamp : the user logged in on it
-- PCL_Qty : the quantity of the step (add : the added qty, set : the typed total, undo / change : the change of the total)
-- PCL_Total : the total of the line after the step
-- PCL_Mode : the counting mode (plusOne, scanThenType, typeThenScan, buttons, scanItems = the old Scan Items screen)
-- PCL_Action : add, set (Set Total), undo, change (buttons of the old Scan Items screen)
-- PCL_DStamp : the time of the step on the device, PCL_UploadDStamp : the time it reached the server
-- PCL_DeviceLogID : the id of the row on the device, a log sent twice (upload retried) is only saved once
IF OBJECT_ID('dbo.tbl_CPP_PhysicalCountLogs') IS NULL
CREATE TABLE [dbo].[tbl_CPP_PhysicalCountLogs]
(
[PCL_RecID] [int] IDENTITY(1,1) NOT NULL,
[PCL_CountID] [decimal] (18, 0) NOT NULL,
[PCL_Location] [varchar] (2) NOT NULL,
[PCL_LineRecID] [decimal] (18, 0) NOT NULL,
[PCL_StkID] [decimal] (18, 0) NULL,
[PCL_Operator] [nvarchar] (30) NOT NULL,
[PCL_Qty] [decimal] (18, 3) NOT NULL,
[PCL_Total] [decimal] (18, 3) NULL,
[PCL_Mode] [varchar] (20) NOT NULL,
[PCL_Action] [varchar] (10) NOT NULL,
[PCL_Unit] [nvarchar] (30) NULL,
[PCL_DeviceLogID] [int] NOT NULL,
[PCL_UStamp] [varchar] (4) NULL,
[PCL_DStamp] [datetime] NOT NULL,
[PCL_UploadDStamp] [datetime] NOT NULL CONSTRAINT [DF_tbl_CPP_PhysicalCountLogs_UploadDStamp] DEFAULT (GETDATE()),
CONSTRAINT [PK_tbl_CPP_PhysicalCountLogs] PRIMARY KEY CLUSTERED ([PCL_RecID]) ON [PRIMARY]
) ON [PRIMARY]
GO

IF NOT EXISTS ( SELECT 1 FROM sys.indexes WHERE name = 'UX_tbl_CPP_PhysicalCountLogs_DeviceLog' AND object_id = OBJECT_ID('dbo.tbl_CPP_PhysicalCountLogs') )
CREATE UNIQUE NONCLUSTERED INDEX [UX_tbl_CPP_PhysicalCountLogs_DeviceLog] ON [dbo].[tbl_CPP_PhysicalCountLogs] ([PCL_Operator], [PCL_Location], [PCL_CountID], [PCL_DeviceLogID]) ON [PRIMARY]
GO

IF NOT EXISTS ( SELECT 1 FROM sys.indexes WHERE name = 'IX_tbl_CPP_PhysicalCountLogs_Line' AND object_id = OBJECT_ID('dbo.tbl_CPP_PhysicalCountLogs') )
CREATE NONCLUSTERED INDEX [IX_tbl_CPP_PhysicalCountLogs_Line] ON [dbo].[tbl_CPP_PhysicalCountLogs] ([PCL_Location], [PCL_CountID], [PCL_LineRecID]) ON [PRIMARY]
GO
