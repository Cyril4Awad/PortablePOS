package com.example.erpdatacollectorn;

public class ClsConfigurationSettingsDB {
	
    //Table Name
    public static final String TABLE_Name = "tblConfigurationSettings";


    //Column Names
    public static final String COL__id_Name = "_id";
    public static final String COL__id_Type = " INTEGER PRIMARY KEY";
    public static final String COL_LocCode_Name = "LocCode";
    public static final String COL_LocCode_Type = " TEXT";
    public static final String COL_serviceUrl_Name = "serviceUrl";
    public static final String COL_SelectedRecId_Name = "SelectedRecId";
    public static final String COL_Selected_Name = "Selected";
    public static final String COL_Selected_Type = " INTEGER";
    public static final String COL_Text_Type = " TEXT";
    
    public static final String COL_SP_DecNo_Name = "SP_DecNo";
    public static final String COL_SP_Fashion_Name = "SP_Fashion";
    public static final String COL_SP_Expiry_Name = "SP_Expiry";
    
    public static final String CREATE_TABLE = "CREATE TABLE IF NOT EXISTS "
            + TABLE_Name + "("
            + COL__id_Name + COL__id_Type + ","
            + COL_LocCode_Name + COL_LocCode_Type + ","
            + COL_serviceUrl_Name + COL_Text_Type + ","
            + COL_SelectedRecId_Name + COL_Text_Type + ","
            + COL_SP_DecNo_Name + COL_Text_Type + ","
            + COL_SP_Fashion_Name + COL_Text_Type + ","
            + COL_SP_Expiry_Name + COL_Text_Type + ","
            + COL_Selected_Name + COL_Selected_Type + ")";

    public static final String DROP_TABLE = "DROP TABLE IF EXISTS "+ TABLE_Name;
    

}
