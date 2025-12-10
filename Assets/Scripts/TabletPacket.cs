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
    Up, Down, Left, Right,
    Touch
}

public enum AxisID
{
    LeftStickX, LeftStickY,
    RightStickX, RightStickY,

    GyroX, GyroY, GyroZ, GyroW, Gyro,

    TouchX, TouchY
}

public enum PlayerID :byte
{
    Player1 = 0x10,
    Player2 = 0x20,
}

public static class PlayerIDExtensions
{
    // P1‚È‚ç0AP2‚È‚ç1‚ð•Ô‚·
    public static int ToIndex(this PlayerID id)
    {
        return (int)id >> 5;
    }
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TabletData
{
    public byte HeaderAndTouch; // ID+PacketID
    public ushort Buttons;
    public short LStickX;
    public short LStickY;
    public short RStickX;
    public short RStickY;
    public ushort TouchX;
    public ushort TouchY;
    public short GyroX;
    public short GyroY;
    public short GyroZ;
    public short GyroW;

    public TabletData(PlayerID id)
    {
        this = default;

        HeaderAndTouch = (byte)id;
    }

    public void Set(PlayerID id)
    {
        HeaderAndTouch = (byte)((HeaderAndTouch & 0x80) | ((byte)id & 0x7F));
        return;
    }

    public void Set(ButtonID id, bool isPressed)
    {
        if (id == ButtonID.Touch)
        {
            if (isPressed) HeaderAndTouch |= 0x80;
            else HeaderAndTouch &= 0x7F;
            return;
        }

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

            case AxisID.TouchX: TouchX = PixelToUShort(value, Screen.width); break;
            case AxisID.TouchY: TouchY = PixelToUShort(value, Screen.height); break;
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