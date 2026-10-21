CREATE TABLE [dbo].[tbl_CPP_ControlizedMenu]
(
[CM_RecKey] [varchar] (10) NOT NULL,
[CM_MenuActive] [bit] NOT NULL,
[CM_UStamp] [varchar] (4) NOT NULL,
[CM_DStamp] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_ControlizedMenu] ADD CONSTRAINT [PK_tbl_CPP_ControlizedMenu] PRIMARY KEY CLUSTERED ([CM_RecKey]) ON [PRIMARY]
GO

/* Steps of the portable main menu (recKey of the 'MI' items in MainMenu.js STEPS) */
INSERT INTO dbo.tbl_CPP_ControlizedMenu ( CM_RecKey, CM_MenuActive, CM_UStamp, CM_DStamp )
SELECT  s.RecKey, s.MenuActive, 'SU', GETDATE()
FROM    ( VALUES ( 'C1', 1 ),   -- Clients View
                 ( 'C2', 1 ),   -- Clients Visits
                 ( 'C3', 1 ),   -- Potential Clients
                 ( 'S1', 1 ),   -- Stock Search
                 ( 'S2', 1 ),   -- Missing Items
                 ( 'S3', 1 ),   -- Physical Count
                 ( 'T1', 1 ),   -- Prepare Sales Orders
                 ( 'T2', 1 ),   -- Portable Clients Orders
                 ( 'T3', 1 ),   -- Return Items Orders
                 ( 'T4', 1 ),   -- Sales Invoice
                 ( 'T5', 1 ),   -- Mobile Collection
                 ( 'R1', 1 ),   -- Reprint Documents (proof of concept : Client Receipt, Consumption)
                 ( 'R2', 1 ),   -- Define Cash Counted
                 ( 'R3', 0 )    -- Daily Journal (no screen yet)
        ) s ( RecKey, MenuActive )
WHERE   NOT EXISTS ( SELECT 1 FROM dbo.tbl_CPP_ControlizedMenu WHERE CM_RecKey = s.RecKey );
GO

/* Reprint Documents has a screen since 2026-10-15 */
UPDATE dbo.tbl_CPP_ControlizedMenu SET CM_MenuActive = 1, CM_DStamp = GETDATE() WHERE CM_RecKey = 'R1' AND CM_MenuActive = 0;
GO
