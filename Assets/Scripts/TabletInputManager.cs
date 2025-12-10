using TMPro;
using UnityEngine;

public class TabletInputManager : SingletonMonoBehaviour<TabletInputManager>
{
    public TabletData TabletData = new TabletData(PlayerID.Player1);
    public bool IsDirty = false;

    private TabletInputAction _inputActions;

    protected override void OnInitialize()
    {
        _inputActions = new TabletInputAction();
        _inputActions.Enable();

        _inputActions.TabletInput.LeftStick.performed += ctx =>
        {
            Vector2 value = ctx.ReadValue<Vector2>();
            TabletData.Set(AxisID.LeftStickX, value.x);
            TabletData.Set(AxisID.LeftStickY, value.y);
            IsDirty = true;
        };
        _inputActions.TabletInput.LeftStick.canceled += ctx =>
        {
            TabletData.Set(AxisID.LeftStickX, 0f);
            TabletData.Set(AxisID.LeftStickY, 0f);
            IsDirty = true;
        };

        _inputActions.TabletInput.RightStick.performed += ctx =>
        {
            Vector2 value = ctx.ReadValue<Vector2>();
            TabletData.Set(AxisID.RightStickX, value.x);
            TabletData.Set(AxisID.RightStickY, value.y);
            IsDirty = true;
        };
        _inputActions.TabletInput.RightStick.canceled += ctx =>
        {
            TabletData.Set(AxisID.RightStickX, 0f);
            TabletData.Set(AxisID.RightStickY, 0f);
            IsDirty = true;
        };

        _inputActions.TabletInput.Pointer.performed += ctx => { 
            Vector2 value = ctx.ReadValue<Vector2>();
            TabletData.Set(AxisID.TouchX, value.x);
            TabletData.Set(AxisID.TouchY, value.y);
            IsDirty = true;
        };
        _inputActions.TabletInput.PointerPress.performed += ctx => { TabletData.Set(ButtonID.Touch, true); IsDirty = true; };
        _inputActions.TabletInput.PointerPress.canceled += ctx => { TabletData.Set(ButtonID.Touch, false); IsDirty = true; };

        _inputActions.TabletInput.LeftStickPress.performed += ctx => { TabletData.Set(ButtonID.L3, true); IsDirty = true; };
        _inputActions.TabletInput.LeftStickPress.canceled += ctx => { TabletData.Set(ButtonID.L3, false); IsDirty = true; };
        _inputActions.TabletInput.RightStickPress.performed += ctx => { TabletData.Set(ButtonID.R3, true); IsDirty = true; };
        _inputActions.TabletInput.RightStickPress.canceled += ctx => { TabletData.Set(ButtonID.R3, false); IsDirty = true; };

        _inputActions.TabletInput.ButtonEast.performed += ctx => { TabletData.Set(ButtonID.East, true); IsDirty = true; };
        _inputActions.TabletInput.ButtonEast.canceled += ctx => { TabletData.Set(ButtonID.East, false); IsDirty = true; };
        _inputActions.TabletInput.ButtonWest.performed += ctx => { TabletData.Set(ButtonID.West, true); IsDirty = true; };
        _inputActions.TabletInput.ButtonWest.canceled += ctx => { TabletData.Set(ButtonID.West, false); IsDirty = true; };
        _inputActions.TabletInput.ButtonNorth.performed += ctx => { TabletData.Set(ButtonID.North, true); IsDirty = true; };
        _inputActions.TabletInput.ButtonNorth.canceled += ctx => { TabletData.Set(ButtonID.North, false); IsDirty = true; };
        _inputActions.TabletInput.ButtonSouth.performed += ctx => { TabletData.Set(ButtonID.South, true); IsDirty = true; };
        _inputActions.TabletInput.ButtonSouth.canceled += ctx => { TabletData.Set(ButtonID.South, false); IsDirty = true; };

        _inputActions.TabletInput.Up.performed += ctx => { TabletData.Set(ButtonID.Up, true); IsDirty = true; };
        _inputActions.TabletInput.Up.canceled += ctx => { TabletData.Set(ButtonID.Up, false); IsDirty = true; };
        _inputActions.TabletInput.Down.performed += ctx => { TabletData.Set(ButtonID.Down, true); IsDirty = true; };
        _inputActions.TabletInput.Down.canceled += ctx => { TabletData.Set(ButtonID.Down, false); IsDirty = true; };
        _inputActions.TabletInput.Left.performed += ctx => { TabletData.Set(ButtonID.Left, true); IsDirty = true; };
        _inputActions.TabletInput.Left.canceled += ctx => { TabletData.Set(ButtonID.Left, false); IsDirty = true; };
        _inputActions.TabletInput.Right.performed += ctx => { TabletData.Set(ButtonID.Right, true); IsDirty = true; };
        _inputActions.TabletInput.Right.canceled += ctx => { TabletData.Set(ButtonID.Right, false); IsDirty = true; };

        _inputActions.TabletInput.LeftShoulder.performed += ctx => { TabletData.Set(ButtonID.LB, true); IsDirty = true; };
        _inputActions.TabletInput.LeftShoulder.canceled += ctx => { TabletData.Set(ButtonID.LB, false); IsDirty = true; };
        _inputActions.TabletInput.RightShoulder.performed += ctx => { TabletData.Set(ButtonID.RB, true); IsDirty = true; };
        _inputActions.TabletInput.RightShoulder.canceled += ctx => { TabletData.Set(ButtonID.RB, false); IsDirty = true; };
        _inputActions.TabletInput.LeftTrigger.performed += ctx => { TabletData.Set(ButtonID.LT, true); IsDirty = true; };
        _inputActions.TabletInput.LeftTrigger.canceled += ctx => { TabletData.Set(ButtonID.LT, false); IsDirty = true; };
        _inputActions.TabletInput.RightTrigger.performed += ctx => { TabletData.Set(ButtonID.RT, true); IsDirty = true; };
        _inputActions.TabletInput.RightTrigger.canceled += ctx => { TabletData.Set(ButtonID.RT, false); IsDirty = true; };

        _inputActions.TabletInput.Start.performed += ctx => { TabletData.Set(ButtonID.Start, true); IsDirty = true; };
        _inputActions.TabletInput.Start.canceled += ctx => { TabletData.Set(ButtonID.Start, false); IsDirty = true; };
        _inputActions.TabletInput.Select.performed += ctx => { TabletData.Set(ButtonID.Select, true); IsDirty = true; };
        _inputActions.TabletInput.Select.canceled += ctx => { TabletData.Set(ButtonID.Select, false); IsDirty = true; };
    }

    public void InjectGyroData(Quaternion data)
    {
        TabletData.Set(AxisID.Gyro, data);
        IsDirty = true;
    }
}
