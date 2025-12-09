using System;
using UnityEngine;

public class BleGattCallbackProxy : AndroidJavaProxy
{
    private Action<AndroidJavaObject, int, int> _onConnectionStateChange;
    private Action<AndroidJavaObject, int> _onServicesDiscovered;
    private Action<AndroidJavaObject, AndroidJavaObject> _onCharacteristicChanged;

    public BleGattCallbackProxy(
        Action<AndroidJavaObject, int, int> onConnectionStateChange,
        Action<AndroidJavaObject, int> onServicesDiscovered,
        Action<AndroidJavaObject, AndroidJavaObject> onCharacteristicChanged
    ) : base("com.PETA.BLECallback.UnityGattCallback$IGattListener") // Javaの内部インターフェースを指定
    {
        _onConnectionStateChange = onConnectionStateChange;
        _onServicesDiscovered = onServicesDiscovered;
        _onCharacteristicChanged = onCharacteristicChanged;
    }

    public void onConnectionStateChange(AndroidJavaObject gatt, int status, int newState)
    {
        if (_onConnectionStateChange != null) _onConnectionStateChange(gatt, status, newState);
    }

    public void onServicesDiscovered(AndroidJavaObject gatt, int status)
    {
        if (_onServicesDiscovered != null) _onServicesDiscovered(gatt, status);
    }

    public void onCharacteristicChanged(AndroidJavaObject gatt, AndroidJavaObject characteristic)
    {
        if (_onCharacteristicChanged != null) _onCharacteristicChanged(gatt, characteristic);
    }
}