using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class PCReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private int listenPort = 5000;

    private UdpClient udpServer;
    private bool isRunning = true;

    private ConcurrentDictionary<int, TabletData> latestInputs = new ConcurrentDictionary<int, TabletData>();

    void Start()
    {
        StartUdpReceiver();
    }

    void OnDisable()
    {
        isRunning = false;
        udpServer?.Close();
        udpServer?.Dispose();
    }

    void Update()
    {
        // InputSystemへの注入はメインスレッドで行う必要がある
        foreach (var kvp in latestInputs)
        {
            int playerId = kvp.Key;
            TabletData data = kvp.Value;

            // TODO: TabletInputにデータを注入するコードをここに追加
        }
    }

    private async void StartUdpReceiver()
    {
        udpServer = new UdpClient(listenPort);
        Debug.Log($"UDP Receiver Started on port {listenPort}");

        while (isRunning)
        {
            try
            {
                // 非同期で受信待機
                var result = await udpServer.ReceiveAsync();

                // 受信処理
                DeserializePacket(result.Buffer);
            }
            catch (ObjectDisposedException)
            {
                // 終了時に発生するので無視
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UDP Receive Error: {e.Message}");
            }
        }
    }

    private void DeserializePacket(byte[] bytes)
    {
        var data = TabletData.Deserialize(bytes);

        byte playerId = (byte)(data.HeaderAndTouch & 0x7F);
        if (playerId != (byte)PlayerID.Player1 && playerId != (byte)PlayerID.Player2) return;// 登録されてないIDは無視

        latestInputs.AddOrUpdate(playerId, data, (key, oldValue) => data);
    }
}