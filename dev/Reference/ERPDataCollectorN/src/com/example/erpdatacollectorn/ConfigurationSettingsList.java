package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.app.Activity;
import android.app.AlertDialog.Builder;
import android.content.Intent;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.AdapterView.OnItemClickListener;

public class ConfigurationSettingsList extends Activity {

	LinearLayout LManageSettings;
	Button btnManageSettings;
	
	String ParentActivity = "";
	
	SQLiteDatabase mdb;
	
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_configuration_settings_list);
		
		LManageSettings = (LinearLayout) findViewById(R.id.LManageSettings);
		btnManageSettings = (Button) findViewById(R.id.btnManageSettings);
		btnManageSettings.setOnClickListener(new Button.OnClickListener() {
			@Override
			public void onClick(View arg0) {
				Intent mintent = new Intent(getApplicationContext(),
						ServerSettingsActivity.class);
				mintent.putExtra("ParentActivity", "ConfigurationSettingsList");
				startActivityForResult(mintent, 501);
			}
		});
		
		Bundle extras = getIntent().getExtras();
		if (extras != null) {
			ParentActivity = extras.getString("ParentActivity");
	        // Set the LinearLayout and Button visibility to GONE
			LManageSettings.setVisibility(View.GONE); // Makes it invisible and removes it from the layout space
	        btnManageSettings.setVisibility(View.GONE); // Also makes it invisible and removes it from the layout space

	        // Alternatively, set the visibility to INVISIBLE if you want them hidden but retain their space
	        // lManageSettings.setVisibility(View.INVISIBLE);
	        // btnManageSettings.setVisibility(View.INVISIBLE);
		}
		
		InitMainDB();
		init();
	}

	void InitMainDB() {
		try {
			mdb = DBHelper.getInstance(getApplicationContext())
					.getWritableDatabase();
		} catch (Exception e) {
			showMessage("Error", e.getMessage());
		}
	}
	
	void init() {
		Cursor Items = mdb.rawQuery("SELECT * FROM tblConfigurationSettings", null);
		final ListView lvItems = (ListView) findViewById(R.id.settingsListView);
		ClsConfigurationSettingsCursorAdapter todoAdapter = new ClsConfigurationSettingsCursorAdapter(this, Items, 0);
		lvItems.setAdapter(todoAdapter);
		lvItems.setBackgroundColor(Color.WHITE);
		lvItems.setOnItemClickListener(new OnItemClickListener() {
			@Override
			public void onItemClick(AdapterView<?> arg0, View arg1,
					int position, long arg3) {
				// Get the cursor, positioned to the corresponding row in the
				// result set
				Cursor cursor = (Cursor) lvItems.getItemAtPosition(position);

				// Get the state's capital from this row in the database.
				String LocalID = cursor.getString(cursor
						.getColumnIndexOrThrow("_id"));
				
				
				
				if (ParentActivity.equals("")) {
					//we are selecting this settings to be the current
					try {
						mdb.execSQL("update tblConfigurationSettings set Selected = 0");
					} catch (SQLException e) { 
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					
					Cursor c = mdb.rawQuery(
							"SELECT * FROM tblConfigurationSettings Where _id = "
									+ Integer.valueOf(LocalID), null);
					if (c.moveToFirst()) {
						try {
							ClsConfigurationSettings cls = new ClsConfigurationSettings(c);
							
							cls.setSelected(true);

							// The row you want to update, assuming `rowId` is
							// the _id of the row to update:
							String selection = "_id = ?";
							// This will replace the '?' in the selection:
							String[] selectionArgs = { String.valueOf(LocalID) };

							// Perform the update:
							int count = mdb.update(
									ClsConfigurationSettingsDB.TABLE_Name, // The
																			// table
																			// to
																			// update
									cls.getContentValues(), // The columns and
															// new values
									selection, // The where clause
									selectionArgs // The where clause arguments
									);

							// 'count' holds the number of rows that were
							// updated.
							// if (count > 0) {
							// Log.d("Database", "Updated " + count + " rows");
							// } else {
							// Log.e("Database", "No rows were updated");
							// }

							ConfigurationSettings cs;
							cs = new ConfigurationSettings(getApplicationContext());
							cs.LocCode = cls.getLocCode();
							cs.DBName = "ERPDataCollectorNDB_" + cs.LocCode;
							cs.SelectedRecId = cls.getSelectedRecId();
							cs.serviceUrl = cls.getserviceUrl();
							cs.SP_DecNo = cls.getSP_DecNo();
							cs.SP_Expiry = cls.getSP_Expiry();
							cs.SP_Fashion = cls.getSP_Fashion();
							cs.savesettings4(
									cls.getserviceUrl(),
									cs.DBName,
									cs.LocCode,
									cls.getSelectedRecId(),
									cls.getSP_DecNo(),
									cls.getSP_Fashion(),
									cls.getSP_Expiry());
							
							
							
						} catch (SQLException e) {
							showMessage("Error", e.getMessage());
						}
					}
					c.close();
					
					
					
				}
				
				Intent intent=new Intent();  
		        intent.putExtra("LocalID", LocalID);   
		        setResult(Activity.RESULT_OK,intent);  
		        finish();//finishing activity
		        
			   	
			}
		});
	}
	
	
	public void showMessage(String title, String message) {
		Builder builder = new Builder(this);
		builder.setCancelable(true);
		builder.setTitle(title);
		builder.setMessage(message);
		builder.show();
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.configuration_settings_list, menu);
		return true;
	}

    @Override  
    protected void onActivityResult(int requestCode, int resultCode, Intent data)  
    {  
            super.onActivityResult(requestCode, resultCode, data); 
            init();

    }
}
