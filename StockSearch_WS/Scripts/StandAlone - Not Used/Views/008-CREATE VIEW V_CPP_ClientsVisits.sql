

CREATE VIEW V_CPP_ClientsVisits AS 

WITH    AllDates
          AS ( SELECT   CAST(DATEFROMPARTS(2024, 1, 1) AS DATE) AS [Date]
               UNION ALL
               SELECT   DATEADD(DAY, 1, [Date])
               FROM     AllDates
               WHERE    DATEPART(YEAR, DATEADD(DAY, 1, [Date])) = 2024
             ),
        FirstMondays
          AS ( SELECT   MIN([Date]) AS FirstMonday
               FROM     AllDates
               WHERE    DATEPART(WEEKDAY, [Date]) = 2
               GROUP BY DATEPART(YEAR, [Date]) ,
                        DATEPART(MONTH, [Date])
             ),
        DateRange
          AS ( SELECT   [Date] ,
                        fm.FirstMonday ,
                        CASE WHEN [Date] = fm.FirstMonday THEN 'First Monday'
                             ELSE ''
                        END AS FirstMondayFlag
               FROM     AllDates d
                        JOIN FirstMondays fm ON d.[Date] >= fm.FirstMonday
                                                AND d.[Date] < DATEADD(DAY, 28,
                                                              fm.FirstMonday)
             ),
        Months
          AS ( SELECT   1 AS MonthNumber
               UNION ALL
               SELECT   MonthNumber + 1
               FROM     Months
               WHERE    MonthNumber < 12
             ),
        ClientVisits
          AS ( SELECT   ClientRecID ,
                        ClientCode ,
                        ClientName ,
                        MonthNumber ,
                        VisitWeekNumber ,
                        ROW_NUMBER() OVER ( PARTITION BY MonthNumber ORDER BY Months.MonthNumber ) DayNumber ,
                        Visit
               FROM     ( SELECT    ClientRecID ,
                                    Stk04_ClientsReg.ClientCode ,
                                    Stk04_ClientsReg.ClientName ,
                                    DayOfTheWeek ,
                                    1 [VisitWeekNumber] ,
                                    SUBSTRING(ClR_week, DayOfTheWeek, 1) AS Visit
                          FROM      ( SELECT TOP 7
                                                ROW_NUMBER() OVER ( ORDER BY ( SELECT
                                                              NULL
                                                              ) ) AS DayOfTheWeek
                                      FROM      master.dbo.spt_values
                                    ) Numbers
                                    INNER JOIN ( SELECT ClR_Recid ClientRecID ,
                                                        CONCAT(CL_Code, ' / ',
                                                              CL_No) ClientCode ,
                                                        CL_Name ClientName ,
                                                        SUBSTRING(ClR_week1, 2,
                                                              7) ClR_week
                                                 FROM   Stk04_ClientsReg
                                                        INNER JOIN dbo.Stk04_Clients ON Stk04_Clients.CL_RecID = Stk04_ClientsReg.ClR_Recid
                                               ) Stk04_ClientsReg ON 1 = 1
                          UNION ALL
                          SELECT    ClientRecID ,
                                    Stk04_ClientsReg.ClientCode ,
                                    Stk04_ClientsReg.ClientName ,
                                    DayOfTheWeek ,
                                    2 [VisitWeekNumber] ,
                                    SUBSTRING(ClR_week, DayOfTheWeek, 1) AS Visit
                          FROM      ( SELECT TOP 7
                                                ROW_NUMBER() OVER ( ORDER BY ( SELECT
                                                              NULL
                                                              ) ) AS DayOfTheWeek
                                      FROM      master.dbo.spt_values
                                    ) Numbers
                                    INNER JOIN ( SELECT ClR_Recid ClientRecID ,
                                                        CONCAT(CL_Code, ' / ',
                                                              CL_No) ClientCode ,
                                                        CL_Name ClientName ,
                                                        SUBSTRING(ClR_week2, 2,
                                                              7) ClR_week
                                                 FROM   Stk04_ClientsReg
                                                        INNER JOIN dbo.Stk04_Clients ON Stk04_Clients.CL_RecID = Stk04_ClientsReg.ClR_Recid
                                               ) Stk04_ClientsReg ON 1 = 1
                          UNION ALL
                          SELECT    ClientRecID ,
                                    Stk04_ClientsReg.ClientCode ,
                                    Stk04_ClientsReg.ClientName ,
                                    DayOfTheWeek ,
                                    3 [VisitWeekNumber] ,
                                    SUBSTRING(ClR_week, DayOfTheWeek, 1) AS Visit
                          FROM      ( SELECT TOP 7
                                                ROW_NUMBER() OVER ( ORDER BY ( SELECT
                                                              NULL
                                                              ) ) AS DayOfTheWeek
                                      FROM      master.dbo.spt_values
                                    ) Numbers
                                    INNER JOIN ( SELECT ClR_Recid ClientRecID ,
                                                        CONCAT(CL_Code, ' / ',
                                                              CL_No) ClientCode ,
                                                        CL_Name ClientName ,
                                                        SUBSTRING(ClR_week3, 2,
                                                              7) ClR_week
                                                 FROM   Stk04_ClientsReg
                                                        INNER JOIN dbo.Stk04_Clients ON Stk04_Clients.CL_RecID = Stk04_ClientsReg.ClR_Recid
                                               ) Stk04_ClientsReg ON 1 = 1
                          UNION ALL
                          SELECT    ClientRecID ,
                                    Stk04_ClientsReg.ClientCode ,
                                    Stk04_ClientsReg.ClientName ,
                                    DayOfTheWeek ,
                                    4 [VisitWeekNumber] ,
                                    SUBSTRING(ClR_week, DayOfTheWeek, 1) AS Visit
                          FROM      ( SELECT TOP 7
                                                ROW_NUMBER() OVER ( ORDER BY ( SELECT
                                                              NULL
                                                              ) ) AS DayOfTheWeek
                                      FROM      master.dbo.spt_values
                                    ) Numbers
                                    INNER JOIN ( SELECT ClR_Recid ClientRecID ,
                                                        CONCAT(CL_Code, ' / ',
                                                              CL_No) ClientCode ,
                                                        CL_Name ClientName ,
                                                        SUBSTRING(ClR_week4, 2,
                                                              7) ClR_week
                                                 FROM   Stk04_ClientsReg
                                                        INNER JOIN dbo.Stk04_Clients ON Stk04_Clients.CL_RecID = Stk04_ClientsReg.ClR_Recid
                                               ) Stk04_ClientsReg ON 1 = 1
                        ) ClientVisits
                        INNER JOIN Months ON 1 = 1
             )
    SELECT  ClientVisits.ClientRecID ,
            ClientVisits.ClientCode ,
            ClientVisits.ClientName ,
            ClientVisits.MonthNumber ,
            ClientVisits.VisitWeekNumber ,
            ClientVisits.DayNumber ,
            ClientVisits.Visit ,
            tblDaterates.dr_Date ,
            tblDaterates.dr_MonthNumber ,
            tblDaterates.dr_DayNumber ,
            tblDaterates.FirstMondayFlag
    FROM    ClientVisits
            INNER JOIN ( SELECT [Date] dr_Date ,
                                DATEPART(MONTH, [Date]) dr_MonthNumber ,
                                DATEPART(DAY, [Date]) dr_DayNumber ,
                                FirstMondayFlag
                         FROM   DateRange
                         --WHERE  [Date] = '2024-01-04'
                       ) tblDaterates ON dr_MonthNumber = MonthNumber
                                         AND dr_DayNumber = DayNumber
   





