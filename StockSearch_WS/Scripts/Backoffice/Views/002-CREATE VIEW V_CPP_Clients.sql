SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
CREATE OR ALTER VIEW V_CPP_Clients
AS
    SELECT  CL_RecID [clientRecID] ,
            CONCAT(CL_Code, ' - ', CL_No) [clientCode] ,
            --CASE WHEN ISNULL(Fo_phone1, '') = '' THEN CL_Name
            --     ELSE CONCAT(Fo_phone1, ' - ', CL_Name)
            --END [clientName] ,
			CL_Name [clientName] ,
            CL_Type [clientType] ,
			CONCAT(ClR_Country,' / ',ClR_Region) [clientCountryRegion] ,
			CL_ACExec [clientAccountExecutiveCode] ,
            E_Name [clientAccountExecutive] ,
            CL_PriceCat [clientPriceCat] ,
            CONCAT(CL_Code, '|', CL_No, '|', CL_Name, '|', Fo_phone1, '|', LTRIM(RTRIM(REPLACE(Fo_phone1, '-',''))) , '|' , LTRIM(RTRIM(REPLACE(Fo_phone1, '/','')))) clientSearchString
    FROM    dbo.Stk04_Clients
            LEFT JOIN dbo.STK04_Employee ON STK04_Employee.E_Code = Stk04_Clients.CL_ACExec
            LEFT JOIN dbo.Com_Folio ON Com_Folio.Fo_serno = Stk04_Clients.CL_FolioNo
                                       AND Com_Folio.Fo_subno = Stk04_Clients.CL_FolioSubNo
			LEFT JOIN dbo.Stk04_ClientsReg ON Stk04_ClientsReg.ClR_Recid = Stk04_Clients.CL_RecID
    WHERE   ISNULL(CL_Active, 'N') = 'Y';





GO
