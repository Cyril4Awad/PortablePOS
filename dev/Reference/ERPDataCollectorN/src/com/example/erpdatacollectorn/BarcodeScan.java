package com.example.erpdatacollectorn;

import java.util.ArrayList;
import java.util.List;


import android.media.AudioManager;
import android.media.ToneGenerator;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.app.Activity;
import android.app.AlertDialog.Builder;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.text.InputType;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

public class BarcodeScan extends Activity {

	SQLiteDatabase db;
	EditText txtbar;
	
    TextView tvPhyDetID;
    TextView tvReference;
    TextView tvDescrption;
    TextView tvExp;
    TextView tvLot;
    TextView textViewExp;
    TextView textViewLot;
    TextView tvAvlbQty;
    EditText tvCountedQty;
    
    EditText tvTotalScannedt;
    
    //Button btnSave;
    Button btn_SaveTotalCounted;
    Button btn_UpdateLastScan;
    Button btn_CancelLastScan;
    
	ConfigurationSettings cs;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_barcode_scan);
		
		 cs = new ConfigurationSettings(getApplicationContext());
		 
		db=openOrCreateDatabase(cs.DBName, Context.MODE_PRIVATE, null);
		

        
	      tvPhyDetID= (TextView)findViewById(R.id.tvPhyDetID2);
	      tvReference= (TextView)findViewById(R.id.tvReference2);
	      tvDescrption = (TextView)findViewById(R.id.tvDescrption2);
	      tvExp= (TextView)findViewById(R.id.tvExp2);
	      tvLot = (TextView)findViewById(R.id.tvLot2);
	      
	      textViewExp = (TextView)findViewById(R.id.textViewExp);
	      textViewLot = (TextView)findViewById(R.id.textViewLot);
	      
	      if (cs.SP_Expiry.equalsIgnoreCase("N")){
	    	  tvExp.setVisibility(View.GONE);
	    	  tvLot.setVisibility(View.GONE);
	    	  textViewExp.setVisibility(View.GONE);
	    	  textViewLot.setVisibility(View.GONE);
	      }
	     
	      tvAvlbQty = (TextView)findViewById(R.id.tvAvlbQty2);
	      
	      tvTotalScannedt = (EditText)findViewById(R.id.tvTotalScannedt);
	      tvCountedQty = (EditText)findViewById(R.id.tvCountedQty2);
	      
	      tvPhyDetID.setText("");
	      tvReference.setText("");
	      tvDescrption.setText("");
	      tvExp.setText("");
	      tvLot.setText("");
	      tvAvlbQty.setText("");
	      tvTotalScannedt.setText("");
	      tvCountedQty.setText("");
	      
	      if (cs.SP_DecNo.equals("0")){
	    	
	      }
	      else{
	    	  tvCountedQty.setInputType(InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL);
	    	  tvTotalScannedt.setInputType(InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL);
	      }
	      
 
		txtbar = (EditText)findViewById(R.id.txtbar);
		txtbar.setText("");
		txtbar.setOnKeyListener(new View.OnKeyListener() {
            public boolean onKey(View v, int keyCode, KeyEvent event) {
            	
                // If the event is a key-down event on the "enter" button
                if ((event.getAction() == KeyEvent.ACTION_DOWN) &&
                    (keyCode == KeyEvent.KEYCODE_ENTER)) {
            		GetDescription(txtbar.getText().toString().trim(),true,true,true,false,false);
                	Reinit();
                	
                  return true;
                }
                return false;
            }
        });  
		

		
		btn_SaveTotalCounted = (Button) findViewById(R.id.btn_SaveTotalCounted);
		btn_SaveTotalCounted.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View arg0) {
				GetItemDetails(tvPhyDetID.getText().toString().trim(),false,false,true,false,false);
				
            	Reinit();
			}
		});
		
		btn_UpdateLastScan = (Button) findViewById(R.id.btn_UpdateLastScan);
		btn_UpdateLastScan.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View arg0) {
				GetItemDetails(tvPhyDetID.getText().toString().trim(),false,false,true,true,false);
				
            	Reinit();
			}
		});
		
		btn_CancelLastScan = (Button) findViewById(R.id.btn_CancelLastScan);
		btn_CancelLastScan.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View arg0) {
				GetItemDetails(tvPhyDetID.getText().toString().trim(),false,false,true,false,true);
				
            	Reinit();
			}
		});
		


		 
		Bundle extras = getIntent().getExtras();
		 if (extras != null) {
			 String phydetId = "";
			 phydetId = extras.getString("phydetId");
			 GetItemDetails(phydetId.trim(),true,false,false,false,false);
         	 Reinit();
		 }
		 else{
			 
		 }
		 
			tvCountedQty.setEnabled(false);
			 btn_UpdateLastScan.setEnabled(false);
			 btn_CancelLastScan.setEnabled(false);
	}

    void Reinit(){
    	
    	new CountDownTimer(100,100){
			
    		@Override
    		public void onFinish() {
    			try {
    				txtbar.selectAll();
    				//txtbar.setText("");
    				txtbar.requestFocus();
    				txtbar.selectAll();
    		        
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
    			}
    		@Override
    		public void onTick(long millisUntilFinished) {
    			
    			}
    		}
    		.start();  
    }
    
	private void GetDescription(String bar,Boolean GetDBQty,Boolean IncQty,Boolean SaveQty,Boolean AddCntQty,Boolean SubCntQty){
		String Desc = "Item Not Found";
		String PcBarc_DRecId = "";
		try {
			Cursor c=db.rawQuery("SELECT PcBarc_DRecId FROM phyBarcodes where PcBarc_Barcode='" + bar + "'", null);
			if(c.moveToFirst())
			{
				PcBarc_DRecId = c.getString(0);
				GetItemDetails(PcBarc_DRecId,GetDBQty,IncQty,SaveQty,AddCntQty,SubCntQty);
			}
			else{
				
				Cursor c2=db.rawQuery("SELECT Pcd_RecId FROM phyDetails where Refernce like '" + bar + "%'", null);
				if(c2.moveToFirst())
				{
					List<String> Ids = new ArrayList<String>();
					do {
		            	Ids.add(c2.getString(0));
		            } while (c2.moveToNext());
					
					if(Ids.size()==1){
						for(String sIds : Ids){
							PcBarc_DRecId = sIds;
			            }
						GetItemDetails(PcBarc_DRecId,GetDBQty,IncQty,SaveQty,AddCntQty,SubCntQty);
					}
					else{
						Intent mintent = new Intent(getApplicationContext(), MultiSearchActivity.class);
						//mintent.putExtra("whereCond", " Refernce like '" + bar + "%'");
						mintent.putExtra("whereCond", "");
						mintent.putExtra("PAssedBar", bar);
						mintent.putExtra("OrderBy", "");
						mintent.putExtra("Title","Search By Reference");
						mintent.putExtra("ParentActivity", "BarcodeScan");
						startActivityForResult(mintent, 555);
					}
				}
				else
				{
					
					if(bar.contains("|")==true)
					{
						String[] items = bar.split("\\|"); 
						String QrExp = items[1];
						String[] QrExps = QrExp.split("/"); 
						String exp1 = "";
						if(QrExps[0].equalsIgnoreCase("00"))
						{
							exp1 = QrExps[1] + "." + QrExps[2];
						}
						else {
							exp1 = QrExps[0] + "." + QrExps[1] + "." + QrExps[2];
						}
						Cursor c3=db.rawQuery("SELECT Pcd_RecId FROM phyDetails where Pcd_stkId = '" + items[0] + 
								"' and exp = '" + exp1 + "' and Pcd_Batch = '" + items[2] + "'"  , null);
					
						
						if(c3.moveToFirst())
						{
							PcBarc_DRecId = c3.getString(0);
							GetItemDetails(PcBarc_DRecId,GetDBQty,IncQty,SaveQty,AddCntQty,SubCntQty);
						}
						else
						{
							 tvPhyDetID.setText("");
						      tvReference.setText("");
						      tvDescrption.setText("Item Not Found");
						      tvExp.setText("");
						      tvLot.setText("");
						      tvAvlbQty.setText("");
						      tvTotalScannedt.setText("");
						      tvCountedQty.setText("");
						      
						      showMessage("Warning", "Item Not Found");
						      // send the tone to the "alarm" stream (classic beeps go there) with 100% volume
						      ToneGenerator toneG = new ToneGenerator(AudioManager.STREAM_ALARM, 100);
						      toneG.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD, 200); 
						      
						      ToneGenerator toneGen1 = new ToneGenerator(AudioManager.STREAM_MUSIC, 100);             
						      toneGen1.startTone(ToneGenerator.TONE_CDMA_PIP,150);
						}
						
					}
					
					else
					{
						  tvPhyDetID.setText("");
					      tvReference.setText("");
					      tvDescrption.setText("Item Not Found");
					      tvExp.setText("");
					      tvLot.setText("");
					      tvAvlbQty.setText("");
					      tvTotalScannedt.setText("");
					      tvCountedQty.setText("");
					      
					      showMessage("Warning", "Item Not Found");
					      // send the tone to the "alarm" stream (classic beeps go there) with 100% volume
					      ToneGenerator toneG = new ToneGenerator(AudioManager.STREAM_ALARM, 100);
					      toneG.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD, 200); 
					      
					      ToneGenerator toneGen1 = new ToneGenerator(AudioManager.STREAM_MUSIC, 100);             
					      toneGen1.startTone(ToneGenerator.TONE_CDMA_PIP,150); 
					}
					
				    
				}
  
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			showMessage("Error", e.getMessage());
		}
		
	}
	
	
	private void GetItemDetails(String PcBarc_DRecId,Boolean GetDBQty,Boolean IncQty,Boolean SaveQty,Boolean AddCntQty,Boolean SubCntQty){
		String Desc = "Item Not Found";

		boolean validqty = true;
		if(tvTotalScannedt.getText().toString().trim().length()>6){
			validqty = false;
		}
		if(tvCountedQty.getText().toString().trim().length()>6){
			validqty = false;
		}
		if(validqty == false){
			showMessage("Error", "Incorrect Qty");
		}
		
		else{
			
		
		try {
			Cursor c2= db.rawQuery("SELECT * FROM phyDetails where Pcd_RecId='" + PcBarc_DRecId + "'", null);
			if(c2.moveToFirst()){
				tvPhyDetID.setText(PcBarc_DRecId);
				
			    tvReference.setText(c2.getString(c2.getColumnIndexOrThrow("Refernce")));
			    tvDescrption.setText(c2.getString(c2.getColumnIndexOrThrow("stk_Desc")));
			    tvExp.setText(c2.getString(c2.getColumnIndexOrThrow("exp")));
			    tvLot.setText(c2.getString(c2.getColumnIndexOrThrow("Pcd_Batch")));
			    tvAvlbQty.setText(c2.getString(c2.getColumnIndexOrThrow("Pcd_Qty")));
			    
			    double qtyToIncrement = 1;
			    double scanqty = 0;
			    scanqty = Double.parseDouble(c2.getString(c2.getColumnIndexOrThrow("usercounted")));
			   
			    tvCountedQty.setEnabled(true);
			    btn_UpdateLastScan.setEnabled(true);
			    btn_CancelLastScan.setEnabled(true);
			    
			    if(GetDBQty==false){
			    	scanqty = Double.parseDouble(tvTotalScannedt.getText().toString().trim());
			    }
			    
			    
			    if(IncQty==true){
			    	tvCountedQty.setText("1");
			    	qtyToIncrement = Double.parseDouble(tvCountedQty.getText().toString().trim());
			    	qtyToIncrement = 1;
			    	scanqty = scanqty + qtyToIncrement;
			    }
			    
			    
			    if(AddCntQty==true){
			    	qtyToIncrement = Double.parseDouble(tvCountedQty.getText().toString().trim()) - 1;
			    	scanqty = scanqty + qtyToIncrement;
			    	tvCountedQty.setEnabled(false);
				    btn_UpdateLastScan.setEnabled(false);
				    btn_CancelLastScan.setEnabled(false);
			    }
			    
			    if(SubCntQty==true){
			    	qtyToIncrement = Double.parseDouble(tvCountedQty.getText().toString().trim());
			    	scanqty = scanqty - qtyToIncrement;
			    	tvCountedQty.setEnabled(false);
			    	btn_UpdateLastScan.setEnabled(false);
					btn_CancelLastScan.setEnabled(false);
			    }
			    
			    if(SaveQty==true){
			    	db.execSQL("UPDATE phyDetails SET updated = '1', usercounted = '" + String.valueOf(scanqty) + "' WHERE Pcd_RecId = '" + PcBarc_DRecId + "'");
			    }
			    
			    //tvCountedQty.setText(String.valueOf(scanqty));
			    tvTotalScannedt.setText(String.valueOf(scanqty));
			    //tvTotalScannedt.setText(c2.getString(c2.getColumnIndexOrThrow("usercounted")));
			    
			}
			else{
			      tvPhyDetID.setText("");
			      tvReference.setText("");
			      tvDescrption.setText("Item Not Found");
			      tvExp.setText("");
			      tvLot.setText("");
			      tvAvlbQty.setText("");
			      tvCountedQty.setText("");
			      tvTotalScannedt.setText("");
			      showMessage("Warning", "Item Not Found");
			      
			   // send the tone to the "alarm" stream (classic beeps go there) with 100% volume
			      ToneGenerator toneG = new ToneGenerator(AudioManager.STREAM_ALARM, 100);
			      toneG.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD, 200); 
			      
			      ToneGenerator toneGen1 = new ToneGenerator(AudioManager.STREAM_MUSIC, 100);             
			      toneGen1.startTone(ToneGenerator.TONE_CDMA_PIP,150); 
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			showMessage("Error", e.getMessage());
		}
		
		}
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
	        switch (requestCode){
		        case 555:
		        	if(resultCode == Activity.RESULT_OK){
		        		//GetDescription(txtbar.getText().toString().trim(),true,true,true,false,false);
		        		String dID = data.getStringExtra("phydetId").trim();
						GetItemDetails(dID,true,true,true,false,false);
		        	}
		        	   
		        }
	    }
	 
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.barcode_scan, menu);
		return true;
	}

}
