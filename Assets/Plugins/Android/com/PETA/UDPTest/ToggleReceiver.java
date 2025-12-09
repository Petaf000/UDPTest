package com.PETA.UDPTest;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.app.NotificationManager;
import android.app.PendingIntent;
import androidx.core.app.NotificationCompat;
import android.os.Build;

public class ToggleReceiver extends BroadcastReceiver {
    private static final String TAG = "ToggleReceiver";

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d(TAG, "==== Notification Tapped! ====");
        
        // 1. アプリが生きていて、かつ前面にいるかチェック
        boolean isForeground = OverlayController.isForeground;
        boolean isAppAlive = (OverlayController.unityActivity != null);

        Log.d(TAG, "Status - Alive: " + isAppAlive + ", Foreground: " + isForeground);

        try {
            if (isAppAlive && isForeground) {
                // A. アプリが前面にいる -> 裏に隠す
                Log.d(TAG, "Action: Minimizing app");
                OverlayController.unityActivity.moveTaskToBack(true);
                updateNotification(context, "待機中 (タップして起動)");
            } else {
                // B. アプリが裏にいる、または死んでいる -> 前面に呼び戻す
                Log.d(TAG, "Action: Launching/Bringing to front");
                
                // UnityのActivityを起動するIntent
                // ※ ここはご自身のActivity名に合わせてください（通常は UnityPlayerGameActivity か UnityPlayerActivity）
                Intent launchIntent = new Intent(context, com.unity3d.player.UnityPlayerGameActivity.class);
                
                // 既存のタスクがあればそれを使う、なければ作る
                launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_REORDER_TO_FRONT | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                
                context.startActivity(launchIntent);
                updateNotification(context, "操作中 (タップして隠す)");
            }
        } catch (Exception e) {
            Log.e(TAG, "Error in toggle: " + e.getMessage());
        }
    }

    // 通知の文言を書き換える処理
    private void updateNotification(Context context, String statusText) {
        try {
            NotificationManager manager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
            
            // 再度PendingIntentを作る
            Intent toggleIntent = new Intent(context, ToggleReceiver.class);
            toggleIntent.setAction(OverlayController.ACTION_TOGGLE);
            
            int flags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= 23) flags |= PendingIntent.FLAG_IMMUTABLE;
            
            PendingIntent pendingIntent = PendingIntent.getBroadcast(context, 100, toggleIntent, flags);

            // 通知を再構築し	て上書き更新
            // アイコンはアプリのアイコンを取得
            int iconId = context.getApplicationInfo().icon;
            if (iconId == 0) iconId = android.R.drawable.ic_dialog_info;

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, OverlayController.CHANNEL_ID)
                    .setSmallIcon(iconId)
                    .setContentTitle("コントローラー")
                    .setContentText(statusText) // ここで文言が変わる
                    .setContentIntent(pendingIntent)
                    .setPriority(NotificationCompat.PRIORITY_MAX)
                    .setOngoing(true)
                    .setSilent(true); // 更新時は音を鳴らさない

            manager.notify(1, builder.build());
        } catch (Exception e) {
            Log.e(TAG, "Failed to update notification text: " + e.getMessage());
        }
    }
}