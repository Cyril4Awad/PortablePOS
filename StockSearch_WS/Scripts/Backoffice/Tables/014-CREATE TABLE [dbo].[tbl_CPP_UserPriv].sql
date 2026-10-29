SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The privileges of the users on the portable menu items, for the whole database (User Privilege tab of the backend)
-- UP_StepCode : the menu item (CM_RecKey of tbl_CPP_ControlizedMenu, its title is CM_MenuTitle)
-- UP_Allowed : 1 the user can use the menu item, 0 not (no row = not allowed, the super user SU is always allowed)
-- UP_PrivTill : the last day the privilege is valid, NULL = no end

-- The first version had a company column (UP_Comp) : that table is dropped and created again
IF COL_LENGTH('dbo.tbl_CPP_UserPriv', 'UP_Comp') IS NOT NULL
DROP TABLE [dbo].[tbl_CPP_UserPriv]
GO

IF OBJECT_ID('dbo.tbl_CPP_UserPriv') IS NULL
CREATE TABLE [dbo].[tbl_CPP_UserPriv]
(
[UP_UserCode] [varchar] (4) NOT NULL,
[UP_StepCode] [varchar] (10) NOT NULL,
[UP_PrivTill] [datetime] NULL,
[UP_Allowed] [bit] NOT NULL CONSTRAINT [DF_tbl_CPP_UserPriv_Allowed] DEFAULT (0),
[UP_UStamp] [varchar] (4) NOT NULL,
[UP_DStamp] [datetime] NOT NULL CONSTRAINT [DF_tbl_CPP_UserPriv_DStamp] DEFAULT (GETDATE()),
CONSTRAINT [PK_tbl_CPP_UserPriv] PRIMARY KEY CLUSTERED ([UP_UserCode], [UP_StepCode]) ON [PRIMARY]
) ON [PRIMARY]
GO
