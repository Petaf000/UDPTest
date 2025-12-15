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

    private ConcurrentDictionary<PlayerID, TabletData> latestInputs = new ConcurrentDictionary<PlayerID, TabletData>();

    void Start()
    {
        StartUdpReceiver();

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }

    void OnDisable()
    {
        isRunning = false;
        udpServer?.Close();
        udpServer?.Dispose();
    }

    private void Update()
    {
        // InputSystemへの注入はメインスレッドで行う必要がある
        foreach (var kvp in latestInputs)
        {
            TabletDeviceDriver.Instance.InjectData(kvp.Key, kvp.Value);
        }
    }

    private async void StartUdpReceiver()
    {
        udpServer = new UdpClient(listenPort);
        udpServer.Client.ReceiveBufferSize = 65536;
        Debug.Log($"UDP Receiver Started on port {listenPort}");

        while (isRunning)
        {
            try
            {
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
        Debug.Log("受信");
        var data = TabletData.Deserialize(bytes);

        PlayerID playerId = (PlayerID)(data.Header & 0x7F);
        if (playerId != PlayerID.Player1 && playerId != PlayerID.Player2) return;

        latestInputs.AddOrUpdate(playerId, data, (key, oldValue) => data);
    }
}