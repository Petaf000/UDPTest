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
import androidx.core.app.NotificationManagerCompat;

public class OverlayController extends Service {
    private static final String TAG = "OverlayController";
    // IDを更新して通知設定をリセット
    public static final String CHANNEL_ID = "Overlay_Channel_v12"; 
    public static final String ACTION_TOGGLE = "com.PETA.UDPTest.ACTION_TOGGLE";
    
    public static Activity unityActivity;
    public static boolean isForeground = true;

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d(TAG, "Service Started");
        createNotificationChannel();

        // 外部ファイルの ToggleReceiver を呼び出す
        Intent toggleIntent = new Intent(this, ToggleReceiver.class);
        toggleIntent.setAction(ACTION_TOGGLE);
        
        int pendingFlags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= 23) pendingFlags |= PendingIntent.FLAG_IMMUTABLE;
        
        PendingIntent pendingIntent = PendingIntent.getBroadcast(this, 100, toggleIntent, pendingFlags);

        int iconId = getApplicationInfo().icon;
        if (iconId == 0) iconId = android.R.drawable.ic_dialog_info;

        Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                .setSmallIcon(iconId)
                .setContentTitle("コントローラー")
                .setContentText("操作中 (タップして隠す)")
                .setContentIntent(pendingIntent)
                .setPriority(NotificationCompat.PRIORITY_MAX)
                .setOngoing(true)
                .build();

        if (Build.VERSION.SDK_INT >= 34) {
             startForeground(1, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_CONNECTED_DEVICE);
        } else {
             startForeground(1, notification);
        }

        return START_STICKY;
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

    public static void setIsForeground(boolean foreground) {
        isForeground = foreground;
    }
}