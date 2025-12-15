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
    

    // ジャイロ
    [InputControl(name = "Gyro", layout = "Quaternion")]
    public Quaternion gyro;

    // タッチ座標/タッチしてるか
    [InputControl(name = "TouchPos0", layout = "Vector2")] public Vector2 touchPos0;
    [InputControl(name = "TouchPress0", layout = "Button", bit = 16)] public bool touchPress0;

    [InputControl(name = "TouchPos1", layout = "Vector2")] public Vector2 touchPos1;
    [InputControl(name = "TouchPress1", layout = "Button", bit = 17)] public bool touchPress1;

    [InputControl(name = "TouchPos2", layout = "Vector2")] public Vector2 touchPos2;
    [InputControl(name = "TouchPress2", layout = "Button", bit = 18)] public bool touchPress2;

    [InputControl(name = "TouchPos3", layout = "Vector2")] public Vector2 touchPos3;
    [InputControl(name = "TouchPress3", layout = "Button", bit = 19)] public bool touchPress3;

    [InputControl(name = "TouchPos4", layout = "Vector2")] public Vector2 touchPos4;
    [InputControl(name = "TouchPress4", layout = "Button", bit = 20)] public bool touchPress4;

    [InputControl(name = "TouchPos5", layout = "Vector2")] public Vector2 touchPos5;
    [InputControl(name = "TouchPress5", layout = "Button", bit = 21)] public bool touchPress5;

    [InputControl(name = "TouchPos6", layout = "Vector2")] public Vector2 touchPos6;
    [InputControl(name = "TouchPress6", layout = "Button", bit = 22)] public bool touchPress6;

    [InputControl(name = "TouchPos7", layout = "Vector2")] public Vector2 touchPos7;
    [InputControl(name = "TouchPress7", layout = "Button", bit = 23)] public bool touchPress7;

    [InputControl(name = "TouchPos8", layout = "Vector2")] public Vector2 touchPos8;
    [InputControl(name = "TouchPress8", layout = "Button", bit = 24)] public bool touchPress8;

    [InputControl(name = "TouchPos9", layout = "Vector2")] public Vector2 touchPos9;
    [InputControl(name = "TouchPress9", layout = "Button", bit = 25)] public bool touchPress9;

    [InputControl(name = "TouchPress", layout = "Button", bit = 26)]
    public bool touchPress;// 何かタッチしてたらtrue
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

    // タッチ部
    public ButtonControl press { get; private set; }
    public Vector2Control[] touchPositions { get; private set; }
    public ButtonControl[] touchPresses { get; private set; }

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

        press = GetChildControl<ButtonControl>("TouchPress");

        touchPositions = new Vector2Control[10];
        touchPresses = new ButtonControl[10];

        for (int i = 0; i < 10; i++)
        {
            touchPositions[i] = GetChildControl<Vector2Control>($"TouchPos{i}");
            touchPresses[i] = GetChildControl<ButtonControl>($"TouchPress{i}");
        }
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