SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The negated buttons of the users, per company (Negations tab of the backend)
-- NB_BSRecID : the button (NBS_RecID of tbl_CPP_NegButtonSteps), NB_StepCode / NB_Button : copied from it
-- NB_HasNeg : 1 the user can do the action, 0 not (no row = not allowed, the super user SU is always allowed)
-- NB_Comp : the company of the location the action is done on
IF OBJECT_ID('dbo.tbl_CPP_NegationButton') IS NULL
CREATE TABLE [dbo].[tbl_CPP_NegationButton]
(
[NB_UserCode] [varchar] (4) NOT NULL,
[NB_Comp] [varchar] (2) NOT NULL,
[NB_BSRecID] [int] NOT NULL,
[NB_StepCode] [varchar] (10) NOT NULL,
[NB_Button] [varchar] (50) NOT NULL,
[NB_HasNeg] [bit] NOT NULL CONSTRAINT [DF_tbl_CPP_NegationButton_HasNeg] DEFAULT (0),
[NB_UStamp] [varchar] (4) NOT NULL,
[NB_DStamp] [datetime] NOT NULL CONSTRAINT [DF_tbl_CPP_NegationButton_DStamp] DEFAULT (GETDATE()),
CONSTRAINT [PK_tbl_CPP_NegationButton] PRIMARY KEY CLUSTERED ([NB_UserCode], [NB_Comp], [NB_BSRecID]) ON [PRIMARY],
CONSTRAINT [FK_tbl_CPP_NegationButton_NegButtonSteps] FOREIGN KEY ([NB_BSRecID]) REFERENCES [dbo].[tbl_CPP_NegButtonSteps] ([NBS_RecID])
) ON [PRIMARY]
GO
