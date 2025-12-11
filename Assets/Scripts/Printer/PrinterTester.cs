using UnityEngine;

public class PrinterTester : MonoBehaviour
{
    public CanvasToTexture capturer; // インスペクターで設定
    public TexturePrinter printer;   // さっきの印刷スクリプト

    public void OnComplete()
    {
        Debug.Log("印刷処理開始");
        Texture2D tex = capturer.CaptureCanvasToTexture();

        // 2. 印刷へ回す
        printer.PrintTexture(tex);
        // もし特定のプリンター名を指定するならこう
        // printer.PrintTexture(generatedTexture, "Brother HL-L2375DW");
    }
}
