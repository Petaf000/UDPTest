using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[StructLayout(LayoutKind.Sequential)]
public struct TabletInputTypeInfo : IInputStateTypeInfo
{
    public FourCC format => new FourCC('T', 'A', 'B', 'L'); // Tablet Controller

    // 左スティック
    [InputControl(name = "LeftStick", layout = "Vector2")]
    public Vector2 Lstick;

    // 右スティック
    [InputControl(name = "RightStick", layout = "Vector2")]
    public Vector2 Rstick;

    // Aボタン (South)
    [InputControl(name = "ButtonSouth", layout = "Button", bit = 0)]
    public bool buttonSouth;

    // Xボタン (West)
    [InputControl(name = "ButtonWest", layout = "Button", bit = 1)]
    public bool buttonWest;

    // Yボタン (North)
    [InputControl(name = "ButtonNorth", layout = "Button", bit = 2)]
    public bool buttonNorth;

    // Bボタン (East)
    [InputControl(name = "ButtonEast", layout = "Button", bit = 3)]
    public bool buttonEast;

    [InputControl(name = "D-Pad_Up", layout = "Button", bit = 4)]
    public bool up;

    [InputControl(name = "D-Pad_Down", layout = "Button", bit = 5)]
    public bool down;

    [InputControl(name = "D-Pad_Left", layout = "Button", bit = 6)]
    public bool left;

    [InputControl(name = "D-Pad_Right", layout = "Button", bit = 7)]
    public bool right;

    [InputControl(name = "ShoulderLeft", layout = "Button", bit = 8)]
    public bool LB;

    [InputControl(name = "TriggerLeft", layout = "Button", bit = 9)]
    public bool LT;

    [InputControl(name = "ShoulderRight", layout = "Button", bit = 10)]
    public bool RB;

    [InputControl(name = "TriggerRight", layout = "Button", bit = 11)]
    public bool RT;

    [InputControl(name = "StickPressLeft", layout = "Button", bit = 12)]
    public bool L3;

    [InputControl(name = "StickPressRight", layout = "Button", bit = 13)]
    public bool R3;

    [InputControl(name = "Start", layout = "Button", bit = 14)]
    public bool start;

    [InputControl(name = "Select", layout = "Button", bit = 15)]
    public bool select;

    // タッチしてるかどうか
    [InputControl(name = "TouchPress", layout = "Button", bit = 14)]
    public bool touchPress;

    // ジャイロ
    [InputControl(name = "Gyro", layout = "Quaternion")]
    public Quaternion gyro;

    // タッチ座標
    [InputControl(name = "TouchPos", layout = "Vector2")]
    public Vector2 touchPos;
}

[InputControlLayout(stateType = typeof(TabletInputTypeInfo), displayName = "Tablet Controller")]
public class TabletDevice : InputDevice
{
    public Vector2Control leftStick { get; private set; }
    public Vector2Control rightStick { get; private set; }

    public ButtonControl buttonSouth { get; private set; } // A
    public ButtonControl buttonWest { get; private set; }  // X
    public ButtonControl buttonNorth { get; private set; } // Y
    public ButtonControl buttonEast { get; private set; }  // B

    public ButtonControl up { get; private set; }
    public ButtonControl down { get; private set; }
    public ButtonControl left { get; private set; }
    public ButtonControl right { get; private set; }
    
    public ButtonControl leftShoulder { get; private set; }
    public ButtonControl leftTrigger { get; private set; }
    public ButtonControl rightShoulder { get; private set; }
    public ButtonControl rightTrigger { get; private set; }
    
    public ButtonControl leftStickPress { get; private set; }
    public ButtonControl rightStickPress { get; private set; }

    public ButtonControl start { get; private set; }
    public ButtonControl select { get; private set; }

    public QuaternionControl gyro { get; private set; }

    public Vector2Control touchPos { get; private set; }
    public ButtonControl press { get; private set; }

    protected override void FinishSetup()
    {
        base.FinishSetup();

        leftStick = GetChildControl<Vector2Control>("LeftStick");
        rightStick = GetChildControl<Vector2Control>("RightStick");

        buttonSouth = GetChildControl<ButtonControl>("ButtonSouth");
        buttonWest = GetChildControl<ButtonControl>("ButtonWest");
        buttonNorth = GetChildControl<ButtonControl>("ButtonNorth");
        buttonEast = GetChildControl<ButtonControl>("ButtonEast");

        up = GetChildControl<ButtonControl>("D-Pad_Up");
        down = GetChildControl<ButtonControl>("D-Pad_Down");
        left = GetChildControl<ButtonControl>("D-Pad_Left");
        right = GetChildControl<ButtonControl>("D-Pad_Right");

        leftShoulder = GetChildControl<ButtonControl>("ShoulderLeft");
        leftTrigger = GetChildControl<ButtonControl>("TriggerLeft");
        rightShoulder = GetChildControl<ButtonControl>("ShoulderRight");
        rightTrigger = GetChildControl<ButtonControl>("TriggerRight");

        leftStickPress = GetChildControl<ButtonControl>("StickPressLeft");
        rightStickPress = GetChildControl<ButtonControl>("StickPressRight");

        start = GetChildControl<ButtonControl>("Start");
        select = GetChildControl<ButtonControl>("Select");

        gyro = GetChildControl<QuaternionControl>("Gyro");

        touchPos = GetChildControl<Vector2Control>("TouchPos");
        press = GetChildControl<ButtonControl>("Press");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer()
    {
        InputSystem.RegisterLayout<TabletDevice>("TabletController");
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
        InputSystem.RegisterLayout<TabletDevice>("TabletController");
    }
#endif
}