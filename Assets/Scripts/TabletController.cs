using System;
using Unity.Mathematics;
using UnityEngine;

public class TabletController : MonoBehaviour
{
    [SerializeField]
    private PlayerID playerID = PlayerID.Player1;

    private TabletSender sender;

    private float _lastSendTime;
    private const float SendInterval = 1f / 120f;

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
        // 変更があり、かつ前回の送信から時間が経っていれば送る
        if (TabletInputManager.Instance.IsDirty && Time.time - _lastSendTime >= SendInterval)
        {
            sender.SendPacket(TabletInputManager.Instance.TabletData.Serialize());
            TabletInputManager.Instance.IsDirty = false; // フラグを下ろす
            _lastSendTime = Time.time;

            byte[] bytes = TabletInputManager.Instance.TabletData.Serialize();
            TabletData data = TabletData.Deserialize(bytes);

            float DecodeShort(short val) => val / 32767f;

            bool isTouching = (data.HeaderAndTouch & 0x80) != 0;
            byte deviceId = (byte)(data.HeaderAndTouch & 0x7F);

            Debug.Log($"--- Tablet Packet State ---");
            Debug.Log($"[Header] ID: 0x{deviceId:X2} | Touching: {isTouching}");

            string binaryButtons = Convert.ToString(data.Buttons, 2).PadLeft(16, '0');
            Debug.Log($"[Buttons] Hex: 0x{data.Buttons:X4} | Binary: {binaryButtons}");

            Debug.Log($"[LStick] X: {DecodeShort(data.LStickX):F3} | Y: {DecodeShort(data.LStickY):F3}");
            Debug.Log($"[RStick] X: {DecodeShort(data.RStickX):F3} | Y: {DecodeShort(data.RStickY):F3}");

            Debug.Log($"[Gyro] W: {DecodeShort(data.GyroW):F3} | X: {DecodeShort(data.GyroX):F3}");

            Debug.Log($"[Touch] X (Raw): {data.TouchX} | Y (Raw): {data.TouchY}");
            Debug.Log($"---------------------------");
        }
    }
}
