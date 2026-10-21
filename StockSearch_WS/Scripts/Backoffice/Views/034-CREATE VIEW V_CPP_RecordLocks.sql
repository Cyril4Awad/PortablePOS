SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The records opened for modification in the ERP (sysLockRecords) : the portable does not save a receiving order
-- or import a purchase order that is locked on a machine
CREATE OR ALTER VIEW V_CPP_RecordLocks
AS
    SELECT  lr_Table [lockTable] ,
            lr_Record [lockRecord] ,
            lr_Machine [lockMachine] ,
            lr_Application [lockApplication] ,
            lr_User [lockUser] ,
            lr_Dstmp [lockDate]
    FROM    dbo.sysLockRecords;


GO
