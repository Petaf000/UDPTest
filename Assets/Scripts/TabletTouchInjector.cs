using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class TabletTouchInjector : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster targetRaycaster;
    [SerializeField] private PlayerID deviceIndex = PlayerID.Player1;
    [SerializeField] private RectTransform cursorIcon;

    // --- マルチタッチ用の状態管理クラス ---
    private class TouchState
    {
        public PointerEventData pointerData;
        public GameObject currentPressedObject;
        public GameObject currentDraggingObject;
        public GameObject currentHoveredObject;
        public bool wasPressed;

        public TouchState(EventSystem eventSystem)
        {
            pointerData = new PointerEventData(eventSystem);
        }
    }

    private TouchState[] touchStates = new TouchState[10];
    private EventSystem eventSystem;
    private TabletDevice myDevice;
    private Camera uiCamera;

    void Start()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("シーンに EventSystem がありません。UIが動きません。");
            return;
        }

        myDevice = deviceIndex == PlayerID.Player1 ? TabletDeviceDriver.Instance.DeviceP1 : TabletDeviceDriver.Instance.DeviceP2;
        uiCamera = targetRaycaster.eventCamera;

        for (int i = 0; i < 10; i++)
        {
            touchStates[i] = new TouchState(eventSystem);
            // カメラ設定
            if (uiCamera != null)
            {
                touchStates[i].pointerData.displayIndex = uiCamera != null ? uiCamera.targetDisplay : 0;
            }
        }
    }

    void Update()
    {
        if (myDevice == null) return;

        float targetWidth = (uiCamera != null) ? uiCamera.pixelWidth : Screen.width;
        float targetHeight = (uiCamera != null) ? uiCamera.pixelHeight : Screen.height;

        for (int i = 0; i < 10; i++)
        {
            // InputSystemから各指の情報を取得
            Vector2 touchPos = myDevice.touchPositions[i].ReadValue();
            bool isPressed = myDevice.touchPresses[i].isPressed;

            // スクリーン座標変換
            Vector2 screenPos = new Vector2(touchPos.x, touchPos.y);

            // 指ごとの処理を実行
            ProcessTouch(i, screenPos, isPressed);
        }
    }


    private void ProcessTouch(int index, Vector2 screenPos, bool isPressed)
    {
        TouchState state = touchStates[index];
        PointerEventData pData = state.pointerData;

        // --- 1. カーソル表示 (Index 0 のみ) ---
        if (index == 0 && cursorIcon != null && isPressed)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorIcon.parent as RectTransform,
                screenPos, // 計算済みのスクリーン座標を使用
                uiCamera,
                out localPoint
            );

            // Z軸をリセットしてUI裏隠れを防止
            cursorIcon.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
            cursorIcon.gameObject.SetActive(true);
        }
        else if (index == 0 && cursorIcon != null && !isPressed)
        {
            // 離している間は見えなくするならコメントアウトを外す
            // cursorIcon.gameObject.SetActive(false);
        }

        // --- 2. Delta計算 ---
        if (isPressed)
        {
            if (!state.wasPressed)
            {
                pData.delta = Vector2.zero; // 押し始め
            }
            else
            {
                pData.delta = screenPos - pData.position; // 移動中
            }
            pData.position = screenPos;
        }
        else
        {
            pData.delta = Vector2.zero;
            pData.position = screenPos;
        }

        // --- 3. Raycast実行 ---
        List<RaycastResult> results = new List<RaycastResult>();
        targetRaycaster.Raycast(pData, results);

        if (results.Count > 0) pData.pointerCurrentRaycast = results[0];
        else pData.pointerCurrentRaycast = new RaycastResult();

        GameObject hitObjectRaw = results.Count > 0 ? results[0].gameObject : null;

        // ホバー処理
        HandleHover(state, hitObjectRaw);

        // --- 4. Press/Drag処理 ---
        if (isPressed)
        {
            PressProcess(state, hitObjectRaw);
        }
        else
        {
            ReleaseProcess(state, hitObjectRaw);
        }

        // 前フレーム状態更新
        state.wasPressed = isPressed;
    }

    // --- 以下、状態管理オブジェクト(state)を受け取るように変更したメソッド群 ---

    private void HandleHover(TouchState state, GameObject hitObjectRaw)
    {
        GameObject hitObject = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hitObjectRaw);
        if (hitObject == null) hitObject = hitObjectRaw;

        if (hitObject != state.currentHoveredObject)
        {
            if (state.currentHoveredObject != null)
                ExecuteEvents.Execute(state.currentHoveredObject, state.pointerData, ExecuteEvents.pointerExitHandler);

            if (hitObject != null)
                ExecuteEvents.Execute(hitObject, state.pointerData, ExecuteEvents.pointerEnterHandler);

            state.currentHoveredObject = hitObject;
            state.pointerData.pointerEnter = hitObject;
        }
    }

    private void PressProcess(TouchState state, GameObject hitObjectRaw)
    {
        // 押し始め
        if (state.currentPressedObject == null)
        {
            if (hitObjectRaw == null) return;

            state.pointerData.pointerPressRaycast = state.pointerData.pointerCurrentRaycast;

            GameObject downHandler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(hitObjectRaw);
            GameObject target = downHandler != null ? downHandler : hitObjectRaw;

            ExecuteEvents.Execute(target, state.pointerData, ExecuteEvents.pointerDownHandler);

            state.currentPressedObject = target;
            state.pointerData.pointerPress = target;

            // ドラッグ開始
            GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(hitObjectRaw);
            if (dragHandler != null)
            {
                ExecuteEvents.Execute(dragHandler, state.pointerData, ExecuteEvents.beginDragHandler);
                state.currentDraggingObject = dragHandler;
            }
        }
        else
        {
            // ドラッグ中
            if (state.currentDraggingObject != null)
            {
                ExecuteEvents.Execute(state.currentDraggingObject, state.pointerData, ExecuteEvents.dragHandler);
            }
        }
    }

    private void ReleaseProcess(TouchState state, GameObject hitObjectRaw)
    {
        if (state.currentPressedObject != null)
        {
            ExecuteEvents.Execute(state.currentPressedObject, state.pointerData, ExecuteEvents.pointerUpHandler);

            GameObject clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObjectRaw);

            if (state.currentPressedObject == clickHandler)
            {
                ExecuteEvents.Execute(state.currentPressedObject, state.pointerData, ExecuteEvents.pointerClickHandler);
            }

            if (state.currentDraggingObject != null)
            {
                ExecuteEvents.Execute(state.currentDraggingObject, state.pointerData, ExecuteEvents.endDragHandler);
                state.currentDraggingObject = null;
            }

            state.pointerData.pointerPress = null;
            state.currentPressedObject = null;
        }
    }
}