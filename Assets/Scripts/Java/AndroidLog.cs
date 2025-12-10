using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AndroidLog : SingletonMonoBehaviour<AndroidLog>
{
    [Header("UI Output (Optional)")]
    [SerializeField] private TMP_Text debugText;

    // スレッドセーフ確保のためのキュー
    private Queue<string> logQueue = new Queue<string>();
    private object lockObj = new object();

    private const int MAX_LOG_LINES = 30;

    public void Log(string message)
    {
        lock (lockObj)
        {
            logQueue.Enqueue(message);
        }
    }

    void Update()
    {
        lock (lockObj)
        {
            while (logQueue.Count > 0)
            {
                string msg = logQueue.Dequeue();

                // 一応Unityコンソールに出力
                Debug.Log($"[AndroidLog] {msg}");

                if (debugText != null)
                {
                    debugText.text = msg + "\n" + debugText.text;

                    if (debugText.text.Length > 2000)
                    {
                        debugText.text = debugText.text.Substring(0, 2000);
                    }
                }
            }
        }
    }

    public void Clear()
    {
        if (debugText != null) debugText.text = "";
    }
}