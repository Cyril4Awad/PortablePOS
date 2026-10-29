
Imports System.Threading
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Net
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports PortablePOS_WS
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization

Public Class DataService
    Implements IDataService

    Private LocCode As String = ConfigurationManager.AppSettings("LocCode").ToString

#Region "Credentials"

    Public Function UserLogin(userCode As String, userPassword As String, deviceName As String) As String Implements IDataService.UserLogin

        Dim strsql As String

        Try

            If userCode.ToUpper = "SU" Then

                Dim dd, times As Int32
                Dim YY, Pswd As String

                dd = Now.Day : YY = Now.Year.ToString

                If dd Mod 2 = 0 Then
                    times = 2
                Else
                    times = 3
                End If

                Pswd = (Now.Month * times).ToString.PadLeft(2, "0"c) &
                YY.Substring(YY.Length - 1) & (dd * times).ToString.PadLeft(2, "0"c)

                If userPassword.ToUpper = Pswd.ToUpper Then
                    Return "[{""UserName"":""SuperUser""}]"
                Else
                    Return ""
                End If

            Else

                'Only a registered device logs in (SU always can) : the row of the device in tbl_CPP_Devices with the IDs filled by Register Device,
                'the token header sent by the portable is DeviceName|DeviceID_UniqueID (same check as Global.asax)
                Dim deviceToken As String = If(System.ServiceModel.Web.WebOperationContext.Current?.IncomingRequest.Headers("token"), "")

                If Not RecordsExists($"SELECT TOP 1 1 FROM tbl_CPP_Devices
                                       WHERE DEV_Name = '{deviceName.Trim.Replace("'", "''")}'
                                         AND ISNULL(DEV_ID, '') <> ''
                                         AND ISNULL(DEV_UniqueID, '') <> ''
                                         AND CONCAT(DEV_Name, '|', DEV_ID, '_', DEV_UniqueID) = '{deviceToken.Replace("'", "''")}' ") Then

                    Return "UNR"

                End If

                If RecordsExists($"SELECT TOP 1 1 FROM tbl_CPP_Devices WHERE DEV_LoggedInUser = '{userCode.Trim.Replace("'", "''")}' AND DEV_Name <> '{deviceName.Trim.Replace("'", "''")}' ") Then

                    Return "UAL"

                Else

                    strsql = $" SELECT User_Name [UserName] 
                                FROM Com_UserInfo 
                                WHERE User_Code = '" & userCode.Trim.Replace("'", "''") & "' 
                                  AND User_Pwd = '" & EncPS(userPassword.Trim.Replace("'", "''")).Replace("'", "''") & "'
                                  AND ISNULL(User_Status,'I') = 'A' "

                    Using dtUserLogin As New DataTable

                        Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                            daSql.SelectCommand.CommandTimeout = 600
                            daSql.Fill(dtUserLogin)
                        End Using

                        If dtUserLogin.Rows.Count = 0 Then
                            Return ""
                        Else

                            Execute_WOXACT($" UPDATE tbl_CPP_Devices 
                                              SET DEV_LoggedInUser = '{userCode.Trim.Replace("'", "''")}' ,
                                                  DEV_DateStamp = GETDATE()
                                              WHERE DEV_Name = '{deviceName.Trim.Replace("'", "''")}'  ")

                            Return GetJson(dtUserLogin)
                        End If

                    End Using

                End If

            End If

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    Public Function UserLogout(deviceName As String) As String Implements IDataService.UserLogout

        Try

            Execute_WOXACT($" UPDATE tbl_CPP_Devices 
                                  SET DEV_LoggedInUser = NULL ,
                                      DEV_DateStamp = GETDATE()
                                  WHERE DEV_Name = '{deviceName.Trim.Replace("'", "''")}'  ")

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    'Active users for the offline login of the portable, the password is never sent :
    'pwdHash = SHA-256 of OfflineLoginSalt + UPPER(user code) + '|' + hex of the stored User_Pwd bytes (EncPS result),
    'the portable computes the same hash from the typed password (SecurityData.js)
    Private Const OfflineLoginSalt As String = "App|OfflineLogin|"

    Public Function GetOfflineUsers(deviceName As String) As String Implements IDataService.GetOfflineUsers

        Try

            Using dtUsers As New DataTable

                Using daSql As New SqlDataAdapter($" SELECT  UPPER(LTRIM(RTRIM(User_Code))) [userCode] ,
                                                             User_Name [userName] ,
                                                             CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONCAT('{OfflineLoginSalt}', UPPER(LTRIM(RTRIM(User_Code))), '|',
                                                                     CONVERT(VARCHAR(20), CAST(RTRIM(User_Pwd) AS VARBINARY(10)), 2))), 2) [pwdHash]
                                                     FROM    Com_UserInfo
                                                     WHERE   ISNULL(User_Status, 'I') = 'A'
                                                       AND   ISNULL(User_Pwd, '') <> '' ",
                                                  DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtUsers)
                End Using

                If dtUsers.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtUsers)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Online / offline status of the portable menu footer
    Public Function Ping() As String Implements IDataService.Ping
        Return "Success"
    End Function

#End Region

#Region "POS Info"

    Private Function CurrentLocationWorkingDay() As String
        Try

            'Pending : Since we are working on a location head office only, not need to check if the day is closed or no

            Return ExecuteScalar_WOXACT("SELECT MAX(CT_OPDATE) FROM dbo.POS_CASHTRANSACTION WHERE CT_LOCATION='" & LocCode & "' AND CT_RECID='O'")

        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Function GetLocationPretty() As String Implements IDataService.GetLocationPretty

        Try

            Return ExecuteScalar_WOXACT($" SELECT  CONCAT(Loc_Code, ' - ', Loc_Desc)
                                           FROM    dbo.STK04_Location
                                           WHERE   Loc_Code = '{LocCode}' ")

        Catch ex As Exception
            Return LocCode
        End Try

    End Function

    Public Function RegisterDevice(deviceName As String, deviceID As String, deviceUniqueID As String) As String Implements IDataService.RegisterDevice

        Dim strResult As String

        Try

            strResult = ExecuteScalar_WOXACT($" 

                DECLARE @DeviceName NVARCHAR(30);
                DECLARE @DeviceID NVARCHAR(100);
                DECLARE @DeviceUniqueID NVARCHAR(100);

                SET @DeviceName = '{deviceName.Replace("'", "''")}';
                SET @DeviceID = '{deviceID.Replace("'", "''")}';
                SET @DeviceUniqueID = '{deviceUniqueID.Replace("'", "''")}';

                IF EXISTS ( SELECT TOP 1
                                    1
                            FROM    tbl_CPP_Devices
                            WHERE   DEV_Name = @DeviceName
                                    AND ISNULL(DEV_ID, '') = ''
                                    AND ISNULL(DEV_UniqueID, '') = '' )
                    BEGIN
                        UPDATE  tbl_CPP_Devices
                        SET     DEV_ID = @DeviceID ,
                                DEV_UniqueID = @DeviceUniqueID ,
                                DEV_DateStamp = GETDATE()
                        WHERE   DEV_Name = @DeviceName;
                        SELECT  'Device registered successfully';
                    END;
                ELSE
                    IF EXISTS ( SELECT TOP 1
                                        1
                                FROM    tbl_CPP_Devices
                                WHERE   DEV_Name = @DeviceName
                                        AND ISNULL(DEV_ID, '') = @DeviceID
                                        AND ISNULL(DEV_UniqueID, '') = @DeviceUniqueID )
                        BEGIN
                            SELECT  'Device already registered';
                        END;	
                    ELSE
                        BEGIN
                            SELECT  'Unauthorized Device';
                        END;

                ")

            Return strResult

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    Public Function GetCashRegister(deviceName As String) As String Implements IDataService.GetCashRegister

        Try

            Return ExecuteScalar_WOXACT($" SELECT  CR_Code
                                           FROM    dbo.pos_cashreg
                                           WHERE   CR_Loc = '{LocCode}'
                                                   AND CR_CompName = '{deviceName.Trim.Replace("'", "''")}'
                                                   AND ISNULL(CR_Active, '') = 'Y' ")

        Catch ex As Exception
            Return ""
        End Try

    End Function

    Public Function GetCashRegisterPretty(deviceName As String) As String Implements IDataService.GetCashRegisterPretty

        Try

            Return ExecuteScalar_WOXACT($" SELECT  CONCAT(CR_Code , ' - ' , CR_Desc)
                                           FROM    dbo.pos_cashreg
                                           WHERE   CR_Loc = '{LocCode}'
                                                   AND CR_CompName = '{deviceName.Trim.Replace("'", "''")}'
                                                   AND ISNULL(CR_Active, '') = 'Y' ")

        Catch ex As Exception
            Return ""
        End Try

    End Function

    Public Function GetWorkingDay() As String Implements IDataService.GetWorkingDay
        Try

            Return CDate(CurrentLocationWorkingDay()).ToString("dd/MM/yyyy")

        Catch ex As Exception
            Return ""
        End Try

    End Function

#End Region

#Region "Clients"

    Public Function GetClientGroups(clientGroupType As String, userCode As String) As String Implements IDataService.GetClientGroups

        Dim strsql As String

        Try

            If clientGroupType.ToUpper = "CT" Then

                strsql = $"SELECT  'CT' clientGroupType,
                                   clientTypeCode clientGroupCode,
                                   clientTypeDesc clientGroupDesc
                           FROM    V_CPP_ClientTypes 
                           WHERE  EXISTS (SELECT TOP 1 1 
                                          FROM dbo.V_CPP_Clients 
                                          WHERE (clientTypeCode = clientType OR clientTypeCode = 'ALL')
                                          /* AND clientAccountExecutiveCode = '{userCode}' */ ) "

            Else

                strsql = $"SELECT  'CCR' clientGroupType,
                                   clientCountryRegionsCode clientGroupCode,
                                   clientCountryRegionsDesc clientGroupDesc
                           FROM    V_CPP_ClientCountryRegions
                           WHERE   EXISTS ( SELECT TOP 1 1
                                            FROM   dbo.V_CPP_Clients
                                            WHERE  clientCountryRegion = clientCountryRegionsCode
                                             /*  AND clientAccountExecutiveCode = '{userCode}' */ ) "

            End If


            Using dtClientGroups As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtClientGroups)
                End Using

                If dtClientGroups.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtClientGroups)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    Public Function GetClients(clientType As String, clientCountryRegion As String, searchString As String, latitude As Double, longitude As Double, userCode As String, onlyHavingVisits As Boolean, visitDate As String) As String Implements IDataService.GetClients

        Dim strsql As String

        Try

            strsql = $" SELECT  clientRecID ,
                                clientCode ,
                                clientName ,
                                clientType ,
                                clientCountryRegion ,
                                clientAccountExecutive ,
                                clientPriceCat ,
                                clientSearchString ,
                                CASE WHEN CLV_CLRecID IS NULL THEN 'N' ELSE 'Y' END clientVisited,
                                CASE WHEN CGL_CLRecID IS NULL THEN -1
                                     ELSE CASE WHEN CGL_Coordinates.STDistance(geography::Point({latitude}, {longitude},
                                                                                      4326)) <= CGL_RadiusInMeters
                                               THEN 1
                                               ELSE 0
                                          END
                                END clientGeoStatus
                                {IIf(onlyHavingVisits, " , CONVERT(VARCHAR,dr_Date,23) dr_Date ", "").ToString}
                        FROM    V_CPP_Clients "

            If onlyHavingVisits Then

                strsql &= $" INNER JOIN (SELECT ClientRecID VisitClientRecID , dr_Date
                                         FROM V_CPP_ClientsVisits 
                                         WHERE Visit = 1
                                         {IIf(String.IsNullOrEmpty(visitDate), "", $" AND dr_date = '{visitDate}' ").ToString}) 
                                         V_CPP_ClientsVisits ON V_CPP_Clients.clientRecID = V_CPP_ClientsVisits.VisitClientRecID "

            End If

            strsql &= $" LEFT JOIN dbo.STK04_ClientsGeoLocations ON STK04_ClientsGeoLocations.CGL_CLRecID = clientRecID "

            strsql &= $" LEFT JOIN ( SELECT  DISTINCT
                                             CLV_CLRecID
                                     FROM    dbo.STK04_ClientsVisits
                                     WHERE   CLV_UserCode = '{userCode}'
                                             AND CONVERT(DATE, CLV_DateTime) = CONVERT(DATE, GETDATE())
                                   ) STK04_ClientsVisits ON STK04_ClientsVisits.CLV_CLRecID = clientRecID "

            strsql &= $" WHERE 1=1 /* clientAccountExecutiveCode = '{userCode}' */ "

            If Not String.IsNullOrEmpty(clientType) Then
                If Not clientType.Trim.ToUpper = "ALL".ToUpper Then
                    strsql &= $" AND clientType = '{clientType.Trim.Replace("'", "''")}' "
                End If
            End If

            If Not String.IsNullOrEmpty(clientCountryRegion) Then
                strsql &= $" AND clientCountryRegion = '{clientCountryRegion.Trim.Replace("'", "''")}' "
            End If

            If Not String.IsNullOrEmpty(searchString) Then
                strsql &= $" AND (
                                    clientCode LIKE '%{searchString.Trim.Replace("'", "''")}%'
							        OR clientName LIKE '%{searchString.Trim.Replace("'", "''")}%'
                                )"
            End If

            strsql &= " ORDER BY clientGeoStatus DESC, clientName "

            'V_CPP_ClientsVisits lists the days of the year with a recursive CTE (365 levels, the default limit is 100)
            If onlyHavingVisits Then strsql &= " OPTION (MAXRECURSION 400) "

            Using dtClients As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtClients)
                End Using

                If dtClients.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtClients)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function SaveClientVisit(clientRecID As String, userCode As String) As String Implements IDataService.SaveClientVisit

        Dim strsql As String
        Try

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        strsql = $" INSERT INTO dbo.STK04_ClientsVisits
                                            ( CLV_CLRecID ,
                                              CLV_UserCode ,
                                              CLV_DateTime
                                            )
                                    VALUES  ( {clientRecID} , /* CLV_CLRecID - numeric */
                                              '{userCode}' , /* CLV_UserCode - varchar(4) */
                                              GETDATE()  /* CLV_DateTime - datetime */
                                            ) "

                        cmsql.CommandText = strsql
                        cmsql.ExecuteNonQuery()

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try

    End Function


#End Region

#Region "Geo Location"

    Private Function SaveClientGeoLocation(clientRecID As String, latitude As Double, longitude As Double, accuracy As Double, radiusInMeters As Double, userCode As String) As String Implements IDataService.SaveClientGeoLocation
        Dim strsql As String
        Try

            If Not RecordsExists($" SELECT TOP 1 1 FROM dbo.STK04_ClientsGeoLocations WHERE CGL_CLRecID = {clientRecID}") Then

                strsql = $" INSERT INTO dbo.STK04_ClientsGeoLocations
	                        VALUES  ( {clientRecID} , /* CGL_CLRecID - numeric */
	                                    geography::Point({latitude}, {longitude}, 4326) , /* CGL_Coordinates - geography */
	                                    {accuracy} , /* CGL_Accuracy - numeric */
	                                    {radiusInMeters} , /* CGL_RadiusInMeters - numeric */
	                                    '{userCode}' , /* CGL_CrUStamp - varchar(4) */
	                                    GETDATE() , /* CGL_CrDStamp - datetime */
	                                    '{userCode}' , /* CGL_UStamp - varchar(4) */
	                                    GETDATE()  /* CGL_DStamp - datetime */
	                                )  "

            Else

                If NegationPriv(userCode, "MnuPortablePOS", "btnChangeClientGeoLocation") Then

                    strsql = $" UPDATE dbo.STK04_ClientsGeoLocations
	                        SET CGL_Coordinates = geography::Point({latitude}, {longitude}, 4326) ,
	                            CGL_Accuracy = {accuracy} , 
	                            CGL_RadiusInMeters = {radiusInMeters} , 
	                            CGL_UStamp = '{userCode}' , 
	                            CGL_DStamp = GETDATE() 
	                        WHERE CGL_CLRecID = {clientRecID} "

                Else

                    Return "No Priv. to change client geo location"

                End If

            End If

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnsql.Open()

                Using cmsql As New SqlCommand(strsql, cnsql)
                    cmsql.CommandTimeout = 300
                    cmsql.ExecuteNonQuery()
                End Using

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try
    End Function

    Public Function GetClientGeoLocation(clientRecID As String, latitude As Double, longitude As Double) As String Implements IDataService.GetClientGeoLocation

        Dim strsql As String

        Try

            strsql = $"SELECT  CGL_Coordinates.Lat latitude,
                               CGL_Coordinates.Long longitude,
                               CGL_Accuracy accuracy,
                               CGL_RadiusInMeters radiusInMeters,
                               CGL_Coordinates.STDistance(geography::Point({latitude}, {longitude}, 4326)) AS distanceInMeters ,
                               CASE WHEN CGL_Coordinates.STDistance(geography::Point({latitude}, {longitude}, 4326)) <= CGL_RadiusInMeters
                                 THEN 1
                                 ELSE 0
                               END status
                       FROM    dbo.STK04_ClientsGeoLocations
                       WHERE CGL_CLRecID = {clientRecID} "

            Using dtClientGeoLocation As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtClientGeoLocation)
                End Using

                If dtClientGeoLocation.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtClientGeoLocation)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

#End Region

#Region "Dues"

    Function GetClientDues(clientRecID As String) As String Implements IDataService.GetClientDues

        Dim strsql As String
        Try

            strsql = $" EXEC dbo.SP_CPP_GetClientDues {clientRecID} "

            Using dtGetClientDues As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtGetClientDues)
                End Using

                If dtGetClientDues.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtGetClientDues)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Purchases"

    Public Function GetClientLastPurchases(clientRecID As String) As String Implements IDataService.GetClientLastPurchases

        Dim strsql As String

        Try

            strsql = $" 

 SELECT ROW_NUMBER() OVER ( ORDER BY POS_SALEINVHEAD.INVH_DATE DESC, POS_SALEINVHEAD.INVH_RECID DESC ) recKey ,
        STK04_STKFIL.STK_RECID ,
        CONVERT(VARCHAR(10), POS_SALEINVHEAD.INVH_DATE,103) AS [invoiceDate] ,
        INVD_Salesman AS [salesMan] ,
        POS_SALEINVHEAD.INVH_DOCTYP1 + ' ' + POS_SALEINVHEAD.INVH_DOCPREFIX
        + ' ' + CAST(POS_SALEINVHEAD.INVH_SERNO AS VARCHAR(18)) AS [invoiceNumber] ,
        ISNULL(NULLIF(INVH_CURR, ''), INVH_CURR) AS [invoiceCurrency] ,
        INVH_RATE [invoiceRate] ,
        STK_REF [itemReference] ,
        POS_SALEINVDET.INVD_ITMDESC AS [itemDescription] ,
        ( CASE WHEN INVD_NET < 0 THEN -dbo.POS_SALEINVDET.INVD_QTY
               WHEN INVD_GIFT = 'Y'
                    AND INVH_DOCTYP1 = 'CN' THEN -dbo.POS_SALEINVDET.INVD_QTY
               ELSE dbo.POS_SALEINVDET.INVD_QTY
          END ) AS [itemQty] ,
        ISNULL(NULLIF(INVD_UPRICE, 0), INVD_UPRICE) [itemUnitPrice] ,
        CASE WHEN ( STK04_STKFIL.STK_VATPER > 0
                    AND STK04_STKFIL.STK_VATTYPE = 'T'
                    AND Loc_VATRetType = 'T'
                  ) THEN 'TTC'
             WHEN ( STK04_STKFIL.STK_VATPER > 0
                    AND STK04_STKFIL.STK_VATTYPE = 'T'
                    AND Loc_VATRetType = 'A'
                  ) THEN 'TAX'
             ELSE 'No'
        END AS [itemVatType] ,
        dbo.POS_SALEINVDET.INVD_DISC AS [itemDiscount] ,
        ROUND(ISNULL(NULLIF(INVD_NET, 0), INVD_NET)
              - CASE WHEN INVH_VATTYPE = 'T'
                     THEN ISNULL(ISNULL(NULLIF(INVD_VATVAL, 0), INVD_VATVAL),
                                 0)
                     ELSE 0
                END, 3) [itemNetTotal] ,
        ISNULL(INVD_GIFT, 'N') [itemIsGift]
 FROM   dbo.POS_SALEINVDET
        INNER JOIN ( SELECT INVH_RECID ,
                            INVH_LOC ,
                            INVH_DATE ,
                            INVH_CLIENTSUP ,
                            INVH_CLNAME ,
                            INVH_DOCTYP1 ,
                            INVH_DOCPREFIX ,
                            INVH_SERNO ,
                            INVH_CURR ,
                            INVH_RATE ,
                            INVH_RKS ,
                            INVH_CLADDTEL ,
                            INVH_VATTYPE ,
                            INVH_VATLCRate
                     FROM   dbo.POS_SALEINVHEAD
                     WHERE  INVH_LOC = '{LocCode}'
                            AND INVH_CLIENTSUP = {clientRecID}
                            AND INVH_DOCTYP1 IN ( 'IN' )
                   ) POS_SALEINVHEAD ON POS_SALEINVDET.INVD_LOC = POS_SALEINVHEAD.INVH_LOC
                                        AND POS_SALEINVDET.INVD_HEADID = POS_SALEINVHEAD.INVH_RECID
        INNER JOIN ( SELECT Loc_Code ,
                            Loc_Desc ,
                            Loc_VATRetType ,
                            Loc_CoCode
                     FROM   dbo.STK04_Location
                     WHERE  Loc_Code = '{LocCode}'
                   ) stk04_location ON INVD_LOC = Loc_Code
        INNER JOIN ( SELECT INVH_CLIENTSUP ,
                            INVD_STKID mxRecid ,
                            MAX(INVH_DATE) AS [Mxdate]
                     FROM   dbo.POS_SALEINVDET
                            INNER JOIN ( SELECT INVH_LOC ,
                                                INVH_RECID ,
                                                INVH_CLIENTSUP ,
                                                INVH_DATE
                                         FROM   dbo.POS_SALEINVHEAD
                                         WHERE  INVH_LOC = '{LocCode}'
                                                AND INVH_CLIENTSUP = {clientRecID}
                                                AND INVH_DOCTYP1 IN ( 'IN' )
                                       ) INVHEAD ON dbo.POS_SALEINVDET.INVD_LOC = INVHEAD.INVH_LOC
                                                    AND dbo.POS_SALEINVDET.INVD_HEADID = INVHEAD.INVH_RECID
                            INNER JOIN ( SELECT STK_RECID ,
                                                STK_Brand ,
                                                STK_CATEGORY ,
                                                STK_ITMTYPE ,
                                                STK_ITMIDENT
                                         FROM   STK04_STKFIL
                                       ) STK04_STKFIL ON STK04_STKFIL.STK_RECID = dbo.POS_SALEINVDET.INVD_STKID
                     GROUP BY INVD_STKID ,
                            INVH_CLIENTSUP
                   ) MAXClientP ON POS_SALEINVDET.INVD_STKID = MAXClientP.mxRecid
                                   AND POS_SALEINVHEAD.INVH_DATE = MAXClientP.Mxdate
        INNER JOIN ( SELECT STK_RECID ,
                            STK_REF ,
                            STK_BOOKID ,
                            STK_VATPER ,
                            STK_VATTYPE
                     FROM   STK04_STKFIL
                   ) STK04_STKFIL ON dbo.POS_SALEINVDET.INVD_STKID = STK04_STKFIL.STK_RECID
 ORDER BY POS_SALEINVHEAD.INVH_DATE DESC, POS_SALEINVHEAD.INVH_RECID DESC "

            Using dtClientLastPurchases As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtClientLastPurchases)
                End Using

                If dtClientLastPurchases.Rows.Count = 0 Then
                    Return ""
                Else

                    Dim LastInvoiceNumber As String = ""

                    For Each rec In dtClientLastPurchases.Select()

                        If LastInvoiceNumber.ToUpper = rec("InvoiceNumber").ToString.ToUpper Then
                            rec("InvoiceNumber") = ""
                            rec("invoiceDate") = ""
                        Else
                            LastInvoiceNumber = rec("InvoiceNumber").ToString
                        End If

                    Next


                    Return GetJson(dtClientLastPurchases)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

#End Region

#Region "Client Statemenet"

    Public Function GetClientStatement(clientRecID As String, userCode As String) As String Implements IDataService.GetClientStatement

        Dim strsql As String

        Try

            strsql = TemClientBalnc(clientRecID, userCode)

            Using dtClientStatement As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtClientStatement)
                End Using

                If dtClientStatement.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtClientStatement)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    Public Function TemClientBalnc(ByVal ClientRecID As String, ByVal UserCode As String) As String
        Dim strsql As String

        Dim LinkServerAndLinkDB As String
        Dim TempAllTransactionCC, pTempSale As String
        Dim LocOrgCurr, Copercurr As String
        Dim gbCurr1, gbCurr2, gbApp, gbComp, gbYear As String
        Try

            Copercurr = "FC"

            LinkServerAndLinkDB = ""

            gbApp = "POSO"

            strsql = "  SELECT TOP 1
                                Ac_Year ,
                                Ac_Comp ,
                                Ac_Curr1 ,
                                Ac_Curr2
                         FROM   dbo.Com_AppComp WITH ( NOLOCK )
                         WHERE  Ac_Code = '" & gbApp & "'
                                AND Ac_Comp = ( SELECT  Loc_CoCode
                                                FROM    dbo.STK04_Location
                                                WHERE   Loc_Code = '" & LocCode & "'
                                              )
                         ORDER BY Ac_Year DESC; "

            Using dtCurr As New DataTable
                Using daCurr As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCurr.Fill(dtCurr)
                End Using

                gbCurr1 = dtCurr(0)("Ac_Curr1").ToString
                gbCurr2 = dtCurr(0)("Ac_Curr2").ToString
                gbComp = dtCurr(0)("Ac_Comp").ToString
                gbYear = dtCurr(0)("Ac_Year").ToString
                LocOrgCurr = gbCurr2

            End Using



            TempAllTransactionCC = "tmpWS_AllTransactionCC_" & UserCode
            pTempSale = "tmpWS_pTempSale_" & UserCode

            DropSqlObject(pTempSale, "TABLE")

            strsql = " SELECT  INVH_DATE ,"
            strsql &= " INVH_LOC ,INVH_RKS  ,"
            strsql &= " ISNULL( NULLIF(INVH_NETFC,0),INVH_NET) OrgNet,"
            strsql &= " ISNULL( NULLIF(INVH_CURRFC,''),INVH_CURR) OrgCurr,"
            strsql &= " dbo.convert5FC('POSO', '" & gbComp & "',  " & gbYear & ", INVH_NET, INVH_CURR, '" & LocOrgCurr & "',"
            strsql &= " INVH_RATE,CONVERT(DATETIME, INVH_DATE)   ) invh_net,  INVH_CURR ,"
            strsql &= " INVH_RATE ,ISNULL( NULLIF(INVH_RATEFC,0),INVH_RATE) RateUSed , "
            strsql &= " INVH_DOCTYP1 ,"
            strsql &= " INVH_DOCPREFIX ,"
            strsql &= " INVH_SERNO ,"
            strsql &= " INVH_RECID ,"
            strsql &= " INVH_CLIENTSUP"
            strsql &= " INTO " & pTempSale & ""
            strsql &= " FROM    " & LinkServerAndLinkDB & "POS_SALEINVHEAD"
            strsql &= " inner join " & LinkServerAndLinkDB & "Stk04_Clients  on  INVH_CLIENTSUP = CL_RecID "

            strsql &= "  where CL_RecID = '" & ClientRecID & "' "

            strsql &= " and INVH_DOCTYP1 IN( 'IN' ,'CN','CV','RC','DV')"
            Execute_WOXACT(strsql)

            DropSqlObject(TempAllTransactionCC, "Table")
            strsql = " SELECT INVH_CLIENTSUP,INVH_RKS, INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,"
            strsql &= " OrgNet ,OrgCurr ,"
            strsql &= " invh_net"
            strsql &= " , SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  * CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' THEN"
            strsql &= " dbo.convert5FC('POSO', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Paid ,"
            strsql &= " "
            strsql &= " invh_net - SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  *  CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' THEN"
            strsql &= " dbo.convert5FC('POSO', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Remain ,"

            strsql &= " invh_net - SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  *  CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' AND CT_SETTLTYPE <> 'APY' THEN"
            strsql &= " dbo.convert5FC('POSO', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Bal"

            strsql &= " INTO " & TempAllTransactionCC & " FROM " & pTempSale & ""
            strsql &= " INNER JOIN " & LinkServerAndLinkDB & "POS_CASHTRANSACTION ON CT_LOCATION =  invh_loc"
            strsql &= " AND CT_INVHEADER = INVH_RECID AND INVH_DOCTYP1 IN( 'IN' ,'CN')"
            strsql &= " GROUP BY INVH_CLIENTSUP,  INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,OrgNet ,OrgCurr, invh_net,INVH_RKS"
            strsql &= " UNION all"
            strsql &= " SELECT INVH_CLIENTSUP,INVH_RKS, INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,"
            strsql &= " OrgNet ,OrgCurr ,"
            strsql &= " invh_net"
            strsql &= " , 0 Paid ,"
            strsql &= " "
            strsql &= " CASE WHEN INVH_DOCTYP1 in ('CV','DV') then 1 else -1 end  *  invh_net  Remain,"
            strsql &= " CASE WHEN INVH_DOCTYP1 in ('CV','DV') then 1 else -1 end  *  invh_net  Bal"
            strsql &= " FROM " & pTempSale & ""
            strsql &= " INNER JOIN " & LinkServerAndLinkDB & "POS_CASHTRANSACTION ON CT_LOCATION =  invh_loc"
            strsql &= " AND CT_INVHEADER = INVH_RECID AND INVH_DOCTYP1 IN( 'RC' ,'CV' , 'DV' )"
            strsql &= " GROUP BY INVH_CLIENTSUP,  INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,OrgNet ,OrgCurr, invh_net,INVH_RKS"
            strsql &= " UNION all"

            strsql &= " SELECT CL_RecID,'BBF', CL_BBFDate ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL,NULL,NULL,"
            strsql &= "  dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleCC,0) - isnull(CL_BBFCreditCC,0), '" & gbCurr2 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= "  + dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleLC,0) - isnull(CL_BBFCreditLC,0), '" & gbCurr1 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate)),"

            strsql &= "  dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleCC,0) - isnull(CL_BBFCreditCC,0), '" & gbCurr2 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= "  + dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleLC,0) - isnull(CL_BBFCreditLC,0), '" & gbCurr1 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= " FROM   dbo.Stk04_Clients"
            strsql &= "  where CL_RecID = '" & ClientRecID & "' "

            strsql &= "  and  CL_BBFDate is not NULL "
            Execute_WOXACT(strsql)

            strsql = " SELECT  CONVERT(VARCHAR(10), INVH_DATE,103) [transactionDate] ,"
            strsql &= " INVH_LOC [transactionLocationCode] ,"
            strsql &= " Loc_Desc [transactionLocationName] ,"
            strsql &= " CONCAT(INVH_DOCTYP1, ' ', INVH_DOCPREFIX, ' ', convert(varchar, INVH_SERNO)) [transactionNumber] ,"

            strsql &= " INVH_RKS  [transactionRemarks],"
            strsql &= " OrgNet [transactionOriginalNet] ,"
            strsql &= " OrgCurr [transactionCurrency] ,"

            strsql &= " (dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ",( invh_net ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
            strsql &= " CONVERT(DATE , GETDATE())))"
            strsql &= "  [transactionNet],"
            strsql &= "   "
            strsql &= " (dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ",( Paid ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
            strsql &= " CONVERT(DATE , GETDATE())))"
            strsql &= " [transactionPaid],"
            strsql &= "   "
            strsql &= " (dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ",( Remain ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
            strsql &= " CONVERT(DATE , GETDATE())))"
            strsql &= " [transactionRemain],"
            strsql &= " SUM(dbo.convert5('POSO', '" & gbComp & "',  " & gbYear & ",( Bal ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
            strsql &= " CONVERT(DATE , GETDATE()))) OVER ( ORDER BY INVH_DATE , INVH_LOC , INVH_DOCTYP1 , INVH_DOCPREFIX , INVH_SERNO ) transactionBalance"

            strsql &= " FROM    " & TempAllTransactionCC & ""
            strsql &= " LEFT JOIN dbo.STK04_Location ON Loc_Code = INVH_LOC "
            strsql &= " LEFT JOIN pos_stocktransaction ON INVH_LOC = STT_LOCATION AND INVH_DOCTYP1 = STT_DOCTYPE1 AND INVH_DOCPREFIX = STT_PREFIX"
            strsql &= " ORDER BY INVH_DATE ,"
            strsql &= " INVH_LOC ,"
            strsql &= " INVH_DOCTYP1 ,"
            strsql &= " INVH_DOCPREFIX ,"
            strsql &= " INVH_SERNO"

            Return strsql

        Catch ex As Exception
            Throw ex
        End Try
    End Function

#End Region

#Region "Potential Client"

    Public Function SavePotentialClient(clientName As String, business As String, contact As String, phoneMobile As String, address As String, remarks As String, latitude As Double, longitude As Double, accuracy As Double, radiusInMeters As Double, userCode As String) As String Implements IDataService.SavePotentialClient

        Dim strsql As String, RecID As Double
        Try

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        strsql = " SELECT ISNULL(MAX(PC_RecID),0) + 1 FROM tbl_CPP_PotentialClients WITH (TABLOCKX) WHERE PC_LocCode = '" & LocCode & "' "
                        cmsql.CommandText = strsql
                        RecID = CDbl(cmsql.ExecuteScalar)

                        strsql = "  INSERT INTO dbo.tbl_CPP_PotentialClients"
                        strsql &= " VALUES  ( "
                        strsql &= " '" & LocCode & "' , " '-- PC_LocCode - varchar(2)
                        strsql &= " " & RecID & " , " '-- PC_RecID - numeric(18,0)
                        strsql &= " N'" & Left(clientName, 100).Trim.Replace("'", "''") & "' , " '-- PC_ClientName - nvarchar(100)

                        If business.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(business, 100).Trim.Replace("'", "''") & "' , " '-- PC_Business - nvarchar(100)
                        End If

                        If contact.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(contact, 100).Trim.Replace("'", "''") & "' , " '-- PC_Contact - nvarchar(100)
                        End If

                        If phoneMobile.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(phoneMobile, 100).Trim.Replace("'", "''") & "' , " '-- PC_PhoneMobile - nvarchar(100)
                        End If

                        If address.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(address, 500).Trim.Replace("'", "''") & "' , " '-- PC_Address - nvarchar(100)
                        End If

                        If remarks.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(remarks, 500).Trim.Replace("'", "''") & "' , " '-- PC_Remarks - nvarchar(100)
                        End If

                        strsql &= $" geography::Point({latitude}, {longitude}, 4326) , " '-- PC_Coordinates - geography
                        strsql &= $" {accuracy} , " '-- PC_Accurency - numeric
                        strsql &= $" {radiusInMeters} , " '-- PC_RaduisInMeters - numeric

                        strsql &= " N'" & userCode & "' , " '-- PC_UserStamp - varchar(4)
                        strsql &= " GetDate() " '-- PC_DateStamp - datetime
                        strsql &= " )"

                        cmsql.CommandText = strsql
                        cmsql.ExecuteNonQuery()

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try

    End Function

#End Region

#Region "Stock Items"

    'searchFor = RO : the product groups of the items a Receiving Order can receive (V_CPP_ROItems, the privileges of userCode)
    Public Function GetItemProductGroups(searchFor As String, userCode As String) As String Implements IDataService.GetItemProductGroups

        Dim strsql As String

        Try

            If If(searchFor, "").Trim.ToUpper = "RO" Then
                Return ROJson($" SELECT DISTINCT I.itemProductGroup [itemProductGroupCode] , I.itemProductGroupDesc
                                 FROM   V_CPP_ROItems I
                                 WHERE  I.itemProductGroup <> '' {ROItemAccess(userCode, "I")}
                                 ORDER BY I.itemProductGroupDesc ", ROUserParams(userCode))
            End If

            strsql = " SELECT itemProductGroupCode ,
                              itemProductGroupDesc
                       FROM   V_CPP_ItemProductGroups "

            Using dtItemProductGroups As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtItemProductGroups)
                End Using

                If dtItemProductGroups.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtItemProductGroups)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'searchFor = RO : the items a Receiving Order can receive (FrmDO conditions, V_CPP_ROItems) with the privileges of userCode,
    'the supplier references of supplierRecID are searched too. Same columns as V_CPP_Items (the price is the average cost)
    Public Function GetItems(itemProductGroup As String, itemPromotionCriteria As String, searchString As String,
                             searchFor As String, userCode As String, supplierRecID As String) As String Implements IDataService.GetItems

        Dim strsql As String

        Try

            If If(searchFor, "").Trim.ToUpper = "RO" Then Return GetROItems(itemProductGroup, searchString, userCode, supplierRecID)

            strsql = $" SELECT itemRecID ,
                               itemReference ,
                               itemBarcode ,
                               itemDescription ,
                               itemVatType ,
                               itemVatPerc ,
                               itemProductGroup ,
                               itemUnit ,
                               itemPrice ,
                               itemCurrency ,
                               itemAvailableQty ,
                               itemPromotionCriteria ,
                               itemSearchString
                        FROM   V_CPP_Items
                        WHERE 1=1 "

            Using daSql As New SqlDataAdapter(" ", DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                'The values are parameters, never part of the SQL text
                Dim parameters As SqlParameterCollection = daSql.SelectCommand.Parameters

                If Not String.IsNullOrEmpty(itemProductGroup) Then
                    If Not itemProductGroup.Trim.ToUpper = "ALL".ToUpper Then
                        strsql &= " AND  itemProductGroup = @ProductGroup "
                        parameters.Add("@ProductGroup", SqlDbType.NVarChar, 50).Value = itemProductGroup.Trim
                    End If
                End If

                If Not String.IsNullOrEmpty(itemPromotionCriteria) Then
                    strsql &= " AND  itemPromotionCriteria = @PromotionCriteria "
                    parameters.Add("@PromotionCriteria", SqlDbType.NVarChar, 50).Value = itemPromotionCriteria.Trim
                End If

                'Any part of the reference, the barcode or the description (the line breaks of a scan are removed,
                'the LIKE wildcards typed are searched as text)
                'The QR of a label (link ending with the record id, see LabelQRLink) : the item itself
                Dim search As String = If(searchString, "").Replace(vbCr, "").Replace(vbLf, "").Trim
                Dim linkRecID As String = ItemRecIDFromLink(search)
                If linkRecID <> "" Then
                    strsql &= " AND itemRecID = @LinkRecID "
                    parameters.Add("@LinkRecID", SqlDbType.Decimal).Value = CDec(linkRecID)
                ElseIf search <> "" Then
                    strsql &= " AND ( itemReference LIKE @SearchLike
                                      OR itemBarcode LIKE @SearchLike
                                      OR itemDescription LIKE @SearchLike ) "
                    parameters.Add("@SearchLike", SqlDbType.NVarChar, 210).Value = "%" & search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%"
                End If

                daSql.SelectCommand.CommandText = strsql
                daSql.SelectCommand.CommandTimeout = 600

                Using dtItems As New DataTable

                    daSql.Fill(dtItems)

                    If dtItems.Rows.Count = 0 Then
                        Return ""
                    Else
                        Return GetJson(dtItems)
                    End If

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetItemSpecs(itemRecID As String) As String Implements IDataService.GetItemSpecs

        Dim strsql As String

        Try

            strsql = " SELECT * 
                       FROM   V_CPP_ItemSpecs 
                       WHERE  itemRecID = " & itemRecID

            Using dtItemSpecs As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtItemSpecs)
                End Using

                Using dtSpecs As New DataTable

                    dtSpecs.Columns.Add("fieldTitle")
                    dtSpecs.Columns.Add("fieldData")

                    For Each col As DataColumn In dtItemSpecs.Columns

                        If col.ColumnName.Trim.ToUpper <> "itemRecID".ToUpper Then

                            If dtItemSpecs(0)(col).ToString.Trim <> "" Then
                                dtSpecs.Rows.Add(col.ColumnName, dtItemSpecs(0)(col).ToString)
                            End If

                        End If

                    Next

                    If dtSpecs.Rows.Count = 0 Then
                        Return ""
                    Else
                        Return GetJson(dtSpecs)
                    End If

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetItemImage(itemRecID As String) As String Implements IDataService.GetItemImage
        Dim strsql As String
        Dim arrPicture() As Byte
        Try

            strsql = " SELECT itemImage 
                       FROM   V_CPP_ItemImages 
                       WHERE  itemRecID = " & itemRecID.Trim.Replace("'", "''") & " "

            Using dtItemImage As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtItemImage)
                End Using

                If dtItemImage.Rows.Count = 0 Then
                    Return ""
                Else

                    Try

                        arrPicture = CType(dtItemImage.Rows(0)("itemImage"), Byte()).ToArray()
                        Return Convert.ToBase64String(arrPicture)

                    Catch ex As Exception
                        Return ""
                    End Try

                End If

            End Using

        Catch ex As Exception
            Return ""
        Finally
            arrPicture = Nothing
        End Try
    End Function

    Public Function GetItemPriceDiscount(itemRecID As String, clientRecID As String, currency As String, qty As Double) As String Implements IDataService.GetItemPriceDiscount

        Dim strsql As String
        Try

            strsql = $" EXEC dbo.SP_CPP_GetItemPriceAndDiscount '{LocCode}', {clientRecID}, {itemRecID}, '{currency}' , {qty} "

            Using dtItemPriceDiscount As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtItemPriceDiscount)
                End Using

                If dtItemPriceDiscount.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtItemPriceDiscount)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetItemVatValue(amount As String, itemVatType As String, itemVatPerc As String, clientPriceCat As String) As String Implements IDataService.GetItemVatValue

        Dim strsql As String
        Try

            strsql = $" SELECT dbo.F_CPP_VatValue ({amount} , '{LocCode}', '{itemVatType}', {itemVatPerc}, '{clientPriceCat}') itemVatValue "

            Using dtItemVatValue As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtItemVatValue)
                End Using

                If dtItemVatValue.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtItemVatValue)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Missing Items"

    Public Function SaveMissingItem(description As String, brand As String, requestedBy As String, userCode As String) As String Implements IDataService.SaveMissingItem

        Dim strsql As String, RecID As Double
        Try

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        strsql = " SELECT ISNULL(MAX(MI_RecID),0) + 1 FROM tbl_CPP_MissingItems WITH (TABLOCKX) WHERE MI_LocCode = '" & LocCode & "' "
                        cmsql.CommandText = strsql
                        RecID = CDbl(cmsql.ExecuteScalar)

                        strsql = "  INSERT INTO dbo.tbl_CPP_MissingItems"
                        strsql &= " VALUES  ( "
                        strsql &= " '" & LocCode & "' , " '-- MI_LocCode - varchar(2)
                        strsql &= " " & RecID & " , " '-- MI_RecID - numeric(18,0)
                        strsql &= " N'" & Left(description, 500).Trim.Replace("'", "''") & "' , " '-- MI_Description - nvarchar(500)

                        If brand.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(brand, 100).Trim.Replace("'", "''") & "' , " '-- MI_Brand - nvarchar(100)
                        End If

                        If requestedBy.Trim = "" Then
                            strsql &= " NULL , "
                        Else
                            strsql &= " N'" & Left(requestedBy, 500).Trim.Replace("'", "''") & "' , " '-- MI_RequestedBy - nvarchar(500)
                        End If

                        strsql &= " N'" & userCode & "' , " '-- MI_UserStamp - varchar(4)
                        strsql &= " GetDate() " '-- MI_DateStamp - datetime
                        strsql &= " )"

                        cmsql.CommandText = strsql
                        cmsql.ExecuteNonQuery()

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try

    End Function

#End Region

#Region "Common"

    Public Function GetCurrencies() As String Implements IDataService.GetCurrencies
        Dim strsql, WorkingDay As String
        Try

            WorkingDay = CurrentLocationWorkingDay()

            strsql = $"DECLARE @OpCurrency AS VARCHAR(3);
                       DECLARE @DefaultCurrency AS VARCHAR(3);

                       SELECT  @OpCurrency = OP_OPCurr
                       FROM    dbo.STK04_OPParam WITH (NOLOCK)
                       WHERE   OP_Loc = '{LocCode}';

                       SELECT  @DefaultCurrency = CR_DefaultCurrency
                       FROM    dbo.pos_cashreg WITH (NOLOCK)
                       WHERE   CR_Loc = '{LocCode}' AND CR_DefaultCurrency IS NOT NULL 

                       SELECT TOP 1
                               Ac_Curr1 localCurrency ,
                               Ac_Curr2 controlCurrency ,
                               ISNULL(@OpCurrency, '') operatingCurrency ,
                               CASE @OpCurrency
                                 WHEN Ac_Curr1 THEN Ac_Curr1
                                 WHEN Ac_Curr2 THEN Ac_Curr2
                                 ELSE CASE WHEN ISNULL(@DefaultCurrency, '') = '' THEN Ac_Curr1
                                           ELSE ISNULL(@DefaultCurrency, '')
                                      END
                               END defaultCurrency ,
                               dbo.convert5('POSP',Ac_Comp,Ac_Year,CONVERT(NUMERIC(18,3),1),Ac_Curr2, Ac_Curr1, '{CDate(WorkingDay).ToString("MM/dd/yyyy")}') rateLCCC
                       FROM    dbo.Com_AppComp WITH (NOLOCK)
                       WHERE   Ac_Code = 'POSO'; "

            Using dtCurrencies As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtCurrencies)
                End Using

                If dtCurrencies.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtCurrencies)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try
    End Function

    Public Function GetVatType(clientPriceCat As String) As String Implements IDataService.GetVatType

        Dim strsql As String
        Try

            strsql = $" SELECT dbo.F_CPP_VatType ('{LocCode}', '{clientPriceCat}') vatType "

            Using dtVatType As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtVatType)
                End Using

                If dtVatType.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtVatType)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Portable Orders"

    Public Function GetOrders(userCode As String, isReturn As Boolean) As String Implements IDataService.GetOrders

        Dim strsql As String

        Try

            strsql = $" SELECT  ORDH_Number orderNumber,
                                CONVERT(VARCHAR(10),ORDH_Date,103) orderDate,
                                clientCode orderClientCode,
                                clientName orderClientName,
                                ORDH_HeaderRemark orderHeaderRemark,
                                ORDH_FooterRemark orderFooterRemark,
                                ORDH_Currency orderCurrency,
                                ORDH_Total orderTotal,
                                ORDH_TotalVAT orderTotalVAT,
                                ISNULL(ORDH_OfferRecID, -1) orderOfferRecID,
                                ORDH_Discrepancy orderDiscrepancy
                        FROM    dbo.tbl_CPP_OrderHeader
                        INNER JOIN dbo.V_CPP_Clients ON clientRecID = ORDH_ClientRecID
                        WHERE   ORDH_LocCode = '{LocCode}'
                                AND ORDH_UserStamp = '{userCode}'
                                AND EXISTS ( SELECT TOP 1
                                                    1
                                             FROM   dbo.tbl_CPP_OrderDetails
                                             WHERE  ORDH_LocCode = ORDD_LocCode
                                                    AND ORDH_Number = ORDD_Number
                                                    AND ORDD_ItemQty {IIf(isReturn, "<", ">").ToString} 0 );"

            Using dtOrders As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtOrders)
                End Using

                If dtOrders.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtOrders)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetOrderDetails(orderNumber As String, isReturn As Boolean, userCode As String) As String Implements IDataService.GetOrderDetails

        Dim strsql As String

        Try

            strsql = $" SELECT  ISNULL(itemReference, '') lineItemReference,
                                ISNULL(itemDescription, '') lineItemDescription,
                                ABS(ORDD_ItemQty) lineItemQty,
                                ORDD_ItemPrice lineItemPrice,
                                ORDD_ItemDiscount lineItemDiscount,
                                ORDD_ItemTotal lineItemTotal,
                                ORDD_ItemVatValue lineItemVatValue,
                                ISNULL(ORDD_ItemRemark, '') lineItemRemark
                        FROM    dbo.tbl_CPP_OrderDetails
                                LEFT JOIN dbo.V_CPP_Items ON itemRecID = ORDD_ItemRecID
                        WHERE   ORDD_LocCode = '{LocCode}'
                                AND ORDD_Number = {orderNumber}
                                AND ORDD_ItemQty {IIf(isReturn, "<", ">").ToString} 0
                                AND EXISTS ( SELECT TOP 1
                                                    1
                                             FROM   dbo.tbl_CPP_OrderHeader
                                             WHERE  ORDH_LocCode = ORDD_LocCode
                                                    AND ORDH_Number = ORDD_Number
                                                    AND ORDH_UserStamp = '{userCode}' )
                        ORDER BY ORDD_RecID;"

            Using dtOrderDetails As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtOrderDetails)
                End Using

                If dtOrderDetails.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtOrderDetails)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function SaveOrder(isReturn As Boolean, clientRecID As String, orderVatType As String, orderDetails As Object, orderHeaderRemark As String, orderFooterRemark As String, orderCurrency As String, orderTotal As String, orderTotalVAT As String, orderDiscrepancy As Boolean, userCode As String) As String Implements IDataService.SaveOrder

        Dim CreatedOrders As New List(Of String)

        Try
            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using trnSql As SqlTransaction = cnsql.BeginTransaction()

                    Try
                        Using cmsql As New SqlCommand("", cnsql, trnSql)

                            cmsql.CommandTimeout = 900

                            Dim jss As New JavaScriptSerializer()
                            Dim model As Object = jss.Deserialize(Of Object)(CStr(orderDetails))

                            Dim orderGroups As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))()

                            For Each line In CType(model, Array)

                                Dim lineDic As Dictionary(Of String, Object) = CType(line, Dictionary(Of String, Object))
                                Dim issueLoc As String = LocCode

                                If isReturn Then

                                    cmsql.Parameters.Clear()
                                    cmsql.CommandText =
                                        " SELECT ISNULL(NULLIF(STK_LOCSRC, '00'), '') " &
                                        " FROM stk04_stkfil " &
                                        " WHERE STK_RECID = @ItemRecID "

                                    cmsql.Parameters.AddWithValue("@ItemRecID", lineDic("lineItemRecID"))

                                    Dim locResult As Object = cmsql.ExecuteScalar()

                                    If locResult IsNot Nothing AndAlso locResult IsNot DBNull.Value Then
                                        Dim tmpLoc As String = locResult.ToString().Trim()

                                        If tmpLoc <> "" AndAlso tmpLoc <> "00" Then
                                            issueLoc = tmpLoc
                                        End If
                                    End If

                                End If

                                If Not orderGroups.ContainsKey(issueLoc) Then
                                    orderGroups.Add(issueLoc, New List(Of Dictionary(Of String, Object))())
                                End If

                                orderGroups(issueLoc).Add(lineDic)

                            Next

                            For Each grp In orderGroups

                                Dim currentLocCode As String = grp.Key
                                Dim currentLines = grp.Value
                                Dim Number As Double

                                cmsql.Parameters.Clear()
                                cmsql.CommandText =
                                    " SELECT ISNULL(MAX(ORDH_Number), 0) + 1 " &
                                    " FROM tbl_CPP_OrderHeader WITH (TABLOCKX) " &
                                    " WHERE ORDH_LocCode = @LocCode "

                                cmsql.Parameters.AddWithValue("@LocCode", currentLocCode)

                                Number = CDbl(cmsql.ExecuteScalar())

                                Dim groupTotal As Decimal = 0
                                Dim groupTotalVAT As Decimal = 0

                                For Each lineDic In currentLines
                                    groupTotal += CDec(lineDic("lineItemTotal"))
                                    groupTotalVAT += CDec(lineDic("lineItemVatValue"))
                                Next

                                Dim saveDiscrepancy As Boolean = isReturn AndAlso orderDiscrepancy

                                cmsql.Parameters.Clear()
                                cmsql.CommandText =
                                    " INSERT INTO dbo.tbl_CPP_OrderHeader " &
                                    " VALUES ( " &
                                    " @LocCode, @Number, @Date, @ClientRecID, @VatType, " &
                                    " @HeaderRemark, @FooterRemark, @Currency, @Total, @TotalVAT, " &
                                    " NULL, @Discrepancy, @UserStamp, GETDATE() ) "

                                cmsql.Parameters.AddWithValue("@LocCode", currentLocCode)
                                cmsql.Parameters.AddWithValue("@Number", Number)
                                cmsql.Parameters.AddWithValue("@Date", Today)
                                cmsql.Parameters.AddWithValue("@ClientRecID", clientRecID)
                                cmsql.Parameters.AddWithValue("@VatType", orderVatType)
                                cmsql.Parameters.AddWithValue("@HeaderRemark", orderHeaderRemark)
                                cmsql.Parameters.AddWithValue("@FooterRemark", orderFooterRemark)
                                cmsql.Parameters.AddWithValue("@Currency", orderCurrency)
                                cmsql.Parameters.AddWithValue("@Total", groupTotal)
                                cmsql.Parameters.AddWithValue("@TotalVAT", groupTotalVAT)
                                cmsql.Parameters.AddWithValue("@Discrepancy", saveDiscrepancy)
                                cmsql.Parameters.AddWithValue("@UserStamp", userCode)

                                cmsql.ExecuteNonQuery()

                                For Each lineDic In currentLines

                                    cmsql.Parameters.Clear()
                                    cmsql.CommandText =
                                        " INSERT INTO dbo.tbl_CPP_OrderDetails " &
                                        " SELECT " &
                                        " (SELECT ISNULL(MAX(ORDD_RecID), 0) + 1 FROM tbl_CPP_OrderDetails), " &
                                        " @LocCode, @Number, @ItemRecID, @Qty, @Price, @Discount, @Total, @VatValue, @Remark "

                                    Dim qty As Decimal = CDec(lineDic("lineItemQty"))

                                    If isReturn Then
                                        qty = qty * -1
                                    End If

                                    cmsql.Parameters.AddWithValue("@LocCode", currentLocCode)
                                    cmsql.Parameters.AddWithValue("@Number", Number)
                                    cmsql.Parameters.AddWithValue("@ItemRecID", lineDic("lineItemRecID"))
                                    cmsql.Parameters.AddWithValue("@Qty", qty)
                                    cmsql.Parameters.AddWithValue("@Price", lineDic("lineItemPrice"))
                                    cmsql.Parameters.AddWithValue("@Discount", lineDic("lineItemDiscount"))
                                    cmsql.Parameters.AddWithValue("@Total", lineDic("lineItemTotal"))
                                    cmsql.Parameters.AddWithValue("@VatValue", lineDic("lineItemVatValue"))
                                    cmsql.Parameters.AddWithValue("@Remark", lineDic("lineItemRemark"))

                                    cmsql.ExecuteNonQuery()

                                Next

                                CreatedOrders.Add(currentLocCode & "-" & Number.ToString())

                            Next

                        End Using

                        trnSql.Commit()

                    Catch
                        trnSql.Rollback()
                        Throw
                    End Try

                End Using

            End Using

            Return "Success|" & String.Join(",", CreatedOrders)

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try

    End Function

#End Region

#Region "Prepare Sales Orders"

    Public Function GetSalesOrders() As String Implements IDataService.GetSalesOrders

        Dim strsql As String

        Try

            strsql = $" EXEC SP_CPP_SalesOrders '{LocCode}' "

            Using dtSalesOrders As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtSalesOrders)
                End Using

                If dtSalesOrders.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtSalesOrders)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetSalesOrderItems(headerRecID As String) As String Implements IDataService.GetSalesOrderItems

        Dim strsql As String

        Try

            strsql = $" EXEC SP_CPP_SalesOrderItems '{LocCode}' , '{headerRecID}' "

            Using dtSalesOrderItems As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtSalesOrderItems)
                End Using

                If dtSalesOrderItems.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtSalesOrderItems)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function GetSalesOrderDetailedQty(headerRecID As String, counter As String) As String Implements IDataService.GetSalesOrderDetailedQty

        Dim strsql As String

        Try

            strsql = $" SELECT  ROW_NUMBER() OVER ( ORDER BY ODPQ_RecID ) sopqRowNumber ,
                                ODPQ_RecID sopqRecID ,
                                {IIf(counter.Trim = "", " DOFR_ITMDESC sopqItemDescription , ", "")}
                                REPLACE(ODPQ_ExpiryDate,'00/','') sopqExpiryDate ,
                                ODPQ_LotNumber sopqBatchNumber ,
                                ODPQ_Qty sopqQty
                        FROM    dbo.STK04_OfferDetailsPreparedQty
                        {IIf(counter.Trim = "", " INNER JOIN dbo.STK04_OfferDetails ON ODPQ_Location = DOFR_LOC
                                                     AND ODPQ_HeaderRecID = DOFR_HeadRecId
                                                     AND ODPQ_Counter = DOFR_Counter ", "")}
                        WHERE   ODPQ_Location = '{LocCode}'
                                AND ODPQ_HeaderRecID = {headerRecID}
                                {IIf(counter.Trim = "", "", $" AND ODPQ_Counter = {counter} ")} 
                        ORDER BY ODPQ_RecID DESC"

            Using dtSalesOrderDetailedQty As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtSalesOrderDetailedQty)
                End Using

                If dtSalesOrderDetailedQty.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtSalesOrderDetailedQty)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Public Function SaveSalesOrderDetailedQty(headerRecID As String, counter As String, itemRecID As String, expiryLotString As String, qty As String, machine As String, userCode As String) As String Implements IDataService.SaveSalesOrderDetailedQty

        Dim strsql As String
        Dim ExpiriesLots As String()
        Try

            ExpiriesLots = expiryLotString.Split(CChar("|"))

            If ExpiriesLots.Length <> 3 Then
                Return "Invalid Scanned Item"
            End If

            If Not IsNumeric(ExpiriesLots(0)) Then
                Return "Invalid Item key"
            End If

            If itemRecID.Trim <> "" Then
                If ExpiriesLots(0) <> itemRecID Then
                    Return "The scanned QR belongs to another item"
                End If
            End If

            If (Len(ExpiriesLots(1)) < 10 OrElse Len(ExpiriesLots(1).Substring(ExpiriesLots(1).IndexOf("/") + 1)) <> 7 _
                            OrElse Len(ExpiriesLots(1).Substring(0, ExpiriesLots(1).IndexOf("/"))) <> 2 _
                            OrElse ExpiriesLots(1).Substring(6, 4) < "1800") OrElse (ExpiriesLots(1).Substring(3, 2) = "00") OrElse
                            Not IsDate(ExpiriesLots(1).Substring(3, 2) & "/" & ExpiriesLots(1).Substring(0, 2).Replace("00", "01") & "/" & ExpiriesLots(1).Substring(6, 4)) Then
                Return "Invalid Expiry Date"
            End If

            If ExpiriesLots(2).Trim = "" Then
                Return "Invalid Lot #"
            End If

            If Not RecordsExists($" SELECT TOP 1 1
                                    FROM dbo.STK04_STKQTY 
                                    WHERE STQ_loccode='00' 
                                      AND STQ_STKRECID='{ExpiriesLots(0).Trim.Replace("'", "''")}'
                                      AND (STQ_ExpiryDt IS Not null OR STQ_BatchInfo IS Not NULL) 
                                      AND STQ_expiryDt='{IIf(Len(ExpiriesLots(1)) = 7, "00.", "").ToString & ExpiriesLots(1).Trim.Replace("/", ".").Replace("'", "''")}' 
                                      AND STQ_BatchInfo ='{ExpiriesLots(2).Trim.Replace("'", "''")}' ") Then
                Return "Invalid Expiry / Lot #"
            End If

            If Not IsNumeric(qty) OrElse CDbl(qty) <= 0 OrElse CDbl(qty) > 99999999 Then
                Return "Invalid Qty"
            End If

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        If counter.Trim = "" AndAlso itemRecID.Trim = "" Then

                            strsql = $" EXEC SP_CPP_SalesOrderCheckItem '{LocCode}' , '{headerRecID}' , '{ExpiriesLots(0)}' "

                            Using dtCheckItem As New DataTable

                                Using daCheckItem As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                                    daCheckItem.SelectCommand.CommandTimeout = 300
                                    daCheckItem.Fill(dtCheckItem)
                                End Using

                                If dtCheckItem.Rows.Count = 0 Then
                                    Return "Invalid Scanned Item"
                                Else

                                    If dtCheckItem.Select(" sodQtyPending > 0 ").Count = 0 Then

                                        InsertSODetailedQty(cmsql,
                                                            headerRecID,
                                                            dtCheckItem.Select(" 1=1 ", "sodCounter DESC")(0)("sodCounter").ToString,
                                                            ExpiriesLots,
                                                            qty,
                                                            machine,
                                                            userCode)

                                    Else

                                        Dim TotalRemainingQty As Double
                                        TotalRemainingQty = CDbl(qty) '2

                                        Dim RowNumber As Integer
                                        RowNumber = dtCheckItem.Select("sodQtyPending > 0").Count

                                        For Each itm In dtCheckItem.Select("sodQtyPending > 0", "sodCounter")

                                            If TotalRemainingQty > 0 Then

                                                If RowNumber = 1 OrElse CDbl(itm("sodQtyPending")) >= TotalRemainingQty Then

                                                    InsertSODetailedQty(cmsql,
                                                                        headerRecID,
                                                                        itm("sodCounter").ToString,
                                                                        ExpiriesLots,
                                                                        TotalRemainingQty,
                                                                        machine,
                                                                        userCode)

                                                    TotalRemainingQty = 0

                                                Else

                                                    InsertSODetailedQty(cmsql,
                                                                        headerRecID,
                                                                        itm("sodCounter").ToString,
                                                                        ExpiriesLots,
                                                                        CDbl(itm("sodQtyPending")).ToString,
                                                                        machine,
                                                                        userCode)

                                                    TotalRemainingQty -= CDbl(itm("sodQtyPending"))

                                                End If

                                            End If

                                            RowNumber -= 1

                                        Next

                                    End If

                                End If

                            End Using

                        Else

                            InsertSODetailedQty(cmsql,
                                               headerRecID,
                                               counter,
                                               ExpiriesLots,
                                               qty,
                                               machine,
                                               userCode)

                        End If

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using


            If ExistItemWithNearestExpiry(ExpiriesLots(0), ExpiriesLots(1).Replace("/", ".")) Then
                Return "Success1"
            Else
                Return "Success"
            End If

        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    Private Sub InsertSODetailedQty(_cmsql As SqlCommand,
                                    _headerRecID As String,
                                    _counter As String,
                                    _ExpiriesLots As String(),
                                    _qty As String,
                                    _machine As String,
                                    _userCode As String)

        Dim strsql As String, RecID As Double
        Try

            strsql = " SELECT ISNULL(MAX(ODPQ_RecID),0) + 1 FROM STK04_OfferDetailsPreparedQty WITH (TABLOCKX) "
            _cmsql.CommandText = strsql
            RecID = CDbl(_cmsql.ExecuteScalar)

            strsql = $" INSERT INTO dbo.STK04_OfferDetailsPreparedQty
                                                VALUES  ( 
                                                {RecID} , /* ODPQ_RecID - numeric */
                                                '{LocCode}' , /* ODPQ_Location - varchar(2) */
                                                {_headerRecID} , /* ODPQ_HeaderRecID - numeric */
                                                {_counter} , /* ODPQ_Counter - numeric */
                                                {_ExpiriesLots(0)} , /* ODPQ_ItemRecID - numeric */
                                                '{_ExpiriesLots(1)}' , /* ODPQ_ExpiryDate - varchar(10) */
                                                '{_ExpiriesLots(2)}' , /* ODPQ_LotNumber - varchar(15) */
                                                {CDbl(_qty)} , /* ODPQ_Qty - numeric */
                                                N'{_machine}' , /* ODPQ_Machine - nvarchar(100) */
                                                '{_userCode}' , /* ODPQ_UserStamp - varchar(4) */
                                                GETDATE()  /* ODPQ_DateStamp - datetime */
                                                        ) "

            _cmsql.CommandText = strsql
            _cmsql.ExecuteNonQuery()

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function DeleteSalesOrderDetailedQty(recID As String, headerRecID As String, counter As String) As String Implements IDataService.DeleteSalesOrderDetailedQty

        Dim strsql As String
        Try

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 900

                    strsql = $" DELETE FROM dbo.STK04_OfferDetailsPreparedQty
                                WHERE ODPQ_RecID = {recID}
                                    AND ODPQ_Location = '{LocCode}'
                                    AND ODPQ_HeaderRecID = {headerRecID}
                                    AND ODPQ_Counter = {counter} "

                    cmsql.CommandText = strsql
                    cmsql.ExecuteNonQuery()

                End Using

            End Using

            Return "Success"

        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    Public Function OpenSalesOrder(headerRecID As String, machine As String, userCode As String) As String Implements IDataService.OpenSalesOrder
        Try

            Return CheckCreateLock(userCode, machine, "Portable POS", "stk04_offerHeader", "HOFR_HeadRecId = '" & headerRecID & "' AND HOFR_LOC = '" & LocCode & "'", "Sales Order")

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try
    End Function

    Public Function CloseSalesOrder(headerRecID As String, machine As String, userCode As String) As String Implements IDataService.CloseSalesOrder
        Try

            ReleaseLock(userCode, machine, "Portable POS", "stk04_offerHeader", "HOFR_HeadRecId = '" & headerRecID & "' AND HOFR_LOC = '" & LocCode & "'")

            Return "Success"

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try
    End Function

    Public Function SendSalesOrderToPOS(headerRecID As String, machine As String, userCode As String) As String Implements IDataService.SendSalesOrderToPOS
        Dim strsql As String
        Try

            strsql = $"IF NOT EXISTS ( SELECT TOP 1
                                               1 FROM STK04_PreparedOffers
                                       WHERE   PROF_Location = '{LocCode}'
                                               AND PROF_HeaderRecID = {headerRecID} )
                           BEGIN
                               INSERT  INTO STK04_PreparedOffers
                               VALUES  ( '{LocCode}', /* PROF_Location - varchar(2) */
                                         {headerRecID}, /* PROF_HeaderRecID - numeric */
                                         N'{machine.Replace("'", "''")}', /* PROF_Machine - nvarchar(100) */
                                         '{userCode}', /* PROF_UserStamp - varchar(4) */
                                         GETDATE()  /* PROF_DateStamp - datetime */
                                         );
                           END;
                       ELSE
                           BEGIN
                               UPDATE  STK04_PreparedOffers
                               SET     PROF_Machine = '{machine.Replace("'", "''")}' ,
                                       PROF_UserStamp = '{userCode}' ,
                                       PROF_DateStamp = GETDATE()
                               WHERE   PROF_Location = '{LocCode}'
                                       AND PROF_HeaderRecID = {headerRecID};
                           END;"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnsql.Open()

                Using cmsql As New SqlCommand(strsql, cnsql)
                    cmsql.CommandTimeout = 300
                    cmsql.ExecuteNonQuery()

                    strsql = $" INSERT INTO dbo.tbl_LabelsToPrint
                                ( lbl_OfferHeaderRecID, lbl_OfferLoc ) values ({headerRecID}, '{LocCode}')"

                    cmsql.CommandText = strsql
                    cmsql.ExecuteNonQuery()

                End Using

                cnsql.Close()
            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try
    End Function

    Private Function ExistItemWithNearestExpiry(_ItemRecID As String, _ExpiryDate As String) As Boolean
        Dim strsql As String
        Dim i As Integer
        Dim drsStockExpiry() As DataRow
        Try

            Dim ExpDate As String
            If Mid(_ExpiryDate, 1, 3) = "00." Then

                ExpDate = ExecuteScalar_WOXACT($"SELECT CASE WHEN LEFT('{_ExpiryDate}', 3) = '00.'
                                                                 THEN DATEADD(""d"", -1,
                                                                              DATEADD(""m"", 1,
                                                                                      SUBSTRING('{_ExpiryDate}', 4, 2) + '.'
                                                                                      + '01.' + SUBSTRING('{_ExpiryDate}', 7, 4)))
                                                                 ELSE CONVERT(DATETIME, '{_ExpiryDate}', 103)
                                                            END")
            Else
                ExpDate = ExecuteScalar_WOXACT($"SELECT CONVERT(DATETIME, '{_ExpiryDate}', 103)")
            End If

            Using dtExpiries As New DataTable

                Using daExp As New SqlDataAdapter($" SELECT  STQ_STKRECID stk_recid ,
                                                            CASE WHEN LEFT(Qty1.STQ_ExpiryDt, 3) = '00.'
                                                                 THEN DATEADD(""d"", -1,
                                                                              DATEADD(""m"", 1,
                                                                                      SUBSTRING(Qty1.STQ_ExpiryDt, 4, 2) + '.'
                                                                                      + '01.' + SUBSTRING(Qty1.STQ_ExpiryDt, 7, 4)))
                                                                 ELSE CONVERT(DATETIME, Qty1.STQ_ExpiryDt, 103)
                                                            END ExpiryDt ,
                                                            STQ_ExpiryDt dbar_expdate,
                                                            STQ_BatchInfo dbar_Batch
                                                    FROM    dbo.stk04_stkqty Qty1
                                                    WHERE   STQ_LOCCODE = '00'
                                                            AND STQ_STKRECID = {_ItemRecID}
                                                            AND CASE WHEN LEFT(Qty1.STQ_ExpiryDt, 3) = '00.'
                                                                 THEN DATEADD(""d"", -1,
                                                                              DATEADD(""m"", 1,
                                                                                      SUBSTRING(Qty1.STQ_ExpiryDt, 4, 2) + '.'
                                                                                      + '01.' + SUBSTRING(Qty1.STQ_ExpiryDt, 7, 4)))
                                                                 ELSE CONVERT(DATETIME, Qty1.STQ_ExpiryDt, 103)
                                                            END < '{ExpDate}' 
                                                            AND STQ_ExpiryDt <> '{_ExpiryDate}' ", DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daExp.Fill(dtExpiries)
                End Using

                drsStockExpiry = dtExpiries.Select("[stk_recid] = '" & _ItemRecID & "' ", "ExpiryDt")

                i = 0
                If drsStockExpiry.Length <> 0 Then
                    For i = 0 To drsStockExpiry.Length - 1
                        strsql = "SELECT isnull(AVAILABLEQty,0) Qty FROM View_StkAvailable POS_STKQTY "
                        strsql &= " WHERE STQ_STKRECID='" & _ItemRecID & "' "
                        strsql &= " AND STQ_ExpiryDt='" & drsStockExpiry(i)("dbar_expdate").ToString & "' "
                        strsql &= " AND STQ_BatchInfo='" & drsStockExpiry(i)("dbar_Batch").ToString & "'"
                        strsql &= " AND STQ_LOCCODE = '" & LocCode & "'"
                        Using Dt_Expiry As New DataTable
                            Using Sub_Cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                                Sub_Cnsql.Open()
                                Using Da_Expiry As New SqlDataAdapter(strsql, Sub_Cnsql)
                                    Da_Expiry.Fill(Dt_Expiry)
                                End Using
                            End Using
                            If Dt_Expiry.Rows.Count <> 0 Then
                                Dim Dr_expiry As DataRow
                                Dr_expiry = Dt_Expiry(0)
                                With Dr_expiry

                                    If CDbl(.Item("Qty").ToString) > 0 Then
                                        Return True
                                    End If
                                End With
                            End If
                        End Using

                    Next
                End If

            End Using

            Return False

        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region

#Region "Mobile Collection"

    Private ClocHO As Boolean = True

    Private Function SaveClientReceipt(cashRegister As String, clientRecID As String, clientName As String, paidAmountLC As String, paidAmountCC As String, userCode As String, duesList As Object,
                                       invoiceRate As Double, cashLC As Double, cashCC As Double) As String Implements IDataService.SaveClientReceipt
        Dim ret As String = ""

        Dim cLC, cCC As Double

        Try

            cLC = 0
            cCC = 0


            If IsNumeric(paidAmountLC) AndAlso CDbl(paidAmountLC) > 0 Then

                If cashLC < paidAmountLC Then
                    cLC = cashLC

                    cCC = (paidAmountLC - cLC) / invoiceRate

                Else
                    cLC = paidAmountLC

                    cCC = 0

                End If

                ret &= SaveReceipt(cashRegister, clientRecID, clientName, paidAmountLC, "LC", userCode, duesList, invoiceRate, cLC, cCC)

                cashLC -= cLC
                cashCC -= cCC

            End If

            If IsNumeric(paidAmountCC) AndAlso CDbl(paidAmountCC) > 0 Then
                ret &= SaveReceipt(cashRegister, clientRecID, clientName, paidAmountCC, "CC", userCode, duesList, invoiceRate, cashLC, cashCC)
            End If

            Return ret

        Catch ex As Exception

        End Try
    End Function

    Public Sub InsertInvHead(ByVal WorkingDay As String, ByVal Copercurr As String,
                             gbApp As String, gbComp As String, gbYear As String, gbCurr1 As String, gbCurr2 As String,
                             ByVal cmSql As SqlCommand, ByVal InvHead As Double, ByVal DocTyp1 As String, ByVal DocPrefix As String, ByVal DocSerNo As String,
                        ByVal SaleTyp As String, ByVal EmpCode As String, ByVal CashReg As String, ByVal CashSession As String,
                        ByVal clcode As String, ByVal ClName As String, ByVal ClAddress As String, ByVal ClDelAdd As String,
                        ByVal sRemarks As String, ByVal PriceUsed As String, ByVal OpCurr As String, ByVal GrossTot As Double,
                        ByVal TotDisc As Double, ByVal AddDisc As Double, ByVal NetTot As Double, ByVal VatType As String,
                        ByVal VatFC As Double, ByVal VatLc As Double, ByVal IncentLC As String, ByVal IncentCC As String,
                        ByVal IncentPer As String, ByVal CurRate As Double, Optional ByVal tbCashTab As String = "", Optional ByVal Logging As Boolean = False,
                        Optional ByVal RptHead1 As String = "", Optional ByVal RptHead2 As String = "", Optional ByVal rptFoot As String = "", Optional _invFCRATE As Double = 0, Optional FromForm As String = "", Optional _InvhVatPer As String = "NULL",
                        Optional ByVal cciLoc As String = "", Optional ByVal cciRecordID As String = "", Optional VATRateLC As Double = 0,
                        Optional _PreReceipt As Boolean = False)



        Dim CashInv As String = "T"
        Dim strsql As String

        Try
            CashInv = "Y"

            If EmpCode = "" Then EmpCode = "NA"
            If Logging = True Then
                strsql = "Insert Into POS_SALEINVHEAD_LOG "
            ElseIf _PreReceipt Then
                strsql = "Insert Into POS_PreReceipt "
            Else
                strsql = "Insert Into POS_SaleInvHead "
            End If
            strsql &= " Values('" & LocCode & "'"
            strsql &= "," & InvHead
            strsql &= ",'" & DocTyp1 & "'"
            strsql &= ",'" & DocPrefix & "'"
            strsql &= "," & DocSerNo
            strsql &= ",'" & CType(WorkingDay, Date).ToString("MM/dd/yyyy") & "'"
            strsql &= ",'" & SaleTyp & "'"
            If EmpCode = "NULL" Then
                strsql &= ",Null"
            Else
                strsql &= ",'" & EmpCode & "'"
            End If
            strsql &= ",'" & CashReg & "'," & Val(CashSession)
            If clcode = "Null" Then
                strsql &= ",Null,"
            Else
                strsql &= "," & clcode & ","
            End If
            strsql &= " N'" & Replace(ClName, "'", "''") & "',N'" & Replace(ClAddress, "'", "''")
            strsql &= "',N'" & Replace(ClDelAdd, "'", "''")
            strsql &= "',N'" & Replace(sRemarks, "'", "''") & "'," & CStr(IIf(PriceUsed = "", "null", "'" & PriceUsed & "'"))

            '...Jad JE - Invoicing in FC Currency
            If Copercurr = "FC" Then
                strsql &= ",'" & gbCurr2 & "', "
                If OpCurr = gbCurr2 Then
                    strsql &= " " & GrossTot + TotDisc + AddDisc & "," & TotDisc & ""
                    strsql &= "," & AddDisc & "," & NetTot & ""
                    strsql &= ",'" & VatType & "'," & CStr(VatFC)
                    'We should convert the vat LC based on the invoice rate
                    'strsql &= "," & convert5(VatLc, OpCurr, gbCurr1, CDate(WorkingDay))
                    If VATRateLC <> 0 Then
                        strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                             OpCurr & "','" & gbCurr1 & "', '" & VATRateLC & "' ,'" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                    Else

                        If _invFCRATE <> 0 Then
                            strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear &
                                  "', dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                                  OpCurr & "','" & gbCurr2 & "', '" & _invFCRATE & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "') ,'" & gbCurr2 & "','" &
                                  gbCurr1 & "', '" & CurRate & "' ,'" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                        Else
                            'strsql &= "," & convert5(VatLc, OpCurr, gbCurr1, CDate(WorkingDay))
                        End If
                    End If

                Else

                    If _invFCRATE <> 0 Then
                        'client receipt (Convert Using Form Rate)
                        strsql &= " dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & GrossTot + TotDisc + AddDisc & ",'" & OpCurr & "', '" & gbCurr2 & "', '" & _invFCRATE & "' ,'" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  ,"
                        strsql &= " dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & TotDisc & ",'" & OpCurr & "', '" & gbCurr2 & "', '" & _invFCRATE & "'  ,'" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  ,"
                        strsql &= " dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & AddDisc & ",'" & OpCurr & "', '" & gbCurr2 & "', '" & _invFCRATE & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  ,"
                        strsql &= " dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & NetTot & ",'" & OpCurr & "', '" & gbCurr2 & "', '" & _invFCRATE & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                        strsql &= ",'" & VatType & "',"
                        strsql &= " dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatFC & ",'" & OpCurr & "','" & gbCurr2 & "', '" & _invFCRATE & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                        If OpCurr = gbCurr1 Then
                            strsql &= "," & VatLc
                        Else
                            If VATRateLC <> 0 Then
                                strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                              OpCurr & "','" & gbCurr1 & "', '" & VATRateLC & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                            Else
                                strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear &
                              "', dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                              OpCurr & "','" & gbCurr2 & "', '" & _invFCRATE & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "') ,'" & gbCurr2 & "','" &
                              gbCurr1 & "', '" & CurRate & "' ,'" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                            End If
                        End If

                    Else
                        'strsql &= convert5(GrossTot + TotDisc + AddDisc, OpCurr, gbCurr2, CDate(WorkingDay)) & ", "
                        'strsql &= convert5(TotDisc, OpCurr, gbCurr2, CDate(WorkingDay)) & ", "
                        'strsql &= convert5(AddDisc, OpCurr, gbCurr2, CDate(WorkingDay)) & ", "
                        'strsql &= convert5(NetTot, OpCurr, gbCurr2, CDate(WorkingDay)) & " "
                        'strsql &= ",'" & VatType & "',"

                        'strsql &= CStr(convert5(VatFC, OpCurr, gbCurr2, CDate(WorkingDay)))

                        'If OpCurr = gbCurr1 Then
                        '    strsql &= "," & VatLc
                        'Else
                        '    If VATRateLC <> 0 Then
                        '        strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                        '      OpCurr & "','" & gbCurr1 & "', '" & VATRateLC & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "')  "
                        '    Else
                        '        strsql &= "," & convert5(VatLc, OpCurr, gbCurr1, CDate(WorkingDay))
                        '    End If
                        'End If

                    End If

                End If
            Else
                strsql &= ",'" & OpCurr & "'," & GrossTot + TotDisc + AddDisc & "," & TotDisc & ""
                strsql &= "," & AddDisc & "," & NetTot & ""
                'VAT in LC must be rounded (with no decimals) issued by Mr. Karam on 18/09/2006
                strsql &= ",'" & VatType & "'," & CStr(VatFC)

                If OpCurr = gbCurr1 Then
                    strsql &= "," & VatLc
                Else
                    'strsql &= "," & convert5(VatLc, OpCurr, gbCurr1, CDate(WorkingDay))

                    If VATRateLC <> 0 Then
                        strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                                  OpCurr & "','" & gbCurr1 & "', '" & VATRateLC & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "') "
                    Else
                        If CurRate <> 0 Then
                            strsql &= " , dbo.convert5fc('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & VatLc & ",'" &
                                  OpCurr & "','" & gbCurr1 & "', '" & CurRate & "' , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "') "
                        Else
                            'strsql &= "," & convert5(VatLc, OpCurr, gbCurr1, CDate(WorkingDay))
                        End If
                    End If

                End If

            End If


            strsql &= "," & IncentLC
            strsql &= "," & IncentCC
            strsql &= "," & IncentPer
            strsql &= "," & CurRate
            strsql &= ",'N',NULL"
            strsql &= ",0"
            strsql &= ",'N'"
            If FromForm = "frmStkAdjust" Then
                strsql &= ",'N'"
            Else
                strsql &= ",'" & CashInv & "'"
            End If
            strsql &= " , getdate()"
            strsql &= " , '" & EmpCode & "',"

            '...Jad JE - Invoicing in FC Currency
            If Copercurr = "FC" Then
                strsql &= "'" & OpCurr & "'," & GrossTot + TotDisc + AddDisc & "," & TotDisc & ""
                strsql &= "," & AddDisc & "," & NetTot & ""
                strsql &= "," & CStr(VatFC)
                strsql &= ",'0','" & _invFCRATE & "'"
            Else
                If FromForm = "frmStkAdjust" OrElse FromForm = "FrmClReceiptAC" OrElse FromForm = "frmNormClientReceipt" OrElse FromForm = "frmCashOutIn" Then
                    strsql &= " NULL "
                Else
                    strsql &= " '' "
                End If
                strsql &= ",'0','0','0','0','0','0','0'"
            End If


            If Logging = False Then
                If FromForm = "frmStkAdjust" Then
                    strsql &= " ,NULL,NULL,NULL "
                Else
                    strsql &= " ,'" & RptHead1 & "','" & RptHead2 & "','" & rptFoot & "' "
                End If

            End If

            strsql &= ", NULL "
            If Logging = False Then

                strsql &= " ,  " & _InvhVatPer & " " 'Invh_VATPer 
            End If
            strsql &= "," & CStr(IIf(cciLoc Is Nothing OrElse cciLoc.Trim = "", "NULL", "'" & cciLoc & "'")) & " "
            strsql &= "," & CStr(IIf(cciRecordID Is Nothing OrElse cciRecordID.Trim = "", "NULL", "'" & cciRecordID & "'")) & " "

            Dim PerVal As Double
            If GrossTot + TotDisc + AddDisc = 0 Then
                PerVal = 0
            Else
                PerVal = CDbl(AddDisc) * 100 / CDbl(GrossTot + TotDisc + AddDisc)
                If PerVal > 100 Then PerVal = 100
            End If

            strsql &= ",'" & CStr(PerVal) & "' "
            strsql &= ",'V'"
            If VATRateLC <> 0 Then
                strsql &= " , '" & VATRateLC & "'"  'VAT rate
            Else
                strsql &= " , '" & CurRate & "'"  'VAT rate
            End If

            If _PreReceipt Then
                strsql &= " , 'P' , NULL " 'P : Pending
            End If

            strsql &= " ) "

            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Function ConvTime(WorkingDay As String, Optional add24 As Boolean = True) As String
        Dim HH As Integer
        Dim MM As Integer
        Dim min As String
        Dim hor As String

        Try
            HH = Now.Hour
            MM = Now.Minute

            If Len(LTrim(Str(MM))) < 2 Then
                min = "0" + LTrim(Str(MM))
            Else
                min = LTrim(Str(MM))
            End If

            If Len(LTrim(Str(HH))) < 2 Then
                hor = "0" + LTrim(Str(HH))
            Else
                hor = LTrim(Str(HH))
            End If

            If add24 AndAlso ClocHO = False AndAlso CType(WorkingDay, Date) <> CType(Now.ToString("dd/MM/yyyy"), Date) Then
                ConvTime = Trim(Str(HH + 24)) & ":" & min
            Else
                ConvTime = hor & ":" & min
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function SaveReceipt(cashRegister As String, clientRecID As String, clientName As String, paidAmount As String, currency As String, userCode As String, duesList As Object, invoiceRate As Double, cashLC As Double, cashCC As Double) As String
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim AddedRem As String
        Dim CTOpNbr As Double
        Dim RptHead1, rptHead2, rptFoot As String
        Dim WSerno, lblSerno1, MaxCash As Long
        Dim docType1, docPrefix, TxtItype, txttime As String

        Dim WorkingDay As String
        Dim Copercurr As String
        Dim LocVatPrice As String
        Dim PriceUsed As String

        Dim invh_rateFC As Double
        Dim PAmountLC, PAmountCC As Double
        Dim gbCurr1, gbCurr2, gbApp, gbComp, gbYear As String
        Try

            gbApp = "POSO"

            strsql = "  SELECT TOP 1
                                Ac_Year ,
                                Ac_Comp ,
                                Ac_Curr1 ,
                                Ac_Curr2
                         FROM   dbo.Com_AppComp WITH ( NOLOCK )
                         WHERE  Ac_Code = '" & gbApp & "'
                                AND Ac_Comp = ( SELECT  Loc_CoCode
                                                FROM    dbo.STK04_Location
                                                WHERE   Loc_Code = '" & LocCode & "'
                                              )
                         ORDER BY Ac_Year DESC; "

            Using dtCurr As New DataTable
                Using daCurr As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCurr.Fill(dtCurr)
                End Using

                gbCurr1 = dtCurr(0)("Ac_Curr1").ToString
                gbCurr2 = dtCurr(0)("Ac_Curr2").ToString
                gbComp = dtCurr(0)("Ac_Comp").ToString
                gbYear = dtCurr(0)("Ac_Year").ToString

            End Using

            If currency = "LC" Then
                currency = gbCurr1
            Else
                currency = gbCurr2
            End If

            WorkingDay = CurrentLocationWorkingDay()
            Copercurr = ExecuteScalar_WOXACT("SELECT OP_OPCurr FROM dbo.STK04_OPParam WHERE OP_Loc = '" & LocCode & "'")
            LocVatPrice = ExecuteScalar_WOXACT(" SELECT  CASE WHEN Loc_Type1 = 'P' THEN 'R'
                                                            ELSE Loc_Type1
                                                    END
                                                FROM    dbo.STK04_Location
                                                WHERE Loc_Code = '" & LocCode & "'")
            PriceUsed = ExecuteScalar_WOXACT("SELECT sl_prefprc FROM dbo.pos_splocation WHERE SL_Loc = '" & LocCode & "'")

            'invoiceRate = CDbl(ExecuteScalar_WOXACT($" SELECT  ISNULL(MAX(CASE App_Rate
            '                                                  WHEN 1 THEN DR_Rate1LC
            '                                                  WHEN 2 THEN DR_Rate2LC
            '                                                  ELSE DR_Rate3LC
            '                                                END), 0)
            '                                        FROM    dbo.Com_DailyRate
            '                                                INNER JOIN dbo.Com_Application ON 1 = 1
            '                                        WHERE   App_Code = 'POSO'
            '                                                AND DR_CurCode = '{gbCurr2}'
            '                                                AND DR_Date = '{CDate(WorkingDay).ToString("MM/dd/yyyy")}' "))

            If invoiceRate = 0 Then
                Return "Rate is not defined"
            End If

            invh_rateFC = invoiceRate

            docType1 = "RC"
            docPrefix = "M"


            AddedRem = ""

            AddedRem &= "Rate " & gbCurr1 & " / " & gbCurr2 & " : " & CDbl(invoiceRate).ToString("N0")


            strsql = " SELECT STT_PRINTED1, STT_PRINTED2, STT_FOOTER FROM pos_stocktransaction"
            strsql &= " WHERE STT_LOCATION = '" & LocCode & "' AND STT_DOCTYPE1 ='" & docType1 & "'"
            strsql &= " AND STT_PREFIX = '" & docPrefix & "' "

            Using dtRepHeader As New DataTable

                Using daRepHeader As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daRepHeader.Fill(dtRepHeader)
                End Using

                If dtRepHeader.Rows.Count <> 1 Then
                    Return $"Document Type {docType1}/{docPrefix} is not defined in Location {LocCode}"
                Else

                    RptHead1 = dtRepHeader(0)("STT_PRINTED1").ToString
                    rptHead2 = dtRepHeader(0)("STT_PRINTED2").ToString
                    rptFoot = dtRepHeader(0)("STT_FOOTER").ToString

                End If

            End Using

            cnsql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
            cnsql.Open()
            trnSql = cnsql.BeginTransaction

            cmsql = New SqlCommand(strsql, cnsql, trnSql)
            cmsql.CommandTimeout = 600

            cmsql.CommandText = $" SELECT isnull(max(INVH_RECID),0) + 1 FROM POS_SALEINVHEAD WHERE INVH_LOC = '{LocCode}' "
            WSerno = CLng(cmsql.ExecuteScalar)

            cmsql.CommandText = " SELECT isnull(nullif(STT_SERNO,0),1) FROM POS_stocktransaction WHERE STT_LOCATION = '" & LocCode & "' and STT_DOCTYPE1 = '" & docType1 & "' and STT_PREFIX = '" & docPrefix & "' "
            lblSerno1 = CLng(cmsql.ExecuteScalar)

            TxtItype = Trim(docType1) + Trim(docPrefix) + Str(lblSerno1)

            'ConvTime()

            Dim myvattype As String
            myvattype = "T" 'No Payment In advance

            If currency = gbCurr1 Then

                PAmountLC = CDbl(paidAmount)
                PAmountCC = CDbl(paidAmount) / invoiceRate

            Else

                PAmountLC = CDbl(paidAmount) * invoiceRate
                PAmountCC = CDbl(paidAmount)

            End If


            'Currency is always either gbCurr1 or gbCurr2

            Dim VATRate As Double = 0 'No Payment In advance

            InsertInvHead(WorkingDay, Copercurr, gbApp, gbComp, gbYear, gbCurr1, gbCurr2, cmsql, WSerno, docType1, docPrefix, CStr(lblSerno1), LocVatPrice,
                userCode, cashRegister, "0", clientRecID, clientName, "", "",
                "", PriceUsed, currency, CDbl(paidAmount), 0, 0, CDbl(paidAmount), myvattype,
                CDbl(0), CDbl(0), "0", "0", "0",
                invoiceRate,
                "", , RptHead1, rptHead2, rptFoot, invh_rateFC, "FrmClReceiptAC", , "", "", VATRate, False)

            txttime = ConvTime(WorkingDay)

            If cashLC > 0 Then

                cmsql.CommandText = " select Isnull(Max(CT_OPNBR),0)  from POS_CASHTRANSACTION WHERE CT_LOCATION ='" & LocCode & "' "
                MaxCash = CLng(cmsql.ExecuteScalar)

                TxtItype = docType1 + docPrefix + " " + CStr(lblSerno1)

                strsql = " SELECT ISNULL(MAX(CT_OPNBR),0) FROM POS_CASHTRANSACTION WITH (TABLOCKX) WHERE CT_LOCATION = '" & LocCode & "' "
                cmsql.CommandText = strsql
                CTOpNbr = CDbl(cmsql.ExecuteScalar)

                strsql = "INSERT INTO POS_CASHTRANSACTION
                        SELECT '" & LocCode & "', " & WSerno
                strsql &= ", '" & cashRegister & "', "
                strsql &= " " & CTOpNbr & " + 1 , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "', 'T', '"
                strsql &= txttime & "', '" & TxtItype & "', '+', '010', '" & gbCurr1 & "', "
                strsql &= " Abs(" & CDbl(cashLC) & "), "

                If currency = gbCurr1 Then
                    strsql &= "  Abs(" & CDbl(cashLC) & "),"
                ElseIf currency = gbCurr2 Then
                    strsql &= "  Abs(" & CDbl(cashLC / invoiceRate) & "),"
                End If

                strsql &= " NULL, NULL, "

                strsql &= " '', NULL, NULL, "
                strsql &= " NULL, NULL, NULL, NULL, '', '', NULL, "
                strsql &= "NULL , "

                strsql &= "getdate(), '" & userCode & "' "
                strsql &= " , NULL  "  'client account number
                'strsql &= " FROM " & tbCashTab

                strsql &= " ; "

                strsql &= " INSERT INTO POS_CashTransCVRate "
                strsql &= " SELECT '" & LocCode & "', " & WSerno
                strsql &= ", '" & cashRegister & "', "
                strsql &= " " & CTOpNbr & " + 1 , "

                strsql &= " isnull(" & CDbl(cashLC) & ",0) , "
                strsql &= " isnull(" & CDbl(IIf(currency = gbCurr1, 1, invoiceRate)) & ",0)  , "
                strsql &= " isnull(" & CDbl(cashLC / invoiceRate) & ",0) , "
                strsql &= " isnull(" & CDbl(IIf(currency = gbCurr2, 1, invoiceRate)) & ",0) "

                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()

            End If

            If cashCC > 0 Then

                cmsql.CommandText = " select Isnull(Max(CT_OPNBR),0)  from POS_CASHTRANSACTION WHERE CT_LOCATION ='" & LocCode & "' "
                MaxCash = CLng(cmsql.ExecuteScalar)

                TxtItype = docType1 + docPrefix + " " + CStr(lblSerno1)

                strsql = " SELECT ISNULL(MAX(CT_OPNBR),0) FROM POS_CASHTRANSACTION WITH (TABLOCKX) WHERE CT_LOCATION = '" & LocCode & "' "
                cmsql.CommandText = strsql
                CTOpNbr = CDbl(cmsql.ExecuteScalar)

                strsql = "INSERT INTO POS_CASHTRANSACTION
                        SELECT '" & LocCode & "', " & WSerno
                strsql &= ", '" & cashRegister & "', "
                strsql &= " " & CTOpNbr & " + 1 , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "', 'T', '"
                strsql &= txttime & "', '" & TxtItype & "', '+', '010', '" & gbCurr2 & "', "
                strsql &= " Abs(" & CDbl(cashCC) & "), "

                If currency = gbCurr1 Then
                    strsql &= "  Abs(" & CDbl(cashCC * invoiceRate) & "),"
                ElseIf currency = gbCurr2 Then
                    strsql &= "  Abs(" & CDbl(cashCC) & "),"
                End If

                strsql &= " NULL, NULL, "

                strsql &= " '', NULL, NULL, "
                strsql &= " NULL, NULL, NULL, NULL, '', '', NULL, "
                strsql &= "NULL , "

                strsql &= "getdate(), '" & userCode & "' "
                strsql &= " , NULL  "  'client account number
                'strsql &= " FROM " & tbCashTab

                strsql &= " ; "

                strsql &= " INSERT INTO POS_CashTransCVRate "
                strsql &= " SELECT '" & LocCode & "', " & WSerno
                strsql &= ", '" & cashRegister & "', "
                strsql &= " " & CTOpNbr & " + 1 , "

                strsql &= " isnull(" & CDbl(cashCC * invoiceRate) & ",0) , "
                strsql &= " isnull(" & CDbl(IIf(currency = gbCurr1, 1, invoiceRate)) & ",0)  , "
                strsql &= " isnull(" & CDbl(cashCC) & ",0) , "
                strsql &= " isnull(" & CDbl(IIf(currency = gbCurr2, 1, invoiceRate)) & ",0) "

                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()

            End If


            'If AllowShowingAccRem Then InsertAccRemark(cmsql, WSerno)

            'Update Invoice number
            strsql = " Update POS_StockTransaction set stt_serno = " & (Val(lblSerno1) + 1)
            strsql &= " Where STT_LOCATION = '" & LocCode & "'"
            strsql &= " and STT_DOCTYPE1 = '" & docType1 & "'"
            strsql &= " and STT_PREFIX = '" & docPrefix & "'"

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()


            If 1 = 1 Then 'always linked from the mobile

                Dim jss As New JavaScriptSerializer()
                Dim model As Object = jss.Deserialize(Of Object)(duesList)

                strsql = " DECLARE @tblPaidDues AS TABLE
                                    (
                                      invoiceLocation VARCHAR(2) ,
                                      invoiceHeadRecID NUMERIC(18, 0) ,
                                      dueCashRegister VARCHAR(2) ,
                                      dueOperationNumber NUMERIC(18, 0) ,
                                      dueCounter NUMERIC(18, 0) ,
                                      paidAmount NUMERIC(18, 3)
                                    );  "

                For Each line In CType(model, Array)

                    If CType(line, Dictionary(Of String, Object))("dueCurrency").ToString = currency Then

                        If IsNumeric(CType(line, Dictionary(Of String, Object))("amountToPay")) AndAlso CDbl(CType(line, Dictionary(Of String, Object))("amountToPay")) > 0 Then

                            strsql &= $"  INSERT  INTO @tblPaidDues
                                    VALUES  ( '{CType(line, Dictionary(Of String, Object))("locationCode").ToString}', 
                                              {CType(line, Dictionary(Of String, Object))("invoiceRecID")}, 
                                              '{CType(line, Dictionary(Of String, Object))("dueCashRegister").ToString}', 
                                              {CType(line, Dictionary(Of String, Object))("dueOperationNumber").ToString}, 
                                              {CType(line, Dictionary(Of String, Object))("dueCounter").ToString}, 
                                              {CDbl(CType(line, Dictionary(Of String, Object))("amountToPay").ToString)} 
                                              ); "

                        End If

                    End If

                Next

                strsql &= "  INSERT INTO POS_SALEINVLinkCNRC "
                strsql &= " SELECT invoiceLocation , invoiceHeadRecID , '" & LocCode & "' , " & WSerno & " , "
                strsql &= " (SELECT ISNULL(MAX(INVL_Counter),0)+1 from POS_SALEINVLinkCNRC where INVL_Loc = invoiceLocation AND INVL_RecId = invoiceHeadRecID) ,"
                strsql &= " sumPaidAmount FROM (SELECT  invoiceLocation ,
                                                            invoiceHeadRecID ,
                                                            SUM(paidAmount) sumPaidAmount
                                                    FROM    @tblPaidDues
                                                    GROUP BY invoiceLocation ,
                                                            invoiceHeadRecID) tblPaidDues ;"

                strsql &= " INSERT INTO POS_SALEINVLinkRCDue"
                strsql &= " SELECT invoiceLocation , invoiceHeadRecID , dueCashRegister , dueOperationNumber , dueCounter , '" & LocCode & "' , " & WSerno & " , paidAmount "
                strsql &= ",  ISNULL((SELECT MAX(lrc_LinkCounter)   FROM POS_SALEINVLinkRCDue"
                strsql &= " "
                strsql &= " WHERE lrc_CNRCLoc ='" & LocCode & "' AND lrc_CNRCHRecid = " & WSerno & ""
                strsql &= " AND lrc_InvLoc = invoiceLocation AND  lrc_InvHRecid=  invoiceHeadRecID"
                strsql &= " AND lrc_OpNum =  dueOperationNumber"
                strsql &= " AND lrc_DueCounter = dueCounter"
                strsql &= " ),0) + ROW_NUMBER() OVER (PARTITION BY invoiceLocation,invoiceHeadRecID,dueOperationNumber,dueCounter"
                strsql &= " ORDER BY  paidAmount				),"
                strsql &= " GETDATE()"
                strsql &= " FROM @tblPaidDues WHERE paidAmount <> 0"

                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()

            End If

            'strsql = $" INSERT INTO dbo.tbl_DocumentToPrint
            '            ( DTP_Type, DTP_RecID )
            '            VALUES  ( 'RC', 
            '                      {WSerno}  
            '                      ) "

            'cmsql.CommandText = strsql
            'cmsql.ExecuteNonQuery()

            trnSql.Commit() : trnSql = Nothing

            Return "Success|" & docType1 + docPrefix + " " + CStr(lblSerno1) & " "

        Catch ex As Exception
            Try
                trnSql.Rollback()
            Catch exRB As Exception
            End Try
            Return "Error : " & ex.Message
        Finally

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If

        End Try
    End Function

#End Region

#Region "Sales Invoice"

    Private Function SaveInvoice(cashRegister As String, isReturn As Boolean, clientRecID As String, orderVatType As String, orderDetails As Object, orderHeaderRemark As String, orderFooterRemark As String, orderCurrency As String, orderTotal As String, orderTotalVAT As String, userCode As String, invoiceRate As Double, paidAmount As Double, clientName As String) As String Implements IDataService.SaveInvoice
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim trnSql As SqlTransaction
        'Dim AddedRem As String
        Dim CTOpNbr As Double
        Dim RptHead1, rptHead2, rptFoot As String
        Dim WSerno, lblSerno1, MaxCash As Long
        Dim docType1, docPrefix, TxtItype, txttime As String

        Dim WorkingDay As String
        Dim Copercurr As String
        Dim LocVatPrice As String
        Dim PriceUsed As String

        Dim invh_rateFC As Double
        Dim PAmountLC, PAmountCC As Double
        Dim gbCurr1, gbCurr2, gbApp, gbComp, gbYear As String
        Try

            gbApp = "POSO"

            strsql = "  SELECT TOP 1
                                Ac_Year ,
                                Ac_Comp ,
                                Ac_Curr1 ,
                                Ac_Curr2
                         FROM   dbo.Com_AppComp WITH ( NOLOCK )
                         WHERE  Ac_Code = '" & gbApp & "'
                                AND Ac_Comp = ( SELECT  Loc_CoCode
                                                FROM    dbo.STK04_Location
                                                WHERE   Loc_Code = '" & LocCode & "'
                                              )
                         ORDER BY Ac_Year DESC; "

            Using dtCurr As New DataTable
                Using daCurr As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCurr.Fill(dtCurr)
                End Using

                gbCurr1 = dtCurr(0)("Ac_Curr1").ToString
                gbCurr2 = dtCurr(0)("Ac_Curr2").ToString
                gbComp = dtCurr(0)("Ac_Comp").ToString
                gbYear = dtCurr(0)("Ac_Year").ToString

            End Using

            'If Currency = "LC" Then
            '    Currency = gbCurr1
            'Else
            '    Currency = gbCurr2
            'End If

            WorkingDay = CurrentLocationWorkingDay()
            Copercurr = ExecuteScalar_WOXACT("SELECT OP_OPCurr FROM dbo.STK04_OPParam WHERE OP_Loc = '" & LocCode & "'")
            LocVatPrice = ExecuteScalar_WOXACT(" SELECT  CASE WHEN Loc_Type1 = 'P' THEN 'R'
                                                            ELSE Loc_Type1
                                                    END
                                                FROM    dbo.STK04_Location
                                                WHERE Loc_Code = '" & LocCode & "'")
            PriceUsed = ExecuteScalar_WOXACT("SELECT sl_prefprc FROM dbo.pos_splocation WHERE SL_Loc = '" & LocCode & "'")

            'invoiceRate = CDbl(ExecuteScalar_WOXACT($" SELECT  ISNULL(MAX(CASE App_Rate
            '                                                  WHEN 1 THEN DR_Rate1LC
            '                                                  WHEN 2 THEN DR_Rate2LC
            '                                                  ELSE DR_Rate3LC
            '                                                END), 0)
            '                                        FROM    dbo.Com_DailyRate
            '                                                INNER JOIN dbo.Com_Application ON 1 = 1
            '                                        WHERE   App_Code = 'POSO'
            '                                                AND DR_CurCode = '{gbCurr2}'
            '                                                AND DR_Date = '{CDate(WorkingDay).ToString("MM/dd/yyyy")}' "))

            If invoiceRate = 0 Then
                Return "Rate is not defined"
            End If

            invh_rateFC = invoiceRate

            docType1 = "IN"
            docPrefix = "M"

            strsql = " SELECT STT_PRINTED1, STT_PRINTED2, STT_FOOTER FROM pos_stocktransaction"
            strsql &= " WHERE STT_LOCATION = '" & LocCode & "' AND STT_DOCTYPE1 ='" & docType1 & "'"
            strsql &= " AND STT_PREFIX = '" & docPrefix & "' "

            Using dtRepHeader As New DataTable

                Using daRepHeader As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daRepHeader.Fill(dtRepHeader)
                End Using

                If dtRepHeader.Rows.Count <> 1 Then
                    Return $"Document Type {docType1}/{docPrefix} is not defined in Location {LocCode}"
                Else

                    RptHead1 = dtRepHeader(0)("STT_PRINTED1").ToString
                    rptHead2 = dtRepHeader(0)("STT_PRINTED2").ToString
                    rptFoot = dtRepHeader(0)("STT_FOOTER").ToString

                End If

            End Using

            cnsql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
            cnsql.Open()
            trnSql = cnsql.BeginTransaction

            cmsql = New SqlCommand(strsql, cnsql, trnSql)
            cmsql.CommandTimeout = 600

            cmsql.CommandText = $" SELECT isnull(max(INVH_RECID),0) + 1 FROM POS_SALEINVHEAD WITH (TABLOCKX) WHERE INVH_LOC = '{LocCode}' "
            WSerno = CLng(cmsql.ExecuteScalar)

            cmsql.CommandText = " SELECT isnull(nullif(STT_SERNO,0),1) FROM POS_stocktransaction WHERE STT_LOCATION = '" & LocCode & "' and STT_DOCTYPE1 = '" & docType1 & "' and STT_PREFIX = '" & docPrefix & "' "
            lblSerno1 = CLng(cmsql.ExecuteScalar)

            TxtItype = Trim(docType1) + Trim(docPrefix) + Str(lblSerno1)

            'ConvTime()

            Dim myvattype As String
            myvattype = "T" 'No Payment In advance

            If orderCurrency = gbCurr1 Then

                PAmountLC = CDbl(paidAmount)
                PAmountCC = CDbl(paidAmount) / invoiceRate

            Else

                PAmountLC = CDbl(paidAmount) * invoiceRate
                PAmountCC = CDbl(paidAmount)

            End If


            'Currency is always either gbCurr1 or gbCurr2

            Dim VATRate As Double = 0 'No Payment In advance

            InsertInvHead(WorkingDay, Copercurr, gbApp, gbComp, gbYear, gbCurr1, gbCurr2, cmsql, WSerno, docType1, docPrefix, CStr(lblSerno1), LocVatPrice,
                userCode, cashRegister, "0", clientRecID, clientName, "", "",
                "", PriceUsed, orderCurrency, CDbl(paidAmount), 0, 0, CDbl(paidAmount), myvattype,
                CDbl(0), CDbl(0), "0", "0", "0",
                invoiceRate,
                "", , RptHead1, rptHead2, rptFoot, invh_rateFC, "FrmClReceiptAC", , "", "", VATRate, False)

            txttime = ConvTime(WorkingDay)



            cmsql.CommandText = " select Isnull(Max(CT_OPNBR),0)  from POS_CASHTRANSACTION WHERE CT_LOCATION ='" & LocCode & "' "
            MaxCash = CLng(cmsql.ExecuteScalar)

            TxtItype = docType1 + docPrefix + " " + CStr(lblSerno1)

            strsql = " SELECT ISNULL(MAX(CT_OPNBR),0) FROM POS_CASHTRANSACTION WITH (TABLOCKX) WHERE CT_LOCATION = '" & LocCode & "' "
            cmsql.CommandText = strsql
            CTOpNbr = CDbl(cmsql.ExecuteScalar)

            strsql = "INSERT INTO POS_CASHTRANSACTION
                        SELECT '" & LocCode & "', " & WSerno
            strsql &= ", '" & cashRegister & "', "
            strsql &= " " & CTOpNbr & " + 1 , '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "', 'T', '"
            strsql &= txttime & "', '" & TxtItype & "', '+', '010', '" & gbCurr1 & "', "
            strsql &= " Abs(" & CDbl(paidAmount) & "), "
            strsql &= "  Abs(" & CDbl(paidAmount) & "),"

            strsql &= " NULL, NULL, "

            strsql &= " '', NULL, NULL, "
            strsql &= " NULL, NULL, NULL, NULL, '', '', NULL, "
            strsql &= "NULL , "

            strsql &= "getdate(), '" & userCode & "' "
            strsql &= " , NULL  "  'client account number
            'strsql &= " FROM " & tbCashTab

            strsql &= " ; "

            strsql &= " INSERT INTO POS_CashTransCVRate "
            strsql &= " SELECT '" & LocCode & "', " & WSerno
            strsql &= ", '" & cashRegister & "', "
            strsql &= " " & CTOpNbr & " + 1 , "

            strsql &= " isnull(" & CDbl(paidAmount) & ",0) , "
            strsql &= " isnull(" & CDbl(IIf(orderCurrency = gbCurr1, 1, invoiceRate)) & ",0)  , "
            strsql &= " isnull(" & CDbl(paidAmount / invoiceRate) & ",0) , "
            strsql &= " isnull(" & CDbl(IIf(orderCurrency = gbCurr2, 1, invoiceRate)) & ",0) "

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()




            'If AllowShowingAccRem Then InsertAccRemark(cmsql, WSerno)

            'Update Invoice number
            strsql = " Update POS_StockTransaction set stt_serno = " & (Val(lblSerno1) + 1)
            strsql &= " Where STT_LOCATION = '" & LocCode & "'"
            strsql &= " and STT_DOCTYPE1 = '" & docType1 & "'"
            strsql &= " and STT_PREFIX = '" & docPrefix & "'"

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()


            'strsql = $" INSERT INTO dbo.tbl_DocumentToPrint
            '            ( DTP_Type, DTP_RecID )
            '            VALUES  ( 'RC', 
            '                      {WSerno}  
            '                      ) "

            'cmsql.CommandText = strsql
            'cmsql.ExecuteNonQuery()

            trnSql.Commit() : trnSql = Nothing

            Return "Success|" & docType1 + docPrefix + " " + CStr(lblSerno1) & " "

        Catch ex As Exception
            Try
                trnSql.Rollback()
            Catch exRB As Exception
            End Try
            Return "Error : " & ex.Message
        Finally

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If

        End Try
    End Function

#End Region

#Region "Cash Count"

    Public Function SaveCashCount(userCode As String, deviceName As String, cashRegister As String, amountLC As Double, amountCC As Double) As String Implements IDataService.SaveCashCount

        Dim strsql As String
        Dim CashCount As Int64
        Dim txttime As String
        Dim operCurr As String
        Dim MaxCashCount As String
        Dim LocalCurrency, ControlCurrency As String
        Dim strControlMsg As String
        Dim WorkingDay As String
        Try

            WorkingDay = CurrentLocationWorkingDay()

            LocalCurrency = ExecuteScalar_WOXACT(" SELECT TOP 1 Ac_Curr1 FROM dbo.Com_AppComp WITH(NOLOCK) WHERE Ac_Code = 'POSO' ")
            ControlCurrency = ExecuteScalar_WOXACT(" SELECT TOP 1 Ac_Curr2 FROM dbo.Com_AppComp WITH(NOLOCK) WHERE Ac_Code = 'POSO' ")

            strControlMsg = BeforeCashCount(userCode, deviceName, cashRegister, WorkingDay, LocalCurrency)

            If Not String.IsNullOrEmpty(strControlMsg) Then
                Return strControlMsg
            End If

            MaxCashCount = GetFieldDesc("pos_cashreg", "cr_loc='" & LocCode & "' AND cr_code='" & cashRegister & "'", "CR_NoCshCnt")

            operCurr = ExecuteScalar_WOXACT($" SELECT OP_OPCurr FROM dbo.STK04_OPParam WHERE OP_Loc = '{LocCode}' ")
            CashCount = CLng(GetFieldDesc("pos_cashcounted", "ccnt_date = '" & CType(WorkingDay, Date).ToString("MM/dd/yyyy") & "' and ccnt_loc = '" & LocCode & "' and ccnt_settype = '010' and ccnt_curr = '" & operCurr & "' and ccnt_crcode = '" & cashRegister & "'", "count(*)"))

            If CashCount = CDbl(MaxCashCount) Then
                Return "You Have Already Defined The Cash Counted No Process"
            End If

            Dim r As Integer
            txttime = ConvTime(WorkingDay, True)

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        strsql = "insert into pos_cashcounted select '" & LocCode & "','" & cashRegister & "','" & CType(WorkingDay, Date).ToString("MM/dd/yyyy") & "',"
                        strsql &= "'" & txttime & "','010' [sett.code], '" & LocalCurrency & "' ,'R',"
                        strsql &= " " & CDbl(amountLC) & " amount, 'From Mobile' Remarks , getdate(),'" & userCode & "' "

                        cmsql.CommandText = strsql
                        cmsql.ExecuteNonQuery()

                        strsql = "insert into pos_cashcounted select '" & LocCode & "','" & cashRegister & "','" & CType(WorkingDay, Date).ToString("MM/dd/yyyy") & "',"
                        strsql &= "'" & txttime & "','010' [sett.code], '" & ControlCurrency & "' ,'R',"
                        strsql &= " " & CDbl(amountCC) & " amount, 'From Mobile' Remarks , getdate(),'" & userCode & "' "

                        cmsql.CommandText = strsql
                        cmsql.ExecuteNonQuery()

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error" & ex.Message
        End Try

    End Function

#End Region

#Region "Promotion"

    Private PromotionCriteria As String = "G"

    Public Function GetPromotions() As String Implements IDataService.GetPromotions

        Dim WorkingDay, strsql As String

        Try

            WorkingDay = CurrentLocationWorkingDay()

            strsql = $"  SELECT  ROW_NUMBER() OVER (ORDER BY PromH_Recid, PromD_ReciID) promRecKey, 
                                PromH_Recid promHeaderRecId ,
                                CASE WHEN promd_status = 'f'
                                     THEN CONCAT(promh_code, ' - ', PromH_Desc)
                                     ELSE ''
                                END promDescription ,
                                CASE WHEN promd_status = 'f' THEN
                                    CASE PromH_Satuts
                                        WHEN 'G' THEN 'Global'
                                        WHEN 'T' THEN 'Specific Client Type'
                                        WHEN 'C' THEN 'Specific Client'
                                    END 
                                ELSE ''
                                END promType ,
                                CASE WHEN promd_status = 'f' 
                                        THEN 'Buy (' + CAST(PromD_Qty AS VARCHAR) + ')'
                                        ELSE 'Get' + CASE WHEN PromH_GetTotQty > 0
                                                        THEN ' (' + CAST(PromH_GetTotQty AS VARCHAR)
                                                            + ')'
                                                        ELSE ''
                                                    END
                                END promBuyGet ,
                                CASE WHEN PromD_StkId IS NOT NULL THEN 'Item'
                                        WHEN PromD_CateCode IS NOT NULL THEN 'Category'
                                        WHEN PromD_ItmIdentif IS NOT NULL THEN 'Item Identification'
                                        WHEN PromD_PGCode IS NOT NULL THEN 'Promotion Group'
                                END AS promItemCategoryIdentificationType ,
                                CASE WHEN PromD_StkId IS NOT NULL
                                        THEN stk04_stkfil.STK_REF
                                            + ' - ' + stk_desc
                                        ELSE "

            If PromotionCriteria = "C" Then
                strsql &= " Cat_Code + ' - ' + Cat_Desc "
            ElseIf PromotionCriteria = "I" Then
                strsql &= " ID_CODE + ' - ' + id_Desc "
            Else
                strsql &= " PGP_Code + ' - ' + PGP_Desc "
            End If

            strsql &= $" END AS promItemCategoryIdentification ,
                                CASE WHEN promd_status = 'f' THEN NULL
                                        ELSE CASE WHEN PromH_GetTotQty > 0 THEN NULL
                                                ELSE PromD_Qty
                                            END
                                END promGetQty ,
                                CASE WHEN promd_status = 'f' THEN NULL
                                        ELSE PromD_Disc
                                END AS promGetDisc
                            FROM   Stk04_promohead
                                INNER JOIN stk04_promodet ON promh_recid = promd_hrecid
                                LEFT JOIN {IIf(ClocHO, "", "pos_stkfil")} stk04_stkfil ON stk_recid = promd_StkId
                                LEFT JOIN dbo.Stk04_Category ON Cat_Code = PromD_CateCode
                                LEFT JOIN dbo.STK04_ItmIdentif ON ID_RECID = PromD_ItmIdentif
                                LEFT JOIN dbo.Stk04_PromotionGroup ON PGP_Code = PromD_PGCode
                            WHERE  promh_locCode = '{LocCode}'
                                AND PromH_Amount = 0
                                AND PromH_Active = 'Y'
                                AND PromH_Confirmed = 'Y'
                                AND '{CDate(WorkingDay).ToString("MM/dd/yyyy")}' BETWEEN PromH_ValidFrom
                                                    AND     PromH_ValidTo
                            ORDER BY PromH_Recid ,
                                PromD_ReciID; "

            Using drPromotions As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(drPromotions)
                End Using

                If drPromotions.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(drPromotions)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try

    End Function

    Public Function GetPromotion(itemRecID As String, itemQty As Double, clientRecID As String, takenPromotions As Object, transactionCurrency As String) As String Implements IDataService.GetPromotion

        Dim strsql As String
        Dim WorkingDay, gbApp, gbCurr1, gbCurr2, gbComp, gbYear As String
        Dim LocVatType As String, LocVatRetT As String, LocVatWhT As String
        Dim ClientCategory As String, priceUsed As String, Cl_ClientType As String
        Try

            Return "" 'Temporary

            WorkingDay = CurrentLocationWorkingDay()

            gbApp = "POSO"

            strsql = "  SELECT TOP 1
                                Ac_Year ,
                                Ac_Comp ,
                                Ac_Curr1 ,
                                Ac_Curr2
                         FROM   dbo.Com_AppComp WITH ( NOLOCK )
                         WHERE  Ac_Code = '" & gbApp & "'
                                AND Ac_Comp = ( SELECT  Loc_CoCode
                                                FROM    dbo.STK04_Location
                                                WHERE   Loc_Code = '" & LocCode & "'
                                              )
                         ORDER BY Ac_Year DESC; "

            Using dtCurr As New DataTable
                Using daCurr As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCurr.Fill(dtCurr)
                End Using

                gbCurr1 = dtCurr(0)("Ac_Curr1").ToString
                gbCurr2 = dtCurr(0)("Ac_Curr2").ToString
                gbComp = dtCurr(0)("Ac_Comp").ToString
                gbYear = dtCurr(0)("Ac_Year").ToString

            End Using

            strsql = "SELECT Loc_vat,Loc_vatno,Loc_Vatrettype,loc_vatwtype,loc_type1,loc_type3,Loc_AllowRef  FROM Stk04_Location where loc_code = '" & LocCode & "'"

            Using dtLoc As New DataTable
                Using daLoc As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daLoc.Fill(dtLoc)
                End Using

                LocVatType = dtLoc(0)("Loc_vat").ToString
                LocVatRetT = dtLoc(0)("Loc_Vatrettype").ToString
                LocVatWhT = dtLoc(0)("loc_vatwtype").ToString

            End Using

            strsql = " SELECT  CL_PriceCat ,
                               CL_UsedPrice ,
                               CL_Type 
                       FROM    dbo.Stk04_Clients
                       WHERE   CL_RecID = " & clientRecID & " "

            Using dtCl As New DataTable
                Using daCl As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCl.Fill(dtCl)
                End Using

                ClientCategory = dtCl(0)("CL_PriceCat").ToString
                priceUsed = dtCl(0)("CL_UsedPrice").ToString
                Cl_ClientType = dtCl(0)("CL_Type").ToString

            End Using

            Return promotionExecution(takenPromotions, itemQty, 'itemQtyClient, itemQtyClientType, itemCatQty, itemCatQtyClient, itemCatQtyClientType, 
                                              "S", "", "T", itemRecID, clientRecID,
                   WorkingDay, gbCurr1, gbCurr2, LocVatType, LocVatRetT, LocVatWhT, transactionCurrency, ClientCategory, priceUsed, Cl_ClientType, False)

        Catch ex As Exception
            Return "Error " & ex.ToString
        End Try
    End Function

    Private Function promotionExecution(takenPromotions As Object, itemQty As Double, 'qtyClient As Double, qtyClinetType As Double, catqty As Double, catqtyClient As Double, catqtyClinetType As Double,
                                        _TXTStatus As String, _Counter As String,
                                   _ItemClass As String, ' _Color As String, _Grid As String, _DetGrid As String, _ExpiryDate As String, _Batch As String,
                                   itemRecid As Integer, CL_RecId As String, CurrentInvWorkingDay As String, ' Optional _lstPromotionIdsUsed As List(Of String) = Nothing,
                                    gbCurr1 As String, gbCurr2 As String, LocVatType As String, LocVatRetT As String, LocVatWhT As String, TransactionCurrency As String,
                                        ClientCategory As String, priceUsed As String, Cl_ClientType As String,
                                    Optional withPromAmount As Boolean = False) As String
        'Optional itmCounterDel As Integer = 0

        'This function will add items for promotion, update existing quantities if the quantity of the initial item is modified, delete items if the initial item is deleted
        Dim strsql As String

        Dim promotionId As String = ""
        Dim promotionIdClinet As String = ""
        Dim promotionIdClinetType As String = ""
        Dim promotionCatorIDId As String = ""
        Dim promotionCatorIDIdClinet As String = ""
        Dim promotionCatorIDIdClinetType As String = ""

        Dim ItemcatorID As String

        Try
            'If _Counter.Trim = "" AndAlso withPromAmount = False Then Exit Try

            'If dsDetInv.Tables("DetailInv").Select("COUNTER = '" & _Counter & "' and (isnull(OfferRecId,0) <> 0 or isnull(DORecID,0) <>0 )  ").Count <> 0 Then Exit Try

            If PromotionCriteria = "C" Then
                ItemcatorID = ExecuteScalar_WOXACT("SELECT STK_CATEGORY FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & itemRecid & " ").ToString
            ElseIf PromotionCriteria = "I" Then
                ItemcatorID = ExecuteScalar_WOXACT("SELECT STK_ITMIDENT FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & itemRecid & " ").ToString
            Else
                ItemcatorID = ExecuteScalar_WOXACT("SELECT PGI_PGPCode FROM Stk04_PromotionGroupItems WHERE EXISTS ( SELECT TOP 1 1 FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & itemRecid & " AND STK_RECID = PGI_StockRecID ) ").ToString
            End If

            If itemRecid <> 0 Then
                If Not RecordsExists("SELECT TOP 1 1 FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID = " & itemRecid & " AND STK_PROMO = 'y'") Then Return ""
            End If

            If CheckPromotion(CurrentInvWorkingDay, itemRecid.ToString, ItemcatorID) = False AndAlso withPromAmount = False Then Return ""

            'Execute("  DELETE FROM " & TmpProm & " WHERE PR_Counter = '" & _Counter.Trim & "' ")

            promotionId = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                             CL_RecId, Cl_ClientType, "G", False, True, CurrentInvWorkingDay) ', TmpProm)

            promotionIdClinet = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                                   CL_RecId, Cl_ClientType, "C", False, True, CurrentInvWorkingDay) ', TmpProm)

            promotionIdClinetType = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                                       CL_RecId, Cl_ClientType, "T", False, True, CurrentInvWorkingDay) ', TmpProm)

            promotionCatorIDId = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                                    CL_RecId, Cl_ClientType, "G", True, True, CurrentInvWorkingDay) ', TmpProm)

            promotionCatorIDIdClinet = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                                          CL_RecId, Cl_ClientType, "C", True, True, CurrentInvWorkingDay) ', TmpProm)

            promotionCatorIDIdClinetType = getpromotionHeader(_Counter, CStr(itemRecid), _ItemClass, '_Color, _Grid, _DetGrid, _ExpiryDate, _Batch,
                                                              CL_RecId, Cl_ClientType, "T", True, True, CurrentInvWorkingDay) ', TmpProm)


            'calculate the qty
            Dim qty As Double

            qty = 0 'itemQty

            Dim jss As New JavaScriptSerializer()
            Dim model As Object = jss.Deserialize(Of Object)(takenPromotions)

            For Each line In CType(model, Array)

                If CType(line, Dictionary(Of String, Object))("itemRecID").ToString.ToUpper = itemRecid Or
                    CType(line, Dictionary(Of String, Object))("itemPromotionCriteria").ToString.ToUpper = ItemcatorID Then

                    qty += CDbl(CType(line, Dictionary(Of String, Object))("newItemQty").ToString.ToUpper)

                End If

                'If promotionId.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Or
                '   promotionIdClinet.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Or
                '   promotionIdClinetType.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Or
                '   promotionCatorIDId.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Or
                '   promotionCatorIDIdClinet.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Or
                '   promotionCatorIDIdClinetType.ToUpper.Contains(CType(line, Dictionary(Of String, Object))("tpPromotionRecId").ToString.ToUpper) Then

                '    qty -= CDbl(CType(line, Dictionary(Of String, Object))("tpTakenQty"))

                'End If

            Next

            ExecuteScalar_WOXACT($"Select {qty} [sumOfQty] ")


            'Dim rate2 As String
            'If txtRate2.Text.Trim = "" Then
            '    rate2 = ""
            'Else
            '    rate2 = txtRate2.Text.Trim.Substring(1)
            'End If

            If promotionId <> "" OrElse promotionIdClinetType <> "" OrElse promotionIdClinet <> "" _
                OrElse promotionCatorIDId <> "" OrElse promotionCatorIDIdClinetType <> "" OrElse promotionCatorIDIdClinet <> "" Then

                strsql = promotionQuery(gbCurr1, gbCurr2, LocVatType, LocVatRetT, LocVatWhT, "I", CurrentInvWorkingDay, TransactionCurrency, "", ClientCategory, "",'rate 2 ''
                                        CL_RecId, Cl_ClientType, priceUsed, 0, 0,
                                        withPromAmount, itemRecid, qty, promotionId, qty, promotionIdClinet, qty, promotionIdClinetType, qty, promotionCatorIDId, qty, promotionCatorIDIdClinet, qty, promotionCatorIDIdClinetType)

            Else

                qty = 0
                strsql = promotionQuery(gbCurr1, gbCurr2, LocVatType, LocVatRetT, LocVatWhT, "I", CurrentInvWorkingDay, TransactionCurrency, "", ClientCategory, "",
                                        CL_RecId, Cl_ClientType, priceUsed, 0, 0,
                                        withPromAmount, itemRecid, qty)

            End If

            Dim trnRate As Double 'pending
            If TransactionCurrency = gbCurr2 Then

                trnRate = CDbl(ExecuteScalar_WOXACT($" SELECT  ISNULL(MAX(CASE App_Rate
                                                                  WHEN 1 THEN DR_Rate1LC
                                                                  WHEN 2 THEN DR_Rate2LC
                                                                  ELSE DR_Rate3LC
                                                                END), 0)
                                                        FROM    dbo.Com_DailyRate
                                                                INNER JOIN dbo.Com_Application ON 1 = 1
                                                        WHERE   App_Code = 'POSO'
                                                                AND DR_CurCode = '{gbCurr2}'
                                                                AND DR_Date = '{CDate(CurrentInvWorkingDay).ToString("MM/dd/yyyy")}' "))

            End If

            strsql = $" WITH tblProm as ( {strsql} ) SELECT  tblProm.Nbr recKey ,
            PromH_RecID promRecID, 
            CASE WHEN ISNULL(tblProm.[Get Item Recid], 0) = 0 THEN 'C'
                 ELSE 'I'
            END [type] ,
            CASE WHEN ISNULL(tblProm.[Get Item Recid], 0) = 0 THEN 'Criteria'
                 ELSE 'Item'
            END [typeDescription] ,
            tblProm.InitialGet * tblProm.Coef getQty ,
            ISNULL(tblProm.[Get Item Recid], 0) itemRecID ,
         CASE WHEN ISNULL(tblProm.[Get Item Recid], 0) = 0 THEN '' else   tblProm.[Get Item Ref.] END itemReference ,
         CASE WHEN ISNULL(tblProm.[Get Item Recid], 0) = 0 THEN '' else   tblProm.[Get Item Description] END itemDescription ,
            tblProm.[U.Price] {IIf(TransactionCurrency = gbCurr1, "", $" / {trnRate}")} itemPrice ,
            tblProm.[Disc %] itemDiscount ,
            CASE WHEN ISNULL(tblProm.BuyItemRecId, 0) = 0
                 THEN tblProm.[Get Item Ref.]
                 ELSE ''
            END crCode ,
            CASE WHEN ISNULL(tblProm.BuyItemRecId, 0) = 0
                 THEN tblProm.[Get Item Description]
                 ELSE ''
            END crDescription ,
            tblProm.PromD_AndOr andOr ,
            tblProm.InitialBuy * tblProm.Coef buyQty,
            'N' selected ,
            'Y' canSelect
    FROM    tblProm; "

            Using dtPromotion As New DataTable

                Using daPromotion As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daPromotion.Fill(dtPromotion)
                End Using

                If dtPromotion.Rows.Count > 0 Then
                    Return GetJson(dtPromotion)
                Else
                    Return ""
                End If

            End Using

        Catch ex As Exception
            Throw ex
        Finally

            'If Not lcdtPromotion Is Nothing Then
            '    lcdtPromotion.Dispose()
            '    lcdtPromotion = Nothing
            'End If

            'Disposecontrols(cnSql, cmSql, drSql, , , daPromotion)

        End Try
    End Function

    Public Function getpromotionHeader(ItemCounter As String, ItemId As String, ItemClass As String, 'ItemColor As String, ItemGrid As String, ItemDetGrid As String, ItemExpDate As String, ItemBatch As String,
                                     ClientRecID As String, ClientType As String, PromoStatus As String, CatorIDPromo As Boolean, InsertInTemp As Boolean, WorkDay As String) As String
        Dim strsql As String
        Dim ItemcatorID, RetString As String
        Try

            If PromotionCriteria = "C" Then
                ItemcatorID = ExecuteScalar_WOXACT("SELECT STK_CATEGORY FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & ItemId & " ").ToString
            ElseIf PromotionCriteria = "I" Then
                ItemcatorID = ExecuteScalar_WOXACT("SELECT STK_ITMIDENT FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & ItemId & " ").ToString
            Else
                ItemcatorID = ExecuteScalar_WOXACT("SELECT PGI_PGPCode FROM Stk04_PromotionGroupItems WHERE EXISTS ( SELECT TOP 1 1 FROM " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " WHERE STK_RECID  =  " & ItemId & " AND STK_RECID = PGI_StockRecID ) ").ToString
            End If

            strsql = " SELECT PromH_RecID PHRECID "
            strsql &= " FROM dbo.stk04_PromoHead"
            strsql &= " INNER JOIN dbo.stk04_PromoDet ON stk04_PromoDet.PromD_HRecID = stk04_PromoHead.PromH_RecID"
            strsql &= " left JOIN dbo.Stk04_PromoSizeColExp ON Stk04_PromoSizeColExp.PromSub_DetRecID = stk04_PromoDet.PromD_ReciID"

            strsql &= " WHERE   PromD_Status = 'F'"
            If CatorIDPromo = False Then
                strsql &= " AND (PromD_StkId = " & ItemId & " ) "
            Else
                strsql &= " AND ( "
                If PromotionCriteria = "C" Then
                    strsql &= " PromD_CateCode = '" & ItemcatorID & "' "
                ElseIf PromotionCriteria = "I" Then
                    strsql &= " PromD_ItmIdentif = NULLIF('" & ItemcatorID & "','') "
                Else
                    strsql &= " PromD_PGCode = '" & ItemcatorID & "' "
                End If
                strsql &= " ) "
            End If

            strsql &= " AND PromH_LocCode = '" & LocCode & "'"
            strsql &= " AND PromH_Active = 'Y'"
            strsql &= " AND PromH_Confirmed = 'Y'"
            strsql &= " AND '" & CDate(WorkDay).ToString("MM/dd/yyyy") & "' BETWEEN PromH_ValidFrom"
            strsql &= " AND PromH_ValidTo"

            'If CatorIDPromo = False AndAlso HaveSubPromo(CStr(ItemId), WorkDay) = True Then
            '    If ItemClass = "F" Then
            '        strsql &= " AND PromSub_Color = '" & ItemColor.Trim.Replace("'", "''") & "'"
            '        strsql &= " AND PromSub_GridSize = '" & ItemDetGrid.Trim.Replace("'", "''") & "'"
            '        strsql &= " AND PromSub_GridCode = '" & ItemGrid.Trim.Replace("'", "''") & "'"
            '    ElseIf ItemClass = "E" Then
            '        strsql &= " AND PromSub_Expiry ='" & ItemExpDate.Trim.Replace("'", "''") & "'"
            '        strsql &= " AND PromSub_Batch ='" & ItemBatch.Trim.Replace("'", "''") & "'"
            '    End If
            'End If

            If PromoStatus = "G" Then
                strsql &= " and PromH_Satuts = 'G' "
            ElseIf PromoStatus = "C" Then
                strsql &= " and PromH_Satuts = 'C' and EXISTS ( SELECT TOP 1 1 from stk04_PromoHeadClients where PromHC_PromRecId = PromH_RecID AND PromHC_CltRecid = " & ClientRecID & " ) "
            ElseIf PromoStatus = "T" Then
                strsql &= " and PromH_Satuts = 'T' and PromH_Clienttype =   '" & ClientType & "'"
            End If

            Using dtGetProm As New DataTable

                Using daGetProm As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daGetProm.SelectCommand.CommandTimeout = 300
                    daGetProm.Fill(dtGetProm)
                End Using

                If dtGetProm.Rows.Count = 0 Then
                    Return ""
                Else

                    RetString = ""

                    For Each Hdr In dtGetProm.Select()
                        RetString &= Hdr.Item("PHRECID").ToString & ","

                        'insertInPrTemp(TmpProm, ItemCounter, IIf(PromoStatus = "G" And Not CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString,
                        '               IIf(PromoStatus = "C" And Not CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString,
                        '               IIf(PromoStatus = "T" And Not CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString,
                        '               IIf(PromoStatus = "G" And CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString,
                        '               IIf(PromoStatus = "C" And CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString,
                        '               IIf(PromoStatus = "T" And CatorIDPromo, Hdr.Item("PHRECID").ToString, "").ToString)

                    Next

                    RetString = Microsoft.VisualBasic.Left(RetString, RetString.Length - 1)

                    Return " " & RetString & " "

                End If

            End Using

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function HaveSubPromo(ByVal itemId As String, WorkDay As String) As Boolean
        Dim strsql As String
        Try
            strsql = " SELECT TOP 1"
            strsql &= " 1"
            strsql &= " FROM    dbo.stk04_PromoHead"
            strsql &= " INNER JOIN dbo.stk04_PromoDet ON stk04_PromoDet.PromD_HRecID = stk04_PromoHead.PromH_RecID"
            strsql &= " INNER JOIN dbo.Stk04_PromoSizeColExp ON Stk04_PromoSizeColExp.PromSub_DetRecID = stk04_PromoDet.PromD_ReciID"
            strsql &= " WHERE   PromD_Status = 'F'"
            strsql &= " AND PromD_StkId = " & itemId & ""
            strsql &= " and   PromH_LocCode = '" & LocCode & "'"
            strsql &= " AND PromH_Active = 'Y'"
            strsql &= " AND PromH_Confirmed = 'Y'"
            strsql &= " AND '" & CDate(WorkDay).ToString("MM/dd/yyyy") & "' BETWEEN PromH_ValidFrom"
            strsql &= " AND     PromH_ValidTo"
            If RecordsExists(strsql) = True Then
                Return True
                Exit Function
            End If
            Return False
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function CheckPromotion(WorkDay As String, itemRecid As String, ItemCategoryOrIDOrPG As String) As Boolean
        Dim strsql As String
        Try

            strsql = " SELECT  TOP 1 1"
            strsql &= " FROM    dbo.stk04_PromoHead"
            strsql &= " INNER JOIN ( SELECT PromD_StkId ,PromD_CateCode , PromD_ItmIdentif , PromD_PGCode , "
            strsql &= " PromD_HRecID"
            strsql &= " FROM   stk04_PromoDet"
            strsql &= " WHERE  PromD_Status = 'F'"
            strsql &= " ) BuyItems ON BuyItems.PromD_HRecID = dbo.stk04_PromoHead.PromH_RecID"
            strsql &= " AND  ( PromD_StkId =" & itemRecid & "  or "

            If PromotionCriteria = "C" Then
                strsql &= " PromD_CateCode = '" & ItemCategoryOrIDOrPG & "' "
            ElseIf PromotionCriteria = "I" Then
                strsql &= " PromD_ItmIdentif = NULLIF('" & ItemCategoryOrIDOrPG & "','') "
            Else
                strsql &= " PromD_PGCode = '" & ItemCategoryOrIDOrPG & "' "
            End If

            strsql &= " ) INNER JOIN (  SELECT STK_RECID ,STK_CATEGORY, STK_ITMIDENT, PGI_PGPCode FROM  " & IIf(ClocHO, "Stk04", "POS").ToString & "_STKFIL
                                        LEFT JOIN dbo.Stk04_PromotionGroupItems ON Stk04_PromotionGroupItems.PGI_StockRecID = STK_RECID 
                                        WHERE STK_PROMO = 'Y' ) STK04_STKFIL ON PromD_StkId = STK04_STKFIL.STK_RECID OR "

            If PromotionCriteria = "C" Then
                strsql &= " STK_CATEGORY = PromD_CateCode  "
            ElseIf PromotionCriteria = "I" Then
                strsql &= " STK_ITMIDENT = PromD_ItmIdentif "
            Else
                strsql &= " PGI_PGPCode = PromD_PGCode "
            End If

            strsql &= " WHERE   PromH_LocCode = '" & LocCode & "'"
            strsql &= " AND PromH_Active = 'Y'"
            strsql &= " AND PromH_Confirmed = 'Y'"
            strsql &= " AND '" & CDate(WorkDay).ToString("MM/dd/yyyy") & "' BETWEEN PromH_ValidFrom"
            strsql &= " AND PromH_ValidTo"
            strsql &= " AND PromH_Amount = 0"

            Return RecordsExists(strsql)

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function promotionQuery(gbCurr1 As String, gbCurr2 As String, LocVatType As String, LocVatRetT As String, LocVatWhT As String,
                                  Src As String, WorkDay As String, Curr As String, Rate2 As String, ClCat As String, TrntbTable As String, ClientRecID As String, ClientType As String, PriceUsed As String, NetLC As String, NetCC As String,
                            withPromAmount As Boolean, itemRecid As Integer, qty As Double, Optional ByVal PromotionHead As String = "",
                            Optional ByVal QtyClient As Double = 0, Optional ByVal PromotionHeadClient As String = "", Optional ByVal QtyClientType As Double = 0,
                            Optional ByVal PromotionHeadClientType As String = "", Optional ByVal Catqty As Double = 0, Optional ByVal CatPromotionHead As String = "",
                            Optional ByVal CatQtyClient As Double = 0, Optional ByVal CatPromotionHeadClient As String = "",
                            Optional ByVal CatQtyClientType As Double = 0, Optional ByVal CatPromotionHeadClientType As String = "", Optional _FromBuyToGet As Boolean = False) As String

        Dim strsql As StringBuilder
        Dim headIdConcat As String
        Dim AmountFromOfferOrDo As Double
        Dim stramount As String
        Try

            strsql = New StringBuilder
            If Rate2.Trim = "" Then
                Rate2 = "0"
            End If

            If Src = "I" Then

                If withPromAmount Then
                    'If LocVatType <> "N" AndAlso ClCat <> "X" And ((LocVatPrice <> "W" And ClCat <> "W" And LocVatRetT = "A") Or (ClCat = "W" And LocVatWhT = "A") Or (LocVatPrice = "W" And LocVatWhT = "A")) Then
                    '    stramount = " SELECT SUM(Net-DiscVal+VatVal) FROM  " & TrntbTable & ""
                    '    stramount &= " WHERE  ISNULL(OfferRecId,0) <>0 OR ISNULL(DORecID,0) <> 0"
                    'Else

                    '    stramount = " SELECT SUM(Net-DiscVal) FROM  " & TrntbTable & ""
                    '    stramount &= " WHERE  ISNULL(OfferRecId,0) <>0 OR ISNULL(DORecID,0) <> 0"
                    'End If
                    'If RecordsExists(stramount) = True Then
                    '    If ExecuteScalar_WOXACT(stramount).ToString.Trim = "" Then
                    '        AmountFromOfferOrDo = 0
                    '    Else
                    '        AmountFromOfferOrDo = CDbl(ExecuteScalar_WOXACT(stramount))
                    '    End If

                    'Else
                    '    AmountFromOfferOrDo = 0
                    'End If
                End If

            End If

            With strsql
                .Clear()
                .Append("SELECT  ROW_NUMBER() OVER (ORDER BY PromH_RecID) Nbr, PromH_RecID, 'False' chk,PromH_Desc +' '+ case when isnull(PromH_GetTotQty,0) <> 0 then  ") '
                .Append(" 'Get Qty Item Of : '   + convert(varchar,PromH_GetTotQty) ELSE '' end    [Promo. Desc],PromH_Amount [Inv. Amount],PromH_Currency [Curr],")
                .Append(" 0 [BuyItemRecId],itemref [Buy Item Ref.],itemDesc [Buy Item Desc.],BuyItems.PromD_QTY [Buy Item Qty],")
                .Append(" '' formula,")
                .Append(" CASE WHEN BuyItems.PromD_Brand IS NULL THEN CASE WHEN BuyItems.promd_group IS NULL THEN 'Item' ELSE 'Department' end ELSE 'Brand' END [Get Type],")
                .Append(" CASE WHEN BuyItems.PromD_Brand IS NULL THEN CASE WHEN BuyItems.promd_group IS NULL THEN '' ELSE BuyGroup.Grp_DESC end ELSE BuyBrand.br_Name END [Brand/Department Desc],")
                .Append(" GetStkFil.STK_RECID [Get Item Recid],")
                .Append(" ( SELECT TOP 1 ")
                .Append(" dbar_barcode ")
                .Append(" FROM Stk04_DetBarcode ")
                .Append(" WHERE dbar_stkid = getStkFil.STK_RECID ")
                .Append(" ORDER BY  dbar_recid DESC ")
                .Append(" ) [Get Item Barcode] , ")
                .Append(" ( SELECT TOP 1 ")
                .Append(" sref_supref ")
                .Append(" FROM STK04_StkSupRef ")
                .Append(" WHERE SRef_StkRecId = GetStkFil.STK_RECID ")
                .Append(" ORDER BY  SREF_RecId DESC ")
                .Append(" )  [Get Item Supplier Ref] , ")
                .Append(" BO_ISBN [Get Item ISBN],")
                .Append(" ISNULL(GetStkFil.STK_Ref,")

                If PromotionCriteria = "C" Then
                    .Append(" Cat_code ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" ID_RECID ")
                Else
                    .Append(" PGP_Code ")
                End If

                .Append(") [Get Item Ref.] , ISNULL(GetStkFil.STK_DESC,")

                If PromotionCriteria = "C" Then
                    .Append(" Cat_Desc ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" CONCAT(ID_CODE, ' - ', ID_DESC) ")
                Else
                    .Append(" PGP_Desc ")
                End If

                .Append(") [Get Item Description] ,")
                If withPromAmount = True Then
                    .Append(" case when isnull(PromH_GetTotQty,0) = 0 then 1 else 0 END * GetItems.PromD_QTY [Qty], '' [PlusMinus] , BuyItems.PromD_QTY [InitialBuy] , GetItems.PromD_QTY [InitialGet] , ")
                    .Append(" 1 [Coef], ")
                    .Append(" '' as [Btn] ,")
                Else
                    If PromotionHead <> "" OrElse PromotionHeadClient <> "" OrElse PromotionHeadClientType <> "" _
                         OrElse CatPromotionHead <> "" OrElse CatPromotionHeadClient <> "" OrElse CatPromotionHeadClientType <> "" Then
                        .Append(" 	CASE    ")

                        'try to join all qty params in 1 - test 08 04 2024

                        If PromotionHead <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHead & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & qty & " / BuyItems.PromD_QTY) ")
                        End If
                        If PromotionHeadClient <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHeadClient & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & QtyClient & " / BuyItems.PromD_QTY) ")
                        End If
                        If PromotionHeadClientType <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHeadClientType & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & QtyClientType & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHead <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHead & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & Catqty & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHeadClient <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClient & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & CatQtyClient & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHeadClientType <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClientType & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & CatQtyClientType & " / BuyItems.PromD_QTY) ")
                        End If

                        .Append("  End  ")
                    Else
                        .Append(" GetItems.PromD_QTY * FLOOR(" & qty & " / BuyItems.PromD_QTY)  ")
                    End If

                    .Append(" [Qty] , '' [PlusMinus] , BuyItems.PromD_QTY [InitialBuy] , case when isnull(PromH_GetTotQty,0)=0 then GetItems.PromD_QTY else isnull(PromH_GetTotQty,0) END [InitialGet] , ")

                    If PromotionHead <> "" OrElse PromotionHeadClient <> "" OrElse PromotionHeadClientType <> "" _
                         OrElse CatPromotionHead <> "" OrElse CatPromotionHeadClient <> "" OrElse CatPromotionHeadClientType <> "" Then
                        .Append(" 	CASE    ")
                        If PromotionHead <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHead & ") THEN FLOOR(" & qty & " / BuyItems.PromD_QTY) ")
                        End If
                        If PromotionHeadClient <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHeadClient & ") THEN FLOOR(" & QtyClient & " / BuyItems.PromD_QTY) ")
                        End If
                        If PromotionHeadClientType <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & PromotionHeadClientType & ") THEN FLOOR(" & QtyClientType & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHead <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHead & ") THEN FLOOR(" & Catqty & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHeadClient <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClient & ") THEN FLOOR(" & CatQtyClient & " / BuyItems.PromD_QTY) ")
                        End If
                        If CatPromotionHeadClientType <> "" Then
                            .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClientType & ") THEN FLOOR(" & CatQtyClientType & " / BuyItems.PromD_QTY) ")
                        End If

                        .Append("  End  ")
                    Else
                        .Append(" FLOOR(" & qty & " / BuyItems.PromD_QTY) ")
                    End If
                    .Append(" [Coef]  , ")

                    .Append(" '' as [Btn] ,")
                End If
                .Append(" GetItems.PromD_Disc [Disc %], ")

                .Append(" case when '" & LocVatType & "' = 'N' Or '" & ClCat & "' = 'X' or ('" & LocVatType & "' = 'Y' and GetStkFil.stk_vattype <> 'T') or ")
                .Append(" ('" & LocVatType & "' = 'Y' and GetStkFil.stk_vattype = 'T' and '" & ClCat & "' = 'W' and '" & LocVatWhT & "' <> 'T') or ")
                .Append(" ('" & LocVatType & "' = 'Y' and GetStkFil.stk_vattype = 'T' and '" & ClCat & "' <> 'W' and '" & LocVatRetT & "' <> 'T')  then ")
                .Append("       case when '" & Curr & "' = '" & gbCurr1 & "' then ")
                .Append(CStr(IIf(ClocHO = True, "COALESCE(ClientSpPrice.CL_PriceLCEX,ClientType.CL_PriceLCEX,LSP_LCTAXEX,SP_LCTAXEX,0)", "COALESCE(ClientSpPrice.CL_PriceLCEX,ClientType.CL_PriceLCEX,SP_LCTAXEX,0)")))
                .Append(" else " & CStr(IIf(ClocHO = True, "COALESCE(ClientSpPrice.CL_PriceFCEX,ClientType.CL_PriceFCEX,LSP_FCTAXEX,SP_FCTAXEX,0)", "COALESCE(ClientSpPrice.CL_PriceFCEX,ClientType.CL_PriceFCEX,SP_FCTAXEX,0)")) & " end ")
                .Append(" else case when ('" & LocVatType & "' = 'Y' and GetStkFil.stk_vattype = 'T' and '" & ClCat & "' = 'W' and '" & LocVatWhT & "' = 'T') or  ")
                .Append(" ('" & LocVatType & "' = 'Y' and GetStkFil.stk_vattype = 'T' and '" & ClCat & "' <> 'W' and '" & LocVatRetT & "' = 'T') then ")
                .Append("       case when '" & Curr & "' = '" & gbCurr1 & "' then ")
                .Append(CStr(IIf(ClocHO = True, "COALESCE(ClientSpPrice.CL_PriceLCTTC,ClientType.CL_PriceLCTTC,LSP_LCTTC,SP_LCTTC,0)", "COALESCE(ClientSpPrice.CL_PriceLCTTC,ClientType.CL_PriceLCTTC,SP_LCTTC,0)")) & " else ")
                .Append(CStr(IIf(ClocHO = True, "COALESCE(ClientSpPrice.CL_PriceFCTTC,ClientType.CL_PriceFCTTC,LSP_FCTTC,SP_FCTTC,0)", "COALESCE(ClientSpPrice.CL_PriceFCTTC,ClientType.CL_PriceFCTTC,SP_FCTTC,0)")) & " end   ELSE  0 end ")
                .Append(" end  *  case when '" & Curr & "' <> '" & gbCurr1 & "' and '" & Curr & "' <> '" & gbCurr2 & "' then " & IIf(Rate2.Trim = "", "0", Rate2).ToString & " else 1 end   [U.Price] , ")
                .Append(" case when GetItems.PromD_AndOr = 'O' then 'Or' else 'And' end PromD_AndOr, ")
                .Append(" GetStkFil.STK_AVGCOSTLC,GetStkFil.STK_AVGCOSTCC,GetStkFil.STK_AVGCOSTFC,GetStkFil.Stk_Service,GetStkFil.Stk_Promo,GetStkFil.stk_qtyctrl, ")
                .Append(" GetStkFil.stk_vattype,GetStkFil.Stk_VatPer,GetStkFil.Stk_VatDate,GetStkFil.STK_Unit,GetStkFil.STK_ItmClass, ")

                If PromotionHead <> "" OrElse PromotionHeadClient <> "" OrElse PromotionHeadClientType <> "" _
                   OrElse CatPromotionHead <> "" OrElse CatPromotionHeadClient <> "" OrElse CatPromotionHeadClientType <> "" Then
                    .Append(" 	CASE    ")
                    If PromotionHead <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHead & ") THEN PromH_GetTotQty * FLOOR(" & qty & " / BuyItems.PromD_QTY)  ")
                    End If
                    If PromotionHeadClient <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHeadClient & ") THEN PromH_GetTotQty * FLOOR(" & QtyClient & " / BuyItems.PromD_QTY) ")
                    End If
                    If PromotionHeadClientType <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHeadClientType & ") THEN PromH_GetTotQty * FLOOR(" & QtyClientType & " / BuyItems.PromD_QTY)  ")
                    End If
                    If CatPromotionHead <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHead & ") THEN PromH_GetTotQty * FLOOR(" & Catqty & " / BuyItems.PromD_QTY)  ")
                    End If
                    If CatPromotionHeadClient <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClient & ") THEN PromH_GetTotQty * FLOOR(" & CatQtyClient & " / BuyItems.PromD_QTY) ")
                    End If
                    If CatPromotionHeadClientType <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClientType & ") THEN PromH_GetTotQty * FLOOR(" & CatQtyClientType & " / BuyItems.PromD_QTY)  ")
                    End If

                    .Append("  End   ")
                ElseIf withPromAmount = True Then
                    .Append(" PromH_GetTotQty    ")
                Else
                    .Append(" PromH_GetTotQty * FLOOR(" & qty & " / BuyItems.PromD_QTY)   ")
                End If

                .Append(" [Get Item Qty] , ")
                .Append(" DSTech_Meas [Technical Content]  ")
                .Append(" FROM dbo.stk04_PromoHead")
                .Append(" " & IIf(withPromAmount = False, " Inner ", " Left ").ToString & " JOIN ( SELECT distinct  PromD_HRecID, PromD_Brand,PromD_QTY,PromD_group,")
                .Append(" itemDesc = STUFF((SELECT ' Or '+  case when ")

                If PromotionCriteria = "C" Then
                    .Append(" ISNULL(PromD_CateCode, '') =  '' ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" PromD_ItmIdentif IS NULL  ")
                Else
                    .Append(" ISNULL(PromD_PGCode, '') =  '' ")
                End If

                .Append(" then   STK_DESC   else  ")

                If PromotionCriteria = "C" Then
                    .Append(" Cat_Desc ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" CONCAT(ID_CODE, ' - ', ID_DESC) ")
                Else
                    .Append(" PGP_Desc ")
                End If

                .Append(" end  FROM")
                .Append(" stk04_PromoDet Child")
                .Append(" LEFT JOIN " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " BuyStkFil ON Child.PromD_StkId = BuyStkFil.STK_RECID")

                If PromotionCriteria = "C" Then
                    .Append("  LEFT JOIN dbo.Stk04_Category ON Cat_Code COLLATE SQL_Latin1_General_CP1256_CI_AS = PromD_CateCode COLLATE SQL_Latin1_General_CP1256_CI_AS ")
                ElseIf PromotionCriteria = "I" Then
                    .Append("  LEFT JOIN dbo.STK04_ItmIdentif ON ID_RECID = PromD_ItmIdentif ")
                Else
                    .Append("  LEFT JOIN dbo.Stk04_PromotionGroup ON PGP_Code COLLATE SQL_Latin1_General_CP1256_CI_AS = PromD_PGCode COLLATE SQL_Latin1_General_CP1256_CI_AS ")
                End If

                .Append(" WHERE Child.PromD_HRecID=Head.PromD_HRecID")
                .Append(" and    Child.PromD_Status = 'F'")
                .Append(" FOR XML PATH('')),1,4,'')")
                .Append(" ,")
                .Append(" itemref = STUFF((SELECT ' Or '+  case when ")

                If PromotionCriteria = "C" Then
                    .Append(" ISNULL(PromD_CateCode, '') =  '' ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" PromD_ItmIdentif IS NULL ")
                Else
                    .Append(" ISNULL(PromD_PGCode, '') =  '' ")
                End If

                .Append(" then  STK_REF else ")

                If PromotionCriteria = "C" Then
                    .Append(" PromD_CateCode COLLATE SQL_Latin1_General_CP1256_CI_AS ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" CONVERT(VARCHAR,PromD_ItmIdentif) ")
                Else
                    .Append(" PromD_PGCode COLLATE SQL_Latin1_General_CP1256_CI_AS ")
                End If

                .Append(" end   FROM")
                .Append(" stk04_PromoDet Child")
                .Append(" LEFT JOIN " & CStr(IIf(ClocHO, "STK04_STKFIL", "POS_STKFIL")) & " BuyStkFil ON Child.PromD_StkId = BuyStkFil.STK_RECID")
                .Append(" WHERE Child.PromD_HRecID=Head.PromD_HRecID")
                .Append(" and    Child.PromD_Status = 'F'")
                .Append(" FOR XML PATH('')),1,4,'')")
                .Append(" FROM stk04_PromoDet Head")
                .Append(" WHERE    PromD_Status = 'F'")
                .Append(" ) BuyItems ON BuyItems.PromD_HRecID = dbo.stk04_PromoHead.PromH_RecID")
                .Append(" left JOIN dbo.Stk04_Brand BuyBrand ON BuyItems.PromD_Brand=BuyBrand.br_Code")
                .Append(" left JOIN dbo.Stk04_group BuyGroup ON BuyItems.PromD_group=BuyGroup.Grp_Code")
                .Append(" INNER JOIN dbo.stk04_PromoDet GetItems ON GetItems.PromD_HRecID = PromH_RecID AND GetItems.PromD_Status = 'T'")
                .Append(" LEFT JOIN ( SELECT  STK_RECID , ")
                .Append(" STK_Ref , ")
                .Append(" STK_DESC , ")
                .Append(" stk_vattype , ")
                .Append(" STK_AVGCOSTLC , ")
                .Append(" STK_AVGCOSTCC , ")
                .Append(" STK_AVGCOSTFC , ")
                .Append(" Stk_Service , ")
                .Append(" Stk_Promo , ")
                .Append(" stk_qtyctrl , ")
                .Append(" Stk_VatPer , ")
                .Append(" Stk_VatDate , ")
                .Append(" STK_Unit , ")
                .Append(" STK_ItmClass ")
                .Append(" FROM   " & IIf(ClocHO, "Stk04", "POS").ToString & "_stkfil ")
                .Append(" ) GetStkFil ON GetItems.PromD_StkId = GetStkFil.STK_RECID ")

                If PromotionCriteria = "C" Then
                    .Append(" LEFT JOIN stk04_category ON Cat_Code = GetItems.PromD_CateCode  ")
                ElseIf PromotionCriteria = "I" Then
                    .Append(" LEFT JOIN dbo.STK04_ItmIdentif ON ID_RECID = GetItems.PromD_ItmIdentif  ")
                Else
                    .Append(" LEFT JOIN Stk04_PromotionGroup ON PGP_Code = GetItems.PromD_PGCode  ")
                End If

                .Append(" LEFT JOIN dbo.Stk04_CLSTKPrice ClientSpPrice ON ClientSpPrice.CL_StkRecid = GetStkFil.STK_RECID AND CL_clRecid=" & ClientRecID & " ")
                .Append(" LEFT JOIN dbo.Stk04_CLSTKPrice ClientType ON ClientType.CL_StkRecid = GetStkFil.STK_RECID ")
                .Append(" AND ClientType.CL_ClientType = '" & ClientType & "' ")
                If ClocHO = True Then
                    .Append(" LEFT JOIN Stk04_Locstkprices ON dbo.Stk04_Locstkprices.LSP_StkRecId = GetStkFil.STK_RECID AND LSP_PriceType = '" & PriceUsed.Trim & "' ")
                    .Append(" AND LSP_LocCode = '" & LocCode & "' ")
                End If

                .Append(" LEFT JOIN dbo.Stk04_stkprices ON dbo.Stk04_stkprices.SP_StkRecId = GetStkFil.STK_RECID AND SP_PriceType='" & PriceUsed.Trim & "' 
                          AND ISNULL(SP_StartingQty,0) =   ")

                If PromotionHead <> "" OrElse PromotionHeadClient <> "" OrElse PromotionHeadClientType <> "" _
                         OrElse CatPromotionHead <> "" OrElse CatPromotionHeadClient <> "" OrElse CatPromotionHeadClientType <> "" Then
                    .Append(" 	CASE    ")
                    If PromotionHead <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHead & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & qty & " / BuyItems.PromD_QTY) ")
                    End If
                    If PromotionHeadClient <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHeadClient & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & QtyClient & " / BuyItems.PromD_QTY) ")
                    End If
                    If PromotionHeadClientType <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & PromotionHeadClientType & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & QtyClientType & " / BuyItems.PromD_QTY) ")
                    End If
                    If CatPromotionHead <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHead & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & Catqty & " / BuyItems.PromD_QTY) ")
                    End If
                    If CatPromotionHeadClient <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClient & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & CatQtyClient & " / BuyItems.PromD_QTY) ")
                    End If
                    If CatPromotionHeadClientType <> "" Then
                        .Append("  WHEN PromH_RecID IN (" & CatPromotionHeadClientType & ") THEN case when isnull(PromH_GetTotQty,0)=0 then 1 else 0 END * GetItems.PromD_QTY * FLOOR(" & CatQtyClientType & " / BuyItems.PromD_QTY) ")
                    End If

                    .Append("  End  ")
                Else
                    .Append(" GetItems.PromD_QTY * FLOOR(" & qty & " / BuyItems.PromD_QTY)  ")
                End If

                .Append(" LEFT JOIN bibl1_book ON BO_Recid=GetStkFil.STK_RECID ")
                .Append("  LEFT JOIN dbo.STK04_DSTechnical ON GetStkFil.STK_RECID = STK04_DSTechnical.DSTech_RecId ")
                .Append(" WHERE   PromH_LocCode = '" & LocCode & "'")
                .Append(" AND PromH_Active = 'Y'")
                .Append(" AND PromH_Confirmed = 'Y'")
                .Append(" AND '" & CDate(WorkDay).ToString("MM/dd/yyyy") & "' BETWEEN PromH_ValidFrom")
                .Append(" AND     PromH_ValidTo")
                'withPromAmount: It is used since the promotion frame will open after pressing the payment button if exist promotion directives with promotion amount
                'If the promotion amount is empty, then the system will work item by item

                If withPromAmount = True Then
                    '.Append(" AND dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',PromH_Amount,PromH_Currency,'" & CStr(IIf(Copercurr = "FC", Curr.Trim.Replace("'", "''"), Copercurr)) & "','" & CDate(WorkDay).ToString("MM/dd/yyyy") & "') <= " & (CDbl(IIf(gbCurr1 = Curr, NetLC, NetCC)) - AmountFromOfferOrDo) & " ")
                    '.Append(" and dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',PromH_Amount,PromH_Currency,'" & CStr(IIf(Copercurr = "FC", Curr.Trim.Replace("'", "''"), Copercurr)) & "','" & CDate(WorkDay).ToString("MM/dd/yyyy") & "') <> 0 ")
                Else

                    If PromotionHead <> "" OrElse PromotionHeadClient <> "" OrElse PromotionHeadClientType <> "" OrElse
                         CatPromotionHead <> "" OrElse CatPromotionHeadClient <> "" OrElse CatPromotionHeadClientType <> "" Then
                        headIdConcat = ""
                        If PromotionHead <> "" Then
                            headIdConcat &= PromotionHead & ","
                        End If
                        If PromotionHeadClient <> "" Then
                            headIdConcat &= PromotionHeadClient & ","
                        End If
                        If PromotionHeadClientType <> "" Then
                            headIdConcat &= PromotionHeadClientType & ","
                        End If

                        If CatPromotionHead <> "" Then
                            headIdConcat &= CatPromotionHead & ","
                        End If
                        If CatPromotionHeadClient <> "" Then
                            headIdConcat &= CatPromotionHeadClient & ","
                        End If
                        If CatPromotionHeadClientType <> "" Then
                            headIdConcat &= CatPromotionHeadClientType & ","
                        End If

                        .Append("  AND PromH_RecID IN (" & Microsoft.VisualBasic.Left(headIdConcat, headIdConcat.Length - 1) & ") ")

                        If Not _FromBuyToGet Then

                            .Append(" AND CASE ")
                            If PromotionHead <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & PromotionHead & ") THEN  FLOOR(" & qty & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If
                            If PromotionHeadClient <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & PromotionHeadClient & ") THEN  FLOOR(" & QtyClient & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If
                            If PromotionHeadClientType <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & PromotionHeadClientType & ") THEN  FLOOR(" & QtyClientType & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If
                            If CatPromotionHead <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & CatPromotionHead & ") THEN  FLOOR(" & Catqty & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If
                            If CatPromotionHeadClient <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & CatPromotionHeadClient & ") THEN  FLOOR(" & CatQtyClient & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If
                            If CatPromotionHeadClientType <> "" Then
                                .Append(" WHEN PromH_RecID IN (" & CatPromotionHeadClientType & ") THEN  FLOOR(" & CatQtyClientType & " / CASE WHEN ISNULL(BuyItems.PromD_QTY, 0) = 0 THEN 1")
                                .Append(" ELSE ISNULL(BuyItems.PromD_QTY, 0)")
                                .Append(" END)")
                            End If

                            .Append(" END > 0")

                        End If

                    Else
                        .Append(" AND PromH_Amount = 0 ")
                        .Append(" and FLOOR(" & qty & " / case when isnull(BuyItems.PromD_QTY,0)=0 THEN 1 else isnull(BuyItems.PromD_QTY,0) END)  > 0 ")
                    End If

                End If
                .Append(" and (PromH_Satuts = 'G'")
                .Append(" OR ( PromH_Satuts = 'T' AND PromH_Clienttype = '" & ClientType & "'  )")
                .Append(" OR ( PromH_Satuts = 'C' AND EXISTS ( SELECT TOP 1 1 from stk04_PromoHeadClients where PromHC_PromRecId = PromH_RecID AND PromHC_CltRecid = " & ClientRecID & " ) )")
                .Append(" )")

                '.Append(" ORDER BY PromH_RecID ")

            End With

            promotionQuery = strsql.ToString

        Catch ex As Exception
            Throw ex
        Finally
            strsql = Nothing
        End Try
    End Function

#End Region

#Region "Locking"

    Public Function CheckCreateLock(ByVal _User As String, ByVal _Machine As String, ByVal _Menu As String, ByVal _Table As String, ByVal _Record As String, _RecordDesc As String) As String
        'This Function Will Lock Record If Record Is Not Locked
        Dim strLock As String
        Try

            strLock = isLockedRecord(_User, _Machine, _Menu, _Table, _Record, _RecordDesc)

            If strLock = "" Then
                LockRecord(_User, _Machine, _Menu, _Table, _Record)
                Return "Success"
            Else
                Return strLock
            End If

        Catch ex As Exception
            Throw
        End Try

    End Function

    Public Sub ReleaseLock(ByVal _User As String, ByVal _Machine As String, ByVal _Menu As String, ByVal _Table As String, ByVal _Record As String)
        'Delete Locking Record
        Dim StrDelLock As String
        Try

            StrDelLock = "  DELETE From sysLockRecords WHERE "
            StrDelLock &= " lr_Table = '" & _Table.Trim & "' "
            StrDelLock &= " And lr_Record = '" & _Record.Trim.Replace("'", "''") & "' "
            StrDelLock &= " And lr_ChildApplication = '' "
            StrDelLock &= " And lr_Menu = '" & _Menu.Trim.Replace("'", "''") & "' "
            StrDelLock &= " And lr_User = '" & _User & "' "

            Using cnSqlLock As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnSqlLock.Open()

                Using cmSqlLock As New SqlCommand(StrDelLock, cnSqlLock)
                    cmSqlLock.ExecuteNonQuery()
                End Using

            End Using

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Public Function isLockedRecord(ByVal _User As String, ByVal _Machine As String, ByVal _Menu As String, ByVal _Table As String, ByVal _Record As String, _RecordDesc As String) As String
        'This Function will only check if this record is locked
        Dim strCheckLock As String
        Try

            strCheckLock = "  SELECT TOP 1 "
            strCheckLock &= " lr_Table   [Table] , "
            strCheckLock &= " lr_Machine [Machine] , "
            strCheckLock &= " lr_Menu    [Menu] , "
            strCheckLock &= " lr_User    [UserCode] ,"
            strCheckLock &= " case when lr_User = 'SU' then 'Super User' else [User_Name] end [UserName]  "
            strCheckLock &= " FROM    dbo.sysLockRecords"
            strCheckLock &= " LEFT JOIN dbo.Com_UserInfo ON User_Code = lr_User "
            strCheckLock &= " WHERE   lr_Table = '" & _Table.Trim & "'"
            strCheckLock &= " AND     lr_Record = '" & _Record.Trim.Replace("'", "''") & "'"
            strCheckLock &= " AND     lr_ChildApplication = ''"

            Using dtCheckLock As New DataTable

                Using daCheckLock As New SqlDataAdapter(strCheckLock, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daCheckLock.Fill(dtCheckLock)
                End Using

                If dtCheckLock.Rows.Count > 0 Then

                    If (dtCheckLock(0)("Table").ToString.Trim.ToUpper <> _Table.Trim.ToUpper _
                        OrElse dtCheckLock(0)("Machine").ToString.Trim.ToUpper <> _Machine.Trim.ToUpper _
                        OrElse dtCheckLock(0)("Menu").ToString.Trim.ToUpper <> _Menu.Trim.ToUpper _
                        OrElse dtCheckLock(0)("UserCode").ToString.Trim.ToUpper <> _User.Trim.ToUpper) Then

                        Return _RecordDesc & " is Locked By " & dtCheckLock(0)("UserName").ToString.ToUpper & " - " &
                                    "PC : " & dtCheckLock(0)("Machine").ToString.ToUpper & " - " &
                                    "Menu : " & dtCheckLock(0)("Menu").ToString.ToUpper

                    End If

                End If

            End Using

            Return ""

        Catch ex As Exception
            Throw
        End Try

    End Function

    Private Sub LockRecord(ByVal _User As String, ByVal _Machine As String, ByVal _Menu As String, ByVal _Table As String, ByVal _Record As String)
        'Insert Locking Record
        Dim StrCrlock As String
        Try

            StrCrlock = "  If Not Exists "

            StrCrlock &= " ( Select Top 1 1 From sysLockRecords WHERE "
            StrCrlock &= " lr_Table = '" & _Table.Trim & "' "
            StrCrlock &= " And lr_Record = '" & _Record.Trim.Replace("'", "''") & "' "
            StrCrlock &= " And lr_ChildApplication = '' "
            StrCrlock &= " And lr_Menu = '" & _Menu.Trim.Replace("'", "''") & "' "
            StrCrlock &= " And lr_User = '" & _User & "' )"
            StrCrlock &= " BEGIN "
            StrCrlock &= " INSERT INTO dbo.sysLockRecords"
            StrCrlock &= " VALUES  ("
            StrCrlock &= " '" & _Table.Trim & "' , " '-- lr_Table - varchar(50)"
            StrCrlock &= " '" & _Record.Trim.Replace("'", "''") & "' , " '-- lr_Record - varchar(500)"
            StrCrlock &= " '" & _Machine.Trim.Replace("'", "''") & "' , " '-- lr_Machine - varchar(100)"
            StrCrlock &= " 'POSO' , " '-- lr_Application - varchar(100)"
            StrCrlock &= " '' , " '-- lr_ChildApplication - varchar(100)"
            StrCrlock &= " '" & _Menu.Trim.Replace("'", "''") & "' , " '-- lr_Menu - varchar(200)"
            StrCrlock &= " '" & _User & "' , " '-- lr_User - varchar(30)"
            StrCrlock &= " GETDATE() " '-- lr_Dstmp - datetime"
            StrCrlock &= " )"
            StrCrlock &= " END "

            Using cnSqlLock As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnSqlLock.Open()

                Using cmSqlLock As New SqlCommand(StrCrlock, cnSqlLock)
                    cmSqlLock.ExecuteNonQuery()
                End Using

            End Using

        Catch ex As Exception
            Throw
        End Try

    End Sub

#End Region

#Region "Permissions"

    Public Function NegationPriv(_userCode As String, _menu As String, _button As String) As Boolean
        Dim strsql As String
        Dim value As Boolean
        Try

            strsql = " SELECT Nbu_hasNeg FROM dbo.Com_NegationButton"
            strsql &= " WHERE Nbu_User ='" & _userCode & "' AND Nbu_Comp='" & ConfigurationManager.AppSettings("CompCode").ToString & "'"
            strsql &= " AND Nbu_Appl='POSO' AND Nbu_Module='POSO' AND Nbu_Step='" & _menu & "'"
            strsql &= " AND Nbu_Button='" & _button & "' AND Nbu_Exception=1"

            If RecordsExists(strsql) = True Then
                If ExecuteScalar_WOXACT(strsql).ToString = "1" Then
                    value = True
                Else
                    value = False
                End If
            Else
                strsql = " SELECT TOP 1 1 FROM dbo.Com_GroupNegButton INNER JOIN Com_UserInfoGroup ON Nbg_Group=UIL_GrpCode"
                strsql &= " WHERE UIL_UserCode ='" & _userCode & "' AND Nbg_Comp='" & ConfigurationManager.AppSettings("CompCode").ToString & "'"
                strsql &= " AND Nbg_Appl='POSO' AND Nbg_Module='POSO' AND Nbg_Step='" & _menu & "'"
                strsql &= " AND Nbg_Button='" & _button & "'"
                If RecordsExists(strsql) = True Then
                    value = True
                Else
                    value = False
                End If
            End If

            Return value

        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region

#Region "Controlized Menu"

    Public Function GetControlizedMenu() As String Implements IDataService.GetControlizedMenu

        Try

            Using dtMenu As New DataTable

                Using daSql As New SqlDataAdapter(" SELECT CM_RecKey [recKey], CM_MenuActive [menuActive] FROM dbo.tbl_CPP_ControlizedMenu ",
                                                  DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtMenu)
                End Using

                If dtMenu.Rows.Count = 0 Then
                    Return ""
                Else
                    Return GetJson(dtMenu)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The privileges of the user on the menu items (tbl_CPP_UserPriv, User Privilege tab of the backend) :
    '[{"stepCode":"P1","allowed":"1","privTill":"2026-12-31"}, ...] - privTill "" = no end, a menu item without a row is not allowed
    Public Function GetUserPrivileges(userCode As String) As String Implements IDataService.GetUserPrivileges

        Try

            Using dtPriv As New DataTable

                Using daSql As New SqlDataAdapter(" SELECT  LTRIM(RTRIM(UP_StepCode)) [stepCode] ,
                                                             UP_Allowed [allowed] ,
                                                             ISNULL(CONVERT(VARCHAR(10), UP_PrivTill, 23), '') [privTill]
                                                     FROM    dbo.tbl_CPP_UserPriv
                                                     WHERE   UP_UserCode = @UserCode ",
                                                  DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.SelectCommand.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = If(userCode, "").Trim
                    daSql.Fill(dtPriv)
                End Using

                If dtPriv.Rows.Count = 0 Then
                    Return "[]"
                Else
                    Return GetJson(dtPriv)
                End If

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'menuItems : JSON array [{"recKey":"C1","menuActive":true,"title":"Clients View"}, ...] - only the super user can change the menu
    'title : the menu title of the app (shown by the User Privilege tab of the backend), kept as is when not sent
    Public Function SaveControlizedMenu(userCode As String, menuItems As String) As String Implements IDataService.SaveControlizedMenu

        Try

            If String.IsNullOrEmpty(userCode) OrElse userCode.Trim.ToUpper <> "SU" Then
                Return "Error Only the super user can modify the menu items"
            End If

            Dim arrItems As JArray = JArray.Parse(menuItems)

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" UPDATE dbo.tbl_CPP_ControlizedMenu
                                                    SET    CM_MenuActive = @MenuActive ,
                                                           CM_MenuTitle = ISNULL(@MenuTitle, CM_MenuTitle) ,
                                                           CM_UStamp = 'SU' ,
                                                           CM_DStamp = GETDATE()
                                                    WHERE  CM_RecKey = @RecKey ;

                                                    IF @@ROWCOUNT = 0
                                                        INSERT INTO dbo.tbl_CPP_ControlizedMenu ( CM_RecKey, CM_MenuTitle, CM_MenuActive, CM_UStamp, CM_DStamp )
                                                        VALUES ( @RecKey, @MenuTitle, @MenuActive, 'SU', GETDATE() ) ; ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900
                        cmsql.Parameters.Add("@RecKey", SqlDbType.VarChar, 10)
                        cmsql.Parameters.Add("@MenuTitle", SqlDbType.NVarChar, 50)
                        cmsql.Parameters.Add("@MenuActive", SqlDbType.Bit)

                        For Each item As JObject In arrItems
                            cmsql.Parameters("@RecKey").Value = item("recKey").ToString.Trim
                            Dim strTitle As String = If(item("title")?.ToString, "").Trim
                            cmsql.Parameters("@MenuTitle").Value = If(strTitle = "", CObj(DBNull.Value), Left(strTitle, 50))
                            'The portable can send true / false or "1" / "0" (GetControlizedMenu returns the bit as "1" / "0")
                            Dim strActive As String = item("menuActive").ToString.Trim
                            cmsql.Parameters("@MenuActive").Value = (strActive = "1" OrElse strActive.Equals("true", StringComparison.OrdinalIgnoreCase))
                            cmsql.ExecuteNonQuery()
                        Next

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success"

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Physical Count"

    'The physical count works on the location sent by the device (not the LocCode of the Web.config)
    'and on the database of the server it calls. Replaces the SOAP service ERPDataCollector (InitPullTables / pullRecords / InsertDetail)

    Private Const PhysicalCountMaxPageSize As Integer = 5000
    Private Const PhysicalCountMaxLinesPerSave As Integer = 200000

    'Returns "" when the request is valid, else the error message
    Private Function ValidatePhysicalCountRequest(location As String, countNumber As String, machineName As String) As String

        If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Invalid Location"
        If String.IsNullOrWhiteSpace(countNumber) OrElse Not countNumber.Trim.All(AddressOf Char.IsDigit) OrElse countNumber.Trim.Length > 18 Then Return "Invalid Physical Count Number"
        If String.IsNullOrWhiteSpace(machineName) OrElse machineName.Trim.Length > 5 Then Return "Invalid Machine Name"

        Return ""

    End Function

    'Checks the count header and returns the slot (1 to 6) of the machine in Pch_MachUser1..6
    'Returns "" when the count can be counted by this machine, else the error message
    'lockHeader : inside the save transaction the header row is locked (UPDLOCK, HOLDLOCK) until the commit,
    'so the count cannot be posted to stock or cancelled by the count module in the middle of an upload
    Private Function GetPhysicalCountSlot(cmsql As SqlCommand, location As String, countNumber As String, machineName As String, ByRef slot As Integer, Optional lockHeader As Boolean = False) As String

        slot = 0

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT  ISNULL(Pch_Status, '') Pch_Status ,
                                      ISNULL(Pch_Updated, '') Pch_Updated ,
                                      CASE @MachineName WHEN Pch_MachUser1 THEN 1
                                                        WHEN Pch_MachUser2 THEN 2
                                                        WHEN Pch_MachUser3 THEN 3
                                                        WHEN Pch_MachUser4 THEN 4
                                                        WHEN Pch_MachUser5 THEN 5
                                                        WHEN Pch_MachUser6 THEN 6
                                                        ELSE 0
                                      END Slot
                              FROM    dbo.tbl_PhysicalCountHeader " & IIf(lockHeader, "WITH ( UPDLOCK, HOLDLOCK )", "").ToString & "
                              WHERE   Pch_RecId = @CountNumber
                                      AND Pch_Loc = @Location "
        cmsql.Parameters.Add("@CountNumber", SqlDbType.Decimal).Value = CDec(countNumber.Trim)
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
        cmsql.Parameters.Add("@MachineName", SqlDbType.VarChar, 5).Value = machineName.Trim

        Using drHeader As SqlDataReader = cmsql.ExecuteReader()

            If Not drHeader.Read() Then Return "This Physical Count Does Not Exist"

            If drHeader("Pch_Status").ToString.Trim.ToUpper = "C" Then Return "This Physical Count Was Cancelled"
            If drHeader("Pch_Updated").ToString.Trim.ToUpper <> "N" Then Return "This Physical Count Was Already Updated To Stock"

            slot = CInt(drHeader("Slot"))

        End Using

        If slot = 0 Then Return $"Machine {machineName.Trim} Is Not Assigned To This Physical Count"

        Return ""

    End Function

    'Lines of the count, shared by the details page and the lines count of the header
    Private Function PhysicalCountLinesFromWhere(cmsql As SqlCommand, location As String) As String

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT ISNULL(Loc_Type3, '') FROM dbo.STK04_Location WHERE Loc_Code = @Location "
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim

        Dim stkFil As String = IIf(cmsql.ExecuteScalar()?.ToString.Trim.ToUpper = "S", "dbo.POS_STKFIL", "dbo.STK04_STKFIL").ToString

        Return $" FROM    dbo.tbl_PhysicalCountDetail
                          INNER JOIN {stkFil} ON Pcd_stkid = STK_RECID
                          INNER JOIN dbo.stk04_itemtype ON STK_ITMTYPE = IT_Code
                          INNER JOIN dbo.stk04_supplier ON STK_SUPPLIER = Sup_Code
                          LEFT JOIN dbo.Stk04_COLOR ON Color_Code = Pcd_Color
                          {IIf(PhysicalCountHasBooks(cmsql),
                               "OUTER APPLY ( SELECT TOP 1 BO_Title BookTitle FROM dbo.Bibl1_Book WHERE BO_StkRecId = Pcd_stkid ORDER BY BO_Recid ) Book",
                               "OUTER APPLY ( SELECT CAST(NULL AS NVARCHAR(160)) BookTitle ) Book")}
                          LEFT JOIN dbo.STK04_GRID ON G_Code = Pcd_Grid
                          LEFT JOIN dbo.stk04_DetGRID ON GD_Code = Pcd_Grid
                                                         AND CONVERT(VARCHAR(5), GD_Order) = Pcd_DetGrid
                 WHERE   Pcd_HRecId = @CountNumber
                          AND Pcd_Loc = @Location "

    End Function

    Private Function PhysicalCountHasBooks(cmsql As SqlCommand) As Boolean

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT OBJECT_ID('dbo.Bibl1_Book') "
        Return Not IsDBNull(cmsql.ExecuteScalar())

    End Function

    Private Function PhysicalCountBarcodesFromWhere() As String

        Return " FROM    dbo.tbl_PhysicalCountDetBarcode
                 WHERE   PcBarc_LOC = @Location
                         AND PcBarc_DRecId IN ( SELECT Pcd_RecId
                                                FROM   dbo.tbl_PhysicalCountDetail
                                                WHERE  Pcd_HRecId = @CountNumber
                                                       AND Pcd_Loc = @Location ) "

    End Function

    'Same rule as the SOAP service : the available qty is hidden when a user has mnuOperCountManModify without the show available qty exception
    'Negations of the portable (tbl_CPP_NegationButton, Negations tab of the backend) on the Physical Count (P1)
    Private Const PhysicalCountStep As String = "P1"
    Private Const PhysicalCountButtonAvailableQty As String = "btnShowAvailableQty"
    Private Const PhysicalCountButtonListDifferences As String = "btnListDifferences"

    'True when the user can do the action of the button on the company of the location (NB_HasNeg = 1)
    'No row = not allowed, the super user SU is always allowed
    Private Function PhysicalCountNegation(cmsql As SqlCommand, location As String, userCode As String, button As String) As Boolean

        If String.IsNullOrWhiteSpace(userCode) Then Return False
        If userCode.Trim.ToUpper = "SU" Then Return True

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT TOP 1 1
                              FROM   dbo.tbl_CPP_NegationButton
                              WHERE  NB_UserCode = @UserCode
                                     AND NB_Comp = ( SELECT TOP 1 Loc_CoCode FROM dbo.STK04_Location WHERE Loc_Code = @Location )
                                     AND NB_StepCode = @StepCode
                                     AND NB_Button = @Button
                                     AND NB_HasNeg = 1 "
        cmsql.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = userCode.Trim
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
        cmsql.Parameters.Add("@StepCode", SqlDbType.VarChar, 10).Value = PhysicalCountStep
        cmsql.Parameters.Add("@Button", SqlDbType.VarChar, 50).Value = button

        Dim result As Object = cmsql.ExecuteScalar()
        cmsql.Parameters.Clear()
        Return result IsNot Nothing

    End Function

    'The negations of the logged in user on the count of the location : {"showAvailableQty":"Y","showListDifferences":"N"}
    'Asked again by the device when the user changes, so a count downloaded by another user follows the user logged in
    Public Function GetPhysicalCountNegations(location As String, userCode As String) As String Implements IDataService.GetPhysicalCountNegations

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    Dim showAvailableQty As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonAvailableQty)
                    Dim showListDifferences As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonListDifferences)

                    Return $"{{""showAvailableQty"":""{IIf(showAvailableQty, "Y", "N")}"",""showListDifferences"":""{IIf(showListDifferences, "Y", "N")}""}}"

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Private Sub AddPhysicalCountParameters(cmsql As SqlCommand, location As String, countNumber As String)

        cmsql.Parameters.Clear()
        cmsql.Parameters.Add("@CountNumber", SqlDbType.Decimal).Value = CDec(countNumber.Trim)
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim

    End Sub

    Public Function GetPhysicalCountHeader(location As String, countNumber As String, machineName As String, userCode As String) As String Implements IDataService.GetPhysicalCountHeader

        Try

            Dim errorMessage As String = ValidatePhysicalCountRequest(location, countNumber, machineName)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    Dim slot As Integer
                    errorMessage = GetPhysicalCountSlot(cmsql, location, countNumber, machineName, slot)
                    If errorMessage <> "" Then Return "Error " & errorMessage

                    Dim showAvailableQty As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonAvailableQty)
                    Dim showListDifferences As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonListDifferences)

                    Dim linesFromWhere As String = PhysicalCountLinesFromWhere(cmsql, location)

                    AddPhysicalCountParameters(cmsql, location, countNumber)
                    cmsql.Parameters.Add("@Slot", SqlDbType.Int).Value = slot
                    cmsql.Parameters.Add("@ShowAvailableQty", SqlDbType.VarChar, 1).Value = IIf(showAvailableQty, "Y", "N")
                    cmsql.Parameters.Add("@ShowListDifferences", SqlDbType.VarChar, 1).Value = IIf(showListDifferences, "Y", "N")
                    cmsql.CommandText = $" SELECT  CONVERT(VARCHAR(18), Pch_Recid) countNumber ,
                                                   Pch_Loc location ,
                                                   ISNULL(Loc_Desc, '') locationDescription ,
                                                   CASE Pch_Whouse WHEN '1' THEN 'wh1' ELSE 'wh2' END warehouse ,
                                                   ISNULL(CONVERT(VARCHAR(10), Pch_DateCount, 103), '') dateCount ,
                                                   ISNULL(Pch_NbrItems, 0) nbrItems ,
                                                   @Slot userToUpdate ,
                                                   ( SELECT TOP 1 ISNULL(CONVERT(VARCHAR, SP_DecNo), '0') FROM dbo.stk04_SysParam ) decimals ,
                                                   ( SELECT TOP 1 ISNULL(SP_Trading, 'N') FROM dbo.stk04_SysParam ) tradingMode ,
                                                   ( SELECT TOP 1 ISNULL(SP_Fashion, 'N') FROM dbo.stk04_SysParam ) fashionMode ,
                                                   ( SELECT TOP 1 ISNULL(SP_Expiry, 'N') FROM dbo.stk04_SysParam ) expiryMode ,
                                                   ( SELECT TOP 1 ISNULL(SP_Book, 'N') FROM dbo.stk04_SysParam ) bookMode ,
                                                   @ShowAvailableQty showAvailableQty ,
                                                   @ShowListDifferences showListDifferences ,
                                                   ( SELECT COUNT(*) {linesFromWhere} ) linesCount ,
                                                   ( SELECT COUNT(*) {PhysicalCountBarcodesFromWhere()} ) barcodesCount
                                           FROM    dbo.tbl_PhysicalCountHeader
                                                   LEFT JOIN dbo.STK04_Location ON Loc_Code = Pch_Loc
                                           WHERE   Pch_RecId = @CountNumber
                                                   AND Pch_Loc = @Location "

                    Using dtHeader As New DataTable

                        Using daSql As New SqlDataAdapter(cmsql)
                            daSql.Fill(dtHeader)
                        End Using

                        Return GetJson(dtHeader)

                    End Using

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Used by the settings of the device to test the server of a location : returns the location description
    Public Function CheckPhysicalCountLocation(location As String) As String Implements IDataService.CheckPhysicalCountLocation

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" SELECT ISNULL(Loc_Desc, '') FROM dbo.STK04_Location WHERE Loc_Code = @Location ", cnsql)

                    cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim

                    Dim locationDescription As Object = cmsql.ExecuteScalar()

                    If locationDescription Is Nothing Then Return $"Error Location {location.Trim} Does Not Exist On This Server"

                    Return locationDescription.ToString

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'startFromRow : 1 based, rows are numbered by Pcd_RecId
    Public Function GetPhysicalCountDetails(location As String, countNumber As String, machineName As String, startFromRow As Integer, pageSize As Integer, userCode As String) As String Implements IDataService.GetPhysicalCountDetails

        Try

            Dim errorMessage As String = ValidatePhysicalCountRequest(location, countNumber, machineName)
            If errorMessage <> "" Then Return "Error " & errorMessage

            If startFromRow < 1 Then Return "Error Invalid Start Row"
            If pageSize < 1 OrElse pageSize > PhysicalCountMaxPageSize Then Return $"Error Page Size Should Be Between 1 And {PhysicalCountMaxPageSize}"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    Dim slot As Integer
                    errorMessage = GetPhysicalCountSlot(cmsql, location, countNumber, machineName, slot)
                    If errorMessage <> "" Then Return "Error " & errorMessage

                    'Without the negations the available qty is not sent at all (List Differences needs it too)
                    Dim showAvailableQty As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonAvailableQty) OrElse
                                                      PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonListDifferences)

                    'slot is 1 to 6 (checked above), the column name cannot be a parameter
                    Dim countedColumn As String = "Pcd_QtyCounted" & slot.ToString

                    Dim linesFromWhere As String = PhysicalCountLinesFromWhere(cmsql, location)

                    AddPhysicalCountParameters(cmsql, location, countNumber)
                    cmsql.Parameters.Add("@FromRow", SqlDbType.Int).Value = startFromRow
                    cmsql.Parameters.Add("@ToRow", SqlDbType.Int).Value = startFromRow + pageSize - 1
                    cmsql.CommandText = $" WITH Lines AS
                                           ( SELECT  ROW_NUMBER() OVER ( ORDER BY Pcd_RecId ) rowNumber ,
                                                     CONVERT(VARCHAR(18), Pcd_RecId) lineRecID ,
                                                     CONVERT(VARCHAR(18), Pcd_stkid) itemRecID ,
                                                     ISNULL(STK_REF, '') itemReference ,
                                                     ISNULL(STK_DESC, '') itemDescription ,
                                                     ISNULL(Pcd_Color, '') colorCode ,
                                                     ISNULL(Color_Desc, '') colorDescription ,
                                                     ISNULL(Pcd_Grid, '') gridCode ,
                                                     ISNULL(G_Desc, '') gridDescription ,
                                                     ISNULL(Pcd_DetGrid, '') sizeOrder ,
                                                     ISNULL(GD_SizeDesc, '') sizeDescription ,
                                                     ISNULL(BookTitle, '') bookTitle ,
                                                     CASE WHEN SUBSTRING(Pcd_ExpDate, 0, 3) = '00' THEN SUBSTRING(Pcd_ExpDate, 4, 8)
                                                          ELSE ISNULL(Pcd_ExpDate, '')
                                                     END expiry ,
                                                     ISNULL(Pcd_Batch, '') batch ,
                                                     LTRIM(RTRIM(ISNULL(STK_UNIT, ''))) itemUnit ,
                                                     {IIf(showAvailableQty, "ISNULL(Pcd_Qty, 0) systemQty ,", "")}
                                                     ISNULL({countedColumn}, 0) counted
                                             {linesFromWhere} )
                                           SELECT   *
                                           FROM     Lines
                                           WHERE    rowNumber BETWEEN @FromRow AND @ToRow
                                           ORDER BY rowNumber "

                    Using dtLines As New DataTable

                        Using daSql As New SqlDataAdapter(cmsql)
                            daSql.Fill(dtLines)
                        End Using

                        If dtLines.Rows.Count = 0 Then
                            Return ""
                        Else
                            Return GetJson(dtLines)
                        End If

                    End Using

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'startFromRow : 1 based, rows are numbered by PcBarc_RecId
    Public Function GetPhysicalCountBarcodes(location As String, countNumber As String, machineName As String, startFromRow As Integer, pageSize As Integer) As String Implements IDataService.GetPhysicalCountBarcodes

        Try

            Dim errorMessage As String = ValidatePhysicalCountRequest(location, countNumber, machineName)
            If errorMessage <> "" Then Return "Error " & errorMessage

            If startFromRow < 1 Then Return "Error Invalid Start Row"
            If pageSize < 1 OrElse pageSize > PhysicalCountMaxPageSize Then Return $"Error Page Size Should Be Between 1 And {PhysicalCountMaxPageSize}"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    Dim slot As Integer
                    errorMessage = GetPhysicalCountSlot(cmsql, location, countNumber, machineName, slot)
                    If errorMessage <> "" Then Return "Error " & errorMessage

                    AddPhysicalCountParameters(cmsql, location, countNumber)
                    cmsql.Parameters.Add("@FromRow", SqlDbType.Int).Value = startFromRow
                    cmsql.Parameters.Add("@ToRow", SqlDbType.Int).Value = startFromRow + pageSize - 1
                    cmsql.CommandText = $" WITH Barcodes AS
                                           ( SELECT  ROW_NUMBER() OVER ( ORDER BY PcBarc_RecId ) rowNumber ,
                                                     LTRIM(RTRIM(PcBarc_Barcode)) barcode ,
                                                     CONVERT(VARCHAR(18), PcBarc_DRecId) lineRecID
                                             {PhysicalCountBarcodesFromWhere()} )
                                           SELECT   *
                                           FROM     Barcodes
                                           WHERE    rowNumber BETWEEN @FromRow AND @ToRow
                                           ORDER BY rowNumber "

                    Using dtBarcodes As New DataTable

                        Using daSql As New SqlDataAdapter(cmsql)
                            daSql.Fill(dtBarcodes)
                        End Using

                        If dtBarcodes.Rows.Count = 0 Then
                            Return ""
                        Else
                            Return GetJson(dtBarcodes)
                        End If

                    End Using

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'lines : JSON array [{"lineRecID":"47842","counted":7}, ...] - counted is the absolute total of the machine (replaces the stored value)
    'All the lines are saved or none
    Public Function SavePhysicalCountLines(location As String, countNumber As String, machineName As String, lines As String) As String Implements IDataService.SavePhysicalCountLines

        Try

            Dim errorMessage As String = ValidatePhysicalCountRequest(location, countNumber, machineName)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim arrLines As JArray = JArray.Parse(lines)

            If arrLines.Count = 0 Then Return "Error No Lines To Save"
            If arrLines.Count > PhysicalCountMaxLinesPerSave Then Return $"Error Maximum {PhysicalCountMaxLinesPerSave} Lines Per Save"

            'Check every line before touching the database (a line sent twice keeps its last total)
            Dim lineValues As New Dictionary(Of Decimal, Decimal)

            For Each line As JObject In arrLines

                Dim lineRecID As Decimal
                Dim counted As Decimal

                If Not Decimal.TryParse(line("lineRecID")?.ToString, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, lineRecID) Then
                    Return $"Error Invalid Line {line("lineRecID")}"
                End If

                If Not Decimal.TryParse(line("counted")?.ToString, Globalization.NumberStyles.AllowDecimalPoint, Globalization.CultureInfo.InvariantCulture, counted) _
                    OrElse counted > 99999999999999.999D Then
                    Return $"Error Invalid Counted Qty For Line {lineRecID}"
                End If

                lineValues(lineRecID) = Math.Round(counted, 3)

            Next

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction = Nothing
                Try

                    cnsql.Open()
                    trnSql = cnsql.BeginTransaction

                    Using cmsql As New SqlCommand(" ", cnsql, trnSql)

                        cmsql.CommandTimeout = 900

                        'The header stays locked until the commit : the count cannot be posted or cancelled meanwhile
                        Dim slot As Integer
                        errorMessage = GetPhysicalCountSlot(cmsql, location, countNumber, machineName, slot, True)
                        If errorMessage <> "" Then
                            trnSql.Rollback()
                            Return "Error " & errorMessage
                        End If

                        'All the lines go to a temp table in one bulk copy
                        cmsql.Parameters.Clear()
                        cmsql.CommandText = " CREATE TABLE #PhysicalCountLines ( LineRecID DECIMAL(18, 0) PRIMARY KEY, Counted DECIMAL(18, 3) NOT NULL ) "
                        cmsql.ExecuteNonQuery()

                        Using dtLines As New DataTable
                            dtLines.Columns.Add("LineRecID", GetType(Decimal))
                            dtLines.Columns.Add("Counted", GetType(Decimal))
                            For Each lineValue In lineValues
                                dtLines.Rows.Add(lineValue.Key, lineValue.Value)
                            Next

                            Using bulkCopy As New SqlBulkCopy(cnsql, SqlBulkCopyOptions.Default, trnSql)
                                bulkCopy.DestinationTableName = "#PhysicalCountLines"
                                bulkCopy.BulkCopyTimeout = 900
                                bulkCopy.WriteToServer(dtLines)
                            End Using
                        End Using

                        'Every line must belong to this count
                        AddPhysicalCountParameters(cmsql, location, countNumber)
                        cmsql.CommandText = " SELECT TOP 20 CONVERT(VARCHAR(18), LineRecID)
                                              FROM   #PhysicalCountLines
                                              WHERE  NOT EXISTS ( SELECT 1
                                                                  FROM   dbo.tbl_PhysicalCountDetail
                                                                  WHERE  Pcd_RecId = LineRecID
                                                                         AND Pcd_Loc = @Location
                                                                         AND Pcd_HRecId = @CountNumber ) "

                        Dim missingLines As New List(Of String)
                        Using drMissing As SqlDataReader = cmsql.ExecuteReader()
                            While drMissing.Read()
                                missingLines.Add(drMissing.GetString(0))
                            End While
                        End Using

                        If missingLines.Count > 0 Then
                            trnSql.Rollback()
                            Return "Error Lines Not Found In This Physical Count : " & String.Join(", ", missingLines)
                        End If

                        'One UPDATE for all the lines, slot is 1 to 6 (checked above), the column names cannot be parameters
                        cmsql.CommandText = $" UPDATE  Detail
                                               SET     Pcd_QtyCounted{slot} = Lines.Counted ,
                                                       Pcd_Type{slot} = '10' ,
                                                       Pcd_DateCount = GETDATE()
                                               FROM    dbo.tbl_PhysicalCountDetail Detail
                                                       INNER JOIN #PhysicalCountLines Lines ON Lines.LineRecID = Detail.Pcd_RecId
                                               WHERE   Detail.Pcd_Loc = @Location
                                                       AND Detail.Pcd_HRecId = @CountNumber "
                        cmsql.ExecuteNonQuery()

                        cmsql.Parameters.Clear()
                        cmsql.CommandText = " DROP TABLE #PhysicalCountLines "
                        cmsql.ExecuteNonQuery()

                    End Using

                    trnSql.Commit()

                Catch exsv As Exception
                    Try
                        trnSql?.Rollback()
                    Catch exTRN As Exception
                    End Try
                    Throw
                End Try

            End Using

            Return "Success|" & lineValues.Count.ToString

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'logs : JSON array of the counting steps saved on the device (tbl_CPP_PhysicalCountLogs), sent after the lines :
    '[{"logID":12,"lineRecID":"47842","itemRecID":"1203","qty":5,"total":17,"mode":"scanThenType","action":"add","unit":"PCS","userCode":"SU","dateStamp":"2026-10-28 09:15:02"}, ...]
    'The count is not checked (a closed count keeps its history), a log already received (same machine, location, count, logID) is skipped
    'Returns "Success|n" (n : the logs received)
    Public Function SavePhysicalCountLogs(location As String, countNumber As String, machineName As String, logs As String) As String Implements IDataService.SavePhysicalCountLogs

        Try

            Dim errorMessage As String = ValidatePhysicalCountRequest(location, countNumber, machineName)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim arrLogs As JArray = JArray.Parse(logs)

            If arrLogs.Count = 0 Then Return "Success|0"
            If arrLogs.Count > PhysicalCountMaxLinesPerSave Then Return $"Error Maximum {PhysicalCountMaxLinesPerSave} Logs Per Save"

            Using dtLogs As New DataTable

                dtLogs.Columns.Add("DeviceLogID", GetType(Integer))
                dtLogs.Columns.Add("LineRecID", GetType(Decimal))
                dtLogs.Columns.Add("StkID", GetType(Decimal))
                dtLogs.Columns.Add("Qty", GetType(Decimal))
                dtLogs.Columns.Add("Total", GetType(Decimal))
                dtLogs.Columns.Add("Mode", GetType(String))
                dtLogs.Columns.Add("Action", GetType(String))
                dtLogs.Columns.Add("Unit", GetType(String))
                dtLogs.Columns.Add("UStamp", GetType(String))
                dtLogs.Columns.Add("DStamp", GetType(DateTime))

                Dim logIDs As New HashSet(Of Integer)

                For Each log As JObject In arrLogs

                    Dim logID As Integer
                    Dim lineRecID, itemRecID, qty, total As Decimal
                    Dim dateStamp As DateTime

                    If Not Integer.TryParse(log("logID")?.ToString, logID) Then Return $"Error Invalid Log {log("logID")}"
                    If Not logIDs.Add(logID) Then Continue For

                    If Not Decimal.TryParse(log("lineRecID")?.ToString, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, lineRecID) Then
                        Return $"Error Invalid Line For Log {logID}"
                    End If

                    If Not Decimal.TryParse(log("qty")?.ToString, Globalization.NumberStyles.AllowDecimalPoint Or Globalization.NumberStyles.AllowLeadingSign, Globalization.CultureInfo.InvariantCulture, qty) _
                        OrElse Math.Abs(qty) > 99999999999999.999D Then
                        Return $"Error Invalid Qty For Log {logID}"
                    End If

                    If Not DateTime.TryParseExact(log("dateStamp")?.ToString, "yyyy-MM-dd HH:mm:ss", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, dateStamp) Then
                        Return $"Error Invalid Date For Log {logID}"
                    End If

                    Dim row As DataRow = dtLogs.NewRow
                    row("DeviceLogID") = logID
                    row("LineRecID") = lineRecID
                    row("StkID") = If(Decimal.TryParse(log("itemRecID")?.ToString, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, itemRecID), CObj(itemRecID), DBNull.Value)
                    row("Qty") = Math.Round(qty, 3)
                    row("Total") = If(Decimal.TryParse(log("total")?.ToString, Globalization.NumberStyles.AllowDecimalPoint Or Globalization.NumberStyles.AllowLeadingSign, Globalization.CultureInfo.InvariantCulture, total) _
                                      AndAlso Math.Abs(total) <= 99999999999999.999D, CObj(Math.Round(total, 3)), DBNull.Value)
                    row("Mode") = Left(If(log("mode")?.ToString, "").Trim, 20)
                    row("Action") = Left(If(log("action")?.ToString, "").Trim, 10)
                    row("Unit") = Left(If(log("unit")?.ToString, "").Trim, 30)
                    row("UStamp") = Left(If(log("userCode")?.ToString, "").Trim, 4)
                    row("DStamp") = dateStamp
                    dtLogs.Rows.Add(row)

                Next

                Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                    Dim trnSql As SqlTransaction = Nothing
                    Try

                        cnsql.Open()
                        trnSql = cnsql.BeginTransaction

                        Using cmsql As New SqlCommand(" CREATE TABLE #PhysicalCountLogs ( DeviceLogID INT PRIMARY KEY, LineRecID DECIMAL(18, 0) NOT NULL, StkID DECIMAL(18, 0) NULL,
                                                                                          Qty DECIMAL(18, 3) NOT NULL, Total DECIMAL(18, 3) NULL, Mode VARCHAR(20) NOT NULL,
                                                                                          Action VARCHAR(10) NOT NULL, Unit NVARCHAR(30) NULL, UStamp VARCHAR(4) NULL, DStamp DATETIME NOT NULL ) ",
                                                      cnsql, trnSql)

                            cmsql.CommandTimeout = 900
                            cmsql.ExecuteNonQuery()

                            Using bulkCopy As New SqlBulkCopy(cnsql, SqlBulkCopyOptions.Default, trnSql)
                                bulkCopy.DestinationTableName = "#PhysicalCountLogs"
                                bulkCopy.BulkCopyTimeout = 900
                                bulkCopy.WriteToServer(dtLogs)
                            End Using

                            AddPhysicalCountParameters(cmsql, location, countNumber)
                            cmsql.Parameters.Add("@MachineName", SqlDbType.NVarChar, 30).Value = machineName.Trim
                            cmsql.CommandText = " INSERT INTO dbo.tbl_CPP_PhysicalCountLogs ( PCL_CountID , PCL_Location , PCL_LineRecID , PCL_StkID , PCL_Operator , PCL_Qty , PCL_Total ,
                                                                                         PCL_Mode , PCL_Action , PCL_Unit , PCL_DeviceLogID , PCL_UStamp , PCL_DStamp )
                                                  SELECT  @CountNumber , @Location , LineRecID , StkID , @MachineName , Qty , Total ,
                                                          Mode , Action , NULLIF(Unit, '') , DeviceLogID , NULLIF(UStamp, '') , DStamp
                                                  FROM    #PhysicalCountLogs Logs
                                                  WHERE   NOT EXISTS ( SELECT 1
                                                                       FROM   dbo.tbl_CPP_PhysicalCountLogs
                                                                       WHERE  PCL_Operator = @MachineName
                                                                              AND PCL_Location = @Location
                                                                              AND PCL_CountID = @CountNumber
                                                                              AND PCL_DeviceLogID = Logs.DeviceLogID ) ;
                                                  DROP TABLE #PhysicalCountLogs ; "
                            cmsql.ExecuteNonQuery()

                        End Using

                        trnSql.Commit()

                    Catch exsv As Exception
                        Try
                            trnSql?.Rollback()
                        Catch exTRN As Exception
                        End Try
                        Throw
                    End Try

                End Using

                Return "Success|" & dtLogs.Rows.Count.ToString

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Physical Count - Create"

    'Creates a count from the device, same rules as the count module (frmSelectStkItems : Show then Save) :
    '- items : active (STK_ACTIVE = 'Y'), not related to a consignment item (V_StockNormalCons), with a product group, an identification and a supplier
    '- filters : books only / no books, not counted between two dates (in an open count of the location)
    '- brand, category, product group, identification : the items of the selected codes are checked (OR between them, like the column headers of the module)
    '- one line per variant of the item (STK_QTY of location 00), the available qty of the location, the barcodes of each variant
    '- the machine of the device is the only machine of the count (Pch_MachUser1), warehouse 1, count date = working day of the location
    'The checks of the module before saving (pending transfers, reserved qty, HO sync) are not done for now

    Private Const PhysicalCountMaxItemsPageSize As Integer = 200
    Private Const PhysicalCountMaxFilterCodes As Integer = 1000
    Private Const PhysicalCountMaxSelectionItems As Integer = 200000

    'filters : {"books":"all|books|noBooks","notCounted":null|{"from":"dd/MM/yyyy","to":"dd/MM/yyyy"},"brands":[],"categories":[],"productGroups":[],"identifications":[]}
    Private Class PhysicalCountCreateFilters
        Public Books As String = "all"
        Public NotCounted As Boolean = False
        Public NotCountedFrom As Date? = Nothing
        Public NotCountedTo As Date? = Nothing
        Public Brands As New List(Of String)
        Public Categories As New List(Of String)
        Public ProductGroups As New List(Of String)
        Public Identifications As New List(Of String)
    End Class

    'selection : {"all":false,"groups":true,"include":["itemRecID",...],"exclude":["itemRecID",...]}
    'An item is selected when it is not excluded and (all, or in a checked brand / category / product group / identification, or included)
    'groups = false : the checked codes do not select their items anymore (Uncheck All)
    Private Class PhysicalCountCreateSelection
        Public All As Boolean = False
        Public Groups As Boolean = True
        Public Include As New List(Of Decimal)
        Public Exclude As New List(Of Decimal)
    End Class

    'Returns "" when valid, else the error message
    Private Function ParsePhysicalCountFilters(filters As String, ByRef result As PhysicalCountCreateFilters) As String

        result = New PhysicalCountCreateFilters

        If String.IsNullOrWhiteSpace(filters) Then Return ""

        Dim objFilters As JObject = JObject.Parse(filters)

        Dim books As String = If(objFilters("books")?.ToString, "all").Trim
        If books <> "all" AndAlso books <> "books" AndAlso books <> "noBooks" Then Return "Invalid Books Filter"
        result.Books = books

        Dim notCounted As JToken = objFilters("notCounted")
        If notCounted IsNot Nothing AndAlso notCounted.Type = JTokenType.Object Then
            result.NotCounted = True
            For Each part In {"from", "to"}
                Dim text As String = If(notCounted(part)?.ToString, "").Trim
                If text = "" Then Continue For
                Dim value As Date
                If Not Date.TryParseExact(text, "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, value) Then
                    Return $"Invalid Date {text}"
                End If
                If part = "from" Then result.NotCountedFrom = value Else result.NotCountedTo = value
            Next
        End If

        Dim lists As New Dictionary(Of String, List(Of String)) From {
            {"brands", result.Brands}, {"categories", result.Categories}, {"productGroups", result.ProductGroups}, {"identifications", result.Identifications}}

        For Each list In lists
            Dim codes As JToken = objFilters(list.Key)
            If codes Is Nothing OrElse codes.Type <> JTokenType.Array Then Continue For
            If codes.Count > PhysicalCountMaxFilterCodes Then Return $"Too Many Codes In {list.Key}"
            For Each code As JToken In codes
                Dim text As String = code.ToString.Trim
                If text = "" OrElse text.Length > 10 Then Return $"Invalid Code In {list.Key}"
                If Not list.Value.Contains(text) Then list.Value.Add(text)
            Next
        Next

        Return ""

    End Function

    'Returns "" when valid, else the error message
    Private Function ParsePhysicalCountSelection(selection As String, ByRef result As PhysicalCountCreateSelection) As String

        result = New PhysicalCountCreateSelection

        If String.IsNullOrWhiteSpace(selection) Then Return ""

        Dim objSelection As JObject = JObject.Parse(selection)

        result.All = objSelection("all") IsNot Nothing AndAlso objSelection("all").Type = JTokenType.Boolean AndAlso CBool(objSelection("all"))
        result.Groups = objSelection("groups") Is Nothing OrElse objSelection("groups").Type <> JTokenType.Boolean OrElse CBool(objSelection("groups"))

        For Each part In {"include", "exclude"}
            Dim ids As JToken = objSelection(part)
            If ids Is Nothing OrElse ids.Type <> JTokenType.Array Then Continue For
            If ids.Count > PhysicalCountMaxSelectionItems Then Return $"Maximum {PhysicalCountMaxSelectionItems} Items"
            Dim list As List(Of Decimal) = If(part = "include", result.Include, result.Exclude)
            For Each id As JToken In ids
                Dim value As Decimal
                If Not Decimal.TryParse(id.ToString, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, value) Then Return $"Invalid Item {id}"
                list.Add(value)
            Next
        Next

        Return ""

    End Function

    'Head office : STK04 tables, stand alone location (Loc_Type3 = 'S') : POS tables, like the count module (ClocHO)
    Private Sub PhysicalCountStockTables(cmsql As SqlCommand, location As String, ByRef stkFil As String, ByRef stkQty As String)

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT ISNULL(Loc_Type3, '') FROM dbo.STK04_Location WHERE Loc_Code = @Location "
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim

        Dim standAlone As Boolean = cmsql.ExecuteScalar()?.ToString.Trim.ToUpper = "S"

        stkFil = IIf(standAlone, "dbo.POS_STKFIL", "dbo.STK04_STKFIL").ToString
        stkQty = IIf(standAlone, "dbo.POS_STKQTY", "dbo.STK04_STKQTY").ToString

    End Sub

    'Working day of the location (last opened day of the POS), today when the location has none
    Private Function PhysicalCountWorkingDay(cmsql As SqlCommand, location As String) As Date

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT ISNULL(( SELECT MAX(CT_OPDATE) FROM dbo.POS_CASHTRANSACTION WHERE CT_LOCATION = @Location AND CT_RECID = 'O' ), CONVERT(DATE, GETDATE())) "
        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim

        Return CDate(cmsql.ExecuteScalar()).Date

    End Function

    Private Function PhysicalCountObjectExists(cmsql As SqlCommand, objectName As String) As Boolean

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SELECT OBJECT_ID(@ObjectName) "
        cmsql.Parameters.Add("@ObjectName", SqlDbType.NVarChar, 256).Value = objectName
        Return Not IsDBNull(cmsql.ExecuteScalar())

    End Function

    'Included and excluded items in #PhysicalCountInclude / #PhysicalCountExclude (one bulk copy each)
    Private Sub LoadPhysicalCountSelection(cnsql As SqlConnection, trnSql As SqlTransaction, cmsql As SqlCommand, selection As PhysicalCountCreateSelection)

        cmsql.Parameters.Clear()
        cmsql.CommandText = " CREATE TABLE #PhysicalCountInclude ( StkID DECIMAL(18, 0) PRIMARY KEY )
                              CREATE TABLE #PhysicalCountExclude ( StkID DECIMAL(18, 0) PRIMARY KEY ) "
        cmsql.ExecuteNonQuery()

        For Each part In {"#PhysicalCountInclude", "#PhysicalCountExclude"}

            Dim ids As List(Of Decimal) = If(part = "#PhysicalCountInclude", selection.Include, selection.Exclude)
            If ids.Count = 0 Then Continue For

            Using dtIds As New DataTable
                dtIds.Columns.Add("StkID", GetType(Decimal))
                For Each id In ids.Distinct
                    dtIds.Rows.Add(id)
                Next
                Using bulkCopy As New SqlBulkCopy(cnsql, SqlBulkCopyOptions.Default, trnSql)
                    bulkCopy.DestinationTableName = part
                    bulkCopy.BulkCopyTimeout = 900
                    bulkCopy.WriteToServer(dtIds)
                End Using
            End Using

        Next

    End Sub

    'search : part of the reference, description, supplier, brand, category or product group, or a whole barcode / single reference ("" = no search)
    'exactMatch : a scan, only the item of this barcode, reference or single reference
    'The QR of a label (link ending with the record id, see LabelQRLink) : the item itself, scanned or typed
    'Adds @Search and @SearchLike, returns the condition ("" without search)
    Private Function PhysicalCountSearchWhere(cmsql As SqlCommand, search As String, Optional exactMatch As Boolean = False) As String

        If search = "" Then Return ""

        Dim linkRecID As String = ItemRecIDFromLink(search)
        If linkRecID <> "" Then
            cmsql.Parameters.Add("@LinkRecID", SqlDbType.Decimal).Value = CDec(linkRecID)
            Return " AND Item.STK_RECID = @LinkRecID "
        End If

        If exactMatch Then
            cmsql.Parameters.Add("@Search", SqlDbType.NVarChar, 50).Value = search
            Return " AND ( Item.STK_REF = @Search
                           OR EXISTS ( SELECT 1 FROM dbo.STK04_STKFIL Ref WHERE Ref.STK_RECID = Item.STK_RECID AND ( Ref.STK_SingleRef1 = @Search OR Ref.STK_SingleRef2 = @Search ) )
                           OR EXISTS ( SELECT 1 FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Item.STK_RECID AND dbar_barcode = @Search ) ) "
        End If

        'LIKE wildcards typed by the user are searched as text
        cmsql.Parameters.Add("@Search", SqlDbType.NVarChar, 50).Value = search
        cmsql.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 160).Value = "%" & search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%"

        Return " AND ( Item.STK_REF LIKE @SearchLike
                       OR Item.STK_DESC LIKE @SearchLike
                       OR Sup_Name LIKE @SearchLike
                       OR br_Name LIKE @SearchLike
                       OR Cat_Desc LIKE @SearchLike
                       OR IT_Desc LIKE @SearchLike
                       OR EXISTS ( SELECT 1 FROM dbo.STK04_STKFIL Ref WHERE Ref.STK_RECID = Item.STK_RECID AND ( Ref.STK_SingleRef1 = @Search OR Ref.STK_SingleRef2 = @Search ) )
                       OR EXISTS ( SELECT 1 FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Item.STK_RECID AND dbar_barcode = @Search ) ) "

    End Function

    'The lists to choose the items (filters, items) do not wait for the programs that are updating the stock at the same time
    '(POS sync...) : reading their uncommitted rows is fine to choose items. The count itself is created with committed reads
    Private Sub PhysicalCountReadWithoutWaiting(cmsql As SqlCommand)

        cmsql.Parameters.Clear()
        cmsql.CommandText = " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED "
        cmsql.ExecuteNonQuery()

    End Sub

    'Back to the default before the connection goes back to the pool (older SQL Server versions keep the isolation level in the pool)
    Private Sub PhysicalCountReadCommitted(cnsql As SqlConnection)

        Try
            If cnsql.State <> ConnectionState.Open Then Return
            Using cmReset As New SqlCommand(" SET TRANSACTION ISOLATION LEVEL READ COMMITTED SET LOCK_TIMEOUT -1 ", cnsql)
                cmReset.ExecuteNonQuery()
            End Using
        Catch ex As Exception
        End Try

    End Sub

    'Fills #PhysicalCountBase with the items that can be counted (one row per item, only the keys : a narrow table needs little memory)
    'and their selected flag, to create the count
    'search : part of the reference, description, supplier, brand, category or product group, or a whole barcode / single reference ("" = no search)
    'onlySelected : only the selected items
    'The temp tables are created by commands without parameters : a temp table created by a command with parameters (sp_executesql)
    'is dropped at the end of that command
    Private Sub LoadPhysicalCountBase(cnsql As SqlConnection, trnSql As SqlTransaction, cmsql As SqlCommand, location As String, stkFil As String,
                                      filters As PhysicalCountCreateFilters, selection As PhysicalCountCreateSelection, search As String, onlySelected As Boolean)

        LoadPhysicalCountSelection(cnsql, trnSql, cmsql, selection)

        'The consignment view is read once, not for every item
        Dim hasConsignment As Boolean = PhysicalCountObjectExists(cmsql, "dbo.V_StockNormalCons")

        cmsql.Parameters.Clear()
        cmsql.CommandText = " CREATE TABLE #PhysicalCountCons ( StkID DECIMAL(18, 0) PRIMARY KEY )
                              CREATE TABLE #PhysicalCountBase ( StkID DECIMAL(18, 0) NOT NULL ,
                                                                Reference NVARCHAR(200) NOT NULL ,
                                                                Selected BIT NOT NULL ) " &
                              IIf(hasConsignment, " INSERT INTO #PhysicalCountCons ( StkID )
                                                    SELECT DISTINCT NSTK_RECID FROM dbo.V_StockNormalCons WHERE NSTK_RECID IS NOT NULL ", "").ToString
        cmsql.ExecuteNonQuery()

        Dim fromWhere As String = "", selectedExpression As String = ""
        cmsql.Parameters.Clear()
        PhysicalCountItemsQuery(cmsql, location, stkFil, filters, selection, IIf(hasConsignment, "temp", "").ToString, fromWhere, selectedExpression)

        Dim moreWhere As String = PhysicalCountSearchWhere(cmsql, search)
        If onlySelected Then moreWhere &= $" AND {selectedExpression} = 1 "

        cmsql.CommandText = $" INSERT INTO #PhysicalCountBase ( StkID, Reference, Selected )
                               SELECT  Item.STK_RECID ,
                                       ISNULL(Item.STK_REF, '') ,
                                       {selectedExpression}
                               {fromWhere} {moreWhere} "
        cmsql.ExecuteNonQuery()

        cmsql.Parameters.Clear()
        cmsql.CommandText = " CREATE CLUSTERED INDEX IX_PhysicalCountBase ON #PhysicalCountBase ( Reference, StkID ) "
        cmsql.ExecuteNonQuery()

    End Sub

    Private Sub DropPhysicalCountBase(cmsql As SqlCommand)

        cmsql.Parameters.Clear()
        cmsql.CommandText = " DROP TABLE #PhysicalCountBase
                              DROP TABLE #PhysicalCountCons
                              DROP TABLE #PhysicalCountInclude
                              DROP TABLE #PhysicalCountExclude "
        cmsql.ExecuteNonQuery()

    End Sub

    'Adds @<prefix>0, @<prefix>1... and returns "column IN ( @<prefix>0, ... )", or "" when there is no code
    Private Function PhysicalCountInList(cmsql As SqlCommand, column As String, prefix As String, codes As List(Of String)) As String

        If codes.Count = 0 Then Return ""

        Dim names As New List(Of String)
        For i As Integer = 0 To codes.Count - 1
            names.Add($"@{prefix}{i}")
            cmsql.Parameters.Add($"@{prefix}{i}", SqlDbType.VarChar, 10).Value = codes(i)
        Next

        Return $"{column} IN ( {String.Join(", ", names)} )"

    End Function

    'FROM / WHERE of the items that can be counted (one row per item) with the filters, and the selected expression (1 or 0)
    'Adds @Location, @All, @NotCountedFrom, @NotCountedTo and the codes to the command (the parameters must be cleared before)
    Private Sub PhysicalCountItemsQuery(cmsql As SqlCommand, location As String, stkFil As String, filters As PhysicalCountCreateFilters,
                                        selection As PhysicalCountCreateSelection, consignment As String,
                                        ByRef fromWhere As String, ByRef selectedExpression As String)

        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
        cmsql.Parameters.Add("@All", SqlDbType.Bit).Value = selection.All

        Dim where As New StringBuilder(" WHERE Item.STK_ACTIVE = 'Y' ")

        'consignment : "" (no consignment view), "view" (dbo.V_StockNormalCons) or "temp" (#PhysicalCountCons, the view read once)
        If consignment = "view" Then where.Append(" AND NOT EXISTS ( SELECT 1 FROM dbo.V_StockNormalCons WHERE NSTK_RECID = Item.STK_RECID ) ")
        If consignment = "temp" Then where.Append(" AND NOT EXISTS ( SELECT 1 FROM #PhysicalCountCons WHERE StkID = Item.STK_RECID ) ")

        If filters.Books = "books" Then where.Append(" AND Item.Stk_BookID IS NOT NULL AND Item.Stk_BookID <> 0 ")
        If filters.Books = "noBooks" Then where.Append(" AND ( Item.Stk_BookID IS NULL OR Item.Stk_BookID = 0 ) ")

        'Not counted between the dates : not in an open count (not cancelled, not updated) of the location, warehouse 1
        If filters.NotCounted Then
            where.Append(" AND NOT EXISTS ( SELECT 1
                                            FROM   dbo.tbl_PhysicalCountDetail CountDetail
                                                   INNER JOIN dbo.tbl_PhysicalCountHeader CountHeader ON CountDetail.Pcd_HRecId = CountHeader.Pch_Recid
                                                                                                     AND CountDetail.Pcd_Loc = CountHeader.Pch_Loc
                                            WHERE  CountDetail.Pcd_Loc = @Location
                                                   AND CountDetail.Pcd_stkid = Item.STK_RECID
                                                   AND CountDetail.Pcd_Whouse = '1'
                                                   AND CountHeader.Pch_Status = 'N'
                                                   AND CountHeader.Pch_Updated = 'N' ")
            If filters.NotCountedFrom.HasValue Then
                where.Append(" AND CountDetail.Pcd_DateCount >= @NotCountedFrom ")
                cmsql.Parameters.Add("@NotCountedFrom", SqlDbType.DateTime).Value = filters.NotCountedFrom.Value
            End If
            If filters.NotCountedTo.HasValue Then
                where.Append(" AND CountDetail.Pcd_DateCount < DATEADD(DAY, 1, @NotCountedTo) ")
                cmsql.Parameters.Add("@NotCountedTo", SqlDbType.DateTime).Value = filters.NotCountedTo.Value
            End If
            where.Append(" ) ")
        End If

        fromWhere = $" FROM    {stkFil} Item
                               INNER JOIN dbo.stk04_itemtype ON Item.STK_ITMTYPE = IT_Code
                               INNER JOIN dbo.STK04_ItmIdentif ON Item.STK_ITMIDENT = ID_RECID
                               INNER JOIN dbo.stk04_supplier ON Item.STK_SUPPLIER = Sup_Code
                               LEFT JOIN dbo.Stk04_Brand ON br_Code = Item.STK_Brand
                               LEFT JOIN dbo.Stk04_Category ON Cat_Code = Item.STK_CATEGORY
                       {where} "

        'Checked codes : OR between the brands, categories, product groups and identifications
        Dim groups As New List(Of String)
        If selection.Groups Then
            For Each inList In {PhysicalCountInList(cmsql, "Item.STK_Brand", "Brand", filters.Brands),
                                PhysicalCountInList(cmsql, "Item.STK_CATEGORY", "Category", filters.Categories),
                                PhysicalCountInList(cmsql, "Item.STK_ITMTYPE", "ProductGroup", filters.ProductGroups),
                                PhysicalCountInList(cmsql, "ID_Code", "Identification", filters.Identifications)}
                If inList <> "" Then groups.Add(inList)
            Next
        End If

        selectedExpression = $" CASE WHEN EXISTS ( SELECT 1 FROM #PhysicalCountExclude WHERE StkID = Item.STK_RECID ) THEN 0
                                     WHEN @All = 1 {If(groups.Count > 0, " OR " & String.Join(" OR ", groups), "")}
                                          OR EXISTS ( SELECT 1 FROM #PhysicalCountInclude WHERE StkID = Item.STK_RECID ) THEN 1
                                     ELSE 0
                                END "

    End Sub

    'Brands, categories, product groups and identifications of the items that can be counted, working day, modes and privilege
    Public Function GetPhysicalCountFilters(location As String, userCode As String) As String Implements IDataService.GetPhysicalCountFilters

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    PhysicalCountReadWithoutWaiting(cmsql)

                    cmsql.Parameters.Clear()
                    cmsql.CommandText = " SELECT ISNULL(Loc_Desc, '') FROM dbo.STK04_Location WHERE Loc_Code = @Location "
                    cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
                    Dim locationDescription As Object = cmsql.ExecuteScalar()
                    If locationDescription Is Nothing Then
                        PhysicalCountReadCommitted(cnsql)
                        Return $"Error Location {location.Trim} Does Not Exist On This Server"
                    End If

                    Dim stkFil As String = "", stkQty As String = ""
                    PhysicalCountStockTables(cmsql, location, stkFil, stkQty)

                    Dim workingDay As Date = PhysicalCountWorkingDay(cmsql, location)
                    Dim showAvailableQty As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonAvailableQty)

                    cmsql.Parameters.Clear()
                    cmsql.CommandText = " SELECT TOP 1 ISNULL(SP_Book, 'N') FROM dbo.stk04_SysParam "
                    Dim bookMode As String = If(cmsql.ExecuteScalar()?.ToString.Trim.ToUpper, "N")

                    'Only the codes used by active items, like the lists of the module (codes of the shown items)
                    Dim lists As New Dictionary(Of String, String) From {
                        {"brands", $" SELECT  LTRIM(RTRIM(br_Code)) code , ISNULL(br_Name, '') description
                                      FROM    dbo.Stk04_Brand
                                      WHERE   br_Code IN ( SELECT STK_Brand FROM {stkFil} WHERE STK_ACTIVE = 'Y' )
                                      ORDER BY br_Name "},
                        {"categories", $" SELECT  LTRIM(RTRIM(Cat_Code)) code , ISNULL(Cat_Desc, '') description
                                          FROM    dbo.Stk04_Category
                                          WHERE   Cat_Code IN ( SELECT STK_CATEGORY FROM {stkFil} WHERE STK_ACTIVE = 'Y' )
                                          ORDER BY Cat_Desc "},
                        {"productGroups", $" SELECT  LTRIM(RTRIM(IT_Code)) code , ISNULL(IT_Desc, '') description
                                             FROM    dbo.stk04_itemtype
                                             WHERE   IT_Code IN ( SELECT STK_ITMTYPE FROM {stkFil} WHERE STK_ACTIVE = 'Y' )
                                             ORDER BY IT_Desc "},
                        {"identifications", $" SELECT  LTRIM(RTRIM(ID_Code)) code , ISNULL(ID_Desc, '') description
                                               FROM    dbo.STK04_ItmIdentif
                                               WHERE   ID_RECID IN ( SELECT STK_ITMIDENT FROM {stkFil} WHERE STK_ACTIVE = 'Y' )
                                               ORDER BY ID_Desc "}}

                    Dim result As New StringBuilder("{")
                    result.Append($"""location"":""{location.Trim}"",")
                    result.Append($"""locationDescription"":{New JavaScriptSerializer().Serialize(locationDescription.ToString.Trim)},")
                    result.Append($"""workingDay"":""{workingDay.ToString("dd/MM/yyyy")}"",")
                    result.Append($"""bookMode"":""{bookMode}"",")
                    result.Append($"""showAvailableQty"":""{IIf(showAvailableQty, "Y", "N")}""")

                    For Each list In lists
                        cmsql.Parameters.Clear()
                        cmsql.CommandText = list.Value
                        Using dtList As New DataTable
                            Using daSql As New SqlDataAdapter(cmsql)
                                daSql.Fill(dtList)
                            End Using
                            result.Append($",""{list.Key}"":{GetJson(dtList)}")
                        End Using
                    Next

                    result.Append("}")

                    PhysicalCountReadCommitted(cnsql)

                    Return result.ToString

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Items that can be counted with the filters, one page, ordered by reference
    'searchText : part of the reference, description, supplier, brand, category or product group, or a whole barcode
    'onlySelected : only the selected items
    'exactMatch : a scan, only the item of this barcode or of this reference (not the items containing the text)
    'afterReference / afterItemRecID : last item of the previous page ("" = first page) : the page is read from there,
    'the whole list is not numbered for every page
    'Returns {"totalItems":n,"selectedItems":n,"items":[...]} : the totals are counted on the first page only (-1 on the next pages),
    'selectedItems of the whole count (-1 with a search : the search does not change it, the device keeps its value)
    Public Function GetPhysicalCountItems(location As String, filters As String, selection As String, searchText As String, onlySelected As Boolean,
                                          afterReference As String, afterItemRecID As String, pageSize As Integer, exactMatch As Boolean, userCode As String) As String Implements IDataService.GetPhysicalCountItems

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"
            If pageSize < 1 OrElse pageSize > PhysicalCountMaxItemsPageSize Then Return $"Error Page Size Should Be Between 1 And {PhysicalCountMaxItemsPageSize}"

            Dim search As String = If(searchText, "").Trim
            If search.Length > 50 AndAlso ItemRecIDFromLink(search) = "" Then Return "Error Search Text Too Long"

            Dim firstPage As Boolean = String.IsNullOrWhiteSpace(afterItemRecID)
            Dim afterID As Decimal = 0
            If Not firstPage AndAlso Not Decimal.TryParse(afterItemRecID.Trim, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, afterID) Then
                Return "Error Invalid Item"
            End If
            Dim afterRef As String = If(afterReference, "")

            Dim objFilters As PhysicalCountCreateFilters = Nothing
            Dim errorMessage As String = ParsePhysicalCountFilters(filters, objFilters)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim objSelection As PhysicalCountCreateSelection = Nothing
            errorMessage = ParsePhysicalCountSelection(selection, objSelection)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Try

                    Using cmsql As New SqlCommand(" ", cnsql)

                        cmsql.CommandTimeout = 600

                        PhysicalCountReadWithoutWaiting(cmsql)

                        Dim stkFil As String = "", stkQty As String = ""
                        PhysicalCountStockTables(cmsql, location, stkFil, stkQty)

                        Dim showAvailableQty As Boolean = PhysicalCountNegation(cmsql, location, userCode, PhysicalCountButtonAvailableQty)
                        Dim consignment As String = IIf(PhysicalCountObjectExists(cmsql, "dbo.V_StockNormalCons"), "view", "").ToString

                        LoadPhysicalCountSelection(cnsql, Nothing, cmsql, objSelection)

                        Dim fromWhere As String = "", selectedExpression As String = ""
                        cmsql.Parameters.Clear()
                        PhysicalCountItemsQuery(cmsql, location, stkFil, objFilters, objSelection, consignment, fromWhere, selectedExpression)
                        Dim moreWhere As String = PhysicalCountSearchWhere(cmsql, search, exactMatch) & IIf(onlySelected, $" AND {selectedExpression} = 1 ", "").ToString

                        'Same expression to sort and to read after the last item (a varchar and an nvarchar can sort differently)
                        Dim referenceKey As String = " CONVERT(NVARCHAR(200), ISNULL(Item.STK_REF, '')) "

                        'Totals : first page only, one pass
                        Dim totalItems As Integer = -1, selectedItems As Integer = -1
                        If firstPage Then
                            cmsql.CommandText = $" SELECT COUNT(*) , ISNULL(SUM(Selected), 0)
                                                   FROM   ( SELECT {selectedExpression} Selected {fromWhere} {moreWhere} ) Items "
                            Using drTotals As SqlDataReader = cmsql.ExecuteReader()
                                drTotals.Read()
                                totalItems = drTotals.GetInt32(0)
                                If search = "" Then selectedItems = drTotals.GetInt32(1)
                            End Using
                        End If

                        cmsql.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize
                        Dim afterWhere As String = ""
                        If Not firstPage Then
                            cmsql.Parameters.Add("@AfterReference", SqlDbType.NVarChar, 200).Value = afterRef
                            cmsql.Parameters.Add("@AfterItemRecID", SqlDbType.Decimal).Value = afterID
                            afterWhere = $" AND ( {referenceKey} > @AfterReference OR ( {referenceKey} = @AfterReference AND Item.STK_RECID > @AfterItemRecID ) ) "
                        End If

                        'Only the keys of the page are sorted (item, reference, selected) : a narrow sort needs little memory,
                        'a server short of memory does not keep the query waiting (RESOURCE_SEMAPHORE). The texts are read for the page rows only
                        'The available qty is read only for the rows of the page : qty of the location - reserved + reservation orders
                        cmsql.CommandText = $" SELECT   CONVERT(VARCHAR(18), Page.StkID) itemRecID ,
                                                        Page.Reference itemReference ,
                                                        LTRIM(RTRIM(ISNULL(Item.STK_DESC, ''))) + CASE WHEN Item.STK_NEWUSED = 'U' THEN ' (Used)' ELSE '' END itemDescription ,
                                                        ISNULL(Sup_Name, '') supplier ,
                                                        ISNULL(br_Name, '') brand ,
                                                        ISNULL(Cat_Desc, '') category ,
                                                        ISNULL(IT_Desc, '') productGroup ,
                                                        ISNULL(ID_Desc, '') identification ,
                                                        ISNULL(Item.STK_UNIT, '') unit ,
                                                        {IIf(showAvailableQty, "ISNULL(Qty.Available, 0) + ISNULL(Reservation.ResQty, 0) systemQty ,", "")}
                                                        Page.Selected selected
                                               FROM     ( SELECT TOP ( @PageSize )
                                                                 Item.STK_RECID StkID ,
                                                                 {referenceKey} Reference ,
                                                                 {selectedExpression} Selected
                                                          {fromWhere} {moreWhere} {afterWhere}
                                                          ORDER BY {referenceKey}, Item.STK_RECID ) Page
                                                        INNER JOIN {stkFil} Item ON Item.STK_RECID = Page.StkID
                                                        LEFT JOIN dbo.stk04_itemtype ON Item.STK_ITMTYPE = IT_Code
                                                        LEFT JOIN dbo.STK04_ItmIdentif ON Item.STK_ITMIDENT = ID_RECID
                                                        LEFT JOIN dbo.stk04_supplier ON Item.STK_SUPPLIER = Sup_Code
                                                        LEFT JOIN dbo.Stk04_Brand ON br_Code = Item.STK_Brand
                                                        LEFT JOIN dbo.Stk04_Category ON Cat_Code = Item.STK_CATEGORY
                                                        {IIf(showAvailableQty, $"OUTER APPLY ( SELECT SUM(ISNULL(STQ_AVAILABLE, 0) - ISNULL(STQ_RESERVED, 0)) Available
                                                                                              FROM   {stkQty}
                                                                                              WHERE  STQ_STKRECID = Page.StkID
                                                                                                     AND STQ_LOCCODE = @Location ) Qty
                                                                                 OUTER APPLY ( SELECT SUM(ROD_ResQty * CASE WHEN ROH_Status = 'N' THEN 1 ELSE -1 END) ResQty
                                                                                               FROM   dbo.stk04_ResOrdH
                                                                                                      INNER JOIN dbo.STK04_ResOrdD ON ROD_Loc = ROH_Loc
                                                                                                                                      AND ROD_Hrecid = ROH_Recid
                                                                                               WHERE  ROH_Loc = @Location
                                                                                                      AND ROD_StkRecid = Page.StkID ) Reservation", "")}
                                               ORDER BY Page.Reference, Page.StkID "

                        Using dtItems As New DataTable

                            Using daSql As New SqlDataAdapter(cmsql)
                                daSql.Fill(dtItems)
                            End Using

                            cmsql.Parameters.Clear()
                            cmsql.CommandText = " DROP TABLE #PhysicalCountInclude
                                                  DROP TABLE #PhysicalCountExclude "
                            cmsql.ExecuteNonQuery()

                            Return $"{{""totalItems"":{totalItems},""selectedItems"":{selectedItems},""items"":{GetJson(dtItems)}}}"

                        End Using

                    End Using

                Finally
                    PhysicalCountReadCommitted(cnsql)
                End Try

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Memo 2 of the counts created by the device : "Portable Request : <id>"
    Private Const PhysicalCountRequestPrefix As String = "Portable Request : "

    'Counts of this machine (Pch_MachUser1..6) on the location, the last 50, for My Counts of the device
    'status : open (can be counted), cancelled (Pch_Status = C) or finalized (updated to stock)
    'Returns [{"countNumber":"17","dateCount":"08/10/2026","nbrItems":2300,"status":"open","createdOnPortable":1}, ...]
    Public Function GetMachinePhysicalCounts(location As String, machineName As String) As String Implements IDataService.GetMachinePhysicalCounts

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"
            If String.IsNullOrWhiteSpace(machineName) OrElse machineName.Trim.Length > 5 Then Return "Error Invalid Machine Name"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Try

                    Using cmsql As New SqlCommand(" ", cnsql)

                        cmsql.CommandTimeout = 600

                        'A count being created by another device does not keep the list waiting
                        PhysicalCountReadWithoutWaiting(cmsql)

                        cmsql.Parameters.Clear()
                        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
                        cmsql.Parameters.Add("@MachineName", SqlDbType.VarChar, 5).Value = machineName.Trim
                        cmsql.Parameters.Add("@RequestTag", SqlDbType.NVarChar, 100).Value = PhysicalCountRequestPrefix
                        cmsql.CommandText = " SELECT TOP 50 CONVERT(VARCHAR(18), Pch_Recid) countNumber ,
                                                     ISNULL(CONVERT(VARCHAR(10), Pch_DateCount, 103), '') dateCount ,
                                                     CONVERT(INT, ISNULL(Pch_NbrItems, 0)) nbrItems ,
                                                     CASE WHEN ISNULL(Pch_Status, '') = 'C' THEN 'cancelled'
                                                          WHEN ISNULL(Pch_Updated, '') <> 'N' THEN 'finalized'
                                                          ELSE 'open'
                                                     END status ,
                                                     CASE WHEN Pch_Memo2 LIKE @RequestTag + '%' THEN 1 ELSE 0 END createdOnPortable
                                              FROM   dbo.tbl_PhysicalCountHeader
                                              WHERE  Pch_Loc = @Location
                                                     AND @MachineName IN ( Pch_MachUser1, Pch_MachUser2, Pch_MachUser3, Pch_MachUser4, Pch_MachUser5, Pch_MachUser6 )
                                              ORDER BY Pch_Recid DESC "

                        Using dtCounts As New DataTable
                            Using daSql As New SqlDataAdapter(cmsql)
                                daSql.Fill(dtCounts)
                            End Using
                            Return GetJson(dtCounts)
                        End Using

                    End Using

                Finally
                    PhysicalCountReadCommitted(cnsql)
                End Try

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Descriptions of the checked codes for the memo of the count ("Brands Selected ( A,B )")
    Private Function PhysicalCountMemoPart(cmsql As SqlCommand, title As String, table As String, codeColumn As String, descriptionColumn As String, codes As List(Of String)) As String

        If codes.Count = 0 Then Return ""

        cmsql.Parameters.Clear()
        cmsql.CommandText = $" SELECT ISNULL({descriptionColumn}, '') FROM {table} WHERE {PhysicalCountInList(cmsql, codeColumn, "Code", codes)} ORDER BY 1 "

        Dim descriptions As New List(Of String)
        Using drCodes As SqlDataReader = cmsql.ExecuteReader()
            While drCodes.Read()
                descriptions.Add(drCodes.GetString(0).Trim)
            End While
        End Using

        Return vbCrLf & $"{title} Selected ( {String.Join(",", descriptions)} ) "

    End Function

    'Creates the count with the selected items for this machine only
    'requestID : made by the device for each Create, kept in memo 2 ("Portable Request : <id>"). The device sends the same request again
    'when it did not get the answer in time : the count already created for this request is returned, a second count is never created
    'Returns {"countNumber":"125","itemsCount":n,"linesCount":n,"barcodesCount":n,"existing":false} (existing : created by the first try)
    Public Function CreatePhysicalCount(location As String, machineName As String, filters As String, selection As String, memo As String,
                                        requestID As String) As String Implements IDataService.CreatePhysicalCount

        Try

            If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Error Invalid Location"
            If String.IsNullOrWhiteSpace(machineName) OrElse machineName.Trim.Length > 5 Then Return "Error Invalid Machine Name"

            'Same rule as the count module : U for a user, IT for a machine
            Dim machine As String = machineName.Trim
            If Not machine.ToUpper.StartsWith("U") AndAlso Not machine.ToUpper.StartsWith("IT") Then
                Return "Error Count Machine Or User Name Must Start With 'U' For User Or 'IT' For Machine"
            End If

            Dim userMemo As String = If(memo, "").Trim
            If userMemo.Length > 2900 Then Return "Error Memo Too Long"

            Dim request As String = If(requestID, "").Trim
            If request.Length > 40 OrElse Not request.All(Function(c) Char.IsLetterOrDigit(c) OrElse c = "-"c) Then Return "Error Invalid Request"
            Dim requestTag As String = IIf(request = "", "", PhysicalCountRequestPrefix & request).ToString

            'Memo 2 : the request of the device, then the memo typed by the user
            Dim memo2 As String = requestTag & IIf(requestTag <> "" AndAlso userMemo <> "", vbCrLf, "").ToString & userMemo

            Dim objFilters As PhysicalCountCreateFilters = Nothing
            Dim errorMessage As String = ParsePhysicalCountFilters(filters, objFilters)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim objSelection As PhysicalCountCreateSelection = Nothing
            errorMessage = ParsePhysicalCountSelection(selection, objSelection)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                Dim trnSql As SqlTransaction = Nothing
                Try

                    cnsql.Open()

                    Dim countNumber As Decimal
                    Dim itemsCount, linesCount, barcodesCount As Integer

                    Using cmsql As New SqlCommand(" ", cnsql)

                        cmsql.CommandTimeout = 900

                        'Waits at most 60 seconds for the programs updating the stock at the same time (error 1222 after)
                        cmsql.CommandText = " SET LOCK_TIMEOUT 60000 "
                        cmsql.ExecuteNonQuery()

                        cmsql.Parameters.Clear()
                        cmsql.CommandText = " SELECT 1 FROM dbo.STK04_Location WHERE Loc_Code = @Location "
                        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
                        If cmsql.ExecuteScalar() Is Nothing Then Return $"Error Location {location.Trim} Does Not Exist On This Server"

                        Dim stkFil As String = "", stkQty As String = ""
                        PhysicalCountStockTables(cmsql, location, stkFil, stkQty)

                        Dim countDate As Date = PhysicalCountWorkingDay(cmsql, location)

                        'Memo 1 : count date, machine and the checked codes, like the module
                        If Not objSelection.Groups Then
                            objFilters.Brands.Clear()
                            objFilters.Categories.Clear()
                            objFilters.ProductGroups.Clear()
                            objFilters.Identifications.Clear()
                        End If
                        Dim memo1 As String = "Count Date : " & countDate.ToString("dd/MM/yyyy") &
                                              vbCrLf & IIf(machine.ToUpper.StartsWith("U"), "User Name : ", "Machine Name : ").ToString & machine &
                                              PhysicalCountMemoPart(cmsql, "Brands", "dbo.Stk04_Brand", "br_Code", "br_Name", objFilters.Brands) &
                                              PhysicalCountMemoPart(cmsql, "Categories", "dbo.Stk04_Category", "Cat_Code", "Cat_Desc", objFilters.Categories) &
                                              PhysicalCountMemoPart(cmsql, "Product Groups", "dbo.stk04_itemtype", "IT_Code", "IT_Desc", objFilters.ProductGroups) &
                                              PhysicalCountMemoPart(cmsql, "Item Identifications", "dbo.STK04_ItmIdentif", "ID_Code", "ID_Desc", objFilters.Identifications)
                        If memo1.Length > 3000 Then memo1 = memo1.Substring(0, 3000)

                        trnSql = cnsql.BeginTransaction
                        cmsql.Transaction = trnSql

                        'Same request sent again (the device did not get the answer in time) : the count of the first try.
                        'The headers of the location stay locked until the commit : a second try waits for the first one to finish
                        If requestTag <> "" Then

                            cmsql.Parameters.Clear()
                            cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
                            cmsql.Parameters.Add("@RequestTag", SqlDbType.NVarChar, 100).Value = requestTag
                            cmsql.CommandText = " SELECT TOP 1 Pch_Recid
                                                  FROM   dbo.tbl_PhysicalCountHeader WITH ( UPDLOCK, HOLDLOCK )
                                                  WHERE  Pch_Loc = @Location
                                                         AND Pch_Memo2 LIKE @RequestTag + '%' "
                            Dim existingCount As Object = cmsql.ExecuteScalar()

                            If existingCount IsNot Nothing Then

                                cmsql.Parameters.Add("@CountNumber", SqlDbType.Decimal).Value = CDec(existingCount)
                                cmsql.CommandText = " SELECT ( SELECT CONVERT(INT, ISNULL(Pch_NbrItems, 0)) FROM dbo.tbl_PhysicalCountHeader
                                                               WHERE Pch_Loc = @Location AND Pch_Recid = @CountNumber ) ,
                                                             ( SELECT COUNT(*) FROM dbo.tbl_PhysicalCountDetail
                                                               WHERE Pcd_Loc = @Location AND Pcd_HRecId = @CountNumber ) ,
                                                             ( SELECT COUNT(*) FROM dbo.tbl_PhysicalCountDetBarcode
                                                               WHERE PcBarc_LOC = @Location
                                                                     AND PcBarc_DRecId IN ( SELECT Pcd_RecId FROM dbo.tbl_PhysicalCountDetail
                                                                                            WHERE Pcd_Loc = @Location AND Pcd_HRecId = @CountNumber ) ) "
                                Using drExisting As SqlDataReader = cmsql.ExecuteReader()
                                    drExisting.Read()
                                    itemsCount = drExisting.GetInt32(0)
                                    linesCount = drExisting.GetInt32(1)
                                    barcodesCount = drExisting.GetInt32(2)
                                End Using

                                trnSql.Rollback()
                                PhysicalCountReadCommitted(cnsql)

                                Return $"{{""countNumber"":""{CDec(existingCount)}"",""itemsCount"":{itemsCount},""linesCount"":{linesCount},""barcodesCount"":{barcodesCount},""existing"":true}}"

                            End If

                        End If

                        'Selected items in #PhysicalCountBase
                        LoadPhysicalCountBase(cnsql, trnSql, cmsql, location, stkFil, objFilters, objSelection, "", True)

                        cmsql.Parameters.Clear()
                        cmsql.CommandText = " SELECT COUNT(DISTINCT StkID) FROM #PhysicalCountBase "
                        itemsCount = CInt(cmsql.ExecuteScalar())

                        If itemsCount = 0 Then
                            trnSql.Rollback()
                            Return "Error No Items Selected"
                        End If

                        'Next numbers of the location : the rows stay locked until the commit, two devices cannot get the same number
                        cmsql.Parameters.Clear()
                        cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = location.Trim
                        cmsql.CommandText = " SELECT ISNULL(MAX(Pch_Recid), 0) + 1 FROM dbo.tbl_PhysicalCountHeader WITH ( UPDLOCK, HOLDLOCK ) WHERE Pch_Loc = @Location "
                        countNumber = CDec(cmsql.ExecuteScalar())

                        cmsql.CommandText = " SELECT ISNULL(MAX(Pcd_RecId), 0) FROM dbo.tbl_PhysicalCountDetail WITH ( UPDLOCK, HOLDLOCK ) WHERE Pcd_Loc = @Location "
                        Dim lastLine As Decimal = CDec(cmsql.ExecuteScalar())

                        cmsql.CommandText = " SELECT ISNULL(MAX(PcBarc_RecId), 0) FROM dbo.tbl_PhysicalCountDetBarcode WITH ( UPDLOCK, HOLDLOCK ) WHERE PcBarc_LOC = @Location "
                        Dim lastBarcode As Decimal = CDec(cmsql.ExecuteScalar())

                        cmsql.Parameters.Add("@CountNumber", SqlDbType.Decimal).Value = countNumber
                        cmsql.Parameters.Add("@CountDate", SqlDbType.DateTime).Value = countDate
                        cmsql.Parameters.Add("@MachineName", SqlDbType.VarChar, 5).Value = machine
                        cmsql.Parameters.Add("@Memo1", SqlDbType.NVarChar, 3000).Value = memo1
                        cmsql.Parameters.Add("@Memo2", SqlDbType.NVarChar, 3000).Value = memo2
                        cmsql.Parameters.Add("@NbrItems", SqlDbType.Decimal).Value = itemsCount
                        cmsql.Parameters.Add("@LastLine", SqlDbType.Decimal).Value = lastLine
                        cmsql.Parameters.Add("@LastBarcode", SqlDbType.Decimal).Value = lastBarcode

                        'Header : open (N), not updated (N), warehouse 1, this machine only
                        cmsql.CommandText = " INSERT INTO dbo.tbl_PhysicalCountHeader ( Pch_Recid, Pch_Loc, Pch_Whouse, Pch_DateCount, Pch_MachUser1, Pch_MachUser2, Pch_MachUser3,
                                                                                         Pch_MachUser4, Pch_MachUser5, Pch_MachUser6, Pch_Memo1, Pch_Memo2, Pch_NbrItems,
                                                                                         Pch_Status, Pch_Updated )
                                              VALUES ( @CountNumber, @Location, '1', @CountDate, @MachineName, NULL, NULL, NULL, NULL, NULL, @Memo1, @Memo2, @NbrItems, 'N', 'N' ) "
                        cmsql.ExecuteNonQuery()

                        'Lines : one per variant of the item at location 00 (one line without variant when the item has none),
                        'with the available qty of the location (available - reserved + reservation orders) of the variant
                        cmsql.CommandText = $" INSERT INTO dbo.tbl_PhysicalCountDetail ( Pcd_RecId, Pcd_HRecId, Pcd_Loc, Pcd_Whouse, Pcd_stkid, Pcd_Color, Pcd_Grid, Pcd_DetGrid,
                                                                                         Pcd_ExpDate, Pcd_Batch, Pcd_DateCount, Pcd_Qty,
                                                                                         Pcd_QtyCounted1, Pcd_QtyCounted2, Pcd_QtyCounted3, Pcd_QtyCounted4, Pcd_QtyCounted5, Pcd_QtyCounted6,
                                                                                         Pcd_Type1, Pcd_Type2, Pcd_Type3, Pcd_Type4, Pcd_Type5, Pcd_Type6, Pcd_AdjType, Pcd_AdjFolio )
                                               SELECT  @LastLine + ROW_NUMBER() OVER ( ORDER BY Variant.STK_REF, Variant.StkID, Variant.Color, Variant.Grid, Variant.DetGrid, Variant.ExpDate, Variant.Batch ) ,
                                                       @CountNumber , @Location , '1' , Variant.StkID ,
                                                       Variant.Color , Variant.Grid , Variant.DetGrid , Variant.ExpDate , Variant.Batch ,
                                                       @CountDate ,
                                                       ISNULL(Qty.Available, 0) + ISNULL(Reservation.ResQty, 0) ,
                                                       NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
                                               FROM    ( SELECT DISTINCT Items.StkID ,
                                                                         Items.Reference STK_REF ,
                                                                         Qty00.STQ_Color Color ,
                                                                         Qty00.STQ_Grid Grid ,
                                                                         Qty00.STQ_DetGrid DetGrid ,
                                                                         Qty00.STQ_ExpiryDt ExpDate ,
                                                                         Qty00.STQ_BatchInfo Batch
                                                         FROM   #PhysicalCountBase Items
                                                                LEFT JOIN {stkQty} Qty00 ON Qty00.STQ_STKRECID = Items.StkID
                                                                                            AND Qty00.STQ_LOCCODE = '00' ) Variant
                                                       OUTER APPLY ( SELECT SUM(ISNULL(STQ_AVAILABLE, 0) - ISNULL(STQ_RESERVED, 0)) Available
                                                                     FROM   {stkQty}
                                                                     WHERE  STQ_STKRECID = Variant.StkID
                                                                            AND STQ_LOCCODE = @Location
                                                                            AND ISNULL(STQ_Color, '') = ISNULL(Variant.Color, '')
                                                                            AND ISNULL(STQ_Grid, '') = ISNULL(Variant.Grid, '')
                                                                            AND ISNULL(STQ_DetGrid, '') = ISNULL(Variant.DetGrid, '')
                                                                            AND ISNULL(STQ_ExpiryDt, '') = ISNULL(Variant.ExpDate, '')
                                                                            AND ISNULL(STQ_BatchInfo, '') = ISNULL(Variant.Batch, '') ) Qty
                                                       OUTER APPLY ( SELECT SUM(ROD_ResQty * CASE WHEN ROH_Status = 'N' THEN 1 ELSE -1 END) ResQty
                                                                     FROM   dbo.stk04_ResOrdH
                                                                            INNER JOIN dbo.STK04_ResOrdD ON ROD_Loc = ROH_Loc
                                                                                                            AND ROD_Hrecid = ROH_Recid
                                                                     WHERE  ROH_Loc = @Location
                                                                            AND ROD_StkRecid = Variant.StkID
                                                                            AND COALESCE(ROD_Color, '') = COALESCE(Variant.Color, '')
                                                                            AND COALESCE(ROD_Grid, '') = COALESCE(Variant.Grid, '')
                                                                            AND COALESCE(ROD_DetGrid, '') = COALESCE(Variant.DetGrid, '')
                                                                            AND COALESCE(ROD_ExpDate, '') = COALESCE(Variant.ExpDate, '')
                                                                            AND COALESCE(ROD_Batch, '') = COALESCE(Variant.Batch, '') ) Reservation
                                               SELECT @@ROWCOUNT "
                        linesCount = CInt(cmsql.ExecuteScalar())

                        'Barcodes of each line (same variant)
                        cmsql.CommandText = " INSERT INTO dbo.tbl_PhysicalCountDetBarcode ( PcBarc_RecId, PcBarc_DRecId, PcBarc_Barcode, PcBarc_LOC )
                                              SELECT  @LastBarcode + ROW_NUMBER() OVER ( ORDER BY LineBarcode.Pcd_RecId, LineBarcode.dbar_barcode ) ,
                                                      LineBarcode.Pcd_RecId ,
                                                      LineBarcode.dbar_barcode ,
                                                      @Location
                                              FROM    ( SELECT DISTINCT Pcd_RecId , dbar_barcode
                                                        FROM   dbo.tbl_PhysicalCountDetail
                                                               INNER JOIN dbo.Stk04_DetBarcode ON Pcd_stkid = dbar_stkid
                                                                                                  AND ISNULL(dbar_expdate, '') = ISNULL(Pcd_ExpDate, '')
                                                                                                  AND ISNULL(dbar_batch, '') = ISNULL(Pcd_Batch, '')
                                                                                                  AND ISNULL(dbar_grid, '') = ISNULL(Pcd_Grid, '')
                                                                                                  AND ISNULL(dbar_Detgrid, '') = ISNULL(Pcd_DetGrid, '')
                                                                                                  AND ISNULL(dbar_color, '') = ISNULL(Pcd_Color, '')
                                                        WHERE  Pcd_HRecId = @CountNumber
                                                               AND Pcd_Loc = @Location
                                                               AND ISNULL(dbar_barcode, '') <> '' ) LineBarcode
                                              SELECT @@ROWCOUNT "
                        barcodesCount = CInt(cmsql.ExecuteScalar())

                        DropPhysicalCountBase(cmsql)

                    End Using

                    trnSql.Commit()
                    PhysicalCountReadCommitted(cnsql)

                    Return $"{{""countNumber"":""{countNumber}"",""itemsCount"":{itemsCount},""linesCount"":{linesCount},""barcodesCount"":{barcodesCount},""existing"":false}}"

                Catch exsv As Exception
                    Try
                        trnSql?.Rollback()
                    Catch exTRN As Exception
                    End Try
                    PhysicalCountReadCommitted(cnsql)
                    If TypeOf exsv Is SqlException AndAlso CType(exsv, SqlException).Number = 1222 Then
                        Return "Error The Stock Is Being Updated By Another Program, Please Try Again In A Few Minutes"
                    End If
                    Throw
                End Try

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

#End Region

#Region "Print Label"

    Private Const LabelMaxQty As Integer = 500

    Private Const LabelSearchLimit As Integer = 50

    'The item scanned (any of its codes : references, barcodes, supplier references) or picked in the search (itemRecID)
    'Returns {"item":{itemRecID, itemReference, itemDescription, itemUnit, priceLC, currencyLC, priceCC, currencyCC},
    '         "codes":[{"group":"coding|barcodes|supplier","title":"Reference","value":"IT001"}, ...], "printCode":"..."}
    'or {"matches":[{itemRecID, itemReference, itemDescription}, ...]} when the scanned code belongs to more than one item
    'printCode : the code printed by default, the scanned one (the label prints what was scanned)
    Public Function GetLabelItem(barcode As String, itemRecID As String) As String Implements IDataService.GetLabelItem

        Try

            barcode = If(barcode, "").Trim
            itemRecID = If(itemRecID, "").Trim

            Dim serializer As New JavaScriptSerializer
            Dim recID As Decimal

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)

                    cmsql.CommandTimeout = 600

                    If itemRecID <> "" Then

                        If Not Decimal.TryParse(itemRecID, recID) Then Return "Error Invalid Item"

                    Else

                        If barcode = "" OrElse barcode.Length > 50 Then Return "Error Invalid Barcode"

                        Using dtMatches As DataTable = LabelFindItems(cmsql, barcode, True)
                            If dtMatches.Rows.Count = 0 Then Return $"Error Item Not Found : {barcode}"
                            If dtMatches.Rows.Count > 1 Then
                                Return serializer.Serialize(New Dictionary(Of String, Object) From {{"matches", LabelRows(dtMatches)}})
                            End If
                            recID = CDec(dtMatches.Rows(0)("itemRecID"))
                        End Using

                    End If

                    Dim errorMessage As String = ""
                    Dim item As Dictionary(Of String, Object) = LabelItemData(cmsql, recID, errorMessage)
                    If errorMessage <> "" Then Return "Error " & errorMessage

                    Dim codes As List(Of Dictionary(Of String, Object)) = LabelItemCodes(cmsql, recID)

                    Return serializer.Serialize(New Dictionary(Of String, Object) From {
                        {"item", item},
                        {"codes", codes},
                        {"printCode", LabelDefaultPrintCode(codes, barcode)}})

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Search of the item to print by a part of its references, barcodes, supplier references or description (the first LabelSearchLimit items)
    'Returns [{itemRecID, itemReference, itemDescription}, ...], "" when nothing is found
    Public Function SearchLabelItems(searchText As String) As String Implements IDataService.SearchLabelItems

        Try

            searchText = If(searchText, "").Trim
            If searchText.Length < 2 OrElse searchText.Length > 50 Then Return "Error Type At Least 2 Characters"

            Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

                cnsql.Open()

                Using cmsql As New SqlCommand(" ", cnsql)
                    cmsql.CommandTimeout = 600
                    Using dtMatches As DataTable = LabelFindItems(cmsql, searchText, False)
                        If dtMatches.Rows.Count = 0 Then Return ""
                        Return GetJson(dtMatches)
                    End Using
                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The active label designs of tbl_LabelDesigns, the default one first, the device chooses one of them
    'The same table in the ERP and the super store : read directly, without a view
    'Returns [{"labelDesign":"01","description":"Shelf Label","isDefault":1}, ...], "" when there is none
    Public Function GetLabelDesigns() As String Implements IDataService.GetLabelDesigns

        Try

            Using dtDesigns As New DataTable

                Using daSql As New SqlDataAdapter(" SELECT  LTRIM(RTRIM(CLD_Code)) labelDesign ,
                                                            LTRIM(RTRIM(CLD_Name)) description ,
                                                            CASE WHEN CLD_IsDefault = 1 THEN 1 ELSE 0 END isDefault
                                                    FROM    dbo.tbl_LabelDesigns
                                                    WHERE   CLD_Active = 1
                                                    ORDER BY CASE WHEN CLD_IsDefault = 1 THEN 0 ELSE 1 END , CLD_Name ",
                                                  DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtDesigns)
                End Using

                If dtDesigns.Rows.Count = 0 Then Return ""

                Return GetJson(dtDesigns)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Prints qty labels of the item with the chosen design, on the server printer (LabelPrinterName, or the printer saved in the design)
    'The item and its prices are read again here, the device only sends the item and the code to print
    Public Function PrintLabel(itemRecID As String, printCode As String, labelDesign As String, qty As Integer) As String Implements IDataService.PrintLabel

        Try

            If qty < 1 OrElse qty > LabelMaxQty Then Return $"Error The Quantity Should Be Between 1 And {LabelMaxQty}"

            Dim errorMessage As String = "", designPath As String = ""
            Dim labelItem As ClsBartender.LabelItem = LabelDesignAndItem(itemRecID, printCode, labelDesign, designPath, errorMessage)
            If errorMessage <> "" Then Return "Error " & errorMessage

            ClsBartender.PrintLabel(designPath, labelItem, If(ConfigurationManager.AppSettings("LabelPrinterName"), "").Trim, qty)

            Return "Success"

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Same label as PrintLabel, returned as a PNG image (base 64) instead of being printed : the designs can be checked without a printer
    Public Function PreviewLabel(itemRecID As String, printCode As String, labelDesign As String) As String Implements IDataService.PreviewLabel

        Try

            Dim errorMessage As String = "", designPath As String = ""
            Dim labelItem As ClsBartender.LabelItem = LabelDesignAndItem(itemRecID, printCode, labelDesign, designPath, errorMessage)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Return Convert.ToBase64String(ClsBartender.PreviewLabel(designPath, labelItem))

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Commands (base 64) of the label for the Bluetooth label printer of the device (CPCL), the device sends them to its printer
    'The label is printed at the size of the design (BarTender Page Setup), LabelOffsetXMM moves the print to the right
    Public Function GetLabelPrintData(itemRecID As String, printCode As String, labelDesign As String, qty As Integer) As String Implements IDataService.GetLabelPrintData

        Try

            If qty < 1 OrElse qty > LabelMaxQty Then Return $"Error The Quantity Should Be Between 1 And {LabelMaxQty}"

            Dim errorMessage As String = "", designPath As String = ""
            Dim labelItem As ClsBartender.LabelItem = LabelDesignAndItem(itemRecID, printCode, labelDesign, designPath, errorMessage)
            If errorMessage <> "" Then Return "Error " & errorMessage

            'Label printers : 8 dots per mm (203 dpi)
            Dim offsetXDots As Integer = CInt(LabelSettingMM("LabelOffsetXMM", 0) * 8)

            Return Convert.ToBase64String(ClsBartender.CpclLabel(designPath, labelItem, qty, offsetXDots))

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Test print to set up a label printer : a ruler 80 mm wide (wider than the print heads of the mobile label printers), one sticker high
    Public Function GetLabelRulerData() As String Implements IDataService.GetLabelRulerData

        Try
            Return Convert.ToBase64String(ClsBartender.CpclRuler(ClsBartender.PageWidthDots, 30 * 8))
        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Private Function LabelSettingMM(key As String, defaultValue As Double) As Double

        Dim value As Double
        If Double.TryParse(If(ConfigurationManager.AppSettings(key), ""), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, value) AndAlso value >= 0 Then
            Return value
        End If

        Return defaultValue

    End Function

    'Path of the design (from tbl_LabelDesigns by its code, never a path sent by the device) and the values of the label
    'printCode : the code printed as the barcode / QR of the label, one of the codes of the item (checked here)
    Private Function LabelDesignAndItem(itemRecID As String, printCode As String, labelDesign As String, ByRef designPath As String, ByRef errorMessage As String) As ClsBartender.LabelItem

        designPath = LabelDesignPath(labelDesign, errorMessage)
        If errorMessage <> "" Then Return Nothing

        Dim recID As Decimal
        If Not Decimal.TryParse(If(itemRecID, "").Trim, recID) Then
            errorMessage = "Please Scan An Item"
            Return Nothing
        End If

        printCode = If(printCode, "").Trim

        Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

            cnsql.Open()

            Using cmsql As New SqlCommand(" ", cnsql)

                cmsql.CommandTimeout = 600

                Dim item As Dictionary(Of String, Object) = LabelItemData(cmsql, recID, errorMessage)
                If errorMessage <> "" Then Return Nothing

                If Not LabelItemCodes(cmsql, recID).Any(Function(code) code("value").ToString = printCode) Then
                    errorMessage = "The Code To Print Does Not Belong To This Item, Please Choose It Again"
                    Return Nothing
                End If

                Return New ClsBartender.LabelItem With {
                    .Barcode = printCode,
                    .Reference = item("itemReference").ToString,
                    .Unit = item("itemUnit").ToString,
                    .Description = item("itemDescription").ToString,
                    .PriceLC = CDec(item("priceLC")).ToString("#,##0", Globalization.CultureInfo.InvariantCulture),
                    .CurrencyLC = item("currencyLC").ToString,
                    .PriceCC = CDec(item("priceCC")).ToString("#,##0.00", Globalization.CultureInfo.InvariantCulture),
                    .CurrencyCC = item("currencyCC").ToString,
                    .Volume = "",
                    .Weight = "",
                    .QR = LabelQRLink(recID)
                }

            End Using

        End Using

    End Function

    'Link printed in the QR bound to QR : LabelQRDefaultPath (Web.config) + "/" + record id of the item (only the id when the path is empty)
    Private Function LabelQRLink(recID As Decimal) As String

        Dim defaultPath As String = If(ConfigurationManager.AppSettings("LabelQRDefaultPath"), "").Trim.TrimEnd("/"c)
        Dim recIDText As String = recID.ToString("0", Globalization.CultureInfo.InvariantCulture)

        Return If(defaultPath = "", recIDText, defaultPath & "/" & recIDText)

    End Function

    'Record id of the item of a scanned label QR (LabelQRLink) : any link ending with "/<number>", so the labels printed before
    'a change of LabelQRDefaultPath still scan. "" when the text is not a link (a barcode or a reference never contains "://")
    Private Shared Function ItemRecIDFromLink(scannedText As String) As String

        Dim match As Text.RegularExpressions.Match = Text.RegularExpressions.Regex.Match(If(scannedText, "").Trim, "^[a-z][a-z0-9+.-]*://.*/([0-9]{1,18})/?$", Text.RegularExpressions.RegexOptions.IgnoreCase)
        Return If(match.Success, match.Groups(1).Value, "")

    End Function

    'The file of an active design : CLD_Document, a full path or a file name in CLD_TemplateFolder (.btw added when it has no extension)
    Private Function LabelDesignPath(labelDesign As String, ByRef errorMessage As String) As String

        If String.IsNullOrWhiteSpace(labelDesign) OrElse labelDesign.Trim.Length > 2 Then
            errorMessage = "Please Choose A Label Design"
            Return ""
        End If

        Dim designName, document, folder As String

        Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

            cnsql.Open()

            Using cmsql As New SqlCommand(" SELECT  LTRIM(RTRIM(CLD_Name)) ,
                                                    LTRIM(RTRIM(ISNULL(CLD_Document, ''))) ,
                                                    LTRIM(RTRIM(ISNULL(CLD_TemplateFolder, '')))
                                            FROM    dbo.tbl_LabelDesigns
                                            WHERE   CLD_Code = @Code
                                                    AND CLD_Active = 1 ", cnsql)

                cmsql.CommandTimeout = 600
                cmsql.Parameters.Add("@Code", SqlDbType.VarChar, 2).Value = labelDesign.Trim

                Using drDesign As SqlDataReader = cmsql.ExecuteReader()
                    If Not drDesign.Read() Then
                        errorMessage = "This Label Design Does Not Exist Or Is Not Active Anymore, Please Choose Another One"
                        Return ""
                    End If
                    designName = drDesign.GetString(0)
                    document = drDesign.GetString(1)
                    folder = drDesign.GetString(2)
                End Using

            End Using

        End Using

        If document = "" Then
            errorMessage = $"No Document Is Defined For The Label Design {designName}"
            Return ""
        End If

        Dim designPath As String
        Try
            If Not Path.IsPathRooted(document) AndAlso folder = "" Then
                errorMessage = $"No Folder Is Defined For The Label Design {designName} (Template Folder), Or Put The Full Path In Its Document"
                Return ""
            End If
            designPath = If(Path.IsPathRooted(document), document, Path.Combine(folder, document))
            If Path.GetExtension(designPath) = "" Then designPath &= ".btw"
        Catch ex As ArgumentException
            errorMessage = $"The Path Of The Label Design {designName} Is Not Valid : {folder} {document}"
            Return ""
        End Try

        If Not File.Exists(designPath) Then
            errorMessage = $"The File Of The Label Design {designName} Was Not Found On The Server ({designPath})"
            Return ""
        End If

        Return designPath

    End Function

    'The item with its prices in the local and the control currency : the preferred price type of the location (SL_PrefPrc),
    'its discount while its date is not passed, VAT included or not (exempt location or item), the local price calculated from
    'the control one when the location and the item recalculate it (at the rate of the working day)
    'The currencies are their codes in the ERP (Ac_Curr1 / Ac_Curr2)
    Private Function LabelItemData(cmsql As SqlCommand, itemRecID As Decimal, ByRef errorMessage As String) As Dictionary(Of String, Object)

        Dim item As New Dictionary(Of String, Object)

        cmsql.Parameters.Clear()
        cmsql.Parameters.Add("@ItemRecID", SqlDbType.Decimal).Value = itemRecID
        cmsql.CommandText = " SELECT CONVERT(VARCHAR(18), itemRecID) itemRecID ,
                                     itemReference ,
                                     itemDescription ,
                                     itemUnit
                              FROM   V_CPP_LabelItems
                              WHERE  itemRecID = @ItemRecID "

        Using drItem As SqlDataReader = cmsql.ExecuteReader()
            If Not drItem.Read() Then
                errorMessage = "This Item Does Not Exist Or Is Not Active"
                Return Nothing
            End If
            For column As Integer = 0 To drItem.FieldCount - 1
                item(drItem.GetName(column)) = drItem.GetValue(column)
            Next
        End Using

        cmsql.Parameters.Clear()
        cmsql.Parameters.Add("@ItemRecID", SqlDbType.Decimal).Value = itemRecID
        cmsql.Parameters.Add("@LocationCode", SqlDbType.VarChar, 2).Value = LocCode.Trim
        cmsql.Parameters.Add("@CompCode", SqlDbType.VarChar, 2).Value = If(ConfigurationManager.AppSettings("CompCode"), "").Trim
        cmsql.CommandText = " SELECT  currencyLC localCurrency ,
                                      currencyCC controlCurrency
                              FROM    V_CPP_LabelCurrencies
                              WHERE   companyCode = @CompCode ;

                              SELECT  TOP 1
                                      priceLC ,
                                      priceCC
                              FROM    V_CPP_LabelItemPrices
                              WHERE   itemRecID = @ItemRecID
                                      AND locationCode = @LocationCode
                                      AND companyCode = @CompCode ; "

        Dim localCurrency As String = "", controlCurrency As String = ""
        Dim priceLC As Decimal = 0, priceCC As Decimal = 0

        Using drPrices As SqlDataReader = cmsql.ExecuteReader()

            If drPrices.Read() Then
                localCurrency = drPrices("localCurrency").ToString.Trim
                controlCurrency = drPrices("controlCurrency").ToString.Trim
            End If

            'No price for the preferred price type of the location : 0
            If drPrices.NextResult() AndAlso drPrices.Read() Then
                If Not IsDBNull(drPrices("priceLC")) Then priceLC = CDec(drPrices("priceLC"))
                If Not IsDBNull(drPrices("priceCC")) Then priceCC = CDec(drPrices("priceCC"))
            End If

        End Using

        item("priceLC") = Math.Round(priceLC, 0)
        item("currencyLC") = localCurrency
        item("priceCC") = Math.Round(priceCC, 2)
        item("currencyCC") = controlCurrency

        Return item

    End Function

    'The codes the label can print as its barcode / QR (V_CPP_LabelItemCodes) : coding (the references of the item), its barcodes, its supplier references
    'Returns [{"group":"coding","title":"Reference","value":"IT001","isMainBarcode":0}, ...] without the empty ones and the repeated ones of a group
    Private Function LabelItemCodes(cmsql As SqlCommand, itemRecID As Decimal) As List(Of Dictionary(Of String, Object))

        cmsql.Parameters.Clear()
        cmsql.Parameters.Add("@ItemRecID", SqlDbType.Decimal).Value = itemRecID
        cmsql.CommandText = " SELECT   codeGroup ,
                                       codeTitle ,
                                       codeValue ,
                                       codeIsMainBarcode
                              FROM     V_CPP_LabelItemCodes
                              WHERE    itemRecID = @ItemRecID
                              ORDER BY codeSort , codeValue "

        Dim codes As New List(Of Dictionary(Of String, Object))

        Using drCodes As SqlDataReader = cmsql.ExecuteReader()
            While drCodes.Read()
                Dim group As String = drCodes("codeGroup").ToString
                Dim value As String = If(IsDBNull(drCodes("codeValue")), "", drCodes("codeValue").ToString.Trim)
                If value = "" OrElse codes.Any(Function(code) code("group").ToString = group AndAlso code("value").ToString = value) Then Continue While
                codes.Add(New Dictionary(Of String, Object) From {
                    {"group", group},
                    {"title", If(IsDBNull(drCodes("codeTitle")), "", drCodes("codeTitle").ToString.Trim)},
                    {"value", value},
                    {"isMainBarcode", If(CInt(drCodes("codeIsMainBarcode")) = 1, 1, 0)}})
            End While
        End Using

        Return codes

    End Function

    'The scanned code when it is one of the codes of the item, else the main barcode, a barcode, the reference
    Private Function LabelDefaultPrintCode(codes As List(Of Dictionary(Of String, Object)), scanned As String) As String

        If scanned <> "" AndAlso codes.Any(Function(code) code("value").ToString = scanned) Then Return scanned

        Dim mainBarcode = codes.FirstOrDefault(Function(code) CInt(code("isMainBarcode")) = 1)
        If mainBarcode IsNot Nothing Then Return mainBarcode("value").ToString

        Dim firstBarcode = codes.FirstOrDefault(Function(code) code("group").ToString = "barcodes")
        If firstBarcode IsNot Nothing Then Return firstBarcode("value").ToString

        Return If(codes.Count = 0, "", codes(0)("value").ToString)

    End Function

    'The active items (V_CPP_LabelItems) with the code (exact : equal, else containing it) in one of their codes (V_CPP_LabelItemCodes)
    'A typed search (not exact) also finds the items whose description contains the text
    Private Function LabelFindItems(cmsql As SqlCommand, code As String, exact As Boolean) As DataTable

        cmsql.Parameters.Clear()
        cmsql.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = code
        cmsql.Parameters.Add("@CodeLike", SqlDbType.NVarChar, 160).Value = "%" & code.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%"

        Dim compare As String = If(exact, "= @Code", "LIKE @CodeLike")

        cmsql.CommandText = $" SELECT TOP {LabelSearchLimit} CONVERT(VARCHAR(18), itemRecID) itemRecID ,
                                      itemReference ,
                                      itemDescription
                               FROM   V_CPP_LabelItems
                               WHERE  itemRecID IN ( SELECT itemRecID FROM V_CPP_LabelItemCodes WHERE codeValue {compare} )
                                      {If(exact, "", "OR itemDescription LIKE @CodeLike")}
                               ORDER BY itemReference "

        Dim dtItems As New DataTable
        Using daSql As New SqlDataAdapter(cmsql)
            daSql.Fill(dtItems)
        End Using

        Return dtItems

    End Function

    Private Function LabelRows(dtRows As DataTable) As List(Of Dictionary(Of String, Object))

        Return dtRows.Rows.Cast(Of DataRow).Select(Function(row) dtRows.Columns.Cast(Of DataColumn).ToDictionary(Function(column) column.ColumnName, Function(column) row(column))).ToList

    End Function

#End Region

#Region "Free Receiving"

    'The device downloads the whole stock file (V_CPP_ReceivingItems), the codes and the suppliers, then receives offline
    'The pages follow the record id (afterRecID = the last one stored), so a download can continue where it stopped
    Private Const ReceivingMaxPageSize As Integer = 5000

    'Counts of the download and the decimals of the quantities (SP_DecNo), also the connection test of the settings
    'Returns {itemsCount, barcodesCount, supplierRefsCount, suppliersCount, decimals}
    Public Function GetReceivingInfo() As String Implements IDataService.GetReceivingInfo

        Try

            Using dtInfo As New DataTable

                Using daSql As New SqlDataAdapter(" SELECT  ( SELECT COUNT(*) FROM V_CPP_ReceivingItems ) itemsCount ,
                                                            ( SELECT COUNT(*) FROM V_CPP_ReceivingItemCodes WHERE codeKind = 'barcode' ) barcodesCount ,
                                                            ( SELECT COUNT(*) FROM V_CPP_ReceivingItemCodes WHERE codeKind = 'supplierRef' ) supplierRefsCount ,
                                                            ( SELECT COUNT(*) FROM V_CPP_ReceivingSuppliers ) suppliersCount ,
                                                            ( SELECT TOP 1 ISNULL(CONVERT(VARCHAR, SP_DecNo), '0') FROM dbo.stk04_SysParam ) decimals ",
                                                  DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.Fill(dtInfo)
                End Using

                Return New JavaScriptSerializer().Serialize(LabelRows(dtInfo)(0))

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'afterRecID : "" for the first page, else the last itemRecID stored, returns "" after the last page
    Public Function GetReceivingItems(afterRecID As String, pageSize As Integer) As String Implements IDataService.GetReceivingItems

        Try

            Dim after As Decimal
            Dim errorMessage As String = ReceivingPageCheck(afterRecID, pageSize, after)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Return ReceivingPage($" SELECT TOP {pageSize} CONVERT(VARCHAR(18), itemRecID) itemRecID ,
                                                       itemReference ,
                                                       itemDescription ,
                                                       itemSupplier ,
                                                       itemCategory ,
                                                       itemSerie ,
                                                       itemSingleRef1 ,
                                                       itemSingleRef2 ,
                                                       itemUnit
                                     FROM     V_CPP_ReceivingItems
                                     WHERE    itemRecID > @After
                                     ORDER BY itemRecID ", after, "")

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'codeKind : barcode | supplierRef, afterRecID : "" for the first page, else the last codeRecID stored, returns "" after the last page
    Public Function GetReceivingItemCodes(codeKind As String, afterRecID As String, pageSize As Integer) As String Implements IDataService.GetReceivingItemCodes

        Try

            If codeKind <> "barcode" AndAlso codeKind <> "supplierRef" Then Return "Error Invalid Code Kind"

            Dim after As Decimal
            Dim errorMessage As String = ReceivingPageCheck(afterRecID, pageSize, after)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Return ReceivingPage($" SELECT TOP {pageSize} CONVERT(VARCHAR(18), codeRecID) codeRecID ,
                                                       CONVERT(VARCHAR(18), itemRecID) itemRecID ,
                                                       codeValue ,
                                                       CONVERT(VARCHAR(18), codeSupplierRecID) supplierRecID
                                     FROM     V_CPP_ReceivingItemCodes
                                     WHERE    codeKind = @CodeKind
                                              AND codeRecID > @After
                                     ORDER BY codeRecID ", after, codeKind)

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'All the suppliers in one call (a few hundred rows), returns "" when there is none
    Public Function GetReceivingSuppliers() As String Implements IDataService.GetReceivingSuppliers

        Try

            Return ReceivingPage(" SELECT   CONVERT(VARCHAR(18), supplierRecID) supplierRecID ,
                                            supplierCode ,
                                            supplierName
                                   FROM     V_CPP_ReceivingSuppliers
                                   WHERE    supplierRecID > @After
                                   ORDER BY supplierCode ", -1, "")

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Private Function ReceivingPageCheck(afterRecID As String, pageSize As Integer, ByRef after As Decimal) As String

        afterRecID = If(afterRecID, "").Trim

        If afterRecID = "" Then
            after = -1
        ElseIf Not Decimal.TryParse(afterRecID, after) Then
            Return "Invalid Start Record"
        End If

        If pageSize < 1 OrElse pageSize > ReceivingMaxPageSize Then Return $"Page Size Should Be Between 1 And {ReceivingMaxPageSize}"

        Return ""

    End Function

    Private Function ReceivingPage(sql As String, after As Decimal, codeKind As String) As String

        Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

            cnsql.Open()

            Using cmsql As New SqlCommand(sql, cnsql)

                cmsql.CommandTimeout = 600
                cmsql.Parameters.Add("@After", SqlDbType.Decimal).Value = after
                cmsql.Parameters.Add("@CodeKind", SqlDbType.VarChar, 20).Value = codeKind

                Using dtRows As New DataTable

                    Using daSql As New SqlDataAdapter(cmsql)
                        daSql.Fill(dtRows)
                    End Using

                    If dtRows.Rows.Count = 0 Then Return ""

                    Return GetJson(dtRows)

                End Using

            End Using

        End Using

    End Function

#End Region

#Region "Transfer"

    'The device downloads the items with their price in the location it transfers from, the lots of that location
    'that have an available qty (V_CPP_TransferStock) and the barcodes, then prepares the transfer offline
    'The locations (from and to) are V_CPP_TransferLocations, the pages follow the record id like Free Receiving

    'The locations of the settings and the destinations, also the connection test of the settings
    'Returns [{locationCode, locationDescription}, ...]
    Public Function GetTransferLocations() As String Implements IDataService.GetTransferLocations

        Try

            'isCurrent = 1 : the location of the service (LocCode of the Web.config), the one we transfer from
            Return TransferPage(" SELECT   locationCode ,
                                           locationDescription ,
                                           CASE WHEN locationCode = @Location THEN 1 ELSE 0 END isCurrent
                                  FROM     V_CPP_TransferLocations
                                  ORDER BY locationCode ", -1, LocCode)

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The location we transfer from and the decimals of the quantities (SP_DecNo)
    'Returns {locationCode, locationDescription, decimals}
    Public Function GetTransferInfo(location As String) As String Implements IDataService.GetTransferInfo

        Try

            Dim errorMessage As String = TransferServerLocationCheck(location)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Using dtInfo As DataTable = TransferRead(" SELECT locationCode , locationDescription ,
                                                              ( SELECT TOP 1 ISNULL(CONVERT(VARCHAR, SP_DecNo), '0') FROM dbo.stk04_SysParam ) decimals
                                                       FROM   V_CPP_TransferLocations
                                                       WHERE  locationCode = @Location ", location)

                If dtInfo.Rows.Count = 0 Then Return $"Error Location {location.Trim} Cannot Be Used For Transfers"

                Return New JavaScriptSerializer().Serialize(LabelRows(dtInfo)(0))

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The stock search of Transfer : every lot of the location with an available qty (less the transfers saved and not uploaded yet)
    'of the items that can be transferred, the device searches them by product group, reference, barcode or description
    'Returns [{TransferLotColumns}, ...]
    Public Function GetTransferStock(location As String) As String Implements IDataService.GetTransferStock

        Try

            Dim errorMessage As String = TransferServerLocationCheck(location)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Using dtLots As DataTable = TransferRead($" SELECT {TransferLotColumns(TransferLotAvailable)}
                                                         {TransferLotsFrom}
                                                         WHERE  S.locationCode = @Location AND {TransferLotAvailable} > 0 AND I.itemFourthParty <> 'Y'
                                                         ORDER BY I.itemDescription , S.lotExpiryDt , S.lotRecID ", location)
                If dtLots.Rows.Count = 0 Then Return ""
                Return GetJson(dtLots)
            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'A code scanned or typed (GetItem of frmTransferStock) : a barcode, a single reference or the reference of an item that can be transferred,
    'else the items whose reference starts with it (50 at most). The link of a label QR is the item itself
    'One row per lot with an available qty, an item without one has a row with lotRecID "" (its available qty is 0)
    'Returns [{TransferLotColumns}, ...], "" when no item that can be transferred has this code
    Public Function FindTransferItem(location As String, code As String) As String Implements IDataService.FindTransferItem

        Try

            Dim errorMessage As String = TransferServerLocationCheck(location)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim search As String = If(code, "").Replace(vbCr, "").Replace(vbLf, "").Trim
            If search = "" Then Return ""

            Dim linkRecID As String = ItemRecIDFromLink(search)
            Dim itemsSql As String
            If linkRecID <> "" Then
                itemsSql = $" SELECT I.itemRecID FROM V_CPP_TransferItems I WHERE I.itemRecID = {CDec(linkRecID).ToString("0", Globalization.CultureInfo.InvariantCulture)} "
            Else
                itemsSql = " SELECT I.itemRecID FROM V_CPP_TransferItems I
                             WHERE  I.itemReference = @Code OR I.itemSingleRef1 = @Code OR I.itemSingleRef2 = @Code
                                    OR EXISTS ( SELECT TOP 1 1 FROM V_CPP_ReceivingItemCodes C WHERE C.itemRecID = I.itemRecID AND C.codeKind = 'barcode' AND C.codeValue = @Code ) "
            End If

            Dim lotsSql As String = $" SELECT {TransferLotColumns("ISNULL(S.lotLeft, 0)")}
                                       FROM   V_CPP_TransferItems I
                                              LEFT JOIN ( SELECT S.* , {TransferLotAvailable} [lotLeft]
                                                          {TransferLotsFrom.Replace("INNER JOIN V_CPP_TransferItems I ON I.itemRecID = S.itemRecID", "")}
                                                          WHERE S.locationCode = @Location AND {TransferLotAvailable} > 0 ) S ON S.itemRecID = I.itemRecID
                                       WHERE  I.itemRecID IN ( ##ITEMS## )
                                       ORDER BY I.itemReference , S.lotExpiryDt , S.lotRecID "

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim parameters = Function() As SqlParameter()
                                     Return {TransferParam("@Location", location), New SqlParameter("@Code", SqlDbType.NVarChar, 100) With {.Value = search},
                                             New SqlParameter("@Start", SqlDbType.NVarChar, 102) With {.Value = search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%"}}
                                 End Function

                'The items first, then their lots (one query with both takes a long time when no item has the code)
                Dim itemIDs = Function(sql As String) As List(Of String)
                                  Return RORead(cnsql, Nothing, " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " & sql, parameters()).Rows.Cast(Of DataRow) _
                                             .Select(Function(row) CDec(row("itemRecID")).ToString("0", Globalization.CultureInfo.InvariantCulture)).ToList
                              End Function
                Dim items As List(Of String) = itemIDs(itemsSql)
                If items.Count = 0 AndAlso linkRecID = "" Then
                    items = itemIDs(" SELECT TOP 50 I.itemRecID FROM V_CPP_TransferItems I WHERE I.itemReference LIKE @Start ORDER BY I.itemReference ")
                End If
                If items.Count = 0 Then Return ""

                Dim dtLots As DataTable = RORead(cnsql, Nothing, " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " & lotsSql.Replace("##ITEMS##", String.Join(",", items)), parameters())

                If dtLots.Rows.Count = 0 Then Return ""
                Return GetJson(dtLots)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The available qty now of the lots of the transfer being prepared (less the transfers saved and not uploaded yet), 0 when a lot has none
    'lotRecIDs : the lot record ids separated by commas. Returns [{lotRecID, lotAvailable}, ...]
    Public Function GetTransferLotsAvailable(location As String, lotRecIDs As String) As String Implements IDataService.GetTransferLotsAvailable

        Try

            Dim errorMessage As String = TransferServerLocationCheck(location)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim ids As New List(Of String)
            For Each part As String In If(lotRecIDs, "").Split(","c)
                Dim value As Decimal
                If Decimal.TryParse(part.Trim, Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, value) Then ids.Add(value.ToString("0", Globalization.CultureInfo.InvariantCulture))
            Next
            If ids.Count = 0 Then Return ""

            Using dtLots As DataTable = TransferRead($" SELECT CONVERT(VARCHAR(18), S.lotRecID) lotRecID , {TransferLotAvailable} lotAvailable
                                                         {TransferLotsFrom}
                                                         WHERE  S.locationCode = @Location AND S.lotRecID IN ( {String.Join(",", ids.Distinct)} ) ", location)
                If dtLots.Rows.Count = 0 Then Return ""
                Return GetJson(dtLots)
            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The columns of a lot row : the lot (alias S, lotRecID "" when the item has no lot) and its item (alias I), available : the qty left
    Private Function TransferLotColumns(available As String) As String
        Return $" ISNULL(CONVERT(VARCHAR(18), S.lotRecID), '') lotRecID , CONVERT(VARCHAR(18), I.itemRecID) itemRecID , I.itemReference , I.itemDescription ,
                                                   I.itemUnit , I.itemSingleRef1 , I.itemSingleRef2 , I.itemProductGroup , I.itemProductGroupDesc , I.itemFourthParty ,
                                                   ISNULL(( SELECT TOP 1 C.codeValue FROM V_CPP_ReceivingItemCodes C WHERE C.itemRecID = I.itemRecID AND C.codeKind = 'barcode' ORDER BY C.codeRecID ), '') itemBarcode ,
                                                   ISNULL(S.lotExpiry, '') lotExpiry , ISNULL(S.lotBatch, '') lotBatch , ISNULL(S.lotColor, '') lotColor ,
                                                   ISNULL(S.lotGrid, '') lotGrid , ISNULL(S.lotSize, '') lotSize , {available} lotAvailable "
    End Function

    'A query of the location (read uncommitted like the ERP : a transaction open in the ERP does not block it), the location as VARCHAR
    Private Function TransferRead(sql As String, location As String) As DataTable
        Using cnsql As New SqlConnection(ROConnectionString())
            cnsql.Open()
            Return RORead(cnsql, Nothing, " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " & sql, TransferParam("@Location", location))
        End Using
    End Function

    Private Function TransferParam(name As String, value As String) As SqlParameter
        Return New SqlParameter(name, SqlDbType.VarChar, 10) With {.Value = If(value, "").Trim}
    End Function

    'The lots of a location (alias S) with their item (alias I) and the qty of the transfers saved by the portables and not uploaded yet (alias P)
    Private Const TransferLotsFrom As String = " FROM V_CPP_TransferStock S
                                                  INNER JOIN V_CPP_TransferItems I ON I.itemRecID = S.itemRecID
                                                  OUTER APPLY ( SELECT SUM(ISNULL(TRY_CONVERT(NUMERIC(18, 3), U.TransferQty), 0)) [pendingQty]
                                                                FROM   dbo._TRANSFER_tbl_upload_details U
                                                                WHERE  ISNULL(U.Uploaded, 'N') = 'N'
                                                                       AND U.FromLoc = S.locationCode
                                                                       AND U.STK_RECID = CONVERT(NVARCHAR(50), S.itemRecID)
                                                                       AND ISNULL(U.Expiry_Dt, '') = S.lotExpiryDt
                                                                       AND ISNULL(U.Lot_Nbr, '') = S.lotBatch
                                                                       AND ISNULL(U.Color, '') = S.lotColor
                                                                       AND ISNULL(U.Size, '') = S.lotSize ) P "

    'The qty that can still be transferred from a lot of TransferLotsFrom
    Private Const TransferLotAvailable As String = " ( S.lotAvailable - ISNULL(P.pendingQty, 0) ) "

    'Saves the transfer prepared on the portable into _TRANSFER_tbl_upload_details (uploaded later to the ERP), one transfer per destination
    'The checks of btnConfirm / LineKeyDown of frmTransferStock : the destinations, the items that can be transferred and the available qty
    'of the location (all the lines of a lot together, less the transfers saved and not uploaded yet), all the lines or none
    'saveID : the id of this save on the device, the same id saved again (no answer received) is not saved twice
    'transfer : [{itemRecID, lotRecID, destination, qty}, ...]
    'Returns {alreadySaved, transfers: [{transferID, destination, lines, qty}, ...]}
    Public Function SaveTransfer(location As String, machineName As String, saveID As String, transfer As String) As String Implements IDataService.SaveTransfer

        Try

            Dim errorMessage As String = TransferServerLocationCheck(location)
            If errorMessage <> "" Then Return "Error " & errorMessage

            Dim machine As String = If(machineName, "").Trim
            Dim save As String = If(saveID, "").Trim
            If machine = "" OrElse save = "" OrElse save.Length > 40 Then Return "Error Missing Data"
            If machine.Length > 50 Then machine = machine.Substring(0, 50)

            Dim lines As JArray = JArray.Parse(If(transfer, "[]"))
            If lines.Count = 0 Then Return "Error No Items To Send"

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                'The stock is read uncommitted like the ERP does : a transaction left open in the ERP does not block the save
                'The saves are one at a time (application lock), so two portables cannot send the same available qty
                Using trnsql As SqlTransaction = cnsql.BeginTransaction(IsolationLevel.ReadUncommitted)

                    Try

                        Dim lockResult As Integer = CInt(ROScalar(cnsql, trnsql, " DECLARE @Result INT
                                                                                    EXEC @Result = sp_getapplock @Resource = 'CPP_TransferSave', @LockMode = 'Exclusive',
                                                                                                                 @LockOwner = 'Transaction', @LockTimeout = 30000
                                                                                    SELECT @Result "))
                        If lockResult < 0 Then
                            trnsql.Rollback()
                            Return "Error The server is busy saving another transfer, press Save Transfer again"
                        End If

                        Dim result As String = TransferSave(cnsql, trnsql, location.Trim, machine, save, lines)
                        If result.StartsWith("Error") Then
                            trnsql.Rollback()
                        Else
                            trnsql.Commit()
                        End If
                        Return result

                    Catch
                        Try
                            trnsql.Rollback()
                        Catch exTRN As Exception
                        End Try
                        Throw
                    End Try

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    Private Function TransferSave(cnsql As SqlConnection, trnsql As SqlTransaction, location As String, machine As String, save As String, lines As JArray) As String

        Dim invariant As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

        'Saved already : the answer of the first save did not reach the device
        Dim dtSaved As DataTable = RORead(cnsql, trnsql, " SELECT [_id] [transferID] , MAX(ToLoc) [destination] , COUNT(*) [lines] , SUM(ISNULL(TRY_CONVERT(NUMERIC(18, 3), TransferQty), 0)) [qty]
                                                          FROM   dbo._TRANSFER_tbl_upload_details
                                                          WHERE  [_id] LIKE @SaveLike AND Machine = @Machine
                                                          GROUP BY [_id] ",
                                          ROParam("@SaveLike", save.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "-%"), ROParam("@Machine", machine))
        If dtSaved.Rows.Count > 0 Then
            Dim saved As New JObject
            saved("alreadySaved") = True
            saved("transfers") = ROJArray(dtSaved)
            Return saved.ToString(Newtonsoft.Json.Formatting.None)
        End If

        Dim destinations As New HashSet(Of String)(RORead(cnsql, trnsql, " SELECT locationCode FROM V_CPP_TransferLocations WHERE locationCode <> @Location ",
                                                          ROParam("@Location", location)).Rows.Cast(Of DataRow).Select(Function(row) row("locationCode").ToString.Trim.ToUpper))

        'The lines of each lot and destination, the qty of the lines of the same lot and destination are added
        Dim qtys As New Dictionary(Of String, Decimal)
        Dim lotQtys As New Dictionary(Of Decimal, Decimal)
        For Each line As JObject In lines
            Dim lotRecID, qty As Decimal
            Dim destination As String = If(line.Value(Of String)("destination"), "").Trim.ToUpper
            If destination = "" Then Return "Error Missing Data"
            If Not destinations.Contains(destination) Then Return "Error Invalid Location Code"
            If Not Decimal.TryParse(If(line.Value(Of String)("lotRecID"), ""), Globalization.NumberStyles.Number, invariant, lotRecID) Then Return "Error Item Not Found Or Not Active"
            If Not Decimal.TryParse(If(line.Value(Of String)("qty"), ""), Globalization.NumberStyles.Number, invariant, qty) OrElse qty <= 0 Then Return "Error Quantity Cannot Be Null or Zero"
            Dim key As String = lotRecID.ToString(invariant) & "|" & destination
            qtys(key) = If(qtys.ContainsKey(key), qtys(key), 0D) + qty
            lotQtys(lotRecID) = If(lotQtys.ContainsKey(lotRecID), lotQtys(lotRecID), 0D) + qty
        Next

        'The lots with their item (only the items that can be transferred) and their available qty
        Dim dtLots As DataTable = RORead(cnsql, trnsql, $" SELECT  S.lotRecID , S.itemRecID , S.lotExpiryDt , S.lotBatch , S.lotColor , S.lotSize , {TransferLotAvailable} [lotLeft] ,
                                                                   I.itemReference , I.itemDescription , I.itemBrandName , I.itemSingleRef1 , I.itemSingleRef2 , I.itemClass ,
                                                                   I.itemService , I.itemFourthParty , I.itemProductGroupDesc ,
                                                                   ISNULL(( SELECT TOP 1 C.codeValue FROM V_CPP_ReceivingItemCodes C WHERE C.itemRecID = S.itemRecID AND C.codeKind = 'barcode' ORDER BY C.codeRecID ), '') [itemBarcode]
                                                           {TransferLotsFrom}
                                                           WHERE   S.locationCode = @Location
                                                                   AND S.lotRecID IN ( {String.Join(",", lotQtys.Keys.Select(Function(key) key.ToString(invariant)))} ) ",
                                         ROParam("@Location", location), ROParam("@CompCode", If(ConfigurationManager.AppSettings("CompCode"), "").Trim))

        Dim lots As Dictionary(Of Decimal, DataRow) = dtLots.Rows.Cast(Of DataRow).ToDictionary(Function(row) CDec(row("lotRecID")))

        'The price of the items in the location, read at once (one lookup per lot takes minutes with V_CPP_LabelItemPrices)
        Dim prices As New Dictionary(Of Decimal, Decimal)
        If dtLots.Rows.Count > 0 Then
            For Each row As DataRow In RORead(cnsql, trnsql, $" SELECT itemRecID , MAX(priceLC) [priceLC] FROM V_CPP_LabelItemPrices
                                                                WHERE  locationCode = @Location AND companyCode = @CompCode
                                                                       AND itemRecID IN ( {String.Join(",", dtLots.Rows.Cast(Of DataRow).Select(Function(lot) CDec(lot("itemRecID")).ToString("0", invariant)).Distinct)} )
                                                                GROUP BY itemRecID ",
                                              ROParam("@Location", location), ROParam("@CompCode", If(ConfigurationManager.AppSettings("CompCode"), "").Trim)).Rows
                If Not IsDBNull(row("priceLC")) Then prices(CDec(row("itemRecID"))) = Math.Round(CDec(row("priceLC")), 0)
            Next
        End If
        Dim priceOf = Function(lot As DataRow) As Decimal
                          Dim price As Decimal = 0
                          prices.TryGetValue(CDec(lot("itemRecID")), price)
                          Return price
                      End Function

        'Not found : the lot is not in the location any more or its item cannot be transferred (halted, not active...)
        For Each lotRecID As Decimal In lotQtys.Keys
            If Not lots.ContainsKey(lotRecID) Then Return "Error Item Not Found Or Not Active"
            If lots(lotRecID)("itemFourthParty").ToString = "Y" Then Return "Error You Can’t Transact a Forth Party Item"
        Next

        'The available qty (btnConfirm of frmTransferStock) : a service item is not checked
        Dim itemsWithNoAvlQty As String = ""
        For Each lotRecID As Decimal In lotQtys.Keys
            Dim lot As DataRow = lots(lotRecID)
            If lot("itemService").ToString.ToUpper = "S" Then Continue For
            If CDec(lot("lotLeft")) - lotQtys(lotRecID) < 0 Then itemsWithNoAvlQty &= "- " & lot("itemDescription").ToString & " : " & vbCrLf
        Next
        If itemsWithNoAvlQty <> "" Then Return "Error Exist Items With No Available Quantity :" & vbCrLf & itemsWithNoAvlQty

        Dim currencyLC As String = If(ROScalar(cnsql, trnsql, " SELECT TOP 1 currencyLC FROM V_CPP_LabelCurrencies WHERE companyCode = @CompCode ",
                                               ROParam("@CompCode", If(ConfigurationManager.AppSettings("CompCode"), "").Trim)), "").ToString

        Dim cut = Function(text As Object, length As Integer) As Object
                      Dim value As String = If(text, "").ToString.Trim
                      Return If(value.Length > length, value.Substring(0, length), value)
                  End Function

        'One transfer per destination, the lines in the order of the device, inserted by blocks of rows (one command per block)
        Const rowsPerInsert As Integer = 80
        Dim transfers As New JArray
        For Each destination As String In qtys.Keys.Select(Function(key) key.Split("|"c)(1)).Distinct.OrderBy(Function(code) code)

            Dim transferID As String = save & "-" & destination
            Dim keys As List(Of String) = qtys.Keys.Where(Function(lineKey) lineKey.Split("|"c)(1) = destination).ToList
            Dim transferQty As Decimal = 0

            For first As Integer = 0 To keys.Count - 1 Step rowsPerInsert

                Dim values As New List(Of String)
                Dim parameters As New List(Of SqlParameter) From {
                    ROParam("@TransferID", transferID), ROParam("@Currency", cut(currencyLC, 50)), ROParam("@Location", location),
                    ROParam("@Destination", destination), ROParam("@Machine", machine)
                }

                For index As Integer = first To Math.Min(first + rowsPerInsert, keys.Count) - 1

                    Dim key As String = keys(index)
                    Dim lot As DataRow = lots(CDec(key.Split("|"c)(0)))
                    Dim n As String = index.ToString(invariant)
                    transferQty += qtys(key)

                    values.Add($" ( @TransferID , @RowNumber{n} , @ItemRecID{n} , @Reference{n} , @Description{n} , @SingleRef1{n} , @SingleRef2{n} , @Barcode{n} , @Brand{n} , @ProductGroup{n} ,
                                    @Size{n} , @Color{n} , @Expiry{n} , @Lot{n} , @Available{n} , @Qty{n} , @ItemClass{n} , @Price{n} , @Currency ,
                                    @Location , @Destination , @Machine , 'N' , 'N' , 'N' , GETDATE() ) ")
                    parameters.AddRange({
                        ROParam("@RowNumber" & n, (index + 1).ToString(invariant)),
                        ROParam("@ItemRecID" & n, CDec(lot("itemRecID")).ToString("0", invariant)), ROParam("@Reference" & n, cut(lot("itemReference"), 50)),
                        ROParam("@Description" & n, cut(lot("itemDescription"), 250)), ROParam("@SingleRef1" & n, cut(lot("itemSingleRef1"), 50)),
                        ROParam("@SingleRef2" & n, cut(lot("itemSingleRef2"), 50)), ROParam("@Barcode" & n, cut(lot("itemBarcode"), 50)),
                        ROParam("@Brand" & n, cut(lot("itemBrandName"), 50)), ROParam("@ProductGroup" & n, cut(lot("itemProductGroupDesc"), 50)),
                        ROParam("@Size" & n, lot("lotSize").ToString), ROParam("@Color" & n, lot("lotColor").ToString),
                        ROParam("@Expiry" & n, lot("lotExpiryDt").ToString), ROParam("@Lot" & n, lot("lotBatch").ToString),
                        ROParam("@Available" & n, CDec(lot("lotLeft")).ToString("0.###", invariant)), ROParam("@Qty" & n, qtys(key).ToString("0.###", invariant)),
                        ROParam("@ItemClass" & n, lot("itemClass").ToString), ROParam("@Price" & n, priceOf(lot).ToString("0.###", invariant))
                    })

                Next

                ROExecute(cnsql, trnsql, " INSERT INTO dbo._TRANSFER_tbl_upload_details
                                                  ( [_id] , RowNumber , STK_RECID , STK_REF , STK_DESC , STK_SingleRef1 , STK_SingleRef2 , dbar_barcode , br_Name , IT_DESC ,
                                                    Size , Color , Expiry_Dt , Lot_Nbr , Available_Qty , TransferQty , STK_ItmClass , DisplayPrice , PriceCurr ,
                                                    FromLoc , ToLoc , Machine , ParseError , Uploaded , UploadError , dtstmp )
                                           VALUES " & String.Join(",", values), parameters.ToArray)

            Next

            Dim saved As New JObject
            saved("transferID") = transferID
            saved("destination") = destination
            saved("lines") = keys.Count
            saved("qty") = transferQty
            transfers.Add(saved)

        Next

        Dim result As New JObject
        result("alreadySaved") = False
        result("transfers") = transfers
        Return result.ToString(Newtonsoft.Json.Formatting.None)

    End Function

    'Saves work on the location of the server only (LocCode of the Web.config)
    Private Function TransferServerLocationCheck(location As String) As String

        Dim errorMessage As String = TransferLocationCheck(location)
        If errorMessage <> "" Then Return errorMessage
        If location.Trim.ToUpper <> If(LocCode, "").Trim.ToUpper Then Return $"The location of the server is {LocCode}, open the transfer settings and save them"

        Return ""

    End Function

    Private Function TransferLocationCheck(location As String) As String

        If String.IsNullOrWhiteSpace(location) OrElse location.Trim.Length > 2 Then Return "Invalid Location"

        Return ""

    End Function

    Private Function TransferPage(sql As String, after As Decimal, location As String) As String

        Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

            cnsql.Open()

            'Read uncommitted like the ERP : a transaction open in the ERP does not stop the download
            Using cmsql As New SqlCommand(" SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " & sql, cnsql)

                cmsql.CommandTimeout = 600
                cmsql.Parameters.Add("@After", SqlDbType.Decimal).Value = after
                cmsql.Parameters.Add("@Location", SqlDbType.VarChar, 2).Value = If(location, "").Trim
                cmsql.Parameters.Add("@CompCode", SqlDbType.VarChar, 2).Value = If(ConfigurationManager.AppSettings("CompCode"), "").Trim

                Using dtRows As New DataTable

                    Using daSql As New SqlDataAdapter(cmsql)
                        daSql.Fill(dtRows)
                    End Using

                    If dtRows.Rows.Count = 0 Then Return ""

                    Return GetJson(dtRows)

                End Using

            End Using

        End Using

    End Function

#End Region

#Region "Reprint Documents"

    'Reprint Documents (proof of concept) : Client Receipt (RC) and Consumption Slip (CS), their A4 (laser) layouts
    'Same queries, reports and formulas as the direct print of frmReprintSlips in the ERP (PrintRC / PrintConsump), the report is
    'exported to PDF and the device prints it with the Android print service
    'The reports are the .rpt of the ERP (POSOReports), in the folder of the ReportsFolder setting

    Private Const ReprintDocumentsLimit As Integer = 200

    Private Const ReprintApp As String = "POSO"

    'The values the ERP keeps in its globals (gbComp, gbCurr1, Copercurr, VatLCRound, CRetVal...) for the location of the service
    Private Class ReprintContext
        Public CompCode, CompName, CompAddress, VatReg, VatPer, Year, Curr1, Curr2, OperCurr, LocName, ReceiptReport, RetVal As String
        Public VatLCRound, DecNo As Integer
        Public UserCode, UserPrinted As String
    End Class

    'Documents of the type (RC / CS) of the location between the two dates (dd/MM/yyyy), the newest first
    'Returns [{recID, docKey, docNumber, docDate, description, amount, currency, remarks}], "" when there is none
    Public Function GetReprintDocuments(docType As String, fromDate As String, toDate As String) As String Implements IDataService.GetReprintDocuments

        Try

            docType = If(docType, "").Trim.ToUpper

            Dim dateFrom, dateTo As Date
            If Not Date.TryParseExact(If(fromDate, "").Trim, "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, dateFrom) OrElse
               Not Date.TryParseExact(If(toDate, "").Trim, "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, dateTo) Then
                Return "Error Invalid Dates"
            End If
            If dateFrom > dateTo Then Return "Error The Start Date Should Be Before The End Date"

            Dim strsql As String

            Select Case docType

                Case "RC"

                    Dim fc As String = If(OperCurrency() = "FC", "FC", "")

                    strsql = $" SELECT TOP ({ReprintDocumentsLimit})
                                        INVH_RECID recID ,
                                        CONVERT(VARCHAR(20), INVH_RECID) docKey ,
                                        CONCAT(INVH_DOCTYP1, ' ', INVH_DOCPREFIX, ' ', INVH_SERNO) docNumber ,
                                        CONVERT(VARCHAR(10), INVH_DATE, 103) docDate ,
                                        ISNULL(INVH_CLNAME, '') description ,
                                        INVH_NET{fc} amount ,
                                        ISNULL(INVH_CURR{fc}, '') currency ,
                                        ISNULL(REPLACE(REPLACE(INVH_RKS, CHAR(13), ' '), CHAR(10), ' '), '') remarks
                                FROM    dbo.POS_SALEINVHEAD
                                WHERE   INVH_DOCTYP1 = 'RC'
                                        AND INVH_LOC = @Location
                                        AND INVH_DATE >= @FromDate
                                        AND INVH_DATE < DATEADD(DAY, 1, @ToDate)
                                        AND EXISTS ( SELECT 1
                                                     FROM   dbo.POS_STOCKTRANSACTION
                                                     WHERE  STT_LOCATION = INVH_LOC
                                                            AND STT_DOCTYPE1 = INVH_DOCTYP1
                                                            AND STT_PREFIX = INVH_DOCPREFIX )
                                ORDER BY INVH_DATE DESC , INVH_SERNO DESC "

                Case "CS"

                    'A reverse keeps the number of its consumption : the ERP prints both (same report query)
                    strsql = $" SELECT TOP ({ReprintDocumentsLimit})
                                        CONSH_RECID recID ,
                                        CONCAT(CONSH_RECID, '-', CONSH_STATUS) docKey ,
                                        CONCAT('CS ', CONSH_RECID) docNumber ,
                                        CONVERT(VARCHAR(10), CONSH_DATE, 103) docDate ,
                                        CASE WHEN CONSH_STATUS = 'N' THEN 'Normal' ELSE 'Reverse' END description ,
                                        NULL amount ,
                                        '' currency ,
                                        ISNULL(CONSH_REMARKS, '') remarks
                                FROM    dbo.POS_CONSUMPTIONH
                                WHERE   CONSH_LOCATION = @Location
                                        AND CONSH_DATE >= @FromDate
                                        AND CONSH_DATE < DATEADD(DAY, 1, @ToDate)
                                        AND EXISTS ( SELECT 1
                                                     FROM   dbo.POS_CONSUMPTIOND
                                                     WHERE  CONSD_LOC = CONSH_LOCATION
                                                            AND CONSD_HEADID = CONSH_RECID
                                                            AND CONSD_STATUS = CONSH_STATUS )
                                ORDER BY CONSH_DATE DESC , CONSH_RECID DESC "

                Case Else
                    Return "Error Invalid Type Of Document"

            End Select

            Using dtDocuments As New DataTable

                Using daSql As New SqlDataAdapter(strsql, DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                    daSql.SelectCommand.CommandTimeout = 600
                    daSql.SelectCommand.Parameters.AddWithValue("@Location", LocCode)
                    daSql.SelectCommand.Parameters.AddWithValue("@FromDate", dateFrom)
                    daSql.SelectCommand.Parameters.AddWithValue("@ToDate", dateTo)
                    daSql.Fill(dtDocuments)
                End Using

                If dtDocuments.Rows.Count = 0 Then Return ""

                Return GetJson(dtDocuments)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The document printed by the ERP (direct print, A4 layout) as a PDF (base 64), the device prints it (Android print)
    Public Function GetReprintDocumentPdf(docType As String, recID As String, userCode As String) As String Implements IDataService.GetReprintDocumentPdf

        Try
            Return Convert.ToBase64String(ReprintDocument(docType, recID, userCode, ""))
        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The document printed by the ERP (direct print, A4 layout) on a printer of the server : one of the printers the Windows user
    'of the service has (shared printers of other PCs included, \\PC\Printer), see GetReprintPrinters
    Public Function PrintReprintDocument(docType As String, recID As String, userCode As String, printerName As String) As String Implements IDataService.PrintReprintDocument

        Try

            printerName = If(printerName, "").Trim
            If printerName = "" Then Return "Error Please Choose A Printer"

            'Only a printer of the server, never any name sent by the device
            If Not Drawing.Printing.PrinterSettings.InstalledPrinters.Cast(Of String)().Any(Function(name) name.Equals(printerName, StringComparison.OrdinalIgnoreCase)) Then
                Return "Error Printer Not Found On The Server : " & printerName
            End If

            ReprintDocument(docType, recID, userCode, printerName)

            Return "Success"

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The printers of the server for the Windows user of the service, its default printer first
    'Returns [{"printerName":"\\PC\HP LaserJet","isDefault":1}, ...], "" when there is none
    Public Function GetReprintPrinters() As String Implements IDataService.GetReprintPrinters

        Try

            Dim defaultPrinter As String = New Drawing.Printing.PrinterSettings().PrinterName

            Dim printers = Drawing.Printing.PrinterSettings.InstalledPrinters.Cast(Of String)().
                           OrderBy(Function(name) If(name = defaultPrinter, 0, 1)).ThenBy(Function(name) name).
                           Select(Function(name) New Dictionary(Of String, Object) From {
                                      {"printerName", name},
                                      {"isDefault", If(name = defaultPrinter, 1, 0)}}).ToList

            If printers.Count = 0 Then Return ""

            Return New JavaScriptSerializer().Serialize(printers)

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'printerName "" : returns the PDF, otherwise prints on that printer of the server and returns Nothing
    Private Function ReprintDocument(docType As String, recID As String, userCode As String, printerName As String) As Byte()

        docType = If(docType, "").Trim.ToUpper

        Dim documentRecID As Decimal
        If Not Decimal.TryParse(If(recID, "").Trim, documentRecID) Then Throw New Exception("Invalid Document")

        Dim reportsFolder As String = If(ConfigurationManager.AppSettings("ReportsFolder"), "").Trim
        If reportsFolder = "" Then Throw New Exception("The Reports Folder Is Not Set (ReportsFolder In Web.config)")

        Dim context As ReprintContext = LoadReprintContext(userCode)

        'Unique per print : two devices of the same user can print together (the ERP names them by user only)
        Dim suffix As String = context.UserCode & "_" & Guid.NewGuid.ToString("N").Substring(0, 8)

        Select Case docType
            Case "RC" : Return ReprintClientReceipt(context, documentRecID, reportsFolder, suffix, printerName)
            Case "CS" : Return ReprintConsumption(context, documentRecID, reportsFolder, suffix, printerName)
            Case Else : Throw New Exception("Invalid Type Of Document")
        End Select

    End Function

    Private Function OperCurrency() As String
        Return ExecuteScalar_WOXACT($"SELECT ISNULL(OP_OPCurr, '') FROM dbo.STK04_OPParam WHERE OP_Loc = '{LocCode}'").Trim
    End Function

    'Everything in one query (one round trip per print) : year and currencies of the company of the location (same as
    'TemClientBalnc), company (frmMainMenus : no VAT strategy = no VAT), location (pos_splocation), system parameters,
    'operating currency, user name
    Private Function LoadReprintContext(userCode As String) As ReprintContext

        Dim context As New ReprintContext
        context.UserCode = If(userCode, "").Trim.Replace("'", "")
        If context.UserCode = "" OrElse context.UserCode.Length > 4 Then Throw New Exception("Invalid User")

        Using cnsql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))

            cnsql.Open()

            Using cmsql As New SqlCommand(" ", cnsql)

                cmsql.CommandTimeout = 600
                cmsql.Parameters.AddWithValue("@Location", LocCode)
                cmsql.Parameters.AddWithValue("@UserCode", context.UserCode)

                cmsql.CommandText = $" SELECT  AppComp.Ac_Comp , AppComp.Ac_Year , AppComp.Ac_Curr1 , AppComp.Ac_Curr2 ,
                                               Comp_Name , Comp_VatReg , Comp_vatper , Comp_VatStrat ,
                                               REPLACE(REPLACE(Comp_Addr, CHAR(10), ''), CHAR(13), ' ') Comp_Addr ,
                                               Loc_Desc , ISNULL(SL_VATLCRound, 0) SL_VATLCRound , ISNULL(SL_StkRetVal, '') SL_StkRetVal ,
                                               ISNULL(SL_RptReceipt, '') SL_RptReceipt ,
                                               SysParam.SP_DecNO , SysParam.SP_UserCN ,
                                               ( SELECT ISNULL(OP_OPCurr, '') FROM dbo.STK04_OPParam WHERE OP_Loc = @Location ) OP_OPCurr ,
                                               ( SELECT User_Name FROM dbo.Com_UserInfo WHERE User_Code = @UserCode ) User_Name
                                       FROM    dbo.STK04_Location
                                               INNER JOIN dbo.POS_SPLOCATION ON SL_LOC = Loc_Code
                                               OUTER APPLY ( SELECT TOP 1 Ac_Comp , Ac_Year , Ac_Curr1 , Ac_Curr2
                                                             FROM   dbo.Com_AppComp
                                                             WHERE  Ac_Code = '{ReprintApp}' AND Ac_Comp = Loc_CoCode
                                                             ORDER BY Ac_Year DESC ) AppComp
                                               LEFT JOIN dbo.Com_Company ON Comp_Code = AppComp.Ac_Comp
                                               OUTER APPLY ( SELECT TOP 1 SP_DecNO , SP_UserCN FROM dbo.stk04_SysParam ) SysParam
                                       WHERE   Loc_Code = @Location "

                Using drSql As SqlDataReader = cmsql.ExecuteReader

                    If Not drSql.Read Then Throw New Exception("The Location Is Not Defined In The POS")
                    If IsDBNull(drSql("Ac_Comp")) Then Throw New Exception("The Company Of The Location Is Not Defined")

                    context.CompCode = drSql("Ac_Comp").ToString.Trim
                    context.Year = drSql("Ac_Year").ToString.Trim
                    context.Curr1 = drSql("Ac_Curr1").ToString.Trim
                    context.Curr2 = drSql("Ac_Curr2").ToString.Trim

                    context.CompName = drSql("Comp_Name").ToString
                    context.VatReg = drSql("Comp_VatReg").ToString
                    context.CompAddress = drSql("Comp_Addr").ToString
                    context.VatPer = "0"
                    Dim vatPer As Single
                    If drSql("Comp_VatStrat").ToString <> "" AndAlso Single.TryParse(drSql("Comp_vatper").ToString, vatPer) Then
                        context.VatPer = vatPer.ToString(Globalization.CultureInfo.InvariantCulture)
                    End If

                    context.LocName = drSql("Loc_Desc").ToString
                    context.VatLCRound = CInt(Val(drSql("SL_VATLCRound").ToString))
                    context.RetVal = drSql("SL_StkRetVal").ToString.Trim
                    context.ReceiptReport = drSql("SL_RptReceipt").ToString.Trim

                    context.DecNo = CInt(Val(drSql("SP_DecNO").ToString))
                    context.OperCurr = drSql("OP_OPCurr").ToString.Trim

                    'User printed by code (SP_UserCN = 'C') otherwise by name, as UserPrint of the ERP
                    context.UserPrinted = If(drSql("SP_UserCN").ToString <> "C" AndAlso Not IsDBNull(drSql("User_Name")),
                                             drSql("User_Name").ToString, context.UserCode)

                End Using

            End Using

        End Using

        Return context

    End Function

    'Formulas every report of the ERP receives (PreviewProcedure)
    Private Function ReprintCommonFormulas(context As ReprintContext) As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {"fomCompName", ClsCrystal.FormulaText(context.CompName)},
            {"fomuser", ClsCrystal.FormulaText(context.UserPrinted)},
            {"PrintCopy", "'Y'"}}
    End Function

    'Report SQL of the ERP (PrintPODirectRep) : the query becomes a view, the report reads "select * from" the view
    'printerName "" : returns the PDF of the report, otherwise prints it on that printer of the server and returns Nothing
    Private Function ReprintReportOutput(context As ReprintContext, reportSql As String, viewName As String, reportPath As String,
                                         formulas As Dictionary(Of String, String), subReports As List(Of ClsCrystal.SubReportValues),
                                         companyLogo As Boolean, printerName As String, Optional orderBy As String = "") As Byte()

        Execute_WOXACT($"CREATE VIEW [dbo].[{viewName}] AS {reportSql}")

        Dim connectionString As String = DecryptString(ServerSent.lcConSql, DecPS("REDACTED"))
        Dim viewSql As String = $"select * from {viewName} WHERE 1=1{orderBy}"
        Dim logoSql As String = If(companyLogo, $"SELECT L_Img FROM dbo.COm_Logo WHERE L_Comp='{context.CompCode}'", "")

        If printerName = "" Then
            Return ClsCrystal.ExportPdf(reportPath, connectionString, viewSql, formulas, subReports, logoSql)
        End If

        ClsCrystal.PrintReport(reportPath, connectionString, viewSql, formulas, subReports, printerName, 1, logoSql)
        Return Nothing

    End Function

    'PrintRC of the ERP (Client Receipt, laser layout : SL_RptReceipt of the location or rptNormClReceipt.rpt)
    Private Function ReprintClientReceipt(context As ReprintContext, recID As Decimal, reportsFolder As String, suffix As String, printerName As String) As Byte()

        Dim tempSales As String = "Temp_SalesinvhRC_CPP_" & suffix
        Dim tempCash As String = "Temp_CshRC_CPP_" & suffix
        Dim tempPayMode As String = "Temp_PayMode_CPP_" & suffix
        Dim viewName As String = "VPP_CPP_ReprintRC_" & suffix

        Dim isFC As Boolean = (context.OperCurr = "FC")
        Dim fc As String = If(isFC, "FC", "")

        'Printing currency : the operating currency of the location (frmReprintSlips : LC / CC, otherwise the original one)
        Dim printingCurr As String
        If context.OperCurr = context.Curr1 Then
            printingCurr = context.Curr1
        ElseIf context.OperCurr = context.Curr2 Then
            printingCurr = context.Curr2
        Else
            printingCurr = "FC"
        End If

        Dim convertTo = Function(field As String, toCurr As String) As String
                            Return $" dbo.convert5FC('{ReprintApp}','{context.CompCode}','{context.Year}',{field},POS_SALEINVHEAD.INVH_Curr,'{toCurr}',POS_SALEINVHEAD.INVH_Rate,INVH_Date) "
                        End Function

        Try

            If Not RecordsExists($"SELECT TOP 1 1 FROM dbo.POS_SALEINVHEAD WHERE INVH_LOC = '{LocCode}' AND INVH_RECID = {recID} AND INVH_DOCTYP1 = 'RC'") Then
                Throw New Exception("Client Receipt Not Found")
            End If

            Execute_WOXACT($" SELECT * INTO [{tempSales}] FROM dbo.POS_SALEINVHEAD WHERE INVH_LOC = '{LocCode}' AND INVH_RECID = {recID} ",
                           $" SELECT POS_CASHTRANSACTION.* , INVH_CURR{fc} , INVH_RATE , INVH_RATEFC
                              INTO   [{tempCash}]
                              FROM   dbo.POS_CASHTRANSACTION
                                     INNER JOIN dbo.POS_SALEINVHEAD ON CT_LOCATION = INVH_LOC AND CT_INVHEADER = INVH_RECID
                              WHERE  CT_LOCATION = '{LocCode}' AND CT_INVHEADER = {recID} ")

            'FillPayMode (check info, account to and settlement type printed) : the PayTerm subreport
            Dim amountCV As String
            If printingCurr <> "FC" Then
                If printingCurr = context.Curr2 AndAlso isFC Then
                    amountCV = $" RTRIM(LTRIM(STR(( CASE WHEN CT_currcode <> '{context.Curr2}' THEN CTR_AmtCvCc ELSE 0 END ) * CASE WHEN ct_opsign = '+' THEN 1 ELSE -1 END, 18, 3))) "
                Else
                    amountCV = $" RTRIM(LTRIM(STR((CASE WHEN CT_currcode <> '{printingCurr}' THEN dbo.convert5FC('{ReprintApp}','{context.CompCode}','{context.Year}', Ct_amount,CT_CURRCODE,'{printingCurr}',CTR_RateCvlc,CT_OPDATE) ELSE 0 End) * CASE WHEN ct_opsign = '+' THEN 1 ELSE -1 END, 18, 3))) "
                End If
            Else
                amountCV = $" RTRIM(LTRIM(STR((CASE WHEN CT_currcode <> INVH_CURR{fc} THEN CT_amtOPCURR ELSE 0 End) * CASE WHEN ct_opsign = '+' THEN 1 ELSE -1 END, 18, 3))) "
            End If

            Execute_WOXACT($" CREATE TABLE [{tempPayMode}] ( INV_Loc VARCHAR(2) , Inv_recID NUMERIC(18, 0) , PaymentDesc VARCHAR(30) , Amount NUMERIC(18, 3) ,
                                                              Curr VARCHAR(3) , AmountCV NUMERIC(18, 3) , CT_chkNo VARCHAR(10) , CT_BankInfo VARCHAR(500) ,
                                                              CT_ValueDate DATETIME , CT_AccNo VARCHAR(14) , CT_SETTLTYPE VARCHAR(3) ) ",
                           $" INSERT INTO [{tempPayMode}]
                              SELECT  CT_Location , CT_INVHeader ,
                                      CASE WHEN ST_CODE = '314' OR ST_CODE = '315'
                                           THEN ST_DESC + '#' + CONVERT(VARCHAR, CT_BonAchatSer) + CASE WHEN ST_CODE = '314' AND CT_BonLoc <> '{LocCode}' THEN '-' + CT_BonLoc ELSE '' END
                                           ELSE ST_DESC
                                      END ST_DESC ,
                                      RTRIM(LTRIM(STR(Ct_amount * CASE WHEN ct_opsign = '+' THEN 1 ELSE -1 END, 18, 3))) , CT_currcode ,
                                      {amountCV} ,
                                      CT_chkNo , bnk_bankName COLLATE SQL_Latin1_General_CP1256_CI_AS + '/' + bnk_branch AS CT_BankInfo , CT_ValueDate ,
                                      CT_AccNo , CT_SETTLTYPE
                              FROM    [{tempCash}]
                                      INNER JOIN dbo.stk04_SETTLEMENTTYPE ON ct_location = st_loc AND CT_Settltype = ST_CODE
                                      INNER JOIN dbo.POS_CashTransCVRate ON ct_invheader = CTR_INVHEADER AND CTR_CREGISTER = ct_cregister
                                                                            AND CTR_OPNBR = ct_opnbr AND CTR_LOCATION = ct_location
                                      LEFT JOIN dbo.stk04_banks ON bnk_RecId = ct_bank
                              WHERE   CT_Location = '{LocCode}' AND CT_INVHeader IN ( {recID} ) ")

            'Report query of the laser layout (head office location : STK04_Employee)
            Dim strsql As New StringBuilder
            With strsql
                .Append(" SELECT distinct POS_SALEINVHEAD.INVH_VATTYPE,")
                .Append(" POS_SALEINVHEAD.INVH_LOC ,POS_SALEINVHEAD.INVH_RECID, POS_SALEINVHEAD.INVH_DOCTYP1, ")
                .Append(" POS_SALEINVHEAD.INVH_DOCPREFIX, POS_SALEINVHEAD.INVH_SERNO, ")
                .Append(" POS_SALEINVHEAD.INVH_DATE, POS_SALEINVHEAD.INVH_CLNAME, ")
                .Append(" ClFolio.Fo_addres INVH_CLADDTEL, ")
                .Append(" POS_SALEINVHEAD.INVH_RKS, ")

                If printingCurr = "FC" Then
                    .Append($" POS_SALEINVHEAD.INVH_CURR{fc} INVH_CURR, ")
                Else
                    .Append($" CONVERT(VARCHAR(3),'{printingCurr}') INVH_CURR, ")
                End If

                If printingCurr = "FC" Then
                    .Append($" POS_SALEINVHEAD.INVH_VATFC{If(isFC, "NEW", "")} INVH_VATFC , ")
                ElseIf printingCurr = context.Curr2 AndAlso isFC Then
                    .Append(" POS_SALEINVHEAD.INVH_VATFC INVH_VATFC , ")
                Else
                    .Append(convertTo("INVH_VATFC", printingCurr) & " INVH_VATFC , ")
                End If

                .Append(" POS_SALEINVHEAD.INVH_VATLC,")

                If printingCurr = "FC" Then
                    .Append($" POS_SALEINVHEAD.INVH_NET{fc} INVH_NET , ")
                ElseIf printingCurr = context.Curr2 AndAlso isFC Then
                    .Append(" POS_SALEINVHEAD.INVH_NET INVH_NET , ")
                Else
                    .Append(convertTo("INVH_NET", printingCurr) & " INVH_NET , ")
                End If

                .Append(" POS_SALEINVHEAD.INVH_CURRFC, POS_SALEINVHEAD.INVH_NETFC, ")
                .Append(" POS_SALEINVHEAD.INVH_DSTAMP,POS_Employee.E_Code,")
                .Append(" STK04_Location.Loc_VATNo,INVH_CLIENTSUP, ClFolio.fo_vatreg,LocFolio.FO_Phone1,")
                .Append(" POS_Employee.E_Name ,POS_SALEINVHEAD.INVH_USTAMP,POS_CASHTRANSACTION.CT_RVNo ,")
                .Append(" null CT_chkNo, null Bankinfo, null CT_ValueDate,")
                .Append(" Stk04_Clients.Cl_Code, Stk04_Clients.Cl_no, ")
                .Append($" convert(varchar(15),case when isnull(ca_clac,'') ='' then '' else Substring(isnull(ca_clac,''),1,11) + '{If(isFC, context.Curr2, context.OperCurr)}' end) as Acno ")
                .Append($" , COALESCE(Invh_VATPer,(SELECT TOP 1 cv_vatper FROM dbo.com_vatdate WHERE cv_comp = '{context.CompCode}' AND cv_stdate <= POS_SALEINVhead.INVH_DATE ORDER BY cv_stdate DESC)) cv_vatper ")

                .Append($" From [{tempSales}] as POS_SALEINVHEAD ")
                .Append(" inner join STK04_Employee POS_Employee On POS_SALEINVHEAD.INVH_emp = POS_Employee.E_Code ")
                .Append($" left join [{tempCash}] pos_cashtransaction On POS_CASHTRANSACTION.CT_INVHEADER = POS_SALEINVHEAD.INVH_RECID ")
                .Append(" and POS_SALEINVHEAD.INVH_LOC = pos_cashtransaction.ct_location ")
                .Append(" Inner join stk04_location On loc_code = invh_loc ")
                .Append(" left join stk04_banks on ct_bank = bnk_Recid ")
                .Append(" left join com_folio LocFolio On STK04_Location.Loc_Folio = LocFolio.Fo_serno and STK04_Location.Loc_FolioSub = LocFolio.Fo_subno ")
                .Append($" left join vclaccounts on INVH_ClientSup = Cl_RecId and ca_Comp ='{context.CompCode}' ")
                .Append(" left JOIN Stk04_Clients ON POS_SALEINVHEAD.INVH_CLIENTSUP = Stk04_Clients.CL_RecID ")
                .Append(" LEFT JOIN com_folio ClFolio ON Stk04_Clients.CL_FolioNo = ClFolio.Fo_serno AND Stk04_Clients.CL_FolioSubNo= ClFolio.Fo_subno")
            End With

            'rptParam of the laser layout (the payment modes are in the PayTerm subreport, SetType... stay empty as in the ERP)
            Dim formulas As Dictionary(Of String, String) = ReprintCommonFormulas(context)
            formulas("Company") = ClsCrystal.FormulaText(context.CompName)
            For Each setType As String In {"SetType", "SetType1", "SetType2", "SetType3", "SetType4", "SetType5", "SetType6"}
                formulas(setType) = "''"
            Next
            formulas("CompAddress") = ClsCrystal.FormulaText(context.CompAddress)
            formulas("Location") = ClsCrystal.FormulaText(LocCode & " - " & context.LocName)
            formulas("VatReg") = ClsCrystal.FormulaText(context.VatReg)
            formulas("Title") = ClsCrystal.FormulaText("Client Receipt : ")
            formulas("CompanyVatPer") = ClsCrystal.FormulaText(context.VatPer)
            formulas("Curr") = ClsCrystal.FormulaText(context.Curr1)
            formulas("PRound") = context.VatLCRound.ToString
            formulas("ShowCompName") = "'Y'"

            Dim payTerm As New ClsCrystal.SubReportValues With {.Name = "PayTerm"}
            payTerm.Parameters("@PayTermsStr") = $"SELECT * From {tempPayMode}"
            payTerm.Formulas("Pround") = context.VatLCRound.ToString
            payTerm.Formulas("Curr1") = ClsCrystal.FormulaText(context.Curr1)

            Dim reportName As String = If(context.ReceiptReport = "", "rptNormClReceipt.rpt", context.ReceiptReport)

            Return ReprintReportOutput(context, strsql.ToString, viewName, Path.Combine(reportsFolder, reportName), formulas,
                                       New List(Of ClsCrystal.SubReportValues) From {payTerm}, True, printerName)

        Finally
            DropSqlObject(viewName, "VIEW")
            DropSqlObject(tempSales, "TABLE")
            DropSqlObject(tempCash, "TABLE")
            DropSqlObject(tempPayMode, "TABLE")
        End Try

    End Function

    'PrintConsump of the ERP (Consumption Slip, laser layout rptConsumLaser.rpt)
    Private Function ReprintConsumption(context As ReprintContext, recID As Decimal, reportsFolder As String, suffix As String, printerName As String) As Byte()

        Dim viewName As String = "VPP_CPP_ReprintCS_" & suffix

        Try

            If Not RecordsExists($"SELECT TOP 1 1 FROM dbo.POS_CONSUMPTIONH WHERE CONSH_LOCATION = '{LocCode}' AND CONSH_RECID = {recID}") Then
                Throw New Exception("Consumption Not Found")
            End If

            Dim strsql As New StringBuilder
            With strsql
                .Append(" SELECT")
                .Append(" POS_CONSUMPTIONH.CONSH_RECID, CASE WHEN POS_CONSUMPTIONH.CONSH_STATUS = 'N' THEN 'Normal' ELSE 'Reverse' END CONSH_STATUS, POS_CONSUMPTIONH.CONSH_DATE, POS_CONSUMPTIONH.CONSH_CSHREG, POS_CONSUMPTIONH.CONSH_REMARKS,")
                .Append("CASE WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'I' THEN 'Internal Use' WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'P' THEN 'Production Use' ELSE 'Consumables & Services' END CONSD_TYPE,")
                .Append(" POS_CONSUMPTIOND.CONSD_QTY, POS_CONSUMPTIOND.CONSD_CURR, POS_CONSUMPTIOND.CONSD_COSTLC, POS_CONSUMPTIOND.CONSD_CSTCC, POS_CONSUMPTIOND.CONSD_USRKS,")
                .Append("POS_CONSUMPTIOND.CONSD_BARCODE,")

                'Code printed (SL_StkRetVal), named STK_REF as in the ERP
                Select Case context.RetVal
                    Case "1"
                        .Append(" isnull((SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Stk_Recid")
                        .Append(" AND ISNULL(dbar_expdate,'') = ISNULL(CONSD_ExpiryDt,'')")
                        .Append(" AND ISNULL(dbar_batch,'')=ISNULL(CONSD_BatchInfo,'')),STK_REF) ")
                    Case "2" : .Append(" STK_REF ")
                    Case "3" : .Append(" ISNULL(STK_SingleRef1,STK_REF) ")
                    Case "4" : .Append(" ISNULL(STK_SingleRef2,STK_REF) ")
                End Select

                .Append(" STK_REF, POS_CONSUMPTIOND.CONSD_MACHINE, POS_CONSUMPTIOND.CONSD_MACHPOOL,")
                .Append(" POS_STKFIL.STK_SUPPLIER, POS_STKFIL.STK_CATEGORY, POS_STKFIL.STK_SERIE, POS_STKFIL.STK_DESC, POS_STKFIL.STK_FCCODE,")
                .Append(" STK04_MACHINE.MACH_CODE1, STK04_MACHINE.MACH_CODE2,")
                .Append(" STK04_MACHINE.MACH_DESC,")
                .Append(" STK04_MachinePool.MACHP_CODE,")
                .Append(" STK04_MachinePool.MACHP_DESC,")
                .Append(" STK04_Location.Loc_Desc")
                .Append(" ,PjD_DivCode,PjD_Description ,PjS_SubDivCode ,PjS_Description ")
                .Append(" ,CASE WHEN SUBSTRING(POS_CONSUMPTIOND.CONSD_ExpiryDt,1,2) = '00' THEN SUBSTRING(POS_CONSUMPTIOND.CONSD_ExpiryDt,4,8) ELSE POS_CONSUMPTIOND.CONSD_ExpiryDt end CONSD_ExpiryDt ")
                .Append(" , POS_CONSUMPTIOND.CONSD_BatchInfo ")
                .Append(" , Meas_desc ")
                .Append(" From dbo.POS_CONSUMPTIOND POS_CONSUMPTIOND INNER JOIN STK04_STKFIL POS_STKFIL ")
                .Append(" ON POS_CONSUMPTIOND.CONSD_STKID = POS_STKFIL.STK_RECID ")
                .Append(" left join STK04_Measurements on stk_unit = Meas_Unit ")
                .Append(" INNER JOIN dbo.POS_CONSUMPTIONH POS_CONSUMPTIONH ")
                .Append(" ON POS_CONSUMPTIOND.CONSD_HEADID = POS_CONSUMPTIONH.CONSH_RECID and CONSH_LOCATION = CONSD_LOC ")
                .Append(" LEFT JOIN dbo.STK04_MACHINE STK04_MACHINE ON POS_CONSUMPTIOND.CONSD_MACHINE = STK04_MACHINE.MACH_RECID ")
                .Append(" LEFT JOIN dbo.STK04_MachinePool STK04_MachinePool ON POS_CONSUMPTIOND.CONSD_MACHPOOL = STK04_MachinePool.MACHP_RECID ")
                .Append(" INNER JOIN dbo.STK04_Location STK04_Location ON POS_CONSUMPTIONH.CONSH_LOCATION = STK04_Location.Loc_Code ")
                .Append(" LEFT JOIN tbl_Cst_ProjectDiv ON tbl_Cst_ProjectDiv.PjD_Recid = POS_CONSUMPTIOND.CONSD_DivRecid")
                .Append(" LEFT JOIN tbl_Cst_ProjectSubDiv ON tbl_Cst_ProjectSubDiv.PjS_DivRecid = tbl_Cst_ProjectDiv.PjD_Recid ")
                .Append(" AND tbl_Cst_ProjectSubDiv.PjS_Recid = POS_CONSUMPTIOND.CONSD_SubDivRecid ")
                'The ERP joins its temp table of the checked documents, here the one document
                .Append($" where CONSH_LOCATION ='{LocCode}' AND POS_CONSUMPTIONH.CONSH_RECID = {recID} ")
            End With

            Dim formulas As Dictionary(Of String, String) = ReprintCommonFormulas(context)
            formulas("Ccurr1") = ClsCrystal.FormulaText(context.Curr1)
            formulas("Ccurr2") = ClsCrystal.FormulaText(context.Curr2)
            formulas("location") = ClsCrystal.FormulaText(LocCode)
            formulas("decno") = context.DecNo.ToString
            formulas("Opcurr") = ClsCrystal.FormulaText(context.OperCurr)
            formulas("FoReference") = ClsCrystal.FormulaText(If(context.RetVal = "1", "Barcode", "Reference"))
            formulas("Pround") = context.VatLCRound.ToString
            formulas("FoPrintCost") = ClsCrystal.FormulaText(If(ReprintNegationPriv(context, "MnuConsumpSlip", "btnPrintCost"), "Y", "N"))

            Return ReprintReportOutput(context, strsql.ToString, viewName, Path.Combine(reportsFolder, "rptConsumLaser.rpt"), formulas,
                                       Nothing, False, printerName, " ORDER BY consh_recid DESC ")

        Finally
            DropSqlObject(viewName, "VIEW")
        End Try

    End Function

    'NegationPriv of the ERP : the button allowed to the user (exception of the user, otherwise one of his groups), one query
    Private Function ReprintNegationPriv(context As ReprintContext, stepName As String, button As String) As Boolean

        Return ExecuteScalar_WOXACT($" SELECT  CASE WHEN EXISTS ( SELECT 1 FROM dbo.Com_NegationButton
                                                                  WHERE Nbu_User = '{context.UserCode}' AND Nbu_Comp = '{context.CompCode}'
                                                                        AND Nbu_Appl = '{ReprintApp}' AND Nbu_Module = '{ReprintApp}'
                                                                        AND Nbu_Step = '{stepName}' AND Nbu_Button = '{button}' AND Nbu_Exception = 1 )
                                                    THEN ( SELECT TOP 1 CONVERT(INT, Nbu_hasNeg) FROM dbo.Com_NegationButton
                                                           WHERE Nbu_User = '{context.UserCode}' AND Nbu_Comp = '{context.CompCode}'
                                                                 AND Nbu_Appl = '{ReprintApp}' AND Nbu_Module = '{ReprintApp}'
                                                                 AND Nbu_Step = '{stepName}' AND Nbu_Button = '{button}' AND Nbu_Exception = 1 )
                                                    WHEN EXISTS ( SELECT 1 FROM dbo.Com_GroupNegButton
                                                                         INNER JOIN dbo.Com_UserInfoGroup ON Nbg_Group = UIL_GrpCode
                                                                  WHERE UIL_UserCode = '{context.UserCode}' AND Nbg_Comp = '{context.CompCode}'
                                                                        AND Nbg_Appl = '{ReprintApp}' AND Nbg_Module = '{ReprintApp}'
                                                                        AND Nbg_Step = '{stepName}' AND Nbg_Button = '{button}' )
                                                    THEN 1 ELSE 0
                                               END ") = "1"

    End Function

#End Region

#Region "Receiving Order"

    'Receiving Order (FrmDO "Receiving Order" : trading, barcode and expiry items) works online : the device searches the items
    'and saves the whole RO in one call, the server checks everything again before saving (same rules as FrmDO)
    'Every read and write goes through the V_CPP_RO* / V_CPP_* views so the super store only needs its own version of the scripts
    'ReceivingAppCode (web.config, PORD2 by default) : the application of the item privileges (STK04_APPSTKPRIV) and of the currencies
    'Dates from the device : yyyy-MM-dd, expiry and production dates : DD.MM.YYYY (00 for the day = the whole month)

    Private Class ROSettings
        Public AppCode As String
        Public CompCode As String
        Public Year As Integer
        Public CurrencyLC As String
        Public CurrencyCC As String
    End Class

    Private Class ROLot
        Public Expiry As String
        Public Batch As String
        Public Production As String
        Public Qty As Decimal
        Public Barcodes As New List(Of String)
    End Class

    Private Class ROLine
        Public LineRecID As Decimal
        Public ItemRecID As Decimal
        Public SupplierRef As String
        Public Status As String
        Public Qty As Decimal
        Public Lots As New List(Of ROLot)
        Public Barcodes As New List(Of String)
        Public Item As DataRow
        'An item of the pending stock file : ItemRecID is its pending record id until the item is created in the stock file
        Public IsPending As Boolean
        Public PendingRecID As Decimal
    End Class

    Private Function ROConnectionString() As String
        Return DecryptString(ServerSent.lcConSql, DecPS("REDACTED"))
    End Function

    Private Function ROAppCode() As String
        Dim appCode As String = If(ConfigurationManager.AppSettings("ReceivingAppCode"), "").Trim
        Return If(appCode = "", "PORD2", appCode)
    End Function

    Private Function ROIsSuperUser(userCode As String) As Boolean
        Dim code As String = If(userCode, "").Trim.ToUpper
        Return code = "SA" OrElse code = "SU"
    End Function

    'The items the user can purchase (AppStkCond with the brand) : the item tag in the application and the brand
    Private Function ROItemAccess(userCode As String, itemAlias As String) As String
        If ROIsSuperUser(userCode) Then Return ""
        Return $" AND EXISTS ( SELECT TOP 1 1 FROM V_CPP_UserItemTags UIT WHERE UIT.userCode = @UserCode AND UIT.appCode = @AppCode AND UIT.itemTag = {itemAlias}.itemTag )
                  AND EXISTS ( SELECT TOP 1 1 FROM V_CPP_UserPurchaseAccess UPA WHERE UPA.userCode = @UserCode AND UPA.brandCode <> '' AND UPA.brandCode = {itemAlias}.itemBrand ) "
    End Function

    'The suppliers the user can purchase from (txtSupp_Validating of FrmDO)
    Private Function ROSupplierAccess(userCode As String, supplierAlias As String) As String
        If ROIsSuperUser(userCode) Then Return ""
        Return $" AND EXISTS ( SELECT TOP 1 1 FROM V_CPP_UserPurchaseAccess UPS WHERE UPS.userCode = @UserCode AND UPS.supplierCode <> '' AND UPS.supplierCode = {supplierAlias}.supplierCode ) "
    End Function

    Private Function ROParam(name As String, value As Object) As SqlParameter
        Return New SqlParameter(name, If(value, DBNull.Value))
    End Function

    'The user and application parameters of ROItemAccess / ROSupplierAccess, with the other parameters of the query
    Private Function ROUserParams(userCode As String, ParamArray others() As SqlParameter) As SqlParameter()
        Dim all As New List(Of SqlParameter) From {
            ROParam("@UserCode", If(userCode, "").Trim.ToUpper),
            ROParam("@AppCode", ROAppCode())
        }
        all.AddRange(others)
        Return all.ToArray
    End Function

    Private Function RORead(cnsql As SqlConnection, trnsql As SqlTransaction, sql As String, ParamArray parameters() As SqlParameter) As DataTable
        Using cmsql As New SqlCommand(sql, cnsql, trnsql)
            cmsql.CommandTimeout = 600
            cmsql.Parameters.AddRange(parameters)
            Dim dtRows As New DataTable
            Using daSql As New SqlDataAdapter(cmsql)
                daSql.Fill(dtRows)
            End Using
            cmsql.Parameters.Clear()
            Return dtRows
        End Using
    End Function

    Private Function ROExecute(cnsql As SqlConnection, trnsql As SqlTransaction, sql As String, ParamArray parameters() As SqlParameter) As Integer
        Using cmsql As New SqlCommand(sql, cnsql, trnsql)
            cmsql.CommandTimeout = 600
            cmsql.Parameters.AddRange(parameters)
            Dim count As Integer = cmsql.ExecuteNonQuery()
            cmsql.Parameters.Clear()
            Return count
        End Using
    End Function

    Private Function ROScalar(cnsql As SqlConnection, trnsql As SqlTransaction, sql As String, ParamArray parameters() As SqlParameter) As Object
        Using cmsql As New SqlCommand(sql, cnsql, trnsql)
            cmsql.CommandTimeout = 600
            cmsql.Parameters.AddRange(parameters)
            Dim value As Object = cmsql.ExecuteScalar()
            cmsql.Parameters.Clear()
            Return value
        End Using
    End Function

    'One query outside a transaction, the rows as JSON ("" when there is none)
    Private Function ROJson(sql As String, ParamArray parameters() As SqlParameter) As String
        Using cnsql As New SqlConnection(ROConnectionString())
            cnsql.Open()
            Using dtRows As DataTable = RORead(cnsql, Nothing, sql, parameters)
                If dtRows.Rows.Count = 0 Then Return ""
                Return GetJson(dtRows)
            End Using
        End Using
    End Function

    Private Function ROJArray(dtRows As DataTable) As JArray
        If dtRows.Rows.Count = 0 Then Return New JArray
        Return JArray.Parse(GetJson(dtRows))
    End Function

    'Year and currencies of the application (Com_AppComp) for the conversions of the costs (convert5)
    Private Function ROGetSettings(cnsql As SqlConnection, trnsql As SqlTransaction) As ROSettings
        Dim compCode As String = If(ConfigurationManager.AppSettings("CompCode"), "").Trim
        Dim dtApp As DataTable = RORead(cnsql, trnsql, " SELECT TOP 1 Ac_Year , Ac_Comp , Ac_Curr1 , Ac_Curr2
                                                         FROM   dbo.Com_AppComp
                                                         WHERE  Ac_Code = @AppCode AND ( @CompCode = '' OR Ac_Comp = @CompCode )
                                                         ORDER BY Ac_Year DESC ",
                                        ROParam("@AppCode", ROAppCode()), ROParam("@CompCode", compCode))
        If dtApp.Rows.Count = 0 Then Throw New Exception("The application " & ROAppCode() & " is not defined for the company")
        Return New ROSettings With {
            .AppCode = ROAppCode(),
            .CompCode = dtApp.Rows(0)("Ac_Comp").ToString.Trim,
            .Year = CInt(dtApp.Rows(0)("Ac_Year")),
            .CurrencyLC = dtApp.Rows(0)("Ac_Curr1").ToString.Trim,
            .CurrencyCC = dtApp.Rows(0)("Ac_Curr2").ToString.Trim
        }
    End Function

    Private Function ROParseDate(text As String, ByRef value As Date) As Boolean
        Return Date.TryParseExact(If(text, "").Trim, "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, value)
    End Function

    'DD.MM.YYYY with 00 for the day (the whole month) : lastDay is the last day the date covers (LstExpiry_BeforeColUpdate of FrmDO)
    Private Function ROLotDate(text As String, ByRef lastDay As Date) As Boolean
        If text Is Nothing OrElse Not System.Text.RegularExpressions.Regex.IsMatch(text, "^\d{2}\.\d{2}\.\d{4}$") Then Return False
        Dim day As Integer = CInt(text.Substring(0, 2))
        Dim month As Integer = CInt(text.Substring(3, 2))
        Dim year As Integer = CInt(text.Substring(6, 4))
        If month < 1 OrElse month > 12 OrElse year < 1800 Then Return False
        If day = 0 Then
            lastDay = New Date(year, month, Date.DaysInMonth(year, month))
            Return True
        End If
        If day > Date.DaysInMonth(year, month) Then Return False
        lastDay = New Date(year, month, day)
        Return True
    End Function

    'The qty in the consolidated unit (txtQty_Validating of FrmDO, V_Pord2_GetStkMeas)
    Private Function ROConsQty(item As DataRow, qty As Decimal) As Decimal
        Dim unitPack As Decimal = CDec(item("itemUnitPack"))
        Dim measValue As Decimal = CDec(item("itemMeasValue"))
        If unitPack = 0 OrElse measValue = 0 Then Return 0
        Dim multiply As Boolean = item("itemOperator").ToString.Trim <> "/"
        If item("itemMeasSign").ToString.Trim = "/" Then
            Return Math.Round(If(multiply, unitPack * qty / measValue, qty / unitPack / measValue), 3)
        End If
        Return Math.Round(If(multiply, qty * unitPack * measValue, qty / unitPack * measValue), 3)
    End Function

    'The item columns sent to the device
    Private Const ROItemColumns As String = " I.itemRecID , I.itemReference , I.itemSingleRef1 , I.itemBarcode , I.itemDescription , I.itemClass ,
                                              I.itemUnit , I.itemSubUnit , I.itemVatType , I.itemCostCC , I.itemQtyControl , I.itemMeasType , I.itemConsFactor ,
                                              CASE WHEN I.itemHaltPurchase = 'Y' AND I.itemHaltTill > CONVERT(DATE, GETDATE()) THEN 1 ELSE 0 END [itemHalted] ,
                                              ISNULL(CONVERT(VARCHAR(10), I.itemBBFDate, 120), '') [itemBBFDate] "

    'Why an item cannot be received on this date ("" when it can) : halted activity, BBF date after the RO date (GetItem of FrmDO)
    Private Function ROItemBlock(item As DataRow, roDate As Date) As String
        If CInt(item("itemHalted")) = 1 Then Return "Item's Activity Is Halted!"
        Dim bbfDate As Date
        If Date.TryParseExact(item("itemBBFDate").ToString, "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, bbfDate) _
            AndAlso bbfDate > roDate Then
            Return "This Item Has A BBF Date > than Receiving Order's Date. BBF Date : " & bbfDate.ToString("dd/MM/yyyy")
        End If
        Return ""
    End Function

    'Returns [{supplierRecID, supplierCode, supplierName, supplierForeign, supplierCountry, supplierCurrency}, ...]
    Public Function GetROSuppliers(userCode As String) As String Implements IDataService.GetROSuppliers

        Try

            Return ROJson($" SELECT  S.supplierRecID , S.supplierCode , S.supplierName , S.supplierForeign , S.supplierCountry , S.supplierCurrency
                             FROM    V_CPP_ROSuppliers S
                             WHERE   1 = 1 {ROSupplierAccess(userCode, "S")}
                             ORDER BY S.supplierName , S.supplierCode ", ROUserParams(userCode))

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Returns [{locationCode, locationDescription}, ...]
    Public Function GetROLocations() As String Implements IDataService.GetROLocations

        Try

            'Only the location of the service (LocCode of the Web.config) : the user receives to the location he is in
            Return ROJson(" SELECT locationCode , locationDescription FROM V_CPP_ROLocations WHERE locationCode = @LocCode ",
                          New SqlParameter("@LocCode", SqlDbType.VarChar, 2) With {.Value = LocCode.Trim})

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The stock search of the Receiving Order (GetItems with searchFor = RO) : any part of the reference, single references,
    'description, a barcode or a supplier reference of the supplier. itemPrice = the average cost in the control currency
    Private Function GetROItems(itemProductGroup As String, searchString As String, userCode As String, supplierRecID As String) As String

        Dim supplier As Decimal = 0
        Decimal.TryParse(If(supplierRecID, "").Trim, supplier)
        Dim group As String = If(itemProductGroup, "").Trim
        If group.ToUpper = "ALL" Then group = ""
        Dim search As String = If(searchString, "").Replace(vbCr, "").Replace(vbLf, "").Trim
        Dim searchLike As String = "%" & search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%"

        Using cnsql As New SqlConnection(ROConnectionString())

            cnsql.Open()

            Dim settings As ROSettings = ROGetSettings(cnsql, Nothing)

            Dim sql As String = $" SELECT TOP 5000 I.itemRecID , I.itemReference , I.itemBarcode , I.itemDescription , I.itemVatType , 0 [itemVatPerc] ,
                                          I.itemProductGroup , I.itemUnit , CONVERT(NUMERIC(18, 3), I.itemCostCC) [itemPrice] , @CurrencyCC [itemCurrency] ,
                                          ISNULL(( SELECT SUM(ISNULL(Q.stockAvailable, 0)) FROM V_CPP_StockQty Q WHERE Q.itemRecID = I.itemRecID ), 0) [itemAvailableQty] ,
                                          '' [itemPromotionCriteria] ,
                                          CONCAT(I.itemReference, '|', I.itemBarcode, '|', I.itemSingleRef1, '|', I.itemDescription) [itemSearchString]
                                   FROM   V_CPP_ROItems I
                                   WHERE  1 = 1 {ROItemAccess(userCode, "I")} "

            If group <> "" Then sql &= " AND I.itemProductGroup = @ProductGroup "

            'The QR of a label (link ending with the record id) : the item itself
            Dim linkRecID As String = ItemRecIDFromLink(search)
            If linkRecID <> "" Then
                sql &= " AND I.itemRecID = @LinkRecID "
            ElseIf search <> "" Then
                sql &= " AND ( I.itemReference LIKE @SearchLike OR I.itemBarcode LIKE @SearchLike OR I.itemSingleRef1 LIKE @SearchLike OR I.itemDescription LIKE @SearchLike
                               OR EXISTS ( SELECT TOP 1 1 FROM V_CPP_ROItemCodes C WHERE C.itemRecID = I.itemRecID AND C.itemCode = @Search
                                                                                     AND ( C.codeType <> 'S' OR C.supplierRecID = @SupplierRecID ) ) ) "
            End If

            sql &= " ORDER BY I.itemDescription "

            Using dtItems As DataTable = RORead(cnsql, Nothing, sql, ROUserParams(userCode,
                                                ROParam("@CurrencyCC", settings.CurrencyCC), ROParam("@ProductGroup", group),
                                                ROParam("@LinkRecID", If(linkRecID = "", 0D, CDec(linkRecID))), ROParam("@Search", search),
                                                ROParam("@SearchLike", searchLike), ROParam("@SupplierRecID", supplier)))
                If dtItems.Rows.Count = 0 Then Return ""
                Return GetJson(dtItems)
            End Using

        End Using

    End Function

    'A code scanned or typed (itemValidation of FrmDO) : reference, single reference 1 or 2, barcode, supplier reference of the supplier
    'Returns [{item..., supplierRef, codeExpiry, codeBatch, blockMessage}, ...] : several rows when the code is used by several items,
    '"" when no item the user can receive has this code
    'supplierRef : the code when it is a supplier reference, else the first supplier reference of the item for this supplier, else its single reference 1
    Public Function FindROItem(userCode As String, supplierRecID As String, code As String, roDate As String) As String Implements IDataService.FindROItem

        Try

            Dim supplier As Decimal
            If Not Decimal.TryParse(If(supplierRecID, "").Trim, supplier) Then Return "Error Choose The Supplier First"
            Dim receivingDate As Date
            If Not ROParseDate(roDate, receivingDate) Then receivingDate = Date.Today
            Dim search As String = If(code, "").Replace(vbCr, "").Replace(vbLf, "").Trim
            If search = "" Then Return ""

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim dtItems As DataTable = RORead(cnsql, Nothing, $" SELECT  {ROItemColumns} , C.codeType , C.codeExpiry , C.codeBatch , C.itemCode ,
                                                                             ( SELECT TOP 1 LTRIM(RTRIM(R.supplierRef)) FROM V_CPP_ItemSupplierRefs R
                                                                               WHERE R.itemRecID = I.itemRecID AND R.supplierRecID = @SupplierRecID AND ISNULL(R.supplierRef, '') <> ''
                                                                               ORDER BY R.refRecID ) [itemSupplierRef]
                                                                     FROM    V_CPP_ROItemCodes C
                                                                             INNER JOIN V_CPP_ROItems I ON I.itemRecID = C.itemRecID
                                                                     WHERE   C.itemCode = @Code
                                                                             AND ( C.codeType <> 'S' OR C.supplierRecID = @SupplierRecID )
                                                                             {ROItemAccess(userCode, "I")}
                                                                     ORDER BY I.itemRecID , CASE C.codeType WHEN 'S' THEN 0 WHEN 'B' THEN 1 ELSE 2 END ",
                                                  ROUserParams(userCode, ROParam("@Code", search), ROParam("@SupplierRecID", supplier)))

                Dim settings As ROSettings = ROGetSettings(cnsql, Nothing)

                'One row per item, the supplier reference first then the barcode (it can give the expiry / lot)
                Dim result As New JArray
                Dim seen As New HashSet(Of String)
                For Each row As DataRow In dtItems.Rows
                    If Not seen.Add(row("itemRecID").ToString) Then Continue For
                    Dim supplierRef As String = If(row("codeType").ToString = "S", row("itemCode").ToString.Trim, row("itemSupplierRef").ToString.Trim)
                    Dim item As JObject = ROItemJson(row, supplierRef, receivingDate, settings, False)
                    item("codeExpiry") = row("codeExpiry").ToString
                    item("codeBatch") = row("codeBatch").ToString
                    result.Add(item)
                Next

                'Not in the stock file : an item of the pending stock file (GetPendingInfo of FrmDO), created when the RO is saved
                If result.Count = 0 Then
                    Dim dtPending As DataTable = RORead(cnsql, Nothing, $" SELECT {ROItemColumns} FROM V_CPP_ROPendingItems I
                                                                          WHERE ( I.itemReference = @Code OR I.itemSingleRef1 = @Code OR I.itemBarcode = @Code )
                                                                                {ROItemAccess(userCode, "I")}
                                                                          ORDER BY I.itemRecID ",
                                                        ROUserParams(userCode, ROParam("@Code", search)))
                    For Each row As DataRow In dtPending.Rows
                        result.Add(ROItemJson(row, "", receivingDate, settings, True))
                    Next
                End If

                If result.Count = 0 Then Return ""
                Return result.ToString(Newtonsoft.Json.Formatting.None)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'An item for the device (ROItemColumns of V_CPP_ROItems or V_CPP_ROPendingItems) : supplierRef = the one given, else its single reference 1,
    'itemCurrency = the control currency of the cost, isPending = an item of the pending stock file (cost 0, created on the save)
    Private Function ROItemJson(row As DataRow, supplierRef As String, receivingDate As Date, settings As ROSettings, isPending As Boolean) As JObject
        Dim reference As String = If(supplierRef, "").Trim
        If reference = "" Then reference = row("itemSingleRef1").ToString.Trim
        If reference = "" Then reference = row("itemReference").ToString.Trim
        Return New JObject From {
            {"itemRecID", row("itemRecID").ToString},
            {"itemReference", row("itemReference").ToString},
            {"itemSingleRef1", row("itemSingleRef1").ToString},
            {"itemBarcode", row("itemBarcode").ToString},
            {"itemDescription", row("itemDescription").ToString},
            {"itemClass", row("itemClass").ToString},
            {"itemUnit", row("itemUnit").ToString},
            {"itemSubUnit", row("itemSubUnit").ToString},
            {"itemVatType", row("itemVatType").ToString},
            {"itemCostCC", CDec(row("itemCostCC"))},
            {"itemCurrency", settings.CurrencyCC},
            {"itemQtyControl", row("itemQtyControl").ToString},
            {"itemMeasType", row("itemMeasType").ToString.Trim.ToUpper},
            {"itemConsFactor", CDec(row("itemConsFactor"))},
            {"supplierRef", reference},
            {"codeExpiry", ""},
            {"codeBatch", ""},
            {"isPending", If(isPending, 1, 0)},
            {"blockMessage", ROItemBlock(row, receivingDate)}
        }
    End Function

    'The items of the pending stock file the user can receive (btnPending of FrmDO), the newest first
    Public Function GetROPendingItems(userCode As String) As String Implements IDataService.GetROPendingItems

        Try

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim settings As ROSettings = ROGetSettings(cnsql, Nothing)
                Dim dtPending As DataTable = RORead(cnsql, Nothing, $" SELECT {ROItemColumns} , I.itemProductGroupDesc FROM V_CPP_ROPendingItems I
                                                                      WHERE 1 = 1 {ROItemAccess(userCode, "I")}
                                                                      ORDER BY I.itemRecID DESC ",
                                                    ROUserParams(userCode))
                Dim result As New JArray
                For Each row As DataRow In dtPending.Rows
                    Dim item As JObject = ROItemJson(row, "", Date.Today, settings, True)
                    item("itemProductGroupDesc") = row("itemProductGroupDesc").ToString
                    result.Add(item)
                Next
                If result.Count = 0 Then Return ""
                Return result.ToString(Newtonsoft.Json.Formatting.None)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The lots of an item for the line editor : lots = the stock of the location (lotAvailable = available - reserved - transfer out,
    'the qty a return can take), knownLots = the lots of the item with their production date (location 00, filled for a lot already known)
    'Returns {lots: [{lotExpiry, lotBatch, lotAvailable}], knownLots: [{lotExpiry, lotBatch, lotProduction}]}
    Public Function GetROItemLots(itemRecID As String, locationCode As String) As String Implements IDataService.GetROItemLots

        Try

            Dim item As Decimal
            If Not Decimal.TryParse(If(itemRecID, "").Trim, item) Then Return "Error Invalid Item"

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim dtLots As DataTable = RORead(cnsql, Nothing, " SELECT  ISNULL(stockExpiry, '') [lotExpiry] , ISNULL(stockBatch, '') [lotBatch] ,
                                                                           ISNULL(stockAvailable, 0) - ISNULL(stockReserved, 0) - ISNULL(stockTransferOut, 0) [lotAvailable]
                                                                   FROM    V_CPP_StockQty
                                                                   WHERE   itemRecID = @ItemRecID AND locationCode = @LocationCode ",
                                                  ROParam("@ItemRecID", item), ROParam("@LocationCode", If(locationCode, "").Trim))

                Dim dtKnown As DataTable = RORead(cnsql, Nothing, " SELECT  ISNULL(stockExpiry, '') [lotExpiry] , ISNULL(stockBatch, '') [lotBatch] ,
                                                                            ISNULL(stockProductionDate, '') [lotProduction]
                                                                    FROM    V_CPP_StockQty
                                                                    WHERE   itemRecID = @ItemRecID AND locationCode = '00'
                                                                            AND ISNULL(stockExpiry, '') <> '' AND ISNULL(stockBatch, '') <> '' ",
                                                   ROParam("@ItemRecID", item))

                Return New JObject From {{"lots", ROJArray(dtLots)}, {"knownLots", ROJArray(dtKnown)}}.ToString(Newtonsoft.Json.Formatting.None)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'A barcode typed for a line (LstBarcode_BeforeColUpdate of FrmDO) : "Error ..." when it is the barcode of another item,
    '"Existing" when the item already has it, "New" when it will be added to the item
    Public Function CheckROBarcode(itemRecID As String, barcode As String) As String Implements IDataService.CheckROBarcode

        Try

            Dim item As Decimal
            If Not Decimal.TryParse(If(itemRecID, "").Trim, item) Then Return "Error Invalid Item"
            Dim code As String = If(barcode, "").Replace(vbCr, "").Replace(vbLf, "").Trim
            If code = "" Then Return "Error Empty Barcode"
            If code.Length > 20 Then Return "Error Barcode Should Be Less Than 20 Characters"

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim dtOwners As DataTable = RORead(cnsql, Nothing, " SELECT DISTINCT itemRecID FROM V_CPP_ItemBarcodes WHERE barcode = @Barcode ",
                                                    ROParam("@Barcode", code))

                If dtOwners.Select("itemRecID <> " & item.ToString(Globalization.CultureInfo.InvariantCulture)).Length > 0 Then
                    Return "Error This Barcode Already Exist For Another Item."
                End If

                Return If(dtOwners.Rows.Count > 0, "Existing", "New")

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'A new barcode from the counter of the system parameters with its check digit (BtnGenerateBarcodeTrading and chkBarc of FrmDO)
    Public Function GenerateROBarcode() As String Implements IDataService.GenerateROBarcode

        Try

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Using trnsql As SqlTransaction = cnsql.BeginTransaction

                    Dim lastBarcode As String = If(ROScalar(cnsql, trnsql, " SELECT TOP 1 SP_LastBarcode FROM dbo.stk04_SysParam WITH ( UPDLOCK, HOLDLOCK ) "), "").ToString.Trim
                    If lastBarcode = "" Then
                        trnsql.Rollback()
                        Return "Error No Counter For Barcode Numbers Defined"
                    End If

                    Dim lastValue As Decimal
                    If Not Decimal.TryParse(lastBarcode, lastValue) Then
                        trnsql.Rollback()
                        Return "Error Invalid Last Barcode Number In System Parameters"
                    End If

                    'The leading zeros are kept
                    Dim nextBarcode As String = (lastValue + 1).ToString("0")
                    If lastBarcode.StartsWith("0") AndAlso nextBarcode.Length < lastBarcode.Length Then nextBarcode = nextBarcode.PadLeft(lastBarcode.Length, "0"c)
                    If nextBarcode.Length <> 11 AndAlso nextBarcode.Length <> 12 Then
                        trnsql.Rollback()
                        Return "Error Invalid Last Barcode Number In System Parameters"
                    End If

                    'Check digit : 11 digits (UPC-A) odd positions x 3, 12 digits (EAN-13) even positions x 3
                    Dim total As Integer = 0
                    For position As Integer = 1 To nextBarcode.Length
                        Dim digit As Integer = CInt(nextBarcode.Substring(position - 1, 1))
                        Dim oddPosition As Boolean = position Mod 2 = 1
                        total += If(nextBarcode.Length = 11, If(oddPosition, digit * 3, digit), If(oddPosition, digit, digit * 3))
                    Next
                    Dim checkDigit As Integer = (10 - total Mod 10) Mod 10

                    ROExecute(cnsql, trnsql, " UPDATE dbo.stk04_SysParam SET SP_LastBarcode = @LastBarcode ", ROParam("@LastBarcode", nextBarcode))
                    trnsql.Commit()

                    Return nextBarcode & checkDigit.ToString

                End Using

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'txtDelOrder_Validating of FrmDO : "Exists" when a receiving order already has this ref (a warning only)
    Public Function CheckRORef(roRef As String) As String Implements IDataService.CheckRORef

        Try

            Dim reference As String = If(roRef, "").Trim
            If reference = "" Then Return ""
            Return If(ROJson(" SELECT TOP 1 roRecID FROM V_CPP_ROHeaders WHERE UPPER(roRef) = UPPER(@RORef) ", ROParam("@RORef", reference)) = "", "", "Exists")

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The purchase orders of the supplier that still have a qty to receive (btnPurchOrder of FrmDO)
    'poAfterRO = 1 : the PO date is after the RO date, it cannot be imported. poLocked = 1 : opened in the ERP
    Public Function GetROPurchaseOrders(userCode As String, supplierRecID As String, roDate As String) As String Implements IDataService.GetROPurchaseOrders

        Try

            Dim supplier As Decimal
            If Not Decimal.TryParse(If(supplierRecID, "").Trim, supplier) Then Return "Error Choose The Supplier First"
            Dim receivingDate As Date
            If Not ROParseDate(roDate, receivingDate) Then receivingDate = Date.Today

            Return ROJson(" SELECT  PO.poRecID , CONVERT(VARCHAR(10), PO.poDate, 103) [poDate] , PO.poManualNo ,
                                    ISNULL(CONVERT(VARCHAR(10), PO.poDeliveryDate, 103), '') [poDeliveryDate] , PO.poTotal , PO.poCurrency , PO.poRemark ,
                                    CASE WHEN CONVERT(DATE, PO.poDate) > @RODate THEN 1 ELSE 0 END [poAfterRO] ,
                                    CASE WHEN EXISTS ( SELECT TOP 1 1 FROM V_CPP_RecordLocks L WHERE L.lockTable = 'STK04_mordH' AND L.lockRecord = CONVERT(VARCHAR(18), PO.poRecID) )
                                         THEN 1 ELSE 0 END [poLocked]
                            FROM    V_CPP_ROPurchaseOrders PO
                            WHERE   PO.supplierRecID = @SupplierRecID
                                    AND ( @CompCode = '' OR PO.companyCode = @CompCode )
                                    AND EXISTS ( SELECT TOP 1 1 FROM V_CPP_ROPurchaseOrderLines POL WHERE POL.poRecID = PO.poRecID AND POL.remainingQty > 0 )
                            ORDER BY PO.poDate DESC , PO.poRecID DESC ",
                          ROParam("@SupplierRecID", supplier), ROParam("@RODate", receivingDate),
                          ROParam("@CompCode", If(ConfigurationManager.AppSettings("CompCode"), "").Trim))

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'The lines of a purchase order with the qty still to receive (FillGridDetails of FrmDO), only the stock items the user can receive
    'Returns {lines: [{poLineNo, supplierRef, lineStatus, remainingQty, unitCost, item...}], skipped: lines left out (pending items, no privilege)}
    Public Function GetROPurchaseOrderLines(userCode As String, supplierRecID As String, poRecID As String, roDate As String) As String Implements IDataService.GetROPurchaseOrderLines

        Try

            Dim supplier, purchaseOrder As Decimal
            If Not Decimal.TryParse(If(supplierRecID, "").Trim, supplier) Then Return "Error Choose The Supplier First"
            If Not Decimal.TryParse(If(poRecID, "").Trim, purchaseOrder) Then Return "Error Invalid PO #"
            Dim receivingDate As Date
            If Not ROParseDate(roDate, receivingDate) Then receivingDate = Date.Today

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim check As String = ROCheckPurchaseOrder(cnsql, Nothing, supplier, purchaseOrder, receivingDate)
                If check <> "" Then Return "Error " & check

                'The stock items, and the items of the pending stock file (created when the RO is saved)
                Dim dtLines As DataTable = RORead(cnsql, Nothing, $" SELECT  POL.poLineNo , POL.supplierRef , POL.lineStatus , POL.remainingQty , POL.unitCost , 0 [isPending] , {ROItemColumns}
                                                                     FROM    V_CPP_ROPurchaseOrderLines POL
                                                                             INNER JOIN V_CPP_ROItems I ON I.itemRecID = POL.itemRecID
                                                                     WHERE   POL.poRecID = @PORecID AND POL.remainingQty > 0 {ROItemAccess(userCode, "I")}
                                                                     UNION ALL
                                                                     SELECT  POL.poLineNo , POL.supplierRef , POL.lineStatus , POL.remainingQty , POL.unitCost , 1 [isPending] , {ROItemColumns}
                                                                     FROM    V_CPP_ROPurchaseOrderLines POL
                                                                             INNER JOIN V_CPP_ROPendingItems I ON I.itemRecID = POL.pendingRecID
                                                                     WHERE   POL.poRecID = @PORecID AND POL.itemRecID IS NULL AND POL.remainingQty > 0 {ROItemAccess(userCode, "I")}
                                                                     ORDER BY POL.poLineNo ",
                                                  ROUserParams(userCode, ROParam("@PORecID", purchaseOrder)))

                Dim allLines As Integer = CInt(ROScalar(cnsql, Nothing, " SELECT COUNT(*) FROM V_CPP_ROPurchaseOrderLines WHERE poRecID = @PORecID AND remainingQty > 0 ",
                                                        ROParam("@PORecID", purchaseOrder)))

                Dim settings As ROSettings = ROGetSettings(cnsql, Nothing)
                Dim lines As New JArray
                For Each row As DataRow In dtLines.Rows
                    Dim line As JObject = ROItemJson(row, row("supplierRef").ToString, receivingDate, settings, CInt(row("isPending")) = 1)
                    line("poLineNo") = row("poLineNo").ToString
                    line("lineStatus") = row("lineStatus").ToString.Trim.ToUpper
                    line("remainingQty") = CDec(row("remainingQty"))
                    line("unitCost") = CDec(row("unitCost"))
                    lines.Add(line)
                Next

                Return New JObject From {{"lines", lines}, {"skipped", allLines - dtLines.Rows.Count}}.ToString(Newtonsoft.Json.Formatting.None)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    '"" when the PO can be imported by an RO of this supplier on this date, else the reason (txtOrderNo_Validating of FrmDO)
    Private Function ROCheckPurchaseOrder(cnsql As SqlConnection, trnsql As SqlTransaction, supplier As Decimal, purchaseOrder As Decimal, receivingDate As Date) As String

        Dim dtPO As DataTable = RORead(cnsql, trnsql, " SELECT poDate FROM V_CPP_ROPurchaseOrders
                                                        WHERE poRecID = @PORecID AND supplierRecID = @SupplierRecID AND ( @CompCode = '' OR companyCode = @CompCode ) ",
                                       ROParam("@PORecID", purchaseOrder), ROParam("@SupplierRecID", supplier),
                                       ROParam("@CompCode", If(ConfigurationManager.AppSettings("CompCode"), "").Trim))

        If dtPO.Rows.Count = 0 Then Return "PO # " & purchaseOrder.ToString & " Does Not Exist For This Supplier"
        If CDate(dtPO.Rows(0)("poDate")).Date > receivingDate Then Return "Cannot Import PO With Date Greater Then RO Date"

        Dim locked As String = ROLockedBy(cnsql, trnsql, "STK04_mordH", purchaseOrder)
        If locked <> "" Then Return "PO # " & purchaseOrder.ToString & " Is Opened " & locked

        Return ""

    End Function

    '"" when the record is not opened in the ERP, else "On Machine X By User Y"
    Private Function ROLockedBy(cnsql As SqlConnection, trnsql As SqlTransaction, table As String, record As Decimal) As String
        Dim dtLock As DataTable = RORead(cnsql, trnsql, " SELECT TOP 1 lockMachine , lockUser FROM V_CPP_RecordLocks WHERE lockTable = @LockTable AND lockRecord = @LockRecord ",
                                         ROParam("@LockTable", table), ROParam("@LockRecord", record.ToString(Globalization.CultureInfo.InvariantCulture)))
        If dtLock.Rows.Count = 0 Then Return ""
        Return "On Machine " & dtLock.Rows(0)("lockMachine").ToString.Trim & " By User " & dtLock.Rows(0)("lockUser").ToString.Trim
    End Function

    'The receiving orders of the trading items (btnSearch of FrmDO), the last 200, searchString : RO #, ref, supplier code or name
    'forChange = "Y" : only the ones that can be modified or cancelled (normal, not linked to a PI, not cancelled already)
    Public Function GetReceivingOrders(userCode As String, forChange As String, searchString As String) As String Implements IDataService.GetReceivingOrders

        Try

            Dim search As String = If(searchString, "").Trim
            Dim sql As String = $" SELECT TOP 200 H.roRecID , LTRIM(RTRIM(H.roRef)) [roRef] , CONVERT(VARCHAR(10), H.roDate, 103) [roDate] ,
                                          ISNULL(CONVERT(VARCHAR(10), H.roActualDate, 103), '') [roActualDate] , H.roStatus , H.locationCode ,
                                          ISNULL(L.locationDescription, '') [locationDescription] , S.supplierRecID , S.supplierCode , S.supplierName ,
                                          ISNULL(CONVERT(VARCHAR(18), H.poRecID), '') [poRecID] , ISNULL(CONVERT(VARCHAR(18), H.piRecID), '') [piRecID] ,
                                          ( SELECT COUNT(*) FROM V_CPP_ROLines D WHERE D.roRecID = H.roRecID ) [lineCount]
                                   FROM   V_CPP_ROHeaders H
                                          INNER JOIN V_CPP_ROSuppliers S ON S.supplierRecID = H.supplierRecID
                                          LEFT JOIN V_CPP_ROLocations L ON L.locationCode = H.locationCode
                                   WHERE  H.roType = 'T' AND ISNULL(H.roCurrency, '') = '' {ROSupplierAccess(userCode, "S")} "

            If If(forChange, "").Trim.ToUpper = "Y" Then sql &= ROChangeCondition("H")

            If search <> "" Then
                sql &= " AND ( CONVERT(VARCHAR(18), H.roRecID) = @Search OR H.roRef LIKE @SearchLike OR S.supplierCode LIKE @SearchLike OR S.supplierName LIKE @SearchLike ) "
            End If

            sql &= " ORDER BY H.roRecID DESC "

            Return ROJson(sql, ROUserParams(userCode, ROParam("@Search", search),
                                            ROParam("@SearchLike", "%" & search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") & "%")))

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'An RO that can be modified or cancelled : normal, not linked to a purchase invoice, no reverse RO for it (btnSearch of FrmDO)
    Private Function ROChangeCondition(headerAlias As String) As String
        Return $" AND {headerAlias}.roStatus = 'N' AND {headerAlias}.piRecID IS NULL
                  AND NOT EXISTS ( SELECT TOP 1 1 FROM V_CPP_ROHeaders REV
                                   WHERE REV.roStatus = 'R' AND REV.supplierRecID = {headerAlias}.supplierRecID
                                         AND REV.roRef = {headerAlias}.roRef AND REV.roDate = {headerAlias}.roDate ) "
    End Function

    'One receiving order with its lines, their lots (expiry items) and barcodes (RefreshControls of FrmDO)
    'forChange = "Y" : "Error ..." when it cannot be modified or cancelled (linked to a PI, cancelled, opened in the ERP)
    Public Function GetReceivingOrder(userCode As String, roRecID As String, forChange As String) As String Implements IDataService.GetReceivingOrder

        Try

            Dim receivingOrder As Decimal
            If Not Decimal.TryParse(If(roRecID, "").Trim, receivingOrder) Then Return "Error Invalid RO #"
            Dim change As Boolean = If(forChange, "").Trim.ToUpper = "Y"

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Dim dtHeader As DataTable = RORead(cnsql, Nothing, $" SELECT  H.roRecID , LTRIM(RTRIM(H.roRef)) [roRef] , CONVERT(VARCHAR(10), H.roDate, 120) [roDate] ,
                                                                              ISNULL(CONVERT(VARCHAR(10), H.roActualDate, 120), '') [roActualDate] , H.roStatus , H.locationCode ,
                                                                              ISNULL(L.locationDescription, '') [locationDescription] , S.supplierRecID , S.supplierCode , S.supplierName ,
                                                                              ISNULL(CONVERT(VARCHAR(18), H.poRecID), '') [poRecID] , ISNULL(CONVERT(VARCHAR(18), H.piRecID), '') [piRecID] ,
                                                                              CASE WHEN 1 = 1 {ROChangeCondition("H")} THEN 1 ELSE 0 END [canChange]
                                                                      FROM    V_CPP_ROHeaders H
                                                                              INNER JOIN V_CPP_ROSuppliers S ON S.supplierRecID = H.supplierRecID
                                                                              LEFT JOIN V_CPP_ROLocations L ON L.locationCode = H.locationCode
                                                                      WHERE   H.roRecID = @RORecID AND H.roType = 'T' AND ISNULL(H.roCurrency, '') = ''
                                                                              {ROSupplierAccess(userCode, "S")} ",
                                                   ROUserParams(userCode, ROParam("@RORecID", receivingOrder)))

                If dtHeader.Rows.Count = 0 Then Return "Error Receiving Order " & receivingOrder.ToString & " Not Found"

                If change Then
                    If dtHeader.Rows(0)("piRecID").ToString <> "" Then Return "Error This Receiving Order Is Linked To The Purchase Invoice " & dtHeader.Rows(0)("piRecID").ToString
                    If CInt(dtHeader.Rows(0)("canChange")) = 0 Then Return "Error This Receiving Order Is Already Cancelled"
                    Dim locked As String = ROLockedBy(cnsql, Nothing, "Stk04_PurDelOrdH", receivingOrder)
                    If locked <> "" Then Return "Error This Receiving Order Is Opened " & locked
                End If

                Dim dtLines As DataTable = RORead(cnsql, Nothing, " SELECT  D.lineRecID , LTRIM(RTRIM(D.supplierRef)) [supplierRef] , D.lineStatus , D.lineCounter , D.itemRecID ,
                                                                           D.lineQty , ISNULL(D.lineCostCC, 0) [lineCostCC] , ISNULL(D.lineVatType, '') [lineVatType] ,
                                                                           ISNULL(I.itemReference, '') [itemReference] , ISNULL(I.itemDescription, '') [itemDescription] ,
                                                                           ISNULL(I.itemClass, CASE WHEN EXISTS ( SELECT TOP 1 1 FROM V_CPP_ROLineLots Q WHERE Q.lineRecID = D.lineRecID ) THEN 'E' ELSE 'T' END) [itemClass] ,
                                                                           ISNULL(I.itemUnit, '') [itemUnit] , ISNULL(I.itemBarcode, '') [itemBarcode] ,
                                                                           ISNULL(I.itemMeasType, 'P') [itemMeasType] , ISNULL(I.itemConsFactor, 1) [itemConsFactor] , ISNULL(I.itemSubUnit, '') [itemSubUnit]
                                                                   FROM    V_CPP_ROLines D
                                                                           LEFT JOIN V_CPP_ROItems I ON I.itemRecID = D.itemRecID
                                                                   WHERE   D.roRecID = @RORecID
                                                                   ORDER BY D.lineRecID ",
                                                  ROParam("@RORecID", receivingOrder))

                Dim dtLots As DataTable = RORead(cnsql, Nothing, " SELECT  Q.lineRecID , ISNULL(Q.lotExpiry, '') [lotExpiry] , ISNULL(Q.lotBatch, '') [lotBatch] , ISNULL(Q.lotQty, 0) [lotQty]
                                                                  FROM    V_CPP_ROLineLots Q
                                                                          INNER JOIN V_CPP_ROLines D ON D.lineRecID = Q.lineRecID
                                                                  WHERE   D.roRecID = @RORecID
                                                                  ORDER BY Q.lotRecID ",
                                                 ROParam("@RORecID", receivingOrder))

                Dim dtBarcodes As DataTable = RORead(cnsql, Nothing, " SELECT  B.lineRecID , LTRIM(RTRIM(B.barcode)) [barcode] , ISNULL(B.lotExpiry, '') [lotExpiry] , ISNULL(B.lotBatch, '') [lotBatch]
                                                                      FROM    V_CPP_ROLineBarcodes B
                                                                              INNER JOIN V_CPP_ROLines D ON D.lineRecID = B.lineRecID
                                                                      WHERE   D.roRecID = @RORecID AND ISNULL(B.barcode, '') <> ''
                                                                      ORDER BY B.barcodeRecID ",
                                                     ROParam("@RORecID", receivingOrder))

                Dim settings As ROSettings = ROGetSettings(cnsql, Nothing)
                Dim lines As New JArray
                For Each row As DataRow In dtLines.Rows
                    Dim lineID As String = row("lineRecID").ToString
                    Dim lots As New JArray
                    For Each lot As DataRow In dtLots.Select("lineRecID = " & lineID)
                        Dim lotBarcodes As New JArray
                        For Each barcode As DataRow In dtBarcodes.Select("lineRecID = " & lineID)
                            If barcode("lotExpiry").ToString = lot("lotExpiry").ToString AndAlso barcode("lotBatch").ToString = lot("lotBatch").ToString Then lotBarcodes.Add(barcode("barcode").ToString)
                        Next
                        lots.Add(New JObject From {{"expiry", lot("lotExpiry").ToString}, {"batch", lot("lotBatch").ToString}, {"qty", CDec(lot("lotQty"))}, {"barcodes", lotBarcodes}})
                    Next
                    Dim barcodes As New JArray
                    For Each barcode As DataRow In dtBarcodes.Select("lineRecID = " & lineID)
                        If barcode("lotExpiry").ToString = "" AndAlso barcode("lotBatch").ToString = "" Then barcodes.Add(barcode("barcode").ToString)
                    Next
                    lines.Add(New JObject From {
                        {"lineRecID", lineID},
                        {"itemRecID", row("itemRecID").ToString},
                        {"itemReference", row("itemReference").ToString},
                        {"itemDescription", row("itemDescription").ToString},
                        {"itemClass", row("itemClass").ToString.Trim.ToUpper},
                        {"itemUnit", row("itemUnit").ToString},
                        {"itemBarcode", row("itemBarcode").ToString},
                        {"itemMeasType", row("itemMeasType").ToString.Trim.ToUpper},
                        {"itemConsFactor", CDec(row("itemConsFactor"))},
                        {"itemSubUnit", row("itemSubUnit").ToString},
                        {"supplierRef", row("supplierRef").ToString},
                        {"lineStatus", row("lineStatus").ToString.Trim.ToUpper},
                        {"qty", CDec(row("lineQty"))},
                        {"costCC", CDec(row("lineCostCC"))},
                        {"itemVatType", row("lineVatType").ToString.Trim},
                        {"itemCurrency", settings.CurrencyCC},
                        {"lots", lots},
                        {"barcodes", barcodes}
                    })
                Next

                Dim header As JObject = CType(ROJArray(dtHeader)(0), JObject)
                header.Add("lines", lines)
                Return header.ToString(Newtonsoft.Json.Formatting.None)

            End Using

        Catch ex As Exception
            Return "Error " & ex.Message
        End Try

    End Function

    'Prevent Creating Items Without Barcode (button btnPrevCreaItemWOBar of the Receiving Order step MnuDO1, NegationPriv of the ERP) :
    'the negation of the user when he has one, else the one of his groups. SU never has it
    Private Function ROPreventItemsWithoutBarcode(cnsql As SqlConnection, trnsql As SqlTransaction, user As String, settings As ROSettings) As Boolean

        If user = "SU" Then Return False

        Dim dtUser As DataTable = RORead(cnsql, trnsql, " SELECT Nbu_hasNeg FROM dbo.Com_NegationButton
                                                          WHERE  Nbu_User = @UserCode AND Nbu_Comp = @Comp AND Nbu_Appl = @App AND Nbu_Module = @App
                                                                 AND Nbu_Step = 'MnuDO1' AND Nbu_Button = 'btnPrevCreaItemWOBar' AND Nbu_Exception = 1 ",
                                         ROParam("@UserCode", user), ROParam("@Comp", settings.CompCode), ROParam("@App", settings.AppCode))
        If dtUser.Rows.Count > 0 Then Return dtUser.Rows(0)("Nbu_hasNeg").ToString.Trim = "1"

        Return ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM dbo.Com_GroupNegButton INNER JOIN dbo.Com_UserInfoGroup ON Nbg_Group = UIL_GrpCode
                                         WHERE  UIL_UserCode = @UserCode AND Nbg_Comp = @Comp AND Nbg_Appl = @App AND Nbg_Module = @App
                                                AND Nbg_Step = 'MnuDO1' AND Nbg_Button = 'btnPrevCreaItemWOBar' ",
                        ROParam("@UserCode", user), ROParam("@Comp", settings.CompCode), ROParam("@App", settings.AppCode)).ToString <> "0"

    End Function

    'Creates in the stock file the item of the pending stock file of a line (TestStkFil and InsertStkFil of FrmDO) :
    'when an item with the same reference exists already (created meanwhile) it is used, else the item is created with the values of the
    'pending item (supplier = the supplier of the RO, costs 0), its technical sheet, the lines of the POs, invoices, templates and
    'requisitions get the new item and the pending item is deleted. Returns the record id of the item, or the error message (String)
    Private Function ROCreatePendingItem(cnsql As SqlConnection, trnsql As SqlTransaction, user As String, line As ROLine,
                                         supplierCode As String, settings As ROSettings) As Object

        Dim reference As String = line.Item("itemReference").ToString.Trim

        Dim dtExisting As DataTable = RORead(cnsql, trnsql, " SELECT STK_RECID [itemRecID] , STK_ItmClass [itemClass] FROM dbo.STK04_STKFIL WHERE STK_REF = @Reference ",
                                             ROParam("@Reference", reference))
        If dtExisting.Rows.Count > 0 Then
            If dtExisting.Rows(0)("itemClass").ToString.Trim.ToUpper <> line.Item("itemClass").ToString.Trim.ToUpper Then
                Return "Error The Item '" & line.Item("itemDescription").ToString & "' Was Created With Different Item Classification."
            End If
            Return CDec(dtExisting.Rows(0)("itemRecID"))
        End If

        Dim dtPending As DataTable = RORead(cnsql, trnsql, " SELECT * FROM dbo.STK04_StkFilPending WITH ( UPDLOCK ) WHERE SFP_Recid = @PendingRecID ",
                                            ROParam("@PendingRecID", line.PendingRecID))
        If dtPending.Rows.Count = 0 Then Return "Error The Pending Item " & reference & " Is Not There Any More, Remove Its Line"
        Dim locked As String = ROLockedBy(cnsql, trnsql, "STK04_StkFilPending", line.PendingRecID)
        If locked <> "" Then Return "Error The Pending Item " & reference & " Is Opened " & locked
        Dim pending As DataRow = dtPending.Rows(0)

        'The value of a column of the pending item, Nothing (NULL) when it is empty
        Dim value = Function(column As String) As Object
                        Dim cell As Object = pending(column)
                        If IsDBNull(cell) OrElse cell.ToString.Trim = "" Then Return Nothing
                        Return cell
                    End Function

        Dim newItem As Decimal = CDec(ROScalar(cnsql, trnsql, " SELECT ISNULL(MAX(STK_RECID), 0) + 1 FROM dbo.STK04_STKFIL WITH ( UPDLOCK, HOLDLOCK ) "))
        Dim newUsed As String = pending("SFP_NEWUSED").ToString.Trim

        ROExecute(cnsql, trnsql, " INSERT INTO dbo.STK04_STKFIL ( STK_RECID , STK_COMP , STK_SUPPLIER , STK_CATEGORY , STK_SERIE , STK_USAGE , STK_NEWUSED ,
                                        STK_LOCSRC , STK_DATEINF , STK_ACTIVE , STK_DESC , STK_DESCABV , STK_ENTRYDATE , STK_IMPLOC ,
                                        STK_CONSIG , STK_ITMIDENT , STK_SERVICE , STK_PROMO , STK_ITMTYPE , STK_BOOKID , STK_UNIT ,
                                        STK_QTYCTRL , STK_TRANSPOS , STK_SPECPROMO , STK_SPROMODATE , STK_EPROMODATE , STK_CONSUM ,
                                        STK_LIFCNT , STK_UNITPACK , STK_SUBUNIT , STK_BBFDATE , STK_UNITCOSTLC , STK_UNITCOSTCC , STK_UNITCOSTFC ,
                                        STK_AVGCOSTLC , STK_AVGCOSTCC , STK_AVGCOSTFC , STK_FCCODE , STK_VATTYPE , STK_VATPER , STK_VATDATE ,
                                        STK_MINQTY , STK_MAXQTY , STK_USERENTRY , STK_DATEDEF , STK_DSTAMP , STK_USTAMP , STK_ItmClass , STK_OpSource ,
                                        STK_PotUnitCostLC , STK_PotUnitCostCC , STK_PotUnitCostFC , STK_SingleRef1 , STK_SingleRef2 , STK_Season ,
                                        STK_Brand , STK_Grid , STK_Gender , STK_Style , STK_FollowFidRule , STK_FranchiseSale , STK_OutsideTrade ,
                                        STK_CurrentArchive , STK_HaltActTill , STK_HaltPoPi , STK_HaltSales , STK_HaltTransfers , STK_ReturnSale , STK_CrDStamp ,
                                        STK_ThirdParty , STK_ThirdPartyDisc , STK_FourthParty , STK_SendToPOS , STK_VATSTAT , STK_OPERATOR , STK_cashvan ,
                                        STK_RecalculateLC )
                                   VALUES ( @ItemRecID , @Company , @Supplier , @Category , @Serie , @Usage , @NewUsed ,
                                        @SourceLocation , @SourceDate , 'Y' , @Description , NULL , CONVERT(DATE, GETDATE()) , @ImportLocal ,
                                        'N' , @Identification , @ServiceRegular , 'N' , @ProductGroup , NULL , @Unit ,
                                        @QtyControl , 'N' , 'N' , NULL , NULL , 'Y' ,
                                        0 , @UnitPack , @SubUnit , CONVERT(DATE, GETDATE()) , 0 , 0 , 0 ,
                                        0 , 0 , 0 , @Currency , @VatType , @VatPerc , @VatDate ,
                                        NULL , NULL , 'U' , CONVERT(DATE, GETDATE()) , GETDATE() , @User , @Class , 'DO' ,
                                        0 , 0 , 0 , @SingleRef1 , @SingleRef2 , @Season ,
                                        @Brand , @Grid , @Gender , @Style , 'N' , 'N' , 'N' ,
                                        'C' , NULL , 'N' , 'N' , 'N' , 'Y' , GETDATE() ,
                                        'n' , NULL , 'n' , 'y' , @VatStat , @Operator , 'N' ,
                                        NULL ) ",
                  ROParam("@ItemRecID", newItem), ROParam("@Company", settings.CompCode), ROParam("@Supplier", supplierCode),
                  ROParam("@Category", value("SFP_CATEGORY")), ROParam("@Serie", value("SFP_Serie")), ROParam("@Usage", value("SFP_Usage")),
                  ROParam("@NewUsed", newUsed), ROParam("@SourceLocation", value("SFP_LOCSRC")), ROParam("@SourceDate", value("SFP_InfDate")),
                  ROParam("@Description", value("SFP_Desc")), ROParam("@ImportLocal", value("SFP_ImpLoc")),
                  ROParam("@Identification", value("SFP_ItmIdent")), ROParam("@ServiceRegular", value("SFP_SerReg")),
                  ROParam("@ProductGroup", value("SFP_ItmType")), ROParam("@Unit", value("SFP_UNIT")),
                  ROParam("@QtyControl", If(newUsed.ToUpper = "C", "Y", "N")), ROParam("@UnitPack", value("SFP_UNITPACK")),
                  ROParam("@SubUnit", value("SFP_SUBUNIT")), ROParam("@Currency", value("SFP_FCCode")), ROParam("@VatType", value("SFP_VATType")),
                  ROParam("@VatPerc", value("SFP_VATPER")), ROParam("@VatDate", value("SFP_StartTaxDate")), ROParam("@User", user),
                  ROParam("@Class", value("SFP_ItmClass")), ROParam("@SingleRef1", value("SFP_SingleRef1")), ROParam("@SingleRef2", value("SFP_SingleRef2")),
                  ROParam("@Season", value("SFP_Season")), ROParam("@Brand", value("SFP_Brand")), ROParam("@Grid", value("SFP_Grid")),
                  ROParam("@Gender", value("SFP_Gender")), ROParam("@Style", value("SFP_Style")), ROParam("@VatStat", pending("SFP_VATSTAT").ToString),
                  ROParam("@Operator", pending("sfp_operator").ToString))

        If pending("SFP_TechSheet").ToString.Trim <> "" Then
            ROExecute(cnsql, trnsql, " INSERT INTO dbo.STK04_DSTechnical ( DSTech_RecId , DSTech_Meas , DSTech_Desc1 , DSTech_Desc2 , DSTech_BMP , DSTech_DStamp , DStech_UStamp ,
                                            DSTech_Weight , DSTech_ScannedDoc , DSTech_Country , DSTech_Region , DSTech_HSCode , DSTech_Warranty , DSTech_Volume , DSTech_GrossWeight , DSTech_Video )
                                       VALUES ( @ItemRecID , @TechSheet , NULL , NULL , NULL , GETDATE() , @User , NULL , NULL , NULL , NULL , NULL , NULL , NULL , NULL , NULL ) ",
                      ROParam("@ItemRecID", newItem), ROParam("@TechSheet", pending("SFP_TechSheet").ToString.Trim), ROParam("@User", user))
        End If

        'The lines of the POs, invoices, supplier templates, requisitions and approvals : {table, item column, pending column}
        For Each link As String() In {New String() {"Stk04_MOrdDet", "MOD_RMRecid", "MOD_PendRecid"},
                                      New String() {"STK04_InvDET", "INVD_STKRECID", "INVD_PendRecId"},
                                      New String() {"STK04_PoTemplateDet", "POTD_StkRecid", "POTD_PendRecid"},
                                      New String() {"tbl_RequisitionDetails", "ReqD_StkId", "ReqD_PendRecId"},
                                      New String() {"tbl_ReqItmApproval", "App_Stkid", "App_PenStkId"}}
            ROExecute(cnsql, trnsql, $" UPDATE dbo.{link(0)} SET {link(1)} = @ItemRecID , {link(2)} = NULL WHERE {link(2)} = @PendingRecID ",
                      ROParam("@ItemRecID", newItem), ROParam("@PendingRecID", line.PendingRecID))
        Next

        ROExecute(cnsql, trnsql, " DELETE FROM dbo.STK04_StkFilPending WHERE SFP_Recid = @PendingRecID ", ROParam("@PendingRecID", line.PendingRecID))

        Return newItem

    End Function

    'Saves a receiving order (btnSave_Click of FrmDO) in one transaction, returns "Success|<RO #>" or "Error <message>"
    'saveMode : ADD = new RO, MODIFY = new lines added to the RO roRecID (and its actual date), CANCEL = a reverse RO of roRecID
    'receivingOrder : {roRecID, supplierRecID, roRef, roDate, roActualDate, locationCode, poRecID,
    '                  lines: [{itemRecID, supplierRef, lineStatus (N F R), qty, barcodes: [..],
    '                           lots: [{expiry, batch, production, qty, barcodes: [..]}] (expiry items, the qty of the line is their total)}]}
    'MODIFY sends only the new lines, CANCEL sends no line (the lines of the RO are reversed)
    Public Function SaveReceivingOrder(userCode As String, saveMode As String, receivingOrder As String) As String Implements IDataService.SaveReceivingOrder

        Try

            Dim mode As String = If(saveMode, "").Trim.ToUpper
            If mode <> "ADD" AndAlso mode <> "MODIFY" AndAlso mode <> "CANCEL" Then Return "Error Invalid Save Mode"
            Dim user As String = If(userCode, "").Trim.ToUpper
            If user = "" Then Return "Error Missing User"

            Dim order As JObject = JObject.Parse(If(receivingOrder, "{}"))

            Using cnsql As New SqlConnection(ROConnectionString())

                cnsql.Open()

                Using trnsql As SqlTransaction = cnsql.BeginTransaction

                    Try

                        Dim result As String = ROSave(cnsql, trnsql, user, mode, order)
                        If result.StartsWith("Error") Then
                            trnsql.Rollback()
                        Else
                            trnsql.Commit()
                        End If
                        Return result

                    Catch
                        Try
                            trnsql.Rollback()
                        Catch exTRN As Exception
                        End Try
                        Throw
                    End Try

                End Using

            End Using

        Catch ex As Exception
            Dim message As String = ex.Message
            If message.IndexOf("IX_stk04_PurDelOrdH", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return "Error Same RO Ref. # And Date Already Exist For This Supplier, Change Them To Save."
            ElseIf message.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return "Error This Record Is Already Added By Another User, Please Save Again."
            ElseIf message.IndexOf("deadlock", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return "Error Saving Failed. Please Try Again."
            End If
            Return "Error " & message
        End Try

    End Function

    Private Function ROSave(cnsql As SqlConnection, trnsql As SqlTransaction, user As String, mode As String, order As JObject) As String

        Dim settings As ROSettings = ROGetSettings(cnsql, trnsql)
        Dim today As Date = Date.Today

        '#Header

        Dim receivingOrder As Decimal = 0
        Dim oldHeader As DataRow = Nothing

        If mode <> "ADD" Then

            If Not Decimal.TryParse(If(order.Value(Of String)("roRecID"), "").Trim, receivingOrder) Then Return "Error Invalid RO #"

            Dim dtOld As DataTable = RORead(cnsql, trnsql, $" SELECT H.* , S.supplierCode FROM V_CPP_ROHeaders H WITH ( UPDLOCK )
                                                              INNER JOIN V_CPP_ROSuppliers S ON S.supplierRecID = H.supplierRecID
                                                              WHERE H.roRecID = @RORecID AND H.roType = 'T' AND ISNULL(H.roCurrency, '') = '' ",
                                            ROParam("@RORecID", receivingOrder))
            If dtOld.Rows.Count = 0 Then Return "Error Receiving Order " & receivingOrder.ToString & " Not Found"
            oldHeader = dtOld.Rows(0)

            If Not IsDBNull(oldHeader("piRecID")) Then Return "Error This Receiving Order Is Linked To The Purchase Invoice " & oldHeader("piRecID").ToString
            If ROScalar(cnsql, trnsql, $" SELECT COUNT(*) FROM V_CPP_ROHeaders H WHERE H.roRecID = @RORecID {ROChangeCondition("H")} ", ROParam("@RORecID", receivingOrder)).ToString = "0" Then
                Return "Error This Receiving Order Is Already Cancelled"
            End If
            Dim locked As String = ROLockedBy(cnsql, trnsql, "Stk04_PurDelOrdH", receivingOrder)
            If locked <> "" Then Return "Error This Receiving Order Is Opened " & locked

        End If

        Dim supplier As Decimal
        Dim roRef As String
        Dim roDate, roActualDate As Date
        Dim locationCode As String
        Dim purchaseOrder As Decimal = 0

        If mode = "ADD" Then
            If Not Decimal.TryParse(If(order.Value(Of String)("supplierRecID"), "").Trim, supplier) Then Return "Error You Should Choose A Supplier"
            roRef = If(order.Value(Of String)("roRef"), "").Trim
            If Not ROParseDate(order.Value(Of String)("roDate"), roDate) Then Return "Error Date Can Not Be Null"
            locationCode = If(order.Value(Of String)("locationCode"), "").Trim
            Dim poText As String = If(order.Value(Of String)("poRecID"), "").Trim
            If poText <> "" AndAlso Not Decimal.TryParse(poText, purchaseOrder) Then Return "Error Invalid PO #"
        Else
            supplier = CDec(oldHeader("supplierRecID"))
            roRef = oldHeader("roRef").ToString.Trim
            roDate = CDate(oldHeader("roDate")).Date
            locationCode = oldHeader("locationCode").ToString.Trim
            If Not IsDBNull(oldHeader("poRecID")) Then purchaseOrder = CDec(oldHeader("poRecID"))
        End If

        If mode = "CANCEL" Then
            roActualDate = If(IsDBNull(oldHeader("roActualDate")), roDate, CDate(oldHeader("roActualDate")).Date)
        ElseIf Not ROParseDate(order.Value(Of String)("roActualDate"), roActualDate) Then
            Return "Error Actual Date Can Not Be Null"
        End If

        'txtDate_Validating of FrmDO : the RO date not after today (new RO), the actual date not before the RO date
        If mode = "ADD" AndAlso roDate > today Then Return "Error Date Should Be Equal or Less Than Today's Date"
        If mode <> "CANCEL" AndAlso roActualDate < roDate Then
            Return "Error Actual Date (" & roActualDate.ToString("dd/MM/yyyy") & ") Should Be Greater or Equal than RO Date (" & roDate.ToString("dd/MM/yyyy") & ")"
        End If

        If roRef = "" Then Return "Error You Should Put A RO Ref. #"
        If roRef.Length > 15 Then Return "Error The RO Ref. # Can Have 15 Characters"

        'A new RO cannot take the ref of another one (a reverse RO keeps the ref of the RO it cancels)
        If mode = "ADD" Then
            Dim usedBy As Object = ROScalar(cnsql, trnsql, " SELECT TOP 1 roRecID FROM V_CPP_ROHeaders WHERE UPPER(roRef) = UPPER(@RORef) ", ROParam("@RORef", roRef))
            If usedBy IsNot Nothing AndAlso usedBy IsNot DBNull.Value Then Return "Error The RO Ref. # " & roRef & " Is Already Used By Receiving Order #" & usedBy.ToString
        End If

        Dim dtSupplier As DataTable = RORead(cnsql, trnsql, $" SELECT S.supplierCode , S.supplierName FROM V_CPP_ROSuppliers S
                                                               WHERE S.supplierRecID = @SupplierRecID {ROSupplierAccess(user, "S")} ",
                                             ROUserParams(user, ROParam("@SupplierRecID", supplier)))
        If dtSupplier.Rows.Count = 0 Then Return "Error You Have No Privilege To Purchase From This Supplier, Or It Is Not Active"

        If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_ROLocations WHERE locationCode = @LocationCode ", ROParam("@LocationCode", locationCode)).ToString = "0" Then
            Return "Error You Should Choose Receiving Location"
        End If

        If mode = "ADD" AndAlso locationCode <> LocCode.Trim Then
            Return "Error You Can Only Receive To Location " & LocCode.Trim
        End If

        If mode = "ADD" AndAlso purchaseOrder <> 0 Then
            Dim check As String = ROCheckPurchaseOrder(cnsql, trnsql, supplier, purchaseOrder, roDate)
            If check <> "" Then Return "Error " & check
        End If

        '#Lines

        Dim lines As New List(Of ROLine)

        If mode = "CANCEL" Then

            If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_ROLines WHERE roRecID = @RORecID ", ROParam("@RORecID", receivingOrder)).ToString = "0" Then
                Return "Error This Receiving Order Has No Lines"
            End If

        Else

            Dim lineArray As JArray = If(TryCast(order("lines"), JArray), New JArray)
            If lineArray.Count = 0 Then Return If(mode = "ADD", "Error You should add items to the Receiving Order", "Error There Is No New Line To Add")

            Dim allBarcodes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim preventWithoutBarcode As Boolean = ROPreventItemsWithoutBarcode(cnsql, trnsql, user, settings)

            For Each lineToken As JObject In lineArray

                Dim line As New ROLine
                If Not Decimal.TryParse(If(lineToken.Value(Of String)("itemRecID"), "").Trim, line.ItemRecID) Then Return "Error Item Code Cannot Be Empty"
                line.IsPending = If(lineToken.Value(Of String)("pending"), "").Trim.ToLower = "true"
                If line.IsPending Then line.PendingRecID = line.ItemRecID

                Dim dtItem As DataTable = RORead(cnsql, trnsql, $" SELECT {ROItemColumns} , I.itemCurrency , I.itemUnitCode , I.itemUnitPack , I.itemOperator , I.itemMeasSign ,
                                                                          I.itemMeasValue , I.itemReturnSale
                                                                  FROM   {If(line.IsPending, "V_CPP_ROPendingItems", "V_CPP_ROItems")} I
                                                                  WHERE  I.itemRecID = @ItemRecID {ROItemAccess(user, "I")} ",
                                                  ROUserParams(user, ROParam("@ItemRecID", line.ItemRecID)))
                If dtItem.Rows.Count = 0 Then
                    Return If(line.IsPending, "Error An Item Of The Pending Stock File Is Not There Any More (Created Or Removed), Remove Its Line",
                                              "Error An Item Of The Lines Cannot Be Received (Inactive, Archived Or No Privilege)")
                End If
                line.Item = dtItem.Rows(0)
                Dim reference As String = line.Item("itemReference").ToString

                Dim block As String = ROItemBlock(line.Item, roDate)
                If block <> "" Then Return "Error " & reference & " : " & block

                line.SupplierRef = If(lineToken.Value(Of String)("supplierRef"), "").Replace(vbCr, "").Replace(vbLf, "").Trim
                If line.SupplierRef = "" Then Return "Error " & reference & " : Supplier Reference Cannot Be Empty"
                If line.SupplierRef.Length > 30 Then Return "Error " & reference & " : The Supplier Reference Can Have 30 Characters"

                line.Status = If(lineToken.Value(Of String)("lineStatus"), "").Trim.ToUpper
                If line.Status <> "N" AndAlso line.Status <> "F" AndAlso line.Status <> "R" Then Return "Error " & reference & " : Invalid Status"
                If line.IsPending AndAlso line.Status = "R" Then Return "Error " & reference & " : Pending Item Status Can Be N or F"

                Dim itemClass As String = line.Item("itemClass").ToString
                Dim lineBarcodes As New List(Of String)

                If itemClass = "E" Then

                    Dim lotKeys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                    For Each lotToken As JObject In If(TryCast(lineToken("lots"), JArray), New JArray)

                        Dim lot As New ROLot With {
                            .Expiry = If(lotToken.Value(Of String)("expiry"), "").Trim,
                            .Batch = If(lotToken.Value(Of String)("batch"), "").Trim,
                            .Production = If(lotToken.Value(Of String)("production"), "").Trim
                        }
                        Decimal.TryParse(If(lotToken.Value(Of String)("qty"), "").Trim, Globalization.NumberStyles.Number, Globalization.CultureInfo.InvariantCulture, lot.Qty)

                        Dim lastDay As Date
                        If Not ROLotDate(lot.Expiry, lastDay) Then Return "Error " & reference & " : Invalid Expiry Date " & lot.Expiry
                        If lastDay < roDate Then Return "Error " & reference & " : Item Expired! (" & lot.Expiry & ")"
                        If lot.Batch = "" Then Return "Error " & reference & " : Lot # Can't Be Null"
                        If lot.Batch.Length > 15 Then Return "Error " & reference & " : The Lot # Can Have 15 Characters"
                        If lot.Production <> "" Then
                            If Not ROLotDate(lot.Production, lastDay) Then Return "Error " & reference & " : Invalid Production Date " & lot.Production
                            If lastDay > roDate Then Return "Error " & reference & " : Production Date Cannot Be Greater Than Receiving Order Date!"
                        End If
                        If lot.Qty <= 0 Then Return "Error " & reference & " : Expiry Quantity Cannot Be 0 (" & lot.Expiry & " / " & lot.Batch & ")"
                        If Not lotKeys.Add(lot.Expiry & "|" & lot.Batch) Then Return "Error " & reference & " : Expiry Date And Lot # Already Exist (" & lot.Expiry & " / " & lot.Batch & ")"

                        For Each barcodeToken In If(TryCast(lotToken("barcodes"), JArray), New JArray)
                            Dim barcode As String = barcodeToken.ToString.Replace(vbCr, "").Replace(vbLf, "").Trim
                            If barcode <> "" Then lot.Barcodes.Add(barcode)
                        Next
                        If lot.Barcodes.Count > lot.Qty Then Return "Error " & reference & " : Barcode Number Can't Be Bigger Than Expiry Quantity (" & lot.Expiry & " / " & lot.Batch & ")"
                        lineBarcodes.AddRange(lot.Barcodes)

                        line.Lots.Add(lot)
                        line.Qty += lot.Qty

                    Next
                    If line.Lots.Count = 0 Then Return "Error " & reference & " : Expiry Quantity Cannot Be 0, Add The Expiry Dates And Lots"

                Else

                    Decimal.TryParse(If(lineToken.Value(Of String)("qty"), "").Trim, Globalization.NumberStyles.Number, Globalization.CultureInfo.InvariantCulture, line.Qty)
                    If line.Qty <= 0 Then Return "Error " & reference & " : Qty Cannot Be Empty Or 0"
                    For Each barcodeToken In If(TryCast(lineToken("barcodes"), JArray), New JArray)
                        Dim barcode As String = barcodeToken.ToString.Replace(vbCr, "").Replace(vbLf, "").Trim
                        If barcode <> "" Then line.Barcodes.Add(barcode)
                    Next
                    If line.Barcodes.Count > line.Qty Then Return "Error " & reference & " : Barcode Number Can't Be Bigger Than Quantity"
                    lineBarcodes.AddRange(line.Barcodes)

                End If

                If line.Qty >= 10000000000D Then Return "Error " & reference & " : Invalid Value"

                'The item created from a pending item gets the barcodes of its line (btnSave_Click of FrmDO)
                If line.IsPending AndAlso preventWithoutBarcode AndAlso lineBarcodes.Count = 0 Then
                    Return "Error You Should Define At Least One Barcode For Each Pending Item Created (" & reference & ")"
                End If

                For Each barcode As String In lineBarcodes
                    If barcode.Length > 20 Then Return "Error " & reference & " : Barcode Should Be Less Than 20 Characters (" & barcode & ")"
                    If Not allBarcodes.Add(barcode) Then Return "Error Barcode " & barcode & " Already Exist In This Receiving Order"
                    'A pending item has no barcode yet : any item that has it is another item
                    If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_ItemBarcodes WHERE barcode = @Barcode AND itemRecID <> @ItemRecID ",
                                ROParam("@Barcode", barcode), ROParam("@ItemRecID", If(line.IsPending, -1D, line.ItemRecID))).ToString <> "0" Then
                        Return "Error " & reference & " : The Barcode " & barcode & " Already Exist For Another Item."
                    End If
                Next

                lines.Add(line)

            Next

            'Returns of the quantity controlled items : not more than the qty of the location (QtyCntrEc of FrmDO)
            Dim returns As New Dictionary(Of String, Decimal)
            Dim returnLines As New Dictionary(Of String, ROLine)
            For Each line As ROLine In lines
                If line.Status <> "R" OrElse line.Item("itemQtyControl").ToString <> "Y" Then Continue For
                If line.Lots.Count = 0 Then
                    Dim key As String = line.ItemRecID.ToString & "||"
                    returns(key) = If(returns.ContainsKey(key), returns(key), 0) + line.Qty
                    returnLines(key) = line
                Else
                    For Each lot As ROLot In line.Lots
                        Dim key As String = line.ItemRecID.ToString & "|" & lot.Expiry & "|" & lot.Batch
                        returns(key) = If(returns.ContainsKey(key), returns(key), 0) + lot.Qty
                        returnLines(key) = line
                    Next
                End If
            Next
            For Each key As String In returns.Keys
                Dim parts() As String = key.Split("|"c)
                Dim available As Object = ROScalar(cnsql, trnsql, " SELECT SUM(ISNULL(stockAvailable, 0) - ISNULL(stockReserved, 0) - ISNULL(stockTransferOut, 0))
                                                                    FROM V_CPP_StockQty
                                                                    WHERE itemRecID = @ItemRecID AND locationCode = @LocationCode
                                                                          AND ISNULL(stockExpiry, '') = @Expiry AND ISNULL(stockBatch, '') = @Batch ",
                                                   ROParam("@ItemRecID", CDec(parts(0))), ROParam("@LocationCode", locationCode),
                                                   ROParam("@Expiry", parts(1)), ROParam("@Batch", parts(2)))
                Dim lotText As String = If(parts(1) = "", "", " (" & parts(1) & " / " & parts(2) & ")")
                Dim reference As String = returnLines(key).Item("itemReference").ToString
                If available Is Nothing OrElse IsDBNull(available) Then Return "Error Item " & reference & lotText & " Does Not Have Quantity In This Location"
                If CDec(available) < returns(key) Then Return "Error Item " & reference & lotText & " Have " & CDec(available).ToString("0.###") & " Available Quantity"
            Next

        End If

        '#Record ids : the last ones of each table, locked until the end of the transaction (GetMaxRecId of the ERP)

        Dim lastID = Function(sql As String) As Decimal
                         Dim value As Object = ROScalar(cnsql, trnsql, sql)
                         Return If(value Is Nothing OrElse IsDBNull(value), 0D, CDec(value))
                     End Function

        Dim nextLine As Decimal = lastID(" SELECT MAX(DOD_Recid) FROM dbo.stk04_PurDelOrdD WITH ( UPDLOCK, HOLDLOCK ) ")
        Dim nextLot As Decimal = lastID(" SELECT MAX(DOQ_RecID) FROM dbo.stk04_PurDelOrdDQty WITH ( UPDLOCK, HOLDLOCK ) ")
        Dim nextBarcode As Decimal = lastID(" SELECT MAX(DOB_RecID) FROM dbo.stk04_PurDelOrdDBE WITH ( UPDLOCK, HOLDLOCK ) ")

        '#Header

        Dim savedOrder As Decimal = receivingOrder

        If mode = "MODIFY" Then

            ROExecute(cnsql, trnsql, " UPDATE dbo.stk04_PurDelOrdH SET DOH_ActDelDate = @ActualDate , DOH_Dstamp = GETDATE() , DOH_Ustamp = @User WHERE DOH_Recid = @RORecID ",
                      ROParam("@ActualDate", roActualDate), ROParam("@User", user), ROParam("@RORecID", receivingOrder))

        Else

            'A new RO, or the reverse RO (status R) of the cancel with the same supplier, ref and dates
            savedOrder = lastID(" SELECT MAX(DOH_Recid) FROM dbo.stk04_PurDelOrdH WITH ( UPDLOCK, HOLDLOCK ) ") + 1
            ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdH ( DOH_Recid , DOH_PORecid , DOH_SupRecid , DOH_Ref , DOH_Date , DOH_Status , DOH_loc , DOH_ActDelDate , DOH_PurRecid ,
                                                                     DOH_Dstamp , DOH_Ustamp , DOH_Type , DOH_Brand , DOH_Season , DOH_CreateDate , DOH_CreateUser , DOH_Curr )
                                       VALUES ( @RORecID , @PORecID , @SupplierRecID , @RORef , @RODate , @Status , @LocationCode , @ActualDate , NULL ,
                                                GETDATE() , @User , 'T' , NULL , NULL , GETDATE() , @User , NULL ) ",
                      ROParam("@RORecID", savedOrder), ROParam("@PORecID", If(purchaseOrder = 0, Nothing, CObj(purchaseOrder))),
                      ROParam("@SupplierRecID", supplier), ROParam("@RORef", roRef), ROParam("@RODate", roDate),
                      ROParam("@Status", If(mode = "CANCEL", "R", "N")), ROParam("@LocationCode", locationCode),
                      ROParam("@ActualDate", roActualDate), ROParam("@User", user))

        End If

        '#The stock movements : item / expiry / lot -> signed qty (a return takes out, the cancel reverses everything)

        Dim movements As New List(Of Object())   ' {itemRecID, itemClass, expiry, batch, production, qty, qtyControl, returnSale}

        If mode = "CANCEL" Then

            'The lines, lots and barcodes of the RO copied to the reverse RO
            Dim dtLines As DataTable = RORead(cnsql, trnsql, " SELECT D.* , ISNULL(I.itemClass, '') [itemClass] , ISNULL(I.itemQtyControl, 'N') [itemQtyControl]
                                                               FROM V_CPP_ROLines D LEFT JOIN V_CPP_ROItems I ON I.itemRecID = D.itemRecID
                                                               WHERE D.roRecID = @RORecID ORDER BY D.lineRecID ",
                                              ROParam("@RORecID", receivingOrder))

            For Each row As DataRow In dtLines.Rows

                nextLine += 1
                ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdD ( DOD_Recid , DOD_Hrecid , DOD_ItmRef , DOD_Stat , DOD_Counter , DOD_StkRecid , DOD_Recqty , DOD_Unit , DOD_ConsQty ,
                                                                       DOD_AvgCostLC , DOD_AvgCostCC , DOD_AvgCostFC , DOD_vat , DOD_Dstamp , DOD_Ustamp , DOD_Color ,
                                                                       DOD_CreateDate , DOD_CreateUser , DOD_ColorCode , DOD_Disc , DOD_NetCostLC , DOD_NetCostCC , DOD_NetCostFC )
                                           SELECT @LineRecID , @NewRORecID , DOD_ItmRef , DOD_Stat , DOD_Counter , DOD_StkRecid , DOD_Recqty , DOD_Unit , DOD_ConsQty ,
                                                  DOD_AvgCostLC , DOD_AvgCostCC , DOD_AvgCostFC , DOD_vat , GETDATE() , @User , DOD_Color ,
                                                  GETDATE() , @User , DOD_ColorCode , DOD_Disc , DOD_NetCostLC , DOD_NetCostCC , DOD_NetCostFC
                                           FROM   dbo.stk04_PurDelOrdD WHERE DOD_Recid = @OldLineRecID ",
                          ROParam("@LineRecID", nextLine), ROParam("@NewRORecID", savedOrder), ROParam("@User", user), ROParam("@OldLineRecID", row("lineRecID")))

                Dim dtLots As DataTable = RORead(cnsql, trnsql, " SELECT * FROM V_CPP_ROLineLots WHERE lineRecID = @LineRecID ORDER BY lotRecID ", ROParam("@LineRecID", row("lineRecID")))
                For Each lot As DataRow In dtLots.Rows
                    nextLot += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdDQty ( DOQ_RecID , DOQ_Drecid , DOQ_ExpiryDate , DOQ_BatchInfo , DOQ_GridCode , DOQ_GridSize , DOQ_Recqty , DOQ_CrDstamp , DOQ_CrUstamp , DOQ_Dstamp , DOQ_Ustamp )
                                               SELECT @LotRecID , @LineRecID , DOQ_ExpiryDate , DOQ_BatchInfo , DOQ_GridCode , DOQ_GridSize , DOQ_Recqty , GETDATE() , @User , GETDATE() , @User
                                               FROM   dbo.stk04_PurDelOrdDQty WHERE DOQ_RecID = @OldLotRecID ",
                              ROParam("@LotRecID", nextLot), ROParam("@LineRecID", nextLine), ROParam("@User", user), ROParam("@OldLotRecID", lot("lotRecID")))
                Next

                Dim dtBarcodes As DataTable = RORead(cnsql, trnsql, " SELECT barcodeRecID FROM V_CPP_ROLineBarcodes WHERE lineRecID = @LineRecID ORDER BY barcodeRecID ", ROParam("@LineRecID", row("lineRecID")))
                For Each barcode As DataRow In dtBarcodes.Rows
                    nextBarcode += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdDBE ( DOB_RecID , DOB_Drecid , DOB_Barcode , DOB_ExpiryDate , DOB_BatchInfo , DOB_GridCode , DOB_GridSize , DOB_CrDstamp , DOB_CrUstamp , DOB_Dstamp , DOB_Ustamp )
                                               SELECT @BarcodeRecID , @LineRecID , DOB_Barcode , DOB_ExpiryDate , DOB_BatchInfo , DOB_GridCode , DOB_GridSize , GETDATE() , @User , GETDATE() , @User
                                               FROM   dbo.stk04_PurDelOrdDBE WHERE DOB_RecID = @OldBarcodeRecID ",
                              ROParam("@BarcodeRecID", nextBarcode), ROParam("@LineRecID", nextLine), ROParam("@User", user), ROParam("@OldBarcodeRecID", barcode("barcodeRecID")))
                Next

                'Reversed : what the line added is taken out
                Dim sign As Decimal = If(row("lineStatus").ToString.Trim.ToUpper = "R", 1D, -1D)
                If dtLots.Rows.Count > 0 Then
                    For Each lot As DataRow In dtLots.Rows
                        movements.Add({CDec(row("itemRecID")), "E", lot("lotExpiry").ToString.Trim, lot("lotBatch").ToString.Trim, "", sign * CDec(lot("lotQty")), row("itemQtyControl").ToString, Nothing})
                    Next
                Else
                    movements.Add({CDec(row("itemRecID")), "T", "", "", "", sign * CDec(row("lineQty")), row("itemQtyControl").ToString, Nothing})
                End If

            Next

        Else

            'The PO costs of the lines imported from the PO (FillGridDetails of FrmDO takes MOD_UnitCost), the average cost of the item otherwise
            'A pending item is found by its pending record id (the PO line has no item yet)
            Dim dtPOCosts As New DataTable
            If purchaseOrder <> 0 Then
                dtPOCosts = RORead(cnsql, trnsql, " SELECT ISNULL(itemRecID, -1) [itemRecID] , ISNULL(pendingRecID, -1) [pendingRecID] , supplierRef , lineStatus , unitCost
                                                    FROM V_CPP_ROPurchaseOrderLines WHERE poRecID = @PORecID ",
                                   ROParam("@PORecID", purchaseOrder))
            End If

            'The items of the pending stock file are created in the stock file (TestStkFil and InsertStkFil of FrmDO), their lines get the new item
            Dim createdItems As New Dictionary(Of Decimal, Decimal)
            For Each line As ROLine In lines
                If Not line.IsPending Then Continue For
                If Not createdItems.ContainsKey(line.PendingRecID) Then
                    Dim created As Object = ROCreatePendingItem(cnsql, trnsql, user, line, dtSupplier.Rows(0)("supplierCode").ToString, settings)
                    If TypeOf created Is String Then Return created.ToString
                    createdItems(line.PendingRecID) = CDec(created)
                End If
                line.ItemRecID = createdItems(line.PendingRecID)
            Next

            'The counter of a line : its number among the lines of the RO with the same supplier ref and status
            Dim counters As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim nextSupplierRef As Decimal = lastID(" SELECT MAX(SREF_RecId) FROM dbo.STK04_StkSupRef WITH ( UPDLOCK, HOLDLOCK ) ")
            Dim nextItemBarcode As Decimal = lastID(" SELECT MAX(dbar_recid) FROM dbo.STK04_DetBarcode WITH ( UPDLOCK, HOLDLOCK ) ")
            Dim stampDate As Date = today

            For Each line As ROLine In lines

                Dim counterKey As String = line.SupplierRef & "|" & line.Status
                If Not counters.ContainsKey(counterKey) Then
                    Dim lastCounter As Object = ROScalar(cnsql, trnsql, " SELECT MAX(lineCounter) FROM V_CPP_ROLines WHERE roRecID = @RORecID AND supplierRef = @SupplierRef AND lineStatus = @Status ",
                                                         ROParam("@RORecID", savedOrder), ROParam("@SupplierRef", line.SupplierRef), ROParam("@Status", line.Status))
                    counters(counterKey) = If(lastCounter Is Nothing OrElse IsDBNull(lastCounter), 0, CInt(lastCounter))
                End If
                counters(counterKey) += 1

                Dim cost As Decimal = CDec(line.Item("itemCostCC"))
                Dim poCost() As DataRow = If(purchaseOrder = 0, New DataRow() {},
                                             dtPOCosts.Select(If(line.IsPending, "pendingRecID = " & line.PendingRecID.ToString(Globalization.CultureInfo.InvariantCulture),
                                                                                 "itemRecID = " & line.ItemRecID.ToString(Globalization.CultureInfo.InvariantCulture)) &
                                                              " AND supplierRef = '" & line.SupplierRef.Replace("'", "''") & "' AND lineStatus = '" & line.Status & "'"))
                If poCost.Length > 0 Then cost = CDec(poCost(0)("unitCost"))

                Dim itemCurrency As String = line.Item("itemCurrency").ToString.Trim
                If itemCurrency = "" Then itemCurrency = settings.CurrencyCC
                Dim vatType As String = line.Item("itemVatType").ToString.Trim
                vatType = If(vatType = "", Nothing, vatType.Substring(0, 1).ToUpper)

                nextLine += 1
                line.LineRecID = nextLine

                'Costs in the control currency, converted to the local and the item currency (btnSave_Click of FrmDO)
                ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdD ( DOD_Recid , DOD_Hrecid , DOD_ItmRef , DOD_Stat , DOD_Counter , DOD_StkRecid , DOD_Recqty , DOD_Unit , DOD_ConsQty ,
                                                                       DOD_AvgCostLC , DOD_AvgCostCC , DOD_AvgCostFC , DOD_vat , DOD_Dstamp , DOD_Ustamp , DOD_Color ,
                                                                       DOD_CreateDate , DOD_CreateUser , DOD_ColorCode , DOD_Disc , DOD_NetCostLC , DOD_NetCostCC , DOD_NetCostFC )
                                           VALUES ( @LineRecID , @RORecID , @SupplierRef , @Status , @Counter , @ItemRecID , @Qty , @UnitCode , @ConsQty ,
                                                    dbo.convert5(@App, @Comp, @Year, @Cost, @CurrencyCC, @CurrencyLC, @StampDate) , @Cost ,
                                                    dbo.convert5(@App, @Comp, @Year, @Cost, @CurrencyCC, @ItemCurrency, @StampDate) ,
                                                    @VatType , GETDATE() , @User , NULL , GETDATE() , @User , NULL , NULL ,
                                                    dbo.convert5(@App, @Comp, @Year, @Net, @CurrencyCC, @CurrencyLC, @StampDate) , @Net ,
                                                    dbo.convert5(@App, @Comp, @Year, @Net, @CurrencyCC, @ItemCurrency, @StampDate) ) ",
                          ROParam("@LineRecID", line.LineRecID), ROParam("@RORecID", savedOrder), ROParam("@SupplierRef", line.SupplierRef),
                          ROParam("@Status", line.Status), ROParam("@Counter", counters(counterKey)), ROParam("@ItemRecID", line.ItemRecID),
                          ROParam("@Qty", line.Qty), ROParam("@UnitCode", line.Item("itemUnitCode")), ROParam("@ConsQty", ROConsQty(line.Item, line.Qty)),
                          ROParam("@App", settings.AppCode), ROParam("@Comp", settings.CompCode), ROParam("@Year", settings.Year),
                          ROParam("@Cost", cost), ROParam("@Net", Math.Round(cost * line.Qty, 3)),
                          ROParam("@CurrencyCC", settings.CurrencyCC), ROParam("@CurrencyLC", settings.CurrencyLC), ROParam("@ItemCurrency", itemCurrency),
                          ROParam("@StampDate", stampDate), ROParam("@VatType", vatType), ROParam("@User", user))

                'Lots and barcodes of the line (stk04_PurDelOrdDQty, stk04_PurDelOrdDBE)
                For Each lot As ROLot In line.Lots
                    nextLot += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdDQty ( DOQ_RecID , DOQ_Drecid , DOQ_ExpiryDate , DOQ_BatchInfo , DOQ_GridCode , DOQ_GridSize , DOQ_Recqty , DOQ_CrDstamp , DOQ_CrUstamp , DOQ_Dstamp , DOQ_Ustamp )
                                               VALUES ( @LotRecID , @LineRecID , @Expiry , @Batch , NULL , NULL , @Qty , GETDATE() , @User , GETDATE() , @User ) ",
                              ROParam("@LotRecID", nextLot), ROParam("@LineRecID", line.LineRecID), ROParam("@Expiry", lot.Expiry),
                              ROParam("@Batch", lot.Batch), ROParam("@Qty", lot.Qty), ROParam("@User", user))
                    For Each barcode As String In lot.Barcodes
                        nextBarcode += 1
                        ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdDBE ( DOB_RecID , DOB_Drecid , DOB_Barcode , DOB_ExpiryDate , DOB_BatchInfo , DOB_GridCode , DOB_GridSize , DOB_CrDstamp , DOB_CrUstamp , DOB_Dstamp , DOB_Ustamp )
                                                   VALUES ( @BarcodeRecID , @LineRecID , @Barcode , @Expiry , @Batch , NULL , NULL , GETDATE() , @User , GETDATE() , @User ) ",
                                  ROParam("@BarcodeRecID", nextBarcode), ROParam("@LineRecID", line.LineRecID), ROParam("@Barcode", barcode),
                                  ROParam("@Expiry", lot.Expiry), ROParam("@Batch", lot.Batch), ROParam("@User", user))
                    Next
                Next
                For Each barcode As String In line.Barcodes
                    nextBarcode += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_PurDelOrdDBE ( DOB_RecID , DOB_Drecid , DOB_Barcode , DOB_ExpiryDate , DOB_BatchInfo , DOB_GridCode , DOB_GridSize , DOB_CrDstamp , DOB_CrUstamp , DOB_Dstamp , DOB_Ustamp )
                                               VALUES ( @BarcodeRecID , @LineRecID , @Barcode , NULL , NULL , NULL , NULL , GETDATE() , @User , GETDATE() , @User ) ",
                              ROParam("@BarcodeRecID", nextBarcode), ROParam("@LineRecID", line.LineRecID), ROParam("@Barcode", barcode), ROParam("@User", user))
                Next

                'The supplier reference of the item for this supplier (STK04_StkSupRef, source DO)
                If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_ItemSupplierRefs WHERE itemRecID = @ItemRecID AND supplierRecID = @SupplierRecID
                                                    AND ISNULL(supplierRef, '') = @SupplierRef AND ISNULL(refSupplierColor, '') = '' AND ISNULL(refColor, '') = '' ",
                            ROParam("@ItemRecID", line.ItemRecID), ROParam("@SupplierRecID", supplier), ROParam("@SupplierRef", line.SupplierRef)).ToString = "0" Then
                    nextSupplierRef += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.STK04_StkSupRef ( SRef_StkRecId , SRef_SupRecId , SRef_SupRef , SRef_Source , SRef_UStamp , SRef_DStamp , SRef_SupColor , SRef_ItmColor , SREF_RecId )
                                               VALUES ( @ItemRecID , @SupplierRecID , @SupplierRef , 'DO' , @User , GETDATE() , NULL , NULL , @RefRecID ) ",
                              ROParam("@ItemRecID", line.ItemRecID), ROParam("@SupplierRecID", supplier), ROParam("@SupplierRef", line.SupplierRef),
                              ROParam("@User", user), ROParam("@RefRecID", nextSupplierRef))
                End If

                'The new barcodes are added to the item (STK04_DetBarcode), with the expiry / lot for the expiry items
                Dim itemBarcodes As New List(Of String())
                For Each lot As ROLot In line.Lots
                    For Each barcode As String In lot.Barcodes
                        itemBarcodes.Add({barcode, lot.Expiry, lot.Batch})
                    Next
                Next
                For Each barcode As String In line.Barcodes
                    itemBarcodes.Add({barcode, "", ""})
                Next
                For Each barcode() As String In itemBarcodes
                    If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_ItemBarcodes WHERE itemRecID = @ItemRecID AND barcode = @Barcode
                                                        AND ISNULL(barcodeExpiry, '') = @Expiry AND ISNULL(barcodeBatch, '') = @Batch ",
                                ROParam("@ItemRecID", line.ItemRecID), ROParam("@Barcode", barcode(0)), ROParam("@Expiry", barcode(1)), ROParam("@Batch", barcode(2))).ToString <> "0" Then Continue For
                    nextItemBarcode += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.STK04_DetBarcode ( dbar_recid , dbar_stkid , dbar_grid , dbar_Detgrid , dbar_color , dbar_expdate , dbar_batch , dbar_barcode ,
                                                                                dbar_DSTMP , dbar_USTMP , dbar_CrDSTMP , dbar_CrUSTMP )
                                               VALUES ( @BarcodeRecID , @ItemRecID , NULL , NULL , NULL , @Expiry , @Batch , @Barcode , GETDATE() , @User , GETDATE() , @User ) ",
                              ROParam("@BarcodeRecID", nextItemBarcode), ROParam("@ItemRecID", line.ItemRecID),
                              ROParam("@Expiry", If(barcode(1) = "", Nothing, barcode(1))), ROParam("@Batch", If(barcode(2) = "", Nothing, barcode(2))),
                              ROParam("@Barcode", barcode(0)), ROParam("@User", user))
                Next

                Dim sign As Decimal = If(line.Status = "R", -1D, 1D)
                If line.Lots.Count > 0 Then
                    For Each lot As ROLot In line.Lots
                        movements.Add({line.ItemRecID, "E", lot.Expiry, lot.Batch, lot.Production, sign * lot.Qty, line.Item("itemQtyControl").ToString, line.Item("itemReturnSale")})
                    Next
                Else
                    movements.Add({line.ItemRecID, "T", "", "", "", sign * line.Qty, line.Item("itemQtyControl").ToString, Nothing})
                End If

            Next

        End If

        '#Stock qty of the location (InsertMissingRecQty then the update of btnSave_Click of FrmDO)

        'One movement per item / expiry / lot
        Dim totals As New Dictionary(Of String, Object())
        For Each movement As Object() In movements
            Dim key As String = movement(0).ToString & "|" & movement(2).ToString & "|" & movement(3).ToString
            If totals.ContainsKey(key) Then
                totals(key)(5) = CDec(totals(key)(5)) + CDec(movement(5))
            Else
                totals(key) = CType(movement.Clone, Object())
            End If
        Next

        Dim nextStock As Decimal = lastID(" SELECT MAX(STQ_RecId) FROM dbo.stk04_stkqty WITH ( UPDLOCK, HOLDLOCK ) ")

        For Each movement As Object() In totals.Values

            Dim item As Decimal = CDec(movement(0))
            Dim lotItem As Boolean = movement(1).ToString = "E"
            Dim expiry As String = movement(2).ToString
            Dim batch As String = movement(3).ToString
            Dim qty As Decimal = CDec(movement(5))

            'The missing rows of the location and of the 00 location (a lot item needs its expiry and lot)
            If mode <> "CANCEL" AndAlso (Not lotItem OrElse (expiry <> "" AndAlso batch <> "")) Then
                For Each location As String In {locationCode, "00"}
                    Dim exists As String = If(lotItem,
                        " SELECT COUNT(*) FROM V_CPP_StockQty WHERE itemRecID = @ItemRecID AND locationCode = @LocationCode AND stockExpiry = @Expiry AND stockBatch = @Batch ",
                        " SELECT COUNT(*) FROM V_CPP_StockQty WHERE itemRecID = @ItemRecID AND locationCode = @LocationCode ")
                    If ROScalar(cnsql, trnsql, exists, ROParam("@ItemRecID", item), ROParam("@LocationCode", location),
                                ROParam("@Expiry", expiry), ROParam("@Batch", batch)).ToString <> "0" Then Continue For
                    nextStock += 1
                    ROExecute(cnsql, trnsql, " INSERT INTO dbo.stk04_stkqty ( STQ_STKRECID , STQ_LOCCODE , STQ_BBFQTY , STQ_BBFDATE , STQ_BBFSRC , STQ_RESERVED , STQ_AVAILABLE ,
                                                                            STQ_DSTAMP , STQ_USTAMP , STQ_RecId , STQ_Color , STQ_Grid , STQ_DetGrid , STQ_ExpiryDt , STQ_BatchInfo ,
                                                                            STQ_TransPendO , STQ_TransPendI , STQ_ReturnSale , STQ_ProcProduction , STQ_ProductionDt )
                                               VALUES ( @ItemRecID , @LocationCode , 0 , GETDATE() , 'O' , 0 , 0 ,
                                                        GETDATE() , @User , @StockRecID , NULL , NULL , NULL , @Expiry , @Batch ,
                                                        0 , 0 , @ReturnSale , 0 , @Production ) ",
                              ROParam("@ItemRecID", item), ROParam("@LocationCode", location), ROParam("@User", user), ROParam("@StockRecID", nextStock),
                              ROParam("@Expiry", If(lotItem, expiry, Nothing)), ROParam("@Batch", If(lotItem, batch, Nothing)),
                              ROParam("@ReturnSale", If(lotItem AndAlso location = "00", movement(7), Nothing)),
                              ROParam("@Production", If(lotItem AndAlso movement(4).ToString <> "", movement(4), Nothing)))
                Next
            End If

            'A quantity controlled item cannot go under 0 in the location
            If movement(6).ToString = "Y" Then
                If ROScalar(cnsql, trnsql, " SELECT COUNT(*) FROM V_CPP_StockQty WHERE itemRecID = @ItemRecID AND locationCode = @LocationCode
                                                    AND ISNULL(stockExpiry, '') = @Expiry AND ISNULL(stockBatch, '') = @Batch AND ISNULL(stockAvailable, 0) + @Qty < 0 ",
                            ROParam("@ItemRecID", item), ROParam("@LocationCode", locationCode), ROParam("@Expiry", expiry),
                            ROParam("@Batch", batch), ROParam("@Qty", qty)).ToString <> "0" Then
                    Return "Error Exists Item With No Available Qty (" & ROScalar(cnsql, trnsql, " SELECT TOP 1 itemReference FROM V_CPP_ROItems WHERE itemRecID = @ItemRecID ", ROParam("@ItemRecID", item))?.ToString & ")"
                End If
            End If

            ROExecute(cnsql, trnsql, " UPDATE dbo.stk04_stkqty SET STQ_AVAILABLE = ISNULL(STQ_AVAILABLE, 0) + @Qty , STQ_DSTAMP = GETDATE() , STQ_USTAMP = @User
                                       WHERE STQ_STKRECID = @ItemRecID AND STQ_LOCCODE = @LocationCode AND ISNULL(STQ_ExpiryDt, '') = @Expiry AND ISNULL(STQ_BatchInfo, '') = @Batch ",
                      ROParam("@Qty", qty), ROParam("@User", user), ROParam("@ItemRecID", item), ROParam("@LocationCode", locationCode),
                      ROParam("@Expiry", expiry), ROParam("@Batch", batch))

        Next

        Return "Success|" & savedOrder.ToString("0")

    End Function

#End Region

#Region "Other"

    Public Function GetJson(ByRef _DataTable As DataTable) As String

        Try

            Dim row As Dictionary(Of String, Object)
            Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
            Dim rows As New List(Of Dictionary(Of String, Object))

            For Each dr As DataRow In _DataTable.Rows
                row = New Dictionary(Of String, Object)
                For Each col As DataColumn In _DataTable.Columns
                    row.Add(col.ColumnName, dr(col))
                Next
                rows.Add(row)
            Next

            serializer.MaxJsonLength = 536870912

            Return serializer.Serialize(rows)

        Catch ex As Exception
            Throw ex
        End Try

    End Function

#End Region

End Class
