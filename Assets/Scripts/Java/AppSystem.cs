using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class AppSystem : MonoBehaviour
{
    private Text debugText;
    private string logBuffer = "";
    private bool permissionRequested = false;

    void Start()
    {
        CreateDebugCanvas();
        Log("Unity起動: デバッグ表示開始");

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
                Log("通知権限が付与されました");
                InitializeApp();
            }
        }
    }

    void RequestNotificationPermission()
    {
        Log("通知権限をリクエスト中...");

        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            permissionRequested = true;

            // 権限チェックを3秒後にも実行（フォールバック）
            Invoke(nameof(CheckPermissionAndInit), 3.0f);
        }
        else
        {
            Log("通知権限は既に付与されています");
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
            Log("⚠️ 通知権限が拒否されました。設定から手動で有効にしてください。");
        }
    }

    void InitializeApp()
    {
        Log("アプリ初期化開始");

        // Activity登録とライフサイクル監視
        RegisterActivityWithLifecycle();

        // サービス起動
        StartServiceForce();

        // 5秒後に最小化
        Invoke(nameof(MinimizeApp), 5.0f);
    }

    void CreateDebugCanvas()
    {
        GameObject canvasGO = new GameObject("DebugCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvas.sortingOrder = 999;

        GameObject textGO = new GameObject("DebugText");
        textGO.transform.parent = canvasGO.transform;

        debugText = textGO.AddComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugText.fontSize = 40;
        debugText.color = Color.red;
        debugText.horizontalOverflow = HorizontalWrapMode.Wrap;
        debugText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = debugText.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(50, 50);
        rt.offsetMax = new Vector2(-50, -50);
    }

    void Log(string msg)
    {
        logBuffer += msg + "\n";
        if (debugText != null) debugText.text = logBuffer;
        Debug.Log(msg);
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
                Log("サービスクラス: " + className);

                using (AndroidJavaClass serviceClass = new AndroidJavaClass(className))
                {
                    AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", context, serviceClass);

                    Log("サービス起動命令送信...");
                    if (GetSdkInt() >= 26)
                    {
                        AndroidJavaObject result = context.Call<AndroidJavaObject>("startForegroundService", intent);
                        Log("startForegroundService実行: " + (result != null ? "成功" : "失敗"));
                    }
                    else
                    {
                        AndroidJavaObject result = context.Call<AndroidJavaObject>("startService", intent);
                        Log("startService実行: " + (result != null ? "成功" : "失敗"));
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Log("★サービス起動エラー★: " + e.Message);
            Log("スタックトレース: " + e.StackTrace);
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
            Log("Activity登録完了");

            // ライフサイクルコールバックを設定
            SetupLifecycleCallbacks();
        }
        catch (System.Exception e)
        {
            Log("Activity登録失敗: " + e.Message);
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
            Log("ライフサイクル設定失敗: " + e.Message);
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
                Log(pauseStatus ? "アプリがバックグラウンドへ" : "アプリがフォアグラウンドへ");
            }
        }
        catch (System.Exception e)
        {
            Log("状態更新エラー: " + e.Message);
        }
    }

    void MinimizeApp()
    {
        Log("アプリを最小化します...");
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call<bool>("moveTaskToBack", true);
            }
            Log("最小化完了");
        }
        catch (System.Exception e)
        {
            Log("最小化エラー: " + e.Message);
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