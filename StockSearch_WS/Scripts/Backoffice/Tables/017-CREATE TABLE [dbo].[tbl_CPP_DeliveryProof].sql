SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- The proofs of delivery taken from the portable (Clients > Delivery Proof) : the client delivered, a remark,
-- an optional photo and the geo location of the device when it was saved
-- DP_LocCode : the location of the web service (LocCode of its web.config), DP_RecID : the next number in that location
-- DP_CLRecID : the client (Stk04_Clients.CL_RecID)
-- DP_Photo : the photo as a jpg (compressed on the device), NULL when no photo was taken
-- DP_Coordinates : the position of the device (geography, SRID 4326), DP_Accuracy : its accuracy in meters
-- DP_RadiusInMeters : the radius chosen on the slider
-- DP_UserStamp : the user logged in on the portable, DP_DateStamp : the time it reached the server
-- The script can be run again : it only creates what is missing
IF OBJECT_ID('dbo.tbl_CPP_DeliveryProof') IS NULL
CREATE TABLE [dbo].[tbl_CPP_DeliveryProof]
(
[DP_LocCode] [varchar] (4) NOT NULL,
[DP_RecID] [numeric] (18, 0) NOT NULL,
[DP_CLRecID] [numeric] (18, 0) NOT NULL,
[DP_Remarks] [nvarchar] (max) NOT NULL,
[DP_Photo] [varbinary] (max) NULL,
[DP_Coordinates] [sys].[geography] NOT NULL,
[DP_Accuracy] [numeric] (18, 2) NOT NULL,
[DP_RadiusInMeters] [numeric] (18, 0) NOT NULL,
[DP_UserStamp] [varchar] (4) NOT NULL,
[DP_DateStamp] [datetime] NOT NULL CONSTRAINT [DF_tbl_CPP_DeliveryProof_DateStamp] DEFAULT (GETDATE()),
CONSTRAINT [PK_tbl_CPP_DeliveryProof] PRIMARY KEY CLUSTERED ([DP_LocCode], [DP_RecID]) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

IF NOT EXISTS ( SELECT 1 FROM sys.indexes WHERE name = 'IX_tbl_CPP_DeliveryProof_Client' AND object_id = OBJECT_ID('dbo.tbl_CPP_DeliveryProof') )
CREATE NONCLUSTERED INDEX [IX_tbl_CPP_DeliveryProof_Client] ON [dbo].[tbl_CPP_DeliveryProof] ([DP_CLRecID], [DP_DateStamp]) ON [PRIMARY]
GO
