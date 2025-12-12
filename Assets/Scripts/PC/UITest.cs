using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // 親のCanvasを探して取得（拡大率の計算に必要）
        canvas = GetComponentInParent<Canvas>();
    }

    // これがないと「ドラッグ開始」として認識されないことがあるため記述（中身は空でOK）
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        // TabletTouchInjectorが送ってくる「delta（移動量）」を使って動かす
        if (canvas != null)
        {
            // Canvasのスケール（拡大率）で割ることで、画面解像度が変わっても追従速度を一定にする
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else
        {
            // Canvasが見つからない場合の保険
            rectTransform.anchoredPosition += eventData.delta;
        }
    }

    private void Update()
    {
        
    }
}