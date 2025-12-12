using UnityEngine;
using TMPro;

public class QuaternionDebugger : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text statusText;
    public GameObject targetObject; // 回転させるオブジェクト

    [Header("Current Settings")]
    private bool invertX = false;
    private bool invertY = false;
    private bool invertZ = false;
    private bool invertW = false;
    private AxisMapping xMapping = AxisMapping.X;
    private AxisMapping yMapping = AxisMapping.Y;
    private AxisMapping zMapping = AxisMapping.Z;

    // テスト用のクォータニオン（PicoWControllerから取得する想定）
    private Quaternion testQuaternion = Quaternion.identity;

    private enum AxisMapping
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    void Start()
    {
        UpdateStatusDisplay();
    }

    // === PicoWControllerから呼ばれる（テスト用データ注入） ===
    public void InjectTestQuaternion(Quaternion quat)
    {
        testQuaternion = quat;
        ApplyCurrentMapping();
    }

    // === X軸の符号を反転 ===
    public void ToggleInvertX()
    {
        invertX = !invertX;
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === Y軸の符号を反転 ===
    public void ToggleInvertY()
    {
        invertY = !invertY;
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === Z軸の符号を反転 ===
    public void ToggleInvertZ()
    {
        invertZ = !invertZ;
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === W軸の符号を反転 ===
    public void ToggleInvertW()
    {
        invertW = !invertW;
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === X軸のマッピングを変更（X→Y→Z→X...） ===
    public void CycleXMapping()
    {
        xMapping = (AxisMapping)(((int)xMapping + 1) % 3);
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === Y軸のマッピングを変更（X→Y→Z→X...） ===
    public void CycleYMapping()
    {
        yMapping = (AxisMapping)(((int)yMapping + 1) % 3);
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === Z軸のマッピングを変更（X→Y→Z→X...） ===
    public void CycleZMapping()
    {
        zMapping = (AxisMapping)(((int)zMapping + 1) % 3);
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === リセット ===
    public void ResetToDefault()
    {
        invertX = false;
        invertY = false;
        invertZ = false;
        invertW = false;
        xMapping = AxisMapping.X;
        yMapping = AxisMapping.Y;
        zMapping = AxisMapping.Z;
        UpdateStatusDisplay();
        ApplyCurrentMapping();
    }

    // === 決定：コードを生成してログに出力 ===
    public void GenerateCode()
    {
        string code = "// PicoWController.cs の OnDataReceived 内で使用するコード:\n\n";
        code += "float x = BitConverter.ToSingle(data, 0);\n";
        code += "float y = BitConverter.ToSingle(data, 4);\n";
        code += "float z = BitConverter.ToSingle(data, 8);\n";
        code += "float w = BitConverter.ToSingle(data, 12);\n\n";

        // マッピングされた軸を取得
        string xSource = GetAxisSource(xMapping, invertX);
        string ySource = GetAxisSource(yMapping, invertY);
        string zSource = GetAxisSource(zMapping, invertZ);
        string wSource = invertW ? "-w" : "w";

        code += $"Quaternion rot = new Quaternion({xSource}, {ySource}, {zSource}, {wSource});\n\n";
        code += "latestRotation = rot;\n";
        code += "hasNewRotation = true;";

        Debug.Log("=== 生成されたコード ===");
        Debug.Log(code);
        Debug.Log("======================");

        if (statusText != null)
        {
            statusText.text = "コードをコンソールに出力しました！\n\n" + GetCurrentSettingsText();
        }
    }

    // === 現在の設定を表示 ===
    private void UpdateStatusDisplay()
    {
        if (statusText != null)
        {
            statusText.text = GetCurrentSettingsText();
        }
    }

    private string GetCurrentSettingsText()
    {
        string text = "=== 現在の設定 ===\n\n";

        text += $"X軸: {(invertX ? "-" : "+")}{xMapping}\n";
        text += $"Y軸: {(invertY ? "-" : "+")}{yMapping}\n";
        text += $"Z軸: {(invertZ ? "-" : "+")}{zMapping}\n";
        text += $"W軸: {(invertW ? "-" : "+")}\n\n";

        text += "--- ボタン説明 ---\n";
        text += "反転X/Y/Z/W: 符号を反転\n";
        text += "切替X/Y/Z: 軸マッピング変更\n";
        text += "リセット: 初期値に戻す\n";
        text += "決定: コード生成";

        return text;
    }

    // === 現在のマッピングを適用 ===
    private void ApplyCurrentMapping()
    {
        if (targetObject == null) return;

        // テスト用クォータニオンから値を取得
        float[] values = new float[] { testQuaternion.x, testQuaternion.y, testQuaternion.z };

        // マッピングに従って新しいクォータニオンを作成
        float newX = values[(int)xMapping] * (invertX ? -1 : 1);
        float newY = values[(int)yMapping] * (invertY ? -1 : 1);
        float newZ = values[(int)zMapping] * (invertZ ? -1 : 1);
        float newW = testQuaternion.w * (invertW ? -1 : 1);

        Quaternion mappedQuat = new Quaternion(newX, newY, newZ, newW);

        // 横持ち補正を適用
        Quaternion landscapeCorrection = Quaternion.Euler(0, 0, -90);
        targetObject.transform.rotation = landscapeCorrection * mappedQuat;
    }

    // === 軸のソースコード文字列を取得 ===
    private string GetAxisSource(AxisMapping mapping, bool invert)
    {
        string axisName = "";
        switch (mapping)
        {
            case AxisMapping.X: axisName = "x"; break;
            case AxisMapping.Y: axisName = "y"; break;
            case AxisMapping.Z: axisName = "z"; break;
        }

        return invert ? $"-{axisName}" : axisName;
    }
}