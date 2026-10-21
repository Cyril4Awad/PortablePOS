package com.example.erpdatacollectorn;

import android.content.Context;
import android.database.Cursor;
import android.graphics.Color;
import android.support.v4.widget.CursorAdapter;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

public class ClsConfigurationSettingsCursorAdapter extends CursorAdapter {

	public ClsConfigurationSettingsCursorAdapter(Context context, Cursor cursor, int flags) {
		super(context, cursor, 0);
	}
	
	  // The newView method is used to inflate a new view and return it, 
	  // you don't bind any data to the view at this point. 
	  @Override
	  public View newView(Context context, Cursor cursor, ViewGroup parent) {
	      return LayoutInflater.from(context).inflate(R.layout.settings_item, parent, false);
	  }

	  @Override
	  public void bindView(View view, Context context, Cursor cursor) {
		  
		  TextView lblLocCode= (TextView) view.findViewById(R.id.lblLocCode);
		  TextView lblserviceUrl= (TextView) view.findViewById(R.id.lblserviceUrl);
		  TextView lblSelected= (TextView) view.findViewById(R.id.lblSelected);
		  
	      String LocCode = cursor.getString(cursor.getColumnIndexOrThrow("LocCode"));
	      String serviceUrl  = cursor.getString(cursor.getColumnIndexOrThrow("serviceUrl"));
	      boolean Selected = (cursor.getInt(cursor.getColumnIndex(ClsConfigurationSettingsDB.COL_Selected_Name))>0)?true:false;
	      
	      lblLocCode.setText(LocCode);
	      lblserviceUrl.setText(serviceUrl);
	      if(Selected) {
	    	  lblSelected.setText("True");
	      }  
	      else {
	    	  lblSelected.setText("False");
	      }
	      
	  }
}
