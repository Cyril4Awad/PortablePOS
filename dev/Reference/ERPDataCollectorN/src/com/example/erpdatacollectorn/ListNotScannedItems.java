package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.os.Environment;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.AdapterView.OnItemClickListener;

public class ListNotScannedItems extends Activity {

	SQLiteDatabase db;
	String whereCond = "";
	String myTitle = "";
	TextView txtTitle;
	ConfigurationSettings cs;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_list_not_scanned_items);
        
		
        cs = new ConfigurationSettings(getApplicationContext());
        InitDB();
        
		txtTitle = (TextView) findViewById(R.id.txtTitle);
		Bundle extras = getIntent().getExtras();
        if (extras != null) {
        	whereCond = extras.getString("whereCond");
//        	myTitle = extras.getString("Title");
//        	txtTitle.setText(myTitle);
        }
        init();
      
	}

	void init(){
		  Cursor Items = db.rawQuery("SELECT  * FROM phyDetails " + whereCond, null);
			//Cursor Items = db.rawQuery("SELECT  * FROM phyDetails where ScannedLocations = ''", null);
			//Cursor Items = db.rawQuery("SELECT  * FROM phyDetails where stkLoc = '" + stkLoc + "'", null);
			// Find ListView to populate
			final ListView lvItems = (ListView) findViewById(R.id.itemsListView);
			// Setup cursor adapter using cursor from last step
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
					  
					   Intent mintent = new Intent(getApplicationContext(), BarcodeScan.class);
					   mintent.putExtra("phydetId", phydetId);
	                   startActivityForResult(mintent, 501);
					}
	        });
	}
    @Override  
    protected void onActivityResult(int requestCode, int resultCode, Intent data)  
    {  
            super.onActivityResult(requestCode, resultCode, data); 
            init();

    }
    
    void InitDB(){
        db=openOrCreateDatabase(cs.DBName, Context.MODE_PRIVATE, null);
    }
    


	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.list_not_scanned_items, menu);
		return true;
	}

}
