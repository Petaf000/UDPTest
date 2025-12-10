package com.PETA.UDPTest;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.widget.Toast;

public class ToggleReceiver extends BroadcastReceiver {
    private static final String TAG = "ToggleReceiver";

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d(TAG, "==== Notification Tapped (Hide Action) ====");
        
        try {
	    if (OverlayController.unityActivity != null) {
                Log.d(TAG, "Action: Minimizing app");
                
                // 1. アプリを裏へ
                OverlayController.unityActivity.moveTaskToBack(true);
                
                // 2. フラグを更新して、通知を「起動モード」に切り替える
                OverlayController.setIsForeground(false);
                
                Toast.makeText(context, "最小化しました", Toast.LENGTH_SHORT).show();
            }
        } catch (Exception e) {
            Log.e(TAG, "Error in toggle: " + e.getMessage());
        }
    }
}