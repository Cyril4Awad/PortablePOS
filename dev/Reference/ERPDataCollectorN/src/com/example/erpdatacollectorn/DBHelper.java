package com.example.erpdatacollectorn;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

public class DBHelper extends SQLiteOpenHelper {


    private static DBHelper sInstance;

    public static final int DB_VERSION = 1;
    public static final String DB_NAME = "SSDCSettings";

    /**
     * Constructor should be private to prevent direct instantiation.
     * make call to static method "getInstance()" instead.
     */
    private DBHelper(Context context) {
        super(context, DB_NAME, null, DB_VERSION);
    }

    public static synchronized DBHelper getInstance(Context context) {

        // Use the application context, which will ensure that you
        // don't accidentally leak an Activity's context.
        // See this article for more information: http://bit.ly/6LRzfx
        if (sInstance == null) {
            sInstance = new DBHelper(context.getApplicationContext());
        }
        return sInstance;
    }


    @Override
    public void onCreate(SQLiteDatabase db) {
        createTables(db);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int arg1, int arg2) {
        dropTables(db);
        onCreate(db);
    }

    public void createTables(SQLiteDatabase db) {
        db.execSQL(ClsConfigurationSettingsDB.CREATE_TABLE);
    }

    public void dropTables(SQLiteDatabase db){
        db.execSQL(ClsConfigurationSettingsDB.DROP_TABLE);
    }
    
}
