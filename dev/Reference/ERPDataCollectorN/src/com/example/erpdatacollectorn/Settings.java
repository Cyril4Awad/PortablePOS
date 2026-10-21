package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class Settings extends Activity {

	ConfigurationSettings cs;
	SQLiteDatabase db;
	SQLiteDatabase mdb;
	EditText txturl;
	EditText txtMachineName;
	EditText txtDownloadPageSize;
	//EditText txtLocCode;
	EditText txtLocation;

	Button btnSelectSettings;
	Button btnSave;
	Button btnRegenerateUpload;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_settings);

		cs = new ConfigurationSettings(getApplicationContext());
		btnRegenerateUpload = (Button)findViewById(R.id.btnRegenerateUpload);

		
		txturl = (EditText)findViewById(R.id.txturl);
		txtMachineName = (EditText)findViewById(R.id.txtMachineName);

		txtDownloadPageSize = (EditText)findViewById(R.id.txtDownloadPageSize);
		txtLocation = (EditText)findViewById(R.id.txtLocation);

		txturl.setEnabled(false);
		txtLocation.setEnabled(false);
		
		btnSelectSettings = (Button) findViewById(R.id.btnSelectSettings);
		btnSelectSettings.setOnClickListener(new Button.OnClickListener() {
			@Override
			public void onClick(View arg0) {
				Intent mintent = new Intent(getApplicationContext(),
						ConfigurationSettingsList.class);
				// mintent.putExtra("ParentActivity", "ServerSettings");
				startActivityForResult(mintent, 555);
			}
		});
		
		btnSave = (Button)findViewById(R.id.btnSave);
		
		btnSave.setOnClickListener(new Button.OnClickListener(){

			   @Override
			   public void onClick(View arg0) {
				   cs.LocCode = txtLocation.getText().toString().trim();
				   cs.DBName = "ERPDataCollectorNDB_" + cs.LocCode;
				   cs.savesettings2(txturl.getText().toString().trim(),
				    		txtMachineName.getText().toString().trim(),
				    		cs.DBName,
				    		cs.LocCode,cs.LocDesc,Integer.valueOf(txtDownloadPageSize.getText().toString().trim()));	
				   
					
			    	Toast.makeText(Settings.this,
			    			"Settings Saved",
			                Toast.LENGTH_LONG).show();  
			    	Intent intent=new Intent();  
			        intent.putExtra("Action","Save");   
			        setResult(Activity.RESULT_OK,intent);  
			        finish();//finishing activity
			   }});
		
		
		
		btnRegenerateUpload.setOnClickListener(new Button.OnClickListener(){

			   @Override
			   public void onClick(View arg0) {
				   askBefore();
			   }});
		
		if (UpdateURL() == true) {
			InitDB();
			InitMainDB();
			LoadSelectedSettings();
		} else {
			Intent mintent = new Intent(getApplicationContext(),
					ServerSettingsActivity.class);
			mintent.putExtra("ParentActivity", "Settings");
			startActivityForResult(mintent, 501);
		}
		
		updateform();
	}
	
	void askBefore() {

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Restoring Uploaded Items");
		StringBuilder DispMsg = new StringBuilder();
		DispMsg.append("Do You Want To Restore Uploaded Items?");
		
		builder.setMessage(DispMsg.toString());

		builder.setPositiveButton("Yes", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				dialog.dismiss();
				try {
					String sql = "UPDATE phyDetails SET updated = '1' WHERE updated = '2'";
					db.execSQL(sql);
					showMessage("Restoring Uploaded Items", "Operation Succeded Please Upload Items Again");
				} catch (SQLException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					showMessage("Restoring Uploaded Items", "Operation Failed");
				}
			}

		});

		builder.setNegativeButton("No", new DialogInterface.OnClickListener() {

			@Override
			public void onClick(DialogInterface dialog, int which) {
				// I do not need any action here you might
				dialog.dismiss();
			}
		});

		AlertDialog alert = builder.create();
		alert.show();
	}
	
	
	private boolean UpdateURL() {
		// serviceUrl = cs.serviceUrl;
		if (cs.serviceUrl.trim().equals("")) {
			btnRegenerateUpload.setEnabled(false);
			return false;
		} else {
			//URL = "http://" + cs.serviceUrl.trim()
			//		+ "/ERPDataCollector/Service1.asmx";
			return true;
		}
		
	
	}
	void InitDB() {
		try {
			db = openOrCreateDatabase(cs.DBName, Context.MODE_PRIVATE, null);
			
			if(!isTableExists("phyDetails")){
				btnRegenerateUpload.setEnabled(false);
			} else {
				btnRegenerateUpload.setEnabled(true);
			}
		} catch (Exception e) {
			//finishOperation("Error", e.getMessage(),false);
		}
		
		
	}
	
	void InitMainDB() {
		try {
			mdb = DBHelper.getInstance(getApplicationContext())
					.getWritableDatabase();
		} catch (Exception e) {
			showMessage("Error", e.getMessage());
		}
	}
	
	void LoadSelectedSettings() {
		try {
			Cursor c = mdb.rawQuery(
					"SELECT * FROM tblConfigurationSettings Where Selected = 1", null);
			if (c.moveToFirst()) {
				try {
					String LocalID = c.getString(c
							.getColumnIndexOrThrow("_id"));
					
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


					//ConfigurationSettings cs;
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
			} else {
				cs.saveEmpty();
			}
			c.close();
		
		} catch (Exception e) {
			showMessage("Error", e.getMessage());
		}
	}
	
	
	public boolean isTableExists(String tableName) {
		boolean isExist = false;
		try {
			Cursor cursor = db
					.rawQuery(
							"select DISTINCT tbl_name from sqlite_master where type='table' and tbl_name = '"
									+ tableName + "'", null);
			if (cursor != null) {
				if (cursor.getCount() > 0) {
					isExist = true;
				}
				cursor.close();
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			showMessage("Error", e.getMessage());
		}
		return isExist;
	}
	
	void updateform(){
	

		txturl.setText(cs.serviceUrl);
		txtMachineName.setText(String.valueOf(cs.MachineName));
		txtDownloadPageSize.setText(String.valueOf(cs.DownloadPageSize));
		txtLocation.setText(String.valueOf(cs.LocCode));

	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.settings, menu);
		return true;
	}


	  public void showMessage(String title,String message)
	    {
	    	Builder builder=new Builder(this);
	    	builder.setCancelable(true);
	    	builder.setTitle(title);
	    	builder.setMessage(message);
	    	builder.show();
	    	
		}
		@Override
		protected void onActivityResult(int requestCode, int resultCode, Intent data) {
			super.onActivityResult(requestCode, resultCode, data);
			cs = new ConfigurationSettings(getApplicationContext());
			if (UpdateURL() == true) {
				InitDB();
				InitMainDB();
				LoadSelectedSettings();
			}
			
			updateform();
			
			
		}
}
