package com.PETA.UDPTest;

import android.app.Activity;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ServiceInfo;
import android.os.Build;
import android.os.IBinder;
import android.util.Log;
import androidx.core.app.NotificationCompat;

public class OverlayController extends Service {
    private static final String TAG = "OverlayController";
    public static final String CHANNEL_ID = "Overlay_Channel_v13"; 
    public static final String ACTION_TOGGLE = "com.PETA.UDPTest.ACTION_TOGGLE";
    
    // Service自身のインスタンスを保持（通知更新用）
    private static OverlayController instance;
    public static Activity unityActivity;
    public static boolean isForeground = true;

    @Override
    public void onCreate() {
        super.onCreate();
        instance = this;
    }

    @Override
    public void onDestroy() {
        instance = null;
        super.onDestroy();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d(TAG, "Service Started");
        createNotificationChannel();
        
        // 起動時は「隠す」モードで通知を出す
        updateNotification(true);

        return START_STICKY;
    }

    // ★重要：状況に合わせて通知の中身を書き換えるメソッド
    public void updateNotification(boolean isAppVisible) {
        try {
            PendingIntent pendingIntent;
            String text;

            if (isAppVisible) {
                // A. アプリが見えている時 -> 「Receiver」を呼ぶ (隠すため)
                Intent toggleIntent = new Intent(ACTION_TOGGLE);
                toggleIntent.setPackage(getPackageName());
                int pFlags = PendingIntent.FLAG_UPDATE_CURRENT;
                if (Build.VERSION.SDK_INT >= 23) pFlags |= PendingIntent.FLAG_IMMUTABLE;
                pendingIntent = PendingIntent.getBroadcast(this, 100, toggleIntent, pFlags);
                text = "操作中 (タップして隠す)";
            } else {
                // B. アプリが隠れている時 -> 「Activity」を直接呼ぶ (制限回避のため)
                // これなら「Trampoline制限」に引っかからず、確実に前面に出ます
                Intent launchIntent = getPackageManager().getLaunchIntentForPackage(getPackageName());
                if (launchIntent != null) {
                    launchIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                    int pFlags = PendingIntent.FLAG_UPDATE_CURRENT;
                    if (Build.VERSION.SDK_INT >= 23) pFlags |= PendingIntent.FLAG_IMMUTABLE;
                    pendingIntent = PendingIntent.getActivity(this, 200, launchIntent, pFlags);
                } else {
                    // 万が一取得できない場合はReceiverへ
                     Intent toggleIntent = new Intent(ACTION_TOGGLE);
                     toggleIntent.setPackage(getPackageName());
                     int pFlags = PendingIntent.FLAG_UPDATE_CURRENT;
                     if (Build.VERSION.SDK_INT >= 23) pFlags |= PendingIntent.FLAG_IMMUTABLE;
                     pendingIntent = PendingIntent.getBroadcast(this, 100, toggleIntent, pFlags);
                }
                text = "待機中 (タップして起動)";
            }

            int iconId = getApplicationInfo().icon;
            if (iconId == 0) iconId = android.R.drawable.ic_dialog_info;

            Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .setSmallIcon(iconId)
                    .setContentTitle("コントローラー")
                    .setContentText(text)
                    .setContentIntent(pendingIntent) // ここが動的に変わる
                    .setPriority(NotificationCompat.PRIORITY_MAX)
                    .setOngoing(true)
                    .setSilent(true) // 更新時は音を消す
                    .build();

            // サービスが生きている限り通知を更新
            if (Build.VERSION.SDK_INT >= 34) {
                 startForeground(1, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_CONNECTED_DEVICE);
            } else {
                 startForeground(1, notification);
            }
            
            Log.d(TAG, "Notification Updated. Mode: " + (isAppVisible ? "HIDE" : "SHOW"));

        } catch (Exception e) {
            Log.e(TAG, "Error updating notification: " + e.getMessage());
        }
    }

    @Override
    public void onTaskRemoved(Intent rootIntent) {
        Log.d(TAG, "Task Removed - Stopping Service");
        stopSelf();
        super.onTaskRemoved(rootIntent);
    }
    
    @Override
    public IBinder onBind(Intent intent) { return null; }
    
    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null) {
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Overlay Control", NotificationManager.IMPORTANCE_HIGH);
                channel.setLockscreenVisibility(Notification.VISIBILITY_PUBLIC);
                channel.setSound(null, null);
                manager.createNotificationChannel(channel);
            }
        }
    }

    public static void setActivity(Activity activity) {
        unityActivity = activity;
    }

    // Unity(C#)から呼ばれるメソッド
    public static void setIsForeground(boolean foreground) {
        isForeground = foreground;
        Log.d(TAG, "State Changed: " + foreground);
        // ここで通知を切り替える！
        if (instance != null) {
            instance.updateNotification(foreground);
        }
    }
}