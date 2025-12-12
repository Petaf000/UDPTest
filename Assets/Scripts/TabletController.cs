using System;
using Unity.Mathematics;
using UnityEngine;

public class TabletController : MonoBehaviour
{
    [SerializeField]
    private PlayerID playerID = PlayerID.Player1;
    [SerializeField]
    private PicoWController picoWController;

    private TabletSender sender;

    private float _lastSendTime;
    private const float SendInterval = 1f / 120f;
    private const float KeepAliveInterval = 0.2f;

    private bool wasLbPressed = false;

    void Start()
    {
        // 背景透過用設定
        if(Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0, 0, 0, 0);
        }

        if (!TryGetComponent(out sender))
            Debug.LogError("TabletSenderコンポーネントが見つかりません。");

        TabletInputManager.Instance.TabletData.Set(playerID);
    }
    private void Update()
    {
        bool isKeepAliveTime = Time.time - _lastSendTime >= KeepAliveInterval;

        if ((TabletInputManager.Instance.IsDirty || isKeepAliveTime) && Time.time - _lastSendTime >= SendInterval)
        {
            sender.SendPacket(TabletInputManager.Instance.TabletData.Serialize());
            TabletInputManager.Instance.IsDirty = false; // フラグを下ろす
            _lastSendTime = Time.time;

            // FPSモード用にジャイロのオンオフハードコードしてます
            bool isLbPressed = TabletDeviceDriver.Instance.GetButton(TabletInputManager.Instance.TabletData, ButtonID.LB);
            if (isLbPressed && !wasLbPressed)
            {
                if (picoWController.isSensorOn)
                    picoWController?.StopSensorStream();
                else
                    picoWController?.StartSensorStream();
                Debug.Log($"LB Button Pressed: Sensor Stream {(picoWController.isSensorOn ? "Stopped" : "Started")}");
            }
            wasLbPressed = isLbPressed;
        }
    }
}
