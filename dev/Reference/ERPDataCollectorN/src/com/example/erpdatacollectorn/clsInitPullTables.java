package com.example.erpdatacollectorn;

import android.database.Cursor;

public class clsInitPullTables {

	public int id = 0;
	public String TableName = "";
	public String RowsCount = "";
	public String OriginalTableName = "";
	public String LastImportedRow = "";
	public String PullDisplayHeader = "";
	public String InsertInto = "";
	public String PullFinished = "";
	public String PullPreparedDT = "";
	public String SelectedLocation = "";
	public String MachineName = "";
	public String CreateTable = "";
	public String Notes = "";
	
	public clsInitPullTables(){
		id = 0;
		TableName = "";
		RowsCount = "";
		OriginalTableName = "";
		LastImportedRow = "";
		PullDisplayHeader = "";
		InsertInto = "";
		PullFinished = "";
		PullPreparedDT = "";
		SelectedLocation = "";
		MachineName = "";		
		CreateTable = "";
		Notes = "";
	}
	
	public clsInitPullTables(Cursor c){
		id = c.getInt(c.getColumnIndex("_id"));
		TableName = c.getString(c.getColumnIndex("TableName"));
		RowsCount = c.getString(c.getColumnIndex("RowsCount"));
		OriginalTableName = c.getString(c.getColumnIndex("OriginalTableName"));
		LastImportedRow = c.getString(c.getColumnIndex("LastImportedRow"));
		PullDisplayHeader = c.getString(c.getColumnIndex("PullDisplayHeader"));
		InsertInto = c.getString(c.getColumnIndex("InsertInto"));
		PullFinished = c.getString(c.getColumnIndex("PullFinished"));
		PullPreparedDT = c.getString(c.getColumnIndex("PullPreparedDT"));
		SelectedLocation = c.getString(c.getColumnIndex("SelectedLocation"));
		MachineName = c.getString(c.getColumnIndex("MachineName"));	
		CreateTable = c.getString(c.getColumnIndex("CreateTable"));
		Notes = c.getString(c.getColumnIndex("Notes"));
	}
}
