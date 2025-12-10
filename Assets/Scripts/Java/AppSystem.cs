using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class AppSystem : MonoBehaviour
{
    private bool permissionRequested = false;

    void Start()
    {
        // Android 13以降では通知権限をリクエスト
        if (GetSdkInt() >= 33)
        {
            RequestNotificationPermission();
        }
        else
        {
            InitializeApp();
        }
    }

    void Update()
    {
        // 権限がまだリクエストされていて、結果待ちの場合
        if (permissionRequested && GetSdkInt() >= 33)
        {
            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                permissionRequested = false;
                InitializeApp();
            }
        }
    }

    void RequestNotificationPermission()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            permissionRequested = true;

            // 権限チェックを3秒後にも実行（フォールバック）
            Invoke(nameof(CheckPermissionAndInit), 3.0f);
        }
        else
        {
            InitializeApp();
        }
    }

    void CheckPermissionAndInit()
    {
        if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            permissionRequested = false;
            InitializeApp();
        }
        else
        {
        }
    }

    void InitializeApp()
    {
        // Activity登録とライフサイクル監視
        RegisterActivityWithLifecycle();

        // サービス起動
        StartServiceForce();

        // 5秒後に最小化
        Invoke(nameof(MinimizeApp), 5.0f);
    }

    void StartServiceForce()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

                string className = "com.PETA.UDPTest.OverlayController";

                using (AndroidJavaClass serviceClass = new AndroidJavaClass(className))
                {
                    AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", context, serviceClass);

                    if (GetSdkInt() >= 26)
                    {
                        AndroidJavaObject result = context.Call<AndroidJavaObject>("startForegroundService", intent);
                    }
                    else
                    {
                        AndroidJavaObject result = context.Call<AndroidJavaObject>("startService", intent);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
        }
    }

    void RegisterActivityWithLifecycle()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.PETA.UDPTest.OverlayController"))
                {
                    pluginClass.CallStatic("setActivity", activity);
                }
            }

            // ライフサイクルコールバックを設定
            SetupLifecycleCallbacks();
        }
        catch (System.Exception e)
        {
        }
    }

    void SetupLifecycleCallbacks()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.PETA.UDPTest.OverlayController"))
                {
                    pluginClass.CallStatic("setIsForeground", true);
                }
            }
        }
        catch (System.Exception e)
        {
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        try
        {
            using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.PETA.UDPTest.OverlayController"))
            {
                // pauseStatus = true: バックグラウンド, false: フォアグラウンド
                pluginClass.CallStatic("setIsForeground", !pauseStatus);
            }
        }
        catch (System.Exception e)
        {
        }
    }

    void MinimizeApp()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call<bool>("moveTaskToBack", true);
            }
        }
        catch (System.Exception e)
        {
        }
    }

    int GetSdkInt()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
}