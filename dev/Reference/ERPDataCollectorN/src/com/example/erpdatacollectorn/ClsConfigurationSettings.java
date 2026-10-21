package com.example.erpdatacollectorn;

import java.io.Serializable;
import android.database.Cursor;
import android.content.ContentValues;


public class ClsConfigurationSettings implements Serializable {
    private static final long serialVersionUID = 1L;
    //Class Variables

    private long _id;
    private String LocCode;
    private String serviceUrl;
    private String SelectedRecId;

    private String SP_DecNo;
    private String SP_Fashion;
    private String SP_Expiry;
	
    private boolean Selected;


    public ClsConfigurationSettings() {
        this._id = 0;
        this.LocCode = "";
        this.serviceUrl = "";
        this.SelectedRecId = "";
        
        this.SP_DecNo = "";
        this.SP_Fashion = "";
        this.SP_Expiry = "";
        this.Selected = false;

    }

    public ClsConfigurationSettings(String _LocCode,String _serviceUrl,String _SelectedRecId) {
        this.LocCode = _LocCode;
        this.serviceUrl = _serviceUrl;
        this.SelectedRecId = _SelectedRecId;
        this.Selected = false;
    }
    
    public ClsConfigurationSettings(String _LocCode,String _serviceUrl,String _SelectedRecId,boolean _Selected) {
        this.LocCode = _LocCode;
        this.serviceUrl = _serviceUrl;
        this.SelectedRecId = _SelectedRecId;
        this.Selected = _Selected;
    }

    public ClsConfigurationSettings(String _LocCode,String _serviceUrl,boolean _Selected) {
        this.LocCode = _LocCode;
        this.serviceUrl = _serviceUrl;
        this.Selected = _Selected;
    }

    public ClsConfigurationSettings(Cursor c) {
        this._id = c.getLong(c.getColumnIndex(ClsConfigurationSettingsDB.COL__id_Name));
        this.LocCode = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_LocCode_Name));
        this.serviceUrl = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_serviceUrl_Name));
        this.SelectedRecId = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_SelectedRecId_Name));
        
        this.SP_DecNo = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_SP_DecNo_Name));
        this.SP_Fashion = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_SP_Fashion_Name));
        this.SP_Expiry = c.getString(c.getColumnIndex(ClsConfigurationSettingsDB.COL_SP_Expiry_Name));
        
        this.Selected = (c.getInt(c.getColumnIndex(ClsConfigurationSettingsDB.COL_Selected_Name))>0)?true:false;
    }

    public void set_id(long rowid) {
        this._id = rowid;
    }

    public long get_id() {
        return this._id;
    }
    public String get_idStr() {
        return String.valueOf(this._id);
    }



    public void setLocCode(String _LocCode) {
        this.LocCode = _LocCode;
    }

    public String getLocCode() {
        return this.LocCode;
    }


    public void setserviceUrl(String _serviceUrl) {
        this.serviceUrl = _serviceUrl;
    }

    public String getserviceUrl() {
        return this.serviceUrl;
    }

    public void setSelectedRecId(String _SelectedRecId) {
        this.SelectedRecId = _SelectedRecId;
    }

    public String getSelectedRecId() {
        return this.SelectedRecId;
    }


    public void setSelected(boolean _Selected) {
        this.Selected = _Selected;
    }

    public boolean getSelected() {
        return this.Selected;
    }
    public String getSelectedStr() {
        return String.valueOf(this.Selected);
    }
    public int getSelectedInt() {
        return ((Selected)? 1 : 0);
    }

    public String getSP_DecNo() {
		return SP_DecNo;
	}

	public void setSP_DecNo(String sP_DecNo) {
		SP_DecNo = sP_DecNo;
	}

	public String getSP_Fashion() {
		return SP_Fashion;
	}

	public void setSP_Fashion(String sP_Fashion) {
		SP_Fashion = sP_Fashion;
	}

	public String getSP_Expiry() {
		return SP_Expiry;
	}

	public void setSP_Expiry(String sP_Expiry) {
		SP_Expiry = sP_Expiry;
	}

	public ContentValues getContentValues(){
        ContentValues values = new ContentValues();
        values.put(ClsConfigurationSettingsDB.COL_LocCode_Name, getLocCode());
        values.put(ClsConfigurationSettingsDB.COL_serviceUrl_Name, getserviceUrl());
        values.put(ClsConfigurationSettingsDB.COL_SelectedRecId_Name, getSelectedRecId());
        
        values.put(ClsConfigurationSettingsDB.COL_SP_DecNo_Name, getSP_DecNo());
        values.put(ClsConfigurationSettingsDB.COL_SP_Fashion_Name, getSP_Fashion());
        values.put(ClsConfigurationSettingsDB.COL_SP_Expiry_Name, getSP_Expiry());
        
        values.put(ClsConfigurationSettingsDB.COL_Selected_Name, getSelectedInt());
        return values;
    } 
}

