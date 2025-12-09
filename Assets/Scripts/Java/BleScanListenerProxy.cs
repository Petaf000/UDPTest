using System;
using System.Collections.Generic;
using UnityEngine;

public class BleScanListenerProxy : AndroidJavaProxy
{
    // Actionの引数を増やしました (名前, アドレス, UUIDリスト)
    private Action<string, string, List<string>> _onDeviceFoundCallback;

    public BleScanListenerProxy(Action<string, string, List<string>> onDeviceFoundCallback)
        : base("com.PETA.BLECallback.UnityScanCallback$IScanListener")
    {
        _onDeviceFoundCallback = onDeviceFoundCallback;
    }

    public void onScanResult(int callbackType, AndroidJavaObject result)
    {
        if (result == null) return;

        // 1. 基本情報の取得
        using (AndroidJavaObject device = result.Call<AndroidJavaObject>("getDevice"))
        {
            string name = device.Call<string>("getName");
            string address = device.Call<string>("getAddress");
            if (string.IsNullOrEmpty(name)) name = "No Name";

            // 2. UUIDの取得 (ここが新機能)
            // ScanRecordの中にアドバタイズされたService UUIDが入っています
            List<string> serviceUuids = new List<string>();
            try
            {
                using (AndroidJavaObject scanRecord = result.Call<AndroidJavaObject>("getScanRecord"))
                {
                    if (scanRecord != null)
                    {
                        using (AndroidJavaObject serviceUuidsList = scanRecord.Call<AndroidJavaObject>("getServiceUuids"))
                        {
                            if (serviceUuidsList != null)
                            {
                                // JavaのList<ParcelUuid>を解析する
                                int size = serviceUuidsList.Call<int>("size");
                                for (int i = 0; i < size; i++)
                                {
                                    using (AndroidJavaObject parcelUuid = serviceUuidsList.Call<AndroidJavaObject>("get", i))
                                    {
                                        string uuidStr = parcelUuid.Call<string>("toString");
                                        serviceUuids.Add(uuidStr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // UUID解析失敗時は無視
                Debug.LogWarning("Failed to parse UUIDs: " + e.Message);
            }

            // 3. 司令塔へ報告
            if (_onDeviceFoundCallback != null)
            {
                _onDeviceFoundCallback(name, address, serviceUuids);
            }
        }
    }

    public void onBatchScanResults(AndroidJavaObject results) { }
    public void onScanFailed(int errorCode) { }
}