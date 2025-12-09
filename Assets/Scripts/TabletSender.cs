using UnityEngine;
using UnityEngine.UI; // InputFieldを使うなら必要
using System.Net.Sockets;
using System.Net;
using TMPro;

public class TabletSender : MonoBehaviour
{
    [SerializeField]
    private string pcIpAddress = "192.168.1.10";
    [SerializeField]
    private int port = 5000;
    [SerializeField]
    private TMP_Text tmpText;

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;

    void Start()
    {
        try
        {
            _udpClient = new UdpClient();
            // 初期化時に一回セットアップ
            UpdateRemoteEndPoint(pcIpAddress);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UDP初期化エラー:{e.Message}");
        }
    }

    public void SetIpAddress(string newIp)
    {
        // IPアドレスとして正しい形式かチェックしてから更新
        if (UpdateRemoteEndPoint(newIp))
        {
            pcIpAddress = newIp; // Inspectorの表示用変数も更新しておく
            Debug.Log($"宛先IPを {newIp} に変更しました");
        }
        else
        {
            Debug.LogWarning($"無効なIPアドレス形式です: {newIp}");
        }
    }

    // 内部処理用：宛先ポイントを作り直す
    private bool UpdateRemoteEndPoint(string ipString)
    {
        IPAddress address;
        // TryParseを使うと、変な文字列（"あいうえお"とか）が来た時にエラー落ちせずfalseを返してくれる
        if (IPAddress.TryParse(ipString, out address))
        {
            _remoteEndPoint = new IPEndPoint(address, port);
            tmpText.text=$"IP:port : {ipString}:{port}";
            return true;
        }
        return false;
    }

    void OnDisable()
    {
        _udpClient?.Close();
    }

    public void SendPacket(byte[] data)
    {
        if (_udpClient == null || _remoteEndPoint == null) return;

        try
        {
            // 更新された _remoteEndPoint に向かって送信される
            _udpClient.Send(data, data.Length, _remoteEndPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"送信できませんでした:{e.Message}");
        }
    }
}