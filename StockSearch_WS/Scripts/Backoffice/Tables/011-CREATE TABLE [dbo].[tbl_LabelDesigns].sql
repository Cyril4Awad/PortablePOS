
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbl_LabelDesigns](
	[CLD_Code] [varchar](2) NOT NULL,
	[CLD_Name] [nvarchar](100) NOT NULL,
	[CLD_Document] [nvarchar](100) NOT NULL,
	[CLD_DefaultPrinter] [nvarchar](100) NULL,
	[CLD_Remarks] [nvarchar](300) NULL,
	[CLD_IsDefault] [bit] NOT NULL,
	[CLD_Active] [bit] NOT NULL,
	[CLD_Ustmp] [varchar](5) NOT NULL,
	[CLD_Dstmp] [datetime] NOT NULL,
	[CLD_LabelSize] [varchar](20) NULL,
	[CLD_LabelOption] [varchar](10) NULL,
	[CLD_LabelType] [varchar](10) NULL,
	[CLD_TemplateFolder] [nvarchar](500) NULL,
 CONSTRAINT [PK_tbl_LabelDesigns] PRIMARY KEY CLUSTERED 
(
	[CLD_Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


