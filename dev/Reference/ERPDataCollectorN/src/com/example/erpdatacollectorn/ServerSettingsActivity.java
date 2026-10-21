package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.DialogInterface;
import android.content.Intent;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.Toast;

public class ServerSettingsActivity extends Activity {

	EditText txturl;
	EditText txtLocation;
	CheckBox chkIsSelected;
	Button btnSave;
	Button btnDeleteSettings;
	Button btnViewSettings;

	SQLiteDatabase mdb;
	ClsConfigurationSettings cls;

	String LocalID = "0";

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_server_settings);

		txturl = (EditText) findViewById(R.id.txturl);
		txtLocation = (EditText) findViewById(R.id.txtLocation);
		chkIsSelected = (CheckBox) findViewById(R.id.chkIsSelected);

		btnViewSettings = (Button) findViewById(R.id.btnViewSettings);
		btnViewSettings.setOnClickListener(new Button.OnClickListener() {
			@Override
			public void onClick(View arg0) {
				Intent mintent = new Intent(getApplicationContext(),
						ConfigurationSettingsList.class);
				mintent.putExtra("ParentActivity", "ServerSettings");
				startActivityForResult(mintent, 555);
			}
		});

		btnDeleteSettings = (Button) findViewById(R.id.btnDeleteSettings);
		btnDeleteSettings.setOnClickListener(new Button.OnClickListener() {
			@Override
			public void onClick(View arg0) {
				askBeforeDelete();
			}
		});
		
		btnSave = (Button) findViewById(R.id.btnSave);
		btnSave.setOnClickListener(new Button.OnClickListener() {

			@Override
			public void onClick(View arg0) {
				
				if(txtLocation.getText().toString().trim().equals("") ||
				   txturl.getText().toString().trim().equals("")) {
					showMessage("Warning", "Enter Valid Location and Server.");
					return;
				}
				
				
				
				if(chkIsSelected.isChecked()) {
					//update all server settings to not selected
					try {
						mdb.execSQL("update tblConfigurationSettings set Selected = 0");
					} catch (SQLException e) { 
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
				}
				
				if (LocalID.equals("0")) {
					cls = new ClsConfigurationSettings(txtLocation.getText()
							.toString().trim(), txturl.getText().toString()
							.trim(), chkIsSelected.isChecked());
					long rowid = mdb.insert(
							ClsConfigurationSettingsDB.TABLE_Name, null,
							cls.getContentValues());
					cls.set_id(rowid);
//					LocalID = cls.get_idStr();
//					init();

				} else {
					Cursor c = mdb.rawQuery(
							"SELECT * FROM tblConfigurationSettings Where _id = "
									+ Integer.valueOf(LocalID), null);
					if (c.moveToFirst()) {
						try {
							cls = new ClsConfigurationSettings(c);
							cls.setserviceUrl(txturl.getText().toString()
									.trim());
							cls.setLocCode(txtLocation.getText().toString()
									.trim());
							cls.setSelected(chkIsSelected.isChecked());

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

						} catch (SQLException e) {
							showMessage("Error", e.getMessage());
						}
					}
					c.close();

				}

				// cs.LocCode = txtLocation.getText().toString().trim();
				// cs.DBName = "SuperStoreInvDB_" + cs.LocCode;
				// cs.savesettings2(
				// txturl.getText().toString().trim(),
				// txtMachineName.getText().toString().trim(),
				// cs.DBName,
				// cs.LocCode,
				// cs.LocDesc,
				// chkContainsSearchForRef.isChecked(),
				// chkScanHistory.isChecked(),
				// chkUPCAPreamble.isChecked(),
				// Integer.valueOf(txtDownloadPageSize.getText()
				// .toString().trim()), chkcheckDigit.isChecked());

				Toast.makeText(ServerSettingsActivity.this, "Settings Saved",
						Toast.LENGTH_LONG).show();
				
				LocalID = "0";
				
				LoadSelectedSettings();
				init();
				
				// Intent intent=new Intent();
				// intent.putExtra("Action","Save");
				// setResult(Activity.RESULT_OK,intent);
				// finish();//finishing activity
			}
		});

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
		btnDeleteSettings.setEnabled(false);
		if (LocalID.equals("0")) {
			txturl.setText("");
			txtLocation.setText("");
			chkIsSelected.setChecked(false);
			btnDeleteSettings.setEnabled(false);
		} else {
			Cursor c = mdb.rawQuery(
					"SELECT * FROM tblConfigurationSettings Where _id = "
							+ Integer.valueOf(LocalID), null);
			if (c.moveToFirst()) {
				try {
					cls = new ClsConfigurationSettings(c);
					txturl.setText(cls.getserviceUrl());
					txtLocation.setText(cls.getLocCode());
					chkIsSelected.setChecked(cls.getSelected());
					btnDeleteSettings.setEnabled(true);

				} catch (SQLException e) {
					showMessage("Error", e.getMessage());
				}
			}
			c.close();
		}
	}

	void LoadSelectedSettings() {
		try {
			ConfigurationSettings cs;
			cs = new ConfigurationSettings(getApplicationContext());
			
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
	
	
	void askBeforeDelete() {

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Deleting Settings");
		StringBuilder DispMsg = new StringBuilder();
		DispMsg.append("Do You Want To Delete Current Settings?");
		builder.setMessage(DispMsg.toString());

		builder.setPositiveButton("YES", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				dialog.dismiss();
				mdb.execSQL("delete from tblConfigurationSettings WHERE _id = "
						+ LocalID);
				LocalID = "0";
				init();
				
			}

		});

		builder.setNegativeButton("NO", new DialogInterface.OnClickListener() {

			@Override
			public void onClick(DialogInterface dialog, int which) {
				// I do not need any action here you might
				dialog.dismiss();
				//finish();
				// Status.setText("");
			}
		});

		AlertDialog alert = builder.create();
		alert.show();
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
		getMenuInflater().inflate(R.menu.server_settings, menu);
		return true;
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		switch (requestCode) {
		case 555:
			if (resultCode == Activity.RESULT_OK) {
				String dID = data.getStringExtra("LocalID").trim();
				LocalID = dID;
				init();
			}

		}
	}

}
