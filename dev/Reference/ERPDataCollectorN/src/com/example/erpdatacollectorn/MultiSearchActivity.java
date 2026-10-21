package com.example.erpdatacollectorn;

import java.util.ArrayList;
import java.util.List;

import android.os.Bundle;
import android.app.Activity;
import android.app.AlertDialog.Builder;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.text.Editable;
import android.text.TextUtils;
import android.text.TextWatcher;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.AdapterView.OnItemClickListener;

public class MultiSearchActivity extends Activity {

	ConfigurationSettings cs;
	SQLiteDatabase db;
	String whereCond = "";
	String OrderBy = "";
	String myTitle = "";
	TextView txtTitle;
	EditText txtsearch;
	
	String PAssedBar = "";
	
	String ParentActivity = "";
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_multi_search);
		
		cs = new ConfigurationSettings(getApplicationContext());
		
		InitDB();

		txtsearch = (EditText) findViewById(R.id.txtsearch);
		// Capture Text in EditText
		txtsearch.addTextChangedListener(new TextWatcher() {

					@Override
					public void afterTextChanged(Editable arg0) {
						// TODO Auto-generated method stub
						init();
					}

					@Override
					public void beforeTextChanged(CharSequence arg0, int arg1,
							int arg2, int arg3) {
						// TODO Auto-generated method stub
					}

					@Override
					public void onTextChanged(CharSequence arg0, int arg1, int arg2,
							int arg3) {
						// TODO Auto-generated method stub
					}
				});
		
		txtTitle = (TextView) findViewById(R.id.txtTitle);
		Bundle extras = getIntent().getExtras();
		if (extras != null) {
			ParentActivity = extras.getString("ParentActivity");
			whereCond = extras.getString("whereCond");
			OrderBy = extras.getString("OrderBy");
			myTitle = extras.getString("Title");
			PAssedBar = extras.getString("PAssedBar");
			txtsearch.setText(PAssedBar);
			txtTitle.setText(myTitle);
		}
		init();
		
	}

	void InitDB() {
        try {
			db=openOrCreateDatabase(cs.DBName, Context.MODE_PRIVATE, null);
        } catch (Exception e) {
			showMessage("Error", e.getMessage());
		}
	}

    public boolean isTableExists(String tableName) {
        boolean isExist = false;
        try {
    		Cursor cursor = db.rawQuery("select DISTINCT tbl_name from sqlite_master where type='table' and tbl_name = '" + tableName + "'", null);
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
    
    
	private String GetFilterQuery(String bar){
		String retval = "";
		try {
			
			StringBuilder SqlQuery = new StringBuilder();
			SqlQuery.append("SELECT * FROM phyDetails ");
			
			
			StringBuilder SqlQuery2 = new StringBuilder();
			SqlQuery2.append("SELECT Pcd_RecId FROM phyDetails where Refernce like '" + bar + "%'");
			List<String> Ids2 = new ArrayList<String>();
			
	    	if(isTableExists("phyDetails")==true){
				Cursor c2=db.rawQuery(SqlQuery2.toString(), null);
				if(c2.moveToFirst())
				{
		            do {
		            	Ids2.add(c2.getString(0));
		            } while (c2.moveToNext());
				}
				else{
				}
				c2.close();
    	    }
			
			String str = "";
			StringBuilder whereextra = new StringBuilder();
			if(Ids2.isEmpty()==false){
				whereextra.append(" Pcd_RecId in (");
	            for(String sIds2 : Ids2){
	            	whereextra.append("'");
	            	whereextra.append(sIds2);
	            	whereextra.append("',");
	            }
	            str = whereextra.toString();
	            str = str.substring(0, str.length() - 1);
	            str = str + ")";
			
			}

			retval = str;
			
		} catch (Exception e) {
			// TODO Auto-generated catch block
			retval = "";
		}
		return retval;
	}
	
	void init() {
		
		  String NewWhereCond = whereCond;
		  if(txtsearch.getText().toString().trim().equals("")){
			  //NewWhereCond = whereCond;
		  }
		  else{
			  String filterresultq = GetFilterQuery(txtsearch.getText().toString().trim());
			  if(filterresultq.equals("")==false){
				  if(NewWhereCond.trim().equals("")){
					  
					  NewWhereCond = " where " + filterresultq ;
				  }
				  else{
					  NewWhereCond = NewWhereCond + " and " + filterresultq;
				  }
			  }
			  else{
				  //no results should be displayed
				  NewWhereCond = " where 0=1";
			  }
		  }
		  
		
		Cursor Items = db.rawQuery("SELECT  * FROM phyDetails " + NewWhereCond + " " + OrderBy,
				null);

		final ListView lvItems = (ListView) findViewById(R.id.itemsMListView);
		
		ItemsCursorAdapter todoAdapter = new ItemsCursorAdapter(this, Items,0,cs.SP_DecNo,cs.SP_Fashion,cs.SP_Expiry);
		// Attach cursor adapter to the ListView 
		lvItems.setAdapter(todoAdapter);
		lvItems.setBackgroundColor(Color.WHITE);
		lvItems.setOnItemClickListener(new OnItemClickListener()
        {
				@Override
				public void onItemClick(AdapterView<?> arg0, View arg1,
						int position, long arg3) {
					 // Get the cursor, positioned to the corresponding row in the result set
				   Cursor cursor = (Cursor) lvItems.getItemAtPosition(position);

				   // Get the state's capital from this row in the database.
				   String phydetId = 
				   cursor.getString(cursor.getColumnIndexOrThrow("Pcd_RecId"));
				  
				   
				   	if(ParentActivity.equals("ViewOptions")){
					   	Intent mintent = new Intent(getApplicationContext(), BarcodeScan.class);
					   	mintent.putExtra("phydetId", phydetId);
		                startActivityForResult(mintent, 501);
				   	}
				   	else{
				    	Intent intent=new Intent();  
				        intent.putExtra("phydetId", phydetId);   
				        setResult(Activity.RESULT_OK,intent);  
				        finish();//finishing activity
				   	}
				   	
				}
        });
		
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
	    protected void onActivityResult(int requestCode, int resultCode, Intent data)  
	    {  
	            super.onActivityResult(requestCode, resultCode, data); 
	            init();

	    }
	    
	    
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.multi_search, menu);
		return true;
	}

}
