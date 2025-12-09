package com.PETA.UDPTest;

import android.app.Activity;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
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
    public static final String CHANNEL_ID = "Overlay_Channel_v10";
    public static final String ACTION_TOGGLE = "com.PETA.UDPTest.ACTION_TOGGLE";
    
    public static Activity unityActivity;
    public static boolean isForeground = true;

    @Override
    public void onCreate() {
        super.onCreate();
        Log.d(TAG, "==== Service onCreate called ====");
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d(TAG, "==== Service onStartCommand called ====");
        Log.d(TAG, "Android Version: " + Build.VERSION.SDK_INT);
        Log.d(TAG, "Device Model: " + Build.MODEL);
        
        try {
            // 通知権限チェック（Android 13+）
            if (Build.VERSION.SDK_INT >= 33) {
                NotificationManagerCompat notificationManager = NotificationManagerCompat.from(this);
                boolean areNotificationsEnabled = notificationManager.areNotificationsEnabled();
                Log.d(TAG, "Notifications enabled: " + areNotificationsEnabled);
                
                if (!areNotificationsEnabled) {
                    Log.e(TAG, "⚠️ NOTIFICATIONS ARE DISABLED! Please enable in settings.");
                }
            }
            
            // 通知チャンネル作成
            createNotificationChannel();
            
            // チャンネル作成確認
            verifyNotificationChannel();

            // トグル用のIntent
            Intent toggleIntent = new Intent(ACTION_TOGGLE);
            toggleIntent.setClass(this, ToggleReceiver.class);
            
            int pendingIntentFlags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= 23) {
                pendingIntentFlags |= PendingIntent.FLAG_IMMUTABLE;
            }
            
            PendingIntent pendingIntent = PendingIntent.getBroadcast(
                this, 
                100,  // リクエストコードを変更
                toggleIntent, 
                pendingIntentFlags
            );

            Log.d(TAG, "PendingIntent created: " + (pendingIntent != null));

            // 通知を構築
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .setSmallIcon(android.R.drawable.ic_dialog_info)
                    .setContentTitle("コントローラー起動中")
                    .setContentText("タップして切替 (Android " + Build.VERSION.SDK_INT + ")")
                    .setContentIntent(pendingIntent)
                    .setPriority(NotificationCompat.PRIORITY_MAX)
                    .setCategory(NotificationCompat.CATEGORY_SERVICE)
                    .setOngoing(true)
                    .setAutoCancel(false)
                    .setShowWhen(true)
                    .setVisibility(NotificationCompat.VISIBILITY_PUBLIC);

            // Android 8.0以降ではチャンネルが必須
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                builder.setChannelId(CHANNEL_ID);
            }

            Notification notification = builder.build();
            Log.d(TAG, "Notification built successfully");

            // Foregroundサービスとして起動
            if (Build.VERSION.SDK_INT >= 34) {
                Log.d(TAG, "Calling startForeground with TYPE_CONNECTED_DEVICE (API 34+)");
                startForeground(1, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_CONNECTED_DEVICE);
            } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                Log.d(TAG, "Calling startForeground (API 26+)");
                startForeground(1, notification);
            } else {
                Log.d(TAG, "Calling startForeground (Legacy)");
                startForeground(1, notification);
            }
            
            Log.d(TAG, "✅ Foreground service started successfully!");
            
            // 通知が本当に表示されているか確認
            verifyNotificationDisplayed();

        } catch (Exception e) {
            Log.e(TAG, "❌ Failed to start foreground service", e);
            Log.e(TAG, "Error details: " + e.getMessage());
            if (e.getCause() != null) {
                Log.e(TAG, "Cause: " + e.getCause().getMessage());
            }
            e.printStackTrace();
        }

        return START_STICKY;
    }

    @Override
    public IBinder onBind(Intent intent) { 
        return null; 
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        Log.d(TAG, "Service onDestroy called");
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            Log.d(TAG, "Creating notification channel: " + CHANNEL_ID);
            
            NotificationManager manager = getSystemService(NotificationManager.class);
            
            if (manager == null) {
                Log.e(TAG, "❌ NotificationManager is NULL!");
                return;
            }
            
            // 既存のチャンネルを削除
            try {
                NotificationChannel existingChannel = manager.getNotificationChannel(CHANNEL_ID);
                if (existingChannel != null) {
                    manager.deleteNotificationChannel(CHANNEL_ID);
                    Log.d(TAG, "Deleted existing channel");
                }
            } catch (Exception e) {
                Log.w(TAG, "Could not delete old channel: " + e.getMessage());
            }
            
            // 新しいチャンネルを作成
            NotificationChannel serviceChannel = new NotificationChannel(
                    CHANNEL_ID,
                    "コントローラーオーバーレイ",
                    NotificationManager.IMPORTANCE_HIGH  // 重要度を最高に
            );
            
            serviceChannel.setDescription("アプリの切替コントロール");
            serviceChannel.setLockscreenVisibility(Notification.VISIBILITY_PUBLIC);
            serviceChannel.setShowBadge(true);
            serviceChannel.enableVibration(false);
            serviceChannel.enableLights(false);
            serviceChannel.setSound(null, null);  // サウンドなし
            
            manager.createNotificationChannel(serviceChannel);
            Log.d(TAG, "✅ Notification channel created");
        }
    }
    
    private void verifyNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null) {
                NotificationChannel channel = manager.getNotificationChannel(CHANNEL_ID);
                if (channel != null) {
                    Log.d(TAG, "Channel verification:");
                    Log.d(TAG, "  - Name: " + channel.getName());
                    Log.d(TAG, "  - Importance: " + channel.getImportance());
                    Log.d(TAG, "  - ID: " + channel.getId());
                } else {
                    Log.e(TAG, "❌ Channel is NULL after creation!");
                }
            }
        }
    }
    
    private void verifyNotificationDisplayed() {
        try {
            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                android.service.notification.StatusBarNotification[] notifications = manager.getActiveNotifications();
                Log.d(TAG, "Active notifications count: " + notifications.length);
                for (android.service.notification.StatusBarNotification sbn : notifications) {
                    Log.d(TAG, "  - Notification ID: " + sbn.getId() + ", Package: " + sbn.getPackageName());
                }
            }
        } catch (Exception e) {
            Log.w(TAG, "Could not verify notifications: " + e.getMessage());
        }
    }

    public static void setActivity(Activity activity) {
        Log.d(TAG, "setActivity: " + (activity != null ? activity.getClass().getSimpleName() : "null"));
        unityActivity = activity;
    }

    public static void setIsForeground(boolean foreground) {
        Log.d(TAG, "setIsForeground: " + foreground);
        isForeground = foreground;
    }

    public static class ToggleReceiver extends BroadcastReceiver {
        private static final String TAG = "ToggleReceiver";
        
        @Override
        public void onReceive(Context context, Intent intent) {
            Log.d(TAG, "==== onReceive called ====");
            Log.d(TAG, "Action: " + intent.getAction());
            
            if (ACTION_TOGGLE.equals(intent.getAction())) {
                Log.d(TAG, "Toggle state - isForeground: " + isForeground);
                Log.d(TAG, "unityActivity: " + (unityActivity != null ? "exists" : "NULL"));
                
                if (unityActivity != null) {
                    try {
                        if (isForeground) {
                            Log.d(TAG, "→ Moving to background");
                            unityActivity.moveTaskToBack(true);
                        } else {
                            Log.d(TAG, "→ Bringing to foreground");
                            Intent launchIntent = new Intent(context, unityActivity.getClass());
                            launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                            context.startActivity(launchIntent);
                        }
                    } catch (Exception e) {
                        Log.e(TAG, "❌ Error during toggle", e);
                    }
                } else {
                    Log.e(TAG, "❌ unityActivity is NULL - cannot toggle");
                }
            }
        }
    }
}