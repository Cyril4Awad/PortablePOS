package com.example.erpdatacollectorn;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;

public class ConfigurationSettings {
	public static final String PREFS_NAME = "ERPDataCollectorNSettings";
    private SharedPreferences _sharedPrefs;
    private Editor _prefsEditor;

	public String serviceUrl;
	public String MachineName;
	public String LocCode;
	public String LocDesc;
	public String DBName;
	public int DownloadPageSize;
	
	
	public String SP_DecNo;
	public String SP_Fashion;
	public String SP_Expiry;
	public String SelectedRecId;
	
	private Context _context;
	
    public ConfigurationSettings(Context context) {
    	_context = context;
        this._sharedPrefs = context.getSharedPreferences(PREFS_NAME, Activity.MODE_PRIVATE);
        serviceUrl = _sharedPrefs.getString("serviceUrl", "");
        MachineName = _sharedPrefs.getString("MachineName", "");
        LocCode = _sharedPrefs.getString("LocCode", "");
        LocDesc = _sharedPrefs.getString("LocDesc", "");
        DBName = _sharedPrefs.getString("DBName", "ERPDataCollectorNDB");
        DownloadPageSize = _sharedPrefs.getInt("DownloadPageSize", 5000);
        
        SP_DecNo = _sharedPrefs.getString("SP_DecNo", "0");
        SP_Fashion = _sharedPrefs.getString("SP_Fashion", "N");
        SP_Expiry = _sharedPrefs.getString("SP_Expiry", "N");
        SelectedRecId = _sharedPrefs.getString("SelectedRecId", "");
    }


    public void loademptysettings(){
    	  serviceUrl = "";
          MachineName = "";
          //LocCode = _sharedPrefs.getString("LocCode", "");
          LocDesc = "";
          
          DBName = "";
          DownloadPageSize = 5000;
          SelectedRecId  ="";
    }


    public void savesettings2(String mserviceUrl, String mMachineName, String mDBName, String mLocCode, String mLocDesc, int mDownloadPageSize) {
    	this._prefsEditor = _sharedPrefs.edit();
        
     	  serviceUrl = mserviceUrl;
          MachineName = mMachineName;
          LocCode = mLocCode;
          LocDesc = mLocDesc;

          DBName = mDBName;
          DownloadPageSize = mDownloadPageSize;
          
    	_prefsEditor.putString("serviceUrl", mserviceUrl);
        _prefsEditor.putString("MachineName", mMachineName);
        
        _prefsEditor.putString("DBName", mDBName);
        _prefsEditor.putString("LocCode", mLocCode);
        _prefsEditor.putString("LocDesc", mLocDesc);
        _prefsEditor.putInt("DownloadPageSize", mDownloadPageSize);
        _prefsEditor.commit();
    }
    
    public void savesettings3( String mSP_DecNo, String mSP_Fashion, String mSP_Expiry) {
    	this._prefsEditor = _sharedPrefs.edit();
        SP_DecNo  = mSP_DecNo;
        SP_Fashion = mSP_Fashion;
        SP_Expiry = mSP_Expiry;
        _prefsEditor.putString("SP_DecNo", mSP_DecNo);
        _prefsEditor.putString("SP_Fashion", mSP_Fashion);
        _prefsEditor.putString("SP_Expiry", mSP_Expiry);
        _prefsEditor.commit();
        try {
        	SQLiteDatabase mdb;
        	mdb = DBHelper.getInstance(_context)
					.getWritableDatabase();
			mdb.execSQL("update tblConfigurationSettings set SP_DecNo = '" + SP_DecNo + "', SP_Fashion = '" + SP_Fashion + "', SP_Expiry = '" + SP_Expiry + "' where Selected = 1");
		} catch (SQLException e) { 
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        
    }
    
    public void savesettings4(String mserviceUrl, String mDBName, String mLocCode,String _SelectedRecId,String mSP_DecNo, String mSP_Fashion, String mSP_Expiry) {
    	this._prefsEditor = _sharedPrefs.edit();
        
     	  serviceUrl = mserviceUrl;
          LocCode = mLocCode;
          DBName = mDBName;
          SelectedRecId = _SelectedRecId;
          SP_DecNo  = mSP_DecNo;
          SP_Fashion = mSP_Fashion;
          SP_Expiry = mSP_Expiry;
          
    	_prefsEditor.putString("serviceUrl", mserviceUrl);
        _prefsEditor.putString("DBName", mDBName);
        _prefsEditor.putString("LocCode", mLocCode);
        _prefsEditor.putString("SelectedRecId", _SelectedRecId);
        _prefsEditor.putString("SP_DecNo", mSP_DecNo);
        _prefsEditor.putString("SP_Fashion", mSP_Fashion);
        _prefsEditor.putString("SP_Expiry", mSP_Expiry);
        _prefsEditor.commit();
    }
    
    public void saveEmpty() {
    	this._prefsEditor = _sharedPrefs.edit();
    	  serviceUrl = "";
          LocCode = "";
          _prefsEditor.putString("serviceUrl", "");
          _prefsEditor.putString("DBName", "");
          _prefsEditor.commit();
    }
    public void saveSelectedRecId(String _SelectedRecId){
    	this._prefsEditor = _sharedPrefs.edit();
        _prefsEditor.putString("SelectedRecId", _SelectedRecId);
        SelectedRecId = _SelectedRecId;
        _prefsEditor.commit();
        try {
        	SQLiteDatabase mdb;
        	mdb = DBHelper.getInstance(_context)
					.getWritableDatabase();
			mdb.execSQL("update tblConfigurationSettings set SelectedRecId = '" + _SelectedRecId + "' where Selected = 1");
		} catch (SQLException e) { 
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }
}
