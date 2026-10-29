SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The action buttons of the portable that can be negated, per menu item (Negations tab of the backend)
-- NBS_StepCode : the menu item (CM_RecKey of tbl_CPP_ControlizedMenu), NBS_Button : the action, NBS_ButtonDesc : its title
-- The buttons are added by script only (the list at the end of this script)
IF OBJECT_ID('dbo.tbl_CPP_NegButtonSteps') IS NULL
CREATE TABLE [dbo].[tbl_CPP_NegButtonSteps]
(
[NBS_RecID] [int] IDENTITY(1,1) NOT NULL,
[NBS_StepCode] [varchar] (10) NOT NULL,
[NBS_Button] [varchar] (50) NOT NULL,
[NBS_ButtonDesc] [nvarchar] (100) NOT NULL,
[NBS_UStamp] [varchar] (4) NOT NULL,
[NBS_DStamp] [datetime] NOT NULL CONSTRAINT [DF_tbl_CPP_NegButtonSteps_DStamp] DEFAULT (GETDATE()),
CONSTRAINT [PK_tbl_CPP_NegButtonSteps] PRIMARY KEY CLUSTERED ([NBS_RecID]) ON [PRIMARY],
CONSTRAINT [UQ_tbl_CPP_NegButtonSteps_Button] UNIQUE ([NBS_StepCode], [NBS_Button]) ON [PRIMARY]
) ON [PRIMARY]
GO

/* The buttons that can be negated : new ones are added, the descriptions are kept up to date */
MERGE dbo.tbl_CPP_NegButtonSteps AS t
USING ( VALUES ( 'P1', 'btnShowAvailableQty', N'Show Available Qty' ),     -- Physical Count : Avlb Qty of the lines (also the qty column of Create Physical Count)
               ( 'P1', 'btnListDifferences', N'List Differences' )        -- Physical Count : List Differences of the scan screen
      ) s ( StepCode, Button, ButtonDesc )
ON t.NBS_StepCode = s.StepCode AND t.NBS_Button = s.Button
WHEN MATCHED AND t.NBS_ButtonDesc <> s.ButtonDesc THEN
    UPDATE SET NBS_ButtonDesc = s.ButtonDesc
WHEN NOT MATCHED BY TARGET THEN
    INSERT ( NBS_StepCode, NBS_Button, NBS_ButtonDesc, NBS_UStamp, NBS_DStamp )
    VALUES ( s.StepCode, s.Button, s.ButtonDesc, 'SU', GETDATE() );
GO
