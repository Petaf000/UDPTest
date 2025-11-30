using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class AndroidThemePatcher
{
    [MenuItem("Tools/【最終調整】Manifestを透明化")]
    public static void PatchManifest()
    {
        string manifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

        if (!File.Exists(manifestPath))
        {
            Debug.LogError("エラー: AndroidManifest.xml が見つかりません！\nProject Settings > Player > Publishing Settings > 'Custom Main Manifest' にチェックを入れてから実行してください。");
            return;
        }

        string content = File.ReadAllText(manifestPath);

        // 1. テーマを透明なもの(Theme.Transparent)に書き換える
        // unity3d.player.UnityPlayerActivity または GameActivity を探して置換
        string themePattern = @"android:theme=""@style\/[^""]*""";
        string newTheme = @"android:theme=""@style/Theme.Transparent""";

        if (Regex.IsMatch(content, themePattern))
        {
            // 既存のテーマ指定がある場合は置換
            content = Regex.Replace(content, themePattern, newTheme);
        }
        else
        {
            // テーマ指定がない場合は、activityタグの中に追記
            // <activity ... > を探して、その属性として挿入
            string activityPattern = @"<activity\s+([^>]*)>";
            content = Regex.Replace(content, activityPattern, @"<activity android:theme=""@style/Theme.Transparent"" $1>");
        }

        // 2. ついでに android:exported=""true"" があるか確認し、なければ追加（Android 12以降対応）
        if (!content.Contains(@"android:exported=""true"""))
        {
            string activityStartPattern = @"<activity\s";
            content = Regex.Replace(content, activityStartPattern, @"<activity android:exported=""true"" ");
        }

        File.WriteAllText(manifestPath, content);

        Debug.Log("★ 成功: AndroidManifest.xml に透明化テーマを適用しました！");
        Debug.Log("ビルドして実機確認を行ってください。");
    }
}