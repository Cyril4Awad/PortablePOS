package com.example.erpdatacollectorn;

import java.util.ArrayList;
import java.util.List;

import org.ksoap2.SoapEnvelope;
import org.ksoap2.serialization.PropertyInfo;
import org.ksoap2.serialization.SoapObject;
import org.ksoap2.serialization.SoapPrimitive;
import org.ksoap2.serialization.SoapSerializationEnvelope;
import org.ksoap2.transport.HttpTransportSE;

import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Handler;
import android.os.Message;
import android.os.PowerManager;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.text.TextUtils;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.Chronometer;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;


public class Synchronize extends Activity {

	private static final int PullStatusNotStarted = 0;
	private static final int PullStatusNotCompleted = 1;
	private static final int PullStatusCompleted = 2;

	private static final int reqInitTables = 700;
	private static final int reqPullTable = 701;

	SQLiteDatabase db;
	ConfigurationSettings cs;
	final String NAMESPACE = "http://tempuri.org/";
	String URL = "http://0.0.0.0/ERPDataCollector/Service1.asmx";
	int CurPullStatus = PullStatusNotStarted;
	clsInitPullTables InitPullTable = new clsInitPullTables();

	boolean StopTransfer = false;

	TextView lblFormHeader;
	TextView lblSyncHeader;
	TextView lblSyncDetails;
	TextView lblSyncStatus;
	TextView lblDispMessage;
	Button btnStopDownload;
	Button btnClose;
	ProgressBar simpleProgressBar;
	Chronometer simpleChronometer;
	
	String FormOperation = "";
	
