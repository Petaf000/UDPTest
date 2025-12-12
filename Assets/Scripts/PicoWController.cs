using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PicoWController : MonoBehaviour
{
    [Header("UI Debug")]
    public TMP_Text debugText;

    [Header("Settings")]
    public string targetDeviceName = "Pico";
    private string serviceUUID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    private string charUUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";
    private string descUUID = "00002902-0000-1000-8000-00805f9b34fb";
    private string writeUUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";

    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject bluetoothLeScanner;
    private AndroidJavaObject unityScanCallbackJavaObject;
    private BleScanListenerProxy scanListenerProxy;
    private AndroidJavaObject unityGattCallbackJavaObject;
    private BleGattCallbackProxy gattCallbackProxy;

    private AndroidJavaObject currentGatt;

    private Queue<string> logQueue = new Queue<string>();
    private Quaternion latestRotation = Quaternion.identity;
    private bool hasNewRotation = false;
    private bool isConnected = false;

    private Quaternion coordinateCorrection;
    private float initialObjectYaw;
    private float yawOffset = 0f;
    private bool calibrationRequested = false;

    public bool isSensorOn { get; private set; } = false;

    public void StartSensorStream()
    {
        if (currentGatt == null || !isConnected) return;

        Log("Mode: Active (High Perf)");

        SetConnectionPriority(1);

        SendCommand("START");
        calibrationRequested = true;
        isSensorOn = true;
    }

    public void StopSensorStream()
    {
        if (currentGatt == null || !isConnected) return;

        Log("Mode: Idle (Low Power)");

        // 1. Picoに「STOP」と命令を送る
        SendCommand("STOP");

        // 2. 通信頻度を「低速/バランス (Balanced: 0, or LowPower: 2)」にする
        SetConnectionPriority(2);
        isSensorOn = false;
    }

    // --- コマンド送信処理 ---
    private void SendCommand(string message)
    {
        try
        {
            AndroidJavaObject serviceUuidObj = new AndroidJavaClass("java.util.UUID").CallStatic<AndroidJavaObject>("fromString", serviceUUID);
            AndroidJavaObject service = currentGatt.Call<AndroidJavaObject>("getService", serviceUuidObj);

            if (service != null)
            {
                AndroidJavaObject charUuidObj = new AndroidJavaClass("java.util.UUID").CallStatic<AndroidJavaObject>("fromString", writeUUID);
                AndroidJavaObject characteristic = service.Call<AndroidJavaObject>("getCharacteristic", charUuidObj);

                if (characteristic != null)
                {
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                    characteristic.Call<bool>("setValue", data);
                    currentGatt.Call<bool>("writeCharacteristic", characteristic);
                }
            }
        }
        catch (Exception e)
        {
            Log("Send Cmd Error: " + e.Message);
        }
    }

    private void SetConnectionPriority(int priority)
    {
        try
        {
            currentGatt.Call<bool>("requestConnectionPriority", priority);
        }
        catch (Exception) { }
    }

    void Start()
    {
        Log("Initializing BLE...");
        if (Application.platform == RuntimePlatform.Android)
        {
            InitializeBluetooth();
        }

        coordinateCorrection = Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, 180, 180);
        initialObjectYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        lock (logQueue)
        {
            while (logQueue.Count > 0)
            {
                string msg = logQueue.Dequeue();
                if (debugText != null) debugText.text = msg + "\n" + debugText.text;
                else Debug.Log(msg);
            }
        }

        if (hasNewRotation)
        {
            Quaternion currentSensorUnity = coordinateCorrection * latestRotation;
            Vector3 currentEuler = currentSensorUnity.eulerAngles;

            if (calibrationRequested)
            {
                yawOffset = Mathf.DeltaAngle(currentEuler.y, initialObjectYaw);

                calibrationRequested = false;
                Log($"Calibrated! SensorY:{currentEuler.y:F1} -> TargetY:{initialObjectYaw:F1} (Offset:{yawOffset:F1})");
            }

            float finalX = -currentEuler.x;
            float finalY = currentEuler.y + yawOffset;
            float finalZ = -currentEuler.z;

            transform.rotation = Quaternion.Euler(finalX, finalY, finalZ);

            TabletInputManager.Instance.InjectGyroData(transform.rotation);

            hasNewRotation = false;
        }
    }

    void InitializeBluetooth()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var bluetoothManager = activity.Call<AndroidJavaObject>("getSystemService", "bluetooth"))
            {
                if (bluetoothManager != null)
                {
                    bluetoothAdapter = bluetoothManager.Call<AndroidJavaObject>("getAdapter");
                }
            }

            if (bluetoothAdapter != null && bluetoothAdapter.Call<bool>("isEnabled"))
            {
                bluetoothLeScanner = bluetoothAdapter.Call<AndroidJavaObject>("getBluetoothLeScanner");
                Log("Bluetooth Adapter Ready.");
                StartScan();
            }
            else
            {
                Log("Error: Bluetooth Adapter is null or disabled.");
            }
        }
        catch (Exception e)
        {
            Log("Init Error: " + e.Message);
        }
    }

    // --- 1. スキャン開始 ---
    void StartScan()
    {
        if (bluetoothLeScanner == null) return;
        if (isConnected) return;

        Log("Preparing to Scan...");

        try
        {
            scanListenerProxy = new BleScanListenerProxy((name, address, uuids) =>
            {
                if (!string.IsNullOrEmpty(name) && name.Equals(targetDeviceName))
                {
                    Log($"Target Found: {name}! Stopping scan...");
                    StopScan();
                    ConnectByAddress(address);
                }
            });

            unityScanCallbackJavaObject = new AndroidJavaObject("com.PETA.BLECallback.UnityScanCallback", scanListenerProxy);
            bluetoothLeScanner.Call("startScan", unityScanCallbackJavaObject);
            Log("Scanning Started...");
        }
        catch (Exception e)
        {
            Log("StartScan Error: " + e.Message);
        }
    }

    void StopScan()
    {
        if (bluetoothLeScanner != null && unityScanCallbackJavaObject != null)
        {
            try
            {
                bluetoothLeScanner.Call("stopScan", unityScanCallbackJavaObject);
            }
            catch (Exception) { }
        }
    }

    // --- 2. 接続処理 ---
    public void ConnectByAddress(string address)
    {
        try
        {
            AndroidJavaObject device = bluetoothAdapter.Call<AndroidJavaObject>("getRemoteDevice", address);
            Log($"Connecting to {address}...");

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                gattCallbackProxy = new BleGattCallbackProxy(
                    (gatt, status, newState) => {
                        if (newState == 2)
                        {
                            currentGatt = gatt;

                            Log("Connected! Discovering Services...");
                            OnConnectedSuccess();
                            gatt.Call<bool>("discoverServices");
                        }
                        else if (newState == 0) // Disconnected
                        {
                            OnDisconnected();
                            currentGatt = null;
                            if (gatt != null) gatt.Call("close");
                        }
                    },
                    // onServicesDiscovered
                    (gatt, status) => {
                        if (status == 0)
                        {
                            Log("Services Discovered. Setup Notify...");
                            SetupNotification(gatt);
                        }
                    },
                    // onCharacteristicChanged
                    (gatt, characteristic) => {
                        byte[] data = characteristic.Call<byte[]>("getValue");
                        OnDataReceived(data);
                    }
                );

                unityGattCallbackJavaObject = new AndroidJavaObject("com.PETA.BLECallback.UnityGattCallback", gattCallbackProxy);
                device.Call<AndroidJavaObject>("connectGatt", activity, false, unityGattCallbackJavaObject);
            }
        }
        catch (Exception e)
        {
            Log("Connect Error: " + e.Message);
        }
    }

    private void SetupNotification(AndroidJavaObject gatt)
    {
        try
        {
            AndroidJavaObject serviceUuidObj = new AndroidJavaClass("java.util.UUID").CallStatic<AndroidJavaObject>("fromString", serviceUUID);
            AndroidJavaObject service = gatt.Call<AndroidJavaObject>("getService", serviceUuidObj);

            if (service != null)
            {
                AndroidJavaObject charUuidObj = new AndroidJavaClass("java.util.UUID").CallStatic<AndroidJavaObject>("fromString", charUUID);
                AndroidJavaObject characteristic = service.Call<AndroidJavaObject>("getCharacteristic", charUuidObj);

                if (characteristic != null)
                {
                    gatt.Call<bool>("setCharacteristicNotification", characteristic, true);

                    AndroidJavaObject descUuidObj = new AndroidJavaClass("java.util.UUID").CallStatic<AndroidJavaObject>("fromString", descUUID);
                    AndroidJavaObject descriptor = characteristic.Call<AndroidJavaObject>("getDescriptor", descUuidObj);

                    if (descriptor != null)
                    {
                        byte[] val = new byte[] { 0x01, 0x00 };
                        descriptor.Call<bool>("setValue", val);
                        gatt.Call<bool>("writeDescriptor", descriptor);
                        Log("Notify Setup Done (Wait for START cmd)");

                        StopSensorStream();
                    }
                }
            }
        }
        catch (Exception e) { Log("Setup Notify Error: " + e.Message); }
    }

    public void Log(string msg)
    {
        lock (logQueue)
        {
            logQueue.Enqueue(msg);
        }
    }

    public void OnDataReceived(byte[] data)
    {
        if (data == null || data.Length < 16) return;

        float x = BitConverter.ToSingle(data, 0);
        float y = BitConverter.ToSingle(data, 4);
        float z = BitConverter.ToSingle(data, 8);
        float w = BitConverter.ToSingle(data, 12);

        Quaternion rot = new Quaternion(x, y, z, w);

        latestRotation = rot;
        hasNewRotation = true;
    }

    public void OnConnectedSuccess()
    {
        isConnected = true;
    }

    public void OnDisconnected()
    {
        isConnected = false;
        Log("Disconnected. Restarting scan in 3 seconds...");
        Invoke("StartScan", 3.0f);
    }
}