using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PCReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private int listenPort = 5000;

    private UdpClient udpServer;
    private Thread receiveThread;
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
        StopUdpReceiver();
    }

    private void Update()
    {
        // InputSystemへの注入はメインスレッドで行う必要がある
        foreach (var kvp in latestInputs)
        {
            TabletDeviceDriver.Instance.InjectData(kvp.Key, kvp.Value);
        }
    }

    private void StartUdpReceiver()
    {
        StopUdpReceiver();// 一応

        udpServer = new UdpClient(listenPort);
        isRunning = true;
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log($"UDP Thread Started on port {listenPort}");
    }

    private void StopUdpReceiver()
    {
        isRunning = false;
        if (udpServer != null)
        {
            udpServer.Close();
            udpServer = null;
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (isRunning)
        {
            try
            {
                if (udpServer == null) break;

                // ここで待機するが、別スレッドなのでUnityは止まらない
                byte[] bytes = udpServer.Receive(ref remoteEP);

                if (bytes != null && bytes.Length > 0)
                {
                    DeserializePacket(bytes);
                }
            }
            catch (SocketException) { /* 終了時など */ }
            catch (ThreadAbortException) { /* 終了時 */ }
            catch (Exception e)
            {
                Debug.LogError($"UDP Thread Error: {e.Message}");
            }
        }
    }

    private void DeserializePacket(byte[] bytes)
    {
        var data = TabletData.Deserialize(bytes);

        PlayerID playerId = (PlayerID)(data.Header & 0x7F);
        if (playerId != PlayerID.Player1 && playerId != PlayerID.Player2) return;

        latestInputs.AddOrUpdate(playerId, data, (key, oldValue) => data);
    }
}