	protected PowerManager.WakeLock mWakeLock;
	String deviceInfo = "";
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_synchronize);
		deviceInfo = DeviceInformation.getDeviceInformation(this);
		/*
		 * This code together with the one in onDestroy() will make the screen
		 * be always on until this Activity gets destroyed.
		 */
		final PowerManager pm = (PowerManager) getSystemService(Context.POWER_SERVICE);
		this.mWakeLock = pm.newWakeLock(PowerManager.SCREEN_DIM_WAKE_LOCK,
				"My Tag");
		this.mWakeLock.acquire();

		simpleProgressBar = (ProgressBar) findViewById(R.id.progress1); // initiate
																		// the
																		// progress
																		// bar
		simpleProgressBar.setMax(100); // 100 maximum value for the progress bar
		lblDispMessage = (TextView) findViewById(R.id.lblDispMessage);
		lblDispMessage.setVisibility(View.GONE);
		lblFormHeader = (TextView) findViewById(R.id.lblFormHeader);
		lblSyncHeader = (TextView) findViewById(R.id.lblSyncHeader);
		lblSyncDetails = (TextView) findViewById(R.id.lblSyncDetails);
		lblSyncStatus = (TextView) findViewById(R.id.lblSyncStatus);
		btnClose= (Button) findViewById(R.id.btnClose);
		btnClose.setVisibility(View.GONE);
		btnClose.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View arg0) {
				finish();

			}
		});
		
		btnStopDownload = (Button) findViewById(R.id.btnStopDownload);
		btnStopDownload.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				StopTransfer = true;
				setFormHeader("Cancelling Operation");
				setDispHeader("Cancelling Synchronization");
				setDispDetail("Please Wait Operation To Complete");
				
			}
		});

		simpleChronometer = (Chronometer) findViewById(R.id.simpleChronometer); // initiate a chronometer

		
		
		cs = new ConfigurationSettings(getApplicationContext());
		if (UpdateURL() == true) {
			InitDB();
			simpleChronometer.start(); // start a chronometer
			Bundle extras = getIntent().getExtras();
			if (extras != null) {
				String Operation = extras.getString("Operation");
				FormOperation = Operation;
				if (Operation.equals("ContinueDownload") == true) {
					lblFormHeader.setText("Downloading...");
					if (initNextTablePull()) {
						StartPullRecord();
					} else {
						
						finishOperation("Download Finished",
								"Download Finished Successfully!",true);
						setFormHeader("Download Finished.");
						setDispHeader("Download Finished.");
						setDispDetail("Download Completed Successfully...");
						// setDispText("");
						// finish();
					}
				} else if (Operation.equals("StartDownload") == true) {
					lblFormHeader.setText("Downloading...");
					StartPullInitTransferTables();
				}
				else if(Operation.equals("Upload")==true){
					lblFormHeader.setText("Uploading...");
					StartUpload();
				}
			}

		} else {
			// finish();

		}

	}
	
	String FOTitle = "";
	String FOMsg = "";
	boolean FOOperationSuccess = false;
	
	void finishOperation(String Title,String Msg,boolean _OperationSuccess) {
		FOTitle = Title;
		FOMsg  =Msg;
		FOOperationSuccess = _OperationSuccess;
		
		runOnUiThread(new Runnable() {
		      public void run() {
		    	  finishOperation2(FOTitle,FOMsg,FOOperationSuccess);
		      }
		 });
		
	}
	

	void finishOperation2(String Title,String Msg,boolean _OperationSuccess){
		simpleChronometer.stop();
		btnStopDownload.setVisibility(View.GONE);
		btnClose.setVisibility(View.VISIBLE);
		lblDispMessage.setVisibility(View.VISIBLE);
		lblFormHeader.setText(Title);
		lblDispMessage.setText(Msg);
		
		lblSyncHeader.setBackgroundColor(Color.GRAY);
		lblSyncDetails.setBackgroundColor(Color.GRAY);
		lblSyncStatus.setBackgroundColor(Color.GRAY);
		 
		if(FormOperation.equals("Upload")==false){
			if(_OperationSuccess){
				
				try {
					//db.execSQL("DROP TABLE IF EXISTS tblHistory");
					//String sqlquery = "CREATE TABLE IF NOT EXISTS tblHistory(_id INTEGER PRIMARY KEY AUTOINCREMENT,LocalID INTEGER,stkReference VARCHAR,stkDesc VARCHAR,stkRecID VARCHAR,Qty VARCHAR,Method VARCHAR,DateTimeStmp VARCHAR,barcode VARCHAR)";
					//db.execSQL(sqlquery);
				} catch (SQLException e) { 
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				
				if (isTableExists("phyHeader")) {
					Cursor c = null;
					try {
						String sql = "SELECT SP_DecNo,SP_Fashion,SP_Expiry FROM phyHeader";
						c = db.rawQuery(sql, null);
						if (c.moveToFirst()) {
							cs.savesettings3(c.getString(0), c.getString(1), c.getString(2));
						}
					} catch (Exception e) {
						finishOperation("Error", e.getMessage(),false);
					} finally {
						if (c != null) {
							if (!c.isClosed()) {
								c.close();
							}
						}
					}
				} else {
					
				}
			}
			
		}
		else{
			if(_OperationSuccess){
				if (isTableExists("phyHeader")) {
					Cursor c = null;
					try {
						String sql = "SELECT SP_DecNo,SP_Fashion,SP_Expiry FROM phyHeader";
						c = db.rawQuery(sql, null);
						if (c.moveToFirst()) {
							cs.savesettings3(c.getString(0), c.getString(1), c.getString(2));
						}
					} catch (Exception e) {
						finishOperation("Error", e.getMessage(),false);
					} finally {
						if (c != null) {
							if (!c.isClosed()) {
								c.close();
							}
						}
					}
				} else {
					
				}
				
				//cs.saveSelectedRecId("");
			}
		}
	
		
	}
	
	void setProgress(String cProgress, String cMax) {
		try {
			int rprog = 0;
			int rmax = 100;
			if ((cProgress.equals("") == false) && (cMax.equals("") == false)) {
				if (TextUtils.isDigitsOnly(cProgress)
						&& TextUtils.isDigitsOnly(cMax)) {
					rprog = Integer.valueOf(cProgress);
					rmax = Integer.valueOf(cMax);
					if (rprog > rmax) {
						rprog = rmax;
					}
				}
			}

			if(rmax>0){
				double res = ((rprog * 100) / rmax);
				rprog = (int) res;
				rmax = 100;
				simpleProgressBar.setMax(rmax);
				simpleProgressBar.setProgress(rprog);

				setDispText(String.valueOf(rprog) + " %");
			}
			else{
			
				simpleProgressBar.setMax(0);
				simpleProgressBar.setProgress(0);

				setDispText("");
			}
			
		} catch (NumberFormatException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			finishOperation("Error", e.getMessage(),false);
		}

	}

	void setFormHeader(String txt) {
		lblFormHeader.setText(txt);
	}

	void setDispHeader(String txt) {
		lblSyncHeader.setText(txt);
	}

	void setDispDetail(String txt) {
		lblSyncDetails.setText(txt);
	}

	void setDispText(String txt) {
		lblSyncStatus.setText(txt);
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
			finishOperation("Error", e.getMessage(),false);
		}
		return isExist;
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
			finishOperation("Error", e.getMessage(),false);
		}
	}

	boolean DropCurrentTables() {
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

								String SQLITE_TABLE = c2
										.getString(c2
												.getColumnIndexOrThrow("OriginalTableName"));

								db.execSQL("DROP TABLE IF EXISTS "
										+ SQLITE_TABLE);

								sqlquery = "delete from tblPullStatus where _id = "
										+ c2.getString(c2
												.getColumnIndexOrThrow("_id"));
								db.execSQL(sqlquery);

							} while (c2.moveToNext());
						}
					}
					c2.close();

				}
			}
		} catch (SQLException e) {
			opres = false;
			finishOperation("Error", e.getMessage(),false);
		} finally {
			// db.close();
		}
		return opres;
	}

	void StartPullInitTransferTables() {
		new CountDownTimer(300, 300) { // adjust the milli seconds here
			public void onTick(long millisUntilFinished) {
			}

			public void onFinish() {

				// Toast toast = Toast.makeText(getApplicationContext(),
				// "Initializing Tables...", Toast.LENGTH_SHORT);
				// toast.show();
				try {
					setFormHeader("Downloading.");
					setDispHeader("Initializing Tables.");
					setDispDetail("Please Wait...");
					// setDispText("");
					if (DropCurrentTables()) {
						new Handler().postDelayed(new Runnable() {
							@Override
							public void run() {
								PullInitTransferTables();
							}
						}, 100);
					}
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					finishOperation("Error", e.getMessage(),false);
				}
			}
		}.start();
	}

	void PullInitTransferTables() {

		new Handler().postDelayed(new Runnable() {
			@Override
			public void run() {
				try {
					List<SoapRequestProperty> SRPHeader = new ArrayList<SoapRequestProperty>();

					SRPHeader.add(new SoapRequestProperty("SelectedLocation",
							cs.LocCode));
					SRPHeader.add(new SoapRequestProperty("MachineName",
							cs.MachineName));
					SRPHeader.add(new SoapRequestProperty("ApplicationName",
							"ERPDATACOLLECTOR"));
					SRPHeader.add(new SoapRequestProperty("Extras",
							cs.SelectedRecId.trim()));
					SRPHeader.add(new SoapRequestProperty("DeviceInfo",
							deviceInfo));
					String pullResult = PullTable("InitPullTables",
							"mTableInfo", "tblPullStatus", true, "", SRPHeader);

					OnPullFinish(reqInitTables, pullResult);
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					finishOperation("Error", e.getMessage(),false);
				}
			}
		}, 100);
	}

	boolean initNextTablePull() {
		boolean retres = false;
		Cursor c = null;
		try {
			InitPullTable = new clsInitPullTables();
			String sql = "SELECT * FROM tblPullStatus where PullFinished = '0'"
					+ " AND MachineName = '" + cs.MachineName + "'"
					+ " AND SelectedLocation = '" + cs.LocCode + "'"
					+ " Order By _id";
			c = db.rawQuery(sql, null);
			if (c.moveToFirst()) {
				InitPullTable = new clsInitPullTables(c);
				sql = InitPullTable.CreateTable;
				db.execSQL(sql);
				sql = "delete from " + InitPullTable.OriginalTableName
						+ " where RowNumber > " + InitPullTable.LastImportedRow;
				db.execSQL(sql);
				retres = true;
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			finishOperation("Error", e.getMessage(),false);
		} finally {
			if (c != null) {
				if (!c.isClosed()) {
					c.close();
				}
			}

		}
		return retres;

	}

	void OnPullFinish(int requestCode, String RetMsg) {

		try {
			setProgress(InitPullTable.LastImportedRow, InitPullTable.RowsCount);

			if (StopTransfer == false) {
				switch (requestCode) {
				case reqInitTables:
					if (RetMsg.equals("ok")) {
						if (initNextTablePull()) {
							StartPullRecord();
						} else {
						
							finishOperation("Download Completed",
									"Download Completed Successfully...",true);

							setFormHeader("Download Completed.");
							setDispHeader("Download Completed.");
							setDispDetail("Download Completed Successfully...");
							// setDispText("");

							// finish();
						}
					} else if (RetMsg.equals("no results")) {
						finishOperation("Error While Downloading ",
								"No Data Found.",false);
					} else {
						finishOperation("Error While Downloading ", RetMsg,false);
					}
					break;
				case reqPullTable:
					if (RetMsg.equals("ok")) {
						// continue to request until no results
						if (initNextTablePull()) {
							StartPullRecord();
						} else {
							
							finishOperation("Download Completed",
									"Download Completed Successfully...",true);

							setFormHeader("Download Completed.");
							setDispHeader("Download Completed.");
							setDispDetail("Download Completed Successfully...");
							// setDispText("");
							// finish();
						}

					} else if (RetMsg.equals("no results")) {
						if (initNextTablePull()) {
							StartPullRecord();
						} else {
							
							finishOperation("Download Completed",
									"Download Completed Successfully...",true);

							setFormHeader("Download Completed.");
							setDispHeader("Download Completed.");
							setDispDetail("Download Completed Successfully...");
							setDispText("");
							// finish();
						}
					} else {
						finishOperation("Error While Downloading ", RetMsg,false);
					}
					break;
				}
			} else {
				setFormHeader("Operation Cancelled.");
				setDispHeader("Operation Cancelled.");
				setDispDetail("You Can Restart Operation From Main Form");
				setDispText("");
				finishOperation("Synchronize Cancelled",
						"Synchronize Cancelled By User",false);
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			finishOperation("Error", e.getMessage(),false);
		}

	}

	void StartPullRecord() {
		new CountDownTimer(300, 300) { // adjust the milli seconds here
			public void onTick(long millisUntilFinished) {
			}

			public void onFinish() {

				try {
					String mDisplay = "";
					StringBuilder builder = new StringBuilder();
					// builder.append(InitPullTable.PullDisplayHeader);
					// builder.append(System.getProperty("line.separator"));
					builder.append("Starting At Row ");
					builder.append(String.valueOf(Integer
							.valueOf(InitPullTable.LastImportedRow) + 1));
					builder.append("/");
					builder.append(InitPullTable.RowsCount);
					mDisplay = builder.toString();

					setFormHeader("Downloading.");
					setDispHeader(InitPullTable.PullDisplayHeader);
					setDispDetail(mDisplay);
					// setDispText("Record " +
					// String.valueOf(Integer.valueOf(InitPullTable.LastImportedRow)
					// + 1) + "/" + InitPullTable.RowsCount);

					setProgress(InitPullTable.LastImportedRow,
							InitPullTable.RowsCount);
					if (Integer.valueOf(InitPullTable.LastImportedRow) >= Integer
							.valueOf(InitPullTable.RowsCount)) {
						mDisplay = "Checking Data";
						setDispDetail(mDisplay);
						// setDispText("");

					}
				} catch (NumberFormatException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					finishOperation("Error", e.getMessage(),false);
				}

				// Toast toast = Toast.makeText(getApplicationContext(),
				// mDisplay,
				// Toast.LENGTH_SHORT);
				// toast.show();

				// setDispText(mDisplay);
				new Handler().postDelayed(new Runnable() {
					@Override
					public void run() {
						PullRecords();
					}
				}, 100);
			}
		}.start();
	}

	private String thResult = "";

	public Handler handler = new Handler(new Handler.Callback() {

		@Override
		public boolean handleMessage(Message msg) {
			switch (msg.what) {

			case 0:
				OnPullFinish(reqPullTable, thResult);
				break;
			case 1:
				finishOperation("Error", thResult,false);
				break;
			case 2:
				setFormHeader("Operation Cancelled.");
				setDispHeader("Operation Cancelled.");
				setDispDetail("You Can Restart Operation From Main Form");
				setDispText("");
				finishOperation("Synchronize Cancelled",
						"Synchronize Cancelled By User",false);
				break;
			case 3:
				String mDisplay = "";
				StringBuilder builder = new StringBuilder();
				// builder.append(InitPullTable.PullDisplayHeader);
				// builder.append(System.getProperty("line.separator"));
				builder.append("Starting At Row ");
				builder.append(String.valueOf(CurUploadedCount));
				builder.append("/");
				builder.append(String.valueOf(ItemsToUpload));
				mDisplay = builder.toString();

				setFormHeader("Uploading.");
				setDispHeader("Uploading Items");
				setDispDetail(mDisplay);
				
				setProgress(String.valueOf(CurUploadedCount) , String.valueOf(ItemsToUpload));
				
				break;
			case 4:
				StartUploadHistory();
				break;
			case 5:
				finishOperation("Sending Finished.", "Sending Finished",true);
				setFormHeader("Sending Finished.");
				setDispHeader("");
				setDispDetail("");
				break;
				
				
			}
			return false;
		}

	});

	void PullRecords() {
		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					List<SoapRequestProperty> SRPHeader = new ArrayList<SoapRequestProperty>();
					// Adjust Request Parameters
					SRPHeader.add(new SoapRequestProperty("TableName",
							InitPullTable.TableName));
					SRPHeader.add(new SoapRequestProperty("PIndex", "0"));
					SRPHeader.add(new SoapRequestProperty("PSize", String
							.valueOf(cs.DownloadPageSize)));
					SRPHeader.add(new SoapRequestProperty(
							"StartFromRow",
							String.valueOf(Integer
									.valueOf(InitPullTable.LastImportedRow) + 1)));

					String pullResult = NewPullTable("pullRecords", "mRecords",
							InitPullTable.OriginalTableName, SRPHeader);

					UpdateLastPullRecord();

					thResult = pullResult;
					// OnPullFinish(reqPullTable, pullResult);
					handler.sendEmptyMessage(0);
				} catch (NumberFormatException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					thResult = e.getMessage();
					handler.sendEmptyMessage(1);
					// showMessage("Error", e.getMessage());
				}
			}
		}).start();

	}

	void UpdateLastPullRecord() {
		String sql = "";
		if (isTableExists(InitPullTable.OriginalTableName)) {
			Cursor c = null;
			try {
				sql = "SELECT count(*) FROM " + InitPullTable.OriginalTableName;
				c = db.rawQuery(sql, null);
				if (c.moveToFirst()) {
					InitPullTable.LastImportedRow = c.getString(0);
					if (Integer.valueOf(InitPullTable.LastImportedRow) >= Integer
							.valueOf(InitPullTable.RowsCount)) {
						InitPullTable.PullFinished = "1";
					}
				}
			} catch (Exception e) {
				finishOperation("Error", e.getMessage(),false);
			} finally {
				if (c != null) {
					if (!c.isClosed()) {
						c.close();
					}
				}
			}
		} else {
			InitPullTable.LastImportedRow = "0";
			InitPullTable.PullFinished = "0";
		}
		sql = "update tblPullStatus set LastImportedRow = '"
				+ InitPullTable.LastImportedRow + "'" + ",PullFinished = '"
				+ InitPullTable.PullFinished + "'" + " where _id = "
				+ String.valueOf(InitPullTable.id);
		db.execSQL(sql);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.synchronize, menu);
		return true;
	}

