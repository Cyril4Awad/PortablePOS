package com.example.erpdatacollectorn;

import android.content.Context;
import android.database.Cursor;
import android.graphics.Color;
import android.support.v4.widget.CursorAdapter;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

public class ItemsCursorAdapter extends CursorAdapter {

	 String SP_DecNo;
	 String SP_Fashion;
	 String SP_Expiry;
	
	public ItemsCursorAdapter(Context context, Cursor cursor, int flags,String _SP_DecNo,String _SP_Fashion,String _SP_Expiry) {
		super(context, cursor, 0);
		SP_DecNo = _SP_DecNo;
		SP_Fashion = _SP_Fashion;
		SP_Expiry = _SP_Expiry;
	}

	  // The newView method is used to inflate a new view and return it, 
	  // you don't bind any data to the view at this point. 
	  @Override
	  public View newView(Context context, Cursor cursor, ViewGroup parent) {
	      return LayoutInflater.from(context).inflate(R.layout.lst_items, parent, false);
	  }

	  // The bindView method is used to bind all data to a given view
	  // such as setting the text on a TextView. 
	  @Override
	  public void bindView(View view, Context context, Cursor cursor) {
	      // Find fields to populate in inflated template
		  
		  TextView tvPhyDetID= (TextView) view.findViewById(R.id.tvPhyDetID);
	      TextView tvReference= (TextView) view.findViewById(R.id.tvReference);
	      TextView tvDescrption = (TextView) view.findViewById(R.id.tvDescrption);
	      TextView tvExp= (TextView) view.findViewById(R.id.tvExp);
	      TextView tvLot = (TextView) view.findViewById(R.id.tvLot);
	      TextView tvAvlbQty = (TextView) view.findViewById(R.id.tvAvlbQty);
	      TextView tvCountedQty = (TextView) view.findViewById(R.id.tvCountedQty);
	      
	      TextView tt3 = (TextView) view.findViewById(R.id.tt3);
	      TextView tt4 = (TextView) view.findViewById(R.id.tt4);
	      
	      // Extract properties from cursor
	      String Pcd_RecId = cursor.getString(cursor.getColumnIndexOrThrow("Pcd_RecId"));
	      String Refernce = cursor.getString(cursor.getColumnIndexOrThrow("Refernce"));
	      String stk_Desc =  cursor.getString(cursor.getColumnIndexOrThrow("stk_Desc"));
	      String exp =  cursor.getString(cursor.getColumnIndexOrThrow("exp"));
	      String Pcd_Batch =  cursor.getString(cursor.getColumnIndexOrThrow("Pcd_Batch"));
	      String Pcd_Qty =  cursor.getString(cursor.getColumnIndexOrThrow("Pcd_Qty"));
	      String usercounted =  cursor.getString(cursor.getColumnIndexOrThrow("usercounted"));
	      
	      
	      // Populate fields with extracted properties
	      tvPhyDetID.setText(Pcd_RecId);
	      tvPhyDetID.setTextColor(Color.rgb(0, 153, 255));//dark blue

	      tvReference.setText(Refernce);
	      tvReference.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      tvDescrption.setText(stk_Desc);
	      tvDescrption.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      tvExp.setText(exp);
	      tvExp.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      tvLot.setText(Pcd_Batch);
	      tvLot.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      tvAvlbQty.setText(Pcd_Qty);
	      tvAvlbQty.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      tvCountedQty.setText(usercounted);
	      tvCountedQty.setTextColor(Color.rgb(0, 153, 255));//dark blue
	      
	      if (SP_Expiry.equalsIgnoreCase("N")){
	    	  tvExp.setVisibility(View.GONE);
	    	  tvLot.setVisibility(View.GONE);
	    	  tt3.setVisibility(View.GONE);
	    	  tt4.setVisibility(View.GONE);
	      }

	  }

}
