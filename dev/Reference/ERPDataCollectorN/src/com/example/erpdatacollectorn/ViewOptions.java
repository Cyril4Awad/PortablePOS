package com.example.erpdatacollectorn;

import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;

public class ViewOptions extends Activity {

	Button btnScannedItems;
	Button btnNotScannedItems;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_view_options);
		
		btnScannedItems = (Button) findViewById(R.id.btnScannedItems);
		btnScannedItems.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent mintent = new Intent(getApplicationContext(), ViewItems.class);
                mintent.putExtra("whereCond", "where updated = '1'");
                mintent.putExtra("Title", "Scanned Items");
                startActivityForResult(mintent, 501);
			}
		});
		
		btnNotScannedItems = (Button) findViewById(R.id.btnNotScannedItems);
		btnNotScannedItems.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent mintent = new Intent(getApplicationContext(), ViewItems.class);
                mintent.putExtra("whereCond", "where updated = '0'");
                mintent.putExtra("Title", "Not Scanned Items");
                startActivityForResult(mintent, 501);
			}
		});
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.view_options, menu);
		return true;
	}

}
