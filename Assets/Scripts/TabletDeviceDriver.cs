using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class TabletDeviceDriver : SingletonMonoBehaviour<TabletDeviceDriver>
{
    // シーンになくても勝手に起動して常駐する
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoStart()
    {
        var obj = new GameObject("[System] TabletDeviceDriver");
        obj.AddComponent<TabletDeviceDriver>();
        DontDestroyOnLoad(obj);
    }

    // 仮想デバイスの実体
    public TabletDevice DeviceP1 { get; private set; }
    public TabletDevice DeviceP2 { get; private set; }

    protected override void OnInitialize()
    {
        // 仮想デバイスを2つ作成して接続
        DeviceP1 = InputSystem.AddDevice<TabletDevice>("TabletP1");
        DeviceP2 = InputSystem.AddDevice<TabletDevice>("TabletP2");
        Debug.Log("仮想タブレット P1/P2 を接続しました");
    }

    void OnDestroy()
    {
        if (DeviceP1 != null) InputSystem.RemoveDevice(DeviceP1);
        if (DeviceP2 != null) InputSystem.RemoveDevice(DeviceP2);
    }

    float DecodeShort(short val) => val / 32767f;
    float DecodeUShort(ushort val) => val / 65535f;

    bool GetButton(TabletData data, ButtonID id)
    {
        return (data.Buttons & (1 << (int)id)) != 0;
    }

    public void InjectData(PlayerID playerId, TabletData data)
    {
        var targetDevice = (playerId == PlayerID.Player1) ? DeviceP1 : DeviceP2;

        if (targetDevice == null)
        {
            Debug.LogWarning($"指定されたPlayerID {playerId} に対応するTabletDeviceが見つかりません。");
            return;
        }

        if (targetDevice == null) return;

        using (StateEvent.From(targetDevice, out var stateEvent))
        {
            targetDevice.leftStick.WriteValueIntoEvent(
                new Vector2(DecodeShort(data.LStickX), DecodeShort(data.LStickY)),
                stateEvent
            );

            targetDevice.rightStick.WriteValueIntoEvent(
                new Vector2(DecodeShort(data.RStickX), DecodeShort(data.RStickY)),
                stateEvent
            );

            // ABXY (South/East/West/North)
            targetDevice.buttonSouth.WriteValueIntoEvent(GetButton(data, ButtonID.South) ? 1f : 0f, stateEvent);
            targetDevice.buttonEast.WriteValueIntoEvent(GetButton(data, ButtonID.East) ? 1f : 0f, stateEvent);
            targetDevice.buttonWest.WriteValueIntoEvent(GetButton(data, ButtonID.West) ? 1f : 0f, stateEvent);
            targetDevice.buttonNorth.WriteValueIntoEvent(GetButton(data, ButtonID.North) ? 1f : 0f, stateEvent);

            // D-Pad
            targetDevice.up.WriteValueIntoEvent(GetButton(data, ButtonID.Up) ? 1f : 0f, stateEvent);
            targetDevice.down.WriteValueIntoEvent(GetButton(data, ButtonID.Down) ? 1f : 0f, stateEvent);
            targetDevice.left.WriteValueIntoEvent(GetButton(data, ButtonID.Left) ? 1f : 0f, stateEvent);
            targetDevice.right.WriteValueIntoEvent(GetButton(data, ButtonID.Right) ? 1f : 0f, stateEvent);

            // Shoulder / Trigger
            targetDevice.leftShoulder.WriteValueIntoEvent(GetButton(data, ButtonID.LB) ? 1f : 0f, stateEvent);
            targetDevice.rightShoulder.WriteValueIntoEvent(GetButton(data, ButtonID.RB) ? 1f : 0f, stateEvent);
            targetDevice.leftTrigger.WriteValueIntoEvent(GetButton(data, ButtonID.LT) ? 1f : 0f, stateEvent);
            targetDevice.rightTrigger.WriteValueIntoEvent(GetButton(data, ButtonID.RT) ? 1f : 0f, stateEvent);

            // Stick Press / Option
            targetDevice.leftStickPress.WriteValueIntoEvent(GetButton(data, ButtonID.L3) ? 1f : 0f, stateEvent);
            targetDevice.rightStickPress.WriteValueIntoEvent(GetButton(data, ButtonID.R3) ? 1f : 0f, stateEvent);
            targetDevice.start.WriteValueIntoEvent(GetButton(data, ButtonID.Start) ? 1f : 0f, stateEvent);
            targetDevice.select.WriteValueIntoEvent(GetButton(data, ButtonID.Select) ? 1f : 0f, stateEvent);

            // --- ジャイロ (short -> float -> Quaternion) ---
            targetDevice.gyro.WriteValueIntoEvent(
                new Quaternion(
                    DecodeShort(data.GyroX),
                    DecodeShort(data.GyroY),
                    DecodeShort(data.GyroZ),
                    DecodeShort(data.GyroW)
                ),
                stateEvent
            );

            // --- タッチ (ushort -> 0-1正規化 -> Screen座標) ---
            bool isTouching = (data.HeaderAndTouch & 0x80) != 0;

            targetDevice.press.WriteValueIntoEvent(isTouching ? 1f : 0f, stateEvent);

            targetDevice.normalizedTouchPos.WriteValueIntoEvent(
                new Vector2(
                    DecodeUShort(data.TouchX),
                    DecodeUShort(data.TouchY)
                ),
                stateEvent
            );

            targetDevice.touchPos.WriteValueIntoEvent(
                new Vector2(
                    DecodeUShort(data.TouchX) * Screen.width,
                    DecodeUShort(data.TouchY) * Screen.height
                ),
                stateEvent
            );

            InputSystem.QueueEvent(stateEvent);
        }
    }
}