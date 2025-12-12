using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [SerializeField] PlayerID playerID = PlayerID.Player1;
    private TabletDevice device;

    private void Start()
    {
        device = (playerID == PlayerID.Player1) ? TabletDeviceDriver.Instance.DeviceP1 : TabletDeviceDriver.Instance.DeviceP2;
    }

    private void Update()
    {
        if (device == null) return;
        Quaternion gyroQuat = device.gyro.ReadValue();
        transform.localRotation = gyroQuat;
    }
}
