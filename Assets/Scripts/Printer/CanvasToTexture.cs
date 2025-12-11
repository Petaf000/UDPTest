using UnityEngine;
using UnityEngine.UI;

public class CanvasToTexture : MonoBehaviour
{
    [Header("UIを映しているカメラ")]
    public Camera uiCamera;

    [Header("キャプチャしたいCanvas (レイアウト更新用)")]
    public Canvas targetCanvas;
    public CanvasScaler canvasScaler;

    /// <summary>
    /// UIをTexture2Dに変換して返す関数
    /// </summary>
    public Texture2D CaptureCanvasToTexture()
    {
        // 1. 印刷用サイズの設定 (B5 300dpi想定: 2150x3035)
        int width = 3035;
        int height = 2150;

        // 2. RenderTextureを作成 (ここがフィルムになる)
        RenderTexture rt = new RenderTexture(width, height, 24);

        // 3. カメラとRenderTextureを紐付け
        // これでカメラは画面ではなく、このrtに向かって描画します
        var prevTarget = uiCamera.targetTexture; // 元の設定を保存
        uiCamera.targetTexture = rt;

        // 【重要】解像度が変わるのでCanvasのレイアウトを強制更新させる
        // これをやらないと、文字や画像の配置がズレたり崩れたりします
        // ※CanvasScalerが「Scale With Screen Size」になっている前提
        if (canvasScaler != null)
        {
            // 一時的にCanvasScalerの参照解像度を合わせるなどの調整が必要な場合もありますが
            // 基本的には画面サイズが変わったと認識させてリビルドを走らせます
            Canvas.ForceUpdateCanvases();
        }

        // 4. カシャッ！ (手動で1フレームレンダリング)
        uiCamera.Render();

        // 5. RenderTextureからTexture2Dにデータを吸い出す
        RenderTexture.active = rt; // ReadPixelsはactiveなRTから読むため
        Texture2D resultTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        resultTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resultTex.Apply();

        // 6. 後始末 (設定を元に戻す)
        uiCamera.targetTexture = prevTarget;
        RenderTexture.active = null;
        Destroy(rt); // 使い終わったRenderTextureは破棄

        Debug.Log("Canvasのキャプチャ完了！");
        return resultTex;
    }
}