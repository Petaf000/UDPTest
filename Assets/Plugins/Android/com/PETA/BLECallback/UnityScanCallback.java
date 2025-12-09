package com.PETA.BLECallback; // 自分のパッケージ名に合わせてください

import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanResult;
import java.util.List;
// Unityと通信するためのクラス（必要に応じて）
// import com.unity3d.player.UnityPlayer; 

public class UnityScanCallback extends ScanCallback {
    // C#側からリスナーを受け取るためのインターフェースを定義
    public interface IScanListener {
        void onScanResult(int callbackType, ScanResult result);
        void onBatchScanResults(List<ScanResult> results);
        void onScanFailed(int errorCode);
    }

    private IScanListener listener;

    public UnityScanCallback(IScanListener listener) {
        this.listener = listener;
    }

    @Override
    public void onScanResult(int callbackType, ScanResult result) {
        if (listener != null) {
            listener.onScanResult(callbackType, result);
        }
    }

    @Override
    public void onBatchScanResults(List<ScanResult> results) {
        if (listener != null) {
            listener.onBatchScanResults(results);
        }
    }

    @Override
    public void onScanFailed(int errorCode) {
        if (listener != null) {
            listener.onScanFailed(errorCode);
        }
    }
}