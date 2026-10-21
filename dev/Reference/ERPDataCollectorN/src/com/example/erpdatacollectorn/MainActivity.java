package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.os.StrictMode;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.text.InputType;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;


public class MainActivity extends Activity {

	// private TextView Status;

	private static final int ACTIVITY_SETTINGS = 500;
	private static final int ACTIVITY_BarcodeScan = 502;
	private static final int ACTIVITY_ListItems = 503;
	private static final int ACTIVITY_Download = 504;
	private static final int ACTIVITY_Upload = 504;
	


	private static final int PullStatusNotStarted = 0;
	private static final int PullStatusNotCompleted = 1;
	private static final int PullStatusCompleted = 2;

	int CurPullStatus = PullStatusNotStarted;
	clsInitPullTables InitPullTable = new clsInitPullTables();
	SQLiteDatabase db;
	final String NAMESPACE = "http://tempuri.org/";
	String URL = "http://0.0.0.0/ERPDataCollector/Service1.asmx";
	ConfigurationSettings cs;

	Button btnScan;
	Button btnDownload;
	Button btnUpload;
	Button btnViewItems;
	Button btnSettings;

	int nbrCalls = 0;
	int curCall = 0;

	@SuppressLint("NewApi")
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder()
				.permitAll().build();
		StrictMode.setThreadPolicy(policy);

		cs = new ConfigurationSettings(getApplicationContext());

