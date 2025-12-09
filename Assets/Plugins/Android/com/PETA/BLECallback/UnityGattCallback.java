package com.PETA.BLECallback;

import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;

public class UnityGattCallback extends BluetoothGattCallback {
    // C#側で実装するためのインターフェース
    public interface IGattListener {
        void onConnectionStateChange(BluetoothGatt gatt, int status, int newState);
        void onServicesDiscovered(BluetoothGatt gatt, int status);
        void onCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic);
    }

    private IGattListener listener;

    public UnityGattCallback(IGattListener listener) {
        this.listener = listener;
    }

    @Override
    public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
        if (listener != null) listener.onConnectionStateChange(gatt, status, newState);
    }

    @Override
    public void onServicesDiscovered(BluetoothGatt gatt, int status) {
        if (listener != null) listener.onServicesDiscovered(gatt, status);
    }

    // Android 13以降でも互換性のためこの古いシグネチャが呼ばれます（または転送されます）
    @Override
    public void onCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
        if (listener != null) listener.onCharacteristicChanged(gatt, characteristic);
    }
}