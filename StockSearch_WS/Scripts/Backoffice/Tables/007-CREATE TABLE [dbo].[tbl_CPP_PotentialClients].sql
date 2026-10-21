CREATE TABLE [dbo].[tbl_CPP_PotentialClients]
(
[PC_LocCode] [varchar] (4) NOT NULL,
[PC_RecID] [numeric] (18, 0) NOT NULL,
[PC_ClientName] [nvarchar] (100) NOT NULL,
[PC_Business] [nvarchar] (100) NULL,
[PC_Contact] [nvarchar] (100) NULL,
[PC_PhoneMobile] [nvarchar] (100) NULL,
[PC_Address] [nvarchar] (500) NULL,
[PC_Remarks] [nvarchar] (500) NULL,
[PC_Coordinates] [sys].[geography] NOT NULL,
[PC_Accuracy] [numeric] (18, 0) NOT NULL,
[PC_RadiusInMeters] [numeric] (18, 0) NULL,
[PC_UserStamp] [varchar] (4) NOT NULL,
[PC_DateStamp] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_CPP_PotentialClients] ADD CONSTRAINT [PK_tbl_CPP_PotentialClients] PRIMARY KEY CLUSTERED ([PC_LocCode], [PC_RecID]) ON [PRIMARY]
GO