		btnScan = (Button) findViewById(R.id.btnScan);
		btnScan.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				db.close();
				Intent intent = new Intent(MainActivity.this,
						BarcodeScan.class);
				//intent.putExtra("LocalID", "0");
				startActivityForResult(intent, ACTIVITY_BarcodeScan);
			}
		});

		btnViewItems = (Button) findViewById(R.id.btnViewItems);
		btnViewItems.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				db.close();
				
				Intent mintent = new Intent(getApplicationContext(), ViewOptions.class);
                //mintent.putExtra("whereCond", "where updated = '0'");
                startActivityForResult(mintent, ACTIVITY_ListItems);
			}
		});

		btnUpload = (Button) findViewById(R.id.btnUpload);
		btnUpload.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				db.close();
				Intent intent = new Intent(MainActivity.this, Synchronize.class);
				intent.putExtra("Operation", "Upload");
				startActivityForResult(intent, ACTIVITY_Upload);

			}
		});

		btnDownload = (Button) findViewById(R.id.btnDownload1);
		btnDownload.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {

				switch (CurPullStatus) {
				case PullStatusNotStarted:
					cs.saveSelectedRecId("");
					DownloadAction("StartDownload");
					//StartPullInitTransferTables();
					break;
				case PullStatusNotCompleted:
					askBeforeReinitPull();
					break;
				case PullStatusCompleted:
					askBeforeNewPull();
					break;
				}

			}
		});

		btnSettings = (Button) findViewById(R.id.btnSettings);
		btnSettings.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				db.close();
				Intent intent = new Intent(MainActivity.this, Settings.class);
				startActivityForResult(intent, ACTIVITY_SETTINGS);

			}
		});

		if (UpdateURL() == true) {
			InitDB();
			CheckTables();
		} else {
			Intent intent = new Intent(MainActivity.this, Settings.class);
			startActivityForResult(intent, ACTIVITY_SETTINGS);

		}

	}

	private String mInputBox_Text = "";
	void askForPhysicalCountNumber()
	{
		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Enter Physical Count Number");

		// Set up the input
		final EditText input = new EditText(this);
		// Specify the type of input expected; this, for example, sets the input as a password, and will mask the text
		input.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_CLASS_NUMBER);
		builder.setView(input);

		// Set up the buttons
		builder.setPositiveButton("OK", new DialogInterface.OnClickListener() { 
		    @Override
		    public void onClick(DialogInterface dialog, int which) {
		    	mInputBox_Text = input.getText().toString();
		    	cs.saveSelectedRecId(mInputBox_Text);
				DownloadAction("StartDownload");
		    }
		});
		builder.setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
		    @Override
		    public void onClick(DialogInterface dialog, int which) {
		        dialog.cancel();
		    }
		});

		builder.show();

	}
	
	
	void DownloadAction(String Operation){
		if(cs.SelectedRecId .equals("")){
			askForPhysicalCountNumber();
		}
		else{
			db.close();
			Intent intent = new Intent(MainActivity.this, Synchronize.class);
			intent.putExtra("Operation", Operation);
			startActivityForResult(intent, ACTIVITY_Download);
		}
	}
	
	void askBeforeNewPull() {

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Synchronizing Database");
		StringBuilder DispMsg = new StringBuilder();
		DispMsg.append("Do You Want To Delete Current Database And ReDownload?");
		
		builder.setMessage(DispMsg.toString());

		builder.setPositiveButton("Yes", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				dialog.dismiss();
				cs.saveSelectedRecId("");
				DownloadAction("StartDownload");

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
	
	void askBeforeReinitPull() {

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Synchronizing Database");
		StringBuilder DispMsg = new StringBuilder();
		switch (CurPullStatus) {
		case PullStatusNotCompleted:
			DispMsg.append("Download Already Started But Not Completed!");
			DispMsg.append(System.getProperty("line.separator"));
			DispMsg.append("Press Restart To Delete Already Downloaded Data and Restart Download");
			DispMsg.append(System.getProperty("line.separator"));
			DispMsg.append("Or Press Continue To Continue Download");
			break;
		case PullStatusCompleted:

			break;
		}
		
		builder.setMessage(DispMsg.toString());

		builder.setPositiveButton("Restart Download", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				dialog.dismiss();

				cs.saveSelectedRecId("");
				DownloadAction("StartDownload");

			}

		});

		builder.setNegativeButton("Continue Download", new DialogInterface.OnClickListener() {

			@Override
			public void onClick(DialogInterface dialog, int which) {
				// I do not need any action here you might
				dialog.dismiss();
				
				DownloadAction("ContinueDownload");

			}
		});

		AlertDialog alert = builder.create();
		alert.show();
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		switch (requestCode) {
		case ACTIVITY_SETTINGS:
			if (resultCode == Activity.RESULT_OK) {
				String FormRes = data.getStringExtra("Action").trim();
				if (FormRes.equals("Save")) {

				}
			} else {

			}

			break;

		}
		
		cs = new ConfigurationSettings(getApplicationContext());
		if (UpdateURL() == true) {
			InitDB();
			
			if(requestCode==ACTIVITY_Download){
				if(!isTableExists("phyDetails")){
					DropCurrentTables();
					showMessage("Warning:","Selected Inventory Not Download Or Does Not Exist!!!");
				}
			}
			
			// insertNewSettings();
			CheckTables();
		} else {
			finish();
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	private boolean UpdateURL() {
		// serviceUrl = cs.serviceUrl;
		if (cs.serviceUrl.trim().equals("")) {
			return false;
		} else {
			URL = "http://" + cs.serviceUrl.trim()
					+ "/ERPDataCollector/Service1.asmx";
			return true;
		}
	}

	void InitDB() {
		try {
			db = openOrCreateDatabase(cs.DBName, Context.MODE_PRIVATE, null);
		} catch (Exception e) {
			showMessage("Error", e.getMessage());
		}
	}

	void CheckTables() {
		boolean tablesexist = true;

		CurPullStatus = CheckPullStatus();
		if (CurPullStatus != PullStatusCompleted) {
			tablesexist = false;
		}

//		StringBuilder CreateTable = new StringBuilder();
//
//		CreateTable
//				.append("CREATE TABLE IF NOT EXISTS tbl_TransDetail(_id INTEGER PRIMARY KEY AUTOINCREMENT, RecID INTEGER,stkid text,ItemDesc text,CostLL text,CostFC text,Qty1 text,Qty2 text,Qty text)");
//		db.execSQL(CreateTable.toString());

		if (tablesexist == false) {
			btnScan.setEnabled(false);
			btnUpload.setEnabled(false);
			btnViewItems.setEnabled(false);
		} else {
			btnScan.setEnabled(true);
			btnUpload.setEnabled(true);
			btnViewItems.setEnabled(true);

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

	// new pull

	int CheckPullStatus() {
		String sqlquery = "";
		int PullStatus = PullStatusNotStarted;
		try {
			boolean isExist = false;
			// test if initpulltable exist
			sqlquery = "select DISTINCT tbl_name from sqlite_master where type='table' and tbl_name = 'tblPullStatus'";
			Cursor c = db.rawQuery(sqlquery, null);
			if (c != null) {
				if (c.getCount() > 0) {
					isExist = true;
				}
				c.close();
			}
			if (isExist) {
				// check tables of current location and machine name
				sqlquery = "select * from tblPullStatus where SelectedLocation = '"
						+ cs.LocCode
						+ "' And MachineName = '"
						+ cs.MachineName
						+ "'";
				Cursor c2 = db.rawQuery(sqlquery, null);
				if (c2 != null) {
					if (c2.getCount() > 0) {
						PullStatus = PullStatusNotCompleted;
						if (c2.moveToFirst()) {
							isExist = false;
							do {

								String PullFinished = c2.getString(c2
										.getColumnIndexOrThrow("PullFinished"));

							
								if (PullFinished.equals("0") == true) {
									//not completed
									isExist = true;
								}
								else{
									if(!isTableExists(c2.getString(c2
										.getColumnIndexOrThrow("OriginalTableName")))){
										//marked as completed but table not exist
										//should be reimported 
										sqlquery = "update tblPullStatus set PullFinished = '0', LastImportedRow = '0' where _id = "
												+ c2.getString(c2.getColumnIndexOrThrow("_id"));
										db.execSQL(sqlquery);		
										isExist = true;
									}
								}

							} while (c2.moveToNext());
							if (!isExist) {
								PullStatus = PullStatusCompleted;
							}
						}
					} else {

					}
					c2.close();
				}
			}
		} catch (SQLException e) {
			PullStatus = PullStatusNotStarted;
			showMessage("Error", e.getMessage());
		} finally {
			// db.close();
		}
		return PullStatus;
	}

	boolean DropCurrentTables(){
		boolean opres = true;
		String sqlquery = "";
		try {
			boolean isExist = false;
			// test if initpulltable exist
			sqlquery = "select DISTINCT tbl_name from sqlite_master where type='table' and tbl_name = 'tblPullStatus'";
			Cursor c = db.rawQuery(sqlquery, null);
			if (c != null) {
				if (c.getCount() > 0) {
					isExist = true;
				}
				c.close();
			}
			if (isExist) {
				// check tables of current location and machine name
				sqlquery = "select * from tblPullStatus where SelectedLocation = '"
						+ cs.LocCode
						+ "' And MachineName = '"
						+ cs.MachineName
						+ "'";
				Cursor c2 = db.rawQuery(sqlquery, null);
				if (c2 != null) {
					if (c2.getCount() > 0) {
						if (c2.moveToFirst()) {
							isExist = false;
							do {

								String SQLITE_TABLE = c2.getString(c2
										.getColumnIndexOrThrow("OriginalTableName"));
								
								db.execSQL("DROP TABLE IF EXISTS " + SQLITE_TABLE);
								
								sqlquery = "delete from tblPullStatus where _id = "
										+ c2.getString(c2.getColumnIndexOrThrow("_id"));
								db.execSQL(sqlquery);

							} while (c2.moveToNext());
						}
					} 
					c2.close();
					
				}
			}
		} catch (SQLException e) {
			opres = false;
			showMessage("Error", e.getMessage());
		} finally {
			// db.close();
		}
		return opres;
	}
	
	public void showMessage(String title, String message) {
		Builder builder = new Builder(this);
		builder.setCancelable(true);
		builder.setTitle(title);
		builder.setMessage(message);
		builder.show();
	}

}
