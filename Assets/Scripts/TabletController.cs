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
        float timeSinceLast = Time.time - _lastSendTime;

        bool shouldSend = false;

        if (TabletInputManager.Instance.IsDirty)
        {
            if (timeSinceLast >= SendInterval) shouldSend = true;
        }
        else
        {
            if (timeSinceLast >= KeepAliveInterval) shouldSend = true;
        }

        if (shouldSend)
        {
            sender.SendPacket(TabletInputManager.Instance.TabletData.Serialize());

            TabletInputManager.Instance.IsDirty = false;
            _lastSendTime = Time.time;

            // LBボタン処理 (変更なし)
            bool isLbPressed = TabletDeviceDriver.Instance.GetButton(TabletInputManager.Instance.TabletData, ButtonID.LB);
            if (isLbPressed && !wasLbPressed)
            {
                if (picoWController.isSensorOn) picoWController?.StopSensorStream();
                else picoWController?.StartSensorStream();
            }
            wasLbPressed = isLbPressed;
        }
    }
}
