using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum ButtonID
{
    South, East, West, North,
    LB, RB, LT, RT,
    L3, R3,
    Start, Select,
    Up, Down, Left, Right
}

public enum AxisID
{
    LeftStickX, LeftStickY,
    RightStickX, RightStickY,

    GyroX, GyroY, GyroZ, GyroW, Gyro
}

public enum PlayerID :byte
{
    Player1 = 0x10,
    Player2 = 0x20,
}

public static class PlayerIDExtensions
{
    // P1なら0、P2なら1を返す
    public static int ToIndex(this PlayerID id)
    {
        return (int)id >> 5;
    }
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TabletData
{
    public byte Header; // ID
    public ushort Buttons;
    public ushort Touch;   // 10本分のタッチON/OFFフラグ (ビット0~9)
    public short LStickX;
    public short LStickY;
    public short RStickX;
    public short RStickY;
    public short GyroX;
    public short GyroY;
    public short GyroZ;
    public short GyroW;

    // 配列を使うとMarshalの挙動が複雑になるから羅列
    public ushort Touch0X, Touch0Y;
    public ushort Touch1X, Touch1Y;
    public ushort Touch2X, Touch2Y;
    public ushort Touch3X, Touch3Y;
    public ushort Touch4X, Touch4Y;
    public ushort Touch5X, Touch5Y;
    public ushort Touch6X, Touch6Y;
    public ushort Touch7X, Touch7Y;
    public ushort Touch8X, Touch8Y;
    public ushort Touch9X, Touch9Y;

    public TabletData(PlayerID id)
    {
        this = default;

        Header = (byte)id;
    }

    public void Set(PlayerID id)
    {
        Header = (byte)id;
        return;
    }

    public void Set(ButtonID id, bool isPressed)
    {
        int bitIndex = (int)id;
        if (isPressed) Buttons |= (ushort)(1 << bitIndex);
        else Buttons &= (ushort)~(1 << bitIndex);
    }

    public void Set(AxisID id, float value)
    {
        switch (id)
        {
            case AxisID.LeftStickX: LStickX = FloatToShort(value); break;
            case AxisID.LeftStickY: LStickY = FloatToShort(value); break;
            case AxisID.RightStickX: RStickX = FloatToShort(value); break;
            case AxisID.RightStickY: RStickY = FloatToShort(value); break;

            case AxisID.GyroX: GyroX = FloatToShort(value); break;
            case AxisID.GyroY: GyroY = FloatToShort(value); break;
            case AxisID.GyroZ: GyroZ = FloatToShort(value); break;
            case AxisID.GyroW: GyroW = FloatToShort(value); break;
        }
    }

    public void Set(AxisID id, Quaternion value)
    {
        if (id != AxisID.Gyro)
            return;

        GyroX = FloatToShort(value.x);
        GyroY = FloatToShort(value.y);
        GyroZ = FloatToShort(value.z);
        GyroW = FloatToShort(value.w);
    }

    public void Set(int index, bool isTouched, Vector2 position)
    {
        if (index < 0 || index >= 10) return;

        // 1. フラグの設定 (常に実行)
        if (isTouched) Touch |= (ushort)(1 << index);
        else Touch &= (ushort)~(1 << index);

        // 2. 座標の設定 (タッチしている時だけ更新！)
        // これにより、isActive = false で呼んだ時は座標が維持されます
        if (isTouched)
        {
            ushort px = PixelToUShort(position.x, Screen.width);
            ushort py = PixelToUShort(position.y, Screen.height);

            switch (index)
            {
                case 0: Touch0X = px; Touch0Y = py; break;
                case 1: Touch1X = px; Touch1Y = py; break;
                case 2: Touch2X = px; Touch2Y = py; break;
                case 3: Touch3X = px; Touch3Y = py; break;
                case 4: Touch4X = px; Touch4Y = py; break;
                case 5: Touch5X = px; Touch5Y = py; break;
                case 6: Touch6X = px; Touch6Y = py; break;
                case 7: Touch7X = px; Touch7Y = py; break;
                case 8: Touch8X = px; Touch8Y = py; break;
                case 9: Touch9X = px; Touch9Y = py; break;
            }
        }
    }

    public bool GetTouch(int index) => (Touch & (1 << index)) != 0;

    public (ushort, ushort) GetTouchPos(int index)
    {
        ushort ux = 0, uy = 0;
        switch (index)
        {
            case 0: ux = Touch0X; uy = Touch0Y; break;
            case 1: ux = Touch1X; uy = Touch1Y; break;
            case 2: ux = Touch2X; uy = Touch2Y; break;
            case 3: ux = Touch3X; uy = Touch3Y; break;
            case 4: ux = Touch4X; uy = Touch4Y; break;
            case 5: ux = Touch5X; uy = Touch5Y; break;
            case 6: ux = Touch6X; uy = Touch6Y; break;
            case 7: ux = Touch7X; uy = Touch7Y; break;
            case 8: ux = Touch8X; uy = Touch8Y; break;
            case 9: ux = Touch9X; uy = Touch9Y; break;
        }
        return (ux, uy);
    }

    public byte[] Serialize()
    {
        int size = Marshal.SizeOf(typeof(TabletData));
        byte[] arr = new byte[size];

        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }

    public static TabletData Deserialize(byte[] bytes)
    {
        int size = Marshal.SizeOf(typeof(TabletData));
        if (bytes == null || bytes.Length < size) return default;

        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, ptr, size);
            return (TabletData)Marshal.PtrToStructure(ptr, typeof(TabletData));
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private short FloatToShort(float v) => (short)(Mathf.Clamp(v, -1f, 1f) * 32767f);
    private ushort PixelToUShort(float v, float max) => (ushort)(Mathf.Clamp01(v / max) * 65535f);
}