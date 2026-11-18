SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The menu items of the portable that are visible on this database (recKey of the 'MI' items in MainMenu.js STEPS)
-- CM_MenuTitle : the title shown in the app, used by the User Privilege tab of the backend
-- The script can be run again : it creates what is missing and brings the rows to the latest version
IF OBJECT_ID('dbo.tbl_CPP_ControlizedMenu') IS NULL
CREATE TABLE [dbo].[tbl_CPP_ControlizedMenu]
(
[CM_RecKey] [varchar] (10) NOT NULL,
[CM_MenuTitle] [nvarchar] (50) NULL,
[CM_MenuActive] [bit] NOT NULL,
[CM_UStamp] [varchar] (4) NOT NULL,
[CM_DStamp] [datetime] NOT NULL,
CONSTRAINT [PK_tbl_CPP_ControlizedMenu] PRIMARY KEY CLUSTERED ([CM_RecKey]) ON [PRIMARY]
) ON [PRIMARY]
GO

IF COL_LENGTH('dbo.tbl_CPP_ControlizedMenu', 'CM_MenuTitle') IS NULL
ALTER TABLE [dbo].[tbl_CPP_ControlizedMenu] ADD [CM_MenuTitle] [nvarchar] (50) NULL
GO

/* Steps of the portable main menu : new ones are added with their default, the titles are always updated */
MERGE dbo.tbl_CPP_ControlizedMenu AS t
USING ( VALUES ( 'C1', N'Clients View', 1 ),
               ( 'C2', N'Clients Visits', 1 ),
               ( 'C3', N'Potential Clients', 1 ),
               ( 'C4', N'Delivery Proof', 1 ),
               ( 'S1', N'Stock Search', 1 ),
               ( 'S2', N'Missing Items', 1 ),
               ( 'T1', N'Prepare Sales Orders', 1 ),
               ( 'T2', N'Portable Clients Orders', 1 ),
               ( 'T3', N'Return Items Orders', 1 ),
               ( 'T4', N'Sales Invoice', 1 ),
               ( 'T5', N'Mobile Collection', 1 ),
               ( 'T6', N'Free Receiving', 1 ),
               ( 'T7', N'Receiving Order', 1 ),
               ( 'X1', N'Transfer', 1 ),
               ( 'P2', N'Create Physical Count', 1 ),
               ( 'P1', N'Physical Count', 1 ),
               ( 'L1', N'Print Label', 1 ),
               ( 'R1', N'Reprint Documents', 1 ),
               ( 'R2', N'Define Cash Counted', 0 ),
               ( 'R3', N'Daily Journal', 0 )     -- no screen yet
      ) s ( RecKey, MenuTitle, MenuActive )
ON t.CM_RecKey = s.RecKey
WHEN MATCHED AND ISNULL(t.CM_MenuTitle, N'') <> s.MenuTitle THEN
    UPDATE SET CM_MenuTitle = s.MenuTitle
WHEN NOT MATCHED BY TARGET THEN
    INSERT ( CM_RecKey, CM_MenuTitle, CM_MenuActive, CM_UStamp, CM_DStamp )
    VALUES ( s.RecKey, s.MenuTitle, s.MenuActive, 'SU', GETDATE() );
GO

/* S3 (the first Physical Count) is no longer a menu item : Physical Count is P1 */
DELETE FROM dbo.tbl_CPP_ControlizedMenu WHERE CM_RecKey = 'S3';
GO
