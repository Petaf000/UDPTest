using UnityEngine;
using UnityEngine.Android;

public class AndroidPermission : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android 12 (API 31) 以降に必要な権限をまとめてリクエスト
        string[] permissions = new string[]
        {
            "android.permission.BLUETOOTH_SCAN",
            "android.permission.BLUETOOTH_CONNECT",
            "android.permission.ACCESS_FINE_LOCATION" //念のため
        };

        Permission.RequestUserPermissions(permissions);
#endif
    }
}