//	public void showMessage(String title, String message) {
//		Builder builder = new Builder(this);
//		builder.setCancelable(true);
//		builder.setTitle(title);
//		builder.setMessage(message);
//		builder.show();
//	}


	//push
	
	int ItemsToUpload = 0;
	int CurUploadedCount = 0;
	String MachinePositionToSend = "0";
	//private String mInputBox_Text = "";
	
	void StartUpload() {

		String nbrUpdated = "0";

		Cursor c = db.rawQuery("SELECT Count(*) from phyDetails where updated = '1'", null);
		// looping through all rows and adding to list
		if (c.moveToFirst()) {
			nbrUpdated = c.getString(0);
			ItemsToUpload = Integer.valueOf(nbrUpdated);
		}
		c.close();

		if (nbrUpdated.equals("0")) {
			finishOperation("Warning", "No Counted Items Found.",false);
		} else {
			askBeforePush(nbrUpdated);
		}
	}
	
	void askBeforePush(String nbrUpdated) {

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Upload Physical Count");
		StringBuilder DispMsg = new StringBuilder();
		DispMsg.append("This Physical Count Contains " + nbrUpdated + " Items");
		DispMsg.append(System.getProperty("line.separator"));
		DispMsg.append("Do You Want To Upload Current Physical Count?");
		builder.setMessage(DispMsg.toString());

		builder.setPositiveButton("YES", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				dialog.dismiss();

				
				delaidUpload();
				
			}

		});

		builder.setNegativeButton("NO", new DialogInterface.OnClickListener() {

			@Override
			public void onClick(DialogInterface dialog, int which) {
				// I do not need any action here you might
				dialog.dismiss();
				finish();
				// Status.setText("");
			}
		});

		AlertDialog alert = builder.create();
		alert.show();
	}



	void delaidUpload() {
		new CountDownTimer(300, 300) { // adjust the milli seconds here
			public void onTick(long millisUntilFinished) {
			}

			public void onFinish() {

				StartuploadDetails();
			}
		}.start();
	}

	

	void StartuploadDetails() {
		Toast toast = Toast.makeText(getApplicationContext(), "Sending "
				+ String.valueOf(ItemsToUpload)
				+ " Items. Please Wait...", Toast.LENGTH_LONG);
		toast.show();

		new Handler().postDelayed(new Runnable() {
			@Override
			public void run() {
				UploadDetails();
				
			}
		}, 100);
	}

	void UploadDetails() {
		new Thread(new Runnable() {

			@Override
			public void run() {
				
				try {
					
					boolean pSuccess = true;
					String uInvID = "";
			    	String uUserToUpdate = "";
			    	 Cursor c = db.rawQuery("Select InvID,UserToUpdate from phyHeader", null);
			    	 if (c.moveToFirst()) {
			    		 uInvID = c.getString(0);
			    		 uUserToUpdate = c.getString(1);
			    	 }
			    	 c.close();
			    	 
			    	 
			    	
					Cursor cursor = db.rawQuery("SELECT _id , Pcd_RecId,usercounted from phyDetails where updated = '1'", null);
					// looping through all rows and adding to list
					if (cursor.moveToFirst()) {
						do {

							if ((StopTransfer == false) && (pSuccess==true)) {
								CurUploadedCount = CurUploadedCount + 1; 
								
								handler.sendEmptyMessage(3);
								
				            	String id = cursor.getString(0);
				            	String Pcd_RecId = cursor.getString(1);
				            	String usercounted = cursor.getString(2);
				            	

				            	pSuccess=PushItemDetail(String.valueOf(CurUploadedCount),uInvID,id, Pcd_RecId,usercounted,uUserToUpdate);
				            	
								
							}

						} while ((cursor.moveToNext())&& (pSuccess==true)&&(StopTransfer == false));
					}
					cursor.close();

					if(pSuccess){
						if(StopTransfer){
							handler.sendEmptyMessage(2);
						}
						else{
							handler.sendEmptyMessage(4);
						}
					}
					else{
						
					}
					
				} catch (Exception e1) { 
					// TODO Auto-generated catch block
					e1.printStackTrace();
					thResult = e1.getMessage();
					handler.sendEmptyMessage(1);
				}

			}
		}).start();

	}

	boolean PushItemDetail(String transCount, String uInvID,String Id,String Pcd_RecId,String usercounted,String uUserToUpdate) {
		boolean opres = false;

		try {
			String pullResult = "";

			List<SoapRequestProperty> SRPHeader = new ArrayList<SoapRequestProperty>();
			
			SRPHeader.add(new SoapRequestProperty("transCount", transCount));
			SRPHeader.add(new SoapRequestProperty("SelectedLocation", cs.LocCode));
			SRPHeader.add(new SoapRequestProperty("uInvID", uInvID));
			SRPHeader.add(new SoapRequestProperty("id", Id));
			SRPHeader.add(new SoapRequestProperty("Pcd_RecId", Pcd_RecId));
			SRPHeader.add(new SoapRequestProperty("usercounted",usercounted ));
			SRPHeader.add(new SoapRequestProperty("uUserToUpdate", uUserToUpdate));


            
			pullResult = PullServerString("InsertDetail", SRPHeader);

			if (pullResult.equals("ok")) {
				opres = true;
				db.execSQL("UPDATE phyDetails SET updated = '2' WHERE _id = "
						+ Integer.valueOf(Id));
				opres = true;
			} else {
				finishOperation("Error While Sending Server Request ", pullResult,false);
			}
		} catch (NumberFormatException e) { 
			// TODO Auto-generated catch block
			finishOperation("Error", e.getMessage(),false);
		} catch (SQLException e) { 
			// TODO Auto-generated catch block
			finishOperation("Error", e.getMessage(),false);
		}

		return opres;
	}
	
	
	void StartUploadHistory() {
		finishOperation("Sending Finished.", "Sending Finished",true);
		setFormHeader("Sending Finished.");
		setDispHeader("");
		setDispDetail("");
		
		/*String nbrUpdated = "0";

		Cursor c = db.rawQuery("SELECT Count(*) from tblHistory", null);
		// looping through all rows and adding to list
		if (c.moveToFirst()) {
			nbrUpdated = c.getString(0);
			ItemsToUpload = Integer.valueOf(nbrUpdated);
			CurUploadedCount = 0;
			setFormHeader("Uploading History.");
			setDispHeader("Uploading History");
		}
		c.close();

		if (nbrUpdated.equals("0")) {
			finishOperation("Sending Finished.", "Sending Finished",true);
			setFormHeader("Sending Finished.");
			setDispHeader("");
			setDispDetail("");
		} else {
			HistoryUpload();
		}*/
	}
	
	
	
	//end of push	
	
	
	String NewPullTable(String soapobjectname, String RootProperty,
			String TableName, List<SoapRequestProperty> SRP) {
		String opres = "";
		try {
			SoapObject request = new SoapObject(NAMESPACE, soapobjectname);
			for (SoapRequestProperty sSRP : SRP) {
				request.addProperty(sSRP.sName, sSRP.sVal);
			}

			SoapSerializationEnvelope envelope = new SoapSerializationEnvelope(
					SoapEnvelope.VER11);
			envelope.dotNet = true;
			envelope.setOutputSoapObject(request);

			HttpTransportSE androidHttpTransport = new HttpTransportSE(URL,
					30000);

			androidHttpTransport.call("http://tempuri.org/" + soapobjectname,
					envelope);

			// SoapObject response = (SoapObject)envelope.getResponse();

			// Get the SoapResult from the envelope body.
			SoapObject resultRequestSOAP = (SoapObject) envelope.bodyIn;

			SoapObject root = (SoapObject) resultRequestSOAP.getProperty(0);
			SoapObject s_deals = (SoapObject) root.getProperty(RootProperty);

			db.beginTransaction();
			
			for (int i = 0; i < s_deals.getPropertyCount(); i++) {
				Object property = s_deals.getProperty(i);
				if (property instanceof SoapObject) {
					StringBuilder InsertValues = new StringBuilder();
					SoapObject i_list = (SoapObject) property;
					for (int j = 0; j < i_list.getPropertyCount(); j++) {
						PropertyInfo pi = new PropertyInfo();
						i_list.getPropertyInfo(j, pi);

						String iValue = "";
						if (i_list.getProperty(j).toString()
								.equals("anyType{}")) {

						} else {
							iValue = i_list.getProperty(j).toString();
							InsertValues.append(InitPullTable.InsertInto);
							InsertValues.append("(");
							InsertValues.append(iValue);
							InsertValues.append(")");
							db.execSQL(InsertValues.toString());
				
							InitPullTable.LastImportedRow = InitPullTable.LastImportedRow + 1;
						}
					}

					opres = "ok";
				}
			}

			 db.setTransactionSuccessful();
	         db.endTransaction();
			
		} catch (Exception e) {
			opres = "";
			if (e.getMessage().contains("illegal property: " + RootProperty)) {
				opres = "no results";
				// showMessage("Error While Downloading ","No Results For Request "
				// + soapobjectname);
			} else {
				opres = e.getMessage();
				// showMessage("Error While Downloading ",e.getMessage());
			}

			// showMessage("Error", e.getMessage());
			Log.d("Test", e.getMessage());
		}

		return opres;
	}

	String PullTable(String soapobjectname, String RootProperty,
			String TableName, boolean DropAndCreateTable, String ExtraColumn,
			List<SoapRequestProperty> SRP) {
		String opres = "";
		try {
			SoapObject request = new SoapObject(NAMESPACE, soapobjectname);
			for (SoapRequestProperty sSRP : SRP) {
				request.addProperty(sSRP.sName, sSRP.sVal);
			}

			SoapSerializationEnvelope envelope = new SoapSerializationEnvelope(
					SoapEnvelope.VER11);
			envelope.dotNet = true;
			envelope.setOutputSoapObject(request);

			HttpTransportSE androidHttpTransport = new HttpTransportSE(URL,
					30000);

			androidHttpTransport.call("http://tempuri.org/" + soapobjectname,
					envelope);

			// SoapObject response = (SoapObject)envelope.getResponse();

			// Get the SoapResult from the envelope body.
			SoapObject resultRequestSOAP = (SoapObject) envelope.bodyIn;

			SoapObject root = (SoapObject) resultRequestSOAP.getProperty(0);
			SoapObject s_deals = (SoapObject) root.getProperty(RootProperty);

			StringBuilder CreateTable = new StringBuilder();

			CreateTable.append("CREATE TABLE IF NOT EXISTS " + TableName
					+ "(_id INTEGER PRIMARY KEY AUTOINCREMENT, updated text,");
			if (ExtraColumn.equals("") == false) {
				CreateTable.append(ExtraColumn); // + " VARCHAR,");
			}

			boolean isCreated = false;

			for (int i = 0; i < s_deals.getPropertyCount(); i++) {
				Object property = s_deals.getProperty(i);
				if (property instanceof SoapObject) {

					StringBuilder InsertQuery = new StringBuilder();
					StringBuilder InsertValues = new StringBuilder();
					InsertQuery
							.append("Insert into " + TableName + "(updated,");
					InsertValues.append(" values('0',");

					SoapObject i_list = (SoapObject) property;
					for (int j = 0; j < i_list.getPropertyCount(); j++) {
						PropertyInfo pi = new PropertyInfo();
						i_list.getPropertyInfo(j, pi);

						InsertQuery.append(pi.name);

						String iValue = "";
						if (i_list.getProperty(j).toString()
								.equals("anyType{}")) {

						} else {
							iValue = i_list.getProperty(j).toString();
						}

						if (j < i_list.getPropertyCount() - 1) {
							InsertQuery.append(",");
							InsertValues.append("'" + iValue + "',");

						} else {
							InsertQuery.append(")");
							InsertValues.append("'" + iValue + "')");

						}

						if (isCreated == false) {
							CreateTable.append(pi.name + " VARCHAR");
							if (j < i_list.getPropertyCount() - 1) {
								CreateTable.append(",");
							} else {
								CreateTable.append(")");
							}
						}

					}

					if (isCreated == false) {
						if (DropAndCreateTable == true) {
							db.execSQL("DROP TABLE IF EXISTS " + TableName);
							db.execSQL(CreateTable.toString());
						}

						isCreated = true;
					}

					InsertQuery.append(InsertValues);
					db.execSQL(InsertQuery.toString());

					opres = "ok";
				}
			}

		} catch (Exception e) {
			opres = "";
			if (e.getMessage().contains("illegal property: " + RootProperty)) {
				opres = "no results";
				// showMessage("Error While Downloading ","No Results For Request "
				// + soapobjectname);
			} else {
				opres = e.getMessage();
				// showMessage("Error While Downloading ",e.getMessage());
			}

			// showMessage("Error", e.getMessage());
			Log.d("Test", e.getMessage());
		}

		return opres;
	}
	
	String PullServerString(String soapobjectname, List<SoapRequestProperty> SRP) {
		String opres = "";
		try {
			SoapObject request = new SoapObject(NAMESPACE, soapobjectname);
			for (SoapRequestProperty sSRP : SRP) {
				request.addProperty(sSRP.sName, sSRP.sVal);
			}

			SoapSerializationEnvelope envelope = new SoapSerializationEnvelope(
					SoapEnvelope.VER11);
			envelope.dotNet = true;
			envelope.setOutputSoapObject(request);

			HttpTransportSE androidHttpTransport = new HttpTransportSE(URL,
					30000);

			androidHttpTransport.call("http://tempuri.org/" + soapobjectname,
					envelope);

			SoapPrimitive response = (SoapPrimitive) envelope.getResponse();
			opres = response.toString();

		} catch (Exception e) {
			opres = "";
			opres = "Error:" + e.getMessage();

			// showMessage("Error", e.getMessage());
			Log.d("Test", e.getMessage());
		}

		return opres;
	}

	@Override
	public void onDestroy() {
		this.mWakeLock.release();
		super.onDestroy();
	}

}
