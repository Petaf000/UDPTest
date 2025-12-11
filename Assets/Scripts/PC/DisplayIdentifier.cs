using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayIdentifier : MonoBehaviour
{
    [SerializeField] private GameObject probePrefab; // 手順1で作ったCamera入りPrefab

    void Start()
    {
        // 接続されているディスプレイ分ループ
        for (int i = 0; i < Display.displays.Length; i++)
        {
            // 1. ディスプレイをアクティブ化 (必須)
            Display.displays[i].Activate();

            // 2. Prefabを生成
            GameObject probe = Instantiate(probePrefab);

            // 3. Cameraの出力先をこのディスプレイに変更
            Camera cam = probe.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.targetDisplay = i;
            }

            // 4. Canvasの出力先も変更
            Canvas cvs = probe.GetComponentInChildren<Canvas>();
            if (cvs != null)
            {
                cvs.targetDisplay = i;
            }

            // 5. テキストに番号を表示
            var tmpText = probe.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = "Display: " + i;

            var legText = probe.GetComponentInChildren<Text>();
            if (legText != null) legText.text = "Display: " + i;
        }
    }
}