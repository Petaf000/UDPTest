using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerID playerID = PlayerID.Player1;

    private TabletInputAction inputActions;

    private float speed = 10f;
    private Vector2 moveInput = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new TabletInputAction();

        TabletDevice device = null;

        if (playerID == PlayerID.Player1)
            device = TabletDeviceDriver.Instance.DeviceP1;
        else
            device = TabletDeviceDriver.Instance.DeviceP2;

        if (device != null)
            inputActions.devices = new InputDevice[] { device };

        inputActions.TabletInput.LeftStick.performed += ctx => Move(ctx.ReadValue<Vector2>());
        inputActions.TabletInput.ButtonSouth.performed += ctx => Jump();

        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Dispose();
        inputActions = null;
    }

    private void Update()
    {
        var move = new Vector3(moveInput.x, 0f, moveInput.y) * speed * Time.deltaTime;
        transform.Translate(move);
    }

    public void Move(Vector2 inputVal)
    {
        moveInput = inputVal;
    }

    public void Jump()
    {
        Debug.Log("Jumping");
    }
}
