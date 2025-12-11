using System.Collections;
using System.IO;
using UnityEngine;
// 以下の2つを使うために、上の「事前準備」が必要です
using System.Drawing;
using System.Drawing.Printing;

public class TexturePrinter : MonoBehaviour
{
    /// <summary>
    /// 指定されたTexture2Dを、デフォルトプリンターで印刷します
    /// </summary>
    /// <param name="textureToPrint">印刷したいテクスチャ</param>
    /// <param name="printerName">空文字ならデフォルトプリンター。指定したい場合は名前を入れる（例: "EPSON EP-882A"）</param>
    public void PrintTexture(Texture2D textureToPrint, string printerName = "")
    {
        // 1. Texture2Dを一時的なPNGファイルとして保存
        // System.DrawingはUnityのTextureを直接読めないため、一度ファイルにします
        string tempPath = Path.Combine(Application.temporaryCachePath, "print_temp.png");
        File.WriteAllBytes(tempPath, textureToPrint.EncodeToPNG());

        // 2. 印刷ドキュメントの設定
        PrintDocument pd = new PrintDocument();

        // プリンター名の指定があれば設定、なければデフォルトが使われる
        if (!string.IsNullOrEmpty(printerName))
        {
            pd.PrinterSettings.PrinterName = printerName;
        }

        // 3. 用紙サイズの設定（B5 JIS）
        // プリンターによっては "B5 (JIS)" という名前が違う場合があるので
        // 見つからなければデフォルト設定で印刷されます
        foreach (PaperSize size in pd.PrinterSettings.PaperSizes)
        {
            if (size.Kind == PaperKind.B5) // B5サイズを探す
            {
                pd.DefaultPageSettings.PaperSize = size;
                break;
            }
        }

        // 4. 実際の描画処理（イベントハンドラ）
        pd.PrintPage += (sender, e) =>
        {
            // 一時ファイルから画像を読み込む
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(tempPath))
            {
                // 用紙の余白の内側のサイズを取得
                Rectangle m = e.MarginBounds;

                // 画像を用紙サイズに合わせてリサイズ（アスペクト比維持）
                // ここでは単純に用紙いっぱいに引き伸ばす例です
                e.Graphics.DrawImage(img, m);
            }
        };

        try
        {
            Debug.Log("印刷データを送信中: " + pd.PrinterSettings.PrinterName);
            pd.Print(); // 印刷実行！
        }
        catch (System.Exception ex)
        {
            Debug.LogError("印刷エラー: " + ex.Message);
        }
    }
}