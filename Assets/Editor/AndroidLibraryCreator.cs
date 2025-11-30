using UnityEngine;
using UnityEditor;
using System.IO;

public class AndroidLibraryCreator
{
    [MenuItem("Tools/【準備】透明化ライブラリ作成")]
    public static void CreateLibrary()
    {
        // --- パス定義 ---
        string pluginsPath = Path.Combine(Application.dataPath, "Plugins/Android");
        string libName = "TransparentTheme.androidlib";
        string libPath = Path.Combine(pluginsPath, libName);
        string libResPath = Path.Combine(libPath, "res/values");

        // フォルダ作成
        if (!Directory.Exists(libResPath)) Directory.CreateDirectory(libResPath);

        // 1. ライブラリ用 AndroidManifest.xml (中身はパッケージ宣言だけ)
        string libManifestContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
    package=""com.custom.transparenttheme"">
</manifest>";
        File.WriteAllText(Path.Combine(libPath, "AndroidManifest.xml"), libManifestContent);

        // 2. project.properties (これをライブラリとして認識させる)
        File.WriteAllText(Path.Combine(libPath, "project.properties"), "target=android-31\nandroid.library=true");

        // 3. styles.xml (ここで初めて「透明とは何か」を定義する)
        string stylesContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
    <style name=""Theme.Transparent"" parent=""@android:style/Theme.Translucent.NoTitleBar"">
        <item name=""android:windowBackground"">@android:color/transparent</item>
        <item name=""android:colorBackgroundCacheHint"">@null</item>
        <item name=""android:windowIsTranslucent"">true</item>
        <item name=""android:windowAnimationStyle"">@android:style/Animation</item>
        <item name=""android:backgroundDimEnabled"">false</item>
    </style>
</resources>";
        File.WriteAllText(Path.Combine(libResPath, "styles.xml"), stylesContent);

        AssetDatabase.Refresh();
        Debug.Log("★ 準備完了: 透明化に必要な xml ファイル群を作成しました。");
        Debug.Log("次に、Unityの設定からメインのManifestを生成し、Patcherを実行してください。");
    }
}