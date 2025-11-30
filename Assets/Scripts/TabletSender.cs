using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Net.Sockets;
using System.Net;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TabletSender : MonoBehaviour
{
    [SerializeField]
    private string pcIpAddress = "192.168.1.10";
    [SerializeField]
    private int port = 5000;

    private bool _deviceId;

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;

    void Start()
    {
        try
        {
            _udpClient = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(pcIpAddress), port);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UDPèâä˙âªÉGÉâÅ[:{e.Message}");
        }
    }

    void OnDisable()
    {
        _udpClient?.Close();
        EnhancedTouchSupport.Disable();
    }

    public void SendPacket(byte[] data)
    {
        if (_udpClient == null) return;

        try
        {
            _udpClient.Send(data, data.Length, _remoteEndPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ëóêMÇ≈Ç´Ç‹ÇπÇÒÇ≈ÇµÇΩ:{e.Message}");
        }
    }